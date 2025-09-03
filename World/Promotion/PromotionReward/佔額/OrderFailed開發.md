



## 付款失敗

### OrderFailedEventTaskData

{"Id": "0e2aef93-369c-419f-8de6-8cc223d1d9aa", "Source": {"ShopId": 2, "TGCode": "TG250901L00001", "PayType": "CreditCardOnce_Stripe", "MemberId": 34376, "TotalAmount": 30.0, "CurrencyCode": "", "OrderDateTime": "2025-09-01T02:15:29.0453833+00:00"}, "CreatedAt": "2025-09-01T02:15:30.6902987+00:00", "EventArgs": {"Market": null, "ShopId": 2}, "EventName": "OrderFailed", "SourceKey": "TG250901L00001", "SourceType": "Orders", "IdempotencyKey": "TG250901L00001"}


### task data

```json
{"Data":"{\"Id\": \"0e2aef93-369c-419f-8de6-8cc223d1d9aa\", \"Source\": {\"ShopId\": 2, \"TGCode\": \"TG250901L00001\", \"PayType\": \"CreditCardOnce_Stripe\", \"MemberId\": 34376, \"TotalAmount\": 30.0, \"CurrencyCode\": \"\", \"OrderDateTime\": \"2025-09-01T02:15:29.0453833+00:00\"}, \"CreatedAt\": \"2025-09-01T02:15:30.6902987+00:00\", \"EventArgs\": {\"Market\": null, \"ShopId\": 2}, \"EventName\": \"OrderFailed\", \"SourceKey\": \"TG250901L00001\", \"SourceType\": \"Orders\", \"IdempotencyKey\": \"TG250901L00001\"}"}


{"Data":"{\"Id\": \"069645b6-a438-46ac-900e-9fc7e8c573df\", \"Source\": {\"ShopId\": 2, \"TGCode\": \"TG250901T00001\", \"PayType\": \"QFPay\", \"MemberId\": 34410, \"TotalAmount\": 158.2, \"CurrencyCode\": \"\", \"OrderDateTime\": \"2025-09-01T09:14:15.1374376+00:00\"}, \"CreatedAt\": \"2025-09-01T09:14:15.1374376+00:00\", \"EventArgs\": {\"Market\": null, \"ShopId\": 2}, \"EventName\": \"OrderFailed\", \"SourceKey\": \"TG250901T00001\", \"SourceType\": \"Orders\", \"IdempotencyKey\": \"TG250901T00001\"}"}
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