C:\91APP\NineYi.Sms\WebSite\WebSite\Controllers\Apis\GlobalShippingController.cs



```csharp
var shop = this._userService.GetCurrentUser().ShopList.First(s => s.ShopId == shopId);
var key = this._shopRepository.GetShopDefault(shop.ShopId, "GlobalShipping", "EasyParcelAPIKey")?.ShopDefault_Value;
return new Dictionary<string, object>
{
    { "EnableGlobalShipping", string.IsNullOrEmpty(key) == false }
};
```

=> 有 key 應該就會有