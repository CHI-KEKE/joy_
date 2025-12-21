

## LoyaltyPointTransaction + LoyaltyPointTransactionInfo

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


## LoyaltyPointTransaction_OccurTypeId

```sql


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