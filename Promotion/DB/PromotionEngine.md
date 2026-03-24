

## PromotionEngine

```sql
use WebStoreDB

select PromotionEngine_TypeDef,PromotionEngine_Rule,PromotionEngine_StartDateTime,PromotionEngine_EndDateTime,PromotionEngine_GroupCode,*
from PromotionEngine(nolock)
where PromotionEngine_ValidFlag = 1
AND PromotionEngine_TypeDef IN ('RewardReachPriceWithCoupon','RewardReachPriceWithPoint2','RewardReachPriceWithRatePoint2')
AND PromotionEngine_StartDateTime > '2025-11-11'
AND PromotionEngine_EndDateTime < '2025-11-12'
AND PromotionEngine_EndDateTime > GETDATE()
```

<br>
<br>

## 只撈回饋活動

```sql
USE WebStoreDB
 
SELECT
PromotionEngine_Id,
PromotionEngine_Rule,
    PromotionEngine_TypeDef
*
FROM dbo.PromotionEngine(NOLOCK)
JOIN dbo.PromotionEngineSetting(NOLOCK)
    ON PromotionEngineSetting_ValidFlag = 1
    AND PromotionEngineSetting_PromotionEngineId = PromotionEngine_Id
WHERE PromotionEngine_ValidFlag = 1
    AND PromotionEngine_TypeDef IN ('RewardReachPriceWithPoint2', 'RewardReachPriceWithRatePoint2')
    AND PromotionEngine_Id = 5812
    and PromotionEngine_ShopId not in (8,2373)
    and PromotionEngine_TargetTypeDef in ('SalePage','Category')
    and PromotionEngine_ExcludeTargetTypeDef in ('SalePage','Category')
ORDER BY PromotionEngine_CreatedDateTime DESC
```

<br>
<br>

## JsonRule


```sql
use WebStoreDB

SELECT PromotionEngine_ShopId as ShopId,
       Shop_Name as ShopName,
       PromotionEngine_Id as PromotionEngineId,
	   PromotionEngine_StartDateTime,
	   PromotionEngine_EndDateTime,
       PromotionEngine_Name as PromotionEngineName,
       case
           when PromotionEngine_TypeDef = 'RewardReachPriceWithCoupon' then N'滿額給券'
           when PromotionEngine_TypeDef = 'RewardReachPriceWithPoint2' then N'滿額給點'
           when PromotionEngine_TypeDef = 'RewardReachPriceWithRatePoint2' then N'比例給點'
           else 'Error'
       end as PromotionEngineTypeDef,
       CASE
            WHEN JSON_VALUE(PromotionEngine_Rule, '$.MatchedSalesChannels') = 24 THEN N'線下'
            WHEN CAST(JSON_VALUE(PromotionEngine_Rule, '$.MatchedSalesChannels') AS INT) <= 7 THEN N'線上'
            ELSE N'線上 + 線下'
       END AS SalesChannelType,
       JSON_VALUE(PromotionEngine_Rule, '$.MatchedSalesChannels') AS '適用通路',
        JSON_VALUE(PromotionEngine_Rule, '$.IncludedLocationScopes[0].LocationScopeType') AS '適用通路 - 指定門市',
        JSON_VALUE(PromotionEngine_Rule, '$.IncludedProductScopes[0].ProductScopeType') AS '活動範圍 - 指定商品',
        JSON_VALUE(PromotionEngine_Rule, '$.ExcludedProductScopes[0].ProductScopeType') AS '活動範圍 - 排除商品',
        JSON_VALUE(PromotionEngine_Rule, '$.Cyclable') AS '活動內容 - 循環累計',
        CASE
        WHEN ISJSON(PromotionEngineSetting_ExtendInfo) > 0
        THEN JSON_VALUE(PromotionEngineSetting_ExtendInfo, '$[0].RewardDays')  -- 如果是有效 JSON，提取 'name' 值
        ELSE NULL
    END AS '給點規則 - 訂單完成後N天給點',
    JSON_VALUE(PromotionEngine_Rule, '$.MatchedSalesChannels') AS MatchedSalesChannelsValue,
	JSON_QUERY(CAST(PromotionEngine_Rule AS nvarchar(max)), '$.Thresholds')  AS Thresholds,
	PromotionEngineSetting_ExtendInfo
FROM dbo.PromotionEngine WITH (NOLOCK)
INNER JOIN dbo.Shop WITH (NOLOCK)
    ON PromotionEngine_ShopId = Shop_Id
inner join PromotionEngineSetting
on PromotionEngine_Id = PromotionEngineSetting_PromotionEngineId
    AND Shop_ValidFlag = 1
WHERE PromotionEngine_ValidFlag = 1
  AND PromotionEngine_TypeDef IN ('RewardReachPriceWithRatePoint2','RewardReachPriceWithPoint2','RewardReachPriceWithCoupon')
AND PromotionEngine_StartDateTime >= '2025-08-15'
AND PromotionEngine_StartDateTime < '2025-08-16'
AND PromotionEngine_EndDateTime > GETDATE()
ORDER BY PromotionEngine_ShopId,PromotionEngine_Id;
```



