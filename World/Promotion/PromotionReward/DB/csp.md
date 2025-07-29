# CSP 文件

## 目錄
1. [csp_GetPromotionEngineRewardPointData](#1-csp_getpromotionenginerewardpointdata)

<br>

---

## 1. csp_GetPromotionEngineRewardPointData

回饋活動清單

<br>

**搜尋方式概況**

<br>

給券活動 - 獨立方法 searchType - 'PromotionReward'

<br>

內容多語系舊給點模組 - 'RewardPoint'

<br>

其他活動 - searchType 帶 null

<br>

```sql
AS
BEGIN
    SET NOCOUNT ON;
    SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

    DECLARE @searchTypeList varchar(200) = 'RewardReachPriceWithPoint,RewardReachPriceWithRatePoint,RewardReachPriceWithPoint2,RewardReachPriceWithRatePoint2'
    
    IF @targetMemberTypeDef = 'None' ---- 新版折扣活動不使用None來判斷目標會員類型，None等同於不篩選此條件
    BEGIN
        SET @targetMemberTypeDef = NULL;
    END

    IF @targetTypeDef = 'PromotionSalePage'
    BEGIN
        SET @targetTypeDef = 'SalePage'; ---- 目標類型為商品頁 舊版類型：PromotionSalePage / 新版類型：SalePage
    END

    IF @searchType = 'PromotionReward'
    BEGIN
        SET @searchTypeList = 'RewardReachPriceWithCoupon';
    END
    ELSE IF @searchType = 'RewardPoint'
    BEGIN
        SET @searchTypeList = 'RewardReachPriceWithPoint,RewardReachPriceWithRatePoint';
    END
    ELSE
    BEGIN
        SET @searchTypeList = 'RewardReachPriceWithPoint,RewardReachPriceWithRatePoint,RewardReachPriceWithPoint2,RewardReachPriceWithRatePoint2';
    END
```

<br>