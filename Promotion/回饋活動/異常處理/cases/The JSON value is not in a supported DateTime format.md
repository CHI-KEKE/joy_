
## 訊息


```bash
System.FormatException: The JSON value is not in a supported DateTime format.

{
  "Id": "74c109d4-1da1-4678-8e9f-576f08127107",
  "SourceId": "",
  "JobName": "PromotionRewardCoupon",
  "Data": {
    "ShopId": 2,
    "PromotionEngineId": 6716,
    "PromotionEngineType": "RewardReachPriceWithCoupon",
    "OrderTypeDefEnum": "Others",
    "TradesOrderGroupCode": null,
    "CrmSalesOrderId": 329356,
    "OrderDateTime": "2025-05-08 20:00:00.000"
  }
}
```


### 原因

JSON 欄位內容中 `"OrderDateTime":"2025-05-08 20:00:00.000"` 無法被 System.Text.Json 正確解析為 DateTime
DateTime 少了 T 格式問題