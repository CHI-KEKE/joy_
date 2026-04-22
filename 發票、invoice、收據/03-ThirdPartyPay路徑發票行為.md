# ThirdPartyPay 路徑下的發票行為

---

## ThirdPartyProcess 完整時序

**定義位置**：`PayProcessProcessorModule.cs` L388 附近

```
[使用者點擊付款]
    │
    ▼
ArrangeDataProcessor
    ├─ 取得 InvoiceIssueTypeDef（via GetActualInvoiceIssueTypeDef）
    ├─ 計算 IsCollectThreeCopiesInvoice → 決定是否蒐集三聯式
    ├─ 計算 IsIssueInvoice → 寫入 context.IsIssuedInvoice
    └─ 發票資訊整理（ArrangeMemberInvoice）
    │
    ▼
ValidateDataProcessor（各項資料驗證）
    │
    ▼
CreateTradesOrderProcessor
    ├─ L480：tradesOrderSlave.TradesOrderSlave_IsIssuedInvoice = context.IsIssuedInvoice
    └─ 訂單寫入 DB（此欄位建立後不再變更）
    │
    ▼
PaymentMiddlewareRequestProcessor（呼叫第三方金流 API）
    │
    ▼（付款 API 回應成功）
    │
    ▼
CreateGlobalInvoiceProcessor
    ├─ Guard：Global.Invoice.Enable == false → return
    ├─ Guard：DefaultCountry == "TW" AND MobileWallet → return
    └─ 推送 NMQ：GenerateMyInvoiceTask
    │
    ▼
後續 Processors（訂單完成、通知等）
```

---

## 關鍵：IsIssuedInvoice 在哪裡決定

**一旦訂單建立（CreateTradesOrderProcessor），`IsIssuedInvoice` 就寫死到 DB，後續不會變更。**

- 決定時機：付款流程進入前（`ArrangeDataProcessor`）
- 寫入時機：訂單建立時（`CreateTradesOrderProcessor` L480）
- 依據：`IsIssueInvoice()` 的計算結果（見文件 02）

---

## ThirdParty 特點：與 GlobalPay Callback 的差異

| 項目 | ThirdPartyProcess | GlobalPay Callback |
|---|---|---|
| 付款 API 呼叫方式 | 同步（在 Processor 中呼叫） | 非同步（等待 Callback） |
| 發票推送時機 | 付款 API 回應後，同一流程中 | Callback 進來後另一個流程 |
| 適用付款方式 | Razer 系列（SG/MY）、信用卡等 | GlobalPay（大馬原生金流） |
| 是否會 double 推送 | 不會（兩條路徑不重疊） | 不會 |

---

## SG 付款方式在 ThirdPartyProcess 中的行為

SG 主要付款方式均走 ThirdPartyProcess：

| 付款方式 | Enum | 是否在 notIssueInvoicePayTypes | 結果（IsIssueTypeDef = "Issue"） |
|---|---|---|---|
| CreditCardOnce_Razer | 獨立 Enum | ❌ 不在 | ✅ IsIssuedInvoice = true |
| GrabPay_Razer | 獨立 Enum | ❌ 不在 | ✅ IsIssuedInvoice = true |
| PayNow_Razer | 獨立 Enum | ❌ 不在 | ✅ IsIssuedInvoice = true |

> ⚠️ 前提：SG 店的 `InvoiceIssueTypeDef` 設定為 `"Issue"`  
> 若設定為 `NoInvoiceRequired` 則全部 false，不受付款方式影響

---

## 相關程式碼位置

| 檔案 | 說明 |
|---|---|
| `PayProcessProcessorModule.cs` | Processor 排列順序定義，L388 為 ThirdPartyProcess |
| `ArrangeDataProcessor.cs` | 發票資訊整理、IsIssuedInvoice 計算 |
| `CreateTradesOrderProcessor.cs` | L480：IsIssuedInvoice 寫入 DB |
| `CreateGlobalInvoiceProcessor.cs` | 推送 GenerateMyInvoiceTask |
| `PaymentMiddlewareService.cs` | SG 付款方式（Razer 系列）的整合進入點 |
