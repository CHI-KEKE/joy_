## 線上訂單

OrderCancelled / OrderReturned

```json
{
   "Id":"bbb89413-7c87-492e-ba1b-a26d4ef4155d",
   "IdempotencyKey":"TS250812P000148",
   "EventName":"OrderCancelled",
   "EventArgs":{
      "Market":"TW",
      "ShopId":12583
   },
   "Source":{
      "ShopId":12583,
      "TSCode":"TS250812P000148",
      "CancelDateTime":"2025-08-15T01:34:24.0214477+00:00",
      "SaleProductSkuId":1274856,
      "Qty":1,
      "IsReplenished":true
   },
   "SourceType":"Orders",
   "SourceKey":"TS250812P000148",
   "CreatedAt":"2025-08-15T01:38:56.1097455+00:00"
}
```

## 處理邏輯

撈取該 TS CreatedDatetime，確認進行中活動，建立 RecycleLoyaltyPointsV2 / PromotionRecycleCoupon