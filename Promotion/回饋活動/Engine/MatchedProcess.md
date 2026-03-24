

## MatchedProcess

- 取得 threshold
- GetMatchedProductScopeItems
  - SalePrice 小到大
  - Id 大到小
- totalPrice - 符合條件 SalePrice 的 Sum
- 依序從最高門檻往下看有過哪一個門檻
- RestrictPoints 限制最大點數
- 屬於 Merged


## 計算方式說明 (比例給點屬於 Merged)

**Independent獨立計算**：Product.GroupKey 會是 TS，每一張 TS.SalessPrice * priceRateAndPointPair.Rate，最後 SUM 出這整張 TG 的 points
**Merged 合併計算**：TotalPrice 直接 * priceRateAndPointPair.Rate

<br>

## 產生 PromotionRecord

- PromotionId
- SourceType = Promotion
- Group = CartCalculate
- Point = 會有最大給點數的限制
- PurchasedItemIds
- NeedAmortization = no need to 攤提
