# IsIssueInvoice 核心邏輯

**位置**：`WebStore/Frontend/BLV2/Invoice/InvoiceService.cs` L505  
**呼叫時機**：`CreateTradesOrderProcessor` 建立訂單時，決定 `TradesOrderSlave_IsIssuedInvoice` 欄位值

---

## 方法簽名

```csharp
bool IsIssueInvoice(
    string countryCode,           // 來自 ShippingArea.CountryAliasCode（如 "TW"、"MY"、"SG"）
    string invoiceIssueTypeDef,   // 來自 ShopDefault InvoiceIssueTypeDef（GetActualInvoiceIssueTypeDef 計算後）
    PayProfileTypeDefEnum payType,
    ...
)
```

---

## 完整判斷邏輯（虛擬碼）

```csharp
// Step 1: 付款方式黑名單檢查
var notIssueInvoicePayTypes = new[] { PoyaPay, Wallet, RazerPay };
//  ⚠️ 注意：這裡的 RazerPay 是 PaymentMiddleware 的舊 GlobalPay 封裝
//           CreditCardOnce_Razer / GrabPay_Razer / PayNow_Razer 是不同的 Enum 值，不在此列

if (notIssueInvoicePayTypes.Contains(payType))
    return false;

// Step 2: InvoiceIssueTypeDef 判斷
if (invoiceIssueTypeDef == "Issue")
    return true;   // 不限國家，直接代開

if (invoiceIssueTypeDef == "OverseaNotIssue" && countryCode == "TW")
    return true;   // 只有 TW 才開

// "NotIssue" / "NoInvoiceRequired" → false（不在上述條件內）
return false;
```

---

## 各 InvoiceIssueTypeDef 與 countryCode 組合結果

| InvoiceIssueTypeDef | countryCode | 結果 |
|---|---|---|
| `Issue` | 任何 | ✅ `true` |
| `OverseaNotIssue` | `TW` | ✅ `true` |
| `OverseaNotIssue` | `MY` / `SG` / 其他 | ❌ `false` |
| `NotIssue` | 任何 | ❌ `false` |
| `NoInvoiceRequired` | 任何 | ❌ `false` |

---

## GetActualInvoiceIssueTypeDef() 的前處理

**位置**：`TradesOrderService.cs` L799  
**目的**：ShopDefault 設定值不一定等於最終使用值，有以下修正邏輯：

```
若 ShopDefault 設定 = "NotIssue" 或 "OverseaNotIssue"
    └─ 且商店尚未啟用第三方特店 ShopPaymentGateway
        → 強制改為 "Issue"
        ⚠️ 防止商店轉移期間（尚未設定 ThirdParty gateway）導致完全無法開票
```

---

## 付款方式黑名單詳細說明

| Enum 名稱 | 是否在黑名單 | 說明 |
|---|---|---|
| `PoyaPay` | ✅ 在 | POYA 紅利支付 |
| `Wallet` | ✅ 在 | 錢包支付 |
| `RazerPay` | ✅ 在 | **GlobalPay 封裝版本**（舊，值 = 4398046511104） |
| `CreditCardOnce_Razer` | ❌ 不在 | SG/MY Razer 直接整合 |
| `GrabPay_Razer` | ❌ 不在 | SG GrabPay |
| `PayNow_Razer` | ❌ 不在 | SG PayNow |

---

## 相關方法

| 方法 | 說明 |
|---|---|
| `IsCollectThreeCopiesInvoice(countryCode, invoiceIssueTypeDef)` | 是否蒐集三聯式發票（TW 專用，SG/MY 永遠 false） |
| `IsShowInvoiceInformation(countryCode, invoiceIssueTypeDef)` | 結帳頁是否顯示發票填寫區 |
