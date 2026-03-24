

開店會有有一個預設的金流(Initial 那邊)，目前 MY 是 OnlineBanking，SG 商店要先壓成 CreditCardOnce，後續的新增金流就只要有加上 Payprofile 就可以在後台付款 / 配送方式新增


```sql
USE WebStoreDB

SELECT *
FROM SupplierPayType(NOLOCK)
WHERE SupplierPayType_ValidFlag =1
AND SupplierPayType_Id = 79
```


## 更新

```sql
/**
VSTS547913 - MY 更新測試店預設金流 - ShopId: 200136
Step 4-3: 更新 SupplierPayType

目的: 將廠商的金流從 OnlineBanking_Razer 改為 CreditCardOnce_Razer
**/

USE WebStoreDB
GO

-- General
DECLARE @user NVARCHAR(50) = 'VSTS547913';
DECLARE @date DATETIME = GETDATE();

-- Shop 資訊
DECLARE @shopId BIGINT = 83;
DECLARE @supplierId INT;

-- 金流設定
DECLARE @oldPayType VARCHAR(50) = 'OnlineBanking_Razer';

-- 查詢 SupplierId
SELECT
	@supplierId = Shop_SupplierId
FROM dbo.Shop WITH (NOLOCK)
WHERE Shop_Id = @shopId
  AND Shop_ValidFlag = 1;

-- 顯示取得的 SupplierId
SELECT @shopId AS ShopId, @supplierId AS SupplierId, @oldPayType AS OldPayType;

-- 查詢目前的 SupplierPayType 設定
SELECT SupplierPayType_SupplierId,
   SupplierPayType_TypeDef,
   SupplierPayType_PayProfileTypeDef,
   *
FROM dbo.SupplierPayType WITH (NOLOCK)
WHERE SupplierPayType_SupplierId = @supplierId
  AND SupplierPayType_ValidFlag = 1;

-- Backup
SELECT
	* INTO MATempDB.dbo.tmpWebStoreDB_SupplierPayType_VSTS547913_shop_200136
FROM dbo.SupplierPayType WITH (NOLOCK)
WHERE SupplierPayType_SupplierId = @supplierId;

-- Update SupplierPayType
UPDATE dbo.SupplierPayType
SET SupplierPayType_ValidFlag = 0,
    SupplierPayType_UpdatedTimes = SupplierPayType_UpdatedTimes % 255 + 1,
    SupplierPayType_UpdatedDateTime = @date,
    SupplierPayType_UpdatedUser = @user
WHERE SupplierPayType_SupplierId = @supplierId
  AND SupplierPayType_TypeDef = @oldPayType
  AND SupplierPayType_PayProfileTypeDef = @oldPayType
  AND SupplierPayType_ValidFlag = 1;

-- Verify
SELECT SupplierPayType_TypeDef,
   SupplierPayType_PayProfileTypeDef,
   SupplierPayType_UpdatedUser,
   SupplierPayType_UpdatedDateTime,
   SupplierPayType_UpdatedTimes,
   *
FROM dbo.SupplierPayType WITH (NOLOCK)
WHERE SupplierPayType_SupplierId = @supplierId
  AND SupplierPayType_ValidFlag = 1;

-- 確認舊金流已無資料 (應該沒有結果)
SELECT *
FROM dbo.SupplierPayType WITH (NOLOCK)
WHERE SupplierPayType_SupplierId = @supplierId
  AND SupplierPayType_TypeDef = @oldPayType
  AND SupplierPayType_ValidFlag = 1;

GO

```


## 新增


```sql
/**
VSTS547913 - 新增 SupplierPayType 資料

目的: 為 Supplier 200105 新增 CreditCardOnce_Razer 金流設定
**/

USE WebStoreDB
GO

-- DECLARE
DECLARE @user NVARCHAR(50) = 'VSTS547913';
DECLARE @date DATETIME = GETDATE();

-- Supplier 資訊
DECLARE @supplierId INT = 123;

-- 金流設定
DECLARE @payTypeDef VARCHAR(50) = 'CreditCardOnce_Razer';
DECLARE @payProfileTypeDef VARCHAR(50) = 'CreditCardOnce_Razer'; -- 還有 GrabPay_Razer
DECLARE @isEnabled BIT = 1;
DECLARE @isDefault BIT = 1;
DECLARE @validFlag BIT = 1;

-- 顯示參數
SELECT @supplierId AS SupplierId, @payTypeDef AS PayTypeDef, @payProfileTypeDef AS PayProfileTypeDef;

-- 查詢是否已存在相同的 SupplierPayType
SELECT 
   SupplierPayType_Id,
   SupplierPayType_SupplierId,
   SupplierPayType_TypeDef,
   SupplierPayType_PayProfileTypeDef,
   SupplierPayType_IsEnabled,
   SupplierPayType_IsDefault,
   SupplierPayType_ValidFlag
FROM dbo.SupplierPayType WITH (NOLOCK)
WHERE SupplierPayType_SupplierId = @supplierId
  AND SupplierPayType_TypeDef = @payTypeDef
  AND SupplierPayType_PayProfileTypeDef = @payProfileTypeDef;

-- 確認沒有重複資料後,取消下方註解執行 INSERT
-- INSERT INTO dbo.SupplierPayType
-- (
--     SupplierPayType_TypeDef,
--     SupplierPayType_SupplierId,
--     SupplierPayType_IsEnabled,
--     SupplierPayType_IsDefault,
--     SupplierPayType_CreatedDateTime,
--     SupplierPayType_CreatedUser,
--     SupplierPayType_UpdatedTimes,
--     SupplierPayType_UpdatedDateTime,
--     SupplierPayType_UpdatedUser,
--     SupplierPayType_ValidFlag,
--     SupplierPayType_PayProfileTypeDef
-- )
-- VALUES
-- (
--     @payTypeDef,                -- SupplierPayType_TypeDef
--     @supplierId,                -- SupplierPayType_SupplierId
--     @isEnabled,                 -- SupplierPayType_IsEnabled
--     @isDefault,                 -- SupplierPayType_IsDefault
--     @date,                      -- SupplierPayType_CreatedDateTime
--     @user,                      -- SupplierPayType_CreatedUser
--     0,                          -- SupplierPayType_UpdatedTimes
--     @date,                      -- SupplierPayType_UpdatedDateTime
--     @user,                      -- SupplierPayType_UpdatedUser
--     @validFlag,                 -- SupplierPayType_ValidFlag
--     @payProfileTypeDef          -- SupplierPayType_PayProfileTypeDef
-- );

--  --驗證新增結果
-- SELECT 
--    SupplierPayType_Id,
--    SupplierPayType_TypeDef,
--    SupplierPayType_SupplierId,
--    SupplierPayType_IsEnabled,
--    SupplierPayType_IsDefault,
--    SupplierPayType_CreatedDateTime,
--    SupplierPayType_CreatedUser,
--    SupplierPayType_UpdatedTimes,
--    SupplierPayType_UpdatedDateTime,
--    SupplierPayType_UpdatedUser,
--    SupplierPayType_ValidFlag,
--    SupplierPayType_PayProfileTypeDef
-- FROM dbo.SupplierPayType WITH (NOLOCK)
-- WHERE SupplierPayType_SupplierId = @supplierId
--   AND SupplierPayType_TypeDef = @payTypeDef
--   AND SupplierPayType_PayProfileTypeDef = @payProfileTypeDef
-- ORDER BY SupplierPayType_Id DESC;


-- GO

```