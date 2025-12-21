## 退款表

```sql
use ERPDB

select RefundRequest_ResponseMsg,RefundRequest_TransactionId,RefundRequest_CreatedDateTime,*
from RefundRequest(nolock)
where RefundRequest_ValidFlag = 1
and RefundRequest_Id in (121537,124652)
and RefundRequest_ResponseMsg like '%Business Rules Incorrect!%'
and RefundRequest_StatusDef in ('RefundRequestFail','RefundRequestGrouping')
and RefundRequest_UpdatedDateTime > '2025-07-01'
```

## ThirdPartyPayment

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

<br>

## SalePagePayType

```sql
use WebStoreDB

DECLARE @payType VARCHAR(30) = 'ApplePay';

SELECT SalePagePayType_SalePageId AS N'商品序號',
       SalePage_Title,
       SalePage_ShopId AS N'商店序號',
       SalePagePayType_CreatedDateTime AS N'金流套用時間',
       *
FROM SalePagePayType(nolock)
INNER JOIN SalePage(nolock)
    ON SalePagePayType_SalePageId = SalePage_Id
WHERE SalePagePayType_ValidFlag = 1
AND SalePage_ValidFlag = 1
AND SalePagePayType_TypeDef = 'ApplePay'

select *
from ShopCategory(nolock)
where ShopCategory_ValidFlag = 1
and ShopCategory_ShopId = 2
order by ShopCategory_CreatedDateTime desc
```

<br>

## SupplierPayType

```sql
select *
from SupplierPayType(nolock)
where SupplierPayType_SupplierId =2
```

<br>

## PayProfile

```sql
select *
from PayProfile(nolock)
```

<br>

## PayShippingMapping

```sql
select *
from PayShippingMapping(nolock)
```

<br>

## PayShippingDefault

```sql
select Shop_IsPayShippingDefault,*
from Shop(nolock)
where Shop_Id =44;
```

<br>

## Config DB

<br>

CSP.Payment.PayTypes

```sql
SELECT TOP (1000) [AppSetting_Id]
      ,[AppSetting_Key]
      ,[AppSetting_Value]
      ,[AppSetting_CreatedDateTime]
      ,[AppSetting_CreatedUser]
      ,[AppSetting_UpdatedTimes]
      ,[AppSetting_UpdatedDateTime]
      ,[AppSetting_UpdatedUser]
      ,[AppSetting_ValidFlag]
      ,[AppSetting_Rowversion]
      ,[AppSetting_Description]
  FROM [ConfigDB].[dbo].[AppSetting]
  where AppSetting_Key = 'CSP.Payment.PayTypes'
```









## ShopDefault

```sql
use WebStoreDB


select *
from ShopDefault(nolock)
where ShopDefault_GroupTypeDef = 'ShopFunction'
and ShopDefault_Key = 'IsRememberCreditCard'
```


## RefundRequest

```sql
use ERPDB

select *
from RefundRequest(nolock)
where RefundRequest_ValidFlag = 1
and RefundRequest_DateTime > '2025-11-20'
```
