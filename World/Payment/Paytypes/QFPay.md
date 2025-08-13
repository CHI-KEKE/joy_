# QFPay 文件

![alt text](./image.png)


## 目錄
0. [基本資訊](#0-基本資訊)
1. [異常紀錄](#1-異常紀錄)
2. [正常發動付款頁面的相關資訊](#2-正常發動付款頁面的相關資訊)
3. [商戶後台與登入資料](#3-商戶後台與登入資料)
4. [環境URL](#4-環境url)
5. [測試卡](#5-測試卡)
6. [三方頁面處理](#6-三方頁面處理)
7. [金額限制](#7-金額限制)
8. [逆流程](#8-逆流程)
9.  [Paytypes](#10-paytypes)
10. [Currency](#11-currency)
11. [Transaction Status Codes](#12-transaction-status-codes)
12. [Refund](#13-refund)
13. [問題清單](#14-問題清單)
14. [關帳](#15-關帳)

<br>

---

## 0. 基本資訊

### 0.1 串接方式

hosted payment pages

<br>

### 0.2 開發文件

https://sdk.qfapi.com/

<br>

### 0.3 聯絡方式

WhatsApp

<br>

### 0.4 測試卡資訊

**測試卡文件:** https://sdk.qfapi.com/docs/online-shop/visa-master-online-payment#test-cards

<br>

| 卡別 | 卡號 | 預期結果 |
|------|------|----------|
| MasterCard | 5200000000001096 | 付款成功 |
| Visa | 4000000000001091 | 付款成功 |
| MasterCard | 5200000000001005 | 付款成功 (免 3D 驗證) |
| Visa | 4000000000001000 | 付款成功 (免 3D 驗證) |
| MasterCard | 5200000000001120 | 付款失敗 (at verification) |
| Visa | 4000000000001125 | 付款失敗 (at verification) |
| MasterCard | 5200000000001013 | 付款失敗 (at 3DS frictionless) |
| Visa | 4000000000001018 | 付款失敗 (at 3DS frictionless) |

<br>

### 0.5 特殊測試卡行為說明

#### 0.5.1 付款失敗 (at verification) 測試卡

**適用卡號:**
- MasterCard: 5200000000001120
- Visa: 4000000000001125

<br>

**回應資訊:**
- Return code: 1145
- Message: 處理中，請稍等

<br>

**行為特徵:**
1. 重新到相同付款頁仍然可以用正確卡號結帳轉導
2. QFPay 會有 2 筆紀錄
3. 屬於交易進行中情境
4. 跳轉 FailedUrl

<br>

#### 0.5.2 付款失敗 (at 3DS frictionless) 測試卡

**適用卡號:**
- MasterCard: 5200000000001013
- Visa: 4000000000001018

<br>

**回應資訊:**
- Return code: 1205
- Message: 交易失敗，請稍後重试

<br>

**行為特徵:**
1. 填卡頁會直接阻擋
2. 重新填正確卡號可以完成交易跳轉
3. 會有 2 筆資料

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

### 1.2 QFPay Sign_Error

<br>

- ReturnUrl 要 Encode 過
- K 壞掉

<br>

### 1.3 重複的退款申請

#### 1.3.1 訊息

76 out_trade_no is used - Do not use duplicate out_trade_no for request(1251)

<br>

#### 1.3.2 過程

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

#### 1.3.3 釐清

QFPay payment 抱錯失敗變成處理中，結果清空 RefundRequest_TransactionId 重新觸發退款，再試一次會因為不能帶一模一樣的 request 因此阻擋，要請商家到後台退款

<br>

https://91appinc.visualstudio.com/DailyResource/_workitems/edit/499423

<br>

### 1.4 QFPay 本身有異常

<br>

QFPay 說會拿到 1297 (但目前無法測試到這個情境)

<br>

### 1.5 PaymentMiddleware 異常

<br>

資料處理逾時

<br>

### 1.6 3D 驗證錯誤

<br>

"Verification of PaRes failed", 跳轉 交易進行中

<br>

### 1.7 信用卡錯誤

<br>

停在信用卡頁直到逾期

<br>

### 1.8 返回

<br>

選擇一種付款方式後都可按 "返回" 跳回選擇其他付款方式

<br>

### 1.9 逾期

<br>

時間為 12 min, 會在 QFPay UI頁面倒數，直到跳轉回91 , 顯示 "交易進行中"

<br>

### 1.10 同一筆TG 下，會有多筆交易紀錄

<br>

在同一筆TG 下，會有多筆交易紀錄，因為可以開多個視窗產生多筆交易成功，但可能會抓到錯誤的最終狀態 (已交易成功，但抓到處理中的狀態記錄在 91APP)

<br>

**解法:**

<br>

只要有成功的紀錄就抓出來作為交易成功的 TG，若有多筆成功也會記錄下來，並藉由 定期 Recheck 去檢查，並通知商店退款

<br>

### 1.11 WechatPay 在手機板仍只有 QRCode

<br>

請 QFPay 人員配置 H5

<br>

### 1.12 WechatPay配置 H5 後出現商家參數異常問題

<br>

QFPay 人員漏了配置 https://test-openapi-hk.qfapi.com Domain

<br>

### 1.13 MWeb測試會跳轉2次

<br>

from QFPay & 第三方, QFPay人員暫無解

<br>

### 1.14 退款異常，Return Data 為空

<br>

**時間軸：**

<br>

- **10/09 / 10/26** 訂單交易成功
- **11/28** 申請退款
- **11/28 17:00** Refund, 退款接口只能申请 29 天的订单 (0010000293181285348321738)
- 該筆訂單被禁止退款，已經超出可退款時限，FPS 只能退 29 天以內的交易。"respcd\": \"1265\" ==> Pending 等下一輪
- **11/28 18:00** RefundQuery 該筆訂單被禁止退款，23:30-00:30 的交易以及部分特殊交易會被禁止（1265）
- ~~ 持續問到相同狀態 持續 Pending ~~
- **12/01 0:03** 訂單資料為空 RefundFail

<br>

**修正方案：**

<br>

已修正 PaymentMiddleware 多抓 "該筆訂單被禁止退款，已經超出可退款時限" msg 作為區分

<br>

- **"時間區間的禁止退款"** ==> 可 Retry
- **"關帳退款的禁止"** ==> 需壓成 Fail

<br>

---

## 2. 正常發動付款頁面的相關資訊

API: `/api/v1.0/pay/QFPay/TG240618K00002`


1.QFPay 為自己組 Url, 而非打 API 取得 跳轉頁, 因此可能會發生使用者掛在 QFSign_Error 卻沒人知道得狀況 ( 已向 QFPay確認沒有可以用來確認網頁狀態的 API)

2.QA 環境有分 Live / Testing (第三方支付) / SandBox (信用卡)

3.逾期設定參數目前使用 checkoutExpiredTime 這個節點壓 TimeStamp 讓 UI 付款頁逾期

4.expiredTime 用來設定 QRCode 的逾期時間

5.目前在第三方沒有訂單失敗而無法繼續付款的情境

6.不到 3DS 驗證頁面 或者通道沒有拒絕交易 QF 系統都沒有訂單紀錄

7."syssn" = TransactionId


8.信用卡都是 Webview, 第三方支付會需要跳第三方APP ==> 兩個需求都要滿足的話需使用

{appInitPath}-s{shopId:D6}://thirdpartypayconfirm?url={encodedUri}

9.手機端會看不到倒數計時，但實際上會倒數並跳轉

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

我們要對接一個第三方金流（QFPay），他們要求你把付款資訊（例如金額、商品名稱、通知網址）包成一段網址參數，並且用你們雙方約定好的 API Key 做 SHA256 加密簽名。

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

---

## 3. 商戶後台與登入資料

### sandbox

<br>

**URL:** https://sh-int-hk.qfapi.com/

<br>

### testing

<br>

**URL:** https://sh-hk.qfapi.com/#/login

<br>

---

## 4. 環境URL

| Environment Name | Prod. URL |
|------------------|-----------|
| Sandbox (Only for credit card simulations) | https://openapi-int.qfapi.com |
| Live Testing Environment | https://test-openapi-hk.qfapi.com |
| Production | https://openapi-hk.qfapi.com |

<br>

---

## 5. 測試卡

| 卡別 | Value | 預期結果 |
|------|-------|----------|
| MasterCard | 5200000000001096 | 付款成功 |
| Visa | 4000000000001091 | 付款成功 |
| MasterCard | 5200000000001005 | 付款成功 (免 3D 驗證) |
| Visa | 4000000000001000 | 付款成功 (免 3D 驗證) |
| MasterCard | 5200000000001120 | 付款失敗 (at verification) ==> 1145, 處理中，請稍等 ==> 再到相同付款頁仍然可以用正確卡號結帳轉導 ==> QFPay會有兩筆紀錄 ==> 屬於交易進行中情境 |
| Visa | 4000000000001125 | 付款失敗 (at verification) |
| MasterCard | 5200000000001013 | 付款失敗 (at 3DS frictionless)  ==> 1205, 交易失敗，請稍後重试(填卡頁會直接阻擋，停留在填卡頁不轉導) |
| Visa | 4000000000001018 | 付款失敗 (at 3DS frictionless) |

<br>

---

## 6. 三方頁面處理

### 6.1 OTP 亂打

<br>

在OTP頁阻擋

<br>

### 6.2 逾期

<br>

會跳轉但訂單沒有 StatusCode

<br>

### 6.3 付過款的訂單

<br>

發現該訂單已經付過款會直接跳轉

<br>

---

## 7. 金額限制

### 7.1 交易金額官方說沒有限制

<br>

目前測試 0.5 也可以

<br>

### 7.2 金額有上限

<br>

訊息：銀行系統繁忙，請耐心等待，建議稍後重試或使用其他支付方式(1297)

<br>

---

## 8. 逆流程

<br>

- 信用卡付款無法Cancel
- 部分付款方式當天退款必須要全額退款，無法部分退
- 退款金費是扣除當天交易金額，所以可能會發生退款餘額不足的問題

<br>

### 8.2 Refund

<br>

先取得 SalesOrderThirdPartyPayment 進行 Refund，Mapping 以下資訊取得結果

<br>

| 狀態名稱 | Return Code | 描述 | 第三方顯示訊息 | 91APP 狀態 |
|----------|-------------|------|----------------|------------|
| TransactionClosed | 1264 | 第三方顯示訂單已結束，通常發生在退款單重複申請 | x | 退款失敗 |
| Success | 0000 | 退款成功 | x | 退款成功 |
| SameDayPartialRefundRejected | 1124 | 信用卡不可當天部分退 | x | 退款處理中 |
| RequestProcessing | 1145 | 處理中請稍後 | x | 退款處理中 |
| RefundPeriodExceeded | 1265 | 改筆訂單已經關帳 | 該筆訂單被禁止退款，已經超出可退款時限 | 退款失敗 |
| RefundNotAllowedDuringBillingPeriod | 1265 | 11:30 PM~ 12:30 AM 帳期無法退款 | x | 退款處理中 |
| InsufficientRefundableBalance | 1269 | 帳戶餘額不足 | x | 退款處理中 |

<br>

### 8.3 RefundQuery

<br>

有 RefundRequest TransactionId , 執行 RefundQuery 確認 退款狀態 (屬於事後確認)

<br>

| 狀態名稱 | Return Code | 描述 | 第三方顯示訊息 | 91APP 狀態 |
|----------|-------------|------|----------------|------------|
| Success | 0000 | 退款成功 | x | 退款成功 |
| SameDayPartialRefundRejected | 1124 | 信用卡不可當天部分退 | x | 退款處理中 |
| InsufficientRefundableBalance | 1269 | 帳戶餘額不足 | x | 退款處理中 |
| RefundNotAllowedDuringBillingPeriod | 1265 | 11:30 PM~ 12:30 AM 帳期無法退款 | x | 退款處理中 |
| RequestProcessing | 1145 | 處理中請稍後 | x | 退款處理中 |
| TransactionClosed | 1264 | 第三方顯示訂單已結束，通常發生在退款單重複申請 | x | 退款失敗 |

<br>

---

## 10. Paytypes

https://sdk.qfapi.com/docs/preparation/paycode/#paytype

<br>

| Code | Description |
|------|-------------|
| 800008 | Consumer Present QR Code Mode (CPM) for WeChat, Alipay, UNIONPAY Quick Pass |
| 800101 | Alipay Merchant Presented QR Code Payment in store (MPM) (Overseas Merchants) |
| 800108 | Alipay Consumer Presented QR Code Payment (CPM) (Overseas & HK Merchants) |
| 801101 | Alipay Online WEB (in browser Chrome etc.) Payment (Overseas Merchants) ** |
| 801107 | Alipay Online WAP (in mobile browser Chrome etc.) Payment (Overseas Merchants) |
| 801110 | Alipay in-APP Payments (Overseas Merchants) |
| 800107 | Alipay Service Window H5 Payment (in Alipay APP H5 payments) |
| 801501 | Alipay Merchant Presented QR Code (MPM) Payment (HK Merchants) |
| 801510 | Alipay In-App Payment (HK Merchants) |
| 801512 | Alipay Online WAP Payment (HK Merchants) |
| 801514 | Alipay Online WEB Payment (HK Merchants) |
| 800201 | WeChat Merchant Presented QR Code Payment (MPM) (Overseas & HK Merchants) |
| 800208 | WeChat Consumer Presented QR Code Payment (CPM) (Overseas & HK Merchants) |
| 800207 | WeChat JSAPI Payment - WeChat Official Account Payment (in Wechat App)(Overseas & HK Merchants) |
| 800212 | WeChat H5 Payment (In mobile browser) |
| 800210 | WeChat in-APP Payment (Overseas & HK Merchants) |
| 800213 | WeChat Mini-Program Payment (Overseas & HK Merchants) |
| 801008 | WeChat Pay HK Consumer Presented QR Code Payment (CPM) (Direct Settlement, HK Merchants) |
| 801010 | WeChat Pay HK In-App Payment (Direct Settlement, HK Merchants) |
| 805801 | PayMe Merchant Presented QR Code Payment in store (MPM) (HK Merchants) |
| 805808 | PayMe Consumer Presented QR Code Payment (CPM) (HK Merchants) |
| 805814 | PayMe Online WEB (in browser Chrome etc.) Payment (HK Merchants) |
| 805812 | PayMe Online WAP (in mobile browser Chrome etc.) Payment (HK Merchants) |
| 800701 | UNIONPAY Quick Pass Merchant Presented QR Code Payment (MPM) |
| 800708 | UNIONPAY Quick Pass Consumer Presented QR Code Payment (CPM) |
| 800712 | UNIONPAY WAP Payment (HK Merchants) |
| 800714 | UNIONPAY PC-Web Payment (HK Merchants) |
| 802001 | FPS Merchant Presented QR Code Payment (MPM) (HK Merchants)*** |
| 803701 | Octopus dynamic QRC Payment - Merchant Present Mode (MPM) (HK Merchants) |
| 802801 | Visa / Mastercard Online Payments |
| 802808 | Visa / Mastercard Offline Payments |
| 806527 | ApplePay Online Payments |
| 806708 | UnionPay Card Offline Payments |
| 806808 | American Express Card Offline Payments |

<br>

---

## 11. Currency

<br>

| Code | Description |
|------|-------------|
| AED | Arab Emirates Dirham |
| CNY | Chinese Yuan |
| EUR | Euro |
| HKD | Hong Kong Dollar |
| IDR | Indonesian Rupiah |
| JPY | Japanese Yen |
| MMK | Myanmar Kyat |
| MYR | Malaysian Ringgit |
| SGD | Singapore Dollar |
| THB | Thai Baht |
| USD | United States Dollar |
| CAD | Canadian Dollar |
| AUD | Australian Dollar |

<br>

---

## 12. Transaction Status Codes

<br>

https://sdk.qfapi.com/docs/preparation/paycode#transaction-status-codes

<br>

| Return code | Description |
|-------------|-------------|
| 0000 | Transaction successful |
| 1100 | System under maintenance (1100) |
| 1101 | Reversal error (1101) |
| 1102 | Duplicate request (1102) |
| 1103 | Request format error (1103) |
| 1104 | Request parameter error (1104) |
| 1105 | Device not activated (1105) |
| 1106 | Invalid device (1106) |
| 1107 | Device not allowed (1107) |
| 1108 | Signature error (1108) |
| 1125 | Transaction has been refunded already (1125) |
| 1136 | The transaction does not exist or is not operational (1136) |
| 1142 | Order already closed (1142) |
| 1143 | The order has not been paid for, the password is currently being entered (1143) |
| 1145 | Please wait while processing (1145) |
| 1147 | Wechat pay transaction error (1147) |
| 1150 | Your billing method is T0 and does not support canceling transactions. (1150) |
| 1155 | Refund request denied (1155) |
| 1181 | Order expired (1181) |
| 1201 | Insufficient balance, please use a different payment method (1201) |
| 1202 | Incorrect or expired payment code, please show the correct payment code or refresh the payment code and retry (1202) |
| 1203 | Merchant account error, confirm that the payment account is configured correctly (1203) |
| 1204 | Bank error, confirm that the payment wallet is functionable (1204) |
| 1205 | The transaction failed. Please try again later (1205) |
| 1212 | Please use the UnionPay overseas payment code (1212) |
| 1241 | The store does not exist or the status is incorrect. Do not conduct payments (1241) |
| 1242 | The store has not been configured correctly, unable to conduct payments (1242) |
| 1243 | The store has been disabled. Do not conduct payments, contact the owner to confirm (1243) |
| 1250 | The transaction is forbidden. For more information please contact QFPay Customer Service Team (1250) |
| 1251 | The store has not been configured correctly, we are currently working to fix this problem (1251) |
| 1252 | System error when making the order request (1252) |
| 1254 | A problem occured. We are currently resolving the issue (1254) |
| 1260 | The order has already been paid for, please confirm the transaction result before conducting more transactions (1260) |
| 1261 | The order has not been paid for, please confirm the transaction result before conducting more transactions (1261) |
| 1262 | The order has been refunded, please confirm the order status before conducting more transactions (1262) |
| 1263 | The order has been cancelled, please confirm the order status before conducting more transactions (1263) |
| 1264 | The order has been closed, please confirm the order status before conducting more transactions (1264) |
| 1265 | The transaction cannot be refunded. Refunds for transactions between 11:30pm to 0:30am and special promotions cannot be processed. (1265) |
| 1266 | The transaction amount is wrong, please confirm the order status (1266) |
| 1267 | The order information does not match, please confirm the order status (1267) |
| 1268 | The order does not exist, please confirm the order status (1268) |
| 1269 | Today's unsettled transaction amount is insufficient. Refunds cannot be processed. Please confirm that the balance is sufficient (1269) |
| 1270 | This currency does not support partial refunds (1270) |
| 1271 | The selected transaction does not support partial refunds (1271) |
| 1272 | The refund amount is greater than the maximum amount that can be refunded for the original transaction (1272) |
| 1294 | The transaction may be non-compliant and has been prohibited by the bank (1294) |
| 1295 | The connection is slow, waiting for a network response (1295) |
| 1296 | The connection is slow, waiting for a network response. Please try again later or use other payment methods (1296) |
| 1297 | The banking system is busy. Please try again later or use other payment methods (1297) |
| 1298 | The connection is slow, waiting for a network response. In case you have already paid, do not repeat the payment. Please confirm the result later (1298) |
| 2005 | The customer payment code is incorrect or has expired, please refresh and restart the transaction process (2005) |
| 2011 | Transaction serial number repeats (2011) |

<br>

---

## 13. Refund

<br>

QFPay API：https://test-openapi-th.qfapi.com/trade/v1/refund

<br>

### 13.1 餘額不足

<br>

```json
{
  "request_id": "6de092a5-023a-41f8-8922-1858050597fa",
  "transaction_id": null,
  "return_code": "3000",
  "return_message": "",
  "extend_info": {
    "syssn": null,
    "orig_syssn": "20240704180500020000018223",
    "txamt": null,
    "sysdtm": null,
    "respcd": "1269",
    "resperr": "Today this payment method refundable balance is 0 HKD, not enough.Please try it again when you have enough incomes.(1269)",
    "paydtm": null,
    "txdtm": null,
    "udid": null,
    "txcurrcd": "HKD",
    "respmsg": "",
    "out_trade_no": "0704Test_2_biiei2eo2i_1",
    "chnlsn": null,
    "cardcd": null
  }
}
```

<br>

### 13.2 Visa / Mastercard卡交易，系统规定，若是当日交易当日退款，需要全额申请，隔日退款才可以用部分退款

<br>

```json
{
  "request_id": "6adb5622-132b-4719-926a-1eeb5cfb315d",
  "transaction_id": null,
  "return_code": "3000",
  "return_message": "",
  "extend_info": {
    "syssn": null,
    "orig_syssn": "20240705180500020000018229",
    "txamt": null,
    "sysdtm": null,
    "respcd": "1124",
    "resperr": "For void request, it must be a full refund, please correct refund amount and re-initiate it",
    "paydtm": null,
    "txdtm": null,
    "udid": null,
    "txcurrcd": null,
    "respmsg": "",
    "out_trade_no": "0705Test",
    "chnlsn": null,
    "cardcd": null
  }
}
```

<br>

### 13.3 退款成功

<br>

```json
{
  "request_id": "00abd607-af40-4984-9f60-2679fb727a91",
  "transaction_id": "20240705180500020000018231",
  "return_code": "0000",
  "return_message": "Void received",
  "extend_info": {
    "syssn": "20240705180500020000018231",
    "orig_syssn": "20240705180500020000018229",
    "txamt": "1000000",
    "sysdtm": "2024-07-05 10:16:05",
    "respcd": "0000",
    "resperr": "交易成功",
    "paydtm": "2024-07-05 10:16:06",
    "txdtm": "2024-07-05 10:14:00",
    "udid": "qiantai2",
    "txcurrcd": "HKD",
    "respmsg": "Void received",
    "out_trade_no": "0705Test",
    "chnlsn": "",
    "cardcd": ""
  }
}
```

<br>

### 13.4 退款成功後 QUERY

<br>

```json
{
  "request_id": "0817f27c-9575-47cf-841b-4c08514ce696",
  "transaction_id": null,
  "return_code": "3000",
  "return_message": "wrong cancel",
  "extend_info": {
    "syssn": null,
    "orig_syssn": "20240703155300020040434464",
    "txamt": null,
    "sysdtm": null,
    "respcd": "1264",
    "resperr": "Order closed, please confirm the order status before continuing(1264)",
    "paydtm": null,
    "txdtm": null,
    "udid": null,
    "txcurrcd": "HKD",
    "respmsg": "wrong cancel",
    "out_trade_no": "c",
    "chnlsn": null,
    "cardcd": null
  }
}
```

<br>

### 13.5 退完再退

<br>

```json
{
  "request_id": "d4bedc24-7a61-4668-84f8-095d9aee4f13",
  "transaction_id": null,
  "return_code": "3000",
  "return_message": "wrong cancel",
  "extend_info": {
    "syssn": null,
    "orig_syssn": "20240703155300020040434464",
    "txamt": null,
    "sysdtm": null,
    "respcd": "1264",
    "resperr": "Order closed, please confirm the order status before continuing(1264)",
    "paydtm": null,
    "txdtm": null,
    "udid": null,
    "txcurrcd": "HKD",
    "respmsg": "wrong cancel",
    "out_trade_no": "c",
    "chnlsn": null,
    "cardcd": null
  }
}
```

<br>

### 13.6 其他退款測試方向

<br>

要確定退款是打完API當天就會直接退給你錢嗎?

<br>

---

## 14. 問題清單

<br>

https://docs.google.com/spreadsheets/d/1F5wXCP7-w-_u2C7vpVbIP_GgMe75Y9kH2HM-rJH7vAQ/edit?gid=1016597365#gid=1016597365

<br>

---

## 15. 關帳

<br>

| Code | Description | 關帳期間 (天) |
|------|-------------|--------------|
| 800201 | WeChat Merchant Presented QR Code Payment (MPM) (Overseas & HK Merchants) | 365 |
| 800212 | WeChat H5 Payment (In mobile browser) | 365 |
| 801512 | Alipay Online WAP Payment (HK Merchants) | 365 |
| 801514 | Alipay Online WEB Payment (HK Merchants) | 365 |
| 802001 | FPS Merchant Presented QR Code Payment (MPM) (HK Merchants)*** | 29 |
| 802801 | Visa / Mastercard Online Payments | 365 |
| 805812 | PayMe Online WAP (in mobile browser Chrome etc.) Payment (HK Merchants) | 90 |
| 805814 | PayMe Online WEB (in browser Chrome etc.) Payment (HK Merchants) | 90 |
| 未定義 | 未定義的例外處理 | 365(取最大值) |

<br>
