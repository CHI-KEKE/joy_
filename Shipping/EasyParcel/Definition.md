```sql
use WebStoreDB

select Definition_Note,*
from Definition(nolock)
where Definition_ValidFlag = 1
and Definition_TableName = 'ShippingOrder'
and Definition_ColumnName = 'ShippingOrder_ForwarderDef'
and Definition_Desc = 'Easyparcel'
and Definition_Code = 9
```