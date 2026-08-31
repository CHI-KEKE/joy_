```sql
--子商品 SKUId:  4090706, salepageid  :　589830
select SaleProductSKU_SalePageId,SaleProductSKUIO_UpdatedUser,SaleProductSKUIO_UpdatedDateTime,SaleProductSKUIO_SaleProductSKUId,SaleProductSKUIO_ChangeValue,SaleProductSKUIO_ChangeToValue,SaleProductSKUIO_ChangeColumn,*
from SaleProductSKUIO(nolock)
inner join SaleProductSKU(nolock)
on SaleProductSKU_Id = SaleProductSKUIO_SaleProductSKUId
where SaleProductSKUIO_SaleProductSKUId = 4090706

```