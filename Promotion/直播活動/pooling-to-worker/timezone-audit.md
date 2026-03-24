# Livebuy 時間規範盤點

> 建立日期：2026-03-13
> 結論：**整個系統全程使用 UTC**

---

## 整體流程時間傳遞

```
CronJob PS Script (sync-live-comments.ps1)
  roundTime = (Get-Date).ToUniversalTime() → "2026-03-13T02:49:00Z"
       ↓ referenceTime 傳進 NMQ payload
NMQ Task Payload
  { "liveSessionId": "...", "shopId": 2, "referenceTime": "2026-03-13T02:49:00Z" }
       ↓
NMQ Worker (SyncLiveSessionCommentJob)
  → 打 Livebuy WebAPI
  POST /api/live/comments/sync/{id}?shopId=2&referenceTime=2026-03-13T02:49:00Z
       ↓
LiveCommentService.cs（line 792）
  sinceTimestamp = (referenceTime ?? DateTime.UtcNow).Subtract(FixedCommentQueryWindow 60秒)
  ← 預期收到 UTC DateTime
       ↓
FacebookCommentProvider.cs（line 733）
  since = new DateTimeOffset(sinceTimestamp).ToUnixTimeSeconds()
  ← 轉 Unix timestamp（秒）送給 Facebook Graph API
       ↓
Facebook Graph API
  回傳留言資料（含 Unix timestamp 格式的 created_time）
       ↓
FeedCommentEventHandler.cs
  DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime
  ← Unix timestamp 轉回 UTC DateTime
       ↓
CommentRepository.cs（DynamoDB）
  comment.Timestamp.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffK")
  ← 以 UTC ISO 8601 格式存入 DynamoDB
       ↓
WebSocket 推送前端
  timestamp 也是 UTC
```

---

## 各層時間格式對照

| 層級 | 時間格式 | 說明 |
|------|----------|------|
| CronJob PS | `"yyyy-MM-ddTHH:mm:ssZ"` | UTC ISO 8601，Z 結尾 |
| NMQ Payload | `"yyyy-MM-ddTHH:mm:ssZ"` | 字串傳入 JSON Data |
| WebAPI 參數 | `DateTime?` (UTC) | QueryString 解析後為 UTC DateTime |
| LiveCommentService | `DateTime` (UTC) | `DateTime.UtcNow` 為預設值 |
| Facebook API 送出 | Unix timestamp（秒） | `ToUnixTimeSeconds()` |
| Facebook API 收回 | Unix timestamp（秒） | `DateTimeOffset.FromUnixTimeSeconds()` |
| DynamoDB 儲存 | `"yyyy-MM-ddTHH:mm:ss.fffK"` | UTC ISO 8601，毫秒精度 |
| 程式碼內部 | `DateTime.UtcNow` | 從不使用 `DateTime.Now` |

---

## 程式碼關鍵證據

### LiveCommentService.cs line 792
```csharp
var sinceTimestamp = (referenceTime ?? DateTime.UtcNow).Subtract(FixedCommentQueryWindow);
// FixedCommentQueryWindow = 60 秒，往前多撈一分鐘避免遺漏
```

### FacebookCommentProvider.cs line 733
```csharp
var sinceUnixTimestamp = new DateTimeOffset(sinceTimestamp).ToUnixTimeSeconds();
queryParams.Add($"since={sinceUnixTimestamp}");
```

### FeedCommentEventHandler.cs（Webhook 收到的時間處理）
```csharp
if (long.TryParse(createdTimeStr, out var timestamp))
{
    return DateTimeOffset.FromUnixTimeSeconds(timestamp).UtcDateTime;
}
if (DateTime.TryParse(createdTimeStr, out var parsedTime))
{
    return parsedTime.ToUniversalTime();
}
return DateTime.UtcNow;
```

### CommentRepository.cs（DynamoDB 寫入）
```csharp
private const string DynamoDbIso8601Format = "yyyy-MM-ddTHH:mm:ss.fffK";
["Timestamp"] = new AttributeValue { S = comment.Timestamp.ToUniversalTime().ToString(DynamoDbIso8601Format) },
["CreatedAt"] = new AttributeValue { S = comment.CreatedAt.ToUniversalTime().ToString(DynamoDbIso8601Format) },
```

---

## UTC 對監控的好處

| 情境 | 說明 |
|------|------|
| CloudWatch / Datadog | 預設顯示 UTC，log 時間軸直接對齊 |
| 跨市場（TW / HK / MY） | 時間統一，不需換算 |
| Facebook API 對帳 | Facebook 本身也是 UTC，直接比對 |
| DynamoDB 查詢 | ISO 8601 UTC 字串直接排序比較，不受 DST 影響 |

---

## 結論

- ✅ CronJob 用 `ToUniversalTime()` 是正確的
- ✅ 整個系統從 CronJob → NMQ → Worker → WebAPI → Facebook → DB → WebSocket 全程 UTC
- ✅ 程式碼內部一律 `DateTime.UtcNow`，不用 `DateTime.Now`
- ⚠️ 若未來要在 log 或 UI 顯示台灣時間，需在**顯示層**才做 UTC → CST (UTC+8) 轉換，不應在商業邏輯層轉換
