## 說明

OrderCreated Event 觸發，一個 Job 代表一張 TG

## Task Data

```json
{
   "Id":"48b97525-26ca-497b-8c02-d601f4d535c7",
   "IdempotencyKey":"TG250305L00003",
   "EventName":"OrderCreated",
   "EventArgs":{
      "Market":null,
      "ShopId":2
   },
   "Source":{
      "ShopId":2,
      "PayType":"CreditCardOnce_Stripe",
      "TSCount":1,
      "MemberId":33056,
      "CellPhone":"908876209",
      "OrderCode":"TG250305L00003",
      "UserAgent":"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/133.0.0.0 Safari/537.36",
      "CountryCode":"886",
      "HttpReferer":"https://shop2.shop.qa1.hk.91dev.tw/V2/ShoppingCart/Index?shopId=2",
      "TotalAmount":4685.0,
      "CurrencyCode":"HKD",
      "OrderDateTime":"2025-03-05T02:26:30.4280073+00:00"
   },
   "SourceType":"Orders",
   "SourceKey":"TG250305L00003",
   "CreatedAt":"2025-03-05T02:26:31.8754994+00:00"
}
```

<br>

## 邏輯

取得進行中點數活動：

**StartDate：** 定義為往前拉 7 天
**EndDate：** 怕因為快取漏拉 + 30min
**Promotion Web API：** `api/promotion-rules/ongoing-list`

根據 On 檔活動一個個觸發回饋 Job

## 餵資料測試

```json
{"Data":"{\"Id\":\"9edb429f-7178-4d0d-a17d-93734f97898f\",\"IdempotencyKey\":\"TG250326T00001\",\"EventName\":\"OrderCreated\",\"EventArgs\":{\"Market\":null,\"ShopId\":11},\"Source\":{\"ShopId\":11,\"PayType\":\"TwoCTwoP\",\"TSCount\":4,\"MemberId\":33367,\"CellPhone\":\"921383890\",\"OrderCode\":\"TG250326T00001\",\"UserAgent\":\"Amazon CloudFront\",\"CountryCode\":\"886\",\"HttpReferer\":null,\"TotalAmount\":20.0,\"CurrencyCode\":\"HKD\",\"OrderDateTime\":\"2025-03-26T09:03:54.314713+00:00\"},\"SourceType\":\"Orders\",\"SourceKey\":\"TG250326T00001\",\"CreatedAt\":\"2025-03-26T09:03:55.3940699+00:00\"}"}
```

```json
{"Data":"{\"Id\":\"6214d1ea-5553-4f5f-bda8-3091e6a8bc66\",\"IdempotencyKey\":\"TG250417P00063\",\"EventName\":\"OrderCreated\",\"EventArgs\":{\"Market\":null,\"ShopId\":12583},\"Source\":{\"ShopId\":12583,\"PayType\":\"CreditCardOnce\",\"TSCount\":1,\"MemberId\":856414,\"CellPhone\":\"0988782931\",\"OrderCode\":\"TG250417P00063\",\"UserAgent\":\"Mozilla/5.0 (iPhone; CPU iPhone OS 17_5 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.2 Mobile/15E148 Safari/604.1\",\"CountryCode\":\"886\",\"HttpReferer\":\"https://12583.shop.qa.91dev.tw/V2/ShoppingCart/Index?shopId=12583\",\"TotalAmount\":70.0,\"CurrencyCode\":\"TWD\",\"OrderDateTime\":\"2025-04-17T05:37:03.7566983+00:00\"},\"SourceType\":\"Orders\",\"SourceKey\":\"TG250417P00063\",\"CreatedAt\":\"2025-04-17T05:37:04.2490675+00:00\"}"}
```