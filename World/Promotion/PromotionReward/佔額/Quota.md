
## 佔額

### OrderFailed Event 觸發 PromotionRewardOccupyDispatcher 解除佔額

### TaskData Examples

```json
{"Id": "0e2aef93-369c-419f-8de6-8cc223d1d9aa", "Source": {"ShopId": 2, "TGCode": "TG250901L00001", "PayType": "CreditCardOnce_Stripe", "MemberId": 34376, "TotalAmount": 30.0, "CurrencyCode": "", "OrderDateTime": "2025-09-01T02:15:29.0453833+00:00"}, "CreatedAt": "2025-09-01T02:15:30.6902987+00:00", "EventArgs": {"Market": null, "ShopId": 2}, "EventName": "OrderFailed", "SourceKey": "TG250901L00001", "SourceType": "Orders", "IdempotencyKey": "TG250901L00001"}

{"Data":"{\"Id\": \"0e2aef93-369c-419f-8de6-8cc223d1d9aa\", \"Source\": {\"ShopId\": 2, \"TGCode\": \"TG250901L00001\", \"PayType\": \"CreditCardOnce_Stripe\", \"MemberId\": 34376, \"TotalAmount\": 30.0, \"CurrencyCode\": \"\", \"OrderDateTime\": \"2025-09-01T02:15:29.0453833+00:00\"}, \"CreatedAt\": \"2025-09-01T02:15:30.6902987+00:00\", \"EventArgs\": {\"Market\": null, \"ShopId\": 2}, \"EventName\": \"OrderFailed\", \"SourceKey\": \"TG250901L00001\", \"SourceType\": \"Orders\", \"IdempotencyKey\": \"TG250901L00001\"}"}


{"Data":"{\"Id\": \"069645b6-a438-46ac-900e-9fc7e8c573df\", \"Source\": {\"ShopId\": 4, \"TGCode\": \"MG250919Q00002\", \"PayType\": \"CreditCardOnce_Razer\", \"MemberId\": 1151, \"TotalAmount\": 119.4, \"CurrencyCode\": \"\", \"OrderDateTime\": \"2025-09-19T06:27:43.4964674+00:00\"}, \"CreatedAt\": \"2025-09-19T06:27:43.4964674+00:00\", \"EventArgs\": {\"Market\": null, \"ShopId\": 4}, \"EventName\": \"OrderFailed\", \"SourceKey\": \"MG250919Q00002\", \"SourceType\": \"Orders\", \"IdempotencyKey\": \"MG250919Q00002\"}"}



{"Data":"{\"Id\":\"62fcff79-a064-412d-a067-7b85917a3811\",\"IdempotencyKey\":\"TG250901U00011_OrderPlaced\",\"EventName\":\"OrderPlaced\",\"EventArgs\":{\"ShopId\":11,\"Market\":null},\"Source\":{\"OrderCode\":\"TG250901U00011\",\"TotalAmount\":260.0,\"CurrencyCode\":\"HKD\",\"OrderDateTime\":\"2025-09-01T10:34:53.1699009+00:00\",\"PayType\":\"TwoCTwoP\",\"CountryCode\":\"886\",\"CellPhone\":\"909872134\",\"UserAgent\":\"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/139.0.0.0 Safari/537.36\",\"HttpReferer\":\"https://shop11.shop.qa1.hk.91dev.tw/V2/ShoppingCart/Index?shopId=11\",\"TSCount\":2,\"ShopId\":11,\"MemberId\":34249},\"SourceType\":\"Orders\",\"SourceKey\":\"TG250901U00011\",\"CreatedAt\":\"2025-09-01T10:34:54.3393328+00:00\"}"}

```

## OrderPlaced 觸發線上訂單佔額

```json
{"Data":"{\"Id\":\"7e6b870d-4dc0-4fe7-9772-9a84dc6e0afc\",\"IdempotencyKey\":\"TG251026UA010M_OrderPlaced\",\"EventName\":\"OrderPlaced\",\"EventArgs\":{\"ShopId\":360,\"Market\":null},\"Source\":{\"OrderCode\":\"TG251026UA010M\",\"TotalAmount\":366.0,\"CurrencyCode\":\"TWD\",\"OrderDateTime\":\"2025-10-26T10:40:14.777756+00:00\",\"PayType\":\"FamilyMartOnlinePay\",\"CountryCode\":\"886\",\"CellPhone\":\"0988520184\",\"UserAgent\":\"Mozilla/5.0 (iPhone; CPU iPhone OS 18_6_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148fmshopping+\",\"HttpReferer\":\"https://mart.family.com.tw/V2/ShoppingCart/Index?shopId=360\",\"TSCount\":6,\"ShopId\":360,\"MemberId\":2026629},\"SourceType\":\"Orders\",\"SourceKey\":\"TG251026UA010M\",\"CreatedAt\":\"2025-10-26T10:40:15.2021682+00:00\"}"}
```


## 線下訂單怎麼觸發佔額


線上訂單 : PromotionRewardOccupyDispatcher 長 Record, PromotionRewardLoyaltyPointsV2Job 有 record 才繼續做否則噴
線下訂單 : PromotionRewardLoyaltyPointsV2Job 無 Record, 繼續執行佔額


## 績效報表

1. 非佔額活動
2. 佔額活動且佔額成功(MATCH + 佔)

##　狀態說明

OrderPlaced :

佔額活動且符合活動條件但佔額失敗　MatchWithoutQuota
線上訂單，佔額活動且成功佔額且符合活動條件，狀態為佔額中　Occupy

OrderCreated:

Occupy => WaitToReward

## PCPS API

### GET BY ORDERGROUP


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


## 取得訂單給點紀錄 (促購後台)

`loyalty-point-query` : 取得訂單給點紀錄 要篩選掉 Unmatch、Occupy


##　佔額建立 (促購後台)

佔額無法回朔，過去的訂單，過去的活動可以中，但佔額如果瞄一樣的起始時間不會中
現在佔額是瞄活動開始時間，並且沒有結束時間

- 活動已開始不更新佔額設定
- 過去活動可以中過去訂單、可以站過去的佔額
- 未來活動不能站未來訂單
- Quota = 0 表示無佔額設定(無限制)

## 佔額更新

- 還沒開始的活動還是可以更新時間


資料表 : PromotionEngine_GroupCode


## get-rule-record

`rule-record`

GroupCode 


## 促購前台


因為佔額無關計算規則，是額外的設定，你即使中了，但額部不夠仍然無法回饋!!


## 逆流程

OCCUPY => 重算後沒中就釋放佔額

給券已給不會釋放置佔額
