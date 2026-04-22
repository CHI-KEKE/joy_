
select PromotionEngine_Sort,*
from PromotionEngine(nolock)
where PromotionEngine_ValidFlag = 1
and PromotionEngine_ShopId = 12765
and PromotionEngine_TypeDef = 'CartReachPieceExtraPurchase'
--and PromotionEngine_Name IN (N'allen-test-2-滿額加價購')
--and PromotionEngine_Name IN (N'[D]不指定x多階x合併', N'[D]不指定x多階x不可合併','[D]未來活動')
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
and PromotionEngine_TypeDef = 'CartReachPieceExtraPurchase'
--and PromotionEngine_Name IN (N'[D]不指定x多階x合併', N'[D]不指定x多階x不可合併','[D]未來活動')
--and PromotionEngine_Name IN (N'allen-test-2-滿額加價購')
order by PromotionEngine_CreatedDateTime desc




USE WebStoreDB
GO

SELECT PromotionEngine_Id, PromotionEngine_Rule,*
 FROM DBO.PromotionEngine WITH (NOLOCK)
 WHERE PromotionEngine_ShopId = 12765
AND  PromotionEngine_ValidFlag = 1
--AND PromotionEngine_Rule LIKE '%"MatchedUserScopes":null%'
and PromotionEngine_Id = 40274

update PromotionEngine
set PromotionEngine_Rule = N'{"TypeFullName":"NineYi.Msa.Promotion.Rule.CartReachPieceExtraPurchase","Id":40274,"Name":"測試滿件加價購001","Enabled":true,"Description":"測試滿件加價購活動說明","Since":"2026-04-01T00:00:00","Until":"2026-12-31T23:59:59","UpdatedAt":"2026-04-15T23:13:09.564538+08:00","Cyclable":false,"Accumulated":false,"IncludedProductScopes":[{"ProductScopeType":"NineYi.Msa.Tagging.TagProductScope","Tag":"Collection:f_435216037622860800"}],"ExcludedProductScopes":null,"IncludeRegionScopes":[{"RegionScopeType":"NineYi.Msa.Promotion.Engine.AllCountryRegionScope"}],"MatchedUserScopes":[{"UserScopeType":"NineYi.Msa.Promotion.Engine.AllUserScope"}],"VisibleUserScopes":[{"UserScopeType":"NineYi.Msa.Promotion.Engine.AllUserScope"}],"MatchedSalesChannels":7,"VisibleSalesChannels":7,"IncludedLocationScopes":null,"IsLimitedAddOnsPurchaseQty":false,"IsBirthdayMonthEnabled":false,"CartExtraPurchaseThresholdEligibleSetting":{"IsCartExtraPurchaseDiscountReachPriceWithFreeGift":false,"IsCartExtraPurchaseRegisterReachPiece":false,"IsCartExtraPurchaseRegisterReachPrice":false},"TagsWhenMatched":["ECouponExcludedByPromotion"],"CartExtraPurchaseConditions":[{"ConditionId":1,"ReachPrice":0.0,"ReachPiece":3,"LimitQty":-1}]}'
 WHERE PromotionEngine_ShopId = 12765
AND  PromotionEngine_ValidFlag = 1
--AND PromotionEngine_Rule LIKE '%"MatchedUserScopes":null%'
and PromotionEngine_Id = 40274


SELECT PromotionEngine_Id, PromotionEngine_Rule,PromotionEngine_Sort,*
 FROM DBO.PromotionEngine WITH (NOLOCK)
 WHERE PromotionEngine_ShopId = 10230
AND  PromotionEngine_ValidFlag = 1
and PromotionEngine_Id = 40314