

## 外部點數

```sql
use LoyaltyDB


select top 5 LoyaltyPointProfile_IsExternalCalculate,*
from LoyaltyPointProfile(nolock)
where LoyaltyPointProfile_ValidFlag = 1
```


## 外部點數商店

```sql
select top 5 *
from LoyaltyPointProfile(nolock)
where LoyaltyPointProfile_ShopId = 2
and LoyaltyPointProfile_StatusDef = 'Processing'
```