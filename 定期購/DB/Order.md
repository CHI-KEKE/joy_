```sql
USE ERPDB;

SELECT *
FROM SalesOrderGroup(NOLOCK)
INNER JOIN SalesOrder(NOLOCK)
    ON SalesOrder_SalesOrderGroupId = SalesOrderGroup_Id
INNER JOIN SalesOrderSlave(NOLOCK)
    ON SalesOrder_Id = SalesOrderSlave_SalesOrderId
INNER JOIN RegularOrder(NOLOCK)
ON RegularOrder_TradesOrderSlaveId = SalesOrderSlave_TradesOrderSlaveId
--WHERE SalesOrderGroup_TradesOrderGroupCode IN ('TG250821M00003')  -- 指定交易群組編號
WHERE SalesOrder_DateTime > '2026-05-01'
ORDER BY SalesOrderGroup_DateTime DESC;USE ERPDB;

SELECT *
FROM SalesOrderGroup(NOLOCK)
INNER JOIN SalesOrder(NOLOCK)
    ON SalesOrder_SalesOrderGroupId = SalesOrderGroup_Id
INNER JOIN SalesOrderSlave(NOLOCK)
    ON SalesOrder_Id = SalesOrderSlave_SalesOrderId
INNER JOIN RegularOrder(NOLOCK)
ON RegularOrder_TradesOrderSlaveId = SalesOrderSlave_TradesOrderSlaveId
--WHERE SalesOrderGroup_TradesOrderGroupCode IN ('TG250821M00003')  -- 指定交易群組編號
WHERE SalesOrder_DateTime > '2026-05-01'
ORDER BY SalesOrderGroup_DateTime DESC;
```