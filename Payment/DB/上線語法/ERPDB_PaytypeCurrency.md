

## 說明

基本上就是更新 ERPDB 的 PayProfile_PayProfile_Currency，前台會 Sync 過去


## 既有 Paytype

```sql
/**
VSTS547913 - 更新 PayProfile_Currency 從 ["MYR"] 變成 ["MYR","SGD"]

目的: CreditCardOnce_Razer, GrabPay_Razer 支援 SGD 幣別

**/

USE ERPDB
GO

-- DECLARE
DECLARE @user VARCHAR(50) = 'VSTS547913', 
        @dateTime DATETIME = GETDATE(),
        @fromCurrency VARCHAR(100) = '["MYR"]',
        @toCurrency VARCHAR(100) = '["MYR","SGD"]',
        @payTypeDef1 VARCHAR(30) = 'CreditCardOnce_Razer',
        @payTypeDef2 VARCHAR(30) = 'GrabPay_Razer';

-- 查詢 (目前符合 @fromCurrency 的資料)
SELECT 
    PayProfile_Id,
    PayProfile_TypeDef,
    PayProfile_Currency,
    PayProfile_ValidFlag
FROM dbo.PayProfile WITH(NOLOCK)
WHERE PayProfile_ValidFlag = 1
  AND PayProfile_Currency = @fromCurrency
  AND PayProfile_TypeDef IN (@payTypeDef1, @payTypeDef2)

-- Backup
SELECT *
INTO MATempDB.dbo.tmpERPDB_PayProfile_VSTS547913
FROM dbo.PayProfile WITH(NOLOCK)
WHERE PayProfile_ValidFlag = 1

-- Update
UPDATE dbo.PayProfile
SET 
    PayProfile_Currency = @toCurrency,
    PayProfile_UpdatedTimes = PayProfile_UpdatedTimes % 255 + 1,
    PayProfile_UpdatedDateTime = @dateTime,
    PayProfile_UpdatedUser = @user
WHERE PayProfile_ValidFlag = 1
  AND PayProfile_Currency = @fromCurrency
  AND PayProfile_TypeDef IN (@payTypeDef1, @payTypeDef2)

-- Verify 1 : 預期更新為 ["MYR","SGD"]
SELECT 
    PayProfile_Id,
    PayProfile_TypeDef,
    PayProfile_Currency,
    PayProfile_UpdatedDateTime,
    PayProfile_UpdatedUser,
    PayProfile_ValidFlag
FROM dbo.PayProfile WITH(NOLOCK)
WHERE PayProfile_Currency = @toCurrency
  AND PayProfile_ValidFlag = 1
  AND PayProfile_TypeDef IN (@payTypeDef1, @payTypeDef2)

-- Verify 2 : 預期更新為空結果
SELECT 
    PayProfile_Id,
    PayProfile_TypeDef,
    PayProfile_Currency,
    PayProfile_ValidFlag
FROM dbo.PayProfile WITH(NOLOCK)
WHERE PayProfile_Currency = @fromCurrency
  AND PayProfile_ValidFlag = 1
  AND PayProfile_TypeDef IN (@payTypeDef1, @payTypeDef2)
```


## 全新 Paytype

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