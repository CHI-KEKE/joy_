# regular-create-housekeeper


> K8s 每天台灣時間 **10:15** 啟動一個短暫 Pod，先從 S3 拉設定檔，然後執行 `php /app/admin/daemon/regular_create_housekeeper.php`，約 43 秒跑完後 Pod 自動消失。負責每日掃描所有啟用中的定期購計畫，對「今天應建立期明細」的計畫呼叫 91APP API 建立訂單（regular_order_detail）。

---

## 基本資訊

| 欄位 | 值 |
|---|---|
| **名稱** | `regular-create-housekeeper` |
| **Namespace** | `qa-regular-purchase-service` |
| **對應 PHP** | `admin/daemon/regular_create_housekeeper.php` |
| **建立時間** | 2024-04-02 |
| **最後修改** | 2025-11-24 |

---

## 排程設定

| 欄位 | 值 | 說明 |
|---|---|---|
| **schedule** | `15 02 * * *` | 每天 UTC 02:15 執行（= 台灣時間 10:15） |
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

Step 2：主 Container（regular-create-housekeeper）
  → workingDir: /app
  → command: php /app/admin/daemon/regular_create_housekeeper.php
  → 執行完畢，Pod 結束
```

---

## Container 設定

| 欄位 | 值 |
|---|---|
| **Image** | `docker-dev.build.91app.io/91app/nine1-regular-purchase-service-web-api:vLATEST-p-ed48ecc0-dev-b20251120-0234` |
| **imagePullPolicy** | `IfNotPresent` |
| **workingDir** | `/app` |
| **command** | `php /app/admin/daemon/regular_create_housekeeper.php` |

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
| **lastScheduleTime** | 2026-05-21 02:15:00 UTC |
| **lastSuccessfulTime** | 2026-05-21 02:15:43 UTC |

> 執行時間約 **43 秒**（目前所有 job 中最長，代表需批次處理大量計畫與呼叫外部 API）

---

## 翻寫 .NET Core 注意事項

1. **執行時間最長（43 秒）**：需批次掃描所有啟用中計畫並逐一呼叫 91APP API 建立訂單，新版務必注意：
   - DB query 效能（大量計畫分頁查詢）
   - 外部 API 呼叫的 timeout 與 retry 策略
   - 整體執行時間是否會超過 	erminationGracePeriodSeconds: 30（目前設定偏小，值得討論）
2. **防重複建立（rgo_in_schedule）**：PHP 版本用 `rgo_in_schedule = 1` flag 防止同期明細被重複建立，新版必須保留等效機制。
3. **依賴 91APP API**：建立訂單需呼叫外部 API，需確認新版 Worker 的認證設定。
4. **每日一次**：`15 02 * * *` = 台灣時間 10:15，屬於業務時間內的核心日常 job，是整個定期購流程的起點，設定時需確認 cluster 時區。
5. **terminationGracePeriodSeconds: 30 偏小**：執行時間 43 秒但優雅終止只給 30 秒，若 K8s 強制終止 Pod 可能造成訂單建立中斷，新版建議調高至 60 秒以上。
