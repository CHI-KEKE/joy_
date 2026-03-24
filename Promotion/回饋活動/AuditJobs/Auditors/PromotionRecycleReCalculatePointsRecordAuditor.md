## 線上觸發

**定時觸發**: 每小時 01 分執行
- `AuditPromotionRecycleDispatchJob` ➜ `AuditPromotionRecycleLoyaltyPointsJob`

## 線下觸發

**等級計算觸發**: 等級計算完成後執行
- `等級計算` ➜ `AuditCrmOthersOrderPromotionRecycleLoyaltyPointsJob`