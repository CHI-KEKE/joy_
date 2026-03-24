
## EComOrderPromotionRewardCalculateService => SalesOrderRepository

/src/Nine1.Promotion.Console.DA.Repositories/Repository/SalesOrders/SalesOrderRepository.cs:line 105
→ 於 GetSalesOrderSlavesAsync 查詢資料時發生死結。

/src/Nine1.Promotion.Console.BL/Services/Promotions/EComOrderPromotionRewardCalculateService.cs:line 230
→ 呼叫取得訂單資料失敗。

/src/Nine1.Promotion.Console.BL/Services/Promotions/PromotionRewardCalculateServiceBase.cs:line 237
→ 在計算點數階段發生異常。

/src/Nine1.Promotion.Console.NMQv3Worker/Jobs/PromotionReward/PromotionRewardLoyaltyPointsV2Job.cs:line 476
→ 進行點數回饋處理時，ProcessRewardStateAsync 發生異常。



Message: "Reward Point Error, Exception： Microsoft.Data.SqlClient.SqlException (0x80131904): Transaction (Process ID 226) was deadlocked..."

TargetId: {"ShopId":17,"TradesOrderGroupCode":"TG260122X00077",...,"EventSource":"OrderCreated"}

Error Number:1205,State:52,Class:13

PromotionEngineId: 34957

RewardFlowEnum: Reward

OrderTypeDefEnum: ECom



❌ 發生的異常類型

類型： Microsoft.Data.SqlClient.SqlException

錯誤碼： 1205

錯誤訊息： Transaction was deadlocked on lock resources with another process and has been chosen as the deadlock victim.

此異常發生於處理電商訂單回饋點數的自動任務（PromotionRewardLoyaltyPointsV2Job）中，具體場景如下：

用戶於商店（ShopId: 17）下單（訂單編號 TG260122X00077）。

系統觸發 PromotionEngineId 為 34957 的促銷邏輯，開始處理點數回饋。

在計算回饋點數過程中，系統需讀取訂單細節（從 SalesOrderSlave 取得從屬訂單資料）。

該查詢與其他交易產生資源爭奪（如索引或資料列鎖定），導致資料庫死結，系統將此交易標記為 Deadlock Victim。



{"_ts":"2026-01-28T15:14:39.8173936Z","_msg":"An exception occurred while iterating over the results of a query for context type '\"Nine1.Promotion.Console.DA.ERPDB.Extends.ERPDBReadOnlyContext\"'.\"\n\"\"Microsoft.Data.SqlClient.SqlException (0x80131904): Transaction (Process ID 297) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.\n at Microsoft.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\n at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\n at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)\n at Microsoft.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)\n at Microsoft.Data.SqlClient.SqlDataReader.TryHasMoreRows(Boolean& moreRows)\n at Microsoft.Data.SqlClient.SqlDataReader.TryReadInternal(Boolean setTimeout, Boolean& more)\n at Microsoft.Data.SqlClient.SqlDataReader.ReadAsyncExecute(Task task, Object state)\n at Microsoft.Data.SqlClient.SqlDataReader.InvokeAsyncCall[T](SqlDataReaderBaseAsyncCallContext`1 context)\n--- End of stack trace from previous location ---\n at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()\nClientConnectionId:ef006a37-f235-42d1-8f54-bb4d9afb8f9c\nError Number:1205,State:52,Class:13\"","_lvl":"Error","_ex":"Microsoft.Data.SqlClient.SqlException (0x80131904): Transaction (Process ID 297) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.\n at Microsoft.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\n at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\n at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)\n at Microsoft.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)\n at Microsoft.Data.SqlClient.SqlDataReader.TryHasMoreRows(Boolean& moreRows)\n at Microsoft.Data.SqlClient.SqlDataReader.TryReadInternal(Boolean setTimeout, Boolean& more)\n at Microsoft.Data.SqlClient.SqlDataReader.ReadAsyncExecute(Task task, Object state)\n at Microsoft.Data.SqlClient.SqlDataReader.InvokeAsyncCall[T](SqlDataReaderBaseAsyncCallContext`1 context)\n--- End of stack trace from previous location ---\n at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()\nClientConnectionId:ef006a37-f235-42d1-8f54-bb4d9afb8f9c\nError Number:1205,State:52,Class:13","_ei":10100,"_en":"Microsoft.EntityFrameworkCore.Query.QueryIterationFailed","_srctx":"Microsoft.EntityFrameworkCore.Query","_lt":"Common","_hid":"promotion-console-nmqv3worker-group4-7b9c7cbd8-mxprx","_props":{"contextType":"Nine1.Promotion.Console.DA.ERPDB.Extends.ERPDBReadOnlyContext","newline":"\n","error":"Microsoft.Data.SqlClient.SqlException (0x80131904): Transaction (Process ID 297) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.\n at Microsoft.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\n at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\n at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)\n at Microsoft.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)\n at Microsoft.Data.SqlClient.SqlDataReader.TryHasMoreRows(Boolean& moreRows)\n at Microsoft.Data.SqlClient.SqlDataReader.TryReadInternal(Boolean setTimeout, Boolean& more)\n at Microsoft.Data.SqlClient.SqlDataReader.ReadAsyncExecute(Task task, Object state)\n at Microsoft.Data.SqlClient.SqlDataReader.InvokeAsyncCall[T](SqlDataReaderBaseAsyncCallContext`1 context)\n--- End of stack trace from previous location ---\n at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()\nClientConnectionId:ef006a37-f235-42d1-8f54-bb4d9afb8f9c\nError Number:1205,State:52,Class:13"}




https://monitoring-dashboard.91app.io/d/b5euvwztkbpr9n/nmqv3-task-log?orgId=2&var-MarketENV=HK-Prod&var-Namespace=prod-promotion-service&var-Container=promotion-console-nmqv3worker-group4&var-Keyword_TaskId=66ecb04d-afba-4c5e-921a-65c988ef2596&var-Keyword_Contains_1=&var-Keyword_Contains_2=&from=2026-01-28T15:09:34.678Z&to=2026-01-28T15:19:39.849Z&timezone=Asia%2FTaipei&var-Loki=RjRcuuN4k&var-Cluster=dfHnWT74z


Transaction (Process ID 297) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.