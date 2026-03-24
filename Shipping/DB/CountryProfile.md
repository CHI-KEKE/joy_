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