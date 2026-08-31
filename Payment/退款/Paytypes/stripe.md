# Stripe 退款流程（CreditCardOnce / GooglePay / ApplePay）

## 基本資訊

| 項目 | 值 |
|------|-----|
| PayChannel | `Stripe` |
| 適用 PayType | `CreditCardOnce_Stripe`、`GooglePay`、`ApplePay` |
| 退款路線 | `RefundByRequestId`（逐筆退款） |
| CanGroupingRefund | `false` |
| NMQ JobName | `CreditCardOnceStripeRefundRequestFinish` / `GooglePayStripeRefundRequestFinish` / `ApplePayStripeRefundRequestFinish` |

---

## GooglePay / ApplePay 特殊對應

```csharp
// GetPayTypeWithPayChannel
case nameof(PayProfileTypeDefEnum.GooglePay):
case nameof(PayProfileTypeDefEnum.ApplePay):
    return $"{payType}_Stripe";  // 打 API 時 payType 帶 "_Stripe" 後綴
```

GooglePay / ApplePay 雖然是獨立 PayType，但底層都走 **StripePayChannelService**，
打 PaymentMiddleware API 時 payType 改為 `GooglePay_Stripe` / `ApplePay_Stripe`。

---

## 退款判斷邏輯

### IsQueryRefund

未 override → 預設 `false`（**永遠不走 RefundQuery**）

### IsRefund

未 override → 預設 `true`（直接退款）

> Stripe 不做 RefundQuery，`GetRefundQueryExtendInfo()` 也回傳 `null`。

---

## 打 API 時傳入的額外資訊（ExtendInfo）

```csharp
{
    "payment_flow":           "DirectCharge" or paymentInfoEntity.PaymentFlow,
    "stripe_account":         subAccount,
    "application_fee_amount": decimal,   // 退手續費金額
    "is_refund_application_fee": bool    // 是否退手續費
}
```

### payment_flow 來源

```csharp
// 若 SalesOrderThirdPartyPayment_Info 為空
→ DirectCharge + ShopDefault 的 StripeSubAccount

// 若 Info 有值（JSON）
→ StripeSalesOrderThirdPartyPaymentInfoEntity.PaymentFlow + SubAccount
```

---

## 手續費退款邏輯（application_fee_amount）

Stripe 退款時需同步退還平台手續費（application_fee）。

### 一般情境（無補收單）

```
退手續費 = CalculateFee(退款金額, SCMSalesFeeRate)
         = Math.Round(refundAmount * feeRate, 2)
```

### 退貨情境（有補收單 RechargeReceipt）

```
退手續費 = TS手續費 - 補收手續費

TS手續費   = CalculateFee(退款金額 + 補收金額, feeRate)
補收手續費 = CalculateFee(補收金額, feeRate)
```

> 多付款方式時，補收金額取 `RechargeReceiptPayProfileType_RechargeAmount`；
> 單付款時 fallback 至 `RechargeReceipt_RechargeAmount`。

**驗證**：退款金額 + 補收金額 == `SalesOrderSlavePayProfileType_Amount`，不符合則 throw。

### is_refund_application_fee 條件

```csharp
// 非運費 → 一定退手續費
// 運費 → 2020/07/01 前不退手續費（除特定商店外）
return !isSalesOrderFee || 
       salesOrderSlaveDateTime >= new DateTime(2020, 7, 1) || 
       shopArray.Contains(shopId.ToString());
```

---

## HTTP Headers

從 `SalesOrderThirdPartyPayment_Info` 取得 AccountType，再取對應的 Stripe API Key：

```
X-Secret: {StripeApiKey}  // 從 IStripeApiKeyRetriever 取得，依商店+AccountType決定
```

---

## 退款延時

```csharp
public override void RefundDelay()
{
    // Stripe 不需要延時
}
```

---

## 重試機制

`GetRedoRefundRequestList()` → `null`（無 redo 機制）

---

## 關鍵檔案

| 檔案 | 路徑 | 說明 |
|------|------|------|
| `StripePayChannelService.cs` | `ERP/Backend/BLV2/PayChannel/` | 核心邏輯 |
| `IStripeApiKeyRetriever.cs` | `ERP/Backend/BLV2/Stripes/` | API Key 取得 |
| `PaymentMiddleWareRefundRequestService.cs` | `ERP/Backend/BLV2/ThirdPartyPayments/` | 退款流程 |