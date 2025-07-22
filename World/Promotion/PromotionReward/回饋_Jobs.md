# 回饋 Jobs 文件

## 目錄
1. [PromotionRewardCoupon](#promotionrewardcoupon)
2. [PromotionRewardLoyaltyPointsV2](#promotionrewardloyaltypointsv2)
3. [ValidTypeMemos](#validtypememos)

<br>

---

## PromotionRewardCoupon

### 餵資料測試

以下是 PromotionRewardCoupon 的測試資料範例：

<br>

**範例 1：基本訂單資料**
```json
{"Data":"{\"ShopId\":11,\"TradesOrderGroupCode\":null,\"CrmSalesOrderId\":329444,\"OrderDateTime\":\"2025-05-22T18:30:00\",\"PromotionEngineId\":6886,\"PromotionEngineType\":\"RewardReachPriceWithCoupon\",\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\"}"}
```

<br>

**範例 2：包含會員資料**
```json
{"Data":"{\"ShopId\":11,\"TradesOrderGroupCode\":null,\"CrmSalesOrderId\":329444,\"OrderDateTime\":\"2025-05-22T18:30:00\",\"PromotionEngineId\":6886,\"PromotionEngineType\":\"RewardReachPriceWithCoupon\",\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\",\"MemberId\":123,\"S3Key\":\"abc\"}"}
```

<br>

**範例 3：電商訂單**
```json
{"Data":"{\"ShopId\":2,\"TradesOrderGroupCode\":\"TG250625T00005\",\"CrmSalesOrderId\":0,\"OrderDateTime\":\"2025-06-25T17:45:52.8943886+08:00\",\"PromotionEngineId\":7307,\"PromotionEngineType\":null,\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"ECom\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

---

## PromotionRewardLoyaltyPointsV2

### 餵資料測試

以下是 PromotionRewardLoyaltyPointsV2 的測試資料範例：

<br>

**範例 1：電商訂單資料**
```json
{"Data":"{\"ShopId\":2,\"TradesOrderGroupCode\":\"TG250627P00006\",\"CrmSalesOrderId\":0,\"OrderDateTime\":\"2025-06-27T13:47:44.9064825+08:00\",\"PromotionEngineId\":6137,\"PromotionEngineType\":null,\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"ECom\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

**範例 2：CRM 銷售訂單**
```json
{"Data":"{\"ShopId\":2,\"TradesOrderGroupCode\":\"CrmSalesOrder:329599\",\"CrmSalesOrderId\":329599,\"OrderDateTime\":\"2025-06-30T17:00:00+08:00\",\"PromotionEngineId\":7371,\"PromotionEngineType\":null,\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

**範例 3：電商訂單（商店 11）**
```json
{"Data":"{\"ShopId\":11,\"TradesOrderGroupCode\":\"TG250629S00001\",\"CrmSalesOrderId\":0,\"OrderDateTime\":\"2025-06-29T16:58:04.8305713+08:00\",\"PromotionEngineId\":7366,\"PromotionEngineType\":null,\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"ECom\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

**範例 4：簡化訂單資料**
```json
{"Data":"{\"ShopId\":2,\"CrmSalesOrderId\":329596,\"OrderDateTime\":\"2025-06-29T18:00:00.000\",\"PromotionEngineId\":7371,\"OrderTypeDefEnum\":\"Others\"}"}
```

<br>

**範例 5：點數回饋訂單**
```json
{"Data":"{\"ShopId\":2,\"CrmSalesOrderId\":329596,\"OrderDateTime\":\"2025-06-29T18:00:00.000\",\"PromotionEngineId\":7371,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

**範例 6：點數回饋訂單（訂單 329599）**
```json
{"Data":"{\"ShopId\":2,\"CrmSalesOrderId\":329599,\"OrderDateTime\":\"2025-06-30T17:00:00.000\",\"PromotionEngineId\":7371,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

**範例 7：點數回饋訂單（訂單 329601）**
```json
{"Data":"{\"ShopId\":2,\"CrmSalesOrderId\":329601,\"OrderDateTime\":\"2025-06-30T20:00:00.000\",\"PromotionEngineId\":7398,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

**範例 8：包含關聯訂單**
```json
{"Data":"{\"ShopId\":10230,\"CrmSalesOrderId\":405587,\"OrderDateTime\":\"2025-07-10T22:30:00\",\"PromotionEngineId\":31207,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\",\"RelatedOrderSlaveIdList\":[993236]}"}
```

<br>

---

## ValidTypeMemos

### 有效類型備註清單

以下是系統支援的有效類型備註：

<br>

- **Normal** - 一般訂單
- **Others** - 其他類型
- **Other** - 其他
- **Ignored** - 忽略的訂單
- **Empty** - 空訂單
- **Gift** - 贈品訂單
- **Coupon** - 優惠券訂單
- **Point** - 點數訂單

<br>