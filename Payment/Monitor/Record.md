

## 付款失敗，請回購物車重新結帳

- 這通常是打到 PaymentMiddleware 回覆失敗得情境

`ThirdPartyPayApiProcessor.cs:line 99`

1. 查 Error

```bash
{service="my-qa-payment-middleware"}
|json
|= `Error`
|json
| line_format "{{._msg}}"
```

點開 RequestId, RequestPath 可以看到 TG / MG