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

**版本1**

<br>

```sql
use WebStoreDB

SELECT PromotionEngine_ShopId as ShopId,
       Shop_Name as ShopName,
       PromotionEngine_Id as PromotionEngineId,
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
    JSON_VALUE(PromotionEngine_Rule, '$.MatchedSalesChannels') AS MatchedSalesChannelsValue
       --PromotionEngine_Rule as PromotionEngineRule
FROM dbo.PromotionEngine WITH (NOLOCK)
INNER JOIN dbo.Shop WITH (NOLOCK)
    ON PromotionEngine_ShopId = Shop_Id
    AND Shop_ValidFlag = 1
WHERE PromotionEngine_ValidFlag = 1
  AND PromotionEngine_TypeDef IN ('RewardReachPriceWithRatePoint2','RewardReachPriceWithPoint2','RewardReachPriceWithCoupon')
AND PromotionEngine_CreatedDateTime >= '2025-07-21'
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

<br>