
```sql
USE WebStoreDB
select PromotionEngineSetting_IsInStore,*
from PromotionEngineSetting(nolock)
where PromotionEngineSetting_ValidFlag = 1
order by PromotionEngineSetting_CreatedDateTime desc
```