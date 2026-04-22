


## shopping

cammilo@91app

/api/checkout/complete

ShopId: 99, Key: \"ApplePay\"


Response statusCode OK, response data : {"code":"ApplePay_Success","message":"第三方支付轉導","data":{"TransactionId":null,"ReturnUrl":null,"WebPaymentUrl":null,"AppPaymentUrl":null,"SessionId":null,"IsFirstPay":false,"RedirectHttpMethod":null,"PostData":null,"ExtendInfo":{"payment_method":"pm_1TEBACBNLlm5Q2WrNLTUw92U","clientSecret":"pi_3TEBAEBNLlm5Q2Wr1ArJCZrq_secret_DY3zx3bTKRxWDhDGg135ymIw0"}}


#### applypay 成功的 sample


"Payment\":{\"PayProfileTypeDef\":\"ApplePay\"


Successfully got paymentMethodId: pm_1TEAx9BNLlm5Q2Wrj6YtXYKv



https://monitoring-dashboard.91app.io/explore?schemaVersion=1&panes=%7B%22vm0%22:%7B%22datasource%22:%22RjRcuuN4k%22,%22queries%22:%5B%7B%22refId%22:%22A%22,%22expr%22:%22%7Bservice%3D%5C%22prod-shopping-service%5C%22%7D%5Cr%5Cn%7Cjson%5Cr%5Cn%23%20%7C%3D%20%60ApplePay%60%5Cr%5Cn%23%20%7C%3D%20%60ShopId:%2067%60%5Cr%5Cn%23%20%7C%3D%60cammilo@91app.com%60%5Cr%5Cn%7Cjson%5Cr%5Cn%7C_props_RequestPath%20%3D%20%60%2Fapi%2Fcheckout%2Fcomplete%60%5Cr%5Cn%7C_props_RequestId%3D%600HNK4GJL4PK17:00000162%60%5Cr%5Cn%7C%20line_format%20%5C%22%7B%7B._msg%7D%7D%5C%22%22,%22queryType%22:%22range%22,%22datasource%22:%7B%22type%22:%22loki%22,%22uid%22:%22RjRcuuN4k%22%7D,%22editorMode%22:%22code%22,%22direction%22:%22forward%22%7D%5D,%22range%22:%7B%22from%22:%221774281600000%22,%22to%22:%221774367999000%22%7D,%22panelsState%22:%7B%22logs%22:%7B%22columns%22:%7B%220%22:%22Time%22,%221%22:%22Line%22,%222%22:%22_props_RequestPath%22,%223%22:%22_props_RequestId%22%7D,%22visualisationType%22:%22table%22,%22labelFieldName%22:%22labels%22,%22refId%22:%22A%22%7D%7D%7D%7D&orgId=2



```bash
{service="prod-shopping-service"}
|json
# |= `ApplePay`
# |= `ShopId: 67`
# |=`cammilo@91app.com`
|json
|_props_RequestPath = `/api/checkout/complete`
|_props_RequestId=`0HNK4GJL4PK17:00000162`
| line_format "{{._msg}}"


{service="prod-shopping-service"}
|json
# |= `ApplePay`
|= `ShopId: 67`
# |=`cammilo@91app.com`
# |=`ApplePay_Success`
|=`pm_`
|json
|_props_RequestPath = `/api/checkout/complete`
| line_format "{{._msg}}"
```


## cart

/api/checkouts/complete



