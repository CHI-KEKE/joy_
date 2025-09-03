# MWeb 文件

## 目錄
1. [關鍵字](#1-關鍵字)
2. [Timeout 開關](#2-timeout-開關)

<br>

---

## 1. 關鍵字

RegisterThirdPartyProcess
TradesOrderPaymentService
PaymentMiddlewareService
PayChannelHelper
QFPayPayChannelService
StripePayChannelService.cs
RegisterThirdPartyFinishProcess 付款後轉倒流程

頁面轉導
C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\MobileWebMallV2\Controllers\PayChannelController.cs => PayChannelReturn

<br>

---

## 2. Timeout 開關

**Config 設定**：

<br>

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

<br>

**程式碼使用**：

<br>

```csharp
int timeoutTimeInSeconds = this._payChannelConfigurations.GetTimeoutTimeInSeconds(context.PayProfileType);
```

<br>

**設定格式**：

<br>

```
PayChannel.PaymentTimeout.{payType}
```

<br>

**支援的付款類型**：

<br>

- TwoCTwoP

<br>

---