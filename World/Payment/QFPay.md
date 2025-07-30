# QFPay 文件

## 目錄
1. [異常紀錄](#1-異常紀錄)

<br>

---

## 1. 異常紀錄

### 1.1 訊息

<br>

https://91app.slack.com/archives/C7T5CTALV/p1753837502573089

<br>

該筆訂單仍有正在處理中的退款請求，請勿重複提交申請。

<br>

### 1.2 確認訂單資訊

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

### 1.3 PaymentMiddleware 紀錄

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

### 1.4 打 Query 看是否有其他紀錄

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

### 1.5 結論

<br>

1. query 後沒有其他交易狀態僅有之前的付款成功紀錄
2. 根據 log 並無 TS250729L00150C-160027 重複發送 Refund 紀錄
3. 這張單有3張子單做退款前兩張子單正常成功 最後一張顯示訊息: "You still have prosessing requests, please do not repeat the initiation.\\n該筆訂單仍有正在處理中的退款請求，請勿重複提交申請。"

<br>
