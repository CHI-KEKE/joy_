# 04 — BUG 根因（分享活動）

> 本文件只討論**商品頁維度**：`SalePage.IsShareToBuy`。

---

## 調查結論：現狀不存在 Bug

**`IsShareToBuy = true` 的商品作為加購品，在新版流程中完全不受影響。**  
不需要修改任何程式碼。

---

## 為什麼不受影響

### `IsShareToBuy` 在新版加購品流程中的讀取狀況

| 系統 / 層級 | 是否讀取此欄位 | 根據 |
|------------|--------------|------|
| SPL `SalepageService.SalepagesSearchByIdsAsync()` | ✅ 有設定，但設為**包含** | L235 `IsIncludingShareToBuy = true` |
| SPL `CartExtraPurchaseService` | ❌ 不直接讀取，透過上述方法取得資料 | 呼叫 `SalepagesSearchByIdsAsync()` |
| Cart Domain `SalepageEntity` | ❌ **無此欄位** | 加購品主 Entity 不含此屬性 |
| Cart Domain 所有 Processor（加購/結帳） | ❌ **完全不讀** | 搜尋全域無 `IsShareToBuy` 讀取 |
| MWeb BLV2 ShoppingCartsV2 Processor | ❌ **完全不讀** | 搜尋全域無此判斷 |
| CartClientApp `ISalepage` | ❌ **無此欄位** | 前端不顯示分享相關標籤 |

---

## `IsIncludingShareToBuy = true` 的關鍵作用

SPS 預設行為：**過濾掉** `IsShareToBuy = true` 的商品（讓分享活動商品不出現在一般清單）。

`SalepageService.SalepagesSearchByIdsAsync()` 在 L235 強制設定 `IsIncludingShareToBuy = true`，  
繞過此預設過濾，使 `IsShareToBuy = true` 的商品可以出現在查詢結果中。

```csharp
// SalepageService.cs L235
requestEntity.Filters.IsIncludingShareToBuy = true;
```

由於 `CartExtraPurchaseService.BuildSalePageDictionaryAsync()` 呼叫此方法，  
加購清單可正常顯示分享活動商品，**不需要額外處理**。

---

## 舊流程中存在問題的地方

舊版 `addToCartDirectiveController.ts` 對所有商品一律觸發分享驗證，  
包含加購品（未區分 `IsCartExtraPurchase`）。  
**但新版加購流程不走此路徑，此問題已自然消失。**

---

## 不需要改 Code 的理由

1. 分享活動商品正常出現在 Web 端加購清單 ✅
2. 商品可以被加入購物車，無分享 Dialog 阻斷 ✅
3. 購物車中不顯示分享活動相關標籤 ✅
4. 結帳流程無任何 `IsShareToBuy` 驗證阻擋 ✅
