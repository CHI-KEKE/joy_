
## Domain Knowledge

ShippingTypeDef = Oversea / Home...
ShippingProfileTypeDef = Oversea / Home...
deliveryTypeIdList = 自己設定的物流方式Id
ShopShippingType = 自己設定的物流方式




## Code Keywords

```csharp
var shippingAreaId = this._configurationService.GetConfiguration<long>(ConfigTypeEnum.Normal, "OverseaShipping:SingaporeArea.ShippingAreaId", 35);


if (salesMarketSetting?.SalesMarket == "SG")
{
    localShippingAreaId = this._configurationService.GetConfiguration<long>(ConfigTypeEnum.Normal, "OverseaShipping:SingaporeArea.ShippingAreaId", 35);
}

/// <summary>
/// IMarketModuleAgentService
/// </summary>
private readonly IMarketModuleAgentService _marketModuleAgentService;



await this.VerifyPostCode(memberLocation);

var salesMarketSetting = this._marketModuleAgentService.GetSupplierSalesMarketSettings();

```


AdministrativeRegionFilePath:Singapore
