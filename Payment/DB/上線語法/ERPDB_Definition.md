

## Q

```sql
-- Start: Verify
SELECT *
FROM ERPDB.dbo.Definition WITH(NOLOCK)
WHERE
	Definition_Code in ('CreditCardOnce_Razer','PayNow_Razer')
	AND Definition_ColumnName IN
	(
		'PaymentRequest_PayProfileTypeDef', 'PayProfile_TypeDef', 'PayProfile_StatisticsTypeDef', 'RefundRequest_PayTypeDef', 'RefundRequest_TypeDef', 
		'SalesOrderSlave_PayTypeDef', 'SalesOrderThirdPartyPayment_TypeDef'
	)
ORDER BY Definition_TableName, Definition_ColumnName
-- End: Verify
```
















## INSERT

```sql
USE ERPDB

DECLARE @code VARCHAR(30) = 'PayNow_Razer'
DECLARE @desc NVARCHAR(50) = 'PayNow'
DECLARE @user VARCHAR(50) = 'VSTS569269'
DECLARE @now DATETIME = GETDATE()

INSERT INTO ERPDB.dbo.Definition
(
	Definition_TableName, Definition_ColumnName, Definition_Code, Definition_Desc, Definition_Note,
	Definition_Sort, Definition_CreatedDateTime, Definition_CreatedUser, Definition_UpdatedTimes, Definition_UpdatedDateTime,
	Definition_UpdatedUser, Definition_ValidFlag
)
SELECT 
	Definition_TableName, Definition_ColumnName, @code, @desc, Definition_Note, 
	Definition_Sort + 3 AS 'Definition_Sort', @now, @user, 0, @now,
	@user, 1
FROM ERPDB.dbo.Definition WITH(NOLOCK)
WHERE
	Definition_Code = 'CreditCardInstallment_Razer'
	AND Definition_ColumnName IN
	(
		'PaymentRequest_PayProfileTypeDef', 'PayProfile_TypeDef', 'PayProfile_StatisticsTypeDef', 'RefundRequest_PayTypeDef', 'RefundRequest_TypeDef', 
		'SalesOrderSlave_PayTypeDef', 'SalesOrderThirdPartyPayment_TypeDef'
	)
ORDER BY Definition_TableName, Definition_ColumnName

-- Start: Verify
SELECT *
FROM ERPDB.dbo.Definition WITH(NOLOCK)
WHERE
	Definition_Code = @code
	AND Definition_ColumnName IN
	(
		'PaymentRequest_PayProfileTypeDef', 'PayProfile_TypeDef', 'PayProfile_StatisticsTypeDef', 'RefundRequest_PayTypeDef', 'RefundRequest_TypeDef', 
		'SalesOrderSlave_PayTypeDef', 'SalesOrderThirdPartyPayment_TypeDef'
	)
ORDER BY Definition_TableName, Definition_ColumnName
-- End: Verify
```