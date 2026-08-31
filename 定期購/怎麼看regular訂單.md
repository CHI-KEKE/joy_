


TradesOrderSlave_ShippingTypeDef == (byte)SaleProductShippingTypeEnum.RegularOrder



## 判斷購買一次


//// 排除只購買一次的定期購訂單
var onceRegularOrderSkuIds = context.ShoppingCartV2.SelectedMaxDeliveryCountList.Where(
                                x => x.MaxDeliverCount == 1).Select(x => x.SaleProductSKUId).ToList();



## 結帳拿token


GetCheckOutInfo

if (market == "TW" && context.PayProfileType == PayProfileTypeDefEnum.CreditCardOnce)
{
    var(creditCardToken, creditCardKey, identificationCode, provider, bankCode, firstSix, lastFour) = this.GetCreditCardDataBysCreditCardGateway(context);
    extendInfo.Add("credit_card_token", creditCardToken);
    extendInfo.Add("credit_card_key", creditCardKey);
    extendInfo.Add("identification_code", identificationCode);
    extendInfo.Add("provider", provider);

    if (context.CreditCardGatewayType == PayProcessCreditCardGatewayTypeEnum.Nine1Payment)
    {
        extendInfo.Add("bank_code", bankCode);
        extendInfo.Add("first_six", firstSix);
        extendInfo.Add("last_four", lastFour);
    }
}
else if (market == "MY" && context.PayProfileType == PayProfileTypeDefEnum.CreditCardOnce_Razer)
{
    extendInfo.Add("credit_card_token", context.ThirdPartyPaymentInfo?.ExtendInfo["creditcard_token"]?.ToString());
    extendInfo.Add("credit_card_user_email", context.Email);
    extendInfo.Add("credit_card_user_phone", context.Receiver.CellPhone);
    extendInfo.Add("credit_card_user_fullname", context.ThirdPartyPaymentInfo?.ExtendInfo["billing_name"]?.ToString());
}
else if (market == "HK" && context.PayProfileType == PayProfileTypeDefEnum.CreditCardOnce_Stripe)
{
    //// 優先從 paytype_express_info 取得快速結帳資訊（使用者已儲存的信用卡）
    var thirdPartyPaymentExtendInfo = context.ThirdPartyPaymentInfo?.ExtendInfo;
    string customerId = null;
    string paymentMethod = null;

    if (thirdPartyPaymentExtendInfo == null)
    {
        throw new RegularOrderException("定期購 Stripe 付款: ThirdPartyPaymentInfo.ExtendInfo 為 null，無法取得支付資訊，定期購未建立。");
    }

    if (thirdPartyPaymentExtendInfo.TryGetValue("paytype_express_info", out object infoObj)
        && infoObj is null == false
        && infoObj.ObjToDictionary<object>() is var payTypeExpressInfo
        && payTypeExpressInfo is null == false)
    {
        //// 從快速結帳資訊中取得
        customerId = payTypeExpressInfo.TryGetValue("customer_id", out object expressCustomerId) ? expressCustomerId?.ToString() : null;
        paymentMethod = payTypeExpressInfo.TryGetValue("payment_method", out object expressPaymentMethod) ? expressPaymentMethod?.ToString() : null;
    }

    //// 如果沒有快速結帳資訊，則從直接的 ExtendInfo 取得（付款完成後 Stripe 提供的資料）
    if (string.IsNullOrEmpty(customerId))
    {
        customerId = thirdPartyPaymentExtendInfo.TryGetValue("customer_id", out object stripeCustomerId) ? stripeCustomerId?.ToString() : null;
    }

    if (string.IsNullOrEmpty(paymentMethod))
    {
        paymentMethod = thirdPartyPaymentExtendInfo.TryGetValue("payment_method", out object stripePaymentMethod) ? stripePaymentMethod?.ToString() : null;
    }

    //// 若從 payTypeExpressInfo 或 thirdPartyPaymentExtendInfo 都拿不到資料
    if (string.IsNullOrEmpty(customerId)
        || string.IsNullOrEmpty(paymentMethod))
    {
        throw new RegularOrderException("定期購 Stripe 付款: 無法取得 customer_id 或 payment_method，定期購資料未儲存");
    }

    extendInfo["customer_id"] = customerId;
    extendInfo["payment_method"] = paymentMethod;
    extendInfo["country"] = context.CreditCardInfo?.IssueCountryCode;
    extendInfo["association"] = context.CreditCardInfo?.Brand.ToString();
}