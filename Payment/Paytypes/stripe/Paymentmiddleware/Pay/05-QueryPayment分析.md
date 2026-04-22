# Stripe 信用卡付款 - QueryPayment 完整分析

## 概覽

QueryPayment 是用來查詢一筆 PaymentIntent 的最終付款狀態，主要用於：
1. **3D 驗證完成後**，回來確認付款結果
2. **非同步通知補查**，確認付款是否真正成功
3. **主動輪詢**，在付款後查詢目前狀態

---

## 入口 API

```
POST /api/v1/QueryPayment/{payMethod}_{payChannel}
例如: POST /api/v1/QueryPayment/CreditCardOnce_Stripe
```

---

## Request 結構

### HTTP Body（`QueryPaymentRequestEntity`）

| 欄位 | 型別 | 說明 |
|------|------|------|
| `request_id` | string | 請求唯一識別碼 |
| `transaction_id` | string | **PaymentIntent ID**（`pi_xxx`），查詢的主鍵 |
| `country` | string | 國家代碼 |
| `extend_info` | object | 延伸資訊（見下方） |

### ExtendInfo（`QueryPaymentRequestExtendInfo`）

| 欄位 | JSON Key | 必填 | 說明 |
|------|----------|------|------|
| `payment_flow` | `payment_flow` | ✅ | `DirectCharge` 或 `DestinationCharge` |
| `stripe_account` | `stripe_account` | ✅ | Stripe 子帳號 ID |
| `query_string` | `query_string` | ❌ | ⚠️ **目前未被使用**（dead field） |

---

## 內部流程

```
StripePlugin.QueryPayment()
        │
        ├─ StripePaymentFlow == DirectCharge？
        │       ├─ YES → subAcct = request.ExtendInfo.SubAccount
        │       └─ NO  → subAcct = null
        │
        ▼
GET /v1/payment_intents/{transaction_id}
        │
        ├─ DirectCharge：Header 帶 Stripe-Account: {sub_account}
        └─ DestinationCharge：不帶 Stripe-Account Header
        │
        ▼
GetThirdPartyQueryPaymentDetail(response, payMethod)
        │
        ▼
依 PaymentIntent.status 決定 ReturnCode（見下方）
```

---

## Stripe API 呼叫

### `GET /v1/payment_intents/{id}`

| 項目 | 內容 |
|------|------|
| HTTP Method | GET |
| Path | `/v1/payment_intents/{transaction_id}` |
| Header（DirectCharge） | `Stripe-Account: {sub_account}` |
| Header（DestinationCharge） | 無（使用主帳號） |

---

## 回應狀態判斷邏輯

`GetThirdPartyQueryPaymentDetail()` 依 `PaymentIntent.status` 決定回傳值：

### ✅ status = `succeeded`（付款成功）

**ReturnCode：`0000`**

ExtendInfo 內容（信用卡）：

| 欄位 | 來源 |
|------|------|
| `payment_intent_id` | `PaymentIntentResponseEntity.id` |
| `charge_id` | `charges.data[0].id` |

ExtendInfo 額外內容（Mobile Wallet：GooglePay / ApplePay）：

| 欄位 | 來源 |
|------|------|
| `card_brand` | `charges.data[0].payment_method_details.card.brand` |
| `card_country` | `charges.data[0].payment_method_details.card.country` |
| `card_exp_month` | `charges.data[0].payment_method_details.card.exp_month` |
| `card_exp_year` | `charges.data[0].payment_method_details.card.exp_year` |
| `card_last4` | `charges.data[0].payment_method_details.card.last4` |

---

### ❌ status = `requires_payment_method` + `last_payment_error != null`（付款被拒）

**ReturnCode：`3000`**

| 欄位 | 來源 |
|------|------|
| `status` | `requires_payment_method` |
| `last_payment_error_code` | `last_payment_error.code` |
| `last_payment_error_decline_code` | `last_payment_error.decline_code` |
| `last_payment_error_message` | `last_payment_error.message` |
| `last_payment_error_type` | `last_payment_error.type` |

> ReturnMessage = `last_payment_error.message`（Stripe 原始錯誤文字）

---

### ⏳ status = `requires_action` / `requires_payment_method`（無錯誤）/ `requires_confirmation`（等待中）

**ReturnCode：`2003`**

- ExtendInfo：`null`
- ReturnMessage：原始 Stripe status 字串

| status | 意義 |
|--------|------|
| `requires_action` | 仍在等待 3D 驗證 |
| `requires_payment_method` | 等待用戶提供付款方式（無錯誤） |
| `requires_confirmation` | 等待確認 |

---

### ⚠️ 其他 status（未處理狀態）

**ReturnCode：`9001`（UnhandledException）**

- 觸發 `logger.LogWarning` 記錄完整 PaymentIntentResponseEntity
- ReturnMessage：原始 Stripe status 字串
- 包含 `canceled`、`processing` 等未定義情況

---

### 🔴 Stripe API 呼叫失敗（ApiException）

**ReturnCode：`2003`（WaitingToPay）**

> ⚠️ 注意：QueryPayment 遇到 Stripe API 錯誤時，回傳的是 `2003` 而**不是** `3000`。  
> 設計意圖是「查詢失敗不等於付款失敗，請稍後重試」。

- ReturnMessage：`"status code: {http_status}, message: {error.message}"`
- TransactionId：空字串

---

### 🔴 其他未預期例外（Exception）

- `logger.LogError` 記錄後直接 `throw`
- 回傳 HTTP 500

---

## 完整狀態對照表

| PaymentIntent status | 附加條件 | ReturnCode | ReturnMessage | ExtendInfo |
|---------------------|---------|-----------|--------------|-----------|
| `succeeded` | 無 | `0000` | `"succeeded"` | `payment_intent_id`、`charge_id`（+卡片資訊 for Mobile Wallet） |
| `requires_payment_method` | 有 `last_payment_error` | `3000` | Stripe 錯誤訊息 | 包含 `decline_code`、錯誤詳情 |
| `requires_action` | 無 | `2003` | `"requires_action"` | `null` |
| `requires_payment_method` | 無錯誤 | `2003` | `"requires_payment_method"` | `null` |
| `requires_confirmation` | 無 | `2003` | `"requires_confirmation"` | `null` |
| `canceled`、其他 | 無 | `9001` | 原始 status | `null` |
| Stripe API 失敗（ApiException） | 無 | `2003` | `"status code: xxx, message: ..."` | `null` |
| 其他例外 | 無 | HTTP 500 | — | — |

---

## 兩種 PaymentFlow 差異

| 比較項目 | DirectCharge | DestinationCharge |
|---------|-------------|-------------------|
| `Stripe-Account` Header | ✅ 帶入子帳號 | ❌ 不帶 |
| 查詢對象 | 子帳號下的 PaymentIntent | 主帳號下的 PaymentIntent |
| subAcct 判斷 | `request.ExtendInfo.SubAccount` | `null` |

---

## Request / Response 範例

### Request

```json
POST /api/v1/QueryPayment/CreditCardOnce_Stripe

{
  "request_id": "req-abc-123",
  "transaction_id": "pi_3PxxxxxxxxxxxxxxxxxxxxXX",
  "extend_info": {
    "payment_flow": "DirectCharge",
    "stripe_account": "acct_xxxxxxxxxx"
  }
}
```

### Response（成功）

```json
{
  "request_id": "req-abc-123",
  "return_code": "0000",
  "return_message": "succeeded",
  "transaction_id": "pi_3PxxxxxxxxxxxxxxxxxxxxXX",
  "extend_info": {
    "payment_intent_id": "pi_3PxxxxxxxxxxxxxxxxxxxxXX",
    "charge_id": "ch_3PxxxxxxxxxxxxxxxxxxxxXX"
  }
}
```

### Response（付款被拒）

```json
{
  "request_id": "req-abc-123",
  "return_code": "3000",
  "return_message": "Your card was declined.",
  "transaction_id": "pi_3PxxxxxxxxxxxxxxxxxxxxXX",
  "extend_info": {
    "status": "requires_payment_method",
    "last_payment_error_code": "card_declined",
    "last_payment_error_decline_code": "insufficient_funds",
    "last_payment_error_message": "Your card has insufficient funds.",
    "last_payment_error_type": "card_error"
  }
}
```

### Response（等待 3D 驗證）

```json
{
  "request_id": "req-abc-123",
  "return_code": "2003",
  "return_message": "requires_action",
  "transaction_id": "pi_3PxxxxxxxxxxxxxxxxxxxxXX",
  "extend_info": null
}
```

---

## 注意事項

1. **`transaction_id` 必須為 PaymentIntent ID**（`pi_xxx` 格式），不能是 Charge ID（`ch_xxx`）。  
   雖然 Refund 流程相容兩者，但 QueryPayment 只用 `GetPaymentIntentAsync`。

2. **`query_string` 欄位目前未使用**，傳入也不影響查詢結果。

3. **ApiException 回傳 `2003` 而非 `3000`**，設計上代表「查詢暫時失敗，非付款失敗」，呼叫方需自行決定是否重試。

4. **Mobile Wallet（GooglePay/ApplePay）** 成功時，ExtendInfo 會額外帶回卡片資訊（brand、last4 等），信用卡則不帶。
