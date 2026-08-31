# 定期購 CronJob 排程總覽

> 來源：Rancher 匯出的 K8s CronJob YAML（namespace: qa-regular-purchase-service）
> 更新日：2026-05-21

---

## 排程清單

| CronJob 名稱 | 對應 PHP | cron expression | 台灣時間 | 頻率 | 執行時間 |
|---|---|---|---|---|---|
| create-order-housekeeper | create_order_housekeeper.php | `*/1 00-16 * * *` | 08:00 ~ 00:59 | 每 1 分鐘 | ~7 秒 |
| notify-queue-housekeeper | notify_queue_housekeeper.php | `*/5 00-16 * * *` | 08:00 ~ 00:59 | 每 5 分鐘 | ~7 秒 |
| regular-create-housekeeper | regular_create_housekeeper.php | `15 02 * * *` | 10:15 | 每天 | ~43 秒 |
| regular-check-housekeeper | regular_check_housekeeper.php | `30 03 * * *` | 11:30 | 每天 | ~33 秒 |
| create-notify-housekeeper | create_notify_housekeeper.php | `0 06 * * *` | 14:00 | 每天 | ~15 秒 |
| sync-translate-housekeeper | sync_translate_housekeeper.php | `0 14 * * *` | 22:00 | 每天 | ~23 秒 |
| stock-estimate-housekeeper | stock_estimate_housekeeper.php | `0 02 * * 1` | 每週一 10:00 | 每週 | ~13 秒 |

> **注意**：所有 cron expression 均為 **UTC 時間**，台灣時間 = UTC + 8

---

## 每日時間軸（台灣時間）

```
08:00 ┬─ create-order-housekeeper    (每 1 分鐘，持續到隔天 00:59)
      ├─ notify-queue-housekeeper    (每 5 分鐘，持續到隔天 00:59)
10:15 ├─ regular-create-housekeeper  (每天一次)
10:00 ├─ stock-estimate-housekeeper  (每週一)
11:30 ├─ regular-check-housekeeper   (每天一次)
14:00 ├─ create-notify-housekeeper   (每天一次)
22:00 ├─ sync-translate-housekeeper  (每天一次)
01:00 ┴─ create-order / notify-queue 停止輪詢（深夜不執行）
```

---

## 各 Job 特殊說明

| CronJob 名稱 | 特殊點 |
|---|---|
| create-order-housekeeper | 每分鐘輪詢；`successfulJobsHistoryLimit: 3` |
| notify-queue-housekeeper | 每 5 分鐘輪詢；`successfulJobsHistoryLimit: 3` |
| regular-create-housekeeper | 執行時間最長（43 秒）；terminationGracePeriodSeconds(30) 小於執行時間，有強制終止風險 |
| regular-check-housekeeper | 唯一依賴 **Slack Webhook** 的 job |
| stock-estimate-housekeeper | 唯一**週排程**（每週一）；失敗需等下週 |
| sync-translate-housekeeper | 唯一依賴 **Redis** 的 job；.NET Core 翻寫時可能不需要移植 |

---

## 共用設定（所有 Job 相同）

| 設定 | 值 |
|---|---|
| concurrencyPolicy | `Forbid`（不允許並行） |
| restartPolicy | `Never`（失敗不重啟） |
| terminationGracePeriodSeconds | `30` |
| CPU requests/limits | `100m` |
| Memory requests/limits | `128Mi` |
| Image | `nine1-regular-purchase-service-web-api`（與 API 共用） |
| Config 來源 | initContainer 從 AWS S3 下載 settings.json |
| Secrets 來源 | AWS Secrets Store CSI Driver（secretProviderClass: `regular-purchase-service-web-api`） |

---

## 尚未取得 YAML 的 Job

| CronJob 名稱 | 對應 PHP | 備註 |
|---|---|---|
| regular-order-pending-housekeeper | regular_order_pending_housekeeper.php | JOB06，Rancher 上未找到 |
| delete-log-file-housekeeper | delete_log_file_housekeeper.php | JOB09，Rancher 上未找到；.NET Core 版本可能不需要 |

---

## 詳細文件索引

- [create-notify-housekeeper.md](./create-notify-housekeeper.md)
- [create-order-housekeeper.md](./create-order-housekeeper.md)
- [notify-queue-housekeeper.md](./notify-queue-housekeeper.md)
- [regular-check-housekeeper.md](./regular-check-housekeeper.md)
- [regular-create-housekeeper.md](./regular-create-housekeeper.md)
- [stock-estimate-housekeeper.md](./stock-estimate-housekeeper.md)
- [sync-translate-housekeeper.md](./sync-translate-housekeeper.md)
