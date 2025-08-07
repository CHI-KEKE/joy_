# OSM 文件

## 目錄
1. [三方金物流設定相關](#1-三方金物流設定相關)

<br>

---

## 1. 三方金物流設定相關

### 1.1 API

<br>

**儲存**：https://sms.qa1.hk.91dev.tw/Api/ThirdPartyServices/Payment/UpdateSettingDetails

<br>

**顯示**：https://sms.qa1.hk.91dev.tw/Api/ThirdPartyServices/Payment/GetPaymentSettingsInfo?shopId=2

<br>

### 1.2 設計稿

<br>

https://www.figma.com/design/5DvenyhpnBX5ftehBigTEp/Stripe-Apple-Pay-%26-Google-Pay%E4%B8%B2%E6%8E%A5?t=Wn2ZesEZrfsYrld5-0

<br>

### 1.3 新增 GooglePay / ApplePay 與 Stripe 共用

<br>

**PR**：https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33808/diff

<br>

### 1.4 新增 ApplePay 中間狀態

<br>

**PR**：
- https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33823/diff
- https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33832/diff

<br>

**設定項目**：Stripe-IsApplePayActive-是否開通ApplePay

<br>

### 1.5 調整 Stripe 設定是否有效判斷

<br>

**PR**：https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33868/diff

<br>

### 1.6 新增 PaymentMethodSetting

<br>

新增以下設定類別：
- GooglePayStripePaymentMethodSetting
- ApplePayStripePaymentMethodSetting
- CreditCardOnceStripePaymentMethodSetting

<br>

**PR**：
- https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33845/diff
- https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33907/diff
- https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33918/diff
- https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33981/diff

<br>

### 1.7 ShopSecret 歷程

<br>

**PR**：https://bitbucket.org/nineyi/nineyi.sms/pull-requests/33859/diff

<br>

### 1.8 三方金物流設定開關、PayProfile 開關

<br>

**PR**：https://bitbucket.org/nineyi/nineyi.configuration/pull-requests/4016/overview

<br>

**Key**：
- Prod.PayProfileType.GooglePay.EnabledShopIds
- Prod.ThirdParty.PayProfile.EnbaleSetting

<br>