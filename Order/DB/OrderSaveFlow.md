
## OrderSlaveFlow
```sql
select OrderSlaveFlow_StatusDef,OrderSlaveFlow_StatusForUserDef,OrderSlaveFlow_StatusForSCMDef,OrderSlaveFlow_SalesOrderSlaveStatusDef,*
from OrderSlaveFlow(nolock)
where OrderSlaveFlow_ValidFlag = 1
and OrderSlaveFlow_TradesOrderGroupCode = 'TG250626K00002'
```


## ReturnGoodsOrderSlave join OrderSlaveFlow
```sql
uSE ERPDB

SELECT OrderSlaveFlow_CancelOrderSlaveId,ReturnGoodsOrderSlave_IsClosed,OrderSlaveFlow_TradesOrderSlaveStatusDef,*
FROM OrderSlaveFlow(NOLOCK)
left join ReturnGoodsOrderSlave(nolock)
on OrderSlaveFlow_TradesOrderSlaveId = ReturnGoodsOrderSlave_TradesOrderSlaveId
WHERE OrderSlaveFlow_ValidFlag = 1
AND OrderSlaveFlow_TradesOrderSlaveCode = 'TS250701P000011'
```
