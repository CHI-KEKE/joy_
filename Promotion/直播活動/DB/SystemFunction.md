```sql
USE AuthExternalDB


SELECT SystemFunction_LanguageKey,SystemFunction_CloudLanguageKey,*
FROM SystemFunction(NOLOCK)
WHERE SystemFunction_ValidFlag = 1
and SystemFunction_Id = 876
```