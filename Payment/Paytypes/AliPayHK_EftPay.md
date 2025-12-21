# AliPayHK EftPay 文件

## 目錄
1. [Query](#1-query)
2. [退款](#2-退款)

<br>

---

## 1. Query

**API:**

<br>

```
POST {{url}}/api/{{version}}/QueryPayment/{{payType}}
x-secret: {{secret}}
x-user-key: {{userKey}}
Content-Type: application/json; charset=UTF-8
```

<br>

**Request Body:**

<br>

```json
{  
   "country":"HK",
   "request_id": "{{$guid}}",
   "extend_info": {
      "transaction_id": "597700025000000128u08cbeq303",
      "tg_code": "TG250612Y00012",
      "time": "20250613103100",
      "endpoint": "BankOfChina"
   }
}
```

<br>

時間要打現在否則會出現以下訊息

<br>

```
QueryPayment Response Sign Verify Fail! 12, 时间匹对异常，请检查客户端时间，或通知运营
```

<br>

---

## 2. 退款

### 直接打的 Request Body

<br>

```json
{
    "querytype": "OUT_TRADE",
    "eft_trade_no": "597700025000000128u08cbeq303",
    "out_trade_no": "TG250612Y00012",
    "user_confirm_key": "S0001356",
    "time": "20250612221051",
    "sign": "9447ef8c783d3a892a8f11ca6303beb7b211e76b021303b53ce5393c2015bb62"
}
```

<br>

### PaymentMiddleware Request Body

<br>

```json
{
    "request_id": "22aafad4-96e2-40e2-8da3-9d8a781ffe4e",
    "transaction_id": "597700025000000128u08cbeq303",
    "country": "HK",
    "extend_info": {
      "query_string": "?shopId=17&k=9a11cc3e-f92f-414c-becd-56b49a4c7b68&lang=zh-HK",
      "tg_code": "TG250612Y00012",
      "transaction_id": "597700025000000128u08cbeq303",
      "time": "20250612221051",
      "endpoint": "BankOfChina"
    }
}
```

<br>

### 成功退款 Response

<br>

```json
{
  "request_id": "94f471e4-31fe-4fbf-af8e-57229da121b4",
  "transaction_id": "597700025000000128u08cbeq303",
  "return_code": "0000",
  "return_message": "00,TRADE_REFUND",
  "extend_info": {
    "raw_data": {
      "user_confirm_key": "S0001356",
      "return_amount": "100",
      "wallet": "ALIPAYHK",
      "return_char": "",
      "out_refund_no": "TS250612Y000021_157212",
      "sign": "f45d45ee71f3d8f23543a67d094f2491cd519be895bae7e21ca1a1df74834c65",
      "fee_type": "HKD",
      "eft_trade_no": "597700025000000128u08cbeq303",
      "out_trade_no": "TG250612Y00012",
      "payType": "Alipay",
      "refund_time": "20250613103238",
      "trade_status": "TRADE_REFUND",
      "total_fee": "540.2",
      "return_status": "00",
      "buyerType": "others",
      "refund_trade_no": null,
      "time": "20250613103238"
    }
  }
}
```

<br>