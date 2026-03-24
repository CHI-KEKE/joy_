

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



```sql
use WebStoreDB


select *
from ShopStaticSetting(nolock)
where ShopStaticSetting_ValidFlag = 1
and ShopStaticSetting_GroupName = 'PhoneValidation'
and ShopStaticSetting_Key = 'UseRegexByAliasCodes'


select *
from Shop(nolock)
where Shop_ValidFlag = 1
and Shop_Id = 200136
```


## Task


```sql
use NMQV2DB

select *
from Task(nolock)
inner join Job(nolock)
on Task_JobId = Job_Id
where Job_Name = 'SmsMessage' --SmsMessagePriorityHigh
and Task_ValidFlag = 1
and Job_ValidFlag = 1
order by Task_DispatchTime desc

```