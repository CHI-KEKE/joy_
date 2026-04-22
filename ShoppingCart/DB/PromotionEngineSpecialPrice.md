
   SELECT
       PromotionEngineSpecialPrice_Id,
	   PromotionEngineSpecialPrice_TypeDef,
       PromotionEngineSpecialPrice_ConditionId,
       PromotionEngineSpecialPrice_SalePageId,
       PromotionEngineSpecialPrice_SaleProductSKUId,
       PromotionEngineSpecialPrice_TypeValue,
       PromotionEngineSpecialPrice_ValidFlag,
	   *
   FROM PromotionEngineSpecialPrice
   WHERE PromotionEngineSpecialPrice_ValidFlag = 1
   --and PromotionEngineSpecialPrice_Id = 231260
   --and PromotionEngineSpecialPrice_PromotionEngineId = 39995
   and PromotionEngineSpecialPrice_SalePageId = 1557572
     --AND PromotionEngineSpecialPrice_ShopId = 12765
     --AND PromotionEngineSpecialPrice_PromotionEngineId = 39583





--40314
--slaepageid = 1557572, skuid =  
INSERT INTO dbo.PromotionEngineSpecialPrice
    (
     PromotionEngineSpecialPrice_ShopId,
     PromotionEngineSpecialPrice_SalePageId,
     PromotionEngineSpecialPrice_SaleProductSKUId,
     PromotionEngineSpecialPrice_PromotionEngineId,
     PromotionEngineSpecialPrice_TypeDef,
     PromotionEngineSpecialPrice_TypeValue,
     PromotionEngineSpecialPrice_CreatedDateTime,
     PromotionEngineSpecialPrice_CreatedUser,
     PromotionEngineSpecialPrice_UpdatedTimes,
     PromotionEngineSpecialPrice_UpdatedDateTime,
     PromotionEngineSpecialPrice_UpdatedUser,
     PromotionEngineSpecialPrice_ValidFlag,
     PromotionEngineSpecialPrice_ConditionId,
     PromotionEngineSpecialPrice_Sort
    )
VALUES
    (
     10230,
     1557672,
     2169174,
     40314,
     N'Price',
     2.00,
     getdate(),
     N'alen',
     0,
     getdate(),
     N'alen',
     1,
     1,
     3
    );