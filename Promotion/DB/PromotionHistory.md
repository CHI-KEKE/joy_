
```sql
SELECT PromotionEngine_Rule,PromotionEngine_UpdatedDateTime,*
FROM History.PromotionEngine(NOLOCK)
--WHERE PromotionEngine_Id = 5776
order by PromotionEngine_CreatedDateTime desc
```