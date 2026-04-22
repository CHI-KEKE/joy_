# Cybersource 異常紀錄 — RefundRequestFinish 退款請求失敗案例

## Case 1 — CreditCardOnceCybersourceRefundRequestFinish 失敗





db0bd9ea-2339-40ab-b276-c2ab96c1ff18

```json
{
  "RefundRequestIds": [
    176907
  ],
  "TradesOrderGroupId": 2572476,
  "PayType": "CreditCardOnce_Cybersource"
}
```


scmnmq log

https://payment-middleware-api-internal.hk.91app.io/api/v1.0/Refund/CreditCardOnce_Cybersource/TG260216R00039  



```bash
payment middle ware refund failed. pay type is : CreditCardOnce_Cybersource ,tgCode : TG260216R00039, HttpStatus is : InternalServerError {"ClassName":"System.ApplicationException","Message":"payment middle ware refund failed. pay type is : CreditCardOnce_Cybersource ,tgCode : TG260216R00039, HttpStatus is : InternalServerError","Data":null,"InnerException":null,"HelpURL":null,"StackTraceString":"   at NineYi.ERP.Backend.BLV2.ThirdPartyPayments.PaymentMiddleWareRefundRequestService.Refund(String payType, SalesOrderThirdPartyPayment payment, RefundRequest refundRequest) in D:\\ws\\workspace\\_Build_nineyi.scm.nmqv2_master_2\\ERP\\Backend\\BLV2\\ThirdPartyPayments\\PaymentMiddleWareRefundRequestService.cs:line 568\r\n   at NineYi.ERP.Backend.BLV2.ThirdPartyPayments.PaymentMiddleWareRefundRequestService.RefundByRequestId(PaymentMiddleWareRefundTaskDataEntity entity, SalesOrderThirdPartyPayment salesOrderThirdPartyPayment, IPayChannelService payChannelService) in D:\\ws\\workspace\\_Build_nineyi.scm.nmqv2_master_2\\ERP\\Backend\\BLV2\\ThirdPartyPayments\\PaymentMiddleWareRefundRequestService.cs:line 355\r\n   at NineYi.ERP.Backend.BLV2.ThirdPartyPayments.PaymentMiddleWareRefundRequestService.DoRefundRequestFinish(String taskData) in D:\\ws\\workspace\\_Build_nineyi.scm.nmqv2_master_2\\ERP\\Backend\\BLV2\\ThirdPartyPayments\\PaymentMiddleWareRefundRequestService.cs:line 212\r\n   at NineYi.SCM.Frontend.NMQV2.ThirdPartyPayments.ThirdPartyPaymentRequestFinishProcess.DoJob(TaskInfoEntity data) in D:\\ws\\workspace\\_Build_nineyi.scm.nmqv2_master_2\\SCM\\Frontend\\NMQV2\\ThirdPartyPayments\\ThirdPartyPaymentRequestFinishProcess.cs:line 77\r\n   at NMQ.Core.Worker.Service.WorkerProcess.DoJob(StdIOTask task) in C:\\workspace\\src\\task\\NMQ.Core.Worker\\Service\\WorkerProcess.cs:line 105","RemoteStackTraceString":null,"RemoteStackIndex":0,"ExceptionMethod":"8\nRefund\nNineYi.ERP.Backend.BLV2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\nNineYi.ERP.Backend.BLV2.ThirdPartyPayments.PaymentMiddleWareRefundRequestService\nNineYi.ERP.Backend.BE.ThirdPartyApis.PaymentMiddleWares.PaymentMiddleWareRefundResponseEntity Refund(System.String, NineYi.ERP.DA.ERPDBV2.Tables.SalesOrderThirdPartyPayment, NineYi.ERP.DA.ERPDBV2.Tables.RefundRequest)","HResult":-2146232832,"Source":"NineYi.ERP.Backend.BLV2","WatsonBuckets":null} 


```


PMW Log


```bash
HTTP Response - Status: Created, Body: "{\"_links\":{\"self\":{\"method\":\"GET\",\"href\":\"/pts/v2/refunds/7726510371206992403865\"}},\"clientReferenceInformation\":{\"code\":\"TG260216R00039\"},\"errorInformation\":{\"reason\":\"PROCESSOR_DECLINED\",\"message\":\"Decline - General decline of the card. No other information provided by the issuing bank.\"},\"id\":\"7726510371206992403865\",\"issuerInformation\":{\"transactionInformation\":\"RjEcSLbtREa5oPS2ONbtRA\"},\"processorInformation\":{\"networkTransactionId\":\"0304MCWDZXANJ\",\"retrievalReferenceNumber\":\"606319869224\",\"responseCode\":\"203\"},\"status\":\"DECLINED\"}"
```




## Case 2 — CreditCardOnceCybersource 退款再次失敗



https://payment-middleware-api-internal.hk.91app.io/api/v1.0/Refund/CreditCardOnce_Cybersource/TG260202M00007  


https://payment-middleware-api-internal.hk.91app.io/api/v1.0/Refund/CreditCardOnce_Cybersource/TG260202M00007  


```bash
payment middle ware refund failed. pay type is : CreditCardOnce_Cybersource ,tgCode : TG260202M00007, HttpStatus is : InternalServerError {"ClassName":"System.ApplicationException","Message":"payment middle ware refund failed. pay type is : CreditCardOnce_Cybersource ,tgCode : TG260202M00007, HttpStatus is : InternalServerError","Data":null,"InnerException":null,"HelpURL":null,"StackTraceString":"   at NineYi.ERP.Backend.BLV2.ThirdPartyPayments.PaymentMiddleWareRefundRequestService.Refund(String payType, SalesOrderThirdPartyPayment payment, RefundRequest refundRequest) in D:\\ws\\workspace\\_Build_nineyi.scm.nmqv2_master_2\\ERP\\Backend\\BLV2\\ThirdPartyPayments\\PaymentMiddleWareRefundRequestService.cs:line 568\r\n   at NineYi.ERP.Backend.BLV2.ThirdPartyPayments.PaymentMiddleWareRefundRequestService.RefundByRequestId(PaymentMiddleWareRefundTaskDataEntity entity, SalesOrderThirdPartyPayment salesOrderThirdPartyPayment, IPayChannelService payChannelService) in D:\\ws\\workspace\\_Build_nineyi.scm.nmqv2_master_2\\ERP\\Backend\\BLV2\\ThirdPartyPayments\\PaymentMiddleWareRefundRequestService.cs:line 355\r\n   at NineYi.ERP.Backend.BLV2.ThirdPartyPayments.PaymentMiddleWareRefundRequestService.DoRefundRequestFinish(String taskData) in D:\\ws\\workspace\\_Build_nineyi.scm.nmqv2_master_2\\ERP\\Backend\\BLV2\\ThirdPartyPayments\\PaymentMiddleWareRefundRequestService.cs:line 212\r\n   at NineYi.SCM.Frontend.NMQV2.ThirdPartyPayments.ThirdPartyPaymentRequestFinishProcess.DoJob(TaskInfoEntity data) in D:\\ws\\workspace\\_Build_nineyi.scm.nmqv2_master_2\\SCM\\Frontend\\NMQV2\\ThirdPartyPayments\\ThirdPartyPaymentRequestFinishProcess.cs:line 77\r\n   at NMQ.Core.Worker.Service.WorkerProcess.DoJob(StdIOTask task) in C:\\workspace\\src\\task\\NMQ.Core.Worker\\Service\\WorkerProcess.cs:line 105","RemoteStackTraceString":null,"RemoteStackIndex":0,"ExceptionMethod":"8\nRefund\nNineYi.ERP.Backend.BLV2, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\nNineYi.ERP.Backend.BLV2.ThirdPartyPayments.PaymentMiddleWareRefundRequestService\nNineYi.ERP.Backend.BE.ThirdPartyApis.PaymentMiddleWares.PaymentMiddleWareRefundResponseEntity Refund(System.String, NineYi.ERP.DA.ERPDBV2.Tables.SalesOrderThirdPartyPayment, NineYi.ERP.DA.ERPDBV2.Tables.RefundRequest)","HResult":-2146232832,"Source":"NineYi.ERP.Backend.BLV2","WatsonBuckets":null} 
```


pmw log

```bash
HTTP Response - Status: Created, Body: "{\"_links\":{\"self\":{\"method\":\"GET\",\"href\":\"/pts/v2/refunds/7726509544876902103847\"}},\"clientReferenceInformation\":{\"code\":\"TG260202M00007\"},\"errorInformation\":{\"reason\":\"PROCESSOR_DECLINED\",\"message\":\"Decline - General decline of the card. No other information provided by the issuing bank.\"},\"id\":\"7726509544876902103847\",\"issuerInformation\":{\"transactionInformation\":\"bTxF8KCjS2qi71PIcS5zug\"},\"processorInformation\":{\"networkTransactionId\":\"0304MCWSTYS44\",\"retrievalReferenceNumber\":\"606319081849\",\"responseCode\":\"203\"},\"status\":\"DECLINED\"}"
```
