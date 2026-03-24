# 直播留言補撈機制 — Livebuy API 改造規格書

## 背景與目的

### 現況問題

`LiveEventPollingService` 目前以 `BackgroundService` 的形式運行在 livebuy API Pod 內：

- API Pod 有 2 個 replica，兩個都跑 BackgroundService → 靠 Redis 分散式鎖防止重複，但浪費資源
- BackgroundService 與 HTTP 服務的生命週期綁在一起 → API 部署或重啟時，補撈短暫中斷
- 補撈頻率（10 秒）無法從外部控制，只能改程式碼
- 排程邏輯與業務邏輯混在同一個類別，不易測試

### 改造目標

1. 將「補撈留言」的**業務邏輯**從 BackgroundService 的排程迴圈中分離出來
2. 開放一個 API Endpoint，讓外部 CronJob 可以主動呼叫補撈
3. 移除 API Pod 中的 `AddHostedService` 註冊，由 CronJob 接管排程責任
4. 確保補撈 Endpoint 不對外公開，有內部安全驗證

---

## 名詞定義

| 名詞 | 說明 |
|------|------|
| **補撈（Catchup）** | 當 Facebook Webhook 未能即時送達留言時，主動向 Facebook Graph API 查詢補回遺漏留言的行為 |
| **活躍直播場次** | 目前 `Status = LIVE` 的直播場次 |
| **留言處理流程** | 從 Facebook 取回留言 → 去重 → 寫入 DynamoDB → 推送 WebSocket |

---

## Step 1：在 `ILiveCommentService` 新增補撈方法，並更新 `LiveCommentService` 依賴

### 為什麼要做

目前 `LiveEventPollingService.PollLiveEventsAsync()` 是 `private` 方法，只能被 `ExecuteAsync` 的 while loop 呼叫，無法被 Controller 觸發。

分析現有的 `LiveCommentService` 後發現：它已經注入了補撈所需的兩個核心依賴：
- `ILiveSessionRepository` → 查詢所有 Status=LIVE 的場次 ✅
- `ISocialPlatformCommentProviderFactory` → 呼叫 Facebook 取得留言 ✅

因此，**不需要建立新的 Service 檔案**。只需在 `ILiveCommentService` 新增一個方法，再為 `LiveCommentService` 補上兩個缺少的依賴即可：
- `ICommentProcessingService` → 留言去重 + 寫入 DynamoDB + 推送 WebSocket（目前未注入）
- `IRedisService` → 分散式鎖，防止多個 Pod 同時處理同一場次（目前未注入）

### 做法

**1a. 修改 `ILiveCommentService.cs`，新增方法宣告：**

```csharp
/// <summary>
/// 對所有 Status=LIVE 的直播場次執行一次留言補撈.
/// 固定往前查詢 60 秒內的留言，透過 ICommentProcessingService 去重後寫入 DynamoDB 並推送 WebSocket.
/// </summary>
/// <param name="cancellationToken">取消權杖.</param>
/// <returns>非同步任務.</returns>
Task CatchupMissedCommentsAsync(CancellationToken cancellationToken = default);
```

**1b. 修改 `LiveCommentService.cs` 建構子，新增兩個依賴：**

```csharp
// 新增兩個 private readonly 欄位
private readonly ICommentProcessingService commentProcessingService;
private readonly IRedisService redisService;

// 建構子新增兩個參數
public LiveCommentService(
    ICommentRepository commentRepository,
    IOrderRepository orderRepository,
    ILiveSessionRepository liveSessionRepository,
    ISocialPlatformCommentProviderFactory socialPlatformCommentProviderFactory,
    ICommentProcessingService commentProcessingService,   // ← 新增
    IRedisService redisService,                           // ← 新增
    ILogger<LiveCommentService> logger)
```

**1c. 在 `LiveCommentService.cs` 實作 `CatchupMissedCommentsAsync`，並從 `LiveEventPollingService` 搬移以下方法（邏輯完全不動）：**

| 原方法（在 LiveEventPollingService） | 新位置（在 LiveCommentService） |
|--------------------------------------|--------------------------------|
| `PollLiveEventsAsync` | → `CatchupMissedCommentsAsync`（改為 `public`，實作 interface） |
| `ProcessLiveEventAsync` | → private，直接搬移 |
| `ConvertToStreamComments` | → private，直接搬移 |
| `TryAddStreamComment` | → private，直接搬移 |
| `IsCommentFromPage` | → private，直接搬移 |
| `IsRateLimitException` | → private，直接搬移 |
| `IsFacebookApiError` | → private，直接搬移 |
| `HandleFacebookApiErrorAsync` | → private，直接搬移 |

**搬移常數**（加到 `LiveCommentService` 的 class 頂端）：

```csharp
//// 固定往前查詢 60 秒內的留言
private static readonly TimeSpan FixedCommentQueryWindow = TimeSpan.FromSeconds(60);

//// -1 代表翻頁抓到底（透過 Provider 自行分頁）
private const int FetchAllCommentsLimit = -1;

//// Rate Limit 觸發後退避 60 秒
private static readonly TimeSpan RateLimitBackoff = TimeSpan.FromSeconds(60);
```

**搬移時唯一需要調整的地方**：`PollLiveEventsAsync` 原本使用 `this.serviceProvider.CreateScope()` 來取得 Scoped 服務，因為它在 BackgroundService (Singleton) 中。搬到 `LiveCommentService` 後，所有依賴都已透過建構子注入，**不需要 CreateScope，直接用 `this.commentProcessingService`、`this.redisService`** 即可。

---

## Step 2：重構 `LiveEventPollingService`，改用 `ILiveCommentService`

### 為什麼要做

Step 1 完成後，`LiveEventPollingService` 原有的業務邏輯已移入 `LiveCommentService`，`BackgroundService` 中存在重複程式碼。需要讓它**只負責排程控制**（Redis 開關、while loop、sleep），把實際補撈邏輯委派給 `ILiveCommentService`。

`LiveEventPollingService` 是 `BackgroundService`（Singleton），無法直接注入 Scoped 的 `ILiveCommentService`。需要透過 `IServiceProvider.CreateScope()` 在每輪迴圈建立短暫的 Scope 取得服務，用完即釋放。

### 做法

修改 `LiveEventPollingService.cs`：

1. 刪除所有業務邏輯方法（`PollLiveEventsAsync`、`ProcessLiveEventAsync` 等）以及已搬移的常數
2. `ExecuteAsync` 的 while loop 改為透過 scope 取得 `ILiveCommentService`，呼叫 `CatchupMissedCommentsAsync`

重構後的 `ExecuteAsync` 核心結構：

```csharp
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    this.logger.LogWarning("🔔 LiveEventPollingService 啟動 - 固定輪詢間隔: {IntervalSeconds} 秒", FixedPollingInterval.TotalSeconds);

    while (stoppingToken.IsCancellationRequested == false)
    {
        try
        {
            //// 1. 檢查 Redis 開關（預設關閉）
            var isEnabledStr = await this.lockService.GetStringAsync(PollingEnabledKey);
            // ... （Redis 開關檢查邏輯保持不變）

            if (isEnabled == false)
            {
                await Task.Delay(FixedPollingInterval, stoppingToken);
                continue;
            }

            //// 2. 透過 Scope 取得 ILiveCommentService（Scoped），執行一次補撈
            using var scope = this.serviceProvider.CreateScope();
            var liveCommentService = scope.ServiceProvider.GetRequiredService<ILiveCommentService>();
            await liveCommentService.CatchupMissedCommentsAsync(stoppingToken);

            //// 3. 等待下次輪詢
            await Task.Delay(FixedPollingInterval, stoppingToken);
        }
        catch (OperationCanceledException) { break; }
        catch (Exception ex) { this.logger.LogError(ex, "輪詢服務執行失敗"); }
    }
}
```

> ⚠️ **此步驟完成後，BackgroundService 仍然存在且運作正常**，只是邏輯已分離。功能不應有任何改變，需要進行迴歸測試確認行為一致。

---

## Step 3：新增 Endpoint — `POST /api/live/comments/catchup`

### 為什麼要做

這是整個改造的核心目的：讓外部 CronJob 能透過 HTTP 呼叫觸發補撈，取代 BackgroundService 的排程角色。

### 命名決策

| 候選命名 | 排除原因 |
|----------|---------|
| `POST /internal/polling/run-once` | 「polling」、「run-once」是技術實作語言，不反映業務行為 |
| `POST /comments/sync` | 太泛用，不清楚是哪種 sync |
| `POST /live/comments/catchup` | ✅ 清楚表達「補撈直播遺漏留言」，且符合現有 `LiveCommentsController` 的路由前綴 `api/live/comments` |

### 為什麼加在 `LiveCommentsController` 而非 `LiveSessionsController`

- `LiveCommentsController` 已注入 `ILiveCommentService`，無需改動建構子
- 路由 `/api/live/comments` 前綴語意正確：這是一個對留言的操作

### 做法

**修改 `LiveCommentsController.cs`**，新增以下 Action（建構子不需改，`ILiveCommentService` 已在其中）：

```csharp
/// <summary>
/// 對所有進行中的直播場次執行一次留言補撈.
/// 從 Facebook Graph API 查詢最近 60 秒內的遺漏留言，補寫入 DynamoDB 並推送 WebSocket.
/// 此 Endpoint 僅供內部 CronJob 呼叫.
/// </summary>
/// <param name="cancellationToken">取消權杖.</param>
/// <returns>執行結果.</returns>
[HttpPost("catchup")]
[AllowAnonymous]
[ProducesResponseType(StatusCodes.Status200OK)]
public async Task<IActionResult> CatchupMissedComments(CancellationToken cancellationToken)
{
    //// 觸發一次跨所有活躍直播場次的留言補撈
    await this.liveCommentService.CatchupMissedCommentsAsync(cancellationToken);
    return this.Ok();
}
```

**完整路由**：`POST /api/live/comments/catchup`

---

## Step 4：確認 DI 注入無需額外設定

### 為什麼要確認

選擇將補撈邏輯加到 `ILiveCommentService`（而非建立新 Service）的一個關鍵好處：**不需要修改 DI 設定**。

### 確認現況

查看 `ServiceCollectionExtension.cs` 後確認：

| 依賴 | 現有狀態 |
|------|---------|
| `ILiveCommentService` → `LiveCommentService` | ✅ 已 `AddScoped`（第 357 行） |
| `ICommentProcessingService` → `CommentProcessingService` | ✅ 已 `AddScoped`（第 353 行） |
| `IRedisService` → `RedisService` | ✅ 已 `AddSingleton`（第 650 行）— Singleton 可安全注入 Scoped |

### 做法

**不需要任何修改**。`LiveCommentService` 建構子新增 `ICommentProcessingService` 和 `IRedisService` 後，.NET DI 容器會自動解析這兩個已註冊的依賴。

---

## Step 5：移除 BackgroundService 排程（待驗證後執行）

### 為什麼要「最後」做

這一步是**不可逆的**，且直接影響現有的補撈機制。必須等到：
1. CronJob 已部署並穩定運行（建議觀察 1~2 週）
2. 確認 `POST /api/live/comments/catchup` 的呼叫記錄正常
3. DynamoDB 中的留言補撈數量與之前 BackgroundService 運行時相近

確認後才執行此步驟。

### 做法

修改 `ServiceCollectionExtension.cs`，**刪除以下一行**：

```csharp
// 刪除：
services.AddHostedService<LiveEventPollingService>();
```

（`LiveStatusPollingService` 的移除時機另行討論，由 LiveStatusPolling CronJob 規格書處理）

---

## 變動檔案清單

| 動作 | 檔案 | 說明 |
|------|------|------|
| 修改 | `BL.Services/Comments/ILiveCommentService.cs` | 新增 `CatchupMissedCommentsAsync` 方法宣告 |
| 修改 | `BL.Services/Comments/LiveCommentService.cs` | 建構子新增 `ICommentProcessingService` 和 `IRedisService`；搬入補撈業務邏輯與輔助方法 |
| 修改 | `BL.Services/Polling/LiveEventPollingService.cs` | 移除業務邏輯方法，改透過 `CreateScope` 取得 `ILiveCommentService` 並呼叫 `CatchupMissedCommentsAsync` |
| 修改 | `Web.Api/Controllers/LiveCommentsController.cs` | 新增 `POST /api/live/comments/catchup`，套用 `[AllowAnonymous]` + `[InternalApiAuth]` |
| 新增 | `Web.Api/Middleware/InternalApiAuthAttribute.cs` | Header Token 驗證 |
| 修改 | `config/` 設定檔 | 新增 `InternalApiToken` 設定項目 |
| **不異動** | `Web.Api/Extension/ServiceCollectionExtension.cs` | ILiveCommentService、ICommentProcessingService、IRedisService 均已註冊，無需修改 |

---

## 測試驗收標準

| 項目 | 驗收條件 |
|------|----------|
| **功能一致性** | 呼叫 `POST /api/live-sessions/comments/catchup` 後，DynamoDB 與 WebSocket 推送的行為與原 BackgroundService 完全一致 |
| **安全性** | 不帶 `X-Internal-Token` 或 token 錯誤時，回傳 `401 Unauthorized` |
| **Redis 鎖有效** | 同時呼叫兩次 catchup，只有一次真正執行，另一次因 Redis lock 被跳過 |
| **Redis 開關有效** | 將 `Nine1:Livebuy:Polling:Enabled` 設為 `false` 時，catchup API 仍回 `200 OK` 但不執行任何查詢（維持原行為） |
| **BackgroundService 移除後** | API Pod 啟動時不再出現 `LiveEventPollingService 啟動` 的 Log |
