# MY 部署中 SG 站的電子發票分析

---

## 背景前提

| 項目 | 值 | 說明 |
|---|---|---|
| 部署環境 | MY | `SettingHelper.DefaultCountry = "MY"` |
| SG 訂單 countryCode | `"SG"` | 由 ShippingArea.CountryAliasCode 決定 |
| SG 主要付款方式 | GrabPay_Razer、PayNow_Razer、CreditCardOnce_Razer | 不在 notIssueInvoicePayTypes 黑名單 |
| MY 部署的 NMQ Task | `GenerateMyInvoiceTask` | 設計為大馬 SST 發票用途 |

---

## 決策鏈完整追蹤

```
建立訂單（ThirdPartyProcess）
│
├─ Step 1：取得 InvoiceIssueTypeDef（SG 店的 ShopDefault Key=26）
│    │
│    └─ GetActualInvoiceIssueTypeDef()
│         ├─ "NoInvoiceRequired" → 維持（SG 正確設定方式）
│         ├─ "NotIssue" + ThirdParty gateway 啟用 → 維持 "NotIssue"
│         └─ "NotIssue" + ThirdParty gateway 未設定 → ⚠️ 強制改為 "Issue"
│
├─ Step 2：IsIssueInvoice("SG", invoiceIssueTypeDef, payType)
│    │
│    ├─ payType = GrabPay_Razer / PayNow_Razer / CreditCardOnce_Razer
│    │   → 不在黑名單，通過
│    │
│    ├─ invoiceIssueTypeDef = "NoInvoiceRequired" → ❌ false
│    ├─ invoiceIssueTypeDef = "NotIssue"          → ❌ false
│    └─ invoiceIssueTypeDef = "Issue"（含被強制改的情況） → ✅ true
│
├─ Step 3：IsIssuedInvoice 寫入 DB（CreateTradesOrderProcessor L480）
│
└─ Step 4：CreateGlobalInvoiceProcessor
     ├─ Guard：Global.Invoice.Enable == false → 不推送
     ├─ Guard：DefaultCountry == "TW" → MY 部署不會進入此 Guard
     └─ → 推送 GenerateMyInvoiceTask（若 Enable = true）
```

---

## 三種 InvoiceIssueTypeDef 設定下的 SG 行為

| SG 店設定 | `IsIssuedInvoice` | 前端顯示發票 | GenerateMyInvoiceTask 推送 | 實際產生發票 |
|---|---|---|---|---|
| `NoInvoiceRequired` | `false` | ❌ | 看 `Global.Invoice.Enable`，但 handler 無 `MalaysiaInvoice` DataSetType → 跳過 | ❌ |
| `NotIssue` + ThirdParty gateway 啟用 | `false` | ❌ | 同上 | ❌ |
| `NotIssue` + ThirdParty gateway **未設定** | ⚠️ `true`（被 fallback 成 `Issue`） | ✅ | ✅ 推送 | ⚠️ 會嘗試產生 MY SST 格式發票（不適用 SG） |
| `Issue` | `true` | ✅ | ✅ 推送 | ⚠️ 同上 |

---

## 保護機制（ProcessMyInvoice 的第二層防護）

**位置**：`TradesOrderService.ProcessMyInvoice()` （NMQ handler 端）

```csharp
// 只有訂單擴充資訊含 MalaysiaInvoice DataSetType 才觸發
var malaysiaInvoiceData = orderExtData
    .FirstOrDefault(x => x.DataSetType == DataSetTypeEnum.MalaysiaInvoice);

if (malaysiaInvoiceData == null)
    return;   // SG 訂單沒有此 DataSetType → 提前 return
```

> ✅ 即使 `GenerateMyInvoiceTask` 被推送，SG 訂單因無 `MalaysiaInvoice` 資料，  
> handler 會提前 return，**不實際呼叫馬來西亞發票 API**。

---

## 最終結論

### SG 站在 MY 部署中**不會產生有效的電子發票**

原因：
1. SG 沒有馬來西亞 SST 的稅務需求，`InvoiceIssueTypeDef` 應設為 `NoInvoiceRequired`
2. 即使誤設為 `Issue`，`ProcessMyInvoice` handler 因缺少 `MalaysiaInvoice` DataSetType 也會跳過
3. `IsCollectThreeCopiesInvoice("SG", ...)` 永遠 false → 不蒐集三聯式資訊
4. `IsShowInvoiceInformation("SG", "NotIssue"/"NoInvoiceRequired")` → false → 結帳頁不顯示發票區塊

### 潛在風險

> ⚠️ 若 SG 店的 `InvoiceIssueTypeDef = "NotIssue"` 且**ThirdParty gateway 未設定**，  
> `GetActualInvoiceIssueTypeDef()` 會強制覆寫為 `"Issue"`，  
> 導致 `IsIssuedInvoice = true`，前端會錯誤顯示發票區塊。  
>   
> **建議**：SG 店應明確設定 `InvoiceIssueTypeDef = "NoInvoiceRequired"`，  
> 而非使用 `NotIssue`，以避免 fallback 邏輯帶來的風險。

---

## 結帳頁表現（IsShowInvoiceInformation）

| SG 店設定 | 結帳頁顯示發票填寫區 |
|---|---|
| `NoInvoiceRequired` | ❌ 不顯示 |
| `NotIssue` | ❌ 不顯示（`countryCode != "TW"` 且非 `Issue`） |
| `Issue` | ✅ 顯示（但內容為 MY 格式，不適用 SG） |

---

## 相關程式碼位置

| 檔案 | 說明 |
|---|---|
| `InvoiceService.cs` L505 | `IsIssueInvoice()` 核心判斷 |
| `InvoiceService.cs` L542 | `IsShowInvoiceInformation()` 結帳頁顯示判斷 |
| `TradesOrderService.cs` L799 | `GetActualInvoiceIssueTypeDef()` 設定值修正 |
| `TradesOrderService.cs` L837 | `ProcessMyInvoice()` NMQ handler 端的第二層保護 |
| `CreateTradesOrderProcessor.cs` L480 | `IsIssuedInvoice` 寫入 DB |
| `CreateGlobalInvoiceProcessor.cs` | 推送 GenerateMyInvoiceTask（無 SG 特別排除） |
