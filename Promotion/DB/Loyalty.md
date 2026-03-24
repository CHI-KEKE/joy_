


## 撈取點中紀錄跟訂單狀態的語法，適用線上訂單

```sql

-- @shopId（可為 NULL，代表不限店）
-- @promotionEngineId
-- @orderCode（可能是 TG 開頭的 Group Code，也可能是 TS 開頭的 Slave Code）

USE ERPDB
GO
DECLARE @shopId BIGINT = 8; -- Step01: ShopId(nullable)
DECLARE @promotionEngineId INT = 484885; -- Step02: PromotionEngineId
DECLARE @orderCode VARCHAR(20) = 'TG250826QA00FH'; -- Step03: TG or TS
DECLARE @tgCode VARCHAR(20) = null;

--把 orderCode 統一轉成 TG Code
IF(@orderCode LIKE 'TG%')
    BEGIN
        SET @tgCode = @orderCode;
    END
ELSE
    BEGIN
        SELECT
            @tgCode = OrderSlaveFlow_TradesOrderGroupCode
        FROM dbo.OrderSlaveFlow WITH (NOLOCK)
        WHERE OrderSlaveFlow_TradesOrderSlaveCode = @orderCode
        AND (@shopId IS NULL
        OR OrderSlaveFlow_SalesOrderSlaveShopId = @shopId);
    END


DECLARE @tgId BIGINT;

-- 用 TG Code 找到 TGId（TradesOrderGroupId）
SELECT @tgId = OrderSlaveFlow_TradesOrderGroupId
FROM dbo.OrderSlaveFlow WITH (NOLOCK)
WHERE OrderSlaveFlow_TradesOrderGroupCode = @tgCode
AND OrderSlaveFlow_ValidFlag = 1;

-- 把這張 TG 底下相關的「OccurTypeId 前綴」整理成一個暫存表 #tmpLTOccurTypeId
-- 包含兩種前綴：
--     TGCode|PromotionEngineId
--     TSCode|PromotionEngineId
-- 以及該訂單時間、shopId、vipMemberId
SELECT
    OrderSlaveFlow_TradesOrderGroupCode
   ,OrderSlaveFlow_TradesOrderSlaveCode
   ,OrderSlaveFlow_TotalQty
   ,CONCAT('''', OrderSlaveFlow_TradesOrderGroupCode, '|', @promotionEngineId, '''') AS LT_TG_OccurTypeId
   ,CONCAT('''', OrderSlaveFlow_TradesOrderSlaveCode, '|', @promotionEngineId, '''') AS LT_TS_OccurTypeId
   ,CONCAT(@promotionEngineId, '_', @tgCode) AS DDB_Key
   ,CONCAT('QA/BookingRewardLoyaltyPointsDispatcherV2Job/', @shopId, '/', @promotionEngineId, '/', OrderSlaveFlow_MemberId) AS 'BookingRewardLoyaltyPointsDispatcherV2Job_S3Path'
   ,CONCAT(@promotionEngineId, '_', @tgCode, '_', OrderSlaveFlow_TradesOrderSlaveCode, '_', (OrderSlaveFlow_TotalQty - 1)) AS Max_Slave_DDBKey
   ,OrderSlaveFlow_CreatedDateTime
   ,*
FROM dbo.OrderSlaveFlow WITH (NOLOCK)
WHERE OrderSlaveFlow_TradesOrderGroupId = @tgId
AND (@shopId IS NULL
OR OrderSlaveFlow_SalesOrderSlaveShopId = @shopId);


DROP TABLE IF EXISTS #tmpLTOccurTypeId;
CREATE TABLE #tmpLTOccurTypeId (tmpOccurTypeId VARCHAR(200), tmpCreateDateTime DATETIME, tmpShopId BIGINT, tmpVipMemberId BIGINT);


INSERT INTO #tmpLTOccurTypeId
    SELECT DISTINCT
        CONCAT(OrderSlaveFlow_TradesOrderGroupCode, '|', @promotionEngineId)
       ,OrderSlaveFlow_CreatedDateTime
       ,OrderSlaveFlow_SalesOrderSlaveShopId
       ,VipMemberInfo_VipMemberId
    FROM dbo.OrderSlaveFlow WITH (NOLOCK)
    INNER JOIN dbo.VipMemberInfo WITH (NOLOCK)
        ON OrderSlaveFlow_MemberId = VipMemberInfo_MemberId
        AND OrderSlaveFlow_SalesOrderSlaveShopId = VipMemberInfo_ShopId
        AND VipMemberInfo_ValidFlag = 1
    WHERE OrderSlaveFlow_TradesOrderGroupId = @tgId
    AND (@shopId IS NULL
    OR OrderSlaveFlow_SalesOrderSlaveShopId = @shopId)
    UNION ALL
    SELECT
        CONCAT(OrderSlaveFlow_TradesOrderSlaveCode, '|', @promotionEngineId)
       ,OrderSlaveFlow_CreatedDateTime
       ,OrderSlaveFlow_SalesOrderSlaveShopId
       ,VipMemberInfo_VipMemberId
    FROM dbo.OrderSlaveFlow WITH (NOLOCK)
    INNER JOIN dbo.VipMemberInfo WITH (NOLOCK)
        ON OrderSlaveFlow_MemberId = VipMemberInfo_MemberId
        AND OrderSlaveFlow_SalesOrderSlaveShopId = VipMemberInfo_ShopId
        AND VipMemberInfo_ValidFlag = 1
    WHERE OrderSlaveFlow_TradesOrderGroupId = @tgId
    AND (@shopId IS NULL
    OR OrderSlaveFlow_SalesOrderSlaveShopId = @shopId);

-- 用這些資訊去 LoyaltyDB 查 LoyaltyPointTransaction
-- 找出「這筆訂單之後」產生的點數交易
--     且交易的 OccurTypeId 是以 TG|PE 或 TS|PE 為前綴（LIKE prefix + '%'）
--     且 VipMemberId 要匹配
DECLARE @orderCreateDateTime DATETIME;
SELECT
    @orderCreateDateTime = MIN(tmpCreateDateTime)
   ,@shopId = MIN(tmpShopId)
FROM #tmpLTOccurTypeId;
SELECT
    LPT.*
FROM LoyaltyDB.dbo.LoyaltyPointTransaction AS LPT WITH (NOLOCK)
WHERE LPT.LoyaltyPointTransaction_ShopId = @shopId
  AND LPT.LoyaltyPointTransaction_CreatedDateTime > @orderCreateDateTime
  AND LPT.LoyaltyPointTransaction_ValidFlag = 1
  AND EXISTS (
        SELECT 1
        FROM #tmpLTOccurTypeId AS T
        WHERE LPT.LoyaltyPointTransaction_VipMemberId = T.tmpVipMemberId
          AND LPT.LoyaltyPointTransaction_OccurTypeId LIKE T.tmpOccurTypeId + '%'
    );

SELECT *
FROM #tmpLTOccurTypeId;
```