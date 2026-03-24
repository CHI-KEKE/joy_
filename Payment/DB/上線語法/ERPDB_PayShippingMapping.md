



## Q

```sql
SELECT *
FROM PayShippingMapping(NOLOCK)
WHERE PayShippingMapping_ValidFlag = 1
AND PayShippingMapping_PayProfileTypeDef = 'PayNow_Razer'
```


## INSERT

```sql
USE ERPDB
GO

DECLARE @newPayProfileTypeDef VARCHAR(100) = 'PayNow_Razer'
DECLARE @now DATETIME = GETDATE();
DECLARE @user VARCHAR(50) = 'VSTS569269';

INSERT INTO ERPDB.dbo.PayShippingMapping
(
    PayShippingMapping_PayProfileTypeDef, 
    PayShippingMapping_ShippingProfileTypeDef, 
    PayShippingMapping_CreatedDateTime, 
    PayShippingMapping_CreatedUser, 
    PayShippingMapping_UpdatedTimes,
    PayShippingMapping_UpdatedDateTime, 
    PayShippingMapping_UpdatedUser, 
    PayShippingMapping_ValidFlag
)
SELECT 
    @newPayProfileTypeDef, 
    ShippingProfile_TypeDef, 
    @now, 
    @user, 
    0,
    @now, 
    @user, 
    1
FROM ERPDB.dbo.ShippingProfile WITH(NOLOCK)
WHERE ShippingProfile_ValidFlag = 1 AND ShippingProfile_TypeDef <> 'CashOnDelivery'
ORDER BY ShippingProfile_Id

GO
```