# Query 文件

## 目錄

### 📦 訂單相關

1. [CrmSalesOrder](#1-crmsalesorder)
   - [1.1 會員訂單基礎關聯查詢](#-11-會員訂單基礎關聯查詢)
   - [1.2 會員訂單多join](#-12-會員訂單多join)
   - [1.3 NineYiMemberId](#-13-NineYiMemberId)
   - [1.4 OuterOrderCode](#-14-OuterOrderCode)
   - [1.5 slaveandOuterProductSkuCode](#-15-slaveandOuterProductSkuCode)
   - [1.6 線下訂單專用查詢](#-16-Others)

2. [TradesOrder](#2-TradesOrder)
   - [2.1 多join](#-21-多join)
   - [2.2 TrackSourceTypeDef](#-22-TrackSourceTypeDef)

3. [SalesOrder](#3-SalesOrder)
   - [3.1 多join](#-31-多join)
   - [3.2 SalesOrderThirdPartyPayment](#-32-SalesOrderThirdPartyPayment)

### 🎁 PromotionEngine

1.  [PromotionEngine](#2-promotionengine)
   - [基礎查詢](#基礎)
   - [活動類型與通路](#promotionengine_typedefandchannel)
   - [JSON 規則解析](#parsejsonrule)
   - [目標類型設定](#targettypedef)

2.  [PromotionEngineSetting](#3-promotionenginesetting)
3.  [🏷️ PromotionTag - 促銷標籤](#4-promotiontag)
4.  [📜 PromotionHistory - 促銷歷史](#5-promotionhistory)
5.  [📋 RuleRecord - 規則記錄](#6-rulerecord)

### 🎁 回饋獎勵

1.  [ECoupon](#7-ecoupon)
    - [7.1 自訂券](#-71-自訂券)
    - [7.2 會員](#-72-會員)
    - [7.3 Ids](#-73-Ids)

2.  [LoyaltyPoint](#15-loyaltypoint)
    - [15.1 LoyaltyPointTransaction](#-151-LoyaltyPointTransaction)
    - [15.2 OccurTypeIdEvent](#-152-OccurTypeIdEvent)
    - [15.3 OccurTypeId,](#-153-OccurTypeId)

### 其他

29. [SaleProductSKU](#8-saleproductsku)
30. [ShopStaticSetting](#9-shopstaticsetting)
    - [9.1 CalculateSwitch](#-91-CalculateSwitch)
    - [9.2 SupportMemberCollection](#-92-SupportMemberCollection)
    - [9.3 PromotionReward](#-93-PromotionReward)
31. [CrmShopMemberCard](#11-crmshopmembercard)
32. [BatchUpload](#12-BatchUpload)
    - [12.1 插入新 BatchUpload Type](#121-插入新-batchupload-type)
    - [12.2 主檔與錯誤訊息](#122-主檔與錯誤訊息)
33. [NMQ Task](#13-nmq-task)


<br>

---

## 1. CrmSalesOrder

> **資料庫**: `CRMDB` | **主要功能**: CRM系統會員訂單資料查詢與分析

### 📋 查詢功能總覽

| 查詢類型 | 功能說明 | 關鍵欄位 | 使用場景 |
|---------|---------|----------|----------|
| 🔗 會員訂單關聯 | 會員與訂單完整關聯查詢 | `CrmSalesOrder_CrmMemberId` | 會員購買行為分析 |
| 🆔 九易會員ID | 根據九易系統會員ID查詢 | `CrmMember_NineYiMemberId` | 跨系統會員資料查詢 |
| 📦 外部訂單編號 | 透過外部系統訂單號查詢 | `CrmSalesOrder_OuterOrderCode1` | 第三方系統對接 |
| 🛒 訂單明細 | 訂單商品明細資料查詢 | `CrmSalesOrderSlave` 系列 | 商品銷售分析 |
| 🏪 線下訂單 | 實體門市訂單專用查詢 | `CrmSalesOrder_TypeDef = 'Others'` | 門市業績統計 |

### 🔗 1.1 會員訂單基礎關聯查詢

```sql
-- 基礎會員訂單查詢 (含會員資料)
USE CRMDB;

SELECT *
FROM CrmSalesOrder(NOLOCK)
INNER JOIN CrmMember(NOLOCK)
    ON CrmSalesOrder_CrmMemberId = CrmMember_Id
WHERE CrmSalesOrder_ValidFlag = 1;

-- 條件篩選訂單查詢
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
```


### 📊 1.2 會員訂單多join

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
```

### 🆔 1.3 NineYiMemberId

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

### 📦 1.4 OuterOrderCode

```sql
-- 外部訂單編號查詢
USE CRMDB;

SELECT 
    CrmSalesOrder_TradesOrderFinishDateTime AS 完成時間,
    CrmSalesOrder_OuterOrderCode1 AS 外部訂單號,
    CrmSalesOrder_Id AS 內部訂單ID,
    CrmSalesOrder_TotalPayment AS 總金額,
    *
FROM CrmSalesOrder(NOLOCK)
WHERE CrmSalesOrder_OuterOrderCode1 = 'uat301A';
```

### 🛒 1.5 slaveandOuterProductSkuCode

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
    AND CrmSalesOrderSlave_CrmSalesOrderId = 329081;

-- 根據商品料號查詢明細
SELECT 
    CrmSalesOrderSlave_CrmSalesOrderId AS 訂單ID,
    CrmSalesOrderSlave_OuterProductSkuCode AS 商品料號
FROM CrmSalesOrderSlave(NOLOCK)
WHERE CrmSalesOrderSlave_ValidFlag = 1
    AND CrmSalesOrderSlave_OuterProductSkuCode IN ('IW7385','IW4988','IR5789');

```

### 🏪 1.6 Others

```sql
-- 線下訂單篩選查詢 (高效能版本)
USE CRMDB;

WITH OfflineOrderAnalysis AS (
    SELECT TOP 1000 
        CrmSalesOrder_ShopId AS 店鋪ID,
        CrmMember_ShopId AS 會員店鋪ID,
        CrmMember_Id AS 會員ID,
        CrmSalesOrder_TradesOrderFinishDateTime AS 完成時間,
        CrmSalesOrder_Id AS 訂單ID,
        CrmSalesOrder_TypeDef AS 訂單類型,
        CrmSalesOrder_OuterOrderCode1 AS 外部訂單號,
        ROW_NUMBER() OVER(PARTITION BY CrmSalesOrder_Id ORDER BY CrmSalesOrderSlave_Id DESC) AS 明細排序,
        CrmSalesOrderSlave_PurchaseType AS 購買類型
    FROM CrmSalesOrder(NOLOCK)
    INNER JOIN CrmSalesOrderSlave(NOLOCK)
        ON CrmSalesOrder_Id = CrmSalesOrderSlave_CrmSalesOrderId
    INNER JOIN CrmMember(NOLOCK)
        ON CrmMember_Id = CrmSalesOrder_CrmMemberId
    WHERE CrmMember_ShopId = 2  -- 指定店鋪
        AND CrmSalesOrderSlave_PurchaseType = 'Normal'  -- 一般購買
        AND CrmSalesOrder_TypeDef = 'Others'  -- 線下訂單
    ORDER BY CrmMember_CreatedDateTime DESC
)
SELECT *
FROM OfflineOrderAnalysis
WHERE 明細排序 = 1  -- 每筆訂單只取一筆明細
    AND 訂單ID = 329141  -- 指定訂單ID
ORDER BY 完成時間 DESC;
```

---

## 2. TradesOrder

### 🔍 2.1 多join

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
    AND TradesOrderGroup_Code = 'TG250812PB0001';
```

### 📊 2.2 TrackSourceTypeDef

```sql
-- 多條件篩選交易訂單群組
USE WebStoreDB;

SELECT 
    TradesOrderGroup_Code AS 訂單群組編號,
    TradesOrderGroup_ShopId AS 店鋪ID,
    TradesOrderGroup_CrmShopMemberCardId AS 會員卡ID,
    TradesOrderGroup_TotalPayment AS 總金額,
    TradesOrderGroup_TrackSourceTypeDef AS 來源管道,
    TradesOrderGroup_CreatedDateTime AS 建立時間,
    *
FROM TradesOrderGroup(NOLOCK)
WHERE TradesOrderGroup_ValidFlag = 1
    AND TradesOrderGroup_Code >= 'TG250808BA00LN'  -- 訂單編號範圍
    AND TradesOrderGroup_ShopId = 41571  -- 指定店鋪
    AND TradesOrderGroup_CrmShopMemberCardId IN (4521,4522,4523)  -- 指定會員卡
    AND TradesOrderGroup_TotalPayment >= 8800  -- 最小金額
    AND TradesOrderGroup_TrackSourceTypeDef IN ('AndriodApp','iOSApp')  -- 行動裝置來源
    -- AND TradesOrderGroup_CreatedDateTime >= '2025-08-08 11:00'  -- 可選：時間篩選
ORDER BY TradesOrderGroup_CreatedDateTime DESC;
```

---

## 3. SalesOrder

### 🔍 3.1 多join

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

### 💳 3.2 SalesOrderThirdPartyPayment

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

---

</details>

---

# 🎁 促銷相關查詢

<details>
<summary>📖 點擊展開促銷引擎與活動規則查詢</summary>

## 2. PromotionEngine

### 基礎

```sql
use WebStoreDB

select PromotionEngine_GroupCode,*
from PromotionEngine(nolock)
where PromotionEngine_Id = 8414
```

### PromotionEngine_TypeDefAndChannel

<br>

```sql
use WebStoreDB

SELECT PromotionEngine_ShopId as ShopId,
       Shop_Name as ShopName,
       PromotionEngine_Id as PromotionEngineId,
	   PromotionEngine_StartDateTime,
	   PromotionEngine_EndDateTime,
       PromotionEngine_Name as PromotionEngineName,
       case
           when PromotionEngine_TypeDef = 'RewardReachPriceWithCoupon' then N'滿額給券'
           when PromotionEngine_TypeDef = 'RewardReachPriceWithPoint2' then N'滿額給點'
           when PromotionEngine_TypeDef = 'RewardReachPriceWithRatePoint2' then N'比例給點'
           else 'Error'
       end as PromotionEngineTypeDef,
       CASE
        WHEN JSON_VALUE(PromotionEngine_Rule, '$.MatchedSalesChannels') = 24 THEN N'線下'
        WHEN CAST(JSON_VALUE(PromotionEngine_Rule, '$.MatchedSalesChannels') AS INT) <= 7 THEN N'線上'
        ELSE N'線上 + 線下'
    END AS SalesChannelType,
    JSON_VALUE(PromotionEngine_Rule, '$.MatchedSalesChannels') AS MatchedSalesChannelsValue,
	 JSON_QUERY(CAST(PromotionEngine_Rule AS nvarchar(max)), '$.Thresholds')  AS Thresholds,
	 PromotionEngineSetting_ExtendInfo
       --PromotionEngine_Rule as PromotionEngineRule
FROM dbo.PromotionEngine WITH (NOLOCK)
INNER JOIN dbo.Shop WITH (NOLOCK)
    ON PromotionEngine_ShopId = Shop_Id
inner join PromotionEngineSetting
on PromotionEngine_Id = PromotionEngineSetting_PromotionEngineId
    AND Shop_ValidFlag = 1
WHERE PromotionEngine_ValidFlag = 1
  AND PromotionEngine_TypeDef IN ('RewardReachPriceWithRatePoint2','RewardReachPriceWithPoint2','RewardReachPriceWithCoupon')
AND PromotionEngine_StartDateTime >= '2025-08-15'
AND PromotionEngine_StartDateTime < '2025-08-16'
AND PromotionEngine_EndDateTime > GETDATE()
ORDER BY PromotionEngine_ShopId,PromotionEngine_Id;
```

<br>

### ParseJsonRule

<br>

```sql
USE WebStoreDB
 
SELECT
PromotionEngine_Id,
PromotionEngine_Rule,
    PromotionEngine_TypeDef
    , JSON_VALUE(PromotionEngine_Rule, '$.MatchedSalesChannels') AS '適用通路'
    , JSON_VALUE(PromotionEngine_Rule, '$.IncludedLocationScopes[0].LocationScopeType') AS '適用通路 - 指定門市'
    , JSON_VALUE(PromotionEngine_Rule, '$.IncludedProductScopes[0].ProductScopeType') AS '活動範圍 - 指定商品'
    , JSON_VALUE(PromotionEngine_Rule, '$.ExcludedProductScopes[0].ProductScopeType') AS '活動範圍 - 排除商品'
    , JSON_VALUE(PromotionEngine_Rule, '$.Cyclable') AS '活動內容 - 循環累計'
    , CASE
        WHEN ISJSON(PromotionEngineSetting_ExtendInfo) > 0
        THEN JSON_VALUE(PromotionEngineSetting_ExtendInfo, '$[0].RewardDays')  -- 如果是有效 JSON，提取 'name' 值
        ELSE NULL
    END AS '給點規則 - 訂單完成後N天給點'
    ,*
FROM dbo.PromotionEngine(NOLOCK)
JOIN dbo.PromotionEngineSetting(NOLOCK)
    ON PromotionEngineSetting_ValidFlag = 1
    AND PromotionEngineSetting_PromotionEngineId = PromotionEngine_Id
WHERE PromotionEngine_ValidFlag = 1
    AND PromotionEngine_TypeDef IN ('RewardReachPriceWithPoint2', 'RewardReachPriceWithRatePoint2')
    AND PromotionEngine_Id = 5812
ORDER BY PromotionEngine_CreatedDateTime DESC
```

### TargetTypeDef

```sql
use WebStoreDB

select PromotionEngine_ShopId,PromotionEngine_TypeDef,PromotionEngine_TargetTypeDef,*
from PromotionEngine(nolock)
where PromotionEngine_ValidFlag = 1
and PromotionEngine_TypeDef in ('RewardReachPriceWithCoupon','RewardReachPriceWithPoint2','RewardReachPriceWithRatePoint2')
and PromotionEngine_ShopId not in (8,2373)
and PromotionEngine_TargetTypeDef in ('SalePage','Category')
--and PromotionEngine_EndDateTime > GETDATE()


select PromotionEngine_ShopId,PromotionEngine_TypeDef,PromotionEngine_TargetTypeDef,*
from PromotionEngine(nolock)
where PromotionEngine_ValidFlag = 1
and PromotionEngine_TypeDef in ('RewardReachPriceWithCoupon','RewardReachPriceWithPoint2','RewardReachPriceWithRatePoint2')
and PromotionEngine_ShopId not in (8,2373)
and PromotionEngine_ExcludeTargetTypeDef in ('SalePage','Category')
--and PromotionEngine_EndDateTime > GETDATE()

USE WebStoreDB
select PromotionTagSlave_TargetTypeCode,PromotionTagSlave_TargetTypeId,PromotionTagSlave_TargetTypeCode,PromotionTagSlave_ValidFlag,*
from PromotionTagSlave(nolock)
where PromotionTagSlave_PromotionTagId in (652441,651990,651462,654150)
```

<br>

---

## 3. PromotionEngineSetting

```sql
USE WebStoreDB
select PromotionEngineSetting_IsInStore,*
from PromotionEngineSetting(nolock)
where PromotionEngineSetting_ValidFlag = 1
order by PromotionEngineSetting_CreatedDateTime desc
```

<br>

---

## 4. PromotionTag

```sql
USE WebStoreDB
select PromotionTagSlave_TargetTypeCode,PromotionTagSlave_TargetTypeId,PromotionTagSlave_TargetTypeCode,PromotionTagSlave_ValidFlag,*
from PromotionTagSlave(nolock)
where PromotionTagSlave_PromotionTagId = 5145
--and PromotionTagSlave_ValidFlag = 1
 
 
select PromotionTag_ValidFlag,PromotionTag_IsClosed,*
from PromotionTag(nolock)
where PromotionTag_Id = 5145
```

<br>

---

## 5. PromotionHistory

```sql
SELECT PromotionEngine_Rule,PromotionEngine_UpdatedDateTime,*
FROM History.PromotionEngine(NOLOCK)
--WHERE PromotionEngine_Id = 5776
order by PromotionEngine_CreatedDateTime desc
```

<br>

---

## 6. RuleRecord

```sql
SELECT PromotionEngineRuleRecord_UpdatedDateTime,PromotionEngineRuleRecord_S3Key,PromotionEngineRuleRecord_Version,*
FROM PromotionEngineRuleRecord(NOLOCK)
--WHERE PromotionEngineRuleRecord_PromotionEngineId = 5768
--WHERE PromotionEngineRuleRecord_PromotionEngineTypeDef = 'RewardReachPriceWithPoint'
order by PromotionEngineRuleRecord_CreatedDateTime desc
```

<br>

---

## 7. ECoupon

### 🔍 7.1 自訂券

```sql
-- 自訂優惠券完整查詢
USE WebStoreDB;

SELECT 
    ECouponCustom_Name AS 券券名稱,
    ECoupon_ShopId AS 店鋪ID,
    ECoupon_Id AS 券券ID,
    ECoupon_DiscountPercent AS 折扣百分比,
    ECoupon_DiscountPrice AS 折扣金額,
    *
FROM ECouponCustom(NOLOCK)
INNER JOIN ECouponCustomMapping(NOLOCK)
    ON ECouponCustom_Id = ECouponCustomMapping_ECouponCustomId
INNER JOIN ECoupon(NOLOCK)
    ON ECoupon_Id = ECouponCustomMapping_ECouponId
WHERE ECouponCustom_ValidFlag = 1
    AND ECoupon_ShopId = 10230 
    AND ECoupon_ValidFlag = 1;
```

### 🎁 7.2 會員

```sql
-- 會員優惠券查詢
USE WebStoreDB;

SELECT 
    ECoupon_Id AS 券券ID,
    ECouponSlave_Id AS 券券明細ID,
    ECouponSlave_MemberId AS 會員ID,
    ECouponSlave_ValidFlag AS 有效狀態,
    ECouponSlave_UpdatedDateTime AS 更新時間,
    ECouponSlave_UpdatedUser AS 更新者,
    *
FROM ECoupon(NOLOCK)
INNER JOIN ECouponSlave(NOLOCK)
    ON ECouponSlave_ECouponId = ECoupon_Id
WHERE ECoupon_Id = 222734
    AND ECouponSlave_MemberId = 2778955;
```

### 🏪 7.3 Ids

```sql
-- 店鋪優惠券批次查詢
DECLARE @TargetIds TABLE (Id BIGINT);
INSERT INTO @TargetIds VALUES (41571), (10230), (12345); -- 示例店鋪ID

SELECT 
    ECouponSlave_Id AS 券券明細ID,
    ECouponSlave_ShopId AS 店鋪ID,
    ECouponSlave_ValidFlag AS 有效狀態,
    ECouponSlave_UpdatedDateTime AS 更新時間,
    ECouponSlave_UpdatedUser AS 更新者,
    *
FROM dbo.ECouponSlave(NOLOCK)
WHERE ECouponSlave_ShopId IN (SELECT Id FROM @TargetIds)
ORDER BY ECouponSlave_UpdatedDateTime DESC;
```

<br>

---

## 15. LoyaltyPoint

### 📊 15.1 LoyaltyPointTransaction

```sql
-- 會員點數交易明細查詢
USE LoyaltyDB;

DECLARE @shopId BIGINT = 17;

SELECT 
    LoyaltyPointTransaction_CreatedDateTime AS 建立時間,
    LoyaltyPointTransactionInfo_OuterMemberId AS 外部會員ID,
    LoyaltyPointTransaction_EventTypeDef AS 事件類型,
    LoyaltyPointTransactionInfo_OccurType AS 發生類型,
    LoyaltyPointTransaction_OccurTypeId AS 發生類型ID,
    LoyaltyPointTransaction_Code AS 交易代碼,
    LoyaltyPointTransaction_Point AS 點數異動,
    LoyaltyPointTransactionInfo_OccurDescription AS 說明,
    LoyaltyPointTransaction_CreatedUser AS 建立者,
    -- REPLACE(LoyaltyPointTransaction_OccurTypeId, '|25721', '') AS 清理後代碼, -- 可選：清理特定格式
    *
FROM dbo.LoyaltyPointTransaction(NOLOCK)
JOIN dbo.LoyaltyPointTransactionInfo(NOLOCK)
    ON LoyaltyPointTransactionInfo_ValidFlag = 1
    AND LoyaltyPointTransactionInfo_LoyaltyPointTransactionId = LoyaltyPointTransaction_Id
WHERE LoyaltyPointTransaction_ValidFlag = 1
    AND LoyaltyPointTransaction_ShopId = @shopId
    -- AND LoyaltyPointTransaction_CreatedDateTime >= CONVERT(DATE, GETDATE()) -- 今日記錄
    -- AND LoyaltyPointTransaction_Code IN ('LT2508281400059') -- 特定交易代碼
    -- AND LoyaltyPointTransaction_EventTypeDef IN ('PromotionReward','PromotionRewardStore') -- 促銷回饋記錄
    -- AND LoyaltyPointTransaction_CreatedUser IN ('PromotionRewardLoyaltyPointsV2Job','RecycleLoyaltyPointsV2Job') -- 系統作業
ORDER BY LoyaltyPointTransaction_CreatedDateTime DESC;
```

### 🔍 15.2 OccurTypeIdEvent

```sql
-- 特定條件點數查詢
USE LoyaltyDB;

SELECT 
    LoyaltyPointTransaction_Id AS 交易ID,
    LoyaltyPointTransaction_OccurTypeId AS 發生類型ID,
    LoyaltyPointTransaction_Point AS 點數異動,
    LoyaltyPointTransaction_CreatedDateTime AS 建立時間,
    LoyaltyPointTransactionInfo_OccurDescription AS 說明
FROM dbo.LoyaltyPointTransaction(NOLOCK)
JOIN dbo.LoyaltyPointTransactionInfo(NOLOCK)
    ON LoyaltyPointTransactionInfo_ValidFlag = 1
    AND LoyaltyPointTransactionInfo_LoyaltyPointTransactionId = LoyaltyPointTransaction_Id
WHERE LoyaltyPointTransaction_ValidFlag = 1
    -- 測試會員相關記錄
    AND LoyaltyPointTransaction_OccurTypeId IN ('0704ambertest004|7438','0704ambertest004|7423','0704ambertest004|7439')
    -- 特定活動關聯
    -- AND LoyaltyPointTransaction_OccurTypeId LIKE 'TG702R27KLT2021%'
    -- AND LoyaltyPointTransaction_OccurTypeId LIKE '%3495U%'
    -- AND LoyaltyPointTransaction_OccurTypeId NOT LIKE '%Te25%'
    -- 特定使用者操作
    -- AND LoyaltyPointTransactionInfo_OuterMemberId = 'K46night'
    -- AND LoyaltyPointTransactionInfo_OccurDescription LIKE '%Sprint1980Demo%'
    -- 回收記錄
    AND LoyaltyPointTransaction_EventTypeDef = 'Recycle'
ORDER BY LoyaltyPointTransaction_CreatedDateTime DESC;
```

### 📋 15.3 OccurTypeId,

```sql
-- 常見交易代碼範例說明
/*
交易代碼格式說明：

生產環境格式：
- UAT702R|6177_LT250xxx  -- 訂單+活動+交易號
- UAT1102RA|6200_LT250xxx -- 複合訂單格式  
- FTL-250312182334-dA4Q   -- 特殊產生格式

測試環境格式：
- test0001|753_LT         -- 測試訂單
- 0625ambertest004|7xxx   -- 測試會員
- 753_CrmSalesOrder:062   -- CRM關聯測試
- 753_CrmSa              -- CRM訂單關聯
- 658_CrmSa              -- 其他CRM格式

事件類型：
- PromotionReward         -- 促銷回饋給點
- PromotionRewardStore    -- 門市促銷回饋
- Recycle                -- 點數回收
*/

-- 查詢交易代碼統計
SELECT 
    LEFT(LoyaltyPointTransaction_OccurTypeId, 10) AS 代碼前綴,
    COUNT(*) AS 筆數,
    SUM(LoyaltyPointTransaction_Point) AS 總點數
FROM dbo.LoyaltyPointTransaction(NOLOCK)
WHERE LoyaltyPointTransaction_ValidFlag = 1
    AND LoyaltyPointTransaction_CreatedDateTime >= DATEADD(DAY, -7, GETDATE())
GROUP BY LEFT(LoyaltyPointTransaction_OccurTypeId, 10)
ORDER BY 筆數 DESC;
```

---

## 8. SaleProductSKU

```sql
select *
from SaleProductSKU(nolock)
where SaleProductSKU_SalePageId = 62210
```

<br>

---

## 9. ShopStaticSetting

### 🧮 9.1 CalculateSwitch

```sql
-- 促銷回饋計算開關查詢
USE WebStoreDB;

SELECT 
    ShopStaticSetting_ShopId AS 店鋪ID,
    ShopStaticSetting_GroupName AS 設定群組,
    ShopStaticSetting_Key AS 設定鍵值,
    ShopStaticSetting_Value AS 設定值,
    ShopStaticSetting_CreatedDateTime AS 建立時間,
    ShopStaticSetting_UpdatedDateTime AS 更新時間,
    *
FROM ShopStaticSetting(NOLOCK)
WHERE ShopStaticSetting_ValidFlag = 1
    AND ShopStaticSetting_GroupName = 'PromotionReward'
    AND ShopStaticSetting_Key = 'CalculateSwitch';
```

### 👥 9.2 SupportMemberCollection

```sql
-- 會員收集支援設定查詢
USE WebStoreDB;

SELECT 
    ShopStaticSetting_ShopId AS 店鋪ID,
    ShopStaticSetting_GroupName AS 設定群組,
    ShopStaticSetting_Key AS 設定鍵值,
    ShopStaticSetting_Value AS 設定值,
    ShopStaticSetting_Description AS 設定說明,
    ShopStaticSetting_CreatedDateTime AS 建立時間,
    *
FROM ShopStaticSetting(NOLOCK)
WHERE ShopStaticSetting_ValidFlag = 1
    AND ShopStaticSetting_GroupName = 'PromotionEngine'
    AND ShopStaticSetting_Key = 'SupportMemberCollection';
```

### 🔍 9.3 PromotionReward

```sql
-- 店鋪完整靜態設定查詢
USE WebStoreDB;

SELECT 
    ShopStaticSetting_ShopId AS 店鋪ID,
    ShopStaticSetting_GroupName AS 設定群組,
    ShopStaticSetting_Key AS 設定鍵值,
    ShopStaticSetting_Value AS 設定值,
    ShopStaticSetting_DataType AS 資料類型,
    ShopStaticSetting_Description AS 設定說明,
    ShopStaticSetting_ValidFlag AS 有效標記,
    ShopStaticSetting_CreatedDateTime AS 建立時間,
    ShopStaticSetting_UpdatedDateTime AS 更新時間
FROM ShopStaticSetting(NOLOCK)
WHERE ShopStaticSetting_ValidFlag = 1
    AND ShopStaticSetting_ShopId = 41571  -- 指定店鋪ID
    AND ShopStaticSetting_GroupName IN ('PromotionReward', 'PromotionEngine')  -- 促銷相關設定
ORDER BY ShopStaticSetting_GroupName, ShopStaticSetting_Key;
```

<br>

---

## 11. CrmShopMemberCard

```sql
use CRMDB;
 
select CrmShopMemberCard_Id,CrmShopMemberCard_Name,CrmShopMemberCard_ShopId,CrmShopMemberCard_Level,*
from CrmShopMemberCard(nolock)
where CrmShopMemberCard_ShopId =5
order by CrmShopMemberCard.CrmShopMemberCard_ShopId,CrmShopMemberCard.CrmShopMemberCard_Level desc
```

<br>

---

## 12. BatchUpload

### 12.1 插入新 BatchUpload Type

```sql
USE WebStoreDB;
  
DECLARE @batchUploadType VARCHAR(50) = 'BatchModifyPromotionOuterId',
        @batchUploadTypeDesc NVARCHAR(50) = N'批次更新給點活動料號',
        @vsts VARCHAR(20) = 'VSTS466779',
        @now DATETIME = GETDATE();
  
-- Insert
INSERT INTO dbo.Definition
(
    Definition_TableName,
    Definition_ColumnName,
    Definition_Code,
    Definition_Desc,
    Definition_Note,
    Definition_Sort,
    Definition_CreatedDateTime,
    Definition_CreatedUser,
    Definition_UpdatedTimes,
    Definition_UpdatedDateTime,
    Definition_UpdatedUser,
    Definition_ValidFlag
)
SELECT TOP 1
    Definition_TableName,
    Definition_ColumnName,
    @batchUploadType,
    @batchUploadTypeDesc,
    Definition_Note,
    Definition_Sort + 10,
    @now,
    @vsts,
    0,
    @now,
    @vsts,
    1
FROM dbo.Definition WITH(NOLOCK)
WHERE Definition_ValidFlag = 1
    AND Definition_TableName = 'BatchUpload'
    AND Definition_ColumnName = 'BatchUpload_TypeDef'
ORDER BY Definition_Sort DESC
  
-- Verify
SELECT TOP 1 *
FROM dbo.Definition WITH(NOLOCK)
WHERE Definition_ValidFlag = 1
    AND Definition_TableName = 'BatchUpload'
    AND Definition_ColumnName = 'BatchUpload_TypeDef'
    AND Definition_Code = @batchUploadType
ORDER BY Definition_Sort DESC
 
use WebStoreDB
-- Verify
SELECT *
FROM dbo.Definition WITH(NOLOCK)
WHERE Definition_ValidFlag = 1
    AND Definition_TableName = 'BatchUpload'
    AND Definition_ColumnName = 'BatchUpload_TypeDef'
    --AND Definition_Code = @batchUploadType
    AND Definition_Code = 'ModifyRewardPromotionSalePage'
ORDER BY Definition_Sort DESC
```

<br>

### 12.2 主檔與錯誤訊息

```sql
-- HKQA
-- BatchUploadTask : 50
-- BatchUpload : 49
-- BatchUploadType : BatchModifyPromotionOuterId
 
USE WebStoreDB
 
-- 批次主檔
SELECT *
FROM BatchUpload(NOLOCK)
WHERE BatchUpload_ValidFlag = 1
AND BatchUpload_Code = 'BA2502252300002'
--and BatchUpload_Id = 11255
 
--錯誤訊息
select BatchUploadMessage_StatusDef,*
from BatchUploadMessage(nolock)
where BatchUploadMessage_ValidFlag = 1
and BatchUploadMessage_BatchUploadId = 11255
```

<br>

---

## 13. NMQ Task

```sql
USE NMQV2DB

SELECT *
FROM Job(NOLOCK)
WHERE Job_ValidFlag = 1
AND Job_Name = 'BatchUpload'

select *
from Task(nolock)
where Task_ValidFlag = 1
--and Task_JobId = 50
and Task_Data like '%ExportRewardPromotionSalePage%'
order by Task_CreatedDatetime desc


use NMQV2DB

select *
from Job(nolock)
where Job_ValidFlag = 1
and Job_Name = 'BatchModifyPromotionSalePageTask'
```

<br>