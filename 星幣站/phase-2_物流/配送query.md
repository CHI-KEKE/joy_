
```sql
use ERPDB


-- DigitalDelivery / Home / LocationPickup / Oversea
select *
from ShippingProfile(nolock)
where ShippingProfile_ValidFlag = 1

-- 設定的配送方式(自己套用溫層, 計價, profile)
use WebStoreDB
select *
from ShopShippingType(nolock)
where ShopShippingType_ValidFlag = 1

select *
from ShopShippingTypeIO
where ShopShippingTypeIO_ShopShippingTypeId = 517

-- 從 shopShippingType 攤開 重量與國家等等
select *
from ShopShippingWeightProfile
where ShopShippingWeightProfile_ShopShippingTypeId = 517

-- 配送地區的定義
select ShippingArea_Sort,*
from ShippingArea(nolock)
where ShippingArea_ValidFlag = 1
and ShippingArea_CreatedDateTime > '2025-11-21'
--where ShippingArea_Name like '%Malaysia%'
```