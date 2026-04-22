# By-Shop 發票相關設定（ShopDefault）

---

## 設定位置

**Enum 定義**：`WebStore/Frontend/BE/ShopDefault/ShopDefaultKeyEnum.cs`  
**讀取方式**：透過 `ShopDefaultService.GetShopDefault(shopId, key)` 取得各店設定值

---

## Group 24：Invoice（發票基礎設定）

### InvoiceIssueTypeDef（Key = 26）

**最核心的發票設定，決定整個發票機制的行為。**

| 設定值 | 說明 | IsIssueInvoice 結果 |
|---|---|---|
| `Issue` | 91APP 代開（系統自動開立） | ✅ true（不限國家） |
| `OverseaNotIssue` | 海外不開立，僅 TW 開立 | ✅ true（僅 TW）；其他國家 false |
| `NotIssue` | 第三方特店自開，商店自行負責 | ❌ false（但有例外，見下方） |
| `NoInvoiceRequired` | 不需要發票（如 SG、無發票需求市場） | ❌ false |

> ⚠️ **`NotIssue` 的例外情況**：若商店尚未啟用 ThirdParty ShopPaymentGateway，  
> `GetActualInvoiceIssueTypeDef()` 會強制覆寫為 `"Issue"`，防止建單期間完全無法開票。

---

### InvoiceTypeDef（Key = 100）

**商店的發票種類設定**（主要在 `NotIssue` 時使用）

| 設定值 | 說明 |
|---|---|
| `NoInvoiceRequired` | 不需要發票 |
| （其他 TW 發票種類） | 對應台灣電子發票格式 |

---

## Group 49：ThirdPartyEInvoice（第三方自開發票設定）

當 `InvoiceIssueTypeDef = "NotIssue"` 時，商店自行串接第三方發票系統，以下設定生效：

| Key | Enum 名稱 | 說明 |
|---|---|---|
| 82 | `ThirdPartyVendor` | 第三方發票廠商：`TRADEVAN` / `Universal` / `Standard` / `EasyStore` |
| 83 | `ThirdPartyAPIUrl` | 第三方發票 API 路徑 |
| 84 | `IsEnableInvoiceDescription` | 發票說明開關（`Issue`/`OverseaNotIssue` 時強制 false） |
| 85 | `IsEnableInvoiceEmail` | 發票 Email 開關 |

> **相關服務**：`ThirdPartyElectronicInvoiceSettingsService`  
> 管理第三方廠商設定的讀取與快取

---

## 設定與流程關係圖

```
ShopDefault InvoiceIssueTypeDef
    │
    ├─ "Issue"
    │    └─ GetActualInvoiceIssueTypeDef() → 維持 "Issue"
    │         └─ IsIssueInvoice() → true → 91APP 代開發票
    │
    ├─ "OverseaNotIssue"
    │    └─ GetActualInvoiceIssueTypeDef()
    │         ├─ ThirdParty gateway 啟用 → 維持 "OverseaNotIssue"
    │         └─ ThirdParty gateway 未啟用 → 改為 "Issue" ⚠️
    │
    ├─ "NotIssue"
    │    └─ GetActualInvoiceIssueTypeDef()
    │         ├─ ThirdParty gateway 啟用 → 維持 "NotIssue" → 商店自開
    │         └─ ThirdParty gateway 未啟用 → 改為 "Issue" ⚠️
    │              （ThirdPartyEInvoice Group 49 設定此時生效）
    │
    └─ "NoInvoiceRequired"
         └─ GetActualInvoiceIssueTypeDef() → 維持 "NoInvoiceRequired"
              └─ IsIssueInvoice() → false → 不開立任何發票
```

---

## 查詢對應表（ShopDefaultKeyEnum 速查）

| GroupId | GroupName | KeyId | KeyName | 用途 |
|---|---|---|---|---|
| 24 | Invoice | 26 | InvoiceIssueTypeDef | 發票處理方式（最核心） |
| 24 | Invoice | 100 | InvoiceTypeDef | 發票種類 |
| 49 | ThirdPartyEInvoice | 82 | ThirdPartyVendor | 第三方廠商 |
| 49 | ThirdPartyEInvoice | 83 | ThirdPartyAPIUrl | 廠商 API |
| 49 | ThirdPartyEInvoice | 84 | IsEnableInvoiceDescription | 發票說明開關 |
| 49 | ThirdPartyEInvoice | 85 | IsEnableInvoiceEmail | Email 開關 |
