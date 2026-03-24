```sql
use WebStoreDB
-- 配送地區的定義
select ShippingArea_Sort,ShippingArea_AliasCode,*
from ShippingArea(nolock)
where ShippingArea_ValidFlag = 1
--and ShippingArea_CreatedDateTime > '2025-11-21'
--AND ShippingArea_Name like '%Kuching%'
--and ShippingArea_CountryProfileId = 35

use ERPDB
-- 配送地區的定義
select ShippingArea_Sort,ShippingArea_AliasCode,*
from ShippingArea(nolock)
where ShippingArea_ValidFlag = 1
--and ShippingArea_CreatedDateTime > '2025-11-21'
--AND ShippingArea_Name like '%Kuching%'
--and ShippingArea_CountryProfileId = 35


--use ERPDB
--INSERT INTO ShippingArea (
--    ShippingArea_Sort,
--    ShippingArea_AliasCode,
--    ShippingArea_CountryProfileId,
--    ShippingArea_Name,
--    ShippingArea_CreatedDateTime,
--    ShippingArea_CreatedUser,
--    ShippingArea_UpdatedTimes,
--    ShippingArea_UpdatedDateTime,
--    ShippingArea_UpdatedUser,
--    ShippingArea_ValidFlag,
--    ShippingArea_SalesOrderFeeDefaultEnable,
--    ShippingArea_LanguageKey,
--    ShippingArea_DefaultSelectedEnable
--)
--VALUES (
--    1,
--    'singapore_sentosa',
--    35,
--    'Singapore Sentosa',
--    GETDATE(),
--    'VSTS546072',
--    0,
--    GETDATE(),
--    'VSTS546072',
--    1,
--    0,
--    'singapore_sentosa',
--    0
--);
```