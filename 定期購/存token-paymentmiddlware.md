

## paymentmiddleware

### CreditCardOnce_Razer 是否接受 Token 付款？

✅ **支援 Token 付款**

#### 付款入口：`RazerPlugin.Pay()`

```csharp
//// 有 CardToken 為定期購流程 走 Recurring API 付款
if (string.IsNullOrWhiteSpace(request.ExtendInfo.CardToken) == false)
{
    return await this.PayByRecurring(merchantId, secretKey, verifyKey, request);
}
else
{
    return await this.PayByDirect(merchantId, secretKey, verifyKey, request, payMethod);
}
```

#### 判斷邏輯

| 情境 | 路由 |
|------|------|
| `CardToken` 有值 | → `PayByRecurring()`（Razer Recurring API） |
| `CardToken` 為空 | → `PayByDirect()`（一般直付，需輸入卡號） |

#### 補充

- `CreatePaymentRequestRequestEntity` 建立請求時，固定設定 `mpstokenstatus = "1"`，表示**一般直付也會儲存 Token**，以備後續定期購使用
- Token 付款流程使用 **Razer Recurring API**，與一般刷卡流程完全分開
- `CardToken` 來自前次付款後 QueryPayment 回傳的 `CreditCardToken` 欄位

