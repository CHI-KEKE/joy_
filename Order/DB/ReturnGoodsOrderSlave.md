## 退貨單

```sql
uSE ERPDB

SELECT OrderSlaveFlow_CancelOrderSlaveId,ReturnGoodsOrderSlave_IsClosed,OrderSlaveFlow_TradesOrderSlaveStatusDef,*
FROM OrderSlaveFlow(NOLOCK)
left join ReturnGoodsOrderSlave(nolock)
on OrderSlaveFlow_TradesOrderSlaveId = ReturnGoodsOrderSlave_TradesOrderSlaveId
WHERE OrderSlaveFlow_ValidFlag = 1
AND OrderSlaveFlow_TradesOrderSlaveCode = 'TS250701P000011'
```
