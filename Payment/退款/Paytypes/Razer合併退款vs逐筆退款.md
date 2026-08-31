# Razer 合併退款 vs 逐筆退款

## CanGroupingRefund：目前只有 Razer

```csharp
// RazerPayChannelService.cs
public override bool CanGroupingRefund() => true;   // 只有 Razer 覆寫

// AbstractPayChannelService.cs
public virtual bool CanGroupingRefund() => false;   // 其他 11 個 PayChannel 全走這個預設
```

| 路線 | 金流 |
|------|------|
| **RefundByGroupingAmount**（合併） | Razer 全系列（CreditCardOnce / Installment / OnlineBanking / Boost / TNG / GrabPay / PayNow） |
| **RefundByRequestId**（逐筆） | Stripe、PayMe、CheckoutDotCom、Cybersource、EftPay、SwiftPass、Atome、AsiaPay、2C2P、QFPay、KPay |

---

## 為什麼 Razer 要合併退款？

Razer 打退款 API 時傳的是：

```
RefID = TradesOrderGroupId（TGCode）
BankCode / BeneficiaryName / BeneficiaryAccNo（消費者銀行帳號）
```

Razer（尤其 PayNow）的退款本質是**銀行轉帳**，API 設計上：
- 以 **一個 TGCode** 為單位打一次退款
- 同一個 TGCode **不能重複打退款**（會被視為重複退款）

因此當同一筆 TGCode 下有多筆 RefundRequest（商品退 + 運費退 + 補收單），**必須先合併金額**，才能打一次 API。

---

## 兩條路線完整比較

| 比較點 | `RefundByGroupingAmount`（Razer） | `RefundByRequestId`（其他） |
|--------|-----------------------------------|-----------------------------|
| **退款單位** | 同 TGCode 所有退款單合併成一筆金額 | 每一筆 RefundRequest 各自打一次 |
| **金額計算** | `sum(所有 RefundRequest_Amount)` | 各筆自己的 Amount |
| **判斷邏輯** | `TransactionId 有值 → Query / 無值 → Refund` | `IsQueryRefund → Query / IsRefund → Refund` |
| **退款延時** | 不需要（`RefundDelay()` = 空實作） | 需要（每打一次 API 後 sleep） |
| **Rate Limiting** | `RefundTaskBookingTime()` 回傳 null（不限速） | 各 PayChannel 自行實作限速 |
| **失敗影響範圍** | 合計金額錯 → 整批退款金額全錯 | 某筆失敗不影響其他筆 |

---

## 邏輯流程對比

### RefundByGroupingAmount（Razer）

```bash
Grouping Job：同一 TGCode 所有 RefundRequestIds 打包進一個 NMQ Task
    ↓
Finish Job 收到 Task
    ↓
foreach RefundRequestIds:
  取出退款單 → 跳過非 Processing/Grouping 狀態
    ↓
GroupBy(RefundRequest_TransactionId)   ← 防時間差導致不同 TxnId 混在一起
    ↓ foreach TransactionId 群組
    sum(Amount)  →  合併成一筆金額
    ↓
    TransactionId 為空？
      ├── 是 → POST /Refund/{payType}    （新退款）
      └── 否 → POST /RefundQuery/{payType}（查詢退款結果）
    ↓
foreach 該群組每一筆退款單:
  UpdateRefundRequest（各自更新狀態）
  SourceDef == SalesOrderFee？
    ├── 是 → 不更新大表（運費退款不動 OrderSlaveFlow）
    └── 否 → UpdateOrderSlaveFlow → RefundFinish
```

### RefundByRequestId（其他金流）

```bash
Grouping Job：同一 TGCode 所有 RefundRequestIds 打包進一個 NMQ Task
    ↓
Finish Job 收到 Task
    ↓
foreach RefundRequestIds:
  取出退款單
    ↓
  IsQueryRefund(refundRequest)？
    ├── 是 → RefundQuery（只查不退）
    └── 否 → IsRefund(...)？
              ├── 是 → Refund（執行退款）
              └── 否 → 不處理
    ↓
  ChangeRefundRequestStatus()   ← 訂單超過期限 → 轉匯款
    ↓
  UpdateRefundRequest（更新退款單）
  SourceDef == SalesOrderFee？
    ├── 是 → 不更新大表
    └── 否 → UpdateOrderSlaveFlow
    ↓
  RefundDelay()   ← 避免多筆同時打造成異常
```

---

## 好處與壞處

### RefundByGroupingAmount（合併）

| | 說明 |
|--|------|
| ✅ 好處 | 符合 Razer API 規格（一個 TGCode 一次退款） |
| ✅ 好處 | 消費者只收到一筆入帳，體驗清楚 |
| ✅ 好處 | 不需要 Rate Limiting（`RefundDelay` 空實作） |
| ✅ 好處 | 避免重複退款風險 |
| ❌ 壞處 | **金額計算錯就整批錯**（補收單混入合計時特別危險） |
| ❌ 壞處 | Razer 那側只看到一筆金額，難以追查是哪一筆 RefundRequest 出問題 |
| ❌ 壞處 | 若商品退與運費退被不同批 Grouping Job 撈到，Razer 會收到**兩筆分開的退款** |

### RefundByRequestId（逐筆）

| | 說明 |
|--|------|
| ✅ 好處 | 每筆獨立，某筆失敗不影響其他 |
| ✅ 好處 | 可針對個別 RefundRequest 重試 |
| ✅ 好處 | 金額追查容易（一筆對一筆） |
| ❌ 壞處 | 多筆時打多次 API，需要 Rate Limiting / Delay |
| ❌ 壞處 | 消費者可能收到多筆入帳通知 |

---

## 關鍵檔案

| 檔案 | 路徑 | 說明 |
|------|------|------|
| `PaymentMiddleWareRefundRequestService.cs` | `ERP/Backend/BLV2/ThirdPartyPayments/` | `RefundByGroupingAmount` L225、`RefundByRequestId` L325 |
| `RazerPayChannelService.cs` | `ERP/Backend/BLV2/PayChannel/` | `CanGroupingRefund() => true`、`RefundDelay()` 空實作 |
| `AbstractPayChannelService.cs` | `ERP/Backend/BLV2/PayChannel/` | 預設 `CanGroupingRefund() => false` |
