

## appsetting


//// 設定 AWS DynamoDB & Multilingual SDK
var multilingualAccessKey = ConfigService.GetAppSetting<string>("DynamoDB.Multilingual.AccessKey");
var multilingualSecretKey = ConfigService.GetAppSetting<string>("DynamoDB.Multilingual.AccessSecret");
var multilingualRegion = ConfigService.GetAppSetting<string>("DynamoDB.Multilingual.Region");
var multilingualTableName = ConfigService.GetAppSetting<string>("DynamoDB.Multilingual.TableName");

## trace

呼叫鏈（由外而內）：

RunFlowById.Program.ExecuteFlowId
 → EtlFlowControlService.DoProcess (shopEtlFlowId=180)
 → EtlFlowControlService.DoFlowValidation (line 571，用 Activator.CreateInstance 建立 Flow Service)
 → ProductFeedFlowService 建構子 (line 56)
 → MultilingualService 建構子 (line 43) ← 實際錯誤發生點
 → new DynamoMultilingualAdmin(...) → Table.DescribeTable() → AWS SDK 呼叫 DynamoDB
 → AmazonDynamoDBException: The security token included in the request is invalid.

因為錯誤發生在  DoFlowValidation  階段（透過 Reflection 建立 Flow Service 實例時），此時流程根本還沒進入 EtlFlow 主體執行，所以 Log 才會顯示「未進到 EtlFlow 就發生錯誤」。

根因

 MultilingualService  建構子（ MultilingualService.cs  line 27‑41）用以下設定建立  AmazonDynamoDBClient ：

var multilingualAccessKey = ConfigService.GetAppSetting<string>("DynamoDB.Multilingual.AccessKey");
var multilingualSecretKey = ConfigService.GetAppSetting<string>("DynamoDB.Multilingual.AccessSecret");
...
var amazonDynamoDBClient = new AmazonDynamoDBClient(multilingualAccessKey, multilingualSecretKey, multilingualConfig);

對應  AppSettings.QA.HK.config  (line 277‑278)：

<add key="QA.DynamoDB.Multilingual.AccessKey" value="" />
<add key="QA.DynamoDB.Multilingual.AccessSecret" value="" />

「The security token included in the request is invalid」 是 AWS IAM 明確回傳的錯誤，代表這組 AccessKey/SecretKey 在 AWS 端已失效（常見原因：金鑰被停用/刪除/輪替、或已過期）。由於 TW/HK/MY 三個 QA 環境共用同一組 AccessKey，只要 IAM 那組金鑰被 disable/rotate，所有 QA 站台的多語系（Multilingual）DynamoDB 呼叫都會全部失敗，進而讓所有依賴  MultilingualService  的 EtlFlow（如 ProductFeedFlowService）驗證階段直接拋錯。


建議處理

1. 到 AWS IAM Console 確認  這組金鑰目前狀態（Active/Inactive）。
2. 若已停用或輪替，需請 DevOps/AWS 管理者重新產生新的 AccessKey/Secret，並更新  AppSettings.QA.HK.config （連同 MY/TW 的 QA config 一併確認，因為金鑰是共用的）。
3. 更新後重新部署/重啟該站台再重跑 shopEtlFlowId=180 驗證。