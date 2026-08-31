```sql
use ERPDB


select SalesOrderGroup_DateTime,RefundRequest_TradesOrderSlaveCode,RefundRequest_CreatedUser,RefundRequest_UpdatedUser,RefundRequest_Amount,RefundRequest_TradesOrderGroupCode,RefundRequest_DateTime,Shop_Name,RefundRequest_ResponseMsg,RefundRequest_PayTypeDef,RefundRequest_StatusDef,SalesOrderGroup_ShopId,RefundRequest_Amount,VipMemberInfo_Email,VipMemberInfo_FullName,VipMemberInfo_CellPhone,VipMemberInfo_CountryCode
from RefundRequest(nolock)
inner join SalesOrderGroup(nolock)
on SalesOrderGroup_TradesOrderGroupCode = RefundRequest_TradesOrderGroupCode
inner join VipMemberInfo(nolock)
on SalesOrderGroup_MemberId = VipMemberInfo_MemberId
inner join Shop(nolock)
on SalesOrderGroup_ShopId = Shop_Id
where RefundRequest_ValidFlag = 1
--and RefundRequest_PayTypeDef = 'CreditCardOnce_KPay' -- CreditCardOnce_KPay, TwoCTwoP
--and RefundRequest_DateTime > '2026-08-24'
--and RefundRequest_DateTime < GETDATE()
and RefundRequest_TradesOrderGroupCode in ('TG260730S00090')
--and RefundRequest_Id in (124650,132355)
--AND RefundRequest_StatusDef in( 'RefundRequestFail','RefundRequestGrouping')
--AND SalesOrderGroup_ShopId = 2
and RefundRequest_DateTime > '2026-08-25'
--and SalesOrderGroup_ShopId = 17


select *
from OrderSlaveFlow(nolock)
where OrderSlaveFlow_TradesOrderGroupCode = 'TG260730S00090'

select *
from SalesOrderThirdPartyPayment(nolock)
where SalesOrderThirdPartyPayment_ValidFlag = 1
and SalesOrderThirdPartyPayment_TradesOrderGroupCode = 'TG260730S00090'

select SalesOrder_TotalPayment,SalesOrderSlave_Qty,SalesOrderSlave_ValidFlag,*
from SalesOrderSlave(nolock)
inner join SalesOrder(nolock)
on SalesOrder_Id = SalesOrderSlave_SalesOrderId
inner JOIN SalesOrderGroup(nolock)
on SalesOrder_SalesOrderGroupId = SalesOrderGroup_Id
where SalesOrderGroup_TradesOrderGroupCode = 'TG260730S00090'



select top 50 *
from RechargeReceipt(nolock)
where RechargeReceipt_TradesOrderGroupCode = 'TG260730S00090'
```