

## 說明

商店需要有開店的預設金流


```sql
select *
from ShopPayShippingDefault(nolock)
where ShopPayShippingDefault_ValidFlag = 1
and ShopPayShippingDefault_ShopId = 83
```

## 更新預設金流

```sql
/**
VSTS547913 - MY 更新測試店預設金流 - ShopId: 200136
Step 4-1: 更新 ShopPayShippingDefault

目的: 將商店 200136 的預設金流從 OnlineBanking_Razer 改為 CreditCardOnce_Razer
**/

USE WebStoreDB
GO

-- General
DECLARE @user VARCHAR(50) = 'VSTS547913';
DECLARE @date DATETIME = GETDATE();

-- Shop 資訊
DECLARE @shopId BIGINT = 200136;
DECLARE @supplierId INT;

-- 金流設定
DECLARE @oldPayType VARCHAR(30) = 'OnlineBanking_Razer';
DECLARE @newPayType VARCHAR(30) = 'CreditCardOnce_Razer';

-- 查詢 SupplierId
SELECT
	@supplierId = Shop_SupplierId
FROM dbo.Shop WITH (NOLOCK)
WHERE Shop_Id = @shopId
  AND Shop_ValidFlag = 1;

-- 顯示取得的 SupplierId
SELECT @shopId AS ShopId, @supplierId AS SupplierId, @oldPayType AS OldPayType, @newPayType AS NewPayType;

-- 查詢目前的 ShopPayShippingDefault 設定
SELECT ShopPayShippingDefault_ShopId,
   ShopPayShippingDefault_SupplierId,
   ShopPayShippingDefault_PayProfileTypeDef,
   *
FROM dbo.ShopPayShippingDefault WITH (NOLOCK)
WHERE ShopPayShippingDefault_ShopId = @shopId
  AND ShopPayShippingDefault_SupplierId = @supplierId
  AND ShopPayShippingDefault_ValidFlag = 1;

-- Backup
SELECT
	* INTO MATempDB.dbo.tmpWebStoreDB_ShopPayShippingDefault_VSTS547913_shop_200136
FROM dbo.ShopPayShippingDefault WITH (NOLOCK)
WHERE ShopPayShippingDefault_ShopId = @shopId
 AND ShopPayShippingDefault_SupplierId = @supplierId;

-- Update ShopPayShippingDefault
UPDATE dbo.ShopPayShippingDefault
SET ShopPayShippingDefault_PayProfileTypeDef = @newPayType,
   ShopPayShippingDefault_UpdatedTimes = ShopPayShippingDefault_UpdatedTimes % 255 + 1,
   ShopPayShippingDefault_UpdatedDateTime = @date,
   ShopPayShippingDefault_UpdatedUser = @user
WHERE ShopPayShippingDefault_ShopId = @shopId
 AND ShopPayShippingDefault_SupplierId = @supplierId
 AND ShopPayShippingDefault_PayProfileTypeDef = @oldPayType
 AND ShopPayShippingDefault_ValidFlag = 1;

-- Verify
SELECT ShopPayShippingDefault_PayProfileTypeDef,
   ShopPayShippingDefault_UpdatedUser,
   ShopPayShippingDefault_UpdatedDateTime,
   ShopPayShippingDefault_UpdatedTimes,
   *
FROM dbo.ShopPayShippingDefault WITH (NOLOCK)
WHERE ShopPayShippingDefault_ShopId = @shopId
  AND ShopPayShippingDefault_SupplierId = @supplierId
  AND ShopPayShippingDefault_PayProfileTypeDef = @newPayType
  AND ShopPayShippingDefault_ValidFlag = 1;

-- 確認舊金流已無資料 (應該沒有結果)
SELECT *
FROM dbo.ShopPayShippingDefault WITH (NOLOCK)
WHERE ShopPayShippingDefault_ShopId = @shopId
  AND ShopPayShippingDefault_SupplierId = @supplierId
  AND ShopPayShippingDefault_PayProfileTypeDef = @oldPayType
  AND ShopPayShippingDefault_ValidFlag = 1;

GO
```