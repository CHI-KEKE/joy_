# 連結盤點：加購品列表頁（from=cart 購物車模式）

## 對應 Story

Story #582440：[滿額加價購][加價購商品] 對照商品加價購的加購頁的篩選邏輯

---

## 對應連結

```
https://rosielin666.shop.qa9.91dev.tw/V2/CartExtraPurchase/ExtraPurchaseList/38483?shopId=12765&from=cart&cartUniqueKey=72762ccb-0959-4dc9-98f5-d477dfeecde5
```

> 有 `from=cart` 且有 `cartUniqueKey` → 進入 **cart（購物車互動）模式**

---

## 與 viewOnly 模式的差異

| 項目 | viewOnly 模式 | cart 模式 |
|------|-------------|----------|
| 商品清單 API（SPL）| ✅ 呼叫 | ✅ 呼叫 |
| 已選加購品 API（Shopping）| ❌ 不呼叫 | ✅ **呼叫** |
| 客群驗證 | ❌ 略過 | ✅ **觸發** |
| 門檻達成狀態 | 全部 isReached: false | ✅ 依購物車金額計算 |
| 菜籃預填 | 無 | ✅ 從購物車帶入已選加購品 |
| 確認按鈕行為 | 無（純瀏覽）| ✅ 呼叫 `insertItem` 更新購物車 |

---

## 完整路徑流程

### Layer 1：MVC Controller（MobileWebMallV2）

```csharp
// CartExtraPurchaseController.cs — 完全相同，與 viewOnly 無差異
public ActionResult ExtraPurchaseList(long id)
{
    this.ViewBag.PromotionId = id;
    return this.View();
}
```

- ✅ 無任何篩選邏輯，純 return View

---

### Layer 2：React 前端（CartExtraPurchaseProvider.tsx）

初始化時並行呼叫兩支 API：

#### API 1：可選購加購品清單（SPL）

```
salepage-listing/api/salepage/cart-extra-purchase/{shopId}/{promotionId}
```

- 回傳所有可選購的加購品結構（含 conditions / salePageList / skuList）

#### API 2：購物車內已選加購品（Shopping API）

```
shopping/api/carts/salepage-cart-extra-purchase?promotionId={id}&cartUniqueKey={key}
```

回傳 `ICartExtraPurchaseItemsResponse`：

| 欄位 | 說明 |
|------|------|
| `purchasedItemPrice` | 購物車目前金額（用於門檻計算） |
| `isLackOfPrice` | 是否有任何門檻未達 |
| `salePageGroups` | 已選加購品清單（conditionId + salepageId + skuId + qty）|

---

### Layer 2 前端驗證 / 篩選機制

#### 1. 客群驗證（**cart 模式才觸發**）

```typescript
// validation.utility.ts
isMemberEligibleForCartExtraPurchase(
    pageMode,                          // = 'cart'
    promotionResponse.memberCollectionId,
    promotionMemberCollectionIdListRef.current
)
```

- `memberCollectionId === '-1'`（全會員）→ 通過
- 使用者的 memberCollectionIdList 包含此 ID → 通過
- 否則 → `setIsValid(false)`，顯示 ErrorBlock，不顯示商品清單

#### 2. 門檻狀態計算

```typescript
calculateThresholdStatusRef.current(
    cartItemsResponse.purchasedItemPrice,
    cartItemsResponse.isLackOfPrice
)
```

- 依各 condition 的 `reachPrice` / `reachPiece` 與購物車金額比對
- 結果存入 `thresholdStatusList`，用於 UI 顯示哪個 Tab 已達標

#### 3. 菜籃預填

```typescript
// 若購物車已有加購品，預填至菜籃
if (cartItemsResponse.salePageGroups.length > 0) {
    initBasketFromCartRef.current(cartItemsResponse.salePageGroups, sellingQtyList);
}
```

- 把已在購物車的加購品帶入菜籃，讓使用者看到已選狀態

#### 4. 庫存驗證（加入菜籃時）

```typescript
// handleConfirmAddToBasket()
if (skuInfo.StockQty < totalQty || skuInfo.SellingQty < totalQty) {
    showToast('超過可購數量');
    return;
}
```

- 加入菜籃前比對 `StockQty` 和 `SellingQty`，超量則阻擋

> **前端沒有商品類型篩選邏輯（IsClosed / Bundle / ShareToBuy）**，商品清單完全由 SPL API 決定。

---

### Layer 3：SPL API（CartExtraPurchaseService）— 與 viewOnly 完全相同

**API 路徑**：`salepage-listing/api/salepage/cart-extra-purchase/{shopId}/{promotionId}`

```
CartExtraPurchaseService.BuildPromotionEntitiesAsync()
  ↓
foreach salepage in salePageDic：
  ┌─ if (salepage == null) continue          ← 商品不存在 → 跳過
  └─ if (salepage.IsSoldOut) continue        ← 售完商品 → 跳過
  ↓（以下均無排除）
  ❌ IsClosed 商品頁關閉
  ❌ IsSalePageBundle 組合商品
  ❌ IsShareToBuy FB 分享後購買
  ❌ SellingTime 販售時間有效性
  ↓
回傳加購品清單
```

---

### Layer 4：更新購物車（handleUpdateCart）

使用者點「確認」後：

```
Step 1：先移除購物車中被取消的加購品
  → removeItemModel(shopId, salePageId, skuId, ...)

Step 2：將菜籃清單逐筆加入購物車
  → insertItem(shopId, salePageId, skuId, qty, ...)
  → API: shopping/api/carts 的加入商品 endpoint

Step 3：重導至購物車頁
  → window.location.href = /V2/ShoppingCart/Index?shopId={shopId}
```

---

## 現狀篩選機制彙整（cart 模式）

| 篩選項目 | 篩選位置 | 現狀 | Story AC 要求 |
|---------|---------|------|--------------|
| 售完商品（IsSoldOut） | SPL `BuildPromotionEntitiesAsync` | ✅ **有排除** | — |
| 商品頁關閉（IsClosed） | SPL `BuildPromotionEntitiesAsync` | ❌ **無排除**（且 Response 無欄位）| **需排除** |
| 組合商品（IsSalePageBundle） | SPL `BuildPromotionEntitiesAsync` | ❌ **無排除**（欄位存在但未使用）| **需排除** |
| FB 分享後購買（IsShareToBuy） | SPL `BuildPromotionEntitiesAsync` | ❌ **無排除**（且 Response 無欄位）| **需排除** |
| 活動有效性 | SPL 取活動資料時 | ✅ 已驗證 | — |
| 販售時間有效（SellingTime） | SPL `BuildPromotionEntitiesAsync` | ❌ **未驗證** | AC 要求三角檢核 |
| 客群驗證 | 前端（cart 模式觸發）| ✅ **有驗證** | — |
| 庫存上限（加入菜籃時）| 前端 `handleConfirmAddToBasket` | ✅ **有驗證** | — |

---

## 結論

- **商品清單篩選缺口與 viewOnly 完全相同**，缺口集中在 SPL `CartExtraPurchaseService`
- cart 模式額外的客群驗證、門檻計算、庫存驗證都已正確實作
- **修改位置不需因模式不同而區分**，補齊 SPL 篩選邏輯後，兩個模式同時受益

---

## 相關檔案路徑

| 檔案 | 路徑 |
|------|------|
| 前端 Provider | `...\ClientApp\src\promotion\components\cartExtraPurchase\providers\CartExtraPurchaseProvider.tsx` |
| 前端驗證工具 | `...\utils\validation.utility.ts` |
| 已選加購品 API Model | `...\models\cartExtraPurchase.model.ts` |
| 型別定義 | `...\models\models.d.ts` |
| SPL 主要修改點 | `C:\91APP\Salepage\salepage-listing\...\Cart\CartExtraPurchaseService.cs`（`BuildPromotionEntitiesAsync()`）|
