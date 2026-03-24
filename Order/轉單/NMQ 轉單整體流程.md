

NMQ : TransferOrderToERP



## 1 確認OrderSlaveFlow無資料，才做轉單第1步

log : Step1:ImportWebStoreDBTradesOrdersToERPDBSourceTablesByOrderId

csp_ImportWebStoreDBTradesOrdersToERPDBSourceTablesByOrderId_Mall @runId



##  2確認SalesOrderGroup無資料，才做轉單第2步


log : Step2:TradesOrderTransToSalesOrderWithFlow

csp_TradesOrderTransToSalesOrderWithFlow_Mall



## 3.1 paymentRequest check

log Step3:UpdateDataAfterTradesOrderTransToSalesOrderWithFlow Start

Step3-1: Fail. Check exist PaymentRequestId :{paymentRequestId} , TradesOrderGroupCode: {tradesOrderGroupCode} .
TABLE : paymentRequest.PaymentRequest_TradesOrderGroupCode
因第三方支付成功(Success,RePaySuccess)才會在第三步長請款單，故驗證請款單是否存在，若存在則跳 Exception 



## csp_UpdateDataAfterTradesOrderTransToSalesOrderWithFlow



## 建銷售單


CreateExpenseOrders