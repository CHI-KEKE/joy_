



## 付款失敗

### OrderFailedEventTaskData

{"Id": "0e2aef93-369c-419f-8de6-8cc223d1d9aa", "Source": {"ShopId": 2, "TGCode": "TG250901L00001", "PayType": "CreditCardOnce_Stripe", "MemberId": 34376, "TotalAmount": 30.0, "CurrencyCode": "", "OrderDateTime": "2025-09-01T02:15:29.0453833+00:00"}, "CreatedAt": "2025-09-01T02:15:30.6902987+00:00", "EventArgs": {"Market": null, "ShopId": 2}, "EventName": "OrderFailed", "SourceKey": "TG250901L00001", "SourceType": "Orders", "IdempotencyKey": "TG250901L00001"}


### task data

```json
{"Data":"{\"Id\": \"0e2aef93-369c-419f-8de6-8cc223d1d9aa\", \"Source\": {\"ShopId\": 2, \"TGCode\": \"TG250901L00001\", \"PayType\": \"CreditCardOnce_Stripe\", \"MemberId\": 34376, \"TotalAmount\": 30.0, \"CurrencyCode\": \"\", \"OrderDateTime\": \"2025-09-01T02:15:29.0453833+00:00\"}, \"CreatedAt\": \"2025-09-01T02:15:30.6902987+00:00\", \"EventArgs\": {\"Market\": null, \"ShopId\": 2}, \"EventName\": \"OrderFailed\", \"SourceKey\": \"TG250901L00001\", \"SourceType\": \"Orders\", \"IdempotencyKey\": \"TG250901L00001\"}"}


{"Data":"{\"Id\": \"069645b6-a438-46ac-900e-9fc7e8c573df\", \"Source\": {\"ShopId\": 2, \"TGCode\": \"TG250904R00002\", \"PayType\": \"QFPay\", \"MemberId\": 32905, \"TotalAmount\": 119.4, \"CurrencyCode\": \"\", \"OrderDateTime\": \"2025-09-04T07:27:02.5297333+00:00\"}, \"CreatedAt\": \"2025-09-04T07:27:04.2153719+00:00\", \"EventArgs\": {\"Market\": null, \"ShopId\": 2}, \"EventName\": \"OrderFailed\", \"SourceKey\": \"TG250904R00002\", \"SourceType\": \"Orders\", \"IdempotencyKey\": \"TG250904R00002\"}"}



{"Data":"{\"Id\":\"62fcff79-a064-412d-a067-7b85917a3811\",\"IdempotencyKey\":\"TG250901U00011_OrderPlaced\",\"EventName\":\"OrderPlaced\",\"EventArgs\":{\"ShopId\":11,\"Market\":null},\"Source\":{\"OrderCode\":\"TG250901U00011\",\"TotalAmount\":260.0,\"CurrencyCode\":\"HKD\",\"OrderDateTime\":\"2025-09-01T10:34:53.1699009+00:00\",\"PayType\":\"TwoCTwoP\",\"CountryCode\":\"886\",\"CellPhone\":\"909872134\",\"UserAgent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36\",\"HttpReferer\":\"https://shop11.shop.qa1.hk.91dev.tw/V2/ShoppingCart/Index?shopId=11\",\"TSCount\":2,\"ShopId\":11,\"MemberId\":34249},\"SourceType\":\"Orders\",\"SourceKey\":\"TG250901U00011\",\"CreatedAt\":\"2025-09-01T10:34:54.3393328+00:00\"}"}

```

### JobName

PromotionRewardOccupyDispatcher



## GET BY ORDERGROUP


```JSON
{
  "data": [
    {
      "groupId": "92LLAYUUQkqVTyTdvoI5Cw",
      "receiverId": "userYesterdayallen",
      "state": "Redeemed",
      "dispatchedDateTime": "2025-09-01T06:17:59",
      "redeemedDateTime": "2025-09-01T06:17:59",
      "labels": [
        "TG250814DDD"
      ],
      "name": null,
      "market": "HK",
      "shopId": "2",
      "sourceType": "PromotionReward",
      "id": "d578f0"
    }
  ],
  "token": ""
}
```