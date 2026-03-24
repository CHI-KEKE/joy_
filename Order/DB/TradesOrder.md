
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


## 最新 ts

```sql
USE WebStoreDB
SELECT TradesOrderSlave_Id
FROM dbo.TradesOrderSlave WITH (NOLOCK)
where TradesOrderSlave_ValidFlag = 1
order by TradesOrderSlave_Id desc
```


## 驗證不應該有任何相同的 TS Code


```sql
use WebStoreDB
GO

SELECT t1.TradesOrderSlave_Code, t1.TradesOrderSlave_Code
FROM TradesOrderSlave t1
JOIN TradesOrderSlave t2
ON t1.TradesOrderSlave_Code = t2.TradesOrderSlave_Code
AND t1.TradesOrderSlave_Id <> t2.TradesOrderSlave_Id
WHERE t1.TradesOrderSlave_CreatedDateTime >= '2025-11-24 18:00:00'
AND t1.TradesOrderSlave_Id > 331300500
AND t2.TradesOrderSlave_Id > 331300500
```


## 箱購TS數


```sql
use WebStoreDB
GO

SELECT COUNT(TradesOrderSlave_Id) AS 箱購TS數
FROM dbo.TradesOrder WITH (NOLOCK)
JOIN dbo.TradesOrderSlave WITH (NOLOCK) ON TradesOrder.TradesOrder_Id = TradesOrderSlave.TradesOrderSlave_TradesOrderId and TradesOrderSlave_ValidFlag = 1
JOIN dbo.SalePage WITH (NOLOCK) ON TradesOrderSlave_SalePageId = SalePage_Id and SalePage_ValidFlag = 1
WHERE SalePage_IsSeparated = 1
AND TradesOrder_DateTime > '2025-11-24 18:00:00'
--AND TradesOrderSlave_Id > 331300500
```


## 抽查箱購訂單配號是否正確 

```sql
SELECT TOP 20 TradesOrderGroup_Code AS 'TgCode'
,TradesOrder_Code AS 'TmCode'
,TradesOrderSlave_Code AS 'TsCode', *
FROM dbo.TradesOrderGroup WITH (NOLOCK)
JOIN dbo.TradesOrder WITH (NOLOCK) ON TradesOrder_TradesOrderGroupId = TradesOrderGroup_Id
JOIN dbo.TradesOrderSlave WITH (NOLOCK) ON TradesOrder.TradesOrder_Id = TradesOrderSlave.TradesOrderSlave_TradesOrderId and TradesOrderSlave_ValidFlag = 1
JOIN dbo.SalePage WITH (NOLOCK) ON TradesOrderSlave_SalePageId = SalePage_Id and SalePage_ValidFlag = 1
WHERE SalePage_IsSeparated = 1
AND TradesOrder_DateTime > '2025-11-25 10:00:00'
AND TradesOrderSlave_Id > 331300500
```


## TradesOrderGroup + TradesOrderSlave + TradesOrder

```sql
-- 完整交易訂單結構查詢
SELECT 
    TradesOrderGroup_Code AS 訂單群組編號,
    TradesOrderSlave_Qty AS 商品數量,
    TradesOrder_Id AS 訂單ID,
    TradesOrderSlave_Id AS 明細ID,
    *
FROM TradesOrderGroup(NOLOCK)
INNER JOIN TradesOrder(NOLOCK)
    ON TradesOrder_TradesOrderGroupId = TradesOrderGroup_Id
INNER JOIN TradesOrderSlave(NOLOCK)
    ON TradesOrderSlave_TradesOrderId = TradesOrder_Id
WHERE TradesOrderGroup_ValidFlag = 1
    AND TradesOrderGroup_Code = 'TG250812PB0001'
    AND TradesOrderGroup_Code >= 'TG250808BA00LN'  -- 訂單編號範圍
    AND TradesOrderGroup_ShopId = 41571  -- 指定店鋪
    AND TradesOrderGroup_CrmShopMemberCardId IN (4521,4522,4523)  -- 指定會員卡
    AND TradesOrderGroup_TotalPayment >= 8800  -- 最小金額
    AND TradesOrderGroup_TrackSourceTypeDef IN ('AndriodApp','iOSApp')  -- 行動裝置來源
    -- AND TradesOrderGroup_CreatedDateTime >= '2025-08-08 11:00'  -- 可選：時間篩選
```