


## RecycleLoyaltyPointsV2

<br>

**範例 1：退貨事件**
```json
{"Data":"{\"ShopId\":2,\"EventName\":\"Return\",\"TriggerDatetime\":\"2025-07-02T17:53:47.5375643+08:00\",\"OrderCreateDate\":\"2025-07-01T15:00:00.000\",\"PromotionId\":7371,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"PendingRetryCount\":1,\"CrmSalesOrderSlaveId\":828143,\"OrderTypeDefEnum\":\"Others\"}"}

{"Data":"{\"ShopId\":11,\"EventName\":\"Return\",\"TriggerDatetime\":\"2025-08-19T06:33:52.1939309+08:00\",\"OrderCreateDate\":\"2025-08-14T20:30:00\",\"PromotionId\":7954,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"PendingRetryCount\":1,\"CrmSalesOrderSlaveId\":828883,\"OrderTypeDefEnum\":\"Others\"}"}

{"Data":"{\"ShopId\":11,\"TSCode\":\"\",\"EventName\":\"Return\",\"TriggerDatetime\":\"2025-09-13T06:36:40.2185864+08:00\",\"OrderCreateDate\":\"2025-09-11T18:00:00\",\"PromotionId\":8941,\"PromotionEngineType\":\"RewardReachPriceWithRatePoint2\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":829262,\"OrderTypeDefEnum\":\"Others\",\"MemberId\":34283,\"S3Key\":\"\"}"}


```

<br>

**範例 2：訂單取消事件**
```json
{"Data":"{\"ShopId\":2,\"TSCode\":\"TS250627P000007\",\"EventName\":\"OrderCancelled\",\"TriggerDatetime\":\"2025-06-27T13:50:41.4426856+08:00\",\"OrderCreateDate\":\"2025-06-27T13:46:18.64\",\"PromotionId\":6541,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":0,\"OrderTypeDefEnum\":\"ECom\"}"}
```

<br>

**範例 3：訂單取消事件（活動 6137）**
```json
{"Data":"{\"ShopId\":2,\"TSCode\":\"TS250721P000003\",\"EventName\":\"OrderCancelled\",\"TriggerDatetime\":\"2025-07-21T13:49:17.5995715+08:00\",\"OrderCreateDate\":\"2025-07-21T13:45:37.183\",\"PromotionId\":6137,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":0,\"OrderTypeDefEnum\":\"ECom\"}"}
```

<br>

**範例 4：退貨事件（重試次數 4）**
```json
{"Data":"{\"ShopId\":2,\"TSCode\":null,\"EventName\":\"Return\",\"TriggerDatetime\":\"2025-06-27T11:56:17.7710489+08:00\",\"OrderCreateDate\":\"2025-06-19T08:05:00\",\"PromotionId\":6156,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"PendingRetryCount\":4,\"CrmSalesOrderSlaveId\":828088,\"OrderTypeDefEnum\":\"Others\"}"}
```

<br>

**範例 5：退貨事件（商店 10230）**
```json
{"Data":"{\"ShopId\":10230,\"TSCode\":null,\"EventName\":\"Return\",\"TriggerDatetime\":\"2025-07-12T07:16:26.3944076+08:00\",\"OrderCreateDate\":\"2025-07-10T22:30:00\",\"PromotionId\":31207,\"PromotionEngineType\":\"RewardReachPriceWithRatePoint2\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":993279,\"OrderTypeDefEnum\":\"Others\"}"}
```

<br>

**範例 6：退貨事件（商店 11）**
```json
{"Data":"{\"ShopId\":11,\"TSCode\":null,\"EventName\":\"Return\",\"TriggerDatetime\":\"2025-07-19T06:33:37.0059135+08:00\",\"OrderCreateDate\":\"2025-07-17T14:30:00\",\"PromotionId\":7532,\"PromotionEngineType\":\"RewardReachPriceWithRatePoint2\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":828289,\"OrderTypeDefEnum\":\"Others\"}"}
```

<br>

### 2.2 Task Data

#### 線下訂單
```json
{
  "ShopId": "2",
  "EventName": "Return",
  "TriggerDatetime": "DateTime.Now",
  "OrderCreateDate": "originOrderSlave.CrmSalesOrderSlaveTradesOrderFinishDateTime",
  "PromotionId": "promotion.Id",
  "PromotionEngineType": "promotion.TypeDef",
  "CrmSalesOrderSlaveId": "orderSlave.CrmSalesOrderSlaveId",
  "OrderTypeDefEnum": "CrmOrderSourceTypeDefEnum.Others"
}
```

<br>

#### 線上訂單
```json
{
	"ShopId": 2,
	"TSCode": "TS250619P000011",
	"EventName": "OrderCancelled",
	"TriggerDatetime": "2025-06-19T13:51:56.7019972+08:00",
	"OrderCreateDate": "2025-06-19T13:47:00.223",
	"PromotionId": 5338,
	"PromotionEngineType": "RewardReachPriceWithRatePoint2",
	"PendingRetryCount": 0,
	"CrmSalesOrderSlaveId": 0,
	"OrderTypeDefEnum": "ECom"
}
```
