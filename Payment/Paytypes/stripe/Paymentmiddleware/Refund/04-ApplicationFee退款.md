# Stripe Refund — Application Fee 退款

## 什麼是 Application Fee？

Stripe Connect 架構中，平台（主帳號）在每筆付款時可從 Connected Account（子帳號）收取手續費，稱為 **Application Fee**。

```
付款金額 100 HKD
  └→ 子帳號收到 97 HKD
  └→ 平台主帳號收到 Application Fee 3 HKD
```

當訂單退款時，視情況決定是否連同 Application Fee 一起退還。

---

## 觸發條件

**兩個條件都必須成立**才會執行 Application Fee 退款：

```csharp
if (request.ExtendInfo.IsRefundApplicationFee == true &&
    request.ExtendInfo.ApplicationFeeAmount > 0)
{
    await this._stripeHttpClient.CreateApplicationFeeRefundAsync(charge.ApplicationFee, body);
}
```

| 欄位 | 說明 |
|------|------|
| `is_refund_application_fee` | 旗標，由上游系統決定此筆退款是否退還手續費 |
| `application_fee_amount` | 要退還的手續費金額（原始金額，會乘以 100 轉換） |

> **application_fee_amount = 0 時不退**：  
> 即使 `is_refund_application_fee = true`，若金額為 0 也不執行，避免呼叫 Stripe API 時帶入 0 金額導致錯誤。

---

## API 呼叫

```
POST /v1/application_fees/{applicationFeeId}/refunds
Authorization: Bearer {privateKey}
（無 Stripe-Account header）

{
  "amount": "{AmountConvert(ApplicationFeeAmount)}"
}
```

> `applicationFeeId` 來自 Step 1 取得的 `charge.ApplicationFee`。

---

## 注意事項

### 1. Application Fee Refund 不帶子帳號 header

Application Fee 是由平台主帳號收取，存放在主帳號餘額，退款時直接在主帳號執行，**不論 DirectCharge 或 DestinationCharge 都不帶 `Stripe-Account` header**。

### 2. `refund_application_fee` 在 POST /v1/refunds 固定為 false

在 Step 4 退款時，`refund_application_fee` 設定為 `"false"`，因為 Application Fee 的退款已在 Step 2 獨立精確控制金額：

```csharp
refundRequestBody["refund_application_fee"] = "false";
```

若設為 `true`，Stripe 會自動依比例退還 Application Fee，無法精確控制金額。

### 3. 部分退款的 Application Fee 計算

Application Fee 退款金額（`ApplicationFeeAmount`）由上游傳入，PMW 不自行計算。  
上游應依據退款比例自行計算要退多少手續費。

---

## 情境對照

| 情境 | `is_refund_application_fee` | `application_fee_amount` | 是否執行 Step 2 |
|------|---------------------------|------------------------|----------------|
| 全額退款含手續費 | `true` | `15.00` | ✅ 是 |
| 全額退款不退手續費 | `false` | `15.00` | ❌ 否 |
| 部分退款含手續費 | `true` | `7.50` | ✅ 是 |
| 手續費為零 | `true` | `0` | ❌ 否（金額 = 0 跳過） |
