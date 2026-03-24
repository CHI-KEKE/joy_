

## 排序

ORDER BY PayProfile_DisplaySort



## Q


```sql
USE ERPDB
SELECT *
FROM PayProfile(NOLOCK)
WHERE PayProfile_ValidFlag = 1
AND PayProfile_TypeDef = 'PayNow_Razer'
```



## currency


```sql
-- GrabPay_Razer
--　OnlineBanking_Razer
-- CreditCardOnce_Razer
use ERPDB

select PayProfile_Currency,PayProfile_TypeDef,PayProfile_StatisticsTypeDef,*
from PayProfile(nolock)
--where PayProfile_TypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')
--and PayProfile_StatisticsTypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')

update PayProfile
set PayProfile_Currency = 　N'["MYR","SGD"]'
where PayProfile_TypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')
and PayProfile_StatisticsTypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')

select PayProfile_Currency,PayProfile_TypeDef,PayProfile_StatisticsTypeDef,*
from PayProfile(nolock)
where PayProfile_TypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')
and PayProfile_StatisticsTypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')


```


## insert
```sql
USE ERPDB
GO

DECLARE @newPayProfileTypeDef VARCHAR(100) = 'PayNow_Razer'
DECLARE @note NVARCHAR(100) = ''
DECLARE @paymentMethodDef VARCHAR(30) = 'MobilePayment'
DECLARE @currency VARCHAR(200) = '["SGD"]'
DECLARE @now DATETIME = GETDATE();
DECLARE @user VARCHAR(50) = 'VSTS569269';
DECLARE @displaySort INT = 38;

INSERT INTO ERPDB.dbo.PayProfile
(
    PayProfile_TypeDef, PayProfile_StatisticsTypeDef, PayProfile_HasCvsContract, PayProfile_SupplierStoreProfileDistributorDef, PayProfile_DisplaySort,
    PayProfile_IsCheckEnabledSetting, PayProfile_CreatedDateTime, PayProfile_CreatedUser, PayProfile_UpdatedTimes, PayProfile_UpdatedDateTime,
    PayProfile_UpdatedUser, PayProfile_ValidFlag, PayProfile_SupplierFeeRate, PayProfile_BankFeeRate, PayProfile_IsThirdPartyPayment,
    PayProfile_Note, PayProfile_PaymentMethodDef, PayProfile_Currency
)
VALUES
(
    @newPayProfileTypeDef, @newPayProfileTypeDef, 0, '', @displaySort,
    0, @now, @user, 0, @now,
    @user, 1, 0, 0, 0,
    @note, @paymentMethodDef, @currency
)

-- Start: Verify
SELECT *
FROM ERPDB.dbo.PayProfile WITH(NOLOCK)
WHERE PayProfile_TypeDef = @newPayProfileTypeDef
-- End: Verify

GO
```