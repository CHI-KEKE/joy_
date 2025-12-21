
確認 ERP OrderSlaveFlow無資料，才做轉單第1步

## sp

ERPDB.dbo.csp_ImportWebStoreDBTradesOrdersToERPDBSourceTablesByOrderId_Mall



## 金流定義
```sql
DECLARE @sourcePayTypes VARCHAR(1000) = 
`LinePay,PXPay,JKOPay,Aftee,GlobalPay,CreditCardOnce_Stripe,EWallet_PayMe,CreditCardOnce_CheckoutDotCom,
icashPay,AliPayHK_EftPay,WechatPayHK_EftPay,EasyWallet,PoyaPay,CreditCardOnce_Razer,CreditCardInstallment_Razer,
Boost_Razer,GrabPay_Razer,OnlineBanking_Razer,TNG_Razer,Atome,BoCPay_SwiftPass,UnionPay_EftPay,PXPayPlus,
PlusPay,Wallet,CreditCardOnce_AsiaPay,OnlineBanking_AsiaPay,TNG_AsiaPay,Boost_AsiaPay,GrabPay_AsiaPay`
```

#### Config 定義的 Paytypes
```sql
`CSP.Payment.PayTypes`, @configDBPayTypes
```

@sourcePayTypes = @sourcePayTypes + `,` + @configDBPayTypes;

檢查 TG 的付款方式是否包含上述清單中 && 不存在 TradesOrderThirdpartyPayment 會噴錯

<br>

#### 狀態

撈前台大表 OrderSlaveFlow_StatusDef

- `WaitingToTrans`
- `TransToCancel`
- `WaitingToThirdPartyTrans`

<br>
<br>

## 執行

- WebStoreDB `TradesOrderGroup` => ERPDB `SourceTradesOrderGroup`
- WebStoreDB `TradesOrderGroupInfo` => ERPDB `SourceTradesOrderGroupInfo`
- WebStoreDB `TradesOrder` => ERPDB `SourceTradesOrder`
- WebStoreDB `TradesOrderFee` => `ERPDBSourceTradesOrderFee`
- WebStoreDB `Authorized` =>ERPDB `SourceAuthorized`
- WebStoreDB `AtmAccount` =>ERPDB `SourceAtmAccount`
- WebStoreDB `TradesOrderReceiver` => ERPDB `SourceTradesOrderReceiver`
- WebStoreDB `TradesOrderSecret` → ERPDB`SourceTradesOrderSecret`
- WebStoreDB `TradesOrderSlave` → ERPDB`SourceTradesOrderSlave`
- WebStoreDB `TradesOrderSlavePromotion` → ERPDB `SourceTradesOrderSlavePromotion`
- WebStoreDB `TradesOrderSlavePromotionFreeGift` → ERPDB SourceTradesOrderSlavePromotionFreeGift
- WebStoreDB `TradesOrderSlavePromotionEngine` → ERPDB SourceTradesOrderSlavePromotionEngine
- WebStoreDB `TradesOrderSlavePromotionEngineFreeGift` → ERPDB SourceTradesOrderSlavePromotionEngineFreeGift
- WebStoreDB `TradesOrderThirdPartyPayment` → ERPDB `SourceTradesOrderThirdPartyPayment`