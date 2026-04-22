# OnlineBankingRazer 退款執行流程

> **付款方式**：`OnlineBanking_Razer`  
> **NMQ Task 名稱**：`OnlineBankingRazerRefundRequestFinish`  
> **核心 Service**：`PaymentMiddleWareRefundRequestService`  
> **PayChannel**：`RazerPayChannelService`

---

## 一、整體流程概覽

```
[ERPDB_Routine_GenerateThirdPartyPaymentRefundRequestGrouping（排程觸發）]
              │
              ▼
┌─────────────────────────────────────────────────────────┐
│  PaymentMiddleWareRefundRequestGroupingProcess.DoJob()  │  ← NMQ Grouping Process
│     → PaymentMiddleWareRefundRequestService             │
│       .CreateRefundRequestFinish()                      │
└───────────────────────┬─────────────────────────────────┘
                        │
         ① GetRefundRequestData()
           從 DB 撈 WaitingToRefundRequest 的退款單
           （只撈 OnlineBanking_Razer 類型）
                        │
         ② 按 TradesOrderGroupId 分群 GroupBy
                        │
         ③ 更新 DB 狀態 → RefundRequestGrouping（防止重複執行）
                        │
         ④ 建立 NMQ Task：OnlineBankingRazerRefundRequestFinish
                        │
                        ▼
┌──────────────────────────────────────────────────────────┐
│  PaymentMiddleWareRefundRequestFinishProcess.DoJob()     │  ← NMQ Finish Process
│     → PaymentMiddleWareRefundRequestService              │
│       .DoRefundRequestFinish(taskData)                   │
└──────────────────────────────────────────────────────────┘
```

---

## 二、`DoRefundRequestFinish()` 細部流程

```
DoRefundRequestFinish(taskData)
        │
        ├─ ① 反序列化 taskData → PaymentMiddleWareRefundTaskDataEntity
        │       ↳ 失敗 → throw ApplicationException
        │
        ├─ ② 查 SalesOrderThirdPartyPayment（by TradesOrderGroupId）
        │       ↳ 找不到 → throw ApplicationException
        │
        ├─ ③ GetPayChannel("OnlineBanking_Razer")
        │       → 回傳 RazerPayChannelService
        │
        ├─ ④ CanGroupingRefund() → true（Razer 走 GroupingAmount 路徑）
        │
        └─ ⑤ RefundByGroupingAmount()
                │
                ├─ 撈出每筆 RefundRequest
                ├─ 狀態檢查 IsContinue()
                │   （只處理 RefundRequestGrouping / RefundRequestProcessing）
                │
                ├─ 按 RefundRequest_TransactionId 再次分群
                │
                └─ 判斷 TransactionId 是否已存在
                        │
                 有值（曾退過款）→ RefundQuery()（查詢退款結果）
                 無值（首次退款）→ Refund()（發起退款）
```

---

## 三、`Refund()` 實際呼叫 Razer 流程

```
Refund(payType="OnlineBanking_Razer", payment, refundRequest)
        │
        ├─ ① 取得 RequestId
        │       GET {PaymentMiddlewareUrl}/requestid
        │
        ├─ ② 組 HTTP Request URL
        │       POST {PaymentMiddlewareUrl}/Refund/OnlineBanking_Razer/{TGCode}
        │
        ├─ ③ RazerPayChannelService.GetRefundHttpHeaders()
        │       → 從 ShopSecret (GroupName: RazerMS) 取得：
        │           x-merchant  = RazerMS_MerchantID
        │           x-secret    = RazerMS_SecretKey
        │           x-verify    = RazerMS_VerifyKey
        │
        ├─ ④ RazerPayChannelService.GetRefundExtendInfo()
        │       → 組 ExtendInfo（OnlineBanking 需要銀行帳戶資訊）：
        │           RefID              = TradesOrderGroupId
        │           BankCode           = 銀行代碼（由 BankMasterRepository 查詢）
        │           BeneficiaryName    = 銀行名稱
        │           BeneficiaryAccNo   = 銀行帳號
        │           is_fiuu_enable     = true（若該 Shop 啟用 Fiuu 端點）
        │
        └─ ⑤ POST 到 PaymentMiddleware → 轉發至 Razer 金流
                HTTP 200 → 解析回應 ReturnCode
                非 200   → throw ApplicationException
```

---

## 四、Razer 回應處理與狀態對應

| ReturnCode | 退款狀態 | 說明 |
|---|---|---|
| `Success` | `Finish` | 退款成功，結案 |
| `RefundPeriodExceeded` | `RefundRequestProcessing` | 超過退款期限，轉為匯款方式 |
| `RefundPending` | `RefundRequestProcessing` | 退款 Pending，等待確認 |
| 其他失敗碼（3 天內 + PR020/PR015） | `RefundRequestProcessing` | 自動重試 |
| 其他失敗碼 | `RefundRequestFail` | 最終失敗，需人工處理 |

### 可自動重試的錯誤碼（OnlineBanking_Razer）

| 錯誤碼 | 說明 |
|--------|------|
| `PR020` | 可重試的退款失敗 |
| `PR015` | 可重試的退款失敗 |

> **重試條件**：退款時間在 3 天以內，且 ReturnMessage 包含上述錯誤碼，狀態改為 `RefundRequestProcessing` 等待下次排程重新執行。

---

## 五、退款完成後的後續動作

```
狀態 = Finish
        │
        ├─ UpdateRefundRequest()：
        │   IsClosed                = true
        │   ConfirmDate             = 今天
        │   ConfirmDateTime         = 今天
        │   ExpectedFundingDate     = 今天 + 10 天
        │   TransactionId           = Razer 回傳的 TransactionId
        │
        ├─ 退款來源判斷：
        │   ├─ SalesOrderFee（運費）→ 不更新大表
        │   └─ 其他來源          → UpdateOrderSlaveFlow()
        │                               ├─ 多付款方式
        │                               │   → 建立 NMQ Task：SyncRefundRequestFinishJob
        │                               └─ 單付款方式
        │                                   → OrderSlaveFlow 更新為 RefundFinish
        │                                   → 建立 NMQ Task：SyncOrderFlow（同步前台）
        │
        └─ RazerPayChannelService.RefundDelay() → 無動作（Razer 不需延時）
```

---

## 六、退款狀態流轉總覽

```
WaitingToRefundRequest
        │  排程觸發 CreateRefundRequestFinish()
        ▼
RefundRequestGrouping
        │  NMQ Task OnlineBankingRazerRefundRequestFinish 執行
        ▼
   ┌────────────────────────────────────────────┐
   │          呼叫 Razer 金流 (Refund / Query)   │
   └────────────────────────────────────────────┘
        │
        ├─→ Finish              （退款完成，結案，更新大表）
        │
        ├─→ RefundRequestProcessing   （Pending / 超過退款期限 / 可重試失敗）
        │         └─ 等待下次排程重新觸發
        │
        └─→ RefundRequestFail   （最終失敗，需人工處理）
```

---

## 七、特殊設計說明

### 7-1. `RefundRequestGrouping` 中間狀態的用途

在分群 Job 執行後，立即將退款單狀態改為 `RefundRequestGrouping`，目的是：

- **防止重複執行**：下一輪排程不會重複撈取同一批退款單
- **冪等保護**：若 NMQ Task 執行失敗，狀態仍為 `RefundRequestGrouping`，可安全 Redo 而不會重複對 Razer 發起退款

### 7-2. `CanGroupingRefund() → true`（Grouping Amount 路徑）

Razer 採用 **Grouping Amount** 退款，即同一 `TransactionId` 的多筆退款單會合計金額，**一次性發起對 Razer 的退款請求**，而非逐筆呼叫。

### 7-3. Fiuu 端點支援

若商店在 `ShopStaticSetting` 中設定 `RazerPay.IsFiuuEnable = true`，則 ExtendInfo 會加入 `is_fiuu_enable: true`，PaymentMiddleware 會將請求轉發至 Fiuu 端點。

### 7-4. 銀行資訊來源（MY / SG 市場過濾）

`OnlineBanking_Razer` 屬於網路銀行轉帳退款，需提供退款帳戶資料，銀行清單會依銷售市場篩選：

| 市場 | 銀行類型過濾 |
|------|------------|
| MY (DefaultMarket=MY) | MolPayOnline、Bank |
| SG | Singapore |
| 其他 / TW | 不過濾 |

---

## 八、相關程式碼位置

| 說明 | 檔案路徑 |
|------|----------|
| NMQ Grouping Process | `SCM\Frontend\NMQV2\ThirdPartyPayments\PaymentMiddleWares\PaymentMiddleWareRefundRequestGroupingProcess.cs` |
| NMQ Finish Process | `SCM\Frontend\NMQV2\ThirdPartyPayments\PaymentMiddleWares\PaymentMiddleWareRefundRequestFinishProcess.cs` |
| 核心退款 Service | `ERP\Backend\BLV2\ThirdPartyPayments\PaymentMiddleWareRefundRequestService.cs` |
| Razer PayChannel Service | `ERP\Backend\BLV2\PayChannel\RazerPayChannelService.cs` |
| NMQ Job Name 常數 | `Core\Utility\Constants\NMQJobNameConstants.cs` |
| 退款狀態 Enum | `ERP\Backend\BE\RefundRequest\RefundRequestEnum.cs` |
| 退款類型 Enum | `ERP\Backend\BE\RefundRequest\RefundRequestTypeDefEnum.cs` |
