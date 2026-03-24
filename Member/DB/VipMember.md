
## VipMember

```sql
SELECT *
FROM dbo.VipMember WITH(NOLOCK)
WHERE VipMember_ShopId = 4
AND VipMember_CellPhone LIKE '%88887766%'


use WebStoreDB
--nineyimemberid = 125301667
select Member_Id,*
from VipMemberInfo(nolock)
inner join Member(nolock)
on Member_Id = VipMemberInfo_MemberId
where VipMemberInfo_VipMemberId = 125301667
```
