
6fa741fd-870a-4ac4-b532-9ece687499b7 tgid = 16179 MG251230L00006
6d16b91e-1597-4a88-863a-0b59e6a2e5c0 tgid = 16177 MG251230L00004

執行 csp_TradesOrderTransToSalesOrderWithFlow_Mall 掛掉



```sql
SELECT @checkTradesOrderGroupId = [OrderSlaveFlow_TradesOrderGroupId]
    , @checkStatusDef = [OrderSlaveFlow_StatusDef]
FROM dbo.OrderSlaveFlow 
WHERE [OrderSlaveFlow_TradesOrderGroupId] = @runId;
```


```sql
use ERPDB
select *
from OrderSlaveFlow(nolock)
where OrderSlaveFlow_ValidFlag = 1
and OrderSlaveFlow_TradesOrderGroupId in (16179,16177)
```

確實是空的...why


確實有執行 step1

csp_ImportWebStoreDBTradesOrdersToERPDBSourceTablesByOrderId_Mall

但沒有報錯

沒有更新成

WHERE OrderSlaveFlow_StatusDef IN
      ('WaitingToTrans','TransToCancel','WaitingToThirdPartyTrans') !!!


看起來再 pay 階段就 fail的 不會再當下更新大表 如果停在 waitingToTrans 就不轉了