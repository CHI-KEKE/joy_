
`CrmMember_NineYiMemberId`
`CrmSalesOrder_CrmMemberId`
`CrmSalesOrder_OuterOrderCode1`
`CrmSalesOrder_TypeDef`

<br>
<br>

## CrmSalesOrder

```sql
SELECT 
    CrmSalesOrder_Id AS 訂單ID,
    CrmSalesOrder_TypeDef AS 訂單類型,
    CrmSalesOrder_TradesOrderFinishDateTime AS 完成時間,
    CrmSalesOrder_TotalPayment AS 總金額,
    CrmSalesOrder_CrmShopMemberCardId AS 會員卡ID
FROM CrmSalesOrder(NOLOCK)
WHERE CrmSalesOrder_ValidFlag = 1
    AND CrmSalesOrder_TradesOrderFinishDateTime > '2025-08-25'
    AND CrmSalesOrder_TypeDef = 'Others'  -- 線下訂單
    -- AND CrmSalesOrder_TotalPayment > 3000  -- 可選：金額篩選
    AND CrmSalesOrder_ShopId = 41571
    AND CrmSalesOrder_CrmShopMemberCardId > 4521;
    AND CrmSalesOrder_OuterOrderCode1 = 'uat301A';
```

## CrmSalesOrder + CrmMember

```sql
-- 基礎會員訂單查詢 (含會員資料)
USE CRMDB;

SELECT *
FROM CrmSalesOrder(NOLOCK)
INNER JOIN CrmMember(NOLOCK)
    ON CrmSalesOrder_CrmMemberId = CrmMember_Id
WHERE CrmSalesOrder_ValidFlag = 1;
```

## CrmSalesOrderSlave

```sql
-- 訂單明細基本查詢
USE CRMDB;

SELECT 
    CrmSalesOrderSlave_Id AS 明細ID,
    CrmSalesOrderSlave_CrmMemberId AS 會員ID,
    CrmSalesOrderSlave_TotalPayment AS 明細金額,
    CrmSalesOrderSlave_DataSourceType AS 資料來源類型,
    CrmSalesOrderSlave_OriginalCrmSalesOrderId AS 原始訂單ID,
    CrmSalesOrderSlave_OriginalCrmSalesOrderSlaveId AS 原始明細ID,
    CrmSalesOrderSlave_Qty AS 數量,
    *
FROM CrmSalesOrderSlave(NOLOCK)
WHERE CrmSalesOrderSlave_ValidFlag = 1
    AND CrmSalesOrderSlave_CrmSalesOrderId = 329081
    AND CrmSalesOrderSlave_OuterProductSkuCode IN ('IW7385','IW4988','IR5789');
```

## CrmSalesOrder + CrmSalesOrderSlave + CrmMember

```sql
-- 會員訂單完整分析 (含商品明細與會員資料)
USE CRMDB;

SELECT TOP 1000 
    CrmSalesOrder_ShopId AS 店鋪ID,
    CrmSalesOrderSlave_Price AS 商品價格,
    CrmSalesOrderSlave_Qty AS 購買數量,
    CrmSalesOrderSlave_PurchaseType AS 購買類型,
    CrmSalesOrderSlave_TypeMemo AS 類型備註,
    CrmSalesOrderSlave_TotalPayment AS 明細金額,
    CrmMember_ShopId AS 會員店鋪ID,
    CrmSalesOrderSlave_OriginalCrmSalesOrderId AS 原始訂單ID,
    CrmSalesOrderSlave_OriginalCrmSalesOrderSlaveId AS 原始明細ID,
    CrmMember_Id AS 會員ID,
    CrmSalesOrder_TradesOrderFinishDateTime AS 訂單完成時間,
    CrmSalesOrder_Id AS 訂單ID,
    CrmSalesOrder_TypeDef AS 訂單類型,
    CrmSalesOrder_OuterOrderCode1 AS 外部訂單號,
    ROW_NUMBER() OVER(PARTITION BY CrmSalesOrder_Id ORDER BY CrmSalesOrderSlave_Id DESC) AS 明細排序
FROM CrmSalesOrder(NOLOCK)
INNER JOIN CrmSalesOrderSlave(NOLOCK)
    ON CrmSalesOrder_Id = CrmSalesOrderSlave_CrmSalesOrderId
INNER JOIN CrmMember(NOLOCK)
    ON CrmMember_Id = CrmSalesOrder_CrmMemberId
WHERE CrmMember_ShopId = 12
    AND CrmSalesOrder_Id = 26112651;
     WHERE CrmMember_ShopId = 2  -- 指定店鋪
        AND CrmSalesOrderSlave_PurchaseType = 'Normal'  -- 一般購買
        AND CrmSalesOrder_TypeDef = 'Others'  -- 線下訂單   
```

## CrmMember

```sql
-- 九易會員ID查詢
USE CRMDB;

SELECT 
    CrmMember_NineYiMemberId AS 九易會員ID,
    CrmMember_Id AS CRM會員ID,
    CrmMember_ShopId AS 店鋪ID,
    *
FROM CrmMember(NOLOCK)
WHERE CrmMember_Id = 33309;
```

## 簡單組合技

```sql
use CRMDB

select CrmSalesOrder_CreatedDateTime,CrmSalesOrder_ShopId,CrmSalesOrder_TradesOrderFinishDateTime,CrmSalesOrder_TypeDef,*
from CrmSalesOrder(nolock)
where CrmSalesOrder_ValidFlag = 1
and CrmSalesOrder_Id = 7360537
--and CrmSalesOrder_Id = 7353045 測試


select CrmSalesOrderSlave_CreatedDateTime,CrmSalesOrderSlave_CalculateMemberTierDateTime,CrmSalesOrderSlave_CrmMemberId,CrmSalesOrderSlave_PurchaseType,CrmSalesOrderSlave_TypeMemo,*
from CrmSalesOrderSlave(nolock)
where CrmSalesOrderSlave_ValidFlag = 1
and CrmSalesOrderSlave_CrmSalesOrderId = 7360537
--and CrmSalesOrderSlave_CrmMemberId = 224041 -- 測試


select CrmMember_NineYiMemberId,CrmMember_ShopId,*
from CrmMember(nolock)
where CrmMember_ValidFlag = 1
and CrmMember_Id = 1761228
--and CrmMember_NineYiMemberId = 427029 -- 測試用
```


## 撈 MemberTier 天數

```SQL
use CRMDB

-- 線下 活動SHOPID = 400,41602

--| day | startTime | endTime     | rewardTime  |
--| --- | --------- | ----------- | ----------- |
--| 1   | 10/30     | 10/31 00:00 | 10/31 04:06 |
--| 2   | 10/31     | 11/01 00:00 | 11/01 04:06 |
--| 3   | 11/01     | 11/02 00:00 | 11/02 04:06 |
--| 4   | 11/02     | 11/03 00:00 | 11/03 04:06 |
--| 5   | 11/03     | 11/04 00:00 | 11/04 04:06 |
--| 6   | 11/04     | 11/05 00:00 | 11/05 04:06 |

--declare @shopId BIGINT = 12;
DECLARE @memberTierStartTime DATETIME = '2025-10-30'; 
DECLARE @countDats int = 1;   -- 從第一天開始跑
DECLARE @totalDays int = 6;   -- 要跑幾天：5 天 = 10/30 ~ 11/04

DROP TABLE IF EXISTS #HK_Orders;
CREATE TABLE #HK_Orders(
    shopId BIGINT,
    CrmSalesOrderSlave_CrmSalesOrderId BIGINT,
	MemberTier DATETIME,
	CreatedDatetime DATETIME
);

while(@countDats <= @totalDays)
BEGIN

DECLARE @memberTierEndTime DATETIME = DATEADD(DAY, 1, @memberTierStartTime);
DECLARE @rewardDispatherTime DATETIME = DATEADD(MINUTE, 246, @memberTierEndTime);

    INSERT INTO #HK_Orders(shopId, CrmSalesOrderSlave_CrmSalesOrderId,MemberTier,CreatedDatetime)
	SELECT CrmSalesOrderSlave_ShopId,CrmSalesOrderSlave_CrmSalesOrderId,CrmSalesOrderSlave_CalculateMemberTierDateTime,CrmSalesOrderSlave_CreatedDateTime
	FROM CrmSalesOrderSlave(NOLOCK)
		inner join CrmSalesOrder(nolock)
	on CrmSalesOrderSlave_CrmSalesOrderId = CrmSalesOrder_Id
	WHERE CrmSalesOrderSlave_ValidFlag = 1
		AND CrmSalesOrderSlave_ShopId IN (12,22,45,55,63,76,82)
		AND CrmSalesOrderSlave_CalculateMemberTierDateTime >= @memberTierStartTime
		AND CrmSalesOrderSlave_CalculateMemberTierDateTime < @memberTierEndTime
		AND CrmSalesOrderSlave_CreatedDateTime > @rewardDispatherTime
		and CrmSalesOrder_TypeDef = 'Others'

    SET @memberTierStartTime = DATEADD(DAY, 1, @memberTierStartTime);
    SET @countDats = @countDats + 1;
END


SELECT *
FROM #HK_Orders
DROP TABLE IF EXISTS #HK_Orders;
```