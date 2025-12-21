
## 📋 目錄
1. [MaxRewardPoint, MaxRewardCouponQty](#maxrewardpoint-maxrewardcouponqty)
2. [引擎](#引擎)
3. [Parse](#parse)

---


## MaxRewardPoint, MaxRewardCouponQty

```csharp
/// <summary>
/// 給點上限
/// </summary>
public decimal? MaxRewardPoint { get; set; }
```

PromotionBaseEntity.RewardPointRule.MaxRewardPoint 節點傳入

<br>
<br>

## 引擎

RewardReachPriceWithPointThreshold 加上 MaximumPoints

<br>
<br>

## Parse

- ParsePromotionEngineRuleObject 時需從 threshold 正確 mappingMaxRewardPoint 出來給 PromotionEngine_Rule 可以存取