```sql
use ERPDB

-- DigitalDelivery / Home / LocationPickup / Oversea
select *
from ShippingProfile(nolock)
where ShippingProfile_ValidFlag = 1

```