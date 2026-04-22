# Stripe 信用卡 QueryPayment — 概覽與流程

> 本文件專注於 **Stripe 信用卡（CreditCard）** 的 QueryPayment 分析，不包含 Google Pay / Apple Pay。

---

## 用途

| 使用時機 | 說明 |
|---------|------|
| 3D 驗證完成後 | Pay 回傳 `2003`，用戶完成 3D 後需呼叫此 API 確認最終結果 |
| 非同步補查 | 付款後主動確認 PaymentIntent 是否真正成功 |
| 輪詢查詢 | 持續確認等待中的付款狀態 |

---

## 入口 API

```
POST /api/v1/QueryPayment/{payMethod}_{payChannel}
```

**Stripe 信用卡範例：**
```
POST /api/v1/QueryPayment/CreditCardOnce_Stripe
```

---

## Request 結構

```json
{
  "request_id": "唯一請求識別碼",
  "transaction_id": "pi_xxxxxxxxxxxxxxxxxxxxxxxx",
  "extend_info": {
    "payment_flow": "DirectCharge | DestinationCharge",
    "stripe_account": "acct_xxxxxxxxxx"
  }
}
```

### 欄位說明

| 欄位 | 必填 | 說明 |
|------|------|------|
| `request_id` | ✅ | 本次查詢的唯一識別碼 |
| `transaction_id` | ✅ | **Pay 時回傳的 PaymentIntent ID**（`pi_xxx` 格式） |
| `extend_info.payment_flow` | ✅ | `DirectCharge` 或 `DestinationCharge`，影響 Header 帶法 |
| `extend_info.stripe_account` | ✅ | Stripe 子帳號 ID（DirectCharge 查詢時帶入 Header） |
| `extend_info.query_string` | ❌ | **目前程式碼未使用**，傳入無任何效果 |

---

## 內部執行流程

```
QueryPayment(request)
        │
        ├─ payment_flow == DirectCharge？
        │       ├─ YES → subAcct = ExtendInfo.SubAccount
        │       └─ NO  → subAcct = null
        │
        ▼
[Stripe API] GET /v1/payment_intents/{transaction_id}
   DirectCharge      → Header: Stripe-Account: {sub_account}
   DestinationCharge → Header: 無（使用 Platform 主帳號查詢）
        │
        ├─ 成功 → GetThirdPartyQueryPaymentDetail(response)
        │              依 status 判斷，回傳 ReturnCode + ExtendInfo
        │
        ├─ ApiException（Stripe 回 4xx/5xx）
        │              ReturnCode = 2003（WaitingToPay）
        │
        └─ Exception（其他未知錯誤）
                       logger.LogError → throw → HTTP 500
```

---

## Stripe API 呼叫

### `GET /v1/payment_intents/{id}`

| 項目 | DirectCharge | DestinationCharge |
|------|-------------|-------------------|
| Header | `Stripe-Account: {sub_account}` | 無 |
| 查詢範圍 | 子帳號下的 PaymentIntent | 主帳號下的 PaymentIntent |

---

## Response 結構

```json
{
  "request_id": "唯一請求識別碼",
  "return_code": "0000",
  "return_message": "succeeded",
  "transaction_id": "pi_xxxxxxxxxxxxxxxxxxxxxxxx",
  "extend_info": {
    "payment_intent_id": "pi_xxxxxxxxxxxxxxxxxxxxxxxx",
    "charge_id": "ch_xxxxxxxxxxxxxxxxxxxxxxxx"
  }
}
```

---

## Status 判斷邏輯對照

| PaymentIntent status | 附加條件 | ReturnCode | ReturnMessage | ExtendInfo |
|---------------------|---------|-----------|--------------|-----------|
| `succeeded` | — | `0000` | `"succeeded"` | `payment_intent_id` + `charge_id` |
| `requires_payment_method` | 有 `last_payment_error` | `3000` | Stripe 錯誤文字 | 錯誤詳情（含 `decline_code`） |
| `requires_action` | — | `2003` | `"requires_action"` | `null` |
| `requires_payment_method` | 無 `last_payment_error` | `2003` | `"requires_payment_method"` | `null` |
| `requires_confirmation` | — | `2003` | `"requires_confirmation"` | `null` |
| `canceled` / 其他 | — | `9001` | 原始 status 字串 | `null` |
| Stripe 回傳 4xx/5xx | ApiException | `2003` | `"status code: {n}, message: ..."` | `null` |
| 未知例外 | Exception | HTTP 500 | — | — |

> ⚠️ **設計注意**：ApiException 回傳 `2003` 而非 `3000`，代表「查詢失敗 ≠ 付款失敗」。  
> 上游系統遇到 `2003` 應判斷是否需要重試，不可直接認定為付款失敗。
