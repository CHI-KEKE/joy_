

## AuditPromotionRewardLoyaltyPointsDispatchV2Job - 線下訂單

```json
{"Data":"{\"ExecuteTime\":\"2025-08-14T02:20:41.7973958+00:00\",\"IsCrmOthersOrder\":true,\"ShopId\":2}"}
{"Data":"{\"ExecuteTime\":\"2025-03-18T11:32\",\"ShopId\":2,\"IsCrmOthersOrder\":true}"}
{"Data":"{\"ExecuteTime\":\"2025-03-18T11:32\",\"ShopId\":2,\"IsCrmOthersOrder\":true}"}
{"ExecuteTime":"2025-04-11T00:01:15.6443120+08:00"}
{"Data":"{\"ExecuteTime\":\"2025-04-11T00:01:15.6443120+08:00\"}"}
```


## AuditPromotionRecycleLoyaltyPoints / AuditPromotionRecycleCouponJob


```json
{"Data":"{\"ShopId\":2,\"TSCode\":\"TS250903T000003\",\"EventName\":\"\",\"TriggerDatetime\":\"2025-09-03T19:01:30.7072732+08:00\",\"OrderCreateDate\":\"2025-09-03T17:41:45.45\",\"PromotionId\":0,\"PromotionEngineType\":\"\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":0,\"OrderTypeDefEnum\":\"ECom\",\"MemberId\":0,\"S3Key\":\"\"}"}

{"Data":"{\"ShopId\":2,\"TSCode\":\"TS250917K000001\",\"EventName\":\"\",\"TriggerDatetime\":\"2025-09-17T11:01:27.0909998+08:00\",\"OrderCreateDate\":\"2025-09-17T09:06:37.407\",\"PromotionId\":0,\"PromotionEngineType\":\"\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":0,\"OrderTypeDefEnum\":\"ECom\",\"MemberId\":0,\"S3Key\":\"\"}"}

```json
{"Data":"{\"ShopId\":2,\"TSCode\":\"TS250918Q000001\",\"EventName\":\"\",\"TriggerDatetime\":\"2025-09-18T16:01:16.3509765+08:00\",\"OrderCreateDate\":\"2025-09-18T14:08:35.017\",\"PromotionId\":0,\"PromotionEngineType\":\"\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":0,\"OrderTypeDefEnum\":\"ECom\",\"MemberId\":0,\"S3Key\":\"\"}"}
```

## CheckPromotionRuleRecordJob

```json
{"Data": "{\"StartDatetime\":\"2025-03-07T02:00\",\"EndDatetime\":\"2025-03-20T02:45\"}"}
```


## AuditPromotionRewardLoyaltyPointsV2Job

```json
//// tw qa
{"Data":"{\"Id\":\"ee932cc0-b476-4cc1-96d5-d7fb1900418c\",\"IdempotencyKey\":\"TG250411L00004\",\"EventName\":\"OrderCreated\",\"EventArgs\":{\"Market\":null,\"ShopId\":5},\"Source\":{\"ShopId\":5,\"PayType\":\"CreditCardOnce_Stripe\",\"TSCount\":1,\"MemberId\":32909,\"CellPhone\":\"88888888\",\"OrderCode\":\"TG250411L00004\",\"UserAgent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/134.0.0.0 Safari/537.36\",\"CountryCode\":\"852\",\"HttpReferer\":\"https://cccrrrmmm1.shop.qa1.hk.91dev.tw/V2/ShoppingCart/Index?shopId=5\",\"TotalAmount\":200.0,\"CurrencyCode\":\"HKD\",\"OrderDateTime\":\"2025-04-11T02:20:41.7973958+00:00\"},\"SourceType\":\"Orders\",\"SourceKey\":\"TG250411L00004\",\"CreatedAt\":\"2025-04-11T02:20:42.6189354+00:00\"}"}

//// hk qa
{"Data":"{\"FirstTriggerTime\":\"2025-04-11T14:06:20.4337541+08:00\",\"Id\":\"3ab621a0-2c4a-47e5-9c0e-0d86086c9b46\",\"SourceType\":\"Orders\",\"EventName\":\"OrderCreated\",\"IdempotencyKey\":\"TG250411Q00001\",\"Version\":null,\"SourceKey\":\"TG250411Q00001\",\"CreatedAt\":\"2025-04-11T06:05:52.9052972+00:00\",\"EventArgs\":{},\"Source\":{\"ShopId\":2,\"MemberId\":33132,\"OrderCode\":\"TG250411Q00001\",\"TotalAmount\":721000.0,\"CurrencyCode\":\"HKD\",\"OrderDateTime\":\"2025-04-11T14:05:51.7550714+08:00\",\"PayType\":\"CreditCardOnce_Stripe\",\"CellPhone\":\"934565786\"}}"}
```