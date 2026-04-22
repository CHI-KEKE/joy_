# Stripe Cancel — 取消付款流程

## 概覽

Cancel 用於取消尚未完成的 PaymentIntent（例如付款逾時、用戶放棄付款）。

```
PMW → POST /v1/payment_intents/{id}/cancel
       ↓
   Stripe 回傳 status: "canceled"
       ↓
   比對 status → 回傳 ReturnCode
```

---

## API 呼叫

```
POST /v1/payment_intents/{transactionId}/cancel
Stripe-Account: {subAcct}   ← 僅 DirectCharge 帶此 header
Authorization: Bearer {privateKey}
```

| 欄位 | 來源 |
|------|------|
| `transactionId`（URL） | `request.TransactionId`（付款時的 PaymentIntent ID） |
| `Stripe-Account` | `request.ExtendInfo.SubAccount`（DirectCharge 才帶） |

---

## PaymentFlow 差異

| PaymentFlow | Stripe-Account Header |
|------------|----------------------|
| `DirectCharge` | ✅ 帶 `subAcct` |
| `DestinationCharge` | ❌ 不帶 |

```csharp
string subAcct = request.ExtendInfo.StripePaymentFlow == StripePaymentFlowEnum.DirectCharge
    ? request.ExtendInfo.SubAccount
    : null;
```

---

## 回應與 ReturnCode 判斷

```csharp
string returnCode = cancelPaymentResponseEntity.status == StripeStatusConstants.Cancelled
    ? ReturnCodes.PaymentCancellationSuccess
    : ReturnCodes.PaymentCancellationFailed;
```

| 回應 `status` | ReturnCode | 說明 |
|--------------|-----------|------|
| `"canceled"` | PaymentCancellationSuccess | 取消成功 |
| 其他值 | PaymentCancellationFailed | 取消結果異常 |
| `ApiException` | PaymentCancellationFailed | Stripe API 回傳錯誤（含 error message） |
| 其他未知例外 | **re-throw** | LogError 後往上拋 |

---

## CancelPaymentRequestExtendInfo 欄位

```csharp
// 繼承自 BaseRequestExtendInfo，無額外欄位
public class CancelPaymentRequestEntity : BaseRequestExtendInfo
{
    // StripePaymentFlow（DirectCharge / DestinationCharge）
    // SubAccount（子帳號 ID）
}
```

---

## 回應 ExtendInfo

```json
{
  "raw_data": { /* StripeCancelPaymentResponseEntity 完整原始回應 */ }
}
```

ReturnMessage 直接帶入 `cancelPaymentResponseEntity.status`（如 `"canceled"`）。

---

## 可被 Cancel 的 PaymentIntent 狀態

> Stripe 只允許取消特定狀態的 PaymentIntent：

| PaymentIntent Status | 可取消？ |
|---------------------|---------|
| `requires_payment_method` | ✅ |
| `requires_confirmation` | ✅ |
| `requires_action` | ✅（3D 驗證進行中） |
| `processing` | ✅ |
| `succeeded` | ❌ 不可取消（需改用 Refund） |
| `canceled` | ❌ 已取消 |
