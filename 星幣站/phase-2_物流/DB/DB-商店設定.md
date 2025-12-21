

## Supplier


```SQL
--SupplierContact_SupplierId = 79
use ERPDB
select *
from Supplier(nolock)

use WebStore
select *
from Supplier(nolock)

select Supplier_SalesMarket,*
from Supplier(nolock)
WHERE Supplier_SalesMarket = 'SG'
```

## Shop

```sql
--use WebStoreDB
--select *
--from Shop(nolock)
--where Shop_ValidFlag = 1
--and Shop_Id = 83

----測試店_莉莉測試測試測試測試測試測試測試測試店
--update Shop
--set Shop_Name = N'Singapore'
--where Shop_ValidFlag = 1
--and Shop_Id = 83

--use WebStoreDB
--select *
--from Shop(nolock)
--where Shop_ValidFlag = 1
--and Shop_Id = 83
```

## ShopDefault

```sql
-- GlobalShipping : PickCountry,EasyParcelAPIKey
use WebStoreDB


select *
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_GroupTypeDef = 'GlobalShipping'
and ShopDefault_ShopId = 83

use WebStoreDB
-- courier country

select *
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_GroupTypeDef = 'GlobalShipping'
and ShopDefault_Key = 'PickCountry'
and ShopDefault_ShopId = 83


--update ShopDefault
--set ShopDefault_NewValue = 'SG',
--ShopDefault_Value = 'SG'
--where ShopDefault_ValidFlag = 1
--and ShopDefault_GroupTypeDef = 'GlobalShipping'
--and ShopDefault_Key = 'PickCountry'
--and ShopDefault_ShopId = 83

--use WebStoreDB
---- courier country

--select *
--from ShopDefault(nolock)
--where ShopDefault_ValidFlag = 1
--and ShopDefault_GroupTypeDef = 'GlobalShipping'
--and ShopDefault_Key = 'PickCountry'
--and ShopDefault_ShopId = 83

USE WebStoreDB
select *
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_GroupTypeDef = 'GlobalShipping'
and ShopDefault_Key = 'EasyParcelAPIKey'
and ShopDefault_ShopId = 83
```