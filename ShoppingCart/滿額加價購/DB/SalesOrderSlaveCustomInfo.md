```sql
USE ERPDB;

SELECT COUNT(DISTINCT sog.SalesOrderGroup_TradesOrderGroupCode)
FROM SalesOrderCustomInfo sci WITH (NOLOCK)
INNER JOIN SalesOrderSlave sos WITH (NOLOCK)
    ON sos.SalesOrderSlave_TradesOrderSlaveCode = sci.SalesOrderCustomInfo_SourceCode
INNER JOIN SalesOrder so WITH (NOLOCK)
    ON so.SalesOrder_Id = sos.SalesOrderSlave_SalesOrderId
INNER JOIN SalesOrderGroup sog WITH (NOLOCK)
    ON sog.SalesOrderGroup_Id = so.SalesOrder_SalesOrderGroupId
WHERE sci.SalesOrderCustomInfo_ValidFlag = 1
  AND sci.SalesOrderCustomInfo_SourceType = 'CartExtraPurchase'
  AND sci.SalesOrderCustomInfo_ShopId = 16;




USE ERPDB;
--COUNT(DISTINCT sog.SalesOrderGroup_TradesOrderGroupCode)
SELECT sog.SalesOrderGroup_TradesOrderGroupCode,*
FROM SalesOrderCustomInfo sci WITH (NOLOCK)
INNER JOIN SalesOrderSlave sos WITH (NOLOCK)
    ON sos.SalesOrderSlave_TradesOrderSlaveCode = sci.SalesOrderCustomInfo_SourceCode
INNER JOIN SalesOrder so WITH (NOLOCK)
    ON so.SalesOrder_Id = sos.SalesOrderSlave_SalesOrderId
INNER JOIN SalesOrderGroup sog WITH (NOLOCK)
    ON sog.SalesOrderGroup_Id = so.SalesOrder_SalesOrderGroupId
WHERE sci.SalesOrderCustomInfo_ValidFlag = 1
  AND sci.SalesOrderCustomInfo_SourceType = 'CartExtraPurchase'
  --AND sci.SalesOrderCustomInfo_ShopId = 16
  --and sci.SalesOrderCustomInfo_CreatedDateTime BETWEEN '2026-05-01 16:00' and '2026-05-01 18:00'
  and sos.SalesOrderSlave_ItemType = 'C'
```