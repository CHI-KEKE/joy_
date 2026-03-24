## 關鍵字

```bash
PromotionRewardLoyaltyPointsV2
RecycleLoyaltyPointsV2
|=`Request content: {\\\"promotionRules\\\`
::Received data
```


```bash
{service="hk-qa-promotion-service"} 
|json
|= `正在 GetDatabaseRetryPolic`
|json
| line_format "{{._msg}}"
```

## by jobName

```bash
{service="prod-promotion-service"} 
|json
# |= `504035_TG251103JA007P`
|_props_JobName =`PromotionRewardCoupon`
|json
| line_format "{{._msg}}"
```

## BY TS 看 Recycle 跑幾次要對應 promotionId

```bash
{service="hk-qa-promotion-service"}
|json
|_props_JobName = `RecycleLoyaltyPointsV2`
|=`TS250710P000006`
|=`items:\"[]\`
|=`訂單編號:TS250710P000006，回饋活動序號：7541`
```

## by TaskId

```bash
{service="hk-qa-promotion-service"}
|json
| _props_TaskId = `ec72d512-a629-4478-b8b3-5e7275da2b98`


{service="prod-promotion-service"} 
|json
| _props_TaskId = `bf0f43ae-b413-4340-8585-14c6023799eb`
|json
| line_format "{{._msg}}"
```

## BY JOB + Key words
```bash
{service="prod-promotion-service"}
| json
|_props_JobName = `PromotionRewardBatchDispatcherV2`
| json
|=`200121`
| json
| line_format "{{._msg}}"

```


## 指撈出 lock 失敗

{service="prod-promotion-service"} 
|= `Lock 失敗`
|json
|_props_JobName = `PromotionRewardCoupon`
|json
| line_format "{{._msg}}"
!= "_ts"
|json
# |= `TaskProcess is FAILED.`



# 篩選不要的資料

{service="prod-promotion-service"} 
|= `Lock 失敗`
|json
|_props_JobName = `PromotionRewardCoupon`
|json
| line_format "{{._msg}}"
!= "_ts"
|json
# |= `TaskProcess is FAILED.`



## regexReplaceAll

{service=~"prod-promotion-service", container=~"promotion-console-nmqv3worker-group4"} 
|~ "Lock 失敗" 
!= "MetricsLogEntity" 
!= "將會重新產 Task 執行" 
| json 
| line_format `{{ regexReplaceAll " Lock 失敗" ._msg "" }}`