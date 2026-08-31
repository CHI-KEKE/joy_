


salepageid = 7631

period = ["7","10"]


shop_limit_status = 1 = 無限制配送次數
rpd_buy_setting = 2 = ????


SHOP 7 帳號


01912345678
123456


購物車會依照Provider 去拿商品頁資料


case nameof(RegularOrderProviderTypeDefEnum.Nine1V2):
    return (
        _httpClientFactory.CreateClient("Nine1V2RegularPurchaseWebApiClient"),
        this._configurationService.GetConfiguration<string>(ConfigType.Normal, "InternalServiceDomains:Nine1V2RegularPurchaseWebApi:Endpoints:GetRegularSetting:Uri")!);


default:
    //// GameBall 或未設定，使用原始 GameBall 客戶端
    return (
        _httpClientFactory.CreateClient("RegularPurchaseWebApiClient"),
        this._configurationService.GetConfiguration<string>(ConfigType.Normal, "InternalServiceDomains:RegularPurchaseWebApi:Endpoints:GetRegularSetting:Uri")!);


C:\91APP\Cart\cart2\nine1.cart\src\BusinessLogic\Nine1.Cart.BL.Services\Processor\Cart\Create\GetShoppingCartRegularSalePageProcessor.cs




MG260626R00013
MS260626R000013


