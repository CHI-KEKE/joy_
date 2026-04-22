
| 參數 | 設定值 |
|------|--------|
| **userContext** | `new UserContext(context.Calculate.User.Id, context.Calculate.User.Tags)` |
| **locationContext** | `new LocationContext(context.Shipping.LocationId, context.Shipping.LocationTags, isLocationAlwaysMatch)` (線上訂單) |
| **Channel** | `context.Calculate.Channel` |
| **CurrencyDecimalDigits** | `context.Calculate.CurrencyDecimalDigits` |

**執行購買 Purchase**

🛒 **資料處理**: 打資料倒進 `_purchasedItems`

**📦 ProductItem 結構**
```csharp
productItem
- Id = item.SalepageId
- SkuId = item.SkuId
- ListPrice = item.Payment
- Tags = item.Tags
```

- `ISet<string> flags = item.Flags.ToHashSet()`
- `TotalPrice += purchasedItem.SalePrice` (SalePrice = item.ListPrice)