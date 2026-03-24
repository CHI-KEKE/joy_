## csp_GetPromotionEngineRewardPointData

取得促銷引擎的回饋活動清單，支援多種回饋類型的搜尋和篩選功能

- 給券活動
- 給點活動


| 搜尋類型 | searchType 參數 | 說明 | 對應的回饋類型 |
|----------|----------------|------|----------------|
| **給券活動** | `'PromotionReward'` | 獨立的給券方法 | `RewardReachPriceWithCoupon` |
| **給點活動** | `'RewardPoint'` | 內容多語系舊給點模組 | `RewardReachPriceWithPoint`<br>`RewardReachPriceWithRatePoint` |
| **其他活動** | `null` 或其他值 | 預設的回饋活動類型 | `RewardReachPriceWithPoint`<br>`RewardReachPriceWithRatePoint`<br>`RewardReachPriceWithPoint2`<br>`RewardReachPriceWithRatePoint2` |

**初始化設定**
```sql
AS
BEGIN
    -- 1. 基本設定
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    -- 2. 預設搜尋類型清單（包含所有給點相關類型）
    DECLARE @searchTypeList varchar(200) = 
        'RewardReachPriceWithPoint,RewardReachPriceWithRatePoint,RewardReachPriceWithPoint2,RewardReachPriceWithRatePoint2'
```

**目標會員類型處理**：
```sql
-- 新版折扣活動不使用 'None' 來判斷目標會員類型
-- 'None' 等同於不篩選此條件
IF @targetMemberTypeDef = 'None'
BEGIN
    SET @targetMemberTypeDef = NULL;
END
```

**目標類型相容性處理**：
```sql
-- 處理舊版與新版目標類型的相容性
-- 舊版：PromotionSalePage → 新版：SalePage
IF @targetTypeDef = 'PromotionSalePage'
BEGIN
    SET @targetTypeDef = 'SalePage';
END
```

**搜尋類型**
```sql
-- 根據 searchType 參數決定要查詢的回饋類型
IF @searchType = 'PromotionReward'
BEGIN
    -- 給券活動：僅查詢優惠券相關類型
    SET @searchTypeList = 'RewardReachPriceWithCoupon';
END
ELSE IF @searchType = 'RewardPoint'
BEGIN
    -- 給點活動：查詢基本的給點類型
    SET @searchTypeList = 'RewardReachPriceWithPoint,RewardReachPriceWithRatePoint';
END
ELSE
BEGIN
    -- 其他活動：查詢所有給點相關類型（包含第二版本）
    SET @searchTypeList = 'RewardReachPriceWithPoint,RewardReachPriceWithRatePoint,RewardReachPriceWithPoint2,RewardReachPriceWithRatePoint2';
END
```

**查詢給券活動**
```sql
EXEC csp_GetPromotionEngineRewardPointData 
    @searchType = 'PromotionReward'
    -- 其他參數...
```

**查詢給點活動**
```sql
EXEC csp_GetPromotionEngineRewardPointData 
    @searchType = 'RewardPoint'
    -- 其他參數...
```

**查詢所有回饋活動**
```sql
EXEC csp_GetPromotionEngineRewardPointData 
    @searchType = NULL
    -- 其他參數...
```