## 耗時分析

```bash
{service="prod-payment-middleware"} 
|~ `` 
| json 
| _props_ElapsedMilliseconds > 10000
| line_format "{{._props_RequestPath}}, spend {{._props_ElapsedMilliseconds}}"
```


## 針對 requestPath

```bash
{service="prod-payment-middleware"} 
|~ `` 
| json 
| _props_RequestPath =`/api/v1.0/QueryPayment/CreditCardOnce_Cybersource`
| _props_ElapsedMilliseconds > 0
| line_format "{{._props_RequestPath}}, spend {{._props_ElapsedMilliseconds}}"
```


## requestpath

`/api/v1.0/pay/AliPayHK_EftPay/TG251106J00019`
`/api/v1.0/QueryPayment/CreditCardOnce_Cybersource`
`/api/v1.0/pay/CreditCardOnce_Razer`
`/api/v1.0/pay/CreditCardInstallment_Razer`


## 查 query

```bash
{service="prod-payment-middleware"}
|json
|= `MG251231V00005`
|json
| line_format "{{._msg}}"
|_props_RequestPath = `/api/v1.0/QueryPayment/GrabPay_Razer`
#/api/v1.0/pay/GrabPay_Razer/MG251231V00005
```

## 查 pay (TG)

```bash
{service="prod-payment-middleware"}
|json
|= `MG251231V00005`
|json
| line_format "{{._msg}}"
|_props_RequestPath = `/api/v1.0/pay/GrabPay_Razer/MG251231V00005`

```


## 單純 Keyword

```bash
{service="prod-payment-middleware"}
|json
|= `Error`
|json
| line_format "{{._msg}}"
```

## by status code


{service="prod-payment-middleware"}
|json
|= ` - 50`
|json
| line_format "{{._msg}}"


## Razer 要看 pay response 則

HTTP Response - Status: OK, Body