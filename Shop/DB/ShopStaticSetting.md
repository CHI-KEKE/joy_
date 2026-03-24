## CalculateSwitch

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

## SupportMemberCollection

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


## PromotionReward

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


## 計算給券大開關

**Group:** PromotionReward
**Key:** IsCalculateRewardCoupon



## 計算給點大開關

**Group:** LoyaltyPoints
**Key:** IsCalculateRewardPoint