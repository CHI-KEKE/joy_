
確認SalesOrderGroup無資料，才做轉單第2步


## sp

ERPDB.dbo.csp_TradesOrderTransToSalesOrderWithFlow_Mall

訂單狀態 => TransIsProcessing, 開始建立 SalesOrderGroup 等


## 金流定義

```sql
@sourcePayTypes VARCHAR(1000) = `LinePay,PXPay,JKOPay,GlobalPay,CathayPay,Aftee,icashPay,EasyWallet,PoyaPay,CreditCardOnce_Razer,
CreditCardInstallment_Razer,Boost_Razer,GrabPay_Razer,OnlineBanking_Razer,TNG_Razer,Atome,
PXPayPlus,PlusPay,CreditCardOnce_Stripe,CreditCardOnce_CheckoutDotCom,Wallet,CreditCardOnce_AsiaPay,OnlineBanking_AsiaPay,
TNG_AsiaPay,Boost_AsiaPay,GrabPay_AsiaPay`
```

<br>

## 狀態

- WaitingToTrans
- TransToERPFail
- TransIsProcessing
- TransToCancel
- WaitingToThirdPartyTrans

更新訂單狀態：WaitingToTrans → TransIsProcessing


## flowStatusDef


flowStatusDef
- `ApplePay`, `GooglePay`, `CreditCardInstallment`, `CreditCardOnce` => ''
- `ATM`, `CustomOfflinePayment` => `WaitingToPay`
- 其他 => `WaitingToShipping`

## 執行

- SourceTradesOrderGroup → SalesOrderGroup
- SourceTradesOrderGroupInfo → SalesOrderGroupInfo
- SourceTradesOrder → SalesOrder
- SourceTradesOrderFee → SalesOrderFee
- SourceTradesOrderReceiver → SalesOrderReceiver
- SourceTradesOrderSecret → SalesOrderSecret
- SourceAuthorized → Authorized
- SourceAtmAccount → AtmAccount

根據 @checkStatusDef 和付款方式決定：

- TransToCancel → `Fail`
- 信用卡類型 (ApplePay, GooglePay, CreditCardInstallment, CreditCardOnce)：
  - AuthSuccess → `WaitingToCreditCheck`
  - Expired → `Cancel`
  - 其他 → `Fail`
- ATM, CustomOfflinePayment → `WaitingToPay`
- 第三方支付 → `WaitingThirdPartyPaymentCheck`
- 其他 → `WaitingToShipping`

<br>

## TradesOrderSlave_IsClosed

- `TransToCancel` → 1
- 信用卡類型且 `AuthSuccess` → 0
- 其他失敗情況 → 1
- 正常情況 → 0