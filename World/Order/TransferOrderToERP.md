# TransferOrderToERP 轉單流程文件

## 目錄
1. [基本資訊](#1-基本資訊)
2. [Step1: ImportWebStoreDBTradesOrdersToERPDBSourceTablesByOrderId](#2-step1-importwebstoredbtradesorderstoerpdbsourcetablesbyorderid)
3. [Step2: TradesOrderTransToSalesOrderWithFlow](#3-step2-tradesordertranstosalesorderwithflow)

<br>

---

## 1. 基本資訊

### 1.1 站台

SCM.NMQ

<br>

### 1.2 資料維度

by TG

<br>

### 1.3 Task Data 範例

```json
{
  "Data": "2242990",
  "JobName": "TransferOrderToERP"
}
```

<br>

---

## 2. Step1: ImportWebStoreDBTradesOrdersToERPDBSourceTablesByOrderId

### 2.1 執行 SP

ERPDB.dbo.csp_ImportWebStoreDBTradesOrdersToERPDBSourceTablesByOrderId_Mall

<br>

### 2.2 金流資訊定義

#### CSP 寫死定義金流資訊

```sql
DECLARE @sourcePayTypes VARCHAR(1000) = 
'LinePay,PXPay,JKOPay,Aftee,GlobalPay,CreditCardOnce_Stripe,EWallet_PayMe,CreditCardOnce_CheckoutDotCom,
icashPay,AliPayHK_EftPay,WechatPayHK_EftPay,EasyWallet,PoyaPay,CreditCardOnce_Razer,CreditCardInstallment_Razer,
Boost_Razer,GrabPay_Razer,OnlineBanking_Razer,TNG_Razer,Atome,BoCPay_SwiftPass,UnionPay_EftPay,PXPayPlus,
PlusPay,Wallet,CreditCardOnce_AsiaPay,OnlineBanking_AsiaPay,TNG_AsiaPay,Boost_AsiaPay,GrabPay_AsiaPay'
```

<br>

#### Config 定義的 Paytypes

'CSP.Payment.PayTypes', @configDBPayTypes

<br>

#### 全部加在一起

@sourcePayTypes = @sourcePayTypes + ',' + @configDBPayTypes;

<br>

### 2.3 處理流程

檢查 TG 的付款方式是否包含上述清單中 && 不存在 TradesOrderThirdpartyPayment 會噴錯

<br>

大表撈 OrderSlaveFlow_StatusDef IN ('WaitingToTrans' , 'TransToCancel','WaitingToThirdPartyTrans') 狀態訂單 from 前台

<br>

塞入後台大表

<br>

### 2.4 資料轉移清單

- 撈前台 TradesOrderGroup 塞入 後台 SourceTradesOrderGroup
- 撈前台 TradesOrderGroupInfo 塞入 後台 SourceTradesOrderGroupInfo
- 撈前台 TradesOrder 塞入 SourceTradesOrder
- 撈前台 TradesOrderFee 塞入 SourceTradesOrderFee
- 撈前台 Authorized 塞入 SourceAuthorized
- 撈 AtmAccount 塞入 SourceAtmAccount
- 撈 TradesOrderReceiver 塞入 SourceTradesOrderReceiver
- TradesOrderSecret → SourceTradesOrderSecret
- TradesOrderSlave → SourceTradesOrderSlave
- TradesOrderSlavePromotion → SourceTradesOrderSlavePromotion
- TradesOrderSlavePromotionFreeGift → SourceTradesOrderSlavePromotionFreeGift
- TradesOrderSlavePromotionEngine → SourceTradesOrderSlavePromotionEngine
- TradesOrderSlavePromotionEngineFreeGift → SourceTradesOrderSlavePromotionEngineFreeGift
- TradesOrderThirdPartyPayment → SourceTradesOrderThirdPartyPayment

<br>

---

## 3. Step2: TradesOrderTransToSalesOrderWithFlow

### 3.1 執行 SP

ERPDB.dbo.csp_TradesOrderTransToSalesOrderWithFlow_Mall

<br>

### 3.2 金流定義

```sql
@sourcePayTypes VARCHAR(1000) = 'LinePay,PXPay,JKOPay,GlobalPay,CathayPay,Aftee,icashPay,EasyWallet,PoyaPay,CreditCardOnce_Razer,
CreditCardInstallment_Razer,Boost_Razer,GrabPay_Razer,OnlineBanking_Razer,TNG_Razer,Atome,
PXPayPlus,PlusPay,CreditCardOnce_Stripe,CreditCardOnce_CheckoutDotCom,Wallet,CreditCardOnce_AsiaPay,OnlineBanking_AsiaPay,
TNG_AsiaPay,Boost_AsiaPay,GrabPay_AsiaPay'
```

<br>

### 3.3 狀態檢查

```sql
@checkStatusDef in ('WaitingToTrans','TransToERPFail','TransIsProcessing', 'TransToCancel','WaitingToThirdPartyTrans')
```

<br>

更新訂單狀態：WaitingToTrans → TransIsProcessing

<br>

### 3.4 付款方式決定轉單後狀態

```sql
IF @orderPayProfileDef IN ('ApplePay', 'GooglePay', 'CreditCardInstallment', 'CreditCardOnce')
BEGIN
    SET @flowStatusDef = '';
END
ELSE IF @orderPayProfileDef IN ('ATM', 'CustomOfflinePayment')
BEGIN
    SET @flowStatusDef = 'WaitingToPay';
END
ELSE
BEGIN
    SET @flowStatusDef = 'WaitingToShipping';
END
```

<br>

### 3.5 資料寫入流程

- SourceTradesOrderGroup → SalesOrderGroup
- SourceTradesOrderGroupInfo → SalesOrderGroupInfo
- SourceTradesOrder → SalesOrder
- SourceTradesOrderFee → SalesOrderFee
- SourceTradesOrderReceiver → SalesOrderReceiver
- SourceTradesOrderSecret → SalesOrderSecret
- SourceAuthorized → Authorized
- SourceAtmAccount → AtmAccount

<br>

### 3.6 TradesOrderSlave_StatusDef 決定邏輯

根據 @checkStatusDef 和付款方式決定：

<br>

- TransToCancel → 'Fail'
- 信用卡類型 (ApplePay, GooglePay, CreditCardInstallment, CreditCardOnce)：
  - AuthSuccess → 'WaitingToCreditCheck'
  - Expired → 'Cancel'
  - 其他 → 'Fail'
- ATM, CustomOfflinePayment → 'WaitingToPay'
- 第三方支付 → 'WaitingThirdPartyPaymentCheck'
- 其他 → 'WaitingToShipping'

<br>

### 3.7 TradesOrderSlave_IsClosed 決定邏輯

- TransToCancel → 1
- 信用卡類型且 AuthSuccess → 0
- 其他失敗情況 → 1
- 正常情況 → 0

<br>

---