
## DB


```SQL
use WebStoreDB


select *
from ShopInstallment(nolock)
inner join Installment
on Installment_Id = ShopInstallment_InstallmentId
where ShopInstallment_ValidFlag = 1
```


## BankPayType

- 以 installmentId 為維度
- (一次性 or 分期) / 哪間銀行

Installment

- 期數
- 以 MY 需求來說，installment + bank 的維度, 所以同樣是 3 期會有很多 installmentId







```sql
-- shopInstallmentBank
select *
from ShopInstallmentBank (nolock)

-- iNSERT INTO ShopInstallmentBank
-- (
--     ShopInstallmentBank_ShopId,
--     ShopInstallmentBank_ShopInstallmentId,
--     ShopInstallmentBank_BankPayTypeId,
--     ShopInstallmentBank_AmtLimit,
--     ShopInstallmentBank_Rate,
--     ShopInstallmentBank_HasInterest,
--     ShopInstallmentBank_CreatedDateTime,
--     ShopInstallmentBank_CreatedUser,
--     ShopInstallmentBank_UpdatedTimes,
--     ShopInstallmentBank_UpdatedDateTime,
--     ShopInstallmentBank_UpdatedUser,
--     ShopInstallmentBank_ValidFlag
-- )
-- VALUES
-- (
--     32,                     -- ShopInstallmentBank_ShopId
--     204,                      -- ShopInstallmentBank_ShopInstallmentId
--     57,                       -- ShopInstallmentBank_BankPayTypeId
--     10,                       -- ShopInstallmentBank_AmtLimit
--     0.000,                     -- ShopInstallmentBank_Rate
--     0,                         -- ShopInstallmentBank_HasInterest
--     getdate(), -- ShopInstallmentBank_CreatedDateTime
--     'allenlin@nine-yi.com',    -- ShopInstallmentBank_CreatedUser
--     0,                         -- ShopInstallmentBank_UpdatedTimes
--     getdate(), -- ShopInstallmentBank_UpdatedDateTime
--     'allenlin@nine-yi.com',    -- ShopInstallmentBank_UpdatedUser
--     1                         -- ShopInstallmentBank_ValidFlag
-- );
-- --(
-- --    32,                     -- ShopInstallmentBank_ShopId
-- --    1,                      -- ShopInstallmentBank_ShopInstallmentId
-- --    58,                       -- ShopInstallmentBank_BankPayTypeId
-- --    10,                   -- ShopInstallmentBank_AmtLimit
-- --    0.000,                     -- ShopInstallmentBank_Rate
-- --    0,                         -- ShopInstallmentBank_HasInterest
-- --    getdate(), -- ShopInstallmentBank_CreatedDateTime
-- --    'allenlin@nine-yi.com',    -- ShopInstallmentBank_CreatedUser
-- --    0,                         -- ShopInstallmentBank_UpdatedTimes
-- --    getdate(), -- ShopInstallmentBank_UpdatedDateTime
-- --    'allenlin@nine-yi.com',    -- ShopInstallmentBank_UpdatedUser
-- --    1                         -- ShopInstallmentBank_ValidFlag
-- --);

-- shopInstallment
select *
from ShopInstallment

-- INSERT INTO ShopInstallment
-- (
--     ShopInstallment_SupplierId,
--     ShopInstallment_ShopId,
--     ShopInstallment_InstallmentId,--
--     ShopInstallment_InstallmentDef,--
--     ShopInstallment_InstallmentRate,--
--     ShopInstallment_HasInterest,--
--     ShopInstallment_CreatedDateTime,
--     ShopInstallment_CreatedUser,
--     ShopInstallment_UpdatedTimes,
--     ShopInstallment_UpdatedDateTime,
--     ShopInstallment_UpdatedUser,
--     ShopInstallment_ValidFlag
-- )
-- VALUES
-- (
--     34,
--     32,
--     3,
--     12,
--     0.000,
--     0,
--     getdate(),
--     'allenlin@nine-yi.com',
--     0,
--     getdate(),
--     'allenlin@nine-yi.com',
--     1
-- )
--,
--(
--    100,
--    100,
--    2,
--    6,
--    0.000,
--    0,
--    getdate(),
--    'allenlin@nine-yi.com',
--    0,
--    getdate(),
--    'allenlin@nine-yi.com',
--    1
--);

--Bank
select *
from Bank

-- BankPaytype
select BankPayType_BankName,STRING_AGG(BankPayType_InstallmentDef,',') as N'支援期數'
from BankPayType(nolock)
WHERE BankPayType_ValidFlag = 1
group by BankPayType_BankName


--join all
use WebStoreDB

declare @shopId bigint = 32;

SELECT 
    si.ShopInstallment_Id           AS ShopInstallmentId,
    i.Installment_Id                AS InstallmentId,
    si.ShopInstallment_InstallmentDef AS ShopInstallmentDef,
    sib.ShopInstallmentBank_AmtLimit AS ShopInstallmentBankAmtLimit,
    sib.ShopInstallmentBank_Rate     AS ShopInstallmentBankRate,
    sib.ShopInstallmentBank_HasInterest AS ShopInstallmentBankHasInterest,
    bpt.BankPayType_BankName        AS ShopInstallmentBankName
FROM ShopInstallmentBank sib
INNER JOIN ShopInstallment si
    ON sib.ShopInstallmentBank_ShopInstallmentId = si.ShopInstallment_Id
INNER JOIN Installment i
    ON si.ShopInstallment_InstallmentId = i.Installment_Id
INNER JOIN BankPayType bpt
    ON sib.ShopInstallmentBank_BankPayTypeId = bpt.BankPayType_Id
WHERE i.Installment_StatusDef = 1
  AND i.Installment_ValidFlag = 1
  AND si.ShopInstallment_ShopId = @shopId
  AND si.ShopInstallment_ValidFlag = 1
  AND sib.ShopInstallmentBank_ShopId = @shopId
  AND sib.ShopInstallmentBank_ValidFlag = 1
  AND bpt.BankPayType_StatusDef = 1
  AND bpt.BankPayType_ValidFlag = 1;


  SELECT 
    si.ShopInstallment_Id AS ShopInstallmentId,
    i.Installment_Id AS InstallmentId,
    si.ShopInstallment_InstallmentDef AS ShopInstallmentDef,
    sib.ShopInstallmentBank_AmtLimit AS ShopInstallmentBankAmtLimit,
    sib.ShopInstallmentBank_Rate AS ShopInstallmentBankRate,
    sib.ShopInstallmentBank_HasInterest AS ShopInstallmentBankHasInterest,
    bpt.BankPayType_BankName AS ShopInstallmentBankName
FROM (
    SELECT *
    FROM [ShopInstallmentBank] AS [sib]
    WHERE [sib].[ShopInstallmentBank_ShopId] = @shopId
      AND [sib].[ShopInstallmentBank_ValidFlag] = 1
) AS [sib]
INNER JOIN (
    SELECT *
    FROM [ShopInstallment] AS [si]
    WHERE [si].[ShopInstallment_ShopId] = @shopId
      AND [si].[ShopInstallment_ValidFlag] = 1
) AS [si] 
    ON [sib].[ShopInstallmentBank_ShopInstallmentId] = [si].[ShopInstallment_Id]
INNER JOIN (
    SELECT *
    FROM [Installment] AS [i]
    WHERE [i].[Installment_StatusDef] = 1
      AND [i].[Installment_ValidFlag] = 1
) AS [i]
    ON [si].[ShopInstallment_InstallmentId] = [i].[Installment_Id]
INNER JOIN (
    SELECT *
    FROM [BankPayType] AS [bpt]
    WHERE [bpt].[BankPayType_StatusDef] = 1
      AND [bpt].[BankPayType_ValidFlag] = 1
) AS [bpt]
    ON [sib].[ShopInstallmentBank_BankPayTypeId] = [bpt].[BankPayType_Id];


-- salepagePaytype
select SalePagePayType_ValidFlag,SalePagePayType_TypeDef,*
from SalePagePayType(nolock)
where SalePagePayType_SalePageId = 6036

--shopPaytype
select *
from ShopPayType
where ShopPayType_TypeDef = 'CreditCardInstallment_Razer'
--where PayProfile_TypeDef = 'CreditCardInstallment_Razer'

--INSERT INTO ShopPayType
--(
--    ShopPayType_SupplierId,
--    ShopPayType_ShopId,
--    ShopPayType_TypeDef,
--    ShopPayType_IsEnabled,
--    ShopPayType_CreatedDateTime, -- 
--    ShopPayType_CreatedUser, --
--    ShopPayType_UpdatedTimes, --
--    ShopPayType_UpdatedDateTime, --
--    ShopPayType_UpdatedUser, --
--    ShopPayType_ValidFlag, -- 
--    ShopPayType_PayProfileTypeDef
--)
--VALUES
--(
--    34,
--    32,
--    'CreditCardInstallment_Razer',
--    1,
--    getdate(),
--    'allenlin@nine-yi.com',
--    0,
--    getdate(),
--    'allenlin@nine-yi.com',
--    1,
--    'CreditCardInstallment_Razer'
--);

--supplierPaytype
select *
from SupplierPayType
where SupplierPayType_ValidFlag = 1
and SupplierPayType_TypeDef = 'CreditCardInstallment_Razer'


--INSERT INTO SupplierPayType
--(
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
--)
--VALUES
--(
--    'CreditCardInstallment_Razer',
--    34,
--    1,
--    0,
--    getdate(),
--    'allenlin@nine-yi.com',
--    1,
--    getdate(),
--    'allenlin@nine-yi.com',
--    1,
--    'CreditCardInstallment_Razer'
--);



--PayShippingMapping
select *
from PayShippingMapping



```
