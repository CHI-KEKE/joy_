```sql
USE WebStoreDB

DECLARE @newPayProfile VARCHAR(30) = 'QFPay';
DECLARE @now DATETIME = GETDATE();
DECLARE @user VARCHAR(20) = 'VSTS397090';

INSERT INTO dbo.TradesOrderSlaveProgressBar
(
	TradesOrderSlaveProgressBar_FlowTypeDef,
	TradesOrderSlaveProgressBar_PayProfileTypeDef,
	TradesOrderSlaveProgressBar_ShippingProfileTypeDef,
	TradesOrderSlaveProgressBar_StatusDef,
	TradesOrderSlaveProgressBar_StatusDefDesc,
	TradesOrderSlaveProgressBar_DisplaySort,
	TradesOrderSlaveProgressBar_DisplayVersion,
	TradesOrderSlaveProgressBar_CreatedDateTime,
	TradesOrderSlaveProgressBar_CreatedUser,
	TradesOrderSlaveProgressBar_UpdatedTimes,
	TradesOrderSlaveProgressBar_UpdatedDateTime,
	TradesOrderSlaveProgressBar_UpdatedUser,
	TradesOrderSlaveProgressBar_ValidFlag,
	TradesOrderSlaveProgressBar_IsLogisticCenter
)
SELECT 
	TradesOrderSlaveProgressBar_FlowTypeDef,
	@newPayProfile,
	TradesOrderSlaveProgressBar_ShippingProfileTypeDef,
	TradesOrderSlaveProgressBar_StatusDef,
	TradesOrderSlaveProgressBar_StatusDefDesc,
	TradesOrderSlaveProgressBar_DisplaySort,
	TradesOrderSlaveProgressBar_DisplayVersion,
	@now,
	@user,
	0,
	@now,
	@user,
	1,
	TradesOrderSlaveProgressBar_IsLogisticCenter
FROM WebStoreDB.dbo.TradesOrderSlaveProgressBar WITH(NOLOCK)
WHERE 
	TradesOrderSlaveProgressBar_PayProfileTypeDef = 'CreditCardOnce_Stripe'
	AND TradesOrderSlaveProgressBar_ValidFlag = 1
ORDER BY TradesOrderSlaveProgressBar_PayProfileTypeDef, TradesOrderSlaveProgressBar_ShippingProfileTypeDef, TradesOrderSlaveProgressBar_DisplaySort;


--Verfy
SELECT TOP (1000) *
FROM WebStoreDB.dbo.TradesOrderSlaveProgressBar WITH(NOLOCK)
WHERE 
	TradesOrderSlaveProgressBar_PayProfileTypeDef = @newPayProfile
	AND TradesOrderSlaveProgressBar_ValidFlag = 1
ORDER BY TradesOrderSlaveProgressBar_PayProfileTypeDef, TradesOrderSlaveProgressBar_ShippingProfileTypeDef, TradesOrderSlaveProgressBar_DisplaySort
```