# Alert 文件

## 目錄
1. [HK 退款異常統計](#1-hk-退款異常統計)
2. [HK Prod 第三方付款成功但大表狀態異常](#2-hk-prod-第三方付款成功但大表狀態異常)
3. [30分鐘仍在待付款狀態](#3-30分鐘仍在待付款狀態)

<br>

---

## 1. HK 退款異常統計

<br>

撈取 RefundRequestFail , RefundRequestGrouping 確認 TG , Paytype

<br>

---

## 2. HK Prod 第三方付款成功但大表狀態異常

<br>

**排程：** ☘️ Monitor_PaymentSuccessOrderSlaveFlowHide

<br>

### SQL 查詢的條件（邏輯重點）：

<br>

- 最近 4 小時內（BETWEEN @startTime AND @endTime）
- 第三方付款記錄為成功（StatusDef = 'Success'）但其關聯的大表 OrderSlaveFlow 裡面狀態卻是 StatusForSCMDef = 'Hide'（代表這筆訂單處於異常隱藏狀態）

<br>

只要符合這三個條件的任一筆資料存在，就會產生 alert：

<br>

- 第三方付款成功（例如 TapPay、QFPay 回傳成功）
- 該訂單對應的資料還被標記為 隱藏狀態（StatusForSCMDef = 'Hide'）
- 查詢時間區間是 近 4 小時內

<br>

---

## 3. 30分鐘仍在待付款狀態

<br>

[HK] 有 11 筆訂單超過30分鐘仍在待付款狀態，請確認 Payment Middleware Console 是否正常運作

<br>

```
TradesOrderThirdPartyPayment_TypeDef              : AliPayHK_EftPay
TradesOrderThirdPartyPayment_ShopId               : 17
TradesOrderThirdPartyPayment_TradesOrderGroupCode : TG250616B00009
TradesOrderThirdPartyPayment_TotalPayment         : 1685.10
```

<br>

### 三方表資訊確認

<br>

```
TG : TG250616B00009
Table : TradesOrderThirdPartyPayment
TradesOrderThirdPartyPayment_StatusDef : Success
TradesOrderThirdPartyPayment_UpdatedDateTime : 2025-06-16 01:10:28.033
```

<br>

看起來是付款完成後 更新DB 資料失敗

<br>

### 補更新大表狀態

<br>

PR : https://bitbucket.org/nineyi/nineyi.database.operation/pull-requests/22271/overview

<br>

### 更新後大表確認

<br>

OrderSlaveFlow_StatusDef = WaitingToShipping

<br>

### 相關連結

<br>

VSTS : https://91appinc.visualstudio.com/G11n/_workitems/edit/499919

<br>