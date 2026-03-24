```sql
use WebStoreDB

select *
from MemberLocation(nolock)
where MemberLocation_ValidFlag = 1
and MemberLocation_ShopId = 83

order by MemberLocation_CreatedDateTime desc
```

### zipCode

```sql
select *
from ZipCode(nolock)
```