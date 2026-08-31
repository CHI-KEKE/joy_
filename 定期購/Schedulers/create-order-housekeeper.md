# create-order-housekeeper

> K8s 每天台灣時間 **08:00~00:59（隔天）** 每分鐘啟動一個短暫 Pod，先從 S3 拉設定檔，然後執行 `php /app/admin/daemon/create_order_housekeeper.php`，約 7 秒跑完後 Pod 自動消失。負責消費 order_create_queue，將 91APP 首單事件轉成定期購計畫（regular_order）。

---

## 基本資訊

| 欄位 | 值 |
|---|---|
| **名稱** | `create-order-housekeeper` |
| **Namespace** | `qa-regular-purchase-service` |
| **對應 PHP** | `admin/daemon/create_order_housekeeper.php` |

---

## 排程設定

| 欄位 | 值 | 說明 |
|---|---|---|
| **schedule** | `*/1 00-16 * * *` | UTC 00:00~16:59 每分鐘一次（= 台灣時間 08:00~00:59） |
| **concurrencyPolicy** | `Forbid` | 上一次還沒跑完時，**不允許**同時再啟動新的 Job |
| **suspend** | `false` | 排程啟用中 |
| **failedJobsHistoryLimit** | `1` | 保留最近 1 筆失敗記錄 |
| **successfulJobsHistoryLimit** | `3` | 保留最近 3 筆成功記錄 |
| **restartPolicy** | `Never` | Pod 失敗不重啟，等下次排程（1 分鐘後）再試 |

> **注意**：台灣時間 01:00~07:59（UTC 17:00~23:59）不執行，推測為深夜訂單量極少、節省資源。

---

## 執行流程（Pod 啟動順序）

```
Step 1：initContainer（aws-config-loader）
  → 從 AWS S3 下載 settings.json 到暫存 Volume
  → aws s3 cp s3://91app-ap-northeast-1-private-conf/
              TW-QA/Regular-Purchase-Service/API/
              LATEST-p-5b4f1c54/settings.json /config;

Step 2：主 Container（create-order-housekeeper）
  → workingDir: /app
  → command: php /app/admin/daemon/create_order_housekeeper.php
  → 執行完畢，Pod 結束
```

---

## Container 設定

| 欄位 | 值 |
|---|---|
| **Image** | `docker-dev.build.91app.io/91app/nine1-regular-purchase-service-web-api:vLATEST-p-ed48ecc0-dev-b20251120-0234` |
| **imagePullPolicy** | `IfNotPresent` |
| **workingDir** | `/app` |
| **command** | `php /app/admin/daemon/create_order_housekeeper.php` |

**資源限制：**

| | CPU | Memory |
|---|---|---|
| requests | 100m | 128Mi |
| limits | 100m | 128Mi |

---

## Volume 掛載

| mountPath | 來源 | 說明 |
|---|---|---|
| `/app/config/settings.json` | `config` volume（emptyDir） | 由 initContainer 從 S3 下載 |
| `/app/config/secrets.json` | `secrets` volume（CSI） | 從 AWS Secrets Manager 掛載，key: `regular-purchase-service-web-api` |

---

## 最後執行狀態

| | 時間 |
|---|---|
| **lastScheduleTime** | 2026-05-21 08:02:00 UTC |
| **lastSuccessfulTime** | 2026-05-21 08:02:07 UTC |

> 執行時間約 **7 秒**

---

## 翻寫 .NET Core 注意事項

1. **排程時間窗口**：新版 CronJob 需沿用 `*/1 00-16 * * *`（台灣時間 08:00~00:59），不要改成全天執行。
2. **每分鐘輪詢設計**：這支是 polling 模式，每分鐘掃 order_create_queue 有沒有資料，有才處理。.NET Core Worker 可考慮改為 event-driven（訂單進 queue 時觸發），但需與團隊確認是否有此需求。
3. **successfulJobsHistoryLimit: 3**：此支保留 3 筆成功記錄（其他 job 只保 1 筆），代表需要較多歷史可回溯，可能因為此 job 較重要或偵錯需求較高。
4. **時區**：UTC 00:00~16:59 = 台灣時間 08:00~00:59，設定時需確認 cluster 時區。
