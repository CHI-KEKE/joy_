# 商品加價購 — FB 分享後購買（IsShareToBuy）邏輯現狀

## 概述

說明商品加價購（SalepageExtraPurchaseService）在顯示加購品清單時，
對於「FB 分享後購買（IsShareToBuy）商品」的處理現狀。

---

## IsShareToBuy 是什麼

- SPS index 欄位：`isShareToBuy`（bool）
- C# Model：`SalepageListingResponseDataSalepageModel.IsShareToBuy`（bool）
- **意義**：商品被設定為「需要在 FB 分享後才能購買」的特殊商品
- SPS 預設行為：若不傳 `IsIncludingShareToBuy = true`，此類商品**不會出現**在搜尋結果中

---

## 完整呼叫路徑

```
前端 React
  GET ${FtsApiDomain}/salepage-listing/api/salepage/add-ons/{shopId}/{salepageId}
  ↓
SalepageExtraPurchaseController
  ↓
SalepageExtraPurchaseService.GetSalepageSearchDataAsync()
  ↓
SalepageListingRequestEntity 組裝：
  requestEntity.Filters.IsIncludingInvisible = true    ← 含入隱賣商品
  requestEntity.Filters.IsIncludingShareToBuy = true   ← 含入分享後購買商品（L354）
  ↓
SearchForPresentationAsync（SPS 搜尋）
  ↓ 回傳 SalepageListingResponseDataSalepageModel（含 IsShareToBuy 欄位）
  ↓
foreach salepageInfo：
  if (IsClosed || SellingTime 範圍外) continue;              ← 排除關閉/非開賣
  if (!isEnableHiddenAddOnsSalePage && IsInvisible) continue; ← 依店家設定排除隱賣
  // IsShareToBuy 判斷？ ← ❌ 沒有，ShareToBuy 商品照常通過
  ↓
回傳加購品清單（ShareToBuy 商品顯示）
```

---

## SPS Request 與 Response Model 欄位對照

| 欄位 | 位置 | 說明 |
|------|------|------|
| `IsIncludingShareToBuy` | `SalepageListingRequestEntity.Filters` | 傳給 SPS 的查詢參數，設為 `true` = 要求 SPS 回傳 ShareToBuy 商品 |
| `IsShareToBuy` | `SalepageListingResponseDataSalepageModel` | SPS 回傳值，`true` = 此商品為分享後購買商品 |

---

## 隱賣 vs ShareToBuy 的處理模式對比

| 類型 | SPS 請求 | C# 後置過濾 | 最終行為 |
|------|---------|------------|---------|
| 隱賣（IsInvisible） | `IsIncludingInvisible = true` | ✅ 依 `isEnableHiddenAddOnsSalePage` 店家設定決定 | **條件顯示** |
| 分享後購買（IsShareToBuy） | `IsIncludingShareToBuy = true` | ❌ 無任何後置過濾 | **一律顯示** |

> 這個差異說明設計意圖：
> - **隱賣商品**：先拿到，再依功能開關決定要不要顯示 → **可控**
> - **ShareToBuy 商品**：故意含入且不過濾 → **刻意決定要顯示**

---

## 結論

商品加價購**刻意顯示** ShareToBuy 商品：
1. `IsIncludingShareToBuy = true` 主動告訴 SPS 要含入
2. C# 層完全沒有後置過濾
3. `SalepageListingResponseDataSalepageModel.IsShareToBuy` 欄位存在，技術上可以排除，但**設計上選擇不排除**

---

## 相關檔案路徑

| 檔案 | 路徑 |
|------|------|
| 商品加價購 Service | `C:\91APP\Salepage\salepage-listing\...\Salepage\SalepageExtraPurchaseService.cs` |
| SPS Request Filters | `SalepageListingRequestEntity.Filters.IsIncludingShareToBuy`（L354）|
| SPS Response Model | `SalepageListingResponseDataSalepageModel.IsShareToBuy`（L117-118）|
