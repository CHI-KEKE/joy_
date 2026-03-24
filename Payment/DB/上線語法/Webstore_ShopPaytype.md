
```sql
SELECT *
FROM ShopPayType(NOLOCK)
WHERE ShopPayType_ValidFlag = 1
AND ShopPayType_ShopId = 83

```



```sql
/**
VSTS547913 - MY 更新測試店預設金流 - ShopId: 200136
Step 4-2: 更新 ShopPayType

目的: 將商店 200136 的金流從 OnlineBanking_Razer 改為 CreditCardOnce_Razer
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

-- 查詢 SupplierId
SELECT
	@supplierId = Shop_SupplierId
FROM dbo.Shop WITH (NOLOCK)
WHERE Shop_Id = @shopId
  AND Shop_ValidFlag = 1;

-- 顯示取得的 SupplierId
SELECT @shopId AS ShopId, @supplierId AS SupplierId, @oldPayType AS OldPayType;

-- 查詢目前的 ShopPayType 設定
SELECT ShopPayType_ShopId,
   ShopPayType_SupplierId,
   ShopPayType_TypeDef,
   ShopPayType_PayProfileTypeDef,
   *
FROM dbo.ShopPayType WITH (NOLOCK)
WHERE ShopPayType_ShopId = @shopId
  AND ShopPayType_SupplierId = @supplierId
  AND ShopPayType_ValidFlag = 1;

-- Backup
SELECT
	* INTO MATempDB.dbo.tmpWebStoreDB_ShopPayType_VSTS547913_shop_200136
FROM dbo.ShopPayType WITH (NOLOCK)
WHERE ShopPayType_ShopId = @shopId
  AND ShopPayType_SupplierId = @supplierId;

-- Update ShopPayType
UPDATE dbo.ShopPayType
SET ShopPayType_ValidFlag = 0,
    ShopPayType_UpdatedTimes = ShopPayType_UpdatedTimes % 255 + 1,
    ShopPayType_UpdatedDateTime = @date,
    ShopPayType_UpdatedUser = @user
WHERE ShopPayType_ShopId = @shopId
  AND ShopPayType_SupplierId = @supplierId
  AND ShopPayType_TypeDef = @oldPayType
  AND ShopPayType_PayProfileTypeDef = @oldPayType
  AND ShopPayType_ValidFlag = 1;

-- 確認舊金流已無資料 (應該沒有結果)
SELECT *
FROM dbo.ShopPayType WITH (NOLOCK)
WHERE ShopPayType_ShopId = @shopId
  AND ShopPayType_SupplierId = @supplierId
  AND ShopPayType_TypeDef = @oldPayType
  AND ShopPayType_ValidFlag = 1;

GO

```