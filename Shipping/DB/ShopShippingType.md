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