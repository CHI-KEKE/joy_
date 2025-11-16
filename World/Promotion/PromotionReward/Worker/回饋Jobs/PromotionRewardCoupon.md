## 餵資料測試

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


## GetAndCheckPromotionRuleAsync

Ecom & Others 皆會確認通路

`promotion.Rule.MatchedSalesChannels


## GetCouponRewardRecordAsync

## PromotionRewardCouponCalculateAsync

## ProcessOccupyQuotaAsync

## PromotionRewardLoyaltyPointsPerformanceV2

## InsertCouponRewardRecordAsync

isQuotaPromotion & isOccupy & IsMatch & ECom
==> Occupy

IsMatch & isOccupy == false => MatchWithoutQuota


IsMatch => WaitToReward
IsMatch = false => Unmatch