


## CancelOrderSlave

<br>

```sql
select *
from CancelOrderSlave(nolock)
where CancelOrderSlave_Id = 12285
```

## CancelRequest

```sql
uSE WebStoreDB
select CancelRequest_TradesOrderSlaveId,*
from CancelRequest(nolock)
where CancelRequest_TradesOrderSlaveCode = 'TS250701P000011'
```