
## Alert


```bash
[HK] 有 11 筆訂單超過30分鐘仍在待付款狀態，請確認 Payment Middleware Console 是否正常運作

TradesOrderThirdPartyPayment_TypeDef              : AliPayHK_EftPay
TradesOrderThirdPartyPayment_ShopId               : 17
TradesOrderThirdPartyPayment_TradesOrderGroupCode : TG250616B00009
TradesOrderThirdPartyPayment_TotalPayment         : 1685.10
```


## DB 狀態檢查

**TG**：TG250616B00009
**Table**：TradesOrderThirdPartyPayment
**Status**：
- TradesOrderThirdPartyPayment_StatusDef : Success
- TradesOrderThirdPartyPayment_UpdatedDateTime : 2025-06-16 01:10:28.033


## 原因

看起來是付款完成後更新 DB 資料失敗


## 處理

#### 補更新大表狀態

PR：https://bitbucket.org/nineyi/nineyi.database.operation/pull-requests/22271/overview

<br>

####　更新後大表確認

OrderSlaveFlow_StatusDef = WaitingToShipping
**相關 VSTS**：https://91appinc.visualstudio.com/G11n/_workitems/edit/499919