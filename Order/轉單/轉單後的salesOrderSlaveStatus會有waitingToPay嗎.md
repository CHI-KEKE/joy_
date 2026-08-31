

結論：轉單後 SalesOrderSlave 的 OrderStatus 能是 WaitingToPay 的條件

 SalesOrderSlave_StatusDef 賦值完整路徑

  CSP：csp_TradesOrderTransToSalesOrderWithFlow_Mall.sql

  Step 1 — L193：定義 @sourcePayTypes（Razer 在其中）

   @sourcePayTypes VARCHAR(1000) = '...CreditCardOnce_Razer, Boost_Razer, GrabPay_Razer,...'

  Step 2 — L867：INSERT INTO [dbo].[SalesOrderSlave]

  開始 INSERT，欄位清單第 L893 行是 [SalesOrderSlave_StatusDef]

  Step 3 — L987：對應的 SELECT 來源

  SELECT 出來的值 L1012~L1032 這個 CASE 運算式：

   -- L1012~L1032，這個 CASE 的結果 AS [TradesOrderSlave_StatusDef]
   -- 按順序判斷：
   L1013: WHEN @checkStatusDef = 'TransToCancel'          → 'Fail'
   L1014: WHEN HK + ApplePay/GooglePay                    → 'WaitingThirdPartyPaymentCheck'
   L1015: WHEN CreditCard 系列 → 依授權結果決定
   L1019:     AuthSuccess                                 → 'WaitingToCreditCheck'
   L1020:     Expired                                     → 'Cancel'
   L1021:     Else                                        → 'Fail'
   L1029: WHEN PayTypeDef IN ('ATM','CustomOfflinePayment')→ 'WaitingToPay'
   L1030: WHEN PayTypeDef IN (@sourcePayTypes)             → 'WaitingThirdPartyPaymentCheck'
   L1031: ELSE                                            → 'WaitingToShipping'
   L1032: END AS [TradesOrderSlave_StatusDef]

  Step 4 — 這個值直接 INSERT 進 SalesOrderSlave_StatusDef（L893 對應 L1032）