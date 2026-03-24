


## 怎麼帶 context.txn_channel

```csharp
private string GetChannel(PayProcessContextEntity context)
{
    switch (context.PayProfileType)
    {
        case PayProfileTypeDefEnum.CreditCardOnce_Razer:
            return "CREDIT";
        case PayProfileTypeDefEnum.CreditCardInstallment_Razer:
            return "CREDITBA";
        case PayProfileTypeDefEnum.OnlineBanking_Razer:
            return context.ShoppingCartV2.SelectedCheckoutPayTypeGroup.PayTypeChannel.PayTypeChannelCode.ToUpper();
        case PayProfileTypeDefEnum.GrabPay_Razer:
            return "GRABPAY";
        case PayProfileTypeDefEnum.Boost_Razer:
            return "BOOST";
        case PayProfileTypeDefEnum.TNG_Razer:
            return "TNG-EWALLET";
        default:
            throw new NotImplementedException("Unsupport PayProfileType");
    }
}
```