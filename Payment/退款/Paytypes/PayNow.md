# Razer PayNow 退款流程

## 基本資訊

| 項目 | 值 |
|------|-----|
| PayChannel | `Razer` |
| 適用 PayType | `PayNow_Razer`（新加坡 PayNow） |
| 退款路線 | `RefundByGroupingAmount`（**合併退款**，Razer 獨有） |
| CanGroupingRefund | `true`（全 PayChannel 中唯一覆寫為 true） |
| NMQ JobName | `PayNowRazerRefundRequestFinish` |
| 市場 | 新加坡（SG） |

---

## 為什麼是合併退款？

PayNow 退款本質是**銀行轉帳**（非信用卡退刷），Razer API 設計：
- 以 **TGCode 為 RefID** 作為退款識別
- 同一個 TGCode 只能打一次退款 API
- 因此同一個 TGCode 下的多筆 RefundRequest 必須**加總金額後一次打**

---

## 退款流程（RefundByGroupingAmount）

```bash
Grouping Job：同 TGCode 所有 RefundRequestIds → 一個 NMQ Task
    ↓
Finish Job DoRefundRequestFinish()
    ↓ CanGroupingRefund() == true
RefundByGroupingAmount()
    ↓
foreach RefundRequestIds:
  取出退款單 → 跳過 Finish/Fail 狀態
    ↓
GroupBy(RefundRequest_TransactionId)   ← 防時間差導致不同 TxnId 混入
    ↓ foreach TransactionId 群組
    sum(Amount) → 合併金額
    ↓
    TransactionId 為空？
      ├── 是 → POST /Refund     （新退款，打銀行轉帳）
      └── 否 → POST /RefundQuery（查詢已打過的退款結果）
    ↓
foreach 該群組每一筆退款單：
  UpdateRefundRequest（各自更新狀態）
  SourceDef == SalesOrderFee？
    ├── 是 → 不更新大表（運費退款）
    └── 否 → UpdateOrderSlaveFlow → RefundFinish
```

---

## 打 API 時傳入的額外資訊

### Refund ExtendInfo

```csharp
{
    "RefID":             tradesOrderGroupId,          // TGCode 的 TradesOrderGroupId（數字 ID）
    "BankCode":          bankCode,                     // 消費者銀行代碼（從 BankMaster 查）
    "BeneficiaryName":   refundRequest.RefundRequest_BankName,
    "BeneficiaryAccNo":  refundRequest.RefundRequest_BankAccountNo,
    "is_fiuu_enable":    true  // 若商店啟用 Fiuu 端點才帶此欄位
}
```

### RefundQuery ExtendInfo

```csharp
{
    "RefundedID":     refundRequest.RefundRequest_TransactionId,  // 退款時 Razer 回傳的退款單號
    "is_fiuu_enable": true  // 若商店啟用 Fiuu 端點才帶此欄位
}
```

---

## 銀行清單過濾（依市場）

| 市場（SalesMarket） | 允許的 BankMasterTypeDef |
|---------------------|--------------------------|
| SG（新加坡）| `Singapore` |
| MY（馬來西亞） + DefaultMarket=MY | `MolPayOnline`、`Bank` |
| 其他 | 不過濾 |

---

## Fiuu 端點

Razer 有新的 Fiuu 品牌端點，啟用條件：

```
ShopStaticSetting:
  GroupName = "RazerPay"
  Key = "IsFiuuEnable"
  Value = "true"
```

未設定或解析失敗 → 使用舊 Razer 端點。

---

## HTTP Headers

從 ShopSecret `RazerMS` group 取得：

```
x-merchant: {RazerMS_MerchantID}
x-secret:   {RazerMS_SecretKey}
x-verify:   {RazerMS_VerifyKey}
```

---

## 退款延時 & Rate Limiting

```csharp
public override void RefundDelay()
{
    // Razer 不需要延時（合併後只有一次 API 呼叫）
}

public override DateTime? RefundTaskBookingTime(DateTime startTime, int count)
{
    return null;  // 不限速
}
```

---

## 重試機制（失敗自動重試）

| 條件 | 說明 |
|------|------|
| 原始狀態 = `RefundRequestFail` | 才進入重試判斷 |
| 退款時間在 **3 天內** | 超過則不重試 |
| 錯誤碼 = `PR020` 或 `PR015` | Razer 的可重試錯誤碼 |

滿足條件 → 狀態改回 `RefundRequestProcessing` → 下次 Grouping Job 重撈

`GetRedoRefundRequestList()` → `null`（Razer 無 redo 清單機制，靠重試機制處理）

---

## ReturnCode 對應狀態

| ReturnCode | 退款狀態 | 備註 |
|------------|---------|------|
| `00` | `Finish`（結案）| `IsClosed = true` |
| `PR016` | `RefundRequestProcessing` | 超過退款期限 → TypeDef 改 `Remittance`（匯款） |
| `PR021` | `RefundRequestProcessing` | Pending |
| 其他 | `RefundRequestFail` | 觸發重試判斷 |

---

## 已知問題：金額分批退款

同一筆 TGCode 下若商品退款與運費退款**被不同批 Grouping Job 撈到**（因為建立時間不同）：
- 商品退款先建立 → 第一批 Grouping Job → 打一筆退款給 Razer
- 運費退款後建立 → 第二批 Grouping Job → 再打一筆退款給 Razer
- **結果**：Razer 收到兩筆分開的退款，消費者也看到兩筆入帳通知

---

## 關鍵檔案

| 檔案 | 路徑 | 說明 |
|------|------|------|
| `RazerPayChannelService.cs` | `ERP/Backend/BLV2/PayChannel/` | `CanGroupingRefund() = true`、Fiuu 判斷、銀行過濾 |
| `PaymentMiddleWareRefundRequestService.cs` | `ERP/Backend/BLV2/ThirdPartyPayments/` | `RefundByGroupingAmount` L225 |
