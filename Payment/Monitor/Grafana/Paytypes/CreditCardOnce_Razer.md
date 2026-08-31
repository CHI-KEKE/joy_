CreditCardOnce_Razer
CreditCardInstallment_Razer


`/api/v1.0/pay/CreditCardOnce_Razer`
`/api/v1.0/pay/CreditCardInstallment_Razer`




```bash
## 把 (.*?) 這段抓到的東西命名叫 ErrorDesc
## \\nProcessorResponseCode:代表抓到 \nProcessorResponseCode: 前就停止。注意這裡的 \\n 不是換行，而是你的 log 字串裡真的出現：\n
{service="prod-payment-middleware"}
|= `failed`
|=`ResponseBody`
| json
| _props_RequestPath = `/api/v1.0/QueryPayment/GrabPay_Razer`
 | regexp `ErrorDesc: (?P<ErrorDesc>[^\\]+)`
 | line_format `{{ .ErrorDesc }}`


{service="prod-payment-middleware"}
|= `failed`
|= `ResponseBody`
| json
| json
| _props_RequestPath = `/api/v1.0/QueryPayment/OnlineBanking_Razer`
| regexp `ErrorDesc: (?P<ErrorDesc>.*?)\\nProcessorResponseCode:`
| line_format `{{ .ErrorDesc }}`
```