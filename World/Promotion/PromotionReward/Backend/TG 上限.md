## 📋 目錄
1. [節點 (MaxRewardPoint, MaxRewardCouponQty)](#節點-maxrewardpoint-maxrewardcouponqty)
2. [引擎](#引擎)
3. [Parse](#parse)

---

## 節點 (MaxRewardPoint, MaxRewardCouponQty)

```csharp
/// <summary>
/// 給點上限
/// </summary>
public decimal? MaxRewardPoint { get; set; }
```

PromotionBaseEntity.RewardPointRule.MaxRewardPoint 節點傳入


## 引擎

RewardReachPriceWithPointThreshold 加上 MaximumPoints

## Parse

- ParsePromotionEngineRuleObject 時需從 threshold 正確 mappingMaxRewardPoint 出來給 PromotionEngine_Rule 可以存取