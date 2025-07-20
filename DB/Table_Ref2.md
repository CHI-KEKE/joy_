# 📊 資料表查詢參考2

<br>

## 📖 目錄
  - [🚚 物流相關](#-物流相關)
    - [OrderSlaveFlow（配送狀態查詢）](#orderslaveflow配送狀態查詢)
    - [ShippingOrder & ShipmentTracing（配送紀錄）](#shippingorder--shipmenttracing配送紀錄)
    - [ShopShippingDefault（商店預設物流）](#shopshippingdefault商店預設物流)
    - [ShippingOrderSlave（訂單物流資訊）](#shippingorderslave訂單物流資訊)
    - [ShopShippingType（商店支援物流）](#shopshippingtype商店支援物流)
  - [🎯 點數相關](#-點數相關)
    - [LoyaltyPoint 相關資料表](#loyaltypoint-相關資料表)
  - [👤 會員相關](#-會員相關)
    - [MemberCode & MemberRegister](#membercode--memberregister)
    - [VipMember](#vipmember)
    - [VipMemberInfo](#vipmemberinfo)
  - [⚙️ NMQ 相關](#️-nmq-相關)
    - [建立新 Job](#建立新-job)
    - [JobGroupMapping](#jobgroupmapping)
    - [JobOwner](#jobowner)
    - [JobSwitch](#jobswitch)
  - [🎪 活動相關](#-活動相關)
    - [PromotionEngine](#promotionengine)
    - [PromotionEngineSetting](#promotionenginesetting)
    - [PromotionEngineSpecialPrice](#promotionenginespecialprice)
    - [PromotionTagSlave](#promotiontagslave)
    - [SalesOrderSlavePromotionEngine（訂單與活動關係）](#salesorderslavepromotionengine訂單與活動關係)
  - [📦 商品相關](#-商品相關)
    - [PointsPay（點加金）](#pointspay點加金)
    - [ProductBadge（角標）](#productbadge角標)
    - [SaleProduct](#saleproduct)
  - [🏪 商店設定相關](#-商店設定相關)
    - [ShopDefault](#shopdefault)
    - [ShopSecret](#shopsecret)
    - [SupplierApiProfile](#supplierapiprofile)
    - [ShopStaticSetting](#shopstaticsetting)
    - [AppSetting](#appsetting)
<br>

---

## 🚚 物流相關

### OrderSlaveFlow（配送狀態查詢）

案例 1：藉由 UniKey 查詢 OrderSlaveFlow 查配送狀態

<br>

```sql
use WebStoreDB

DECLARE @tgCode VARCHAR(20);

select @tgCode = TradesOrderGroup_Code
from TradesOrderGroup(nolock)
where TradesOrderGroup_UniqueKey = '2267a146-9e1a-4b7c-8f00-5a013f597da6'

SELECT *
FROM OrderSlaveFlow(NOLOCK)
WHERE OrderSlaveFlow_TradesOrderGroupCode = @tgCode
```

<br>

### ShippingOrder & ShipmentTracing（配送紀錄）

```sql
use ERPDB

select ShippingOrder_ForwarderDef,ShippingOrder_Id,ShippingOrder_Code,*
from ShippingOrder (nolock)
where ShippingOrder_ValidFlag = 1
and ShippingOrder_TradesOrderGroupCode = 'TG231205Q00069'

select ShipmentTracing_ShippingOrderId,ShipmentTracing_DateTime AS Tracing_DateTime,ShipmentTracing_StatusDef,*
from ShipmentTracing(nolock)
where ShipmentTracing_ShippingOrderId = '1545090'
order by ShipmentTracing_DateTime desc;
```

<br>

### ShopShippingDefault（商店預設物流）

```sql
select *
from ShopPayShippingDefault(nolock)
where ShopPayShippingDefault_ShopId = 44
```

<br>

### ShippingOrderSlave（訂單物流資訊）

案例 1：查看出貨時間

<br>

```
ShippingOrderSlave_UpdatedDateTime
2024-01-27 16:38:20.897
```

<br>

### ShopShippingType（商店支援物流）

配送方式：已被設為「基本運費」的資料，請調整為「非基本運費」

<br>

```sql
-- SELECT
SELECT ShopShippingType_IsSalesOrderFeeDefault, *
FROM dbo.ShopShippingType WITH(NOLOCK)
where ShopShippingType_IsSalesOrderFeeDefault = 1
and ShopShippingType_ShopId = 200123
and ShopShippingType_ValidFlag = 1

-- BACKUP
SELECT *
into MATempDB.dbo.tmp_WebStoreDB_ShopShippingType_VSTS359130
FROM dbo.ShopShippingType WITH(NOLOCK)
where ShopShippingType_IsSalesOrderFeeDefault = 1
and ShopShippingType_ShopId = 200123
and ShopShippingType_ValidFlag = 1

-- UPDATE
UPDATE dbo.ShopShippingType
set ShopShippingType_IsSalesOrderFeeDefault = 0
, ShopShippingType_UpdatedDateTime = getdate()
, ShopShippingType_UpdatedTimes = ShopShippingType_UpdatedTimes%255+1
, ShopShippingType_UpdatedUser = 'VSTS359130'
where ShopShippingType_IsSalesOrderFeeDefault = 1
and ShopShippingType_ShopId = 200123
and ShopShippingType_ValidFlag = 1

-- VERIFY
SELECT ShopShippingType_IsSalesOrderFeeDefault,*
FROM dbo.ShopShippingType WITH(NOLOCK)
where ShopShippingType_ShopId = 200123
and ShopShippingType_ValidFlag = 1
```

<br>

---

## 🎯 點數相關

### LoyaltyPoint 相關資料表

LoyaltyPointTransactionInfo、LoyaltyPointTransactionRecord、LoyaltyPoint

<br>

```sql
use LoyaltyDB

SELECT
    LoyaltyPoint_ShopId AS Notification_ShopId
    ,LoyaltyPoint_MemberId AS Notification_MemberId
FROM dbo.LoyaltyPointTransactionInfo WITH (NOLOCK)
INNER JOIN dbo.LoyaltyPointTransactionRecord WITH (NOLOCK)
    ON LoyaltyPointTransactionInfo_LoyaltyPointTransactionId = LoyaltyPointTransactionRecord_LoyaltyPointTransactionId
        AND LoyaltyPointTransactionRecord_ValidFlag = 1
INNER JOIN dbo.LoyaltyPoint WITH (NOLOCK)
    ON LoyaltyPointTransactionRecord_LoyaltyPointId = LoyaltyPoint_Id
        AND LoyaltyPoint_ValidFlag = 1
WHERE LoyaltyPointTransactionInfo_ValidFlag = 1
AND LoyaltyPoint_ValidDateTime > '2024-04-02T10:30:00'
AND LoyaltyPoint_ValidDateTime <= '2024-04-03T10:30:00'
AND LoyaltyPoint_ValidDateTime <= '2024-04-03T11:20:00'
AND LoyaltyPoint_ExpireDateTime > '2024-04-03T11:20:00'
AND LoyaltyPoint_BalancePoints > 0
AND LoyaltyPointTransactionInfo_EventTypeDef = 'Add'
AND LoyaltyPoint_MemberId > 0
GROUP BY LoyaltyPoint_ShopId
        ,LoyaltyPoint_MemberId
        ,LoyaltyPoint_VipMemberId;

SELECT
    LoyaltyPoint_ShopId AS Notification_ShopId
   ,LoyaltyPoint_MemberId AS Notification_MemberId
FROM dbo.LoyaltyPoint WITH (NOLOCK)
WHERE LoyaltyPoint_ExpireDateTime >= '2024-04-02T10:30:00'
AND LoyaltyPoint_ExpireDateTime < '2024-04-03T10:30:00'
AND LoyaltyPoint_ValidDateTime <= GETDATE()
AND LoyaltyPoint_ExpireDateTime > GETDATE()
AND LoyaltyPoint_BalancePoints > 0
AND LoyaltyPoint_ValidFlag = 1
GROUP BY LoyaltyPoint_ShopId
        ,LoyaltyPoint_MemberId
        ,LoyaltyPoint_VipMemberId
```

<br>

---

## 👤 會員相關

### MemberCode & MemberRegister

```sql
SELECT
    MemberCode_MemberCode,
    MemberRegister_CellPhone
FROM
    MemberCode WITH (NOLOCK)
INNER JOIN
    MemberRegister ON MemberRegister_MemberId = MemberCode_MemberID
WHERE
    MemberCode_ValidFlag = 1
    AND MemberCode_MemberCode = '5500613771';
```

<br>

### VipMember

案例 1：用 Email 查 MemberID

<br>

```sql
use WebStoreDB

SELECT *
FROM dbo.VipMemberInfo WITH(NOLOCK)
WHERE VipMemberInfo_Email = 'zerochu@nine-yi.com'
AND VipMemberInfo_ShopId = 2
AND VipMemberInfo_ValidFlag = 1

SELECT *
FROM dbo.VipMember WITH(NOLOCK)
WHERE VipMember_Id = 32129
```

<br>

### VipMemberInfo

案例 1：用電話找資料

<br>

```sql
use WebStoreDB

SELECT *
FROM dbo.VipMemberInfo WITH(NOLOCK)
where VipMemberInfo_CellPhone = '088887788'
AND VipMemberInfo_ShopId = 4

select VipMemberInfo_NoticeLanguageType,*
from VipMemberInfo(nolock)
WHERE VipMemberInfo_MemberId = '1609925'

SELECT *
FROM Shop(NOLOCK)
WHERE Shop_ValidFlag = 1
AND Shop_Name = 'Sox World'
```

<br>

---

## ⚙️ NMQ 相關

### 建立新 Job

```sql
USE NMQV2DB
GO

DECLARE @Region VARCHAR(2) = 'MY', -- TW/HK/PX/MY
@Env VARCHAR(4) = 'QA' -- QA/Prod

DECLARE @JobName VARCHAR(50) = 'GrabPayAsiaPayRefundRequestFinish',
 @JobDesc NVARCHAR(500) = N'PaymentMiddleware退款完成',
 @ClassName VARCHAR(200) = 'NineYi.SCM.Frontend.NMQV2.ThirdPartyPayments.PaymentMiddleWareRefundRequestFinishProcess',
 @User VARCHAR(50) = 'VSTS347891',
 @JobGroupId INT = 1;

DECLARE @Today DATETIME = GETDATE(),
 @JobId BIGINT,
 @teamId INT,
 @AssemblyRoot VARCHAR(500),
 @AssemblyPath VARCHAR(500),
 @AssemblyName VARCHAR(500);

IF LEFT(UPPER(@ClassName), 19) = 'NINEYI.SCM.FRONTEND'
BEGIN
    SET @AssemblyName = 'NineYi.SCM.Frontend.NMQV2'
END
ELSE IF LEFT(UPPER(@ClassName), 19) = 'NINEYI.ERP.BACKEND.'
BEGIN
    SET @AssemblyName = 'NineYi.ERP.Backend.NMQV2'
END

IF @Env = 'Prod'
BEGIN
    IF @Region = 'PX' OR @Region = 'HK' OR @Region = 'MY'
    BEGIN
        SET @AssemblyRoot = 'D:\Prod\NineYi';
        IF @AssemblyName = 'NineYi.SCM.Frontend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'SCM.NMQV2'
        END
        ELSE IF @AssemblyName = 'NineYi.ERP.Backend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'ERP.NMQV2'
        END
    END
    ELSE IF @Region = 'TW'
    BEGIN
        SET @AssemblyRoot = 'D:\Prod\NineYi\NMQV2\Library';
        IF @AssemblyName = 'NineYi.SCM.Frontend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'NineYi.SCM.Frontend'
        END
        ELSE IF @AssemblyName = 'NineYi.ERP.Backend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'NineYi.ERP.Backend'
        END
    END
END
ELSE IF @Env = 'QA'
BEGIN
    IF @Region IN ('PX','HK','MY','TW')
    BEGIN
        SET @AssemblyRoot = 'D:\QA\NineYi';
        IF @AssemblyName = 'NineYi.SCM.Frontend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'SCM.NMQV2'
        END
        ELSE IF @AssemblyName = 'NineYi.ERP.Backend.NMQV2'
        BEGIN      
            SET @AssemblyPath = 'ERP.NMQV2'
        END
    END
END

IF @Region = 'TW' AND @Env = 'Prod'
BEGIN
    SET @JobGroupId = @JobGroupId;
END
ELSE
BEGIN
SET @JobGroupId = 1;
END

INSERT INTO [dbo].[Job] ([Job_Name], [Job_Description], [Job_ClassName], [Job_AssemblyRoot], [Job_AssemblyPath], [Job_AssemblyName], [Job_RetryCount], [Job_Priority], [Job_Enable], [Job_Timeout], [Job_CreatedDateTime], [Job_CreatedUser], [Job_UpdatedTimes], [Job_UpdatedDateTime], [Job_UpdatedUser], [Job_ValidFlag], [Job_ErrorDelay], [Job_PendingDelay])
VALUES (@JobName, @JobDesc, @ClassName, @AssemblyRoot, @AssemblyPath, @AssemblyName, 0, 3, 1, 1200, @Today, @User, 0, @Today, @User, 1, 0, 0);

SELECT @JobId = Job_Id
FROM dbo.Job WITH (NOLOCK)
WHERE Job_Name = @JobName;

INSERT INTO [dbo].[JobGroupMapping] ([JobGroupMapping_JobId], [JobGroupMapping_JobGroupId], [JobGroupMapping_CreatedUser], [JobGroupMapping_CreatedDateTime], [JobGroupMapping_UpdatedTimes], [JobGroupMapping_UpdatedDateTime], [JobGroupMapping_UpdatedUser], [JobGroupMapping_ValidFlag])
VALUES (@JobId, @JobGroupId, @User, @Today, 0, @Today, @User, 1);

SELECT Job_Id
INTO #switchingJobs_AsiaPayGrabPay2
FROM NMQV2DB.dbo.Job WITH(NOLOCK)
WHERE 1 = 1
    AND Job_ValidFlag = 1
    AND Job_Name IN (@JobName);

INSERT INTO NMQV2DB.dbo.JobSwitch (JobSwitch_JobId, JobSwitch_Priority, JobSwitch_ValidFlag, JobSwitch_CreatedDateTime, JobSwitch_CreatedUser, JobSwitch_UpdatedDateTime, JobSwitch_UpdatedUser, JobSwitch_UpdatedTimes)
SELECT Job_Id, 2, 1, @Today, @User, @Today, @User, 0
FROM #switchingJobs_AsiaPayGrabPay2
WHERE 1 = 1

MERGE NMQV2DB.dbo.JobGroupMapping AS TARGET
USING #switchingJobs_AsiaPayGrabPay2 AS SOURCE
    ON JobGroupMapping_JobId = Job_Id
    AND JobGroupMapping_ValidFlag = 1
WHEN MATCHED THEN UPDATE SET
    JobGroupMapping_ValidFlag = 0,
    JobGroupMapping_UpdatedUser = @user,
    JobGroupMapping_UpdatedDateTime = @Today,
    JobGroupMapping_UpdatedTimes = JobGroupMapping_UpdatedTimes % 255 + 1;

SELECT @teamId = Team_Id
FROM [NMQV2DB].[dbo].[Team]
WHERE Team_SlackUserGroupHandle = 'oversea_backend_rd'

INSERT INTO [dbo].[JobOwner] ([JobOwner_JobId], [JobOwner_TeamId], [JobOwner_CreatedUser], [JobOwner_CreatedDateTime], [JobOwner_UpdatedTimes], [JobOwner_UpdatedDateTime], [JobOwner_UpdatedUser], [JobOwner_ValidFlag], [JobOwner_TypeDef])
VALUES (@JobId, @teamId, @User, @Today, 0, @Today, @User, 1, '');
GO
```

<br>

### JobGroupMapping

```sql
select *
from JobGroupMapping(nolock)
where JobGroupMapping_JobId = 432

DELETE FROM JobGroupMapping
where JobGroupMapping_JobId = 434
```

<br>

### JobOwner

```sql
select *
from JobOwner(nolock)
where JobOwner_JobId in (431,433,435);
```

<br>

### JobSwitch

```sql
DELETE FROM JobSwitch
where JobSwitch_JobId = 434

select *
from JobSwitch(nolock)
where JobSwitch_JobId in (431,433,435);
```

<br>

---

## 🎪 活動相關

### PromotionEngine

```sql
SELECT *
FROM dbo.PromotionEngine(NOLOCK)
WHERE PromotionEngine_ValidFlag = 1
AND PromotionEngine_TypeDef = 'DiscountReachPieceWithFreeGift'
AND PromotionEngine_StartDateTime <= GETDATE()
And PromotionEngine_EndDateTime >= GETDATE()
```

<br>

### PromotionEngineSetting

```sql
SELECT promotion.*, promotionSetting.*
FROM PromotionEngine promotion
JOIN PromotionEngineSetting promotionSetting ON promotion.PromotionEngine_Id = promotionSetting.PromotionEngineSetting_PromotionEngineId
WHERE promotion.PromotionEngine_StartDateTime <= @specifyDateTime
  AND promotion.PromotionEngine_ShopId = @shopId
  AND promotion.PromotionEngine_TypeDef IN (@promotionTypes);
```

<br>

### PromotionEngineSpecialPrice

案例 1：特定 SKUId 的單品特價起始時間

<br>

```sql
SELECT *
FROM dbo.PromotionEngineSpecialPrice(NOLOCK)
WHERE PromotionEngineSpecialPrice_ShopId = 41602
    AND PromotionEngineSpecialPrice_ValidFlag = 1
    AND PromotionEngineSpecialPrice_SaleProductSKUId IN (27932198, 27997813, 28009408, 28009416)
    AND PromotionEngineSpecialPrice_PromotionEngineId IN (281935, 287273)
```

<br>

### PromotionTagSlave

```sql
use WebStoreDB

select PromotionTagSlave_PromotionTagId,Count(1)
from PromotionTagSlave (NOLOCK)
where PromotionTagSlave_ValidFlag = 1
and PromotionTagSlave_PromotionTagId = '26673'
group by PromotionTagSlave_PromotionTagId;
```

<br>

### SalesOrderSlavePromotionEngine（訂單與活動關係）

```sql
USE ERPDB

SELECT SalesOrder_TradesOrderCode, SalesOrderSlave_TradesOrderSlaveCode, SalesOrderSlave_SalePageId
FROM dbo.SalesOrder(NOLOCK)
JOIN dbo.SalesOrderSlave(NOLOCK)
    ON SalesOrderSlave_ValidFlag = 1
    AND SalesOrderSlave_SalesOrderId = SalesOrder_Id
JOIN dbo.SalesOrderSlavePromotionEngine(NOLOCK)
    ON SalesOrderSlavePromotionEngine_ValidFlag = 1
    AND SalesOrderSlavePromotionEngine_TradesOrderSlaveId = SalesOrderSlave_TradesOrderSlaveId
WHERE SalesOrder_ShopId = 2
    AND SalesOrder_ValidFlag = 1
    AND SalesOrder_DateTime >= '2023-12-01 10:07'
    AND SalesOrder_DateTime < '2023-12-01 11:12'
```

<br>

---

## 📦 商品相關

### PointsPay（點加金）

撈特定商品是否用點加金

<br>

```sql
USE WebStoreDB

SELECT PointsPay_ValidFlag,PointsPay_UpdatedDateTime,*
FROM dbo.PointsPay(nolock)
WHERE PointsPay_SalePageId = 57249
```

<br>

### ProductBadge（角標）

```sql
select *
from ProductBadge(nolock)
where ProductBadge_ValidFlag = 1
and ProductBadge_EndDateTime < DATEADD(MONTH, -2, GETDATE())
and ProductBadge_ShopId = 2

SELECT TOP 20 *
FROM ProductBadge(nolock)
WHERE ProductBadge_ValidFlag = 1
AND ProductBadge_ShopId = 2131
AND ProductBadge_AppliedSalePageQty != -1
ORDER BY ProductBadge_EndDateTime

use WebStoreDB

SELECT COUNT(*)
FROM ProductBadge(NOLOCK)
WHERE ProductBadge_ValidFlag = 1
AND ProductBadge_ShopId != 200017
AND ProductBadge_AppliedSalePageQty = -1

SELECT Shop_Name,COUNT(*) AS productbadgeCount,SUM(ProductBadge_AppliedSalePageQty) AS salepagecount
FROM ProductBadge(nolock)
INNER JOIN dbo.Shop(nolock)
ON ProductBadge_ShopId = Shop_Id
    AND Shop_ValidFlag = 1
WHERE ProductBadge_ValidFlag = 1
AND ProductBadge_EndDateTime < DATEADD(MONTH,-2,GETDATE())
GROUP BY Shop_Name
ORDER BY salepagecount desc

SELECT ProductBadge_AppliedSalePageQty,ProductBadge_ShopId,ProductBadge_EndDateTime,*
FROM ProductBadge(nolock)
WHERE ProductBadge_ShopId = 2131
AND ProductBadge_ValidFlag = 1
AND ProductBadge_EndDateTime < DATEADD(MONTH,-2,GETDATE())
ORDER BY ProductBadge.ProductBadge_EndDateTime

SELECT ProductBadge_AppliedSalePageQty,ProductBadge_ShopId,ProductBadge_EndDateTime,*
FROM ProductBadge(nolock)
WHERE ProductBadge_ShopId = 2131
AND ProductBadge_ValidFlag = 1
AND ProductBadge_EndDateTime < DATEADD(MONTH,-2,GETDATE())
AND ProductBadge_AppliedSalePageQty = -1
ORDER BY ProductBadge.ProductBadge_EndDateTime
```

<br>

### SaleProduct

```sql
select *
from SaleProduct(nolock)
where SaleProduct_SalePageId = 3830
```

<br>

---

## 🏪 商店設定相關

### ShopDefault

付款方式：

<br>

```sql
select *
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_GroupTypeDef like '%Asia%'
```

<br>

付款相關最後更新時間：

<br>

```sql
USE WebStoreDB

DECLARE @supplierId BIGINT = 24

SELECT ShopDefault_SupplierId, ShopDefault_ShopId, ShopDefault_Key, ShopDefault_NewValue, ShopDefault_Value
FROM dbo.ShopDefault(NOLOCK)
WHERE ShopDefault_ValidFlag = 1
    AND ShopDefault_GroupTypeDef = 'Stripe'
    AND ShopDefault_Key = 'LatestAuditValidDate'
    AND ShopDefault_SupplierId = @supplierId
```

<br>

看帳戶類型：

<br>

```sql
select ShopDefault_Key,ShopDefault_Value,*
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_Key = 'StripeAccountType'
and ShopDefault_Value = 'Standard'
and ShopDefault_ShopId in (8,12,20)
```

<br>

### ShopSecret

```sql
USE WebStoreDB

SELECT TOP 100 *
FROM ShopSecret (nolock)
WHERE ShopSecret_GroupName = 'TwoCTwoP';
```

<br>

### SupplierApiProfile

```sql
USE ERPDB

select top 100 * 
from dbo.SupplierApiProfile with(nolock)
where SupplierApiProfile_ValidFlag = 1
and SupplierApiProfile_Token = '30671037';
```

<br>

### ShopStaticSetting

註冊限制國別：

<br>

```sql
USE WebStoreDB

-- SELECT
SELECT *
FROM ShopStaticSetting(NOLOCK)
WHERE [ShopStaticSetting_ShopId] IN (5)
AND [ShopStaticSetting_GroupName] = N'RegistrationSetting'
AND [ShopStaticSetting_Key] = N'OverseaCountryList'
AND [ShopStaticSetting_Value] = N'HK,MO,TW,MY';

-- Backup ShopStaticSetting 全表備份 
SELECT *
INTO MATempDB.dbo.tempShopStaticSetting_BackupAll_VSTS412788_V2
FROM [WebStoreDB].[dbo].[ShopStaticSetting] WITH(NOLOCK)

-- UPDATE
UPDATE dbo.ShopStaticSetting
SET [ShopStaticSetting_ValidFlag] = 0
WHERE [ShopStaticSetting_ShopId] IN (5)
AND [ShopStaticSetting_GroupName] = N'RegistrationSetting'
AND [ShopStaticSetting_Key] = N'OverseaCountryList'
AND [ShopStaticSetting_Value] = N'HK,MO,TW,MY';

-- Verify
SELECT
    ShopStaticSetting_ValidFlag,*
FROM dbo.ShopStaticSetting WITH(NOLOCK)
WHERE ShopStaticSetting_ShopId IN (5)
AND [ShopStaticSetting_GroupName] = N'RegistrationSetting'
AND [ShopStaticSetting_Key] = N'OverseaCountryList'
AND [ShopStaticSetting_Value] = N'HK,MO,TW,MY';
```

<br>

### AppSetting

查詢包含 TwoCTwoP 設定的應用程式設定：

<br>

```sql
USE ConfigDB

SELECT *
FROM dbo.AppSetting(NOLOCK)
WHERE AppSetting_ValidFlag = 1
    AND AppSetting_Value LIKE '%TwoCTwoP%'
```

<br>