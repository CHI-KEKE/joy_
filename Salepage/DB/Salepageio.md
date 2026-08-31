```sql
-- 組合商品 salepageid :  593720
select SalePageIO_SalePageId,SalePageIO_Action,SalePageIO_ChangeColumn,SalePageIO_ChangeToValue,SalePageIO_CreatedDateTime,SalePageIO_Requester,*
from SalePageIO(nolock)
where SalePageIO_ValidFlag = 1
and SalePageIO_SalePageId =   593720
order by SalePageIO.SalePageIO_CreatedDateTime desc
```