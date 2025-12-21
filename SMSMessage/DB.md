

## CountryProfile


```sql
use ERPDB

select CountryProfile_Enabled,*
from CountryProfile(nolock)
where CountryProfile_ValidFlag = 1
and CountryProfile_AliasCode = 'SG'

use CRMDB

select CountryProfile_Enabled,*
from CountryProfile(nolock)
where CountryProfile_ValidFlag = 1
and CountryProfile_AliasCode = 'SG'

use WebStoreDB

select CountryProfile_Enabled,CountryProfile_CellPhoneValidationPattern,*
from CountryProfile(nolock)
where CountryProfile_ValidFlag = 1
and CountryProfile_AliasCode = 'SG'

--update
USE CRMDB

SELECT CountryProfile_CellPhoneValidationPattern,*
FROM CountryProfile(NOLOCK)
WHERE CountryProfile_ValidFlag = 1
AND CountryProfile_AliasCode = 'SG'

update CountryProfile
set CountryProfile_CellPhoneValidationPattern = N'^[89]\d{7}$'
WHERE CountryProfile_ValidFlag = 1
AND CountryProfile_AliasCode = 'SG'

USE CRMDB
SELECT CountryProfile_CellPhoneValidationPattern,CountryProfile_Enabled,*
FROM CountryProfile(NOLOCK)
WHERE CountryProfile_ValidFlag = 1
AND CountryProfile_AliasCode = 'SG'
```