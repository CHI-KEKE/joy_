

## 拆解

Nine1.Promotion.Console.DA.Repositories.Repository.SalesOrders.SalesOrderRepository.GetSalesOrderSlavesAsync(Int64 shopId, String tradesOrderGroupCode) in /src/Nine1.Promotion.Console.DA.Repositories/Repository/SalesOrders/SalesOrderRepository.cs:line 105

at Nine1.Promotion.Console.BL.Services.PromotionReward.LoyaltyPointService.SetBookingTimeAsync(PromotionRewardLoyaltyPointsEntity record, List`1 details, Boolean isRecalculateMode) in /src/Nine1.Promotion.Console.BL/Services/PromotionReward/LoyaltyPointService.cs:line 964

at Nine1.Promotion.Console.NMQv3Worker.Jobs.PromotionReward.PromotionRewardLoyaltyPointsV2Job.ProcessRewardStateAsync(PromotionRewardRequestEntity entity) in /src/Nine1.Promotion.Console.NMQv3Worker/Jobs/PromotionReward/PromotionRewardLoyaltyPointsV2Job.cs:line 476


at Nine1.Promotion.Console.NMQv3Worker.Jobs.PromotionReward.PromotionRewardLoyaltyPointsV2Job.PromotionRewardProcess(PromotionRewardRequestEntity entity) in /src/Nine1.Promotion.Console.NMQv3Worker/Jobs/PromotionReward/PromotionRewardLoyaltyPointsV2Job.cs:line 254

at Nine1.Promotion.Console.NMQv3Worker.Jobs.PromotionReward.PromotionRewardLoyaltyPointsV2Job.DoJobAsync(String data, CancellationToken cancellationToken) in /src/Nine1.Promotion.Console.NMQv3Worker/Jobs/PromotionReward/PromotionRewardLoyaltyPointsV2Job.cs:line 214

at Nine1.Promotion.Console.NMQv3Worker.Jobs.PromotionReward.PromotionRewardLoyaltyPointsV2Job.DoJob(String data, CancellationToken cancellationToken) in /src/Nine1.Promotion.Console.NMQv3Worker/Jobs/PromotionReward/PromotionRewardLoyaltyPointsV2Job.cs:line 180


ClientConnectionId:e00f70bf-69dd-413a-b7f2-4cf7b4824516\nError Number:1205,State:52,Class:13\"


SalesOrderRepository.cs:line 105
LoyaltyPointService
PromotionRewardLoyaltyPointsV2Job




## log from Dashboard



https://monitoring-dashboard.91app.io/d/b5euvwztkbpr9n/nmqv3-task-log?orgId=2&var-MarketENV=HK-Prod&var-Namespace=prod-promotion-service&var-Container=promotion-console-nmqv3worker-group4&var-Keyword_TaskId=108cbd6a-2c4a-49ed-9507-2351926260cc&var-Keyword_Contains_1=&var-Keyword_Contains_2=&from=2026-01-20T17:08:00.930Z&to=2026-01-20T17:18:03.868Z&timezone=Asia%2FTaipei&var-Loki=RjRcuuN4k&var-Cluster=dfHnWT74z



## PR (Retry Deadlock)

https://gitlab.91app.com/commerce-cloud/nine1.promotion/nine1.promotion.worker/-/merge_requests/999



第一次

{"_ts":"2026-01-20T17:13:03.7706534Z"

{"_ts":"2026-01-20T17:13:03.7706534Z","_msg":"An exception occurred while iterating over the results of a query for context type '\"Nine1.Promotion.Console.DA.ERPDB.Extends.ERPDBReadOnlyContext\"'.\"\n\"\"Microsoft.Data.SqlClient.SqlException (0x80131904): Transaction (Process ID 452) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.\n at Microsoft.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\n at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\n at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)\n at Microsoft.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)\n at Microsoft.Data.SqlClient.SqlDataReader.TryHasMoreRows(Boolean& moreRows)\n at Microsoft.Data.SqlClient.SqlDataReader.TryReadInternal(Boolean setTimeout, Boolean& more)\n at Microsoft.Data.SqlClient.SqlDataReader.ReadAsyncExecute(Task task, Object state)\n at Microsoft.Data.SqlClient.SqlDataReader.InvokeAsyncCall[T](SqlDataReaderBaseAsyncCallContext`1 context)\n--- End of stack trace from previous location ---\n at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()\nClientConnectionId:e00f70bf-69dd-413a-b7f2-4cf7b4824516\nError Number:1205,State:52,Class:13\"","_lvl":"Error","_ex":"Microsoft.Data.SqlClient.SqlException (0x80131904): Transaction (Process ID 452) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.\n at Microsoft.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\n at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\n at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)\n at Microsoft.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)\n at Microsoft.Data.SqlClient.SqlDataReader.TryHasMoreRows(Boolean& moreRows)\n at Microsoft.Data.SqlClient.SqlDataReader.TryReadInternal(Boolean setTimeout, Boolean& more)\n at Microsoft.Data.SqlClient.SqlDataReader.ReadAsyncExecute(Task task, Object state)\n at Microsoft.Data.SqlClient.SqlDataReader.InvokeAsyncCall[T](SqlDataReaderBaseAsyncCallContext`1 context)\n--- End of stack trace from previous location ---\n at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()\nClientConnectionId:e00f70bf-69dd-413a-b7f2-4cf7b4824516\nError Number:1205,State:52,Class:13","_ei":10100,"_en":"Microsoft.EntityFrameworkCore.Query.QueryIterationFailed","_srctx":"Microsoft.EntityFrameworkCore.Query","_lt":"Common","_hid":"promotion-console-nmqv3worker-group4-6c479db858-7nwcm","_props":{"contextType":"Nine1.Promotion.Console.DA.ERPDB.Extends.ERPDBReadOnlyContext","newline":"\n","error":"Microsoft.Data.SqlClient.SqlException (0x80131904): Transaction (Process ID 452) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction.\n at Microsoft.Data.SqlClient.SqlConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\n at Microsoft.Data.SqlClient.SqlInternalConnection.OnError(SqlException exception, Boolean breakConnection, Action`1 wrapCloseInAction)\n at Microsoft.Data.SqlClient.TdsParser.ThrowExceptionAndWarning(TdsParserStateObject stateObj, Boolean callerHasConnectionLock, Boolean asyncClose)\n at Microsoft.Data.SqlClient.TdsParser.TryRun(RunBehavior runBehavior, SqlCommand cmdHandler, SqlDataReader dataStream, BulkCopySimpleResultSet bulkCopyHandler, TdsParserStateObject stateObj, Boolean& dataReady)\n at Microsoft.Data.SqlClient.SqlDataReader.TryHasMoreRows(Boolean& moreRows)\n at Microsoft.Data.SqlClient.SqlDataReader.TryReadInternal(Boolean setTimeout, Boolean& more)\n at Microsoft.Data.SqlClient.SqlDataReader.ReadAsyncExecute(Task task, Object state)\n at Microsoft.Data.SqlClient.SqlDataReader.InvokeAsyncCall[T](SqlDataReaderBaseAsyncCallContext`1 context)\n--- End of stack trace from previous location ---\n at Microsoft.EntityFrameworkCore.Query.Internal.SingleQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()\nClientConnectionId:e00f70bf-69dd-413a-b7f2-4cf7b4824516\nError Number:1205,State:52,Class:13"}


第二次

lock fail


第三次

lock fail



https://monitoring-dashboard.91app.io/explore?schemaVersion=1&panes=%7B%220sc%22:%7B%22datasource%22:%22RjRcuuN4k%22,%22queries%22:%5B%7B%22refId%22:%22A%22,%22expr%22:%22%7Bservice%3D%5C%22prod-promotion-service%5C%22%7D%20%5Cr%5Cn%7Cjson%5Cr%5Cn%7C%3D%20%60%E8%B3%87%E6%96%99%E5%BA%AB%E6%9F%A5%E8%A9%A2%E7%99%BC%E7%94%9F%E9%8C%AF%E8%AA%A4,%E6%AD%A3%E5%9C%A8%E9%80%B2%E8%A1%8C%E7%AC%AC%60%5Cr%5Cn%7Cjson%5Cr%5Cn%7C%20line_format%20%5C%22%7B%7B._msg%7D%7D%5C%22%22,%22queryType%22:%22range%22,%22datasource%22:%7B%22type%22:%22loki%22,%22uid%22:%22RjRcuuN4k%22%7D,%22editorMode%22:%22code%22,%22direction%22:%22backward%22%7D%5D,%22range%22:%7B%22from%22:%22now-2d%22,%22to%22:%22now%22%7D,%22panelsState%22:%7B%22logs%22:%7B%22columns%22:%7B%220%22:%22Time%22,%221%22:%22Line%22,%222%22:%22_props_JobName%22,%223%22:%22_props_TaskId%22%7D,%22visualisationType%22:%22table%22,%22labelFieldName%22:%22labels%22,%22refId%22:%22A%22%7D%7D%7D%7D&orgId=2


有處理到


ae419d27-7777-4faf-938f-1e3ec688bafd
d548f78e-9639-48b8-98f4-53527aa85dc7
a8d8f63f-ff49-4c0f-aa88-f1bea2a54b09



