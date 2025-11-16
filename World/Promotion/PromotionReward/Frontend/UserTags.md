## Cart API 帶 UserTags 到促購

Cart API 的 GetUserTags 方法會取得會員於購物車流程中的「會員標籤」，標籤將用於促銷引擎判斷目標對象（如：全會員、新會員、指定卡別會員）。

<br>

```csharp
/// <summary>
/// 取得會員標籤
/// </summary>
/// <param name="context">CartContext</param>
/// <param name="vipMember">VipMemberBriefDataEntity</param>
/// <returns>會員標籤</returns>
private string[] GetUserTags(CartContext context, VipMemberBriefDataEntity? vipMember)
{
    var userTags = new List<string>
    {
        PromotionEngineConstants.AllUserScope
    };

    if (context.Data.HasCrmShopContract == true)
    {
        userTags.Add($"{PromotionEngineConstants.CrmShopMemberCard}:{context.Data.CrmMemberTier.CrmShopMemberCardId}");
    }

    if (vipMember == null)
    {
        return userTags.ToArray();
    }

    if (vipMember.IsFirstPurchase)
    {
        userTags.Add(PromotionEngineConstants.FirstPurchase);
    }

    return userTags.ToArray();
}
```

<br>

## 輸出

```json
["AllUserScope"]
["AllUserScope", "CrmShopMemberCard:5"]
["AllUserScope", "FirstPurchase"]
```

<br>

## 促購前台組成 Threshold

促購前台會根據 RewardPointRuleList 組成活動條件門檻（Threshold），以便促銷引擎在判斷是否給點時，依據不同會員卡分組進行。

<br>

```csharp
/// <summary>
/// 組成促購引擎規則
/// </summary>
/// <param name="promotionEngineId">PromotionEngine_Id</param>
/// <param name="entity">PromotionBaseEntity</param>
/// <returns>PromotionEngine_Rule Json String</returns>
public string GetPromotionEngineRule(long promotionEngineId, PromotionBaseEntity entity)
{
    //// 組成活動條件階層
    var thresholds = new Dictionary<string, RewardReachPriceWithPointThreshold>();

    //// 給點條件
    entity.RewardPointRuleList.ForEach(rewardPointRule =>
    {
        if (decimal.TryParse(rewardPointRule.Condition.ToString(), out var reachPrice) == false || rewardPointRule.Point == null)
        {
            throw new ApplicationException("活動折抵條件有誤");
        }

        var rewardReachPriceWithPointThreshold = new RewardReachPriceWithPointThreshold()
        {
            ReachPricePointPairs = new List<ReachPricePointPair>()
            {
                new ReachPricePointPair(reachPrice, rewardPointRule.Point.Value)
            }
        };

        //// 若 CrmShopMemberCardId = 0，代表活動適用全體會員
        var crmShopMemberCard = rewardPointRule.CrmShopMemberCardId == 0 ?
            nameof(AllUserScope) :
            $"CrmShopMemberCard:{rewardPointRule.CrmShopMemberCardId}";

        thresholds.Add(crmShopMemberCard, rewardReachPriceWithPointThreshold);
    });
}
```

```json
{
  "AllUserScope": {
    "ReachPricePointPairs": [
      { "ReachPrice": 1000, "Point": 50 }
    ]
  },
  "CrmShopMemberCard:5": {
    "ReachPricePointPairs": [
      { "ReachPrice": 800, "Point": 80 }
    ]
  }
}
```

## UserTags vs Threshold

會員標籤（UserTags）與 Threshold Key 對應：前台組成 UserTags，後台 Thresholds 以相同 Tag 為 Key，促銷引擎在執行時做交集比對。當 CrmShopMemberCardId = 0：代表適用所有會員，使用 AllUserScope

```csharp
public override bool IsMatch(ShoppingCartContext context)
{
    if (!base.IsMatch(context)) return false;

    var threshold = this.GetThreshold(context.Consumer);
    if (threshold == null) return false;

    var minPricePair = threshold.ReachPricePointPairs.OrderBy(x => x.ReachPrice).FirstOrDefault();
    return minPricePair != null && this.GetMatchedProductScopeItems(context).Sum(x => x.SalePrice) >= minPricePair.ReachPrice;
}
```

