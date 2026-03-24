##  Domain

**HK_QA** https://payment-middleware-api-internal.qa1.hk.91dev.tw
**HK_PROD** https://payment-middleware-api-internal.hk.91app.io

<br>

##  Path

- `Pay` /api/v1.0/pay/TwoCTwoP/TG250617Q00046
- `Query` /api/v1.0/QueryPayment/CreditCardOnce_Stripe
- `Refund` /api/v1.0/Refund/TwoCTwoP/TG240229K00030
- `RefundQuery` /api/v1.0/RefundQuery/TwoCTwoP
(有分退後 Query 以及 退前的 Query)

<br>

## Unhandled

如果是開發時在 paymentmiddleware 測試過程 unhandled 的情況就需要到 paymentmiddleware 看 Grafana log

<br>

## ErrorCode

- `4003` => Pending
- `3000` => Failed
