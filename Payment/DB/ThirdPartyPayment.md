```sql
USE WebStoreDB

	SELECT *
		FROM dbo.TradesOrderThirdPartyPayment WITH (NOLOCK)
		WHERE TradesOrderThirdPartyPayment_ValidFlag = 1
		AND TradesOrderThirdPartyPayment_TradesOrderGroupCode = 'TG250820W00092'
		AND TradesOrderThirdPartyPayment_ValidFlag = 1
```

```sql
USE WebStoreDB

declare @startTime DATETIME = '2025-01-01';
declare @endTime DATETIME = '2025-05-01';

	SELECT *
		FROM dbo.TradesOrderThirdPartyPayment WITH (NOLOCK)
		WHERE TradesOrderThirdPartyPayment_ValidFlag = 1
		AND TradesOrderThirdPartyPayment_TradesOrderGroupCode = 'TG250626K00002'
		and TradesOrderThirdPartyPayment_TypeDef IN ('CreditCardOnce_Stripe','CreditCardOnce_CheckoutDotCom','EWallet_PayMe','AliPayHK_EftPay','WechatPayHK_EftPay','BoCPay_SwiftPass','UnionPay_EftPay','Atome','TwoCTwoP','QFPay','CreditCardOnce_Cybersource')
		AND TradesOrderThirdPartyPayment_StatusDef IN('New', 'WaitingToPay')
		AND TradesOrderThirdPartyPayment_StatusUpdatedDateTime > @startTime
		AND TradesOrderThirdPartyPayment_StatusUpdatedDateTime <= @endTime
		AND TradesOrderThirdPartyPayment_ValidFlag = 1
```
