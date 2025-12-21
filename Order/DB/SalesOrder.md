
## SalesOrderGroup

```sql
SELECT SalesOrderGroup_DateTime,SalesOrderGroup_ShopId,*
FROM SalesOrderGroup(NOLOCK)
WHERE SalesOrderGroup_TradesOrderGroupCode IN ('TG240229K00030','TG240118M00056')
```


## SalesOrderGroup + SalesOrder + SalesOrderSlave

```sql
-- 銷售訂單完整結構查詢
USE ERPDB;

SELECT 
    SalesOrderGroup_DateTime AS 訂單時間,
    SalesOrderGroup_ShopId AS 店鋪ID,
    SalesOrderSlave_StatusDef AS 明細狀態,
    SalesOrder_Id AS 訂單ID,
    SalesOrderSlave_Id AS 明細ID,
    *
FROM SalesOrderGroup(NOLOCK)
INNER JOIN SalesOrder(NOLOCK)
    ON SalesOrder_SalesOrderGroupId = SalesOrderGroup_Id
INNER JOIN SalesOrderSlave(NOLOCK)
    ON SalesOrder_Id = SalesOrderSlave_SalesOrderId
WHERE SalesOrderGroup_TradesOrderGroupCode IN ('TG250821M00003')  -- 指定交易群組編號
ORDER BY SalesOrderGroup_DateTime DESC;
```