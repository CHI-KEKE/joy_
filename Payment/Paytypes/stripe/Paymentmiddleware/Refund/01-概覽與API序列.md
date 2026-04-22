# Stripe Refund — 概覽與完整 API 序列

## 重要說明：Stripe 無 RefundQuery

Stripe Plugin **未實作 `IRefundQueryable`**。  
退款一旦呼叫 `POST /v1/refunds` 成功，Stripe 直接回傳最終結果（`succeeded`），**不需要像 Cybersource 一樣輪詢退款狀態**。

---

## 完整退款流程架構

Stripe 退款最多需要 **4 個 API 呼叫**，依 PaymentFlow 與 ApplicationFee 設定決定實際執行哪幾步：

```
Step 1 [必執行]
GET /v1/payment_intents/{id}
  └→ 取得 PaymentIntent，從中拿到 Charge（charge.id、charge.ApplicationFee、charge.Transfer）

Step 2 [條件執行：is_refund_application_fee=true AND application_fee_amount > 0]
POST /v1/application_fees/{applicationFeeId}/refunds
  └→ 退還平台手續費（Application Fee）

Step 3 [條件執行：payment_flow = DestinationCharge]
POST /v1/transfers/{transferId}/reversals
  └→ 撤銷資金轉移（Transfer Reversal）

Step 4 [必執行]
POST /v1/refunds
  └→ 對 Charge 發起實際退款
```

---

## API 呼叫彙總

| 步驟 | API | 說明 | 條件 |
|------|-----|------|------|
| 1 | `GET /v1/payment_intents/{id}` | 取得付款意圖，提取 Charge 資訊 | 必執行 |
| 2 | `POST /v1/application_fees/{fee_id}/refunds` | 退還 Application Fee | `IsRefundApplicationFee=true` 且 `ApplicationFeeAmount > 0` |
| 3 | `POST /v1/transfers/{transfer_id}/reversals` | 撤銷 Transfer | `StripePaymentFlow = DestinationCharge` |
| 4 | `POST /v1/refunds` | 正式退款至持卡人 | 必執行 |

---

## 金額轉換規則

```csharp
private string AmountConvert(decimal amount)
{
    return Math.Floor(amount * 100).ToString();
}
```

> Stripe API 使用最小貨幣單位（smallest currency unit）：  
> HKD 100.00 → Stripe API 傳入 `10000`  
> 退款金額（`request.Amount`）與手續費金額（`ApplicationFeeAmount`）都需經過此轉換。

---

## Step 4：退款 Request Body

```
POST /v1/refunds
Stripe-Account: {subAcct}   ← 僅 DirectCharge 帶此 header

{
  "charge": "{charge.id}",
  "amount": "{AmountConvert(request.Amount)}",
  "refund_application_fee": "false",
  "metadata[request_id]": "{request.RequestId}"
}
```

> `refund_application_fee` 固定為 `"false"`：Application Fee 退款已在 Step 2 獨立處理，這裡不讓 Stripe 自動退。

---

## ReturnCode 對照

| 情境 | ReturnCode | 說明 |
|------|-----------|------|
| 退款成功 | `1000` Success | Stripe `POST /v1/refunds` 正常回應 |
| `ApiException` | `4001` RefundFailed | Stripe API 回傳錯誤（如 insufficient funds） |
| `ArgumentNullException` | `4001` RefundFailed | Charge 資料異常（如 `.Single()` 找不到 Charge） |
| 其他未知例外 | **re-throw** | `LogError` 後直接往上拋，不吞例外 |

---

## RefundRequestExtendInfo 欄位

```csharp
// 繼承自 BaseRequestExtendInfo
public StripePaymentFlowEnum StripePaymentFlow { get; set; }  // DirectCharge / DestinationCharge
public string SubAccount { get; set; }                        // Stripe sub-account ID

// 退款專用欄位
public bool IsRefundApplicationFee { get; set; }              // 是否退還手續費
public decimal ApplicationFeeAmount { get; set; }             // 手續費金額（原始金額，非最小單位）
```

---

## 詳細說明文件

| 文件 | 內容 |
|------|------|
| [02-DirectCharge退款.md](./02-DirectCharge退款.md) | DirectCharge 模式的退款細節 |
| [03-DestinationCharge退款.md](./03-DestinationCharge退款.md) | DestinationCharge 模式（含 Transfer Reversal） |
| [04-ApplicationFee退款.md](./04-ApplicationFee退款.md) | Application Fee 退款條件與邏輯 |
