# 連結盤點：加購品列表頁（from=cart 購物車模式）

## 對應 Story

Story #582440：[滿額加價購][加價購商品] 對照商品加價購的加購頁的篩選邏輯

---

## 對應連結

```
https://rosielin666.shop.qa9.91dev.tw/V2/CartExtraPurchase/ExtraPurchaseList/38483?shopId=12765&from=cart&cartUniqueKey=72762ccb-0959-4dc9-98f5-d477dfeecde5
```

> 有 `from=cart` 且有 `cartUniqueKey` → 進入 **cart（購物車互動）模式**


## MWeb

並行呼叫兩支 API

#### API 1：可選購加購品清單（SPL）

```
salepage-listing/api/salepage/cart-extra-purchase/{shopId}/{promotionId}
```

**商品清單完全由 SPL API 決定。**

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

## SPL API（CartExtraPurchaseService）— 與 viewOnly 完全相同

**API 路徑**：`salepage-listing/api/salepage/cart-extra-purchase/{shopId}/{promotionId}`

```bash
CartExtraPurchaseService.BuildPromotionEntitiesAsync()
  ↓
foreach salepage in salePageDic：
  ┌─ if (salepage == null) continue          ← 商品不存在 → 跳過
  └─ if (salepage.IsSoldOut) continue        ← 售完商品 → 跳過
  ↓
回傳加購品清單
```
