# ApplePay / GooglePay — PMW 付款流程（Pay）

## 架構說明

ApplePay 和 GooglePay 在 PMW 中**沒有獨立的 Plugin**，而是 Stripe Plugin 的一條付款分支。  
入口相同（`POST /Pay`），由 `payMethod` 欄位決定走哪條路：

```
StripePlugin.Pay()
│
├── payMethod ∈ { "ApplePay", "GooglePay" }
│   └→ ProcessMobileWalletPayment()     ← 本文件
│
└── 其他（信用卡相關）
    └→ 一般信用卡路徑
```

---

## 與信用卡付款的根本差異

| 項目 | 信用卡 Pay | ApplePay / GooglePay |
|------|-----------|----------------------|
| 誰建立 PaymentMethod | PMW（帶卡號呼叫 Stripe） | **裝置端**（Device Tokenization）|
| PaymentMethod 來源 | `POST /v1/payment_methods`（PMW 呼叫） | `request.ExtendInfo.PaymentMethod`（前台帶入） |
| API 呼叫數量 | 2（建立 PM + 建立 PI） | **1**（只建立 PI） |
| 需要 `return_url` | ✅ 有（3D 驗證跳轉用） | ❌ 無 |
| 3D 驗證 | 可能觸發 | **不觸發**（裝置已驗證用戶身份） |
| 回傳含 `client_secret` | ❌ | ✅（前台需要）|

---

## Pay 流程

```
前台完成 ApplePay/GooglePay 裝置認證
  └→ 取得 PaymentMethod token（由 Apple/Google 加密）

前台 → POST /Pay（request.ExtendInfo.PaymentMethod = "pm_xxx"）
         ↓
PMW → POST /v1/payment_intents（帶現有 payment_method）
         ↓
   Stripe 直接扣款（無需 3D，裝置已驗證）
         ↓
   回傳 PaymentIntent（status: "succeeded"）
```

---

## API 呼叫：POST /v1/payment_intents

### DirectCharge 模式

```
POST /v1/payment_intents
Stripe-Account: {subAcct}
Authorization: Bearer {privateKey}

{
  "payment_method": "{request.ExtendInfo.PaymentMethod}",   ← 裝置 token，非卡號
  "amount": "{ConvertAmount}",
  "currency": "{request.Currency}",
  "confirmation_method": "automatic",
  "confirm": "true",
  "metadata[request_id]": "{request.RequestId}",
  "metadata[tg_code]": "{request.TradesOrderGroupCode}",
  "application_fee_amount": "{applicationFeeAmount}"
}
```

> ⚠️ 與信用卡 DirectCharge 相比，**沒有 `return_url`**。行動錢包不需要 3D 跳轉。

### DestinationCharge 模式

```
POST /v1/payment_intents
Authorization: Bearer {privateKey}
（無 Stripe-Account header）

{
  "transfer_data[destination]": "{subAccount}",
  "on_behalf_of": "{subAccount}",
  "payment_method": "{request.ExtendInfo.PaymentMethod}",
  "amount": "{ConvertAmount}",
  "currency": "{request.Currency}",
  "confirmation_method": "automatic",
  "confirm": "true",
  "application_fee_amount": "{applicationFeeAmount}",
  "metadata[request_id]": "{request.RequestId}",
  "metadata[tg_code]": "{request.TradesOrderGroupCode}"
}
```

> ⚠️ 與信用卡 DestinationCharge 相比：
> - **沒有 `return_url`**
> - **沒有 `statement_descriptor`**（Mobile Wallet 不呼叫 `GET /v1/accounts/{id}`）

---

## 回傳結構：GetMobileWalletThirdPartyPayResponseEntity

行動錢包有專屬的回傳組建邏輯，與信用卡不同：

| 欄位 | 信用卡 Pay | Mobile Wallet Pay |
|------|-----------|------------------|
| `ReturnCode` | 可能 WaitingToPay（3D）或 Success | **固定 Success**（不檢查 `requires_action`） |
| `ExtendInfo.payment_intent_id` | ✅ | ✅ |
| `ExtendInfo.charge_id` | ✅ | ✅ |
| `ExtendInfo.payment_method` | ✅（卡號 token） | ✅（裝置 token） |
| `ExtendInfo.customer_id` | ✅（記住信用卡時） | ❌ |
| `ExtendInfo.client_secret` | ❌ | **✅**（前台需要完成 Apple/Google Pay session） |
| `ExtendInfo.card_brand` | ❌ | **✅**（如 `"visa"`、`"mastercard"`） |
| `ExtendInfo.card_country` | ❌ | **✅**（發卡行國家） |

```json
// Mobile Wallet Pay 成功回傳範例
{
  "ReturnCode": "1000",
  "TransactionId": "pi_xxx",
  "ExtendInfo": {
    "payment_intent_id": "pi_xxx",
    "charge_id": "ch_xxx",
    "payment_method": "pm_xxx",
    "client_secret": "pi_xxx_secret_yyy",
    "card_brand": "visa",
    "card_country": "US"
  }
}
```

---

## 為什麼行動錢包不需要 3D 驗證？

Apple Pay / Google Pay 使用 **Device Tokenization**：

1. 用戶在裝置上以 Face ID / Touch ID / PIN 完成身份驗證
2. Apple/Google 產生一次性加密 Token（動態 CVV）
3. Token 直接送往 Stripe，Stripe 判斷無需再次 3D 驗證

因此：
- PaymentIntent 不會回傳 `requires_action`
- PMW 回傳固定為 `ReturnCode = 1000`（不會有 WaitingToPay 3D 跳轉）
- Refund / Cancel / QueryPayment 流程與信用卡完全相同
