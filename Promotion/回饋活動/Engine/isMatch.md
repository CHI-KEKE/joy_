

## base.IsMatch

- Enabled
- this.Since <= now && now <= this.Until)
- IsSalesChannelMatched
- IsUserScopeMatched
- IsBirthdayMonthEnabled
- IsConsumerTagMatched
- IsRegionScopeMatched
- IsLocationScopeMatched
- IsPromoCodeMatched
- IsSalesDayOfWeekMatched
- IsPaymentTypeMatched
- IsShippingTypeMatched


## minPricePair

- GetThreshold
- GetMatchedProductScopeItems.Sum(x => x.SalePrice)


## MatchedProductScopeItems

因為各 rule 都有 繼承 PromotionCommonRuleBase, 而 PromotionCommonRuleBase 有實作 IBasicProductScope


```csharp
public interface IBasicProductScope
{
    /// <summary>
    /// Gets the ExcludedProductScopes
    /// </summary>
    IEnumerable<IProductScope> ExcludedProductScopes { get; }

    /// <summary>
    /// Gets the IncludedProductScopes
    /// </summary>
    IEnumerable<IProductScope> IncludedProductScopes { get; }

    /// <summary>
    /// Gets the MustProductScopes
    /// </summary>
    IEnumerable<IProductScope> MustProductScopes { get; }
}
```
