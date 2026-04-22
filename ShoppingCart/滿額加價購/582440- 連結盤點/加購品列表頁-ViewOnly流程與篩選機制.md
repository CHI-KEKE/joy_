# 連結盤點：加購品列表頁（ViewOnly 模式）

## 對應 Story

Story #582440：[滿額加價購][加價購商品] 對照商品加價購的加購頁的篩選邏輯

---

## 對應連結

```
https://rosielin666.shop.qa9.91dev.tw/V2/CartExtraPurchase/ExtraPurchaseList/38483?shopId=12765
```

> 無 `from=cart`、無 `cartUniqueKey`，進入 **viewOnly（純瀏覽）模式**

---

## 完整路徑流程

### Layer 1：MVC Controller（MobileWebMallV2）

**路由**：`/V2/CartExtraPurchase/ExtraPurchaseList/{id}?shopId={shopId}`

```csharp
// CartExtraPurchaseController.cs
public ActionResult ExtraPurchaseList(long id)
{
    this.ViewBag.PromotionId = id;  // 僅傳入 PromotionId = 38483
    return this.View();             // 直接 return React 頁面
}
```

- ✅ **無任何篩選邏輯**，純 return View

---

### Layer 2：React 前端（CartExtraPurchaseProvider.tsx）

頁面初始化後並行呼叫 API：

| API | 用途 | 本路徑是否呼叫 |
|-----|------|--------------|
| `salepage-listing/api/salepage/cart-extra-purchase/{shopId}/{promotionId}` | 可選購加購品清單（SPL）| ✅ 一定呼叫 |
| `shopping/api/carts/salepage-cart-extra-purchase?promotionId=...&cartUniqueKey=...` | 購物車內已選加購品 | ❌ **不呼叫**（無 from=cart）|

#### 前端驗證 / 篩選機制

| 機制 | 說明 | 本路徑是否觸發 |
|------|------|--------------|
| `isMemberEligibleForCartExtraPurchase()` 客群驗證 | pageMode !== 'cart' → 直接 return true | ❌ **略過** |
| 門檻狀態計算（reachPrice / reachPiece） | 無 cartItemsResponse → 所有條件 isReached: false | ✅ 觸發（但僅顯示用）|
| 庫存查詢 `getSellingQtyListNewModel()` | 查 SKU 即時庫存，顯示售完狀態 | ✅ 觸發 |

> **前端本身無商品類型篩選邏輯**，商品清單完全由 SPL API 回傳決定。

---

### Layer 3：SPL API（CartExtraPurchaseService）

這是**商品清單的實際篩選層**。

**API 路徑**：`salepage-listing/api/salepage/cart-extra-purchase/{shopId}/{promotionId}`

**內部呼叫鏈**：

```
CartExtraPurchaseController（SPL）
  ↓
CartExtraPurchaseService.BuildPromotionEntitiesAsync()
  ↓
BuildSalePageDictionaryAsync()
  → SalepageService.SalepagesSearchByIdsAsync()
    → SPS（SearchForPresentationAsync）
      → MapResponse() → GetSalepageListByIdsResponse
  ↓
foreach salepage in salePageDic：
  ┌─ if (salepage == null) continue          ← 商品不存在 → 跳過
  └─ if (salepage.IsSoldOut) continue        ← 售完商品 → 跳過
  ↓
BuildSalePageEntityAsync()  → 組裝 CartExtraPurchaseSalePageEntity
  ↓
回傳加購品清單
```

#### SPS 查詢參數

| 參數 | 值 | 說明 |
|------|----|------|
| `IsIncludingInvisible` | `true` | 含隱賣商品（後置由商店開關決定，但滿額加購未實作此後置）|
| `IsIncludingShareToBuy` | `true`（硬寫）| 含 FB 分享後購買商品 |

---

## 現狀篩選機制彙整

| 篩選項目 | 篩選位置 | 現狀 | Story AC 要求 |
|---------|---------|------|--------------|
| 售完商品（IsSoldOut） | SPL `BuildPromotionEntitiesAsync` | ✅ **有排除** | — |
| 商品頁關閉（IsClosed） | SPL `BuildPromotionEntitiesAsync` | ❌ **無排除**（且 Response 無欄位）| **需排除** |
| 組合商品（IsSalePageBundle） | SPL `BuildPromotionEntitiesAsync` | ❌ **無排除**（欄位存在但未使用）| **需排除** |
| FB 分享後購買（IsShareToBuy） | SPL `BuildPromotionEntitiesAsync` | ❌ **無排除**（且 Response 無欄位）| **需排除** |
| 活動有效性 | SPL 取活動資料時驗證 | ✅ 已驗證 | — |
| 販售時間有效（SellingTime 範圍）| SPL `BuildPromotionEntitiesAsync` | ❌ **未驗證** | AC 要求三角檢核 |
| 會員客群 | 前端（from=cart 時）| ✅ 有驗證（viewOnly 略過）| — |

---

## 需要修改的位置

**主要修改檔案**：`CartExtraPurchaseService.cs`（SPL Repo）

### 修改 1：補 `IsClosed` 排除
需搭配：
- `GetSalepageListByIdsResponse` 新增 `IsClosed` 屬性
- `SalepageService.MapResponse()` 加 `IsClosed = salepage.IsClosed` 映射
- `BuildPromotionEntitiesAsync()` 加 `if (salepage.IsClosed) continue`

### 修改 2：補 `IsSalePageBundle` 排除
- 欄位已存在（`SalePageModes.ContainMode("C")`）
- 直接加 `if (salepage.IsSalePageBundle) continue`

### 修改 3：補 `IsShareToBuy` 排除
需搭配：
- `GetSalepageListByIdsResponse` 新增 `IsShareToBuy` 屬性
- `SalepageService.MapResponse()` 加 `IsShareToBuy = salepage.IsShareToBuy` 映射
- `BuildPromotionEntitiesAsync()` 加 `if (salepage.IsShareToBuy) continue`

### 修改 4：補販售時間有效性（SellingTime）三角檢核
對照商品加價購的排除條件：
```csharp
// SalepageExtraPurchaseService.cs 參考範本
if (salepageInfo.SellingStartDateTime > DateTime.Now
    || salepageInfo.SellingEndDateTime < DateTime.Now)
{
    continue;
}
```
- `GetSalepageListByIdsResponse` 已有 `SellingStartDateTime` / `SellingEndDateTime` 欄位
- 直接加條件即可

---

## 相關檔案路徑

| 檔案 | 路徑 |
|------|------|
| MVC Controller（MWeb）| `C:\91APP\nineyi.webstore.mobilewebmall\WebStore\Frontend\MobileWebMallV2\Controllers\CartExtraPurchaseController.cs` |
| 前端 Provider | `...\ClientApp\src\promotion\components\cartExtraPurchase\providers\CartExtraPurchaseProvider.tsx` |
| 前端驗證工具 | `...\utils\validation.utility.ts` |
| SPL 主要修改點 | `C:\91APP\Salepage\salepage-listing\...\Cart\CartExtraPurchaseService.cs`（`BuildPromotionEntitiesAsync()`，L639 附近）|
| Response Model | `C:\91APP\Salepage\salepage-listing\...\GetSalepageListByIdsResponse.cs` |
| MapResponse | `C:\91APP\Salepage\salepage-listing\...\Salepage\SalepageService.cs`（`MapResponse()`，L693）|
| 商品加價購參考範本 | `C:\91APP\Salepage\salepage-listing\...\Salepage\SalepageExtraPurchaseService.cs`（L388-405）|
