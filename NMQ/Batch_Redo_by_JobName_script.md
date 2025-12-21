

## 參考文件

https://wiki.91app.com/pages/viewpage.action?pageId=352977195

<br>
<br>

## 主要程式碼

```csharp
void Main(string[] args)
{
	var market = "HK"; //// TW HK MY
	var envrionment = "QA";//// QA Prod
	var jobName = JobName.PromotionRewardCoupon;
	var status = StatusName.FAILED; // CANCELED FAILED DOING READY
	var taskPool = "ONGOING";
	var method = MethodName.Redo; // Cancel Redo Archive
	// READY -> Cancel
	// CANCELED -> Archive
	// CANCELED -> Redo
	// FAILED -> Redo
	// FAILED -> Archive

	$"Market: {market}".Dump();
	$"Environment: {envrionment}".Dump();
	$"Job: {jobName.ToString()}".Dump();
	$"Status: {status.ToString()}".Dump();
	$"Action: {method.ToString()}".Dump();

	_httpClient.Timeout = TimeSpan.FromSeconds(30);
	_httpClient2.Timeout = TimeSpan.FromSeconds(30);

	//// 查詢 TaskId
	var taskList = this.GetTaskIdList(market, envrionment, jobName, status, taskPool);
	$"Total Count: {taskList.Count()}".Dump();

	int sleep = 250; //// ms
	//// 異動 Task
	this.UpdateTask(market, envrionment, method, taskList, sleep)
		.GetAwaiter()
		.GetResult();

	$"Done. {DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}".Dump();
}
```

<br>

**Job 名稱定義類別**：

```csharp
class JobName
{
	public const string SyncValidCollectionJob = nameof(SyncValidCollectionJob);
	public const string SaveTradesOrderGroupExtensionInfoToS3Job = nameof(SaveTradesOrderGroupExtensionInfoToS3Job);
	public const string AuditPromotionEngineFreeGiftJob = nameof(AuditPromotionEngineFreeGiftJob);
	public const string CheckPromotionSalepageCollectionJob = nameof(CheckPromotionSalepageCollectionJob);
	public const string AuditPromotionEngineSyncCollectionJob = nameof(AuditPromotionEngineSyncCollectionJob);
	public const string AuditDiscountByMemberWithPriceJob = nameof(AuditDiscountByMemberWithPriceJob);
	public const string SyncSalePageGroupWithSearch = nameof(SyncSalePageGroupWithSearch);
	public const string CheckSaleProductSKUIOAndCreateTaksJob = nameof(CheckSaleProductSKUIOAndCreateTaksJob);
	public const string SyncCollectionToPromotionTagJob = nameof(SyncCollectionToPromotionTagJob);
	public const string PromotionRewardLoyaltyPointsDispatcherV2 = nameof(PromotionRewardLoyaltyPointsDispatcherV2);
	public const string PromotionRewardLoyaltyPointsV2 = nameof(PromotionRewardLoyaltyPointsV2);
	public const string PromotionRewardLoyaltyPointsPerformance = nameof(PromotionRewardLoyaltyPointsPerformance);
	public const string AuditPromotionRewardLoyaltyPointsV2 = nameof(AuditPromotionRewardLoyaltyPointsV2);
	public const string AuditRecycleLoyaltyPointsV2 = nameof(AuditRecycleLoyaltyPointsV2);
	public const string UpdateRoyaleDeliveryStatus = nameof(UpdateRoyaleDeliveryStatus);
	public const string Heartbeat = nameof(Heartbeat);
	public const string SendTemplateMailPriorityLow = nameof(SendTemplateMailPriorityLow);
	public const string ShoppingCartDataVerificationFromNewCartJob = nameof(ShoppingCartDataVerificationFromNewCartJob);
	public const string ExtendInfoAuditJob = nameof(ExtendInfoAuditJob);
	public const string ShippingVerificationJob = nameof(ShippingVerificationJob);
	public const string CreateTransactionInfo = nameof(CreateTransactionInfo);
	public const string PromotionRewardCoupon = nameof(PromotionRewardCoupon);
}
```

<br>

**狀態和方法定義**：


```csharp
class StatusName
{
	public const string READY = nameof(READY);
	public const string DOING = nameof(DOING);
	public const string FAILED = nameof(FAILED);
	public const string CANCELED = nameof(CANCELED);
}

class MethodName
{
	public const string Archive = "archive";
	public const string Cancel = "cancel";
	public const string Redo = "redo";
}
```

<br>

**HTTP 客戶端初始化**：

```csharp
HttpClient _httpClient = new HttpClient();
HttpClient _httpClient2 = new HttpClient();
```

<br>

**批次更新任務方法**：

```csharp
async System.Threading.Tasks.Task UpdateTask(string market, string environment, string method, IList<string> taskList, int sleep)
{
	var count = 1;
	foreach (var taskId in taskList)
	{
		$"{count.ToString("0000")}: {taskId}".Dump();
		await this.CallNmqApi(market, environment, method, taskId);
		Thread.Sleep(sleep);
		count++;
	}
}
```

<br>

**呼叫 NMQ API 方法**：

```csharp
async System.Threading.Tasks.Task CallNmqApi(string market, string environment, string method, string taskId)
{
	var uri = this.GetNmqV3Url(market, environment);
	var uriBuilder = new UriBuilder($"{uri}/{taskId}/{method}");

	var response = method switch
	{
		MethodName.Archive => _httpClient2.DeleteAsync(uriBuilder.Uri),
		MethodName.Redo => _httpClient2.PutAsync(uriBuilder.Uri, null),
		MethodName.Cancel => _httpClient2.PutAsync(uriBuilder.Uri, null),
		_ => throw new NotImplementedException(),
	};
	response.Result.EnsureSuccessStatusCode();

	var responseBody = await response.Result.Content.ReadAsStringAsync();
}
```

<br>

**環境 URL 配置方法**：

```csharp
string GetDashboardUrl(string market, string environment)
{
	var env = string.Join('_', new string[] { market, environment });
	return env switch
	{
		"TW_QA" => "http://nmqv3-dashboard.qa.91dev.tw/graphql",
		"HK_QA" => "http://nmqv3-dashboard.qa1.hk.91dev.tw/graphql",
		"MY_QA" => "http://nmqv3-dashboard.qa1.my.91dev.tw/graphql",
		"TW_Prod" => "http://nmqv3-dashboard.91app.io/graphql",
		"HK_Prod" => "http://nmqv3-dashboard.hk.91app.io/graphql",
		"MY_Prod" => "http://nmqv3-dashboard.my.91app.io/graphql",
		_ => throw new NotSupportedException(),
	};
}

string GetNmqV3Url(string market, string environment)
{
	var env = string.Join('_', new string[] { market, environment });
	return env switch
	{
		"TW_QA" => "http://nmqv3-apimin.qa.91dev.tw/api/v1/task",
		"HK_QA" => "http://nmqv3-apimin.qa1.hk.91dev.tw/api/v1/task",
		"MY_QA" => "http://nmqv3-apimin.qa1.my.91dev.tw/api/v1/task",
		"TW_Prod" => "http://nmqv3-apimin.91app.io/api/v1/task",
		"HK_Prod" => "http://nmqv3-apimin.hk.91app.io/api/v1/task",
		"MY_Prod" => "http://nmqv3-apimin.my.91app.io/api/v1/task",
		_ => throw new NotSupportedException(),
	};
}
```
