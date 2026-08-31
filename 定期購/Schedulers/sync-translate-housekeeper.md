# sync-translate-housekeeper

> K8s 每天台灣時間 **22:00** 啟動一個短暫 Pod，先從 S3 拉設定檔，然後執行 `php /app/admin/daemon/sync_translate_housekeeper.php`，約 23 秒跑完後 Pod 自動消失。負責每晚從 Redis 同步多語系翻譯資料到本地快取，確保前台顯示的文字翻譯保持最新。

---

## 基本資訊

| 欄位 | 值 |
|---|---|
| **名稱** | `sync-translate-housekeeper` |
| **Namespace** | `qa-regular-purchase-service` |
| **對應 PHP** | `admin/daemon/sync_translate_housekeeper.php` |
| **建立時間** | 2024-04-02 |
| **最後修改** | 2025-11-24 |

---

## 排程設定

| 欄位 | 值 | 說明 |
|---|---|---|
| **schedule** | `0 14 * * *` | 每天 UTC 14:00 執行（= 台灣時間 **22:00**） |
| **concurrencyPolicy** | `Forbid` | 上一次還沒跑完時，**不允許**同時再啟動新的 Job |
| **suspend** | `false` | 排程啟用中 |
| **failedJobsHistoryLimit** | `1` | 保留最近 1 筆失敗記錄 |
| **successfulJobsHistoryLimit** | `1` | 保留最近 1 筆成功記錄 |
| **restartPolicy** | `Never` | Pod 失敗不重啟，由下次排程重新執行 |

> **特殊**：所有 job 中執行時間最晚（台灣時間 22:00），屬於離峰時段的維護性 job。

---

## 執行流程（Pod 啟動順序）

```
Step 1：initContainer（aws-config-loader）
  → 從 AWS S3 下載 settings.json 到暫存 Volume
  → aws s3 cp s3://91app-ap-northeast-1-private-conf/
              TW-QA/Regular-Purchase-Service/API/
              LATEST-p-5b4f1c54/settings.json /config;

Step 2：主 Container（sync-translate-housekeeper）
  → workingDir: /app
  → command: php /app/admin/daemon/sync_translate_housekeeper.php
  → 執行完畢，Pod 結束
```

---

## Container 設定

| 欄位 | 值 |
|---|---|
| **Image** | `docker-dev.build.91app.io/91app/nine1-regular-purchase-service-web-api:vLATEST-p-ed48ecc0-dev-b20251120-0234` |
| **imagePullPolicy** | `IfNotPresent` |
| **workingDir** | `/app` |
| **command** | `php /app/admin/daemon/sync_translate_housekeeper.php` |

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
| **lastScheduleTime** | 2026-05-20 14:00:00 UTC |
| **lastSuccessfulTime** | 2026-05-20 14:00:23 UTC |

> 執行時間約 **23 秒**

---

## 翻寫 .NET Core 注意事項

1. **依賴 Redis**：這是所有 job 中**唯一依賴 Redis** 的排程，新版 Worker 需確認 Redis 連線設定（host、port、password）已正確注入。
2. **翻寫評估**：此 job 目的是同步 PHP 系統的翻譯快取，若 .NET Core 版本使用不同的多語系機制（如 resource file 或 i18n library），此 job **可能不需要保留**，需與產品確認。
3. **離峰排程**：`0 14 * * *` = 台灣時間 22:00，選在離峰時段執行，翻寫時應保持相同時段。
4. **時區**：UTC 14:00 = 台灣時間 22:00，設定時需確認 cluster 時區。
