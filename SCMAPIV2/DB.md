

```sql
use ERPDB

select top 10 *
from SupplierApiProfile(nolock)
where SupplierApiProfile_ValidFlag = 1
```

```sql
select top 100 * from dbo.SupplierApiProfile with(nolock)
where SupplierApiProfile_ValidFlag = 1
and SupplierApiProfile_Token = '30671037';
```