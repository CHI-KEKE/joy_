
## Job


PaymentMiddleWareRefundRequestGrouping


## 觸發機制

| 項目 | 說明 |
|------|------|
| **SQL 排程名稱** | `NMQV2_GeneratePaymentMiddleWareRefundRequestGrouping` |
| **觸發頻率** | 每小時 **02 分**塞一筆 Job 進 Task Table |
| **NMQ Job 入口** | `PaymentMiddleWareRefundRequestGroupingProcess.DoJob()` |
| **核心 Service** | `PaymentMiddleWareRefundRequestService.CreateRefundRequestFinish()` |

---

## 支援的金流（28 種付款方式）

### Razer 系列（`CanGroupingRefund = true` → 合併退款）

| PayType | 說明 |
|---------|------|
| `CreditCardOnce_Razer` | 信用卡一次付清 |
| `CreditCardInstallment_Razer` | 信用卡分期 |
| `OnlineBanking_Razer` | 網銀 |
| `Boost_Razer` | Boost 電子錢包 |
| `TNG_Razer` | Touch 'n Go |
| `GrabPay_Razer` | GrabPay |
| `PayNow_Razer` | PayNow（新加坡） |

> **Razer 特性**：`CanGroupingRefund() == true`，同一 TGCode 下的多筆退款會**合併金額後一次打 API**。

---

### 逐筆退款金流（`CanGroupingRefund = false` → 逐筆退款）

| PayChannel | PayType | 說明 |
|------------|---------|------|
| **Stripe** | `CreditCardOnce_Stripe` | 信用卡一次付清 |
| | `GooglePay` | Google Pay |
| | `ApplePay` | Apple Pay |
| **PayMe** | `EWallet_PayMe` | PayMe 電子錢包（香港） |
| **CheckoutDotCom** | `CreditCardOnce_CheckoutDotCom` | 信用卡一次付清 |
| **Cybersource** | `CreditCardOnce_Cybersource` | 信用卡一次付清 |
| **EftPay** | `AliPayHK_EftPay` | 支付寶香港 |
| | `WechatPayHK_EftPay` | 微信支付香港 |
| | `UnionPay_EftPay` | 銀聯 |
| **SwiftPass** | `BoCPay_SwiftPass` | 中銀 Pay |
| **Atome** | `Atome` | Atome 先買後付 |
| **AsiaPay** | `CreditCardOnce_AsiaPay` | 信用卡一次付清 |
| | `Boost_AsiaPay` | Boost |
| | `TNG_AsiaPay` | Touch 'n Go |
| | `GrabPay_AsiaPay` | GrabPay |
| **2C2P** | `TwoCTwoP` | 2C2P |
| **QFPay** | `QFPay` | QFPay |
| **KPay** | `CreditCardOnce_KPay` | 信用卡一次付清（香港） |

---

## Job 流程逐步解析

### Phase 1：Grouping Job（`CreateRefundRequestFinish`）

```bash
SQL 排程每小時 :02 觸發
    ↓
PaymentMiddleWareRefundRequestGroupingProcess.DoJob()
    ↓
PaymentMiddleWareRefundRequestService.CreateRefundRequestFinish()
```

**Step 1 — 撈出待退款資料（`GetRefundRequestData`）**

```csharp
// L920-941
// 各付款方式各自撈 RefundRequestProcessing 狀態的退款單
result.AddRange(_refundRequestRepository.GetThirdPartyPaymentRefundRequestData(payType));

// 同時撈入各 PayChannel 的 redo 重試清單（上次失敗待重試）
var redoRefundRequestList = payChannelService.GetRedoRefundRequestList(payType);
result.AddRange(redoRefundRequestList);
```

**Step 2 — 依 TGCode 分群**

```csharp
// L144
var groupList = thirdPartyPaymentRefundList.GroupBy(i => i.TradesOrderGroupId).ToList();
```

同一購物車（TGCode）的所有退款單合為一個群組。

**Step 3 — 鎖定狀態為 Grouping（防止重複撈取）**

```csharp
// L158
UpdateRefundRequestStatus(refundRequestIds, RefundRequestEnum.RefundRequestGrouping);
```

**Step 4 — 建立 NMQ Task（各付款方式對應不同 JobName）**

| 付款方式 | NMQ JobName |
|----------|-------------|
| `PayNow_Razer` | `PayNowRazerRefundRequestFinish` |
| `CreditCardOnce_Stripe` | `CreditCardOnceStripeRefundRequestFinish` |
| `EWallet_PayMe` | `EWalletPayMeRefundRequestFinish` |
| `Atome` | `AtomeRefundRequestFinish` |
| `CreditCardOnce_KPay` | `CreditCardOnceKPayRefundRequestFinish` |
| 其他... | 依付款方式對應 |

**Step 5 — Rate Limiting（`RefundTaskBookingTime`）**

```csharp
// L166-169
// 計算下一筆退款的預約時間，避免對 PaymentMiddleware 打太密集的請求
var bookTime = payChannelService.RefundTaskBookingTime(startTime, refundRequestIds.Count);
startTime = bookTime ?? startTime;
```

---

### Phase 2：Finish Job（`DoRefundRequestFinish`）

```bash
NMQ Task 到點執行
    ↓
DoRefundRequestFinish(taskData)
    ↓ 解析 Task Data（TGCode, PayType, RefundRequestIds）
    ↓ 查詢 SalesOrderThirdPartyPayment
    ↓
CanGroupingRefund()?
    ├── true  → RefundByGroupingAmount()   ← 僅 Razer
    └── false → RefundByRequestId()        ← 其他所有
```

---

#### 路線 A：`RefundByGroupingAmount`（Razer 合併退款）

```bash
foreach refundRequestIds:
  取出退款單 → 跳過非 Processing/Grouping 狀態
  ↓
GroupBy(RefundRequest_TransactionId)  ← 防止時間差導致不同 TxnId 被混在一起
  ↓
foreach TransactionId 群組:
  sum(RefundRequest_Amount) → 合併金額
  ↓
  TransactionId 為空？
    ├── 是 → POST /Refund/{payType}    ← 新退款
    └── 否 → POST /RefundQuery/{payType} ← 查詢退款狀態
  ↓
foreach 該群組每一筆退款單:
  UpdateRefundRequest（更新退款單狀態）
  SourceDef == SalesOrderFee？
    ├── 是 → 不更新大表（運費退款不動 OrderSlaveFlow）
    └── 否 → UpdateOrderSlaveFlow → RefundFinish
```

---

#### 路線 B：`RefundByRequestId`（逐筆退款）

```bash
foreach refundRequestIds:
  取出退款單
  ↓
  IsQueryRefund？→ 只查詢不退款
  IsRefund？    → 執行退款
  ↓
  ChangeRefundRequestStatus（處理超過退款期限等情境）
  ↓
  RefundDelay()  ← 延時，避免多筆同時打造成異常
  ↓
  UpdateRefundRequest
  UpdateOrderSlaveFlow
```

---

### 更新退款單結果（ReturnCode 對應狀態）

| ReturnCode | 退款狀態 | 備註 |
|------------|---------|------|
| `00` | `Finish`（結案）| `IsClosed = true`，寫入 ConfirmDate |
| `PR016` | `RefundRequestProcessing` | 超過退款期限 → TypeDef 改 `Remittance`（匯款） |
| `PR021` | `RefundRequestProcessing` | Pending，等待中 |
| 其他 | `RefundRequestFail` | 退款失敗 |

---

### 失敗重試機制

條件（AND）：
1. 原始狀態為 `RefundRequestFail`
2. 退款時間在 **3 天內**
3. 錯誤碼為可重試代碼

| PayChannel | 可重試錯誤碼 |
|------------|-------------|
| Razer 系列 | `PR020`、`PR015` |
| KPay | `29523` |

→ 滿足條件時狀態改回 `RefundRequestProcessing`，下次 Grouping Job 重新撈取執行。

---

### 多付款方式（IsMultiPayment）

- 多付款方式退款完成時**不直接更新大表**
- 改建立 `SyncRefundRequestFinishJob` V3 Task
- 等所有退款單都 Finish 後統一更新大表狀態

---

## 完整流程圖

```bash
[SQL 排程] NMQV2_GeneratePaymentMiddleWareRefundRequestGrouping
每小時 :02 分塞 Job
    ↓
PaymentMiddleWareRefundRequestGroupingProcess.DoJob()
    ↓
GetRefundRequestData()
  ├── 撈 RefundRequestProcessing（所有 28 種付款方式）
  └── 撈各 PayChannel redo 清單（失敗重試）
    ↓
GroupBy(TradesOrderGroupId)
    ↓ foreach group
UpdateRefundRequestStatus → RefundRequestGrouping（鎖定防重複）
CreateTask(NMQ JobName, taskData, bookTime)
RefundTaskBookingTime → 避免打爆 API
    ↓
[NMQ Task 到點] DoRefundRequestFinish(taskData)
    ↓
CanGroupingRefund()?
    ├── Razer → RefundByGroupingAmount
    │     GroupBy(TransactionId) → sum(Amount)
    │     POST /Refund 或 /RefundQuery
    │     UpdateRefundRequest + UpdateOrderSlaveFlow（非運費）
    └── 其他 → RefundByRequestId
          逐筆 IsQueryRefund / IsRefund
          ChangeRefundRequestStatus
          RefundDelay → UpdateRefundRequest + UpdateOrderSlaveFlow
```

---

## 關鍵檔案索引

| 檔案 | 路徑 | 說明 |
|------|------|------|
| `PaymentMiddleWareRefundRequestGroupingProcess.cs` | `SCM/Frontend/NMQV2/ThirdPartyPayments/PaymentMiddleWares/` | Job 入口 |
| `PaymentMiddleWareRefundRequestService.cs` | `ERP/Backend/BLV2/ThirdPartyPayments/` | 核心 Service（51KB） |
| `RazerPayChannelService.cs` | `ERP/Backend/BLV2/PayChannel/` | `CanGroupingRefund() = true` |
| `AbstractPayChannelService.cs` | `ERP/Backend/BLV2/PayChannel/` | 預設 `CanGroupingRefund() = false` |
| `ThirdPartyPaymentRequestGroupingProcessSetting.cs` | `SCM/Frontend/NMQV2/ThirdPartyPayments/` | DI 設定 |
