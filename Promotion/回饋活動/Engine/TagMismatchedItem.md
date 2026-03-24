
每一個 purchaseItem 都有 Flags，要在沒中該活動的 purchaseItem 掛上對應 Promotion 的 TagsWhenMismatched


```csharp
/// <summary>
/// 根據PromotionRecord標記tag在未符合的商品上
/// </summary>
/// <param name="cart"></param>
/// <param name="record"></param>
private void TagMismatchedItem(ShoppingCartContext cart, PromotionRecord record)
{
    var mismatchedTag = Rules[record.PromotionRuleId].TagsWhenMismatched ?? new HashSet<string>();

    //// 把Tags標在PurchasedItem的Flags上
    foreach (var pItem in cart.PurchasedItems.Where(x => record.PurchasedItemIds.Contains(x.Id)))
    {
        pItem.Flags.UnionWith(mismatchedTag);
    }
}
```