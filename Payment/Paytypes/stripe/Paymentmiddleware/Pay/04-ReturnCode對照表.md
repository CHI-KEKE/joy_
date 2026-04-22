# Stripe 信用卡付款 - ReturnCode 對照表

## ReturnCode 定義

| ReturnCode | 名稱 | 說明 |
|-----------|------|------|
| `0000` | Success | 付款成功 |
| `2003` | WaitingToPay | 待付款（等待 3D 驗證或用戶操作） |
| `3000` | Failed | 付款失敗（卡被拒、卡號錯誤、餘額不足等） |
| `4000` | RefundFailed | 退款失敗 |
| `5000` | PaymentCancellationSuccess | 付款請求取消成功 |
| `5001` | PaymentCancellationFailed | 付款請求取消失敗 |
| `9001` | UnhandledException | 未處理狀態（QueryPayment 查到未知的 Stripe status） |

---

## Pay 付款回應對照

### 觸發來源：`StripePlugin.Pay()` → `GetThirdPartyPayResponseEntity()`

| 情況 | 條件 | ReturnCode | ExtendInfo 內容 |
|------|------|-----------|----------------|
| 直接付款成功 | `status = succeeded` | `0000` | `payment_intent_id`、`charge_id`、`payment_method`、`customer_id` |
| 需要 3D 驗證（導頁） | `status = requires_action` + `next_action.type = redirect_to_url` | `2003` | 同上 + `Action.WebPaymentUrl` |
| 需要 3D 驗證（SDK） | `status = requires_action` + `next_action.type = use_stripe_sdk` | `2003` | 同上 + `Action.WebPaymentUrl` |
| Stripe 回傳 4xx/5xx | ApiException | `3000` | `ReturnMessage = Stripe error.message` |
| 其他未預期例外 | Exception | HTTP 500 | logger 記錄，直接 throw |

---

## QueryPayment 查詢回應對照

### 觸發來源：`StripePlugin.QueryPayment()` → `GetThirdPartyQueryPaymentDetail()`

| Stripe status | 附加條件 | ReturnCode | 說明 |
|--------------|---------|-----------|------|
| `succeeded` | 無 | `0000` | 付款成功，ExtendInfo 含 `payment_intent_id`、`charge_id` |
| `requires_payment_method` | 有 `last_payment_error` | `3000` | 付款被拒，ExtendInfo 含 `decline_code`、`last_payment_error_message` |
| `requires_action` | 無 | `2003` | 仍在等待 3D 驗證 |
| `requires_payment_method` | 無 `last_payment_error` | `2003` | 等待用戶輸入付款資訊 |
| `requires_confirmation` | 無 | `2003` | 等待確認 |
| 其他（canceled 等） | 無 | `9001` | 未處理狀態，logger.LogWarning 記錄 |
| Stripe 回傳 4xx/5xx | ApiException | `2003` + error message | 查詢失敗 |

---

## Cancel 取消回應對照

### 觸發來源：`StripePlugin.Cancel()`

| Stripe status | ReturnCode | 說明 |
|--------------|-----------|------|
| `canceled` | `5000` | 取消成功 |
| 其他 | `5001` | 取消失敗（PaymentIntent 狀態不允許取消） |
| ApiException | `5001` | Stripe 回傳錯誤 |

---

## 記住信用卡情境的特殊說明

`RememberPaymentMethodProcess` 中，PaymentIntent 付款後的綁卡判斷：

| PaymentIntent status | 綁卡動作 | 最終 ReturnCode |
|---------------------|---------|----------------|
| `succeeded` | 執行 attach 或 create customer | `0000` |
| `requires_action` | 執行 attach 或 create customer | `2003` |
| 其他狀態 | **不執行綁卡** | `0000`（帶原始 PaymentIntent） |

> ⚠️ 注意：當 status 為其他失敗狀態時，程式不綁卡並直接回傳 PaymentIntent，  
> 但最終 `GetThirdPartyPayResponseEntity` 仍會回傳 `0000`（因為沒有 `requires_action`）。  
> 上游系統需自行判斷 ExtendInfo 中的 PaymentIntent status 是否為真正的成功。

---

## 舊卡復用情境的特殊說明

`ReusePaymentMethodPaymentIntentProcess` 的驗證邏輯：

| 情況 | 行為 | 結果 |
|------|------|------|
| `customer_id` 在 CustomerSearch 結果中 | 正常執行 PaymentIntent | 依 status 回傳對應 ReturnCode |
| `customer_id` **不在** CustomerSearch 結果中 | `throw new NotSupportedException("Illegal Customer")` | HTTP 500 |
