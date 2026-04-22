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

## 前端 / 篩選機制

> 商品清單完全由 SPL API 回傳決定

---

## SPL API（CartExtraPurchaseService）

**API 路徑**：`salepage-listing/api/salepage/cart-extra-purchase/{shopId}/{promotionId}`
**內部呼叫鏈**：

```csharp
foreach salepage in salePageDic：
  ┌─ if (salepage == null) continue          ← 商品不存在 → 跳過
  └─ if (salepage.IsSoldOut) continue        ← 售完商品 → 跳過
  ↓
BuildSalePageEntityAsync()  → 組裝 CartExtraPurchaseSalePageEntity
  ↓
回傳加購品清單
```

## SPS 查詢參數

| 參數 | 值 | 說明 |
|------|----|------|
| `IsIncludingInvisible` | `true` | 含隱賣商品（後置由商店開關決定，但滿額加購未實作此後置）|
| `IsIncludingShareToBuy` | `true`（硬寫）| 含 FB 分享後購買商品 |