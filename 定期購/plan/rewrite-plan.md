# 定期購排程系統重構計畫（PHP → .NET Core）

## 背景

將「定期購」系統從 PHP（GameBall 架構）翻寫為 .NET Core（Nine1 架構）。
本次負責範圍：**排程系統（CronJob）**。

PHP 與 .NET Core 共用同一組 MySQL，透過 `spi_regular_provider` 欄位區隔：
- `GameBall` → PHP CronJob 繼續跑
- `Nine1` → .NET Core CronJob 跑

---

## 翻寫優先序

| 優先 | 排程 / 元件 | 說明 |
|:---:|---|---|
| **1** | `regular_check_housekeeper` | 純監控，零副作用，用來熱身、驗證新架構 |
| **2** | `create_notify_housekeeper` | INSERT notify_queue + Publish NMQ |
| **2** | NMQ Consumer: `RegularOrderNotifyHandler` | 接 NMQ → 呼叫通知 API → 更新 notify_queue |
| **3** | `regular_create_housekeeper` | 最核心，前兩步驗收後再動 |
| **4** | `stock_estimate_housekeeper` | 商家庫存通知，可合併進 notify CronJob |
| **5** | `regular_order_pending_housekeeper` | Pending 邊緣案例 |
| — | `notify_queue_housekeeper` | **不需翻寫**（PHP 繼續處理 GameBall） |
| — | `create_order_housekeeper` | **不需翻寫**（Nine1 首單走 NMQ） |
| — | `sync_translate_housekeeper` | **不需翻寫**（多語系，與定期購無關） |

---

## 新版通知架構設計

### 設計目標

- 不再寫 queue 再消費 queue（廢除 `notify_queue_housekeeper` 輪詢模式）
- CronJob 直接 Publish NMQ，Consumer 即時消費
- `notify_queue` 保留，改當**純紀錄表**（防重複 + 稽核 log）

### 流程

```
CronJob (create_notify_housekeeper)
  │
  ├─ 掃 regular_order
  │     D-3: CashOnDelivery / CreditCard 即將扣款提醒
  │     D-1: CreditCardFailed / CreateFailed 失敗通知
  │
  ├─ 查 notify_queue：(rgo_pk, type) 今天有沒有紀錄？
  │       有 → 跳過（防重複）
  │       沒有 ↓
  │
  ├─ INSERT notify_queue（nq_send_status = 0，當紀錄用）
  │
  └─ Publish NMQ（帶 rgo_pk + type，MessageId 防重）

NMQ Consumer (RegularOrderNotifyHandler)
  │
  ├─ 呼叫通知 API（SMS / Email / Push）
  │
  ├─ 成功 → UPDATE notify_queue SET nq_send_status = 1
  └─ 失敗 → UPDATE notify_queue SET nq_send_status = 2
             （由 NMQ 平台 retry，不再靠 DB nq_send_limit）
```

### 與現行 PHP 差異

| 項目 | 現行 PHP | 新版 .NET Core |
|---|---|---|
| 建紀錄 | `create_notify_housekeeper` INSERT | CronJob INSERT（相同） |
| 發送者 | `notify_queue_housekeeper` 輪詢 DB | NMQ Consumer 即時消費 |
| 重試機制 | `nq_send_limit = 3`（DB 欄位）| NMQ 平台 retry |
| `notify_queue` 角色 | 佇列 + 紀錄 | **純紀錄** |
| 需要幾支 CronJob | 2 支（create + queue）| **1 支**（create）|

### NMQ MessageId 設計

```
MessageId = "{rgo_pk}_{type}_{date}"
範例：123456_CreditCard_20240315
```

- NMQ 平台用 MessageId 去重，即使 CronJob 重跑也不重複發送
- `notify_queue` 的 `(rgo_pk, type)` UNIQUE KEY 為第二道防線

---

## 各排程分析文件索引

| 排程 | 文件路徑 |
|---|---|
| regular_create_housekeeper（Step①②） | `scheduled-jobs/regular-create-housekeeper-1-2.md` |
| regular_create_housekeeper（Step③） | `scheduled-jobs/regular-create-housekeeper-3.md` |
| regular_create_housekeeper（Step④） | `scheduled-jobs/regular-create-housekeeper-4.md` |
| regular_create_housekeeper（Step⑤） | `scheduled-jobs/regular-create-housekeeper-5.md` |
| create_notify_housekeeper | `scheduled-jobs/create-notify-housekeeper.md` |
| notify_queue_housekeeper | `scheduled-jobs/notify-queue-housekeeper.md` |
| regular_check_housekeeper | `scheduled-jobs/regular-check-housekeeper.md` |
| regular_order_pending_housekeeper | `scheduled-jobs/regular-order-pending-housekeeper.md` |
| stock_estimate_housekeeper | `scheduled-jobs/stock-estimate-housekeeper.md` |
| sync_translate_housekeeper | `scheduled-jobs/sync-translate-housekeeper.md` |
| 四個狀態欄位說明 | `analysis/status-fields-explained.md` |

---

## 重要技術決策

### Provider 過濾
所有新版查詢加上 `JOIN shop_info ON spi_shop_id = rgo_shop_id WHERE spi_regular_provider = 'Nine1'`

### Pending 設計限制（已知問題）
現行 PHP `regular_order_pending_housekeeper` 的 SQL 限定 `rod_daemon_active_time = 今天`，
Pending 訂單隔天就撈不到。新版翻寫時需要評估是否修正此設計。

### `regular_order` 無通知欄位
`regular_order` 表本身沒有 `notify_time` 或 `notify_status` 欄位，
通知狀態完全靠 `notify_queue` 表管理，新版沿用此設計。
