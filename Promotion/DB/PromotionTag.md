## PromotionTag

```sql
USE WebStoreDB
select PromotionTagSlave_TargetTypeCode,PromotionTagSlave_TargetTypeId,PromotionTagSlave_TargetTypeCode,PromotionTagSlave_ValidFlag,*
from PromotionTagSlave(nolock)
where PromotionTagSlave_PromotionTagId in (652441,651990,651462,654150)
--and PromotionTagSlave_ValidFlag = 1
 
 
select PromotionTag_ValidFlag,PromotionTag_IsClosed,*
from PromotionTag(nolock)
where PromotionTag_Id = 5145
```
