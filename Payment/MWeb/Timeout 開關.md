
## Timeout 開關

**Config 設定**

這個在 V1 站台

```xml
<!--單一Domain開關，上線觀察無異常後拔除-->
<add key="Dev.ShopOwnSsoDomainEnabled" value="true"/>

<add key="Dev.PayChannel.FinishPayment.RequestInterval" value="90" />
<add key="Dev.PayChannel.FinishPayment.PaymentTimeout" value="1800" />
<add key="Dev.PayChannel.PaymentTimeout.Default" value="1800" />
<add key="Dev.PayChannel.PaymentTimeout.CreditCardOnce_Stripe" value="1800" />
<add key="Dev.PayChannel.PaymentTimeout.CreditCardOnce_CheckoutDotCom" value="1800" />
<add key="Dev.PayChannel.PaymentTimeout.CreditCardOnce_Razer" value="3600" />
<add key="Dev.PayChannel.PaymentTimeout.CreditCardOnce_Razer" value="3600" />
```

**程式碼使用**

```csharp
int timeoutTimeInSeconds = this._payChannelConfigurations.GetTimeoutTimeInSeconds(context.PayProfileType);
```

**設定格式**

```
PayChannel.PaymentTimeout.{payType}
```

**支援的付款類型**
- TwoCTwoP


## timeout setting

**Recheck Timeout Setting**

```xml
<!-- Recheck Timeout Setting-->	
<add key="QA.PayChannel.PaymentTimeout.Default" value="720" xdt:Transform="Insert" />
<add key="QA.PayChannel.PaymentTimeout.CreditCardOnce_Stripe" value="1800" xdt:Transform="Insert" />
<add key="QA.PayChannel.PaymentTimeout.CreditCardOnce_CheckoutDotCom" value="1800" xdt:Transform="Insert" />
<add key="QA.PayChannel.PaymentTimeout.CreditCardOnce_Cybersource" value="1800" xdt:Transform="Insert" />
<add key="QA.PayChannel.PaymentTimeout.TwoCTwoP" value="1800" xdt:Transform="Insert" />
<add key="QA.PayChannel.PaymentTimeout.CreditCardOnce_Razer" value="3600" xdt:Transform="Insert" />
```