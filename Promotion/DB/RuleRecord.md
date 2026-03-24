```sql
SELECT PromotionEngineRuleRecord_UpdatedDateTime,PromotionEngineRuleRecord_S3Key,PromotionEngineRuleRecord_Version,*
FROM PromotionEngineRuleRecord(NOLOCK)
--WHERE PromotionEngineRuleRecord_PromotionEngineId = 5768
--WHERE PromotionEngineRuleRecord_PromotionEngineTypeDef = 'RewardReachPriceWithPoint'
order by PromotionEngineRuleRecord_CreatedDateTime desc
```