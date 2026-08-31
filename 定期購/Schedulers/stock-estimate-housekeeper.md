# stock-estimate-housekeeper


> K8s **每週一** 台灣時間 **10:00** 啟動一個短暫 Pod，先從 S3 拉設定檔，然後執行 `php /app/admin/daemon/stock_estimate_housekeeper.php`，約 13 秒跑完後 Pod 自動消失。負責每週預估下一期定期購訂單的商品庫存是否充足，提前發現缺貨風險。

---

## 基本資訊

| 欄位 | 值 |
|---|---|
| **名稱** | `stock-estimate-housekeeper` |
| **Namespace** | `qa-regular-purchase-service` |
| **對應 PHP** | `admin/daemon/stock_estimate_housekeeper.php` |
| **建立時間** | 2024-04-02 |
| **最後修改** | 2025-11-24 |

---

## 排程設定

| 欄位 | 值 | 說明 |
|---|---|---|
| **schedule** | `0 02 * * 1` | 每週一 UTC 02:00 執行（= 台灣時間 **每週一 10:00**） |
| **concurrencyPolicy** | `Forbid` | 上一次還沒跑完時，**不允許**同時再啟動新的 Job |
| **suspend** | `false` | 排程啟用中 |
| **failedJobsHistoryLimit** | `1` | 保留最近 1 筆失敗記錄 |
| **successfulJobsHistoryLimit** | `1` | 保留最近 1 筆成功記錄 |
| **restartPolicy** | `Never` | Pod 失敗不重啟，等下週一再試 |

> **⚠️ 特殊**：這是所有 job 中唯一**非每天執行**的排程，以「週」為單位。

---

## 執行流程（Pod 啟動順序）

```
Step 1：initContainer（aws-config-loader）
  → 從 AWS S3 下載 settings.json 到暫存 Volume
  → aws s3 cp s3://91app-ap-northeast-1-private-conf/
              TW-QA/Regular-Purchase-Service/API/
              LATEST-p-5b4f1c54/settings.json /config;

Step 2：主 Container（stock-estimate-housekeeper）
  → workingDir: /app
  → command: php /app/admin/daemon/stock_estimate_housekeeper.php
  → 執行完畢，Pod 結束
```

---

## Container 設定

| 欄位 | 值 |
|---|---|
| **Image** | `docker-dev.build.91app.io/91app/nine1-regular-purchase-service-web-api:vLATEST-p-ed48ecc0-dev-b20251120-0234` |
| **imagePullPolicy** | `IfNotPresent` |
| **workingDir** | `/app` |
| **command** | `php /app/admin/daemon/stock_estimate_housekeeper.php` |

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
| **lastScheduleTime** | 2026-05-18 02:00:00 UTC（週一） |
| **lastSuccessfulTime** | 2026-05-18 02:00:13 UTC |

> 執行時間約 **13 秒**

---

## 翻寫 .NET Core 注意事項

1. **週排程**：`0 02 * * 1` 是每週一，cron expression 最後一欄 `1` 代表週一（0=週日，1=週一...）。新版需確認使用的 cron library 週數定義一致。
2. **失敗無重試**：`restartPolicy: Never` + 週排程 = 失敗就要等下週一，風險較高，新版可考慮加上告警通知（如 Slack）或手動重跑機制。
3. **依賴 91APP 庫存 API**：預估庫存需呼叫外部 API，需確認新版 Worker 的認證設定。
4. **時區**：`0 02 * * 1` = 台灣時間週一 10:00，設定時需確認 cluster 時區。
