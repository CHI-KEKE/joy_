# 實作規劃：RegularCheckJob（regular_check_housekeeper）

> 優先序：**第一個**（熱身，零副作用）  
> 對應 PHP：`admin/daemon/regular_check_housekeeper.php`（pg_id=8）  
> 觸發時間：每天 11:30 TW（`30 03 * * *` UTC）  
> Repo：`C:\91APP\regular\worker\nine1.regularpurchase.v2.worker`

---

## 一句話目標

查 `regular_daily_create` 今日紀錄，數字異常時發 Slack 告警。

---

## 架構確認

本 Repo 使用 `Nine1.NMQ.Extensions.Hosting` NMQv3 架構：
- 每個 Job 實作 `IProcess`，透過 NMQ 訊息觸發
- CronJob 由 NMQ Cron 在指定時間 Publish 訊息，Worker 收到後執行

```
NMQ Cron (11:30 TW)
  └─ Publish topic: "RegularCheck"
       └─ RegularCheckJob.DoJob()
              ├─ 查 regular_daily_create
              ├─ 數字異常？
              └─ 是 → POST Slack Webhook
```

---

## 需新增的檔案清單

### DA.DB 專案（`Nine1.Regularpurchase.V2.Worker.Console.DA.DB`）

| 檔案 | 說明 |
|---|---|
| `Entities/RegularDailyCreate.cs` | EF Core Entity，對應 `regular_daily_create` |
| `RegularpurchaseV2WorkerDbContext.cs`（修改）| 加入 `DbSet<RegularDailyCreate>` |

### NMQv3Worker 專案（`Nine1.Regularpurchase.V2.Worker.Console.NMQv3Worker`）

| 檔案 | 說明 |
|---|---|
| `Jobs/RegularCheckJob.cs` | IProcess 實作，主邏輯 |
| `DI/RegularCheckJobDIAttribute.cs` | DI 註冊 Attribute |
| `Contract/IRegularCheckService.cs` | Service 介面 |
| `Service/RegularCheckService.cs` | 查 DB + 判斷邏輯 + 發 Slack |
| `Models/RegularCheckJobSettings.cs` | Config POCO |
| `Program.cs`（修改）| 註冊 `"RegularCheck"` → `RegularCheckJob` |
| `appsettings.json`（修改）| 加入 `_N1CONFIG:RegularCheckJob` 設定 |

---

## DB Entity

### `regular_daily_create` 欄位對應

```csharp
// Entities/RegularDailyCreate.cs
[Table("regular_daily_create")]
public class RegularDailyCreate
{
    [Key]
    [Column("rdc_pk")]
    public int RdcPk { get; set; }

    [Column("rdc_date")]
    public DateOnly RdcDate { get; set; }

    [Column("rdc_estimate")]
    public int RdcEstimate { get; set; }

    [Column("rdc_execute")]
    public int RdcExecute { get; set; }

    [Column("rdc_success")]
    public int RdcSuccess { get; set; }

    [Column("rdc_failed")]
    public int RdcFailed { get; set; }
}
```

---

## 告警邏輯

| 條件 | 告警標題 |
|---|---|
| 今日紀錄不存在 | `第二期成單排程未正常執行，請 <!subteam^{groupId}> 協助查詢原因` |
| `rdc_estimate != rdc_execute` | `第二期成單數量異常，請 <!subteam^{groupId}> 協助查詢原因` |
| `rdc_execute != rdc_success + rdc_failed` | 同上 |
| `rdc_success == 0 && rdc_failed > 0` | 同上 |

### PHP 原始告警訊息格式（保留）

```
username: [TW][QA]RegularPurchase Alert
title:    第二期成單數量異常，請 <!subteam^S039NMHFHCK> 協助查詢原因
text:     預期執行數量：N
          實際執行數量：N
          成功數量：N
          失敗數量：N
```

---

## Config 設定（appsettings.json）

```json
"_N1CONFIG": {
  "RegularCheckJob": {
    "SlackWebhookUrl": "",
    "SlackChannel": "",
    "SlackUserGroupId": "S039NMHFHCK"
  }
}
```

> `SlackWebhookUrl` / `SlackChannel` 從 K8s Secret 注入，不寫死。  
> `SlackUserGroupId` 需向 Infra 確認新環境 Group ID 是否仍為 `S039NMHFHCK`。

---

## Job 主邏輯（虛擬碼）

```csharp
public void DoJob(string data, CancellationToken cancellationToken)
{
    var today = DateOnly.FromDateTime(TimeZoneHelper.ConvertToTaipeiTime(DateTime.UtcNow));
    var record = await _context.RegularDailyCreates
        .FirstOrDefaultAsync(r => r.RdcDate == today, cancellationToken);

    var env = $"[{_settings.Market}][{_settings.Env}]";

    if (record == null)
    {
        await SendSlackAsync(env,
            title: $"第二期成單排程未正常執行，請 <!subteam^{_settings.SlackUserGroupId}> 協助查詢原因",
            text:  "");
        return;
    }

    bool hasAnomaly =
        record.RdcEstimate != record.RdcExecute ||
        record.RdcExecute != (record.RdcSuccess + record.RdcFailed) ||
        (record.RdcSuccess == 0 && record.RdcFailed > 0);

    if (hasAnomaly)
    {
        var text = $"預期執行數量：{record.RdcEstimate}\n" +
                   $"實際執行數量：{record.RdcExecute}\n" +
                   $"成功數量：{record.RdcSuccess}\n" +
                   $"失敗數量：{record.RdcFailed}";

        await SendSlackAsync(env,
            title: $"第二期成單數量異常，請 <!subteam^{_settings.SlackUserGroupId}> 協助查詢原因",
            text:  text);
    }
}
```

---

## Program.cs 需新增

```csharp
map.Add("RegularCheck", typeof(RegularCheckJob));
```

---

## NMQ Cron 設定（需 Infra 協助）

```
Topic:     RegularCheck
Cron:      30 03 * * *   （UTC，= 台灣 11:30）
Payload:   {} （空 JSON 即可，或帶 ExecuteTime）
```

---

## 實作步驟順序

1. **DA.DB** — 建 `RegularDailyCreate` Entity，更新 DbContext
2. **NMQv3Worker** — 建 Settings POCO、DI Attribute、Service、Job
3. **Program.cs** — 註冊 Job
4. **appsettings** — 加 config
5. **本地驗證** — 手動呼叫 Job，確認 Slack 收到告警
6. **請 Infra 設定** NMQ Cron

---

## 待確認事項

| 問題 | 說明 |
|---|---|
| Slack Webhook URL / Channel | 新環境的 Webhook 從哪裡取得？ |
| `SlackUserGroupId` | `S039NMHFHCK` 在新環境是否相同？ |
| `regular_daily_create` 是否已存在於新 DB | 共用 MySQL？還是新 Schema？ |
| NMQ Cron payload 格式 | 帶 `ExecuteTime` 還是空 JSON？ |
| 時區處理工具 | Repo 內是否已有 TaipeiTime helper？ |
