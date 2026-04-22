# Stripe Refund — DirectCharge 退款

## 適用條件

`request.ExtendInfo.StripePaymentFlow == StripePaymentFlowEnum.DirectCharge`

---

## DirectCharge 模式簡介

> DirectCharge：付款時 PaymentMethod 和 PaymentIntent 都在**子帳號（Connected Account）**下建立。  
> 退款時，所有操作也必須在**子帳號**下執行，需帶 `Stripe-Account` header。

---

## 完整退款 API 序列

### Step 1：取得 PaymentIntent（帶子帳號）

```
GET /v1/payment_intents/{transactionId}
Stripe-Account: {subAcct}
Authorization: Bearer {privateKey}
```

回應中取出：
```
paymentIntent.charges.data[0].id            → charge.id（退款用）
paymentIntent.charges.data[0].ApplicationFee → fee.id（退手續費用）
```

> `paymentIntent.charges.data.Single()` — 預期只有一筆 charge，若無則拋 `ArgumentNullException`（→ ReturnCode: RefundFailed）

---

### Step 2（條件）：退還 Application Fee

僅在 `IsRefundApplicationFee=true` 且 `ApplicationFeeAmount > 0` 時執行：

```
POST /v1/application_fees/{charge.ApplicationFee}/refunds
Authorization: Bearer {privateKey}
（無 Stripe-Account header — Application Fee 在主帳號下）

{
  "amount": "{AmountConvert(ApplicationFeeAmount)}"
}
```

> ⚠️ Application Fee Refund **不帶子帳號 header**，因為 Application Fee 收取後存放在平台主帳號，退款也在主帳號執行。

---

### Step 3（DirectCharge 跳過）：Transfer Reversal

DirectCharge **不執行 Transfer Reversal**（此步驟為 DestinationCharge 專用）。

---

### Step 4：正式退款

```
POST /v1/refunds
Stripe-Account: {subAcct}   ← DirectCharge 必帶
Authorization: Bearer {privateKey}

{
  "charge": "{charge.id}",
  "amount": "{AmountConvert(request.Amount)}",
  "refund_application_fee": "false",
  "metadata[request_id]": "{request.RequestId}"
}
```

---

## 資金流向示意

```
付款時：
  持卡人 → [Stripe] → 子帳號（Connected Account）
             └→ Platform 收取 Application Fee

退款時：
  子帳號（Connected Account）→ [Stripe] → 持卡人   （Step 4: POST /v1/refunds + Stripe-Account）
  Platform → [Stripe] → 持卡人（或減少 Application Fee）  （Step 2: POST /v1/application_fees/{id}/refunds）
```

---

## 情境範例

### 一般退款（不退手續費）

```json
// Request ExtendInfo
{
  "payment_flow": "DirectCharge",
  "stripe_account": "acct_xxx",
  "is_refund_application_fee": false,
  "application_fee_amount": 0
}
```

執行步驟：Step 1 → Step 4（共 2 個 API 呼叫）

---

### 退款同時退手續費

```json
// Request ExtendInfo
{
  "payment_flow": "DirectCharge",
  "stripe_account": "acct_xxx",
  "is_refund_application_fee": true,
  "application_fee_amount": 15.00
}
```

執行步驟：Step 1 → Step 2 → Step 4（共 3 個 API 呼叫）
