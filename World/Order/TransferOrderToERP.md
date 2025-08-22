# TransferOrderToERP 轉單流程文件

## 目錄
1. [基本資訊](#1-基本資訊)
2. [Step1: ImportWebStoreDBTradesOrdersToERPDBSourceTablesByOrderId](#2-step1-importwebstoredbtradesorderstoerpdbsourcetablesbyorderid)
3. [Step2: TradesOrderTransToSalesOrderWithFlow](#3-step2-tradesordertranstosalesorderwithflow)
4. [檢查資料狀態 同步其他 Table](#4-檢查資料狀態-同步其他-table)
5. [建立費用單](#5-建立費用單)
6. [GenerateMyInvoiceTask](#6-generatemyinvoicetask)
7. [HK CustomOfflinePayment 特殊邏輯](#7-hk-customofflinepayment-特殊邏輯)
8. [轉單完成後呼叫 PayTypeMappingJob](#8-轉單完成後呼叫-paytypemappingjob)

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

### 1.4 定義要處理轉單的金流

#### 1.4.1 MWeb Register TransferOrderToERP Job

#### 1.4.2 在 csp 直接加上金流白名單 或 加入 DB config

<br>

### 1.5 訂單流簡報

https://docs.google.com/presentation/d/1fT4V4umkRRl0fOwIgt5ynN_Pc3v1hN3OknWOvBl5ro0/edit#slide=id.g782a17478a_0_46

<br>

---

## 2. Step1: 資料轉到 後台 Source Table

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

## 3. Step2: 訂單狀態 => TransIsProcessing, 開始建立 SalesOrderGroup 等

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

## 4. 檢查資料狀態 同步其他 Table

### 4.1 檢查請款單存在才長 csp_UpdateDataAfterTradesOrderTransToSalesOrderWithFlow

```csharp
var paymentRequestItem = this._paymentRequestRepository.Get(tradesOrderGroupCode);
```

<br>

### 4.2 csp_UpdateDataAfterTradesOrderTransToSalesOrderWithFlow 包覆大量子 csp

<br>

### 4.3 更新至OrderSlaveFlow 

[csp_UpdateERPDBOrderSlaveFlowByTradesOrderGroupId]
轉單確定完成後，將商店序號、供應商序號、GUID 更新至OrderSlaveFlow

<br>

### 4.4 更新成交手續費率及信用卡費費率

[csp_UpdateSalesOrderSlaveFeeRateByGroupId]
轉單確定完成後，再更新成交手續費率及信用卡費費率

<br>

### 4.5 更新徵信方式 

csp_UpdateCreditCheckTypeDefByGroupId
轉單確定完成後，再更新徵信方式

<br>

### 4.6 溫層商品訂單確認

[csp_ConfirmTemperatureOrderSlave]
轉單後溫層商品訂單確認

<br>

### 4.7 更新第三方支付相關單據

csp_UpdateThirdPartyPaymentByTradesOrderGroupId
更新第三方支付相關單據

<br>

### 4.8 寫入發票開立檔

csp_InsertSalesOrderSlaveElectronicInvoiceByTradesOrderGroupId

<br>

### 4.9 看是否建立 csp_SyncOrderSlaveFlowAfterOrderTransToERPDB

[SalesOrderGroup] + [SalesOrder] -> @thirdPartyPaymentTypeDef
[SalesOrderThirdPartyPayment] + @runId  + @thirdPartyPaymentTypeDef -> @thirdPartyPaymentStatusDef

```sql
SET @isNeedSyncFlow = IIF((@thirdPartyPaymentTypeDef = 'JKOPay' AND @thirdPartyPaymentStatusDef = 'WaitingToPay'),0,1)
```

<br>

---

## 5. 建立費用單

CreateExpenseOrderSlave 

條件是

```csharp
/// 代收：不需處理
/// 直收直付：訂單完成後產生費用單
ExpenseFlowEnum expenseFlow = this._configService.GetAppSetting("ExpenseFlow").ToEnum<ExpenseFlowEnum>();

if (expenseFlow != ExpenseFlowEnum.Direct)
{
    return;
}

//// 也要判定直收付金流
string payProfileTypeDef = this._salesOrderGroupRepository.GetSalesOrderGroupPayProfileTypeDef(tradesOrderGroupId);
bool isEnableCreateExpenseOrder = this._configService.GetAppSetting($"ExpenseOrder.PaymentProcFee.{payProfileTypeDef}.IsEnableCreateExpenseOrder", "false").ToBoolean();
```

<br>

---

## 6. GenerateMyInvoiceTask

NMQ : GenerateMyInvoiceTask

```csharp
if (this._configService.GetAppSetting("Default.Market") == "TW" && orderSlaveFlow.OrderSlaveFlow_ShippingProfileTypeDef == "Oversea")
```

<br>

---

## 7. HK CustomOfflinePayment 特殊邏輯

```csharp
if ("HK".Equals(market, StringComparison.OrdinalIgnoreCase) == true)
{
    string payProfileTypeDef = this._orderSlaveFlowRepository.GetOrderPayProfileTypeDef(tradesOrderGroupId);

    if (nameof(PayProfileTypeDefEnum.CustomOfflinePayment).Equals(payProfileTypeDef, StringComparison.OrdinalIgnoreCase) == true)
    {
        this._taskHelper.CreateTask(NMQJobNameConstants.CreateCustomOfflinePaymentVirtualPayment, tradesOrderGroupId.ToString());
    }
}
```

<br>

---

## 8. 轉單完成後呼叫 PayTypeMappingJob

PayTypeMappingJob 開關 in _shopStaticSetting
NMQ : PayTypeMappingJob

<br>