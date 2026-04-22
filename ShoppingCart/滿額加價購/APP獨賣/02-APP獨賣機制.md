# 02 — APP 獨賣機制

> 本文件只討論**商品頁維度**：`SalePage_IsAPPOnly`。

---

## `SalePage_IsAPPOnly` 是什麼

| 項目 | 說明 |
|------|------|
| DB 欄位 | `SalePage.SalePage_IsAPPOnly (bool?)` |
| 設定路徑 | OSM > 商品頁 > 限購設定 > APP 獨賣設定 |
| 語意 | 此商品限定在 APP 上才能購買 |

---

## 各層是否讀取 `SalePage_IsAPPOnly`

### SPL（Salepage Listing Service）

| 用途 | 是否讀取 | 說明 |
|------|---------|------|
| `SalePageInfoEntity.IsAPPOnly` | ✅ 讀取 | 用於**商品頁展示**（單一商品頁 API 回應） |
| `SkuQueryEntity.IsAPPOnly` | ✅ 讀取 | 用於 **SKU 選擇 Popup** |
| `CouponSalepageEntity.IsAPPOnly` | ✅ 讀取 | 用於**優惠券適用商品清單** |
| `GetSalepageListByIdsResponse` | ❌ **無此欄位** | 加購品清單用的 response，完全沒有 IsAPPOnly |
| `CartExtraPurchaseService.BuildSalePageEntityAsync` | ❌ **不讀取** | 加購品組裝邏輯只過濾 `IsSoldOut` |

```csharp
// CartExtraPurchaseService.cs L640 — 唯一的商品過濾條件
if (salepage.IsSoldOut)
{
    continue;  // APP 獨賣商品不會被此處過濾
}
```

**結論：SPL 的加購品清單流程完全不受 `SalePage_IsAPPOnly` 影響。**

---

### Cart Domain（Cart API）

| 用途 | 是否讀取 | 說明 |
|------|---------|------|
| 所有 Processor | ❌ **完全不讀取** | Cart Domain 新流程無任何邏輯讀此欄位 |
| `PurchaseExtraRepository.cs` | ✅ 讀取 | 僅限**舊版商品加價購**流程（已被新流程取代）|

**結論：Cart Domain 新流程完全忽略 `SalePage_IsAPPOnly`。**

---

### MWeb 前端（CartClientApp）

| 用途 | 是否讀取 | 說明 |
|------|---------|------|
| 加購 Modal 商品 `isAPPOnly` | ❌ **硬碼 false** | `DEFAULT_IS_APP_ONLY = false`（cartExtraPurchase.utility.ts L14） |
| `ISalepage` 介面 | ❌ **無此欄位** | types.ts 中 ISalepage 介面不含 isAPPOnly |

```typescript
// cartExtraPurchase.utility.ts L14, L36
const DEFAULT_IS_APP_ONLY = false;

return {
    isAPPOnly: DEFAULT_IS_APP_ONLY,  // 所有加購品在 Modal 中永遠為 false
};
```

**結論：前端新版加購流程完全不看商品層級的 APP-only。**

---

## SPL 的 SalePageModes — APP 獨賣不在其中

SPL 透過 `SalePageModes` 字串表達商品屬性，但**無 APP 獨賣 mode**：

```csharp
// Nine1.Salepage.Listing.Common.Utils.Extension.SalepageModesEnum
public enum SalepageModesEnum
{
    PP,  // 點加金
    A,   // 18禁商品（非 APP 獨賣）
    B,   // 搶先曝光
    S,   // 規格表
    C,   // 組合商品
    L,   // 門市商品
    M,   // OMO 商品
}
```

`SalesChannel` 只區分 `Online` / `InStore`，沒有 App vs Web 的維度。

---

## 整體結論

`SalePage_IsAPPOnly` 只在**商品頁展示**、**SKU Popup**、**優惠券**三個情境有效。  
在整個**滿額加價購加購品流程**（SPL → Cart Domain → MWeb 前端）中，此欄位**完全不影響任何行為**。
