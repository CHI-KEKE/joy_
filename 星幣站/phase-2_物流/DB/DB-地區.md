
## ShippingProfile

```sql
use ERPDB

-- DigitalDelivery / Home / LocationPickup / Oversea
select *
from ShippingProfile(nolock)
where ShippingProfile_ValidFlag = 1

```

## ShopShippingType

```sql
-- 設定的配送方式(自己套用溫層, 計價, profile)
use WebStoreDB
select *
from ShopShippingType(nolock)
where ShopShippingType_ValidFlag = 1

select *
from ShopShippingTypeIO
where ShopShippingTypeIO_ShopShippingTypeId = 517
```

## ShopShippingWeightProfile

```sql
-- 從 shopShippingType 攤開 重量與國家等等
select *
from ShopShippingWeightProfile
where ShopShippingWeightProfile_ShopShippingTypeId = 517
```

## ShippingArea

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

## CountryProfile


```sql
-- AliasCode : SG
-- CountryProfile_EnglishName　: Singapore
-- Id L 35
-- Code : 65
use WebStoreDB

select *
from ShippingArea(nolock)
where ShippingArea_ValidFlag = 1
and ShippingArea_CountryProfileId = 35

select *
from CountryProfile(nolock)
where CountryProfile_ValidFlag = 1
and CountryProfile_Id = 35
```

## MemberLocation


```sql
use WebStoreDB

select *
from MemberLocation(nolock)
where MemberLocation_ValidFlag = 1
and MemberLocation_ShopId = 83

order by MemberLocation_CreatedDateTime desc
```

### zipCode

```sql
select *
from ZipCode(nolock)
```