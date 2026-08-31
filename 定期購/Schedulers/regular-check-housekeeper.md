# regular-check-housekeeper


> K8s 每天台灣時間 **11:30** 啟動一個短暫 Pod，先從 S3 拉設定檔，然後執行 `php /app/admin/daemon/regular_check_housekeeper.php`，約 33 秒跑完後 Pod 自動消失。負責每日巡檢所有定期購計畫狀態，對異常或即將到期的計畫發出 Slack 告警通知。

---

## 基本資訊

| 欄位 | 值 |
|---|---|
| **名稱** | `regular-check-housekeeper` |
| **Namespace** | `qa-regular-purchase-service` |
| **對應 PHP** | `admin/daemon/regular_check_housekeeper.php` |
| **建立時間** | 2024-04-02 |
| **最後修改** | 2025-11-24 |

---

## 排程設定

| 欄位 | 值 | 說明 |
|---|---|---|
| **schedule** | `30 03 * * *` | 每天 UTC 03:30 執行（= 台灣時間 11:30） |
| **concurrencyPolicy** | `Forbid` | 上一次還沒跑完時，**不允許**同時再啟動新的 Job |
| **suspend** | `false` | 排程啟用中 |
| **failedJobsHistoryLimit** | `1` | 保留最近 1 筆失敗記錄 |
| **successfulJobsHistoryLimit** | `1` | 保留最近 1 筆成功記錄 |
| **restartPolicy** | `Never` | Pod 失敗不重啟，由下次排程重新執行 |

---

## 執行流程（Pod 啟動順序）

```
Step 1：initContainer（aws-config-loader）
  → 從 AWS S3 下載 settings.json 到暫存 Volume
  → aws s3 cp s3://91app-ap-northeast-1-private-conf/
              TW-QA/Regular-Purchase-Service/API/
              LATEST-p-5b4f1c54/settings.json /config;

Step 2：主 Container（regular-check-housekeeper）
  → workingDir: /app
  → command: php /app/admin/daemon/regular_check_housekeeper.php
  → 執行完畢，Pod 結束
```

---

## Container 設定

| 欄位 | 值 |
|---|---|
| **Image** | `docker-dev.build.91app.io/91app/nine1-regular-purchase-service-web-api:vLATEST-p-ed48ecc0-dev-b20251120-0234` |
| **imagePullPolicy** | `IfNotPresent` |
| **workingDir** | `/app` |
| **command** | `php /app/admin/daemon/regular_check_housekeeper.php` |

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
| **lastScheduleTime** | 2026-05-21 03:30:00 UTC |
| **lastSuccessfulTime** | 2026-05-21 03:30:33 UTC |

> 執行時間約 **33 秒**（所有 job 中最長，代表需掃描大量資料）

---

## 翻寫 .NET Core 注意事項

1. **執行時間最長**：33 秒代表需全表掃描定期購計畫，新版需注意 DB query 效能與 timeout 設定。
2. **依賴 Slack Webhook**：發出告警通知需 Slack Webhook URL，需確認新版 Worker 的 Slack 設定來源（secrets 或 settings）。
3. **每日一次**：`30 03 * * *` = 台灣時間 11:30，屬於業務時間內的日報性質 job，設定時需確認 cluster 時區。
4. **successfulJobsHistoryLimit: 1**：只保留 1 筆成功記錄，代表不需要長期追溯歷史。
