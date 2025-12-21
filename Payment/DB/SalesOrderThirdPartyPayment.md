## SalesOrderThirdPartyPayment


```sql
-- 銷售訂單支付狀態查詢
USE ERPDB;

SELECT 
    SalesOrderSlave_Id AS 明細ID,
    SalesOrder_TradesOrderGroupId AS 交易群組ID,
    SalesOrderSlave_StatusDef AS 明細狀態,
    SalesOrderThirdPartyPayment_StatusDef AS 支付狀態,
    SalesOrder_PayProfileTypeDef AS 支付方式,
    SalesOrderSlave_TradesOrderSlaveCode AS 交易明細編號
FROM dbo.SalesOrderSlave WITH (NOLOCK)
INNER JOIN dbo.SalesOrder SO WITH(NOLOCK) 
    ON SO.SalesOrder_Id = SalesOrderSlave.SalesOrderSlave_SalesOrderId 
    AND SalesOrder_ValidFlag = 1
INNER JOIN dbo.SalesOrderThirdPartyPayment WITH (NOLOCK) 
    ON SalesOrderThirdPartyPayment_ValidFlag = 1 
    AND SalesOrderThirdPartyPayment_TradesOrderGroupId = SalesOrder_TradesOrderGroupId
WHERE SalesOrderSlave_ValidFlag = 1
    -- AND SalesOrderSlave_TradesOrderSlaveCode IN ('TS250828J000019','TS250828J000013','TS250828J000025')  -- 可選：指定明細編號
ORDER BY SalesOrderSlave_Id DESC;
```