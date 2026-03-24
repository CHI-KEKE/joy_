
- 佔額與回饋互搶
- 正逆流程互搶
- 稽核正流程互搶


現在好像有工具可以處理


## JobName

BatchReleaseLockJob


## Query 


```bash
{service=~"prod-promotion-service", container=~"promotion-console-nmqv3worker-group4"} 
|~ "Lock 失敗" 
!= "MetricsLogEntity" 
!= "將會重新產 Task 執行" 
| json 
| regexp "(?P<target_id>\\d+_[A-Za-z0-9:]+)"
| line_format `{{.target_id}}`
```


## 佔額活動 lock 衝突

- 避免 OrderCreate 比 OrderPlaced 先做
- OrderPlaced 派發後 PromotionRewardLoyaltyPointsDispatcherV2Job 延遲 bookingTime


https://gitlab.91app.com/commerce-cloud/nine1.promotion/nine1.promotion.worker/-/merge_requests/910/diffs