```sql
use WebStoreDB

select SaleProductSKU_Id,SaleProductSKU_GoodsSKUId,*
from SalePage(nolock)
inner join SaleProductSKU(nolock)
inner join ProductStock(nolock)
on ProductStock_SaleProductSKUId = SaleProductSKU_SaleProductId
on SaleProductSKU_SalePageId = SalePage_Id
where SalePage_Id = 1557557

```