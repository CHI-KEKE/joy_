# Query 文件

## 目錄
1. [CrmSalesOrder](#1-crmsalesorder)
2. [PromotionEngine](#2-promotionengine)
3. [PromotionEngineSetting](#3-promotionenginesetting)
4. [PromotionTag](#4-promotiontag)
5. [PromotionHistory](#5-promotionhistory)
6. [RuleRecord](#6-rulerecord)
7. [ECoupon](#7-ecoupon)
8. [SaleProductSKU](#8-saleproductsku)
9. [ShopStaticSetting](#9-shopstaticsetting)
10. [TradesOrder](#10-tradesorder)
11. [CrmShopMemberCard](#11-crmshopmembercard)
12. [BatchUpload 相關](#12-batchupload-相關)
13. [NMQ Task](#13-nmq-task)
14. [SalesOrder](#14-salesorder)

<br>

---

## 1. CrmSalesOrder

**from crmMemberId**

<br>

```sql
use CRMDB

select *
from CrmSalesOrder(nolock)
INNER JOIN CrmMember(NOLOCK)
ON CrmSalesOrder_CrmMemberId = CrmMember_Id
where CrmSalesOrder_ValidFlag = 1


select CrmSalesOrder_Id,CrmSalesOrder_TypeDef,CrmSalesOrder_TradesOrderFinishDateTime,CrmSalesOrder_TotalPayment,CrmSalesOrder_CrmShopMemberCardId
from CrmSalesOrder(nolock)
where CrmSalesOrder_ValidFlag = 1
and CrmSalesOrder_TradesOrderFinishDateTime > '2025-08-25'
and CrmSalesOrder_TypeDef = 'Others'
--and CrmSalesOrder_TotalPayment > 3000
and CrmSalesOrder_ShopId = 41571
and CrmSalesOrder_CrmShopMemberCardId > 4521
```

<br>

**NineYiMemberId**

<br>

```sql
Select CrmMember_NineYiMemberId,*
from CrmMember(nolock)
where CrmMember_Id = 33309
```

<br>

**outerOrderCode1**

<br>

```sql
use CRMDB
select CrmSalesOrder_TradesOrderFinishDateTime,*
from CrmSalesOrder(nolock)
where CrmSalesOrder_OuterOrderCode1 = 'uat301A'
```

<br>

**crmsalesorderslave**

<br>

```sql

use CRMDB
select CrmSalesOrderSlave_CrmMemberId,CrmSalesOrderSlave_TotalPayment,CrmSalesOrderSlave_DataSourceType,CrmSalesOrderSlave_OriginalCrmSalesOrderId,CrmSalesOrderSlave_OriginalCrmSalesOrderSlaveId,CrmSalesOrderSlave_Qty,*
from CrmSalesOrderSlave(nolock)
where CrmSalesOrderSlave_ValidFlag = 1
and CrmSalesOrderSlave_CrmSalesOrderId = 329081


select CrmSalesOrderSlave_CrmSalesOrderId,CrmSalesOrderSlave_OuterProductSkuCode
from CrmSalesOrderSlave(nolock)
where CrmSalesOrderSlave_ValidFlag = 1
and CrmSalesOrderSlave_OuterProductSkuCode in 
('IW7385','IW4988','IR5789','IX7001','IW7541','IN2390','IR5798','IG4049','IG4052','IV5658','IV8226','IR7103','IR7096','IR6244','IR6245','IN4352','IN8749','IN8745','IV8223','IV8224','IV8221','IN8703','IN4343','IN4372','HZ4268','HZ4269','H63102','IU4623','IP8974','IR5775','IR5791','IR5784','IN2389','IR5782','IW7536','IX7000','IY0104','IN2391','IH2886','IE5700','IH2887','IG4107','IG4105','IN4374','HY1249','IV7749','IN8682','IN4347','IN8756','IR6249','IR6250','IN8701','IZ3128','IV5644','IR7108','IV5647','IR7110','IN4342','IV5657','IR6251','IE3237','IG4026','IG4036','IG4037','IG4095','IE5662','IG4028','IE3239','IS8985','IS8984','IN2401','IN4375','IN4377','IV5555','IZ3168','IV5625','IR6265','IV7737','IN4353','IZ3123','IZ3124','IV5622','IR6268','IZ3122','IZ3121','IV7736','IN4335','IV5836','IR7106','IZ3126','H54025','HM8347','HM8337','HS2721','HM8338','HR1986','HQ5964','HQ5972','HQ5967','HQ3661','HN1966','HT4501','HN1976','HT2303','HN1974','HD3317','GN5938','HM8362','HM8333','HM8356','HM8357','HM8358','HM8366','HM8340','HM8361','HM8354','HM8348','HM8341','HF2143','HQ6510','HR1952','HQ3734','HQ3663','HR1932','HR1951','HR1929','HM5039','HN4328','HT2291','HM5038','FN3359','FN3358','HT4499','GV4198','GV4194','HN4312','GV0336','HN4320','HG6153','HM9347','HT4498','HT4490','GV2799','GV2794','HT4734','HM5036','HB3405','HH8895','HT4732','HM5035','IB8611','HG8604','HM9342','IK9360','IU4628','IU4621','IL6965','IL6964','IP7162','HP3126','ID5430','IG2934','IG2937','IG2936','IG3043','IG0804','IP7678','IP7677','IL2027','IL2046','IL2152','IP7687','H63021','IP7695','IP7698','IL2035','IL2033','IL2040','IJ9902','IJ9907','IJ9891','IJ9872','IJ9880','IJ9877','IJ9876','IN5161','IU4627','IN8053','HZ4267','HZ4266','GK2074','IP1857','IU4629','IU4630','IQ3396','IQ3394','IT7794','IF7789','IE9390','ID5429','IF7791','IF7788','IG2914','IF7787','IG5300','ID5431','IP8779','IP5589','IL2066','IJ9782','IQ1792','HY1251','IP7929','IL2164','H63059','IQ1798','IL2056','H63080','H44782','H44798','IT7522','IT7524','H63044','IA1438','H44800','IA1421','IL4619','IL2059','IQ2137','H63094','HD3306','H62984','IJ3142','H62979','H62978','H62977','H63110','H63099','H63095','FZ6424','FZ6423','FZ6402','FZ6403','IE9396','IF2347','FZ6401','IA1453','H63016','IA1428','H44802','H63067','H63066','IB4785','IB4780','HY1255','HQ3672','HQ3735','JN4884','JN4885','JN7192','JN7183','JP1147','JD2903','JX2462','JW4773','JQ2454','JD1763','JD1761','JD1759','JD1477','JD3518','JD1765','JI7087','JL8301','JL8295','IW0069','JM7845','JN4986','JN7038','JN4945','JM7812','JN4906','JN4907','JD3521','JD3523','JC6396','JI7083','JL8299','JL6880','JM5494','JM7818','JM7817','JN1728','JN1727','JP1144','JP1143','JP1152','JP1153','JP1150','JP1148','JW4667','JD2909','JD2905','JP4745','JW7355','JS1118','JS1111','JN4990','JD5999','JN4873','JN4879','JM7815','JN3751','JM3230','JN4882','JN3719','JN3704','JM3358','JH6028','JP0400','JE2017','JE2016','IY9275','JE4023','IY9274','IF1973','IH0865','IF2047','IF2046','IH0871','IW9995','IX0011','JN7832','IX0009','IW7459','IW7455','IW9994','IW9993','IW9999','JD9794','IW7492','JI9153','IY9278','JE2014','IY4079','IH0869','IF2040','IF2041','IR7100','IR7102','JE3434','IN4398','JE3433','IN4397','JD9791','JE3436','JE9281','JE9282','IW2472','JD9843','IX0402','IW2473','JI9096','IZ3173','IZ3172','JE2012','IY4083','IY4087','H62982','H62981','IH2551','IE1450','IF2025','JE3438','JD9788','IW7472','JE3442','JD9793','JE9274','JD9799','IW7471','IW0080','JD9825','IW0071','JD9796','JF0668','JD9795','JF3665','JI7451','JE0130','IW7382','IW7386','JC7569','JC7568','JC7572','JC7571','IW7387','IW7389','IW7390','JF6361','JN4887','JS3154','JS1116','JS1115','JS3155','JM7844','JN4904','JN4911','JN4910','JD5997','JD1479','JD1474','JD1480','JD1475','JH8062','KA9633','JW4626','JZ0717','JX4745','JX4744','JX4804','JX7292','JW7352','JW7351','KB2608','KB2607','KA9630','JV6776','JV6782','JM7854','JD6002','JR4216','JR4217','JR4225','JR4224','JR6645','JR6644','JR6656','JR6655','KA2308','KC3339','JM9041','JM9039','JV9721','KB9310','JW6219','KA2302','KA7141','JW4624','JW4623','JW4621','KA7128','JX7276','JX7281','JX7333','KB3692','KD4760','JX4754','JX4752','KC8356','JR4201','JS2457','JS2459','JR6657','JX8362','JV9669','JV9720','JV9722','JV9770','KB2077','JY4556','JY4555','JW7354','JW7353','KD4759','KD2555','KE2311','JR4199','JS2463','JX8305','JX8308','JX8327','JX8294')

```

<br>

**查看我們要的線下訂單**

<br>

```sql
USE CRMDB;

WITH A AS(
SELECT TOP 1000 CrmSalesOrder_ShopId,
       CrmMember_ShopId,
       CrmMember_Id,
       CrmSalesOrder_TradesOrderFinishDateTime,
       CrmSalesOrder_Id,
       CrmSalesOrder_TypeDef,
       CrmSalesOrder_OuterOrderCode1,
       ROW_NUMBER() OVER(PARTITION BY CrmSalesOrder_Id ORDER BY CrmSalesOrderSlave_Id desc) AS OrderIdCount,
       CrmSalesOrderSlave_PurchaseType
FROM CrmSalesOrder(NOLOCK)
INNER JOIN CrmSalesOrderSlave(NOLOCK)
ON CrmSalesOrder_Id = CrmSalesOrderSlave_CrmSalesOrderId
INNER JOIN CrmMember(NOLOCK)
ON CrmMember_Id = CrmSalesOrder_CrmMemberId
where CrmMember_ShopId = 2
AND CrmSalesOrderSlave_PurchaseType = 'Normal'
and CrmSalesOrder_TypeDef = 'Others'
ORDER BY CrmMember_CreatedDateTime DESC)
SELECT *
FROM A
WHERE OrderIdCount = 1
and CrmSalesOrder_Id = 329141
ORDER BY CrmSalesOrder_TradesOrderFinishDateTime DESC
```

<br>

---

## 2. PromotionEngine



```sql
use WebStoreDB

select PromotionEngine_GroupCode,*
from PromotionEngine(nolock)
where PromotionEngine_Id = 8414
```

**版本1**

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

**版本2**

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

**版本3**

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

```sql
use WebStoreDB
select ECouponCustom_Name,ECoupon_ShopId,ECoupon_Id,ECoupon_DiscountPercent,ECoupon_DiscountPrice,*
from ECouponCustom(nolock)
inner join ECouponCustomMapping(nolock)
on ECouponCustom_Id = ECouponCustomMapping_ECouponCustomId
inner join ECoupon(nolock)
on ECoupon_Id = ECouponCustomMapping_ECouponId
where ECouponCustom_ValidFlag = 1
and ECoupon_ShopId = 10230
and ECoupon_ValidFlag = 1
```


```sql
use WebStoreDB

select *
from ECoupon(nolock)
inner join ECouponSlave(nolock)
on ECouponSlave_ECouponId = ECoupon_Id
where ECoupon_Id = 222734
and ECouponSlave_MemberId = 2778955
```

<br>

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

**9.1 重算開關啟用時間**

<br>

```sql
use WebStoreDB

select *
from ShopStaticSetting(nolock)
where ShopStaticSetting_ValidFlag = 1
and ShopStaticSetting_GroupName = 'PromotionReward'
and ShopStaticSetting_Key = 'CalculateSwitch'
```

<br>

**9.2 支援 MemberCollection**

<br>

```sql
use WebStoreDB

select *
from ShopStaticSetting(nolock)
where ShopStaticSetting_ValidFlag = 1
and ShopStaticSetting_GroupName = 'PromotionEngine'
and ShopStaticSetting_Key = 'SupportMemberCollectionShop'
```

<br>

---

## 10. TradesOrder

```sql
SELECT TradesOrderSlave_Qty,*
FROM TradesOrderGroup(NOLOCK)
INNER JOIN TradesOrder(NOLOCK)
ON TradesOrder_TradesOrderGroupId = TradesOrderGroup_Id
INNER JOIN TradesOrderSlave(NOLOCK)
ON TradesOrderSlave_TradesOrderId = TradesOrder_Id
WHERE TradesOrderGroup_ValidFlag = 1
AND TradesOrderGroup_Code = 'TG250812PB0001'
```


```sql
USE WebStoreDB
select *
from TradesOrderGroup(nolock)
where TradesOrderGroup_ValidFlag = 1
and TradesOrderGroup_Code >= 'TG250808BA00LN'
AND TradesOrderGroup_ShopId = 41571
AND TradesOrderGroup_CrmShopMemberCardId IN (4521,4522,4523)
AND TradesOrderGroup_TotalPayment >= 8800
AND TradesOrderGroup_TrackSourceTypeDef IN ('AndriodApp','iOSApp')
--and TradesOrderGroup_CreatedDateTime >= '2025-08-08 11:00'
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

## 12. BatchUpload 相關

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

---

## 14. SalesOrder

```sql
use ERPDB
SELECT SalesOrderGroup_DateTime,SalesOrderGroup_ShopId,SalesOrderSlave_StatusDef,*
FROM SalesOrderGroup(NOLOCK)
inner join SalesOrder(nolock)
on SalesOrder_SalesOrderGroupId = SalesOrderGroup_Id
inner join SalesOrderSlave(nolock)
on SalesOrder_Id = SalesOrderSlave_SalesOrderId
WHERE SalesOrderGroup_TradesOrderGroupCode IN ('TG250821M00003')
```