# 🔄 NMQV3 維護文件

<br>

## 📖 目錄

  - [🌐 語系工具設定](#-語系工具設定)
  - [🛠️ Debug 屬性設定](#️-debug-屬性設定)
  - [🎯 Event 設定相關](#-event-設定相關)
  - [🖥️ 加開機器流程](#️-加開機器流程)
  - [🧪 測試資料模板](#-測試資料模板)
  - [� 架構分析關鍵字](#-架構分析關鍵字)
  - [�🔄 Batch Redo NMQ Job 程式碼](#-batch-redo-nmq-job-程式碼)

<br>

---

## 🌐 語系工具設定

**Translations 語系工具執行**：

<br>

NMQV3 開發時需要執行 Translations 語系工具

<br>

**參考文件**：

<br>

詳細的語系工具安裝和使用方法，請參考三中心開發維護文件中的相關章節

<br>

**相關章節包含**：
- Cake Tool 安裝與使用
- 語系工具安裝
- 系統多語系設定

<br>

---

## 🛠️ Debug 屬性設定

**測試 Job 設定**：

<br>

在 Visual Studio 中進行以下設定：

<br>

```
Nine1.Commerce.Nmqv3.Console.NMQv3Worker Debug Properties => 設定測試 Job
```

<br>

**操作步驟**：

<br>

1. 在 Visual Studio 中找到 `Nine1.Commerce.Nmqv3.Console.NMQv3Worker` 專案
2. 右鍵點選專案，選擇「Properties」或「屬性」
3. 導航至「Debug」或「偵錯」頁籤
4. 在 Debug Properties 中設定測試 Job

<br>

---

## 🎯 Event 設定相關

**訂單成單事件分類**：

<br>

訂單成單有分兩個階段：
- **轉單前**：OrderPlaced
- **轉單後**：OrderCreated

<br>

**案例 1：Job 與 Event 綁定問題**

<br>


**問題描述**：

<br>

發現特定的 Job 應該要綁定特定 Event 並執行：`SaveTradesOrderExtendInfoToS3Job` 如預期沒有執行

<br>

**原因分析**：

<br>

QA 環境還沒有把此 Job 綁定到 `OrderPlaced` 上，而是還留在 `OrderCreated`

<br>

**付款方式影響**：

<br>

- 要用信用卡結帳，不能用 2C2P 否則不會成功執行轉單等動作也就是沒有 `OrderCreated` 發生

<br>

**Event 發送的 Job Task Data 分析**：

<br>

- `sourceId` 就是 `EventId`
- `Id` 就是 `TaskId`

<br>

---

## 🖥️ 加開機器流程

**適用情況**：

<br>

發現有大量通知信或 JOB 塞車需要加開機器消化時進行通報

<br>

**通報範例**：

<br>

```
HK Prod NMQV3 目前有大量 SendTemplateMailPriorityLow Job 需要處理，約 9 萬筆以上，還在增加中
將開啟 SG-HK-NMQ3、SG-HK-NMQ4 兩台機器加速消化

cc @jessewang @knighthuang
```

<br>

---

## 🧪 測試資料模板

**NMQV3 餵測試資料的模板**：

<br>

```json
{ "Data":"{\"StartDatetime\":\"2025-03-07 02:45:34.592212\",\"EndDatetime\":\"2025-03-09 02:45:34.592212\"}"}
```

<br>

---

## � 架構分析關鍵字

**NMQV3 核心架構元件**：

<br>

**Host Builder 擴充方法**：

<br>

```csharp
IHostBuilderExtensions.UseNMQv3
```

<br>

**服務註冊**：

<br>

```csharp
services.AddSingleton<IHostedService, WorkerProcess>();
```

<br>

**Worker 處理類別**：

<br>

```csharp
internal class WorkerProcess : BackgroundService
```

<br>

**Worker 處理任務**：

<br>

```csharp
ask workerProcessTask = GetWorkerProcessTask();
```

<br>

**任務執行輸出**：

<br>

```csharp
Console.WriteLine(DoJob(text).ToWorkerState().GetWorkerStateCode());
```

<br>

**處理器執行**：

<br>

```csharp
return _processExecutor.Execute(service, task, _stoppingToken);
```

<br>

---

## �🔄 Batch Redo NMQ Job 程式碼

**參考文件**：

<br>

https://wiki.91app.com/pages/viewpage.action?pageId=352977195

<br>

**主要程式碼**：

<br>

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

<br>

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

<br>

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

<br>

```csharp
HttpClient _httpClient = new HttpClient();
HttpClient _httpClient2 = new HttpClient();
```

<br>

**批次更新任務方法**：

<br>

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

<br>

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

<br>

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

<br>

**使用說明**：

<br>

- 支援市場：TW、HK、MY
- 支援環境：QA、Prod
- 支援操作：Cancel、Redo、Archive
- 支援狀態：READY、DOING、FAILED、CANCELED

<br>

---