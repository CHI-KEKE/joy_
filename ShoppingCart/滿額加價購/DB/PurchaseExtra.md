```sql
select PurchaseExtra_ValidFlag,*
from PurchaseExtra(nolock)
where PurchaseExtra_Id = 88

--select PurchaseExtraSlave_SalePageId,*
--from PurchaseExtra(nolock)
--inner join PurchaseExtraSlave(nolock)
--on PurchaseExtra_Id = PurchaseExtraSlave_PurchaseExtraId
--where PurchaseExtra_ValidFlag = 1
----and PurchaseExtra_Id = 
--and PurchaseExtraSlave_SalePageId = 1554701


update PurchaseExtra
set PurchaseExtra_ValidFlag = 1
where PurchaseExtra_Id = 88

select PurchaseExtra_ValidFlag, *
from PurchaseExtra(nolock)
where PurchaseExtra_Id = 88
```