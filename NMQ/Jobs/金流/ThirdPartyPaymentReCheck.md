## Task Data

```json
{"ThirdPartyPaymentType":"CreditCardOnce_Stripe","startTime":"2025-07-28 16:18:00","endTime":"2025-07-31 16:15:00","PaymentServiceProvider":"PaymentMiddleware"}
```

<br>

## 站台

SCM NMQ

<br>

## 邏輯

#### 將 ThirdPartyPaymentType 解析為 TradesOrderThirdPartyPaymentTypeDefEnum
#### 解析服務是否為 Paymentmiddleware 的 provider
#### 解析 provider

shopId = 0
Table: ShopStaticSetting
Group: PaymentServiceProvider
Key: Paytype

#### 取得需要 Recheck 的訂單

使用 IIThirdPartyPaymentServicve 解析的服務取得需要 Recheck 的訂單

路徑：C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\NMQV2\ThirdPartyPayment\ThirdPartyPaymentReCheckProcessSetting.cs

跨國應為 PaymentMiddlewareReCheckService，他會從 tradesOrderThirdPartyPayment 表拿 WaitingToPay 訂單

#### 執行 OrderReCheck

打 "/webapi/PayChannel/InternalFinishPayment?shopId={body.ShopId}"