# QFPay 文件

## 目錄
1. [異常紀錄](#1-異常紀錄)
2. [正常發動付款頁面的相關資訊](#2-正常發動付款頁面的相關資訊)

<br>

---

## 1. 異常紀錄

### 1.1 該筆訂單仍有正在處理中的退款請求，請勿重複提交申請。

#### 訊息

https://91app.slack.com/archives/C7T5CTALV/p1753837502573089

<br>

#### 確認訂單資訊

<br>

```sql
use ERPDB

select RefundRequest_CreatedDateTime,RefundRequest_TradesOrderGroupCode,RefundRequest_ResponseMsg,RefundRequest_TradesOrderSlaveCode,RefundRequest_TransactionId,*
from RefundRequest(nolock)
where RefundRequest_ValidFlag = 1
--and RefundRequest_StatusDef in ('RefundRequestFail','RefundRequestGrouping')
--and RefundRequest_UpdatedDateTime > '2025-07-29'
--and RefundRequest_ResponseMsg like '%You still have prosessing requests, please do not repeat%'
and RefundRequest_TypeDef = 'QFpay'
--and RefundRequest_TradesOrderSlaveCode = 'TS250729L00150C'
and RefundRequest_TradesOrderGroupCode = 'TG250729L00037'
--order by RefundRequest_DateTime desc
```

<br>

#### PaymentMiddleware 紀錄

<br>

搜尋 `/api/v1.0/Refund/QFPay/TG250729L00037`

<br>

TS250729L00150C-160027

<br>

- 確認 TS250729L00150C-160027 進行時間為 07/29 12:05 且只有執行一次
- 本 TG 3張子單皆有退款處理紀錄 前兩張成功 但後面兩張單的間隔很近

<br>

RefundRequest_StatusUpdatedDateTime:
- 2025-07-29 12:05:16.473
- 2025-07-29 12:05:19.880
- 2025-07-29 12:05:20.820

<br>

#### 打 Query 看是否有其他紀錄

<br>

看起來僅有一筆付款成功的紀錄, 使用 805812 為 Payme , order_type 為 payment, 時間為 2025-07-29 10:32:52

<br>

```json
{
  "request_id": "40b34742-81ca-4dd6-8217-0be3813f039c",
  "return_code": "0000",
  "return_message": "交易成功",
  "transaction_id": "20250729155400020063698239",
  "extend_info": {
    "page": 1,
    "resperr": "请求成功",
    "page_size": 100,
    "respcd": "0000",
    "data": [
      {
        "syssn": "20250729155400020063698239",
        "out_trade_no": "TG250729L00037",
        "chnlsn": "7bed3a81-4dc7-4966-a9e2-2292a0435700",
        "goods_name": "",
        "txcurrcd": "HKD",
        "origssn": null,
        "pay_type": "805812", 
        "order_type": "payment",
        "txdtm": "2025-07-29 10:32:41",
        "txamt": "41820",
        "sysdtm": "2025-07-29 10:32:41",
        "cancel": "5",
        "paydtm": "2025-07-29 10:32:52",
        "cardcd": "",
        "chnlsn2": "1f0e5090-1b5f-4b69-9e6f-502437d4bc35",
        "userid": "1000029318",
        "respcd": "0000",
        "goods_info": "",
        "errmsg": "交易成功",
        "clisn": "037961",
        "cardtp": "5"
      }
    ],
    "successCount": 1,
    "rawData": {
      "page": 1,
      "resperr": "请求成功",
      "page_size": 100,
      "respcd": "0000",
      "data": [
        {
          "syssn": "20250729155400020063698239",
          "out_trade_no": "TG250729L00037",
          "chnlsn": "7bed3a81-4dc7-4966-a9e2-2292a0435700",
          "goods_name": "",
          "txcurrcd": "HKD",
          "origssn": null,
          "pay_type": "805812",
          "order_type": "payment",
          "txdtm": "2025-07-29 10:32:41",
          "txamt": "41820",
          "sysdtm": "2025-07-29 10:32:41",
          "cancel": "5",
          "paydtm": "2025-07-29 10:32:52",
          "cardcd": "",
          "chnlsn2": "1f0e5090-1b5f-4b69-9e6f-502437d4bc35",
          "userid": "1000029318",
          "respcd": "0000",
          "goods_info": "",
          "errmsg": "交易成功",
          "clisn": "037961",
          "cardtp": "5"
        }
      ],
      "successCount": 0,
      "rawData": null
    }
  }
}
```

<br>

#### 1.5 結論

<br>

1. query 後沒有其他交易狀態僅有之前的付款成功紀錄
2. 根據 log 並無 TS250729L00150C-160027 重複發送 Refund 紀錄
3. 這張單有3張子單做退款前兩張子單正常成功 最後一張顯示訊息: "You still have prosessing requests, please do not repeat the initiation.\\n該筆訂單仍有正在處理中的退款請求，請勿重複提交申請。"

<br>

### 1.2 重複的退款申請

#### 1.2.1 訊息

76 out_trade_no is used - Do not use duplicate out_trade_no for request(1251)

<br>

#### 1.2.2 過程

QFPay 退款不可帶相同的單號，因此以 RefundRuestId 作為依據

<br>

RefundRequest_TransactionId 20250604155300020059948058

<br>

**Request Middleware**

<br>

```json
{
  "request_id": "3d698129-607e-4658-9cff-35b43bd528df",
  "transaction_id": "20240924155400020044843907",
  "amount": 0.44,
  "currency": "HKD",
  "extend_info": {
    "orderCode": "TS240924V000087-138965",
    "requestDatetime": "2025-06-18T10:02:33.4112997+08:00",
    "isUsingLiveTest": false
  }
}
```

<br>

**Response Sample**

<br>

```json
{
  "request_id": "3d698129-607e-4658-9cff-35b43bd528df",
  "transaction_id": null,
  "return_code": "4003",
  "return_message": " - 抱歉，當前時段無法處理此交易的退款請求，建議在 11AM 數據更新之後再重新嘗試。\nSorry, we are unable to process the refund request for this transaction at this moment. Please try again after the data update at 11 AM.",
  "extend_info": {
    "syssn": null,
    "orig_syssn": "20240924155400020044843907",
    "txamt": null,
    "sysdtm": null,
    "respcd": "1269",
    "resperr": "抱歉，當前時段無法處理此交易的退款請求，建議在 11AM 數據更新之後再重新嘗試。\nSorry, we are unable to process the refund request for this transaction at this moment. Please try again after the data update at 11 AM.",
    "paydtm": null,
    "txdtm": null,
    "udid": null,
    "txcurrcd": "HKD",
    "respmsg": "",
    "out_trade_no": "TS240924V000087-138965",
    "chnlsn": null,
    "cardcd": null
  }
}
```

<br>

**遇到的怪情況**

<br>

先是請稍後

<br>

```json
{
  "request_id": "922f34a3-e334-4321-a43f-13bda390bf55",
  "transaction_id": "20250604155300020059948058",
  "return_code": "4003",
  "return_message": "處理中，請稍等(1143)",
  "extend_info": {
    "page": 1,
    "resperr": "请求成功",
    "page_size": 10,
    "respcd": "0000",
    "data": [
      {
        "syssn": "20250604155300020059948058",
        "out_trade_no": "TS250515T000B0B-156684",
        "chnlsn": "",
        "goods_name": "",
        "txcurrcd": "HKD",
        "origssn": "20250515155400020058710384",
        "pay_type": "805814",
        "order_type": "refund",
        "txdtm": "2025-06-04 11:03:40",
        "txamt": "26910",
        "sysdtm": "2025-06-04 19:03:40",
        "cancel": "0",
        "paydtm": "2025-06-04 19:03:40",
        "cardcd": "",
        "chnlsn2": "",
        "userid": "1000029318",
        "respcd": "1143",
        "goods_info": "",
        "errmsg": "處理中，請稍等(1143)",
        "clisn": "068620",
        "cardtp": "5"
      }
    ],
    "successCount": 0,
    "rawData": {
      "page": 1,
      "resperr": "请求成功",
      "page_size": 10,
      "respcd": "0000",
      "data": [
        {
          "syssn": "20250604155300020059948058",
          "out_trade_no": "TS250515T000B0B-156684",
          "chnlsn": "",
          "goods_name": "",
          "txcurrcd": "HKD",
          "origssn": "20250515155400020058710384",
          "pay_type": "805814",
          "order_type": "refund",
          "txdtm": "2025-06-04 11:03:40",
          "txamt": "26910",
          "sysdtm": "2025-06-04 19:03:40",
          "cancel": "0",
          "paydtm": "2025-06-04 19:03:40",
          "cardcd": "",
          "chnlsn2": "",
          "userid": "1000029318",
          "respcd": "1143",
          "goods_info": "",
          "errmsg": "處理中，請稍等(1143)",
          "clisn": "068620",
          "cardtp": "5"
        }
      ],
      "successCount": 0,
      "rawData": null
    }
  }
}
```

<br>

再問一次說我重複問

<br>

```json
{
  "request_id": "ac010223-ea6f-484b-992c-f9bcfff7d69c",
  "transaction_id": null,
  "return_code": "3000",
  "return_message": "out_trade_no is used - Do not use duplicate out_trade_no for request(1251)",
  "extend_info": {
    "syssn": null,
    "orig_syssn": "20250515155400020058710384",
    "txamt": null,
    "sysdtm": null,
    "respcd": "1251",
    "resperr": "Do not use duplicate out_trade_no for request(1251)",
    "paydtm": null,
    "txdtm": null,
    "udid": null,
    "txcurrcd": "HKD",
    "respmsg": "out_trade_no is used",
    "out_trade_no": "TS250515T000B0B-156684",
    "chnlsn": null,
    "cardcd": null
  }
}
```

<br>

#### 1.2.3 釐清

QFPay payment 抱錯失敗變成處理中，結果清空 RefundRequest_TransactionId 重新觸發退款，再試一次會因為不能帶一模一樣的 request 因此阻擋，要請商家到後台退款

<br>

https://91appinc.visualstudio.com/DailyResource/_workitems/edit/499423

<br>

---

## 2. 正常發動付款頁面的相關資訊

API: `/api/v1.0/pay/QFPay/TG240618K00002`

<br>

**Request:**

<br>

```json
{
  "amount": 44.00,
  "country": "HK",
  "currency": "HKD",
  "tg_code": "TG240618K00001",
  "request_id": "54921957-b9e7-454d-a728-ca7b9b6ad944",
  "device": "PC",
  "platform": "Web",
  "extend_info": {
    "returnUrl": "https://shop2.shop.qa1.hk.91dev.tw/V2/PayChannel/Default/QFPay/TG240618K00001?shopId=2&k=c44bc1b7-45e5-4d13-9a0f-c7658b1ee6fd&lang=zh-HK"
  }
}
```

<br>

**Payload:**

<br>

```
appcode=51E1B3648E92428A8507BFE0918ED042&out_trade_no=TG240618K00001&paysource=91App_checkout&return_url=https://shop2.shop.qa1.hk.91dev.tw/V2/PayChannel/Default/QFPay/TG240618K00001?shopId=2&k=c44bc1b7-45e5-4d13-9a0f-c7658b1ee6fd&lang=zh-HK&sign_type=sha256&txamt=4400&txcurrcd=HKD&txdtm=0001-01-01 12:00:00
```

<br>

**Signature:**

<br>

```
888e5e6d66f0afb4b7b467ebb17673bef9fe6b299d60ed932672be18eeb5e90f
```

<br>

**Pay Page Url:**

<br>

```
https://openapi-int.qfapi.com/checkstand/#/?appcode=51E1B3648E92428A8507BFE0918ED042&out_trade_no=TG240618K00001&paysource=91App_checkout&return_url=https://shop2.shop.qa1.hk.91dev.tw/V2/PayChannel/Default/QFPay/TG240618K00001?shopId=2&k=c44bc1b7-45e5-4d13-9a0f-c7658b1ee6fd&lang=zh-HK&sign_type=sha256&txamt=4400&txcurrcd=HKD&txdtm=0001-01-01 12:00:00&sign=888e5e6d66f0afb4b7b467ebb17673bef9fe6b299d60ed932672be18eeb5e90f
```

<br>

**Response:**

<br>

```json
{
  "request_id": "54921957-b9e7-454d-a728-ca7b9b6ad944",
  "return_code": "2003",
  "return_message": "付款頁面成功產出",
  "transaction_id": "",
  "tg_code": "TG240618K00001",
  "payment_action": {
    "action": "Redirect",
    "web_payment_url": "https://openapi-int.qfapi.com/checkstand/#/?appcode=51E1B3648E92428A8507BFE0918ED042&out_trade_no=TG240618K00001&paysource=91App_checkout&return_url=https://shop2.shop.qa1.hk.91dev.tw/V2/PayChannel/Default/QFPay/TG240618K00001?shopId=2&k=c44bc1b7-45e5-4d13-9a0f-c7658b1ee6fd&lang=zh-HK&sign_type=sha256&txamt=4400&txcurrcd=HKD&txdtm=0001-01-01 12:00:00&sign=888e5e6d66f0afb4b7b467ebb17673bef9fe6b299d60ed932672be18eeb5e90f",
    "app_payment_url": "https://openapi-int.qfapi.com/checkstand/#/?appcode=51E1B3648E92428A8507BFE0918ED042&out_trade_no=TG240618K00001&paysource=91App_checkout&return_url=https://shop2.shop.qa1.hk.91dev.tw/V2/PayChannel/Default/QFPay/TG240618K00001?shopId=2&k=c44bc1b7-45e5-4d13-9a0f-c7658b1ee6fd&lang=zh-HK&sign_type=sha256&txamt=4400&txcurrcd=HKD&txdtm=0001-01-01 12:00:00&sign=888e5e6d66f0afb4b7b467ebb17673bef9fe6b299d60ed932672be18eeb5e90f",
    "redirect_http_method": "POST",
    "post_data": null
  },
  "extend_info": {
    "paymantPageUrl": "https://openapi-int.qfapi.com/checkstand/#/?appcode=51E1B3648E92428A8507BFE0918ED042&out_trade_no=TG240618K00001&paysource=91App_checkout&return_url=https://shop2.shop.qa1.hk.91dev.tw/V2/PayChannel/Default/QFPay/TG240618K00001?shopId=2&k=c44bc1b7-45e5-4d13-9a0f-c7658b1ee6fd&lang=zh-HK&sign_type=sha256&txamt=4400&txcurrcd=HKD&txdtm=0001-01-01 12:00:00&sign=888e5e6d66f0afb4b7b467ebb17673bef9fe6b299d60ed932672be18eeb5e90f"
  }
}
```

<br>
