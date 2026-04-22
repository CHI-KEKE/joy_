


```sql
select PromotionEngine_Sort,*
from PromotionEngine(nolock)
where PromotionEngine_ValidFlag = 1
and PromotionEngine_ShopId = 12765
and PromotionEngine_TypeDef = 'CartReachPriceExtraPurchase'
--and PromotionEngine_Name IN (N'allen-test-2-滿額加價購')
and PromotionEngine_Name IN (N'[D]不指定x多階x合併', N'[D]不指定x多階x不可合併','[D]未來活動')
order by PromotionEngine_CreatedDateTime desc


UPDATE PromotionEngine
SET PromotionEngine_ValidFlag = 1
where PromotionEngine_ValidFlag = 0
and PromotionEngine_ShopId = 12765
and PromotionEngine_TypeDef = 'CartReachPriceExtraPurchase'
and PromotionEngine_Name IN (N'[D]不指定x多階x合併', N'[D]不指定x多階x不可合併','[D]未來活動')
--and PromotionEngine_Name IN (N'allen-test-2-滿額加價購')
--order by PromotionEngine_CreatedDateTime desc

select PromotionEngine_Sort,*
from PromotionEngine(nolock)
where PromotionEngine_ValidFlag = 1
and PromotionEngine_ShopId = 12765
and PromotionEngine_TypeDef = 'CartReachPriceExtraPurchase'
--and PromotionEngine_Name IN (N'[D]不指定x多階x合併', N'[D]不指定x多階x不可合併','[D]未來活動')
and PromotionEngine_Name IN (N'allen-test-2-滿額加價購')
order by PromotionEngine_CreatedDateTime desc

```