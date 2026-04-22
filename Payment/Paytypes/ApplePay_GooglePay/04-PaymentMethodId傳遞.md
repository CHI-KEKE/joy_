# ApplePay / GooglePay — PaymentMethodId 傳遞

## 4.1 Shopping / Cart 新增節點用來接前端給的 PaymentMethodId

| 端 | 節點位置 |
|----|----------|
| **Shopping** | `PaymentMiddlewareCreditCardInfoEntity.ExtendInfo` |
| **Cart** | `TradesOrderThirdPartyPaymentInfoEntity.ExtendInfo` |

---

## 4.2 實現邏輯

1. **Checkout / Complete**  
   前端會在 `PaymentMiddlewareCreditCardInfoEntity.ExtendInfo` 帶上 `paymentMethodId`。

2. **Cart 處理**  
   在 `GetPayDataProcessor.AssignPayProcessFlowTypeAsync` 判斷是 GooglePay 後，從 ExtendInfo 去 Resolve 出 `paymentMethodId` ==> `payment_method`，放在 `checkoutContext.OldPayProcessContext.ThirdPartyPaymentInfo.ExtendInfo`。

3. **前台付款**  
   `GetMobileWalletPayExtendInfo` 取出後送到 PMW。
