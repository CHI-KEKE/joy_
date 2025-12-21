# OnlineBanking 文件

## 目錄
1. [Pay](#1-pay)
2. [QueryPayment](#2-querypayment)

<br>

---

## 1. Pay

### PaymentMiddleware PayResponse

<br>

```json
{
  "request_id": "e3c2d7a5-39a5-4c22-9e7b-1484a3910c93",
  "return_code": "2003",
  "return_message": "WaitingToPay",
  "transaction_id": null,
  "tg_code": "MG250624J00008",
  "payment_action": {
    "action": "Redirect",
    "web_payment_url": "https://www.paydollar.com/b2c2/eng/dPayment/payComp.jsp",
    "app_payment_url": "https://www.paydollar.com/b2c2/eng/dPayment/payComp.jsp",
    "redirect_http_method": "POST",
    "post_data": {
      "merchantId": "85117277",
      "successUrl": "https://testmorningshop.91app.com.my/V2/PayChannel/OnlineBanking/AsiaPay/MG250624J00008?shopId=15&k=ae4306a6-3bf3-4927-8de4-ef521fab599c",
      "failUrl": "https://testmorningshop.91app.com.my/V2/PayChannel/OnlineBanking/AsiaPay/MG250624J00008?shopId=15&k=ae4306a6-3bf3-4927-8de4-ef521fab599c",
      "cancelUrl": "https://testmorningshop.91app.com.my/V2/PayChannel/OnlineBanking/AsiaPay/MG250624J00008?shopId=15&k=ae4306a6-3bf3-4927-8de4-ef521fab599c",
      "errorUrl": "https://testmorningshop.91app.com.my/V2/PayChannel/OnlineBanking/AsiaPay/MG250624J00008?shopId=15&k=ae4306a6-3bf3-4927-8de4-ef521fab599c",
      "secureHash": "68ae84de59177dc3e681d777b8bd439ac4879ae0",
      "orderRef": "MG250624J00008",
      "amount": "8.00",
      "currCode": "458",
      "lang": "C",
      "payType": "N",
      "pMethod": "MYCLEAR"
    }
  },
  "extend_info": null
}
```

<br>

---

## 2. QueryPayment

### API

/QueryPayment/OnlineBanking_AsiaPay

<br>

### AsiaPay Request

<br>

```json
{
  "orderRef": "MG250624J00008",
  "merchantId": "85117277",
  "loginId": "apiMelvita",
  "password": "LiveDec23",
  "actionType": "Query"
}
```

<br>

### PaymentMiddleware Request

<br>

```json
{
  "request_id": "98558327-7102-47b9-bf4c-a1605c869f14",
  "transaction_id": null,
  "country": "MY",
  "extend_info": {
    "tg_code": "MG250624J00008"
  }
}
```

<br>

### PaymentMiddleware Response

<br>

```json
{
  "request_id": "bca72d1e-a027-4e13-862f-9cc453277388",
  "return_code": "2003",
  "return_message": "訂單不存在",
  "transaction_id": null,
  "extend_info": {
    "raw_data": null
  }
}
```

<br>