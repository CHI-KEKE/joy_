# ThirdPartyPaymentReCheck 文件

## 目錄
1. [Task Data](#1-task-data)
2. [站台](#2-站台)
3. [邏輯](#3-邏輯)

<br>

---

## 1. Task Data

```json
{"ThirdPartyPaymentType":"CreditCardOnce_Stripe","startTime":"2025-07-28 16:18:00","endTime":"2025-07-31 16:15:00","PaymentServiceProvider":"PaymentMiddleware"}
```

<br>

---

## 2. 站台

SCM NMQ

<br>

---

## 3. 邏輯

### 3.1 解析 ThirdPartyPaymentType

將 ThirdPartyPaymentType 解析為 TradesOrderThirdPartyPaymentTypeDefEnum

<br>

### 3.2 解析服務提供商

解析服務是否為 Paymentmiddleware 的 provider

<br>

### 3.3 解析 provider

shopId = 0

<br>

Table: ShopStaticSetting

<br>

Group: PaymentServiceProvider

<br>

Key: Paytype

<br>

### 3.4 取得需要 Recheck 的訂單

使用 IIThirdPartyPaymentServicve 解析的服務取得需要 Recheck 的訂單

<br>

路徑：C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\NMQV2\ThirdPartyPayment\ThirdPartyPaymentReCheckProcessSetting.cs

<br>

跨國應為 PaymentMiddlewareReCheckService，他會從 tradesOrderThirdPartyPayment 表拿 WaitingToPay 訂單

<br>

### 3.5 執行 OrderReCheck

打 "/webapi/PayChannel/InternalFinishPayment?shopId={body.ShopId}"

<br>