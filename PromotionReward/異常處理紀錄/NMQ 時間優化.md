


## 佔額活動 lock 衝突

- 避免 OrderCreate 比 OrderPlaced 先做
- OrderPlaced 派發後 PromotionRewardLoyaltyPointsDispatcherV2Job 延遲 bookingTime


https://gitlab.91app.com/commerce-cloud/nine1.promotion/nine1.promotion.worker/-/merge_requests/910/diffs