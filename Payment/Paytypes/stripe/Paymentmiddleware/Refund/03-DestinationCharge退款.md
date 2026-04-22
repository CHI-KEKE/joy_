# Stripe Refund — DestinationCharge 退款

## 適用條件

`request.ExtendInfo.StripePaymentFlow == StripePaymentFlowEnum.DestinationCharge`

---

## DestinationCharge 模式簡介

> DestinationCharge：付款時在**主帳號**下建立 PaymentIntent，資金收取後透過 `transfer_data[destination]` 轉給子帳號。  
> 退款時，主帳號執行退款，子帳號的 Transfer 也需要同步撤銷（Transfer Reversal）。

---

## 完整退款 API 序列

### Step 1：取得 PaymentIntent（**不帶**子帳號 header）

```
GET /v1/payment_intents/{transactionId}
Authorization: Bearer {privateKey}
（無 Stripe-Account header）
```

回應中取出：
```
paymentIntent.charges.data[0].id            → charge.id（退款用）
paymentIntent.charges.data[0].ApplicationFee → fee.id（退手續費用）
paymentIntent.charges.data[0].Transfer       → transfer.id（撤銷轉移用）
```

---

### Step 2（條件）：退還 Application Fee

與 DirectCharge 相同，僅在 `IsRefundApplicationFee=true` 且 `ApplicationFeeAmount > 0` 時執行：

```
POST /v1/application_fees/{charge.ApplicationFee}/refunds
Authorization: Bearer {privateKey}

{
  "amount": "{AmountConvert(ApplicationFeeAmount)}"
}
```

---

### Step 3（DestinationCharge 必執行）：Transfer Reversal

```
POST /v1/transfers/{charge.Transfer}/reversals
Authorization: Bearer {privateKey}
（無 Stripe-Account header — Transfer 在主帳號下管理）

{
  "amount": "{AmountConvert(request.Amount)}"
}
```

> **目的**：付款時主帳號透過 Transfer 將資金移至子帳號，退款時需同步撤銷這筆轉移，  
> 否則子帳號資金不會回到主帳號，造成資金不平衡。

---

### Step 4：正式退款

```
POST /v1/refunds
Authorization: Bearer {privateKey}
（無 Stripe-Account header — 主帳號退款）

{
  "charge": "{charge.id}",
  "amount": "{AmountConvert(request.Amount)}",
  "refund_application_fee": "false",
  "metadata[request_id]": "{request.RequestId}"
}
```

---

## 與 DirectCharge 的關鍵差異

| 項目 | DirectCharge | DestinationCharge |
|------|-------------|------------------|
| Step 1 subAcct header | ✅ 帶（子帳號） | ❌ 不帶（主帳號） |
| Step 3 Transfer Reversal | ❌ 不執行 | ✅ 必執行 |
| Step 4 subAcct header | ✅ 帶（子帳號） | ❌ 不帶（主帳號） |
| 退款執行帳號 | 子帳號 | 主帳號 |

---

## 資金流向示意

```
付款時：
  持卡人 → [Stripe 主帳號] → Transfer → 子帳號
                └→ Platform 收取 Application Fee

退款時：
  子帳號 ← [Transfer Reversal] ← 主帳號       （Step 3: POST /v1/transfers/{id}/reversals）
  主帳號 → [Stripe] → 持卡人                  （Step 4: POST /v1/refunds，無 subAcct）
  Platform → [Stripe] → 持卡人（退手續費時）   （Step 2: POST /v1/application_fees/{id}/refunds）
```

---

## 情境範例

### 一般退款（不退手續費）

```json
// Request ExtendInfo
{
  "payment_flow": "DestinationCharge",
  "stripe_account": "acct_xxx",
  "is_refund_application_fee": false,
  "application_fee_amount": 0
}
```

執行步驟：Step 1 → Step 3 → Step 4（共 3 個 API 呼叫）

---

### 退款同時退手續費

```json
// Request ExtendInfo
{
  "payment_flow": "DestinationCharge",
  "stripe_account": "acct_xxx",
  "is_refund_application_fee": true,
  "application_fee_amount": 15.00
}
```

執行步驟：Step 1 → Step 2 → Step 3 → Step 4（共 4 個 API 呼叫，最多情況）
