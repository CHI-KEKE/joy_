
## SalesOrderReceiver / SalesOrderGroup / SalesOrder

```sql
use ERPDB

select *
from SalesOrderReceiver(nolock)
where SalesOrderReceiver_ValidFlag = 1
and SalesOrderReceiver_SalesOrderId = 15247;


select SalesOrderGroup_Id,*
from SalesOrderGroup(nolock)
where SalesOrderGroup_ValidFlag = 1
and SalesOrderGroup_TradesOrderGroupCode = 'MG251126U00005'

select *
from SalesOrder(nolock)
where SalesOrder_ValidFlag = 1
and SalesOrder_SalesOrderGroupId = 15252
```


## TradesOrderGroup
```sql
select *
from TradesOrderGroup(nolock)
where TradesOrderGroup_ValidFlag = 1
and TradesOrderGroup_Code in ('TG251121K00002',
'TG251121K00003',
'TG251121K00004',
'TG251121K00006')
```