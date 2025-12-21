

shopId = 200136
supplierId = 200105

## Supplier


```sql
-- SalesCurrency => SGD
use ERPDB

select Shop_SupplierId,*
from Shop(nolock)
where Shop_Id = 200136

select Supplier_SalesCurrency,Supplier_PayTypeDef,*
from Supplier(nolock)
where Supplier_Id = 200105
```

## SupplierPayType

```sql
--SupplierPayType_TypeDef : OnlineBanking_Razer => CreditCardOnce_Razer
SELECT *
FROM SupplierPayType(NOLOCK)
WHERE SupplierPayType_ValidFlag = 1
AND SupplierPayType_SupplierId = 200105
```

## ShopPayType

```sql
-- OnlineBanking_Razer => CreditCardOnce_Razer
select *
from ShopPayType(nolock)
where ShopPayType_ValidFlag = 1
and ShopPayType_ShopId = 200136 
```


## ShopPayShippingDefault


```sql
-- OnlineBanking_Razer => CreditCardOnce_Razer
use WebStoreDB
select *
from ShopPayShippingDefault
where ShopPayShippingDefault_ShopId = 200136
```


## PayShippingMapping

```sql
-- 要有相關交集
select *
from PayShippingMapping
```


## PayProfile

```sql
--更新 PayProfileCurrency
use WebStoreDB

select Supplier_SalesCurrency,Supplier_SalesMarket,*
from Supplier(nolock)
where Supplier_Id = 76

select Shop_SupplierId,*
from Shop(nolock)
where Shop_Id= 80



-- GrabPay_Razer
--　OnlineBanking_Razer
-- CreditCardOnce_Razer
use ERPDB

select PayProfile_Currency,PayProfile_TypeDef,PayProfile_StatisticsTypeDef,*
from PayProfile(nolock)
--where PayProfile_TypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')
--and PayProfile_StatisticsTypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')

update PayProfile
set PayProfile_Currency = 　N'["MYR","SGD"]'
where PayProfile_TypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')
and PayProfile_StatisticsTypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')

select PayProfile_Currency,PayProfile_TypeDef,PayProfile_StatisticsTypeDef,*
from PayProfile(nolock)
where PayProfile_TypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')
and PayProfile_StatisticsTypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')
```