
## 線上觸發

**定時觸發**: 每小時 01 分執行

```mermaid
graph LR
    A[⏰ Cronjob<br/>每小時 01 分] --> B[AuditPromotionRecycleDispatchJob]
    B --> C[AuditPromotionRecycleCouponJob]
    C --> D[執行優惠券回收狀態稽核]
```