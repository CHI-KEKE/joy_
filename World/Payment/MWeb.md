# MWeb 文件

## 目錄
1. [關鍵字](#1-關鍵字)
2. [付款轉導](#2-付款轉導)

<br>

---

## 1. 關鍵字

RegisterThirdPartyProcess
TradesOrderPaymentService
PaymentMiddlewareService
PayChannelHelper
QFPayPayChannelService
StripePayChannelService.cs

<br>

---

## 2. 付款轉導

基本機制是，MWeb 在 Pay Request 的 ExtendoInfo 會帶上 Return to 91APP 的 URL，Pay PaymentMiddleware 會回覆一個 3-Party 頁面，付款完成或其他原因跳轉後會使用 Return to 91APP 的 URL，轉導 url 依照裝置與 Paytypes 會有所不同

<br>

### 2.1 一般三方頁面付款方式 (PayMe, QFPay , TwoCTwoP)

/V2/PayChannel/{payMethod}/{payChannel}/{context.TradesOrderGroup.TradesOrderGroup_Code}

<br>

### 2.2 BoCPay_SwiftPass

/V2/PayChannel/{payMethod}/{payChannel}/{context.TradesOrderGroup.TradesOrderGroup_Code}/{shopId}/{context.UniqueKey}/{locale}

<br>

### 2.3 Cybersource

- PayRequest 時不帶上 ReutnrUrl
- Cybersource 頁面付款完後會自己 call 我們的 api
  - PayChannelController / PayChannelReturnPost
  - ✅ 有人呼叫這支 API（通常是 POST），你就直接回傳一整段 HTML 字串（不是 JSON，也不是檔案），而且是可以在瀏覽器中直接執行的 HTML
  - /V2/PayChannel/CreditCardOnce/Cybersource/{tgCode}?shopId={shopId}&k={uniqueKey}

<br>

### 2.4 Andriod

{appInitPath}-s{shopId:D6}://PayChannelReturn?url={encodedUri}

<br>

### 2.5 iOS

同官網

<br>