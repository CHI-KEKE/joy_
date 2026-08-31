
```sql
USE WebStoreDB
select PromotionEngineSetting_IsInStore,*
from PromotionEngineSetting(nolock)
where PromotionEngineSetting_ValidFlag = 1
order by PromotionEngineSetting_CreatedDateTime desc
```




## 找 + N 天活動


```sql
USE WebStoreDB;

SELECT
    PromotionEngineSetting_ExtendInfo,
    *
FROM PromotionEngine WITH (NOLOCK)
INNER JOIN PromotionEngineSetting WITH (NOLOCK)
    ON PromotionEngineSetting_PromotionEngineId = PromotionEngine_Id
WHERE PromotionEngine_ValidFlag = 1
  AND PromotionEngine_TypeDef IN (
      'RewardReachPriceWithCoupon',
      'RewardReachPriceWithRatePoint2',
      'RewardReachPriceWithPoint2'
  )
  AND PromotionEngine_EndDateTime > GETDATE()
  AND PromotionEngine_StartDateTime < GETDATE()
  AND EXISTS (
      SELECT 1
      FROM OPENJSON(PromotionEngineSetting_ExtendInfo)
      WITH (
          RewardDays INT '$.RewardDays'
      ) AS j
      WHERE j.RewardDays > 0
  );
```