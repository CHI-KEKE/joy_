# 2C2P — 退款

## 前置條件

退款操作僅允許對已結算（Settled）的交易執行，交易狀態必須為 `S`。

> **注意**：Settled 狀態表示銀行或金流商已完成清算，實際資金已從消費者轉入商家帳戶。

<br>

## Error Codes — 退款與取消

| 代碼 | 描述 | 說明 |
|------|------|------|
| 00 | Successful | 操作成功 |
| 12 | Transaction in progress | 交易狀態不允許執行此操作 |
| 46 | 餘額不足，無法執行退款 | RefundPending |
| 99 | Unable to Authenticate Card Holder | 3D 驗證失敗 |

> 完整 Error Codes 請參考：[Reverse Process Codes](https://developer.2c2p.com/docs/response-code-payment-maintenance-result-code)

<br>

## NMQ Job

| Job_Id | Job_Name | Job_Description | Job_ClassName |
|--------|----------|-----------------|---------------|
| 440 | TwoCTwoPRefundRequestFinish | PaymentMiddleware 退款完成 | NineYi.SCM.Frontend.NMQV2.ThirdPartyPayments.PaymentMiddleWareRefundRequestFinishProcess |

**訊息格式範例：**

```json
{"RefundRequestIds":[121537],"TradesOrderGroupId":1476443,"PayType":"TwoCTwoP"}
```

<br>

## API 端點（內部）

```
POST https://payment-middleware-api-internal.hk.91app.io/api/v1.0/Refund/TwoCTwoP/{TG_CODE}
POST https://payment-middleware-api-internal.hk.91app.io/api/v1.0/RefundQuery/TwoCTwoP
```
