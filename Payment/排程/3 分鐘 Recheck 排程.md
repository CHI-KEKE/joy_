
## SQL Job

```sql
WebStoreDB_Routine_ThirdPartyPaymentReCheck_HK

IF(select DBATools.dbo.cfn_GetJobWhetherToPerform('ReadWrite',0)) IN ('HK')
BEGIN
EXEC('
    DECLARE @now DATETIME = GETDATE();
    DECLARE @typeDefs VARCHAR(200) = ''CreditCardOnce_Stripe,CreditCardOnce_CheckoutDotCom,EWallet_PayMe,AliPayHK_EftPay,WechatPayHK_EftPay,BoCPay_SwiftPass,UnionPay_EftPay,Atome,TwoCTwoP,QFPay,CreditCardOnce_Cybersource'';
    DECLARE @startTime DATETIME = FORMAT(DATEADD(DAY, -3, @now),''yyyy-MM-dd HH:mm:00'');
    DECLARE @endTime DATETIME = FORMAT(DATEADD(MINUTE, -3, @now),''yyyy-MM-dd HH:mm:00'');

    EXEC WebStoreDB.[dbo].[csp_GenerateThirdPartyPaymentReCheck]
        @typeDefs,
        @startTime,
        @endTime
')
END
```

## csp

csp_GenerateThirdPartyPaymentReCheck
NMQ：ThirdPartyPaymentReCheck
撈 TradesOrderThirdPartyPaymentStatusEnum.WaitingToPay
每 3分鐘 撈前三天的

<br>

## Debug Context null問題

MWeb Log：SG-HK-QA1-MWeb2  E: log/Nlog PayChannel_InternalFinishPayment_2024071217

Cache = QA2 的資料
但NMQ 只處理QA1 ==> 找不到Context!!
怎知道哪一張TG?
MWeb-QA2 機器 E/ NLog 比對時間, k 值, TG
