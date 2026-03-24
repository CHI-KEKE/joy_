## Supplier


```sql
-- SalesCurrency => SGD
use ERPDB

select Shop_SupplierId,*
from Shop(nolock)
where Shop_Id = 200136

select Supplier_SalesCurrency,Supplier_PayTypeDef,*
from Supplier(nolock)
where Supplier_Id = 200105
```