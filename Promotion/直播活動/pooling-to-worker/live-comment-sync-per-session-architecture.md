# Live Comment Sync — Per-Session Architecture

> **狀態**: 設計確認完成，待實作  
> **目標**: 將 all-sessions sync 拆成 Worker 端驅動的 per-session 併發架構

---

## 背景與動機

### 原本的問題

現有的 `POST /api/live/comments/sync` 是一支「一次做所有事」的 API：

```
POST /api/live/comments/sync
  → GetActiveLiveSessions()  ← 查 DynamoDB
  → foreach session (可能 1~20 個):
      ├── Redis lock
      ├── 打 Facebook Graph API
      ├── 比對留言
      └── 寫 DynamoDB
  → 全部完成才回 HTTP 200
```

**問題：** 直播場次越多，這支 API 跑越久。10 個 session × 每個 2~3 秒 = **20~30 秒**，HTTP timeout 風險高，且任何一個 session 出錯都可能影響後面的場次。

---

## 新架構設計

### 架構概念

Worker 自己負責「協調」，API 只負責「執行單一 session」。

```
Worker (每 10 秒一輪)
  ┌─────────────────────────────────────────────┐
  │                                             │
  │  Step 1: GET /api/live/sessions/active      │
  │          → [{liveSessionId, shopId}, ...]   │
  │                                             │
  │  if (no sessions) → log + skip              │
  │                                             │
  │  if (has sessions) →                        │
  │    ForEach-Object -Parallel (ThrottleLimit=10)│
  │      POST /api/live/comments/sync/{id}      │
  │              ?shopId=xxx&referenceTime=xxx  │
  │    (Wait All + 30s per-request timeout)     │
  │                                             │
  │  sleep 剩餘時間 (10s - 已用時間)             │
  └─────────────────────────────────────────────┘
```

### 執行時序範例（10 個 session）

```
t=0.0s  GET /api/live/sessions/active  → 10 個 session (約 0.1s)

t=0.1s  同時發出 10 個 POST (ThrottleLimit=10，全部同時)
        Session A ─────────── (2s)
        Session B ──────────────── (3s)
        Session C ────── (1s)
        ...
        Session J ──────────── (2.5s)

t=3.1s  全部完成 ✅
t=10.0s 下一輪開始 (sleep 剩餘 6.9s)
```

vs 原本 sequential：Session A→B→C→...→J 依序串行 = **25+ 秒**

---

## 執行策略參數

| 參數 | 值 | 說明 |
|------|-----|------|
| 輪詢間隔 | `10s` | **固定 10s**，與 sync 執行時間無關 |
| 並發限制 | `ThrottleLimit = 10` | 最多同時 10 個 sync 在跑 |
| 等待策略 | **Fire & Forget（背景執行）** | sync 丟到 background thread job，main loop 不等待 |
| Per-request timeout | `30s` | 超時算失敗，log 後繼續 |
| Retry | GET active sessions：retry 3 次<br>Per-session sync：**不 retry**（失敗 log，下一輪自然重試） |

### ⚠️ 為什麼不用「Wait All + sleep 剩餘時間」

```
❌ Wait All 設計（前一輪卡住後一輪）：
t=0s   Round 1 開始 sync → 等全部完成
t=15s  Round 1 完成 → sleep 10s
t=25s  Round 2 才開始    ← 延遲了 15s

✅ Fire & Forget 設計（固定間隔）：
t=0s   Round 1 → 丟背景跑，main loop sleep 10s
t=10s  Round 2 → 丟背景跑，main loop sleep 10s
t=20s  Round 3 → 丟背景跑 ...
(Round 1 的 sync 還在跑也沒關係 — Redis lock 防重複處理)
```

---

## API 變更清單

### 1. 新增：`GET /api/live/sessions/active`

**位置**: `LiveSessionsController`

```csharp
[AllowAnonymous]
[HttpGet("active")]
public async Task<IActionResult> GetActiveSessions(CancellationToken cancellationToken)
```

**回傳格式**:
```json
{
  "data": [
    { "liveSessionId": "abc123", "shopId": 12345 },
    { "liveSessionId": "def456", "shopId": 67890 }
  ]
}
```

**說明**: 直接呼叫既有的 `liveSessionRepository.GetActiveLiveSessionsAsync()`，只回傳 Worker 所需的最小欄位（`liveSessionId` + `shopId`）。

---

### 2. 新增：`POST /api/live/comments/sync/{liveSessionId}`

**位置**: `LiveCommentsController`

```csharp
[AllowAnonymous]
[HttpPost("sync/{liveSessionId}")]
public async Task<IActionResult> SyncSingleSession(
    string liveSessionId,
    [FromQuery] long shopId,
    [FromQuery] DateTime? referenceTime,
    CancellationToken cancellationToken)
```

**說明**: 只補撈指定 liveSessionId 的留言，內部呼叫新的 service 方法。

---

### 3. 新增 service 方法：`SyncSingleLiveSessionAsync`

**位置**: `ILiveCommentService` + `LiveCommentService`

```csharp
// ILiveCommentService
Task SyncSingleLiveSessionAsync(
    string liveSessionId,
    long shopId,
    DateTime? referenceTime = null,
    CancellationToken cancellationToken = default);

// LiveCommentService 實作
// 步驟:
// 1. liveSessionRepository.GetByIdAsync(liveSessionId, shopId)
// 2. 呼叫既有的 ProcessLiveEventAsync(entity, referenceTime, cancellationToken)
```

**說明**: `ProcessLiveEventAsync` 是 private 方法，已包含 Redis lock + Facebook Graph API + DB 寫入的完整邏輯，直接複用。

---

### 4. 保留：`POST /api/live/comments/sync`（不動）

`LiveEventPollingService` 仍在使用這支 API（Background Service），保持 backward compatible。

---

## PowerShell 腳本變更

**檔案**: `src/Nine1.Live.Buy.CreateTask/script/call-livebuy-api.ps1`

### 新腳本結構

```powershell
param(
    [string]$ScheduledJob,
    [string]$TaskDataPath
)

$ApiUrl         = $Env:LIVEBUY_API_URL
$IntervalSec    = 10
$MaxRetry       = 3
$RetryDelaySec  = 2
$SyncTimeoutSec = 30
$ThrottleLimit  = 10

# Main loop: fires every 10s regardless of sync execution time
while ($true) {
    $referenceTime = (Get-Date).ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ")

    # Fire sync work as a background thread job — do NOT wait for completion
    # This ensures the 10s interval is never blocked by a slow sync round
    $null = Start-ThreadJob -ScriptBlock {
        param($apiUrl, $refTime, $throttle, $timeoutSec, $retryMax, $retryDelay)

        # Step 1: GET active sessions (with retry)
        $sessions = $null
        for ($attempt = 1; $attempt -le $retryMax; $attempt++) {
            try {
                $resp     = Invoke-RestMethod -Uri "$apiUrl/api/live/sessions/active" -Method GET -TimeoutSec $timeoutSec
                $sessions = $resp.data
                [Console]::WriteLine("[$refTime] [LiveCommentSync] GET active sessions SUCCESS - found $($sessions.Count) session(s)")
                break
            } catch {
                [Console]::WriteLine("WARN [$refTime] [LiveCommentSync] GET active sessions FAILED attempt $attempt/$retryMax - $($_.Exception.Message)")
                if ($attempt -lt $retryMax) { Start-Sleep -Seconds $retryDelay }
            }
        }

        if ($null -eq $sessions) {
            [Console]::WriteLine("WARN [$refTime] [LiveCommentSync] All retries failed, skipping this interval")
            return
        }

        if ($sessions.Count -eq 0) {
            [Console]::WriteLine("[$refTime] [LiveCommentSync] No active live sessions, skipping sync")
            return
        }

        # Step 2: sync each session concurrently
        $sessions | ForEach-Object -Parallel {
            $s = $_
            try {
                Invoke-RestMethod `
                    -Uri "$using:apiUrl/api/live/comments/sync/$($s.liveSessionId)?shopId=$($s.shopId)&referenceTime=$using:refTime" `
                    -Method POST `
                    -TimeoutSec $using:timeoutSec
                [Console]::WriteLine("[$using:refTime] [LiveCommentSync] SYNC SUCCESS - liveSessionId=$($s.liveSessionId) shopId=$($s.shopId)")
            } catch {
                [Console]::WriteLine("WARN [$using:refTime] [LiveCommentSync] SYNC FAILED - liveSessionId=$($s.liveSessionId) shopId=$($s.shopId) err=$($_.Exception.Message)")
            }
        } -ThrottleLimit $throttle

    } -ArgumentList $ApiUrl, $referenceTime, $ThrottleLimit, $SyncTimeoutSec, $MaxRetry, $RetryDelaySec

    # Always sleep exactly IntervalSec — previous round never blocks this
    Start-Sleep -Seconds $IntervalSec

    # Clean up completed thread jobs to avoid memory accumulation
    Get-Job | Where-Object { $_.State -in 'Completed', 'Failed' } | Remove-Job
}
```

> **補充說明**：
> - `Start-ThreadJob` 在 PowerShell 7 內建，不需額外安裝
> - `[Console]::WriteLine()` 直接寫到 process stdout，在容器 log 中可見（不受 thread job output stream 限制）
> - 每輪結束後 `Remove-Job` 清理已完成的 job，避免記憶體持續增加
> - 若上一輪 sync 還在跑時下一輪已開始，**Redis lock（10s TTL）會防止同一 session 被重複處理**

---

## 受影響的檔案

### `nine1.live.buy` (API 站)

| 檔案 | 異動類型 | 內容 |
|------|---------|------|
| `Controllers/LiveSessionsController.cs` | 新增 endpoint | `GET /api/live/sessions/active` |
| `Controllers/LiveCommentsController.cs` | 新增 endpoint | `POST /api/live/comments/sync/{liveSessionId}` |
| `BL.Services/Comments/ILiveCommentService.cs` | 新增介面方法 | `SyncSingleLiveSessionAsync` |
| `BL.Services/Comments/LiveCommentService.cs` | 新增實作 | `SyncSingleLiveSessionAsync` → 查單一 session → 呼叫 `ProcessLiveEventAsync` |

### `nine1.live.buy.cronjob` (Worker)

| 檔案 | 異動類型 | 內容 |
|------|---------|------|
| `script/call-livebuy-api.ps1` | 改寫 | 兩步驟 + ForEach-Object -Parallel |

---

## 不需要異動的部分

| 項目 | 理由 |
|------|------|
| `POST /api/live/comments/sync`（舊 all-sessions） | `LiveEventPollingService` 仍在用，保留 |
| `ProcessLiveEventAsync`（private 方法） | 邏輯不動，直接複用 |
| Helm charts / values yaml | Worker 部署設定已完成 |
| `nine1-devops-deployments.QA.json` | APP_ROLE: worker 已設定 |

---

## 新舊架構對比

| 項目 | 舊架構 | 新架構 |
|------|--------|--------|
| API 呼叫次數 | 1次（all-in-one） | 1次 GET + N次 POST sync |
| 單次 API 執行時間 | O(N × session) — 無上限 | O(1 session) — 固定可預期 |
| 10 個 session 耗時 | ~25 秒（sequential） | ~3 秒（concurrent） |
| HTTP timeout 風險 | 高（session 多時） | 無（每個 call 都很短） |
| 無直播時 | 仍打 DB + Redis | 只打 1 次輕量 GET |
| Session 失敗隔離 | 無（一個失敗可能卡住後面） | 完全隔離（各自獨立） |
| Worker log 可見度 | 整批成功/失敗 | 每個 session ID 的成功/失敗 |
| Retry 顆粒度 | 整批 retry | Session 失敗 → 下一輪自然再試 |

---

## Repository 方法確認

`GetActiveLiveSessionsAsync` 已存在於 `ILiveSessionRepository` / `LiveSessionRepository`：

```csharp
// 查 DynamoDB GSI: Status-StartTime-index, hashKey = "Live"
// 回傳 List<LiveSessionEntity>，含 LiveSessionId, ShopId, Platform 等完整欄位
Task<List<LiveSessionEntity>> GetActiveLiveSessionsAsync(CancellationToken cancellationToken = default);
```

新的 `GET /api/live/sessions/active` 直接呼叫此方法，只回傳 `liveSessionId` + `shopId`。

---

## 防重複處理機制（三層）

兩輪重疊（上一輪 sync 還沒完成、下一輪已開始）時，以下三層機制確保資料正確性：

### Layer 1：Redis Lock（事前防線 — 效能保護）

```
用途：防止多個 Worker Pod 同時處理同一個 session
TTL：10s（等於輪詢間隔）

單 Pod 情況：TTL 到期後下一輪可取得 lock → 不阻擋
多 Pod 情況：同時只有一個 Pod 能取得 lock → 防止 Facebook API 被重複呼叫
```

> **補充**：Lock TTL = 輪詢間隔 = 10s，因此每輪開始時 lock 已過期，  
> 下一輪永遠不會因為 lock 未釋放而 FAIL 或跳過。

### Layer 2：NMQ Worker 去重（事後防線 — 資料正確性）

```
位置：nine1.live.buy.worker / ProcessLiveCommentJob.HandleCommentDeduplicationAsync()
方式：以 CommentId 查 DynamoDB

if (existingComment.IsProcessed == true) → 跳過，不重複建立訂單
if (existingComment.IsProcessed == false) → redo，繼續處理
if (not found) → 新留言，正常處理
```

### Layer 3：前端 mergeAndSortMessages（UI 防線）

```typescript
// useLiveComments.ts
// key = messageId（Facebook CommentId），Map 結構天然去重
const messageMap = new Map<string, MessageItem>();
incomingMessages.forEach((msg) => {
  if (messageMap.has(msg.messageId)) {
    messageMap.set(msg.messageId, { ...existing, ...msg }); // 覆蓋更新
  } else {
    messageMap.set(msg.messageId, msg); // 新增
  }
});
// → WebSocket 推送同一則留言兩次，UI 只顯示一次
```

### 三層總覽

| 層 | 機制 | 保護目標 | 適用情境 |
|---|------|---------|---------|
| Redis Lock | 同一 session 只有一個 Pod 處理 | Facebook API Rate Limit、NMQ 流量 | 多 Pod |
| NMQ Worker 去重 | IsProcessed flag + CommentId | 訂單不重複建立、DB 不重複寫入 | 任何情況 |
| 前端 messageMap | messageId 為 key 的 Map | UI 不顯示重複留言 | 任何情況 |

---

## 監控策略

### 使用 Grafana + Loki

| 監控項目 | 方式 | 說明 |
|---------|------|------|
| **Worker Pod 存活** | Grafana K8s Pod 健康狀態 | Pod 掛掉、CrashLoopBackOff、OOM Kill |
| **Web API 錯誤** | Loki log alert（HTTP 5xx、exception） | sync API 或 active sessions API 拋錯 |
| **Web API 無流量** | Loki 無 incoming request alert | Worker 沒有在呼叫 API（可能是 Worker 掛了但 Pod 還活著） |

### 監控覆蓋範圍

| 情境 | Worker Pod 監控 | Web API 監控 |
|------|:---:|:---:|
| Pod 掛掉 | ✅ | ❌ |
| Script crash（process 結束） | ✅ | ❌ |
| GET active sessions 持續失敗 | ❌ | ✅（API 有 error log） |
| sync API 回 500 | ❌ | ✅ |
| Worker 停止呼叫 API | ❌ | ✅（無 incoming request） |
| Facebook Rate Limit 觸發 | ❌ | 另外的機制監控 |

> **注意**：Web API「無 incoming request」alert 是關鍵補強，  
> 能抓到「Pod 還活著但 Worker script 不正常」的盲區。
