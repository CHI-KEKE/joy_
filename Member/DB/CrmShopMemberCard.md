## CrmShopMemberCard

```sql
use CRMDB;
 
select CrmShopMemberCard_Id,CrmShopMemberCard_Name,CrmShopMemberCard_ShopId,CrmShopMemberCard_Level,*
from CrmShopMemberCard(nolock)
where CrmShopMemberCard_ShopId =5
order by CrmShopMemberCard.CrmShopMemberCard_ShopId,CrmShopMemberCard.CrmShopMemberCard_Level desc
```