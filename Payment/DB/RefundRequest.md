```sql
use ERPDB

select RefundRequest_ResponseMsg,RefundRequest_TransactionId,RefundRequest_CreatedDateTime,*
from RefundRequest(nolock)
where RefundRequest_ValidFlag = 1
and RefundRequest_Id in (121537,124652)
and RefundRequest_ResponseMsg like '%Business Rules Incorrect!%'
and RefundRequest_StatusDef in ('RefundRequestFail','RefundRequestGrouping')
and RefundRequest_UpdatedDateTime > '2025-07-01'
```


```sql
use ERPDB

select SalesOrderSlave_ShopId,RefundRequest_TradesOrderGroupCode,SalesOrderSlave_TradesOrderSlaveCode,RefundRequest_StatusDef,SalesOrderSlave_StatusDef,RefundRequest_PayProfileTypeDef,RefundRequest_Amount,RefundRequest_UpdatedUser,RefundRequest_CreatedUser,SalesOrderSlave_DateTime,RefundRequest_CreatedDateTime,RefundRequest_UpdatedDateTime,RefundRequest_ResponseMsg,RefundRequest_TransactionId,RefundRequest_CreatedDateTime
from RefundRequest(nolock)
INNER JOIN SalesOrderSlave(NOLOCK)
    ON RefundRequest_TradesOrderSlaveCode = SalesOrderSlave_TradesOrderSlaveCode
--INNER JOIN SalesOrder(NOLOCK)
--    ON SalesOrder_Id = SalesOrderSlave_SalesOrderId
--inner join SalesOrderGroup(NOLOCK)
--on SalesOrder_TradesOrderGroupId = SalesOrderGroup_TradesOrderGroupId
where RefundRequest_ValidFlag = 1
--and RefundRequest_Id in (121537,124652)
--and RefundRequest_ResponseMsg like '%Business Rules Incorrect!%'
--and RefundRequest_StatusDef in ('RefundRequestFail','RefundRequestGrouping')
--and RefundRequest_UpdatedDateTime > '2026-03-02'
and RefundRequest_CreatedDateTime > '2026-03-02'
--and RefundRequest_ResponseMsg = N'51,退款失败：SYSTEM_EXCEPTION'
and SalesOrderSlave_ShopId in (15,50)
and RefundRequest_PayProfileTypeDef = 'AliPayHK_EftPay'
order by RefundRequest_UpdatedDateTime asc

```