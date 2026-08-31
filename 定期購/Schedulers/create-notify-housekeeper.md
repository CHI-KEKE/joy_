# create-notify-housekeeper


> K8s 每天台灣時間 **14:00** 啟動一個短暫 Pod，先從 S3 拉設定檔，然後執行 `php /app/admin/daemon/create_notify_housekeeper.php`，約 15 秒跑完後 Pod 自動消失。K8s 負責「什麼時候跑」，PHP 檔案負責「跑什麼邏輯」，沒有其他魔法。

---

## 基本資訊

| 欄位 | 值 |
|---|---|
| **名稱** | `create-notify-housekeeper` |
| **Namespace** | `qa-regular-purchase-service` |
| **對應 PHP** | `admin/daemon/create_notify_housekeeper.php` |

---

## 排程設定

| 欄位 | 值 | 說明 |
|---|---|---|
| **schedule** | `0 06 * * *` | 每天 UTC 06:00 執行（= 台灣時間 14:00） |
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

Step 2：主 Container（create-notify-housekeeper）
  → workingDir: /app
  → command: php /app/admin/daemon/create_notify_housekeeper.php
  → 執行完畢，Pod 結束
```

---

## Container 設定

| 欄位 | 值 |
|---|---|
| **Image** | `docker-dev.build.91app.io/91app/nine1-regular-purchase-service-web-api:vLATEST-p-ed48ecc0-dev-b20251120-0234` |
| **imagePullPolicy** | `IfNotPresent`（本地有就不重拉） |
| **workingDir** | `/app` |
| **command** | `php /app/admin/daemon/create_notify_housekeeper.php` |

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
| **lastScheduleTime** | 2026-05-21 06:00:00 UTC |
| **lastSuccessfulTime** | 2026-05-21 06:00:15 UTC |

> 執行時間約 **15 秒**