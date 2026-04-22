# ApplePay / GooglePay — PMW 查詢流程（Query）

## QueryPayment 的特殊行為（Mobile Wallet）

`GetThirdPartyQueryPaymentDetail()` 在 `payMethod` 為行動錢包時，  
會額外帶回卡片資訊（信用卡查詢則不帶）：

```csharp
if (this._mobileWalletMethods.Contains(payMethod))
{
    extendInfo.Add("card_brand",     charge.payment_method_details.card.brand);
    extendInfo.Add("card_country",   charge.payment_method_details.card.country);
    extendInfo.Add("card_exp_month", charge.payment_method_details.card.exp_month);
    extendInfo.Add("card_exp_year",  charge.payment_method_details.card.exp_year);
    extendInfo.Add("card_last4",     charge.payment_method_details.card.last4);
}
```

## Pay vs Query 回傳欄位差異

| 欄位 | Pay 回傳 | Query 回傳 |
|------|---------|-----------|
| `card_brand` | ✅ | ✅ |
| `card_country` | ✅ | ✅ |
| `card_exp_month` | ❌ | **✅**（Query 額外補充） |
| `card_exp_year` | ❌ | **✅**（Query 額外補充） |
| `card_last4` | ❌ | **✅**（Query 額外補充） |

> Query 回傳比 Pay 回傳多了 `card_exp_month`、`card_exp_year`、`card_last4`。

## 其他流程

QueryPayment 本體流程（`GET /v1/payment_intents/{id}`）與信用卡完全相同，  
差異僅在 ExtendInfo 組建時額外附加卡片資訊。
