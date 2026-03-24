


## 付款失敗情境


https://91appinc.visualstudio.com/G11n/_workitems/edit/535370


## 文案

### 預設 Razer catch 失敗文案


backend.service.payment_middleware

```csharp
/// <summary>
/// 信用卡一次付清目前無法使用，請嘗試其他付款
/// </summary>
public static string RequestPaymentErrorCreditCardOnceRazer { get { return GetString("RequestPaymentError_CreditCardOnce_Razer"); }}
```


### paymenymiddleware 包成失敗的文案

backend.v2.pay_channel

```csharp
/// <summary>
/// 付款失敗，請回購物車重新結帳
/// </summary>
public static string ErrorFail { get { return GetString("error_fail"); }}
```



## Razer 最低金額不符預期， API 規格以及版本對不起來，導致最低金額有問題


## 最低金額 / 支援卡別 hardcode 後續維護需要注意 
