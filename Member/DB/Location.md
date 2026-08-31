```sql
--EmployeeLocation_LocationEmployeeId = 11
-- locationid = 181
use InfoDB

select EmployeeLocation_LocationEmployeeId,EmployeeLocation_LocationId,Location_ShopId,*
from EmployeeLocation(nolock)
inner join Location(nolock)
on Location_Id = EmployeeLocation_LocationId
where EmployeeLocation_ValidFlag = 1
and Location_ShopId = 2
and Location_ValidFlag = 1
and Location_Id = 181


--CrmSalesOrder_LocationId	= 181
--CrmMember_NineYiMemberId	= 33037
--CrmMember_NineYiVipMemberId = 33043
		
use CRMDB

select top 20 CrmSalesOrder_LocationId,CrmMember_NineYiMemberId,CrmMember_NineYiVipMemberId,*
from CrmSalesOrder(nolock)
inner join CrmSalesOrderSlave(nolock)
on CrmSalesOrder_Id = CrmSalesOrderSlave_CrmSalesOrderId
inner join CrmMember(nolock)
on CrmMember_Id = CrmSalesOrder_CrmMemberId
where CrmSalesOrder_ShopId = 2
and CrmMember_ValidFlag = 1
and CrmSalesOrder_ValidFlag = 1
and CrmSalesOrderSlave_ValidFlag = 1

use WebStoreDB

select VipMemberInfo_CellPhone,VipMemberInfo_CountryCode,VipMemberInfo_CountryProfileId,*
from VipMemberInfo(nolock)
where VipMemberInfo_ValidFlag = 1
and VipMemberInfo_VipMemberId = 33043
```