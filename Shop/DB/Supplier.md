```SQL
--SupplierContact_SupplierId = 79
use ERPDB
select *
from Supplier(nolock)

use WebStore
select *
from Supplier(nolock)

select Supplier_SalesMarket,*
from Supplier(nolock)
WHERE Supplier_SalesMarket = 'SG'
```
