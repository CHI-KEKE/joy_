DiscountReachPriceWithFreeGift

C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\MobileWebMallV2\ClientApp\src\promotion\components\promotionEngineDetail\promotionCard.tsx

C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\MobileWebMallV2\ClientApp\src\common\components\currencyWord\currencyPreferred.tsx

{
    //// 單階
    var firstLevel = this._rule.ReachPriceFreeGiftPairs.OrderBy(x => x.ReachPrice).First();
    rule.Append(StringUtility.PeacefulFormat(Translation.Backend.Service.PromotionEngineDiscountReachPriceWithFreegift.ReachPriceGetGift, this._currencyService.ToCurrency(this._context.ShopId, firstLevel.ReachPrice), firstLevel.Gift.Qty));
}


