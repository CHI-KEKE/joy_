use WebStoreDB

select *
from VipMemberInfo(nolock)
where VipMemberInfo_ValidFlag = 1
and VipMemberInfo_CellPhone like '%61013567%'


SELECT *
FROM ShoppingCart(NOLOCK)
WHERE ShoppingCart_MemberId = 3828207
--and ShoppingCart_DateTime between '2026-05-10' and '2026-05-12'
and ShoppingCart_UnloginId ='5454ca7d-0ee2-400c-b6c2-4aa912dd2bb4'

select SalePage_Modes,*
from SalePage(nolock)
where SalePage_ValidFlag  = 1
and SalePage_Id in (438772,289143,581141,594594,366570,589410,549653,593720,580524,589407,328645,557371,374440,289144,6886,284345)


-- 組合 593720
select SalePageIO_SalePageId,SalePageIO_Action,SalePageIO_ChangeColumn,SalePageIO_ChangeToValue,SalePageIO_CreatedDateTime,SalePageIO_Requester,*
from SalePageIO(nolock)
where SalePageIO_ValidFlag = 1
and SalePageIO_SalePageId =   589830


-- 組合 593720
select SalePageIO_SalePageId,SalePageIO_Action,SalePageIO_ChangeColumn,SalePageIO_ChangeToValue,SalePageIO_CreatedDateTime,SalePageIO_Requester,*
from SalePageIO(nolock)
where SalePageIO_ValidFlag = 1
and SalePageIO_SalePageId =   593720

select SaleProductSKU_Price,*
from SaleProductSKU(nolock)
where SaleProductSKU_SalePageId = 589830


--子商品 SKUId:  4090706, salepageid  :　589830
select SaleProductSKU_SalePageId,SaleProductSKUIO_UpdatedUser,SaleProductSKUIO_UpdatedDateTime,SaleProductSKUIO_SaleProductSKUId,SaleProductSKUIO_ChangeValue,SaleProductSKUIO_ChangeToValue,SaleProductSKUIO_ChangeColumn,*
from SaleProductSKUIO(nolock)
inner join SaleProductSKU(nolock)
on SaleProductSKU_Id = SaleProductSKUIO_SaleProductSKUId
where SaleProductSKUIO_SaleProductSKUId = 4090706

-- 組合商品 salepageid :  593720
select SalePageIO_SalePageId,SalePageIO_Action,SalePageIO_ChangeColumn,SalePageIO_ChangeToValue,SalePageIO_CreatedDateTime,SalePageIO_Requester,*
from SalePageIO(nolock)
where SalePageIO_ValidFlag = 1
and SalePageIO_SalePageId =   593720
order by SalePageIO.SalePageIO_CreatedDateTime desc

use WebStoreDB
select *
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_GroupTypeDef = 'Language'
--and ShopDefault_Key in ('Default','IsEnableAutoDetectLanguage')
and ShopDefault_ShopId = 17


