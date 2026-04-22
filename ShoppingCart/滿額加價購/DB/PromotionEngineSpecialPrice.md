


```sql
   SELECT
       PromotionEngineSpecialPrice_Id,
       PromotionEngineSpecialPrice_ConditionId,
       PromotionEngineSpecialPrice_SalePageId,
       PromotionEngineSpecialPrice_SaleProductSKUId,
       PromotionEngineSpecialPrice_TypeValue,
       PromotionEngineSpecialPrice_ValidFlag
   FROM PromotionEngineSpecialPrice
   WHERE PromotionEngineSpecialPrice_PromotionEngineId = 39583
     AND PromotionEngineSpecialPrice_ShopId = 12765
     AND PromotionEngineSpecialPrice_ValidFlag = 1
```