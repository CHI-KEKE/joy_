## 線下訂單


**來源**
主要子單會包含 : OriginalCrmSalesOrderId == 0 & OriginalCrmSalesOrderId == i.CrmSalesOrderId(勾自己)

+

同天不同單的逆流程

**怎麼組**
OrderSlaveCode = OuterOrderSlaveCode1~6
OrderGroupCode = CrmSalesOrder:CrmSalesOrderId
CrmSalesOrderGroupJoinCode = OuterOrderCode1~5
OrderStatus = Finish
OrderSlaveStatusDef = Finish
OrderSlaveStatusUpdatedDateTime = CrmSalesOrderTradesOrderCreateDateTime
OuterId = OuterProductSkuCode
OrderGroupDateTime = CrmSalesOrderTradesOrderCreateDateTime
OrderId = CrmSalesOrderId