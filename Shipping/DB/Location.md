

```sql
use InfoDB

select Location_AreaId,Location_Domestic,*
from Location(nolock)
where Location_Domestic = 1 --海外
and Location_Id = 1830
```