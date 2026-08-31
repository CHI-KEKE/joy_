# 定期購排程系統（.NET Core 新架構）— Job 總覽

> 版本：2026-06-25  
> 架構：NMQv3 Worker（Nine1.Regularpurchase.V2.Worker）  
> 舊架構對照：PHP CronJob（admin/daemon/）

---

## 時間軸一覽（台灣時間 UTC+8）

```
08:00         10:15         10:30          11:30        14:00        22:00
  │             │             │              │             │            │
  ▼             ▼             ▼              ▼             ▼            │
InitJob      CreateOrder   PendingJob    AuditJob   NotifyJob      (不移植)
(事件驅動)    (主核心)       (MY only)     (稽核)    FailedNotify
                                                       ↑
                                               UTC 06:00 = TW 14:00

每週一 TW 10:00 → StockNotifyJob
```

---

## Job 列表速查

| Job 名稱 | NMQ Topic | 排程 | 對應 PHP |
|---------|-----------|------|---------|
| [RegularOrderInitJob](#1-regularorderinitjob) | `RegularOrderInit` | 事件驅動 (NMQ) | `create_order_housekeeper.php` |
| [RegularCreateOrderJob](#2-regularcreateorderjob) | `RegularCreateOrder` | `15 02 * * *`（TW 10:15）| `regular_create_housekeeper.php` |
| [RegularCheckPendingPaymentJob](#3-regularcheckpendingpaymentjob) | `RegularCheckPendingPayment` | `30 02 * * *`（TW 10:30，MY only）| `regular_order_pending_housekeeper.php` |
| [RegularCreateOrderAuditJob](#4-regularcreateorderauditjob) | `RegularCreateOrderAudit` | `30 03 * * *`（TW 11:30）| `regular_check_housekeeper.php` |
| [RegularCreateOrderNotifyJob](#5-regularcreateordernotifyjob) | `RegularCreateOrderNotify` | `0 06 * * *`（TW 14:00）| `create_notify_housekeeper.php`（D-3） + `notify_queue_housekeeper.php` |
| [RegularOrderFailedNotifyJob](#6-regularorderfailednotifyjob) | `RegularOrderFailedNotify` | `0 06 * * *`（TW 14:00）| `create_notify_housekeeper.php`（D-1）+ `notify_queue_housekeeper.php` |
| [RegularOrderStockNotifyJob](#7-regularorderstocknotifyjob) | `RegularOrderStockNotify` | `0 02 * * 1`（每週一 TW 10:00）| `stock_estimate_housekeeper.php` + `notify_queue_housekeeper.php` |

---

## 1. RegularOrderInitJob

### 觸發時機
- **事件驅動**，由 NewOrder API 在顧客訂閱定期購後透過 NMQ 發送訊息觸發
- 非排程，不需 CronJob

### 職責
> 根據顧客首筆訂單建立定期購主計畫（`regular_order`）與第一期明細（`regular_order_detail`）

### 執行流程

```
接收 NMQ → 呼叫 SCM API 取得首單資料
  ├─ V2_SalesOrder_Get(TgCode, TsCode)   → 取得訂單主從代碼
  └─ V2_RegularOrder_Get(ShopId, ...)    → 取得定期購計畫資訊（商品、頻率、收件人）
     ├─ Member_Info UPSERT               → 建立或更新會員資料
     ├─ Regular_Product.load()           → 確認商品設定存在
     ├─ [Transaction]
     │   ├─ INSERT regular_order         → 建立主計畫（ship_count=1, ship_date=首單日+2天）
     │   └─ INSERT regular_order_detail  → 建立首期明細（rod_ship_sequence=1）
     └─ UpsertRegularOrderMap()          → 建立 rgo → rod 的對應
```

### 查詢條件（讀取資料）

| 資料來源 | 條件 |
|---------|------|
| **SCM API** `V2_SalesOrder_Get` | NMQ 傳入的 `TgCode`、`TsCode` |
| **SCM API** `V2_RegularOrder_Get` | `ShopId` + 訂單資訊 |
| `regular_product` | `rpd_shop_id = ShopId AND rpd_page_id = PageId` |
| `member_info` | `mbi_mbid = MemberId AND mbi_shopid = ShopId` |

### 寫入資料表

| 資料表 | 操作 | 說明 |
|--------|------|------|
| `regular_order` | INSERT | 定期購主計畫（rgo_ship_count=1） |
| `regular_order_detail` | INSERT | 首期配送明細（rod_ship_sequence=1, rod_create_order_status=1） |
| `member_info` | INSERT/UPDATE | 會員資料 UPSERT |
| `regular_order_map` | UPSERT | rgo_pk → rod_pk 對應 |

### 重要常數

| 常數 | 值 | 說明 |
|------|-----|------|
| `GenerateNewOrderDiffDays` | `2` | 首期出貨日 = 首單日 + 2 天 |

---

## 2. RegularCreateOrderJob

### 觸發時機
- **每日** `15 02 * * *`（UTC）= 台灣時間 **10:15**
- 三個市場（TW / HK / MY）各自獨立執行

### 職責
> 第 2 期以後的訂單自動化建單，是整個定期購的核心排程。
> 依序執行 5 個步驟：預估 → 廢棄延展 → 建 detail → 打 API → 統計

### 執行流程

```
Step① EstimateOrderCount    → 寫入今日預估建單量（regular_daily_create）
Step② UpdateFailOrderShippingDate → 廢棄配送日已過的失敗期數，推進週期
Step③ GenerateRegularOrderDetail  → 為下次配送日 = 今天+2天 的訂單建立 detail 佔位
Step④ TakeOrderApiAsync           → 對 2 天內的 detail 呼叫 SubscriptionOrderAutoGen API
Step⑤ CheckOrderCreate            → 回填今日成功/失敗統計到 regular_daily_create
```

### 各 Step 查詢條件

**Step① 預估（不影響業務）**

| 查詢目標 | 條件 |
|---------|------|
| `regular_order_detail`（A：待打 API 數） | `rod_status=1` AND `rod_create_order_status IN (0,2)` AND `rod_ship_date > today` AND `DATEDIFF(rod_ship_date, today) <= 2` |
| `regular_order`（B：待建 detail 數） | `DATEDIFF(rgo_next_ship_date, today) = 2` AND 下方③相同條件 |

**Step② 廢棄過期失敗期**

```sql
-- 查主計畫
WHERE rgo_status = 1
  AND spi_enable_regular = 1
  AND spi_regular_provider = 'Nine1V2'   ← 新架構加的過濾
  AND rgo_create_order_status = 2        -- 主計畫層失敗
  AND rgo_next_ship_date < today         -- 配送日已過

-- 廢棄 detail
UPDATE regular_order_detail SET rod_status = 2
WHERE rod_rgo_pk = ? AND rod_status = 1
  AND rod_create_order_status = 2 AND rod_ship_date < today

-- 推進主計畫週期
rgo_next_ship_date += rgo_ship_period
rgo_in_schedule = 0
```

**Step③ 建立 detail 佔位**

```sql
WHERE rgo_status = 1
  AND rgo_in_schedule = 0               -- 尚未進入排程
  AND rgo_ship_period > 0               -- 非一次性
  AND spi_enable_regular = 1
  AND spi_regular_provider = 'Nine1V2'
  AND DATEDIFF(rgo_next_ship_date, today) = 2   -- 配送日剛好 2 天後
  AND rgo_next_ship_date < rgo_pause_date
  AND (rgo_ship_limit_count = 0 OR rgo_ship_count < rgo_ship_limit_count)
```

**Step④ 打 API 建真實訂單**

```sql
WHERE rod_status = 1
  AND rod_create_order_status IN (0, 2)  -- 待建單 or 失敗可重試
  AND rod_ship_date > today              -- 配送日未過
  AND DATEDIFF(rod_ship_date, today) <= 2
  AND spi_enable_regular = 1
  AND spi_regular_provider = 'Nine1V2'
  AND rgo_status <> 3
```

呼叫：`mweb/Webapi_TradesOrderV2/SubscriptionOrderAutoGen`

| API 結果 | rod 處理 | rgo 處理 |
|---------|---------|---------|
| ✅ Status=Success, FlowStatus≠WaitingToPay | `rod_create_order_status=1`，填入 tg/ts code | 推進 ship_date、next_ship_date、ship_count；`in_schedule=0` |
| ⏳ Status=Success, FlowStatus=WaitingToPay | `rod_status=3`（Pending） | 不更新 |
| ❌ Status≠Success 或例外 | `rod_create_order_status=2`，填 error_code | `rgo_create_order_status=2` |

### 寫入資料表

| 資料表 | 操作 | 說明 |
|--------|------|------|
| `regular_daily_create` | INSERT + UPDATE | Step① 寫預估，Step⑤ 回填結果 |
| `regular_order` | UPDATE | 推進日期、in_schedule、完結判斷 |
| `regular_order_detail` | INSERT + UPDATE | 建佔位、寫 API 結果 |
| `regular_order_map` | UPSERT | Step④ 成功後更新 |
| `create_regular_log` | INSERT | 每次 API 呼叫都記錄（成功/失敗都寫） |

### 重要常數

| 常數 | 值 | 說明 |
|------|-----|------|
| `GenerateNewOrderDiffDays` | `2` | detail 提前幾天建立 |
| `CallApiAndGetNewOrderInDays` | `2` | 配送日前幾天內打 API |
| `Nine1V2Provider` | `"Nine1V2"` | 過濾條件，避免與 PHP 重複處理 |

---

## 3. RegularCheckPendingPaymentJob

### 觸發時機
- **每日** `30 02 * * *`（UTC）= 台灣時間 **10:30**
- **僅 MY 市場部署**（因為 MY 的信用卡授權有非同步延遲）

### 職責
> 查詢當日 Pending 狀態的訂單（rod_status=3），呼叫 SCM 確認付款結果，解除 Pending 或標記失敗。

### 執行流程

```
GetPendingDetailsAsync(today)  → 查當日 rod_status=3 的明細
  └─ foreach pending rod
       └─ ScmApi: GetSalesOrder(ShopId, TgCode, TsCode)
            ├─ FlowStatus IN [WaitingToShipping, ConfirmedToShipping, Finish]
            │   → ✅ 付款成功：rod_status=1, 推進 rgo 週期, UpsertMap
            └─ 其他 / TotalCount=0
                → ❌ 付款失敗：rod_create_order_status=2, error_code="CreditCardFail"
CheckOrderCreateAsync(today)   → 回填統計到 regular_daily_create
```

### 查詢條件

```sql
-- 查 Pending 明細
WHERE rod_status = 3                          -- Pending 狀態
  AND DATE(rod_daemon_active_time) = today    -- 當天建立的（Pending 只有當天有效）
  AND spi_regular_provider = 'Nine1V2'
```

> ⚠️ **Pending 只有當天有效**：`rod_daemon_active_time` 若是昨天，此 Job 不會撈到

### 寫入資料表

| 資料表 | 操作 | 說明 |
|--------|------|------|
| `regular_order_detail` | UPDATE | 解除 Pending / 標記失敗 |
| `regular_order` | UPDATE | 成功時推進週期；失敗時設 rgo_create_order_status=2 |
| `regular_order_map` | UPSERT | 成功時更新 |
| `regular_daily_create` | UPDATE | 回填統計 |

---

## 4. RegularCreateOrderAuditJob

### 觸發時機
- **每日** `30 03 * * *`（UTC）= 台灣時間 **11:30**
- 需在 RegularCreateOrderJob（10:15）完成後執行

### 職責
> 讀取今日 `regular_daily_create` 統計數字，若有異常（預估≠實際 或全部失敗）發送 Slack 告警。

### 執行流程

```
GetByDateAsync(today)
  ├─ 找不到紀錄           → Slack 告警「排程未正常執行」
  ├─ estimate ≠ execute   → Slack 告警「成單數量異常」
  ├─ execute ≠ success+failed → Slack 告警「成單數量異常」
  └─ success==0 && failed>0   → Slack 告警「成單數量異常」
```

### 查詢條件

```sql
SELECT * FROM regular_daily_create
WHERE rdc_date = today
```

### 告警欄位

```
[TW/HK/MY][QA/Prod] RegularPurchase Alert
預期執行數量：{rdc_estimate}
實際執行數量：{rdc_execute}
成功數量：{rdc_success}
失敗數量：{rdc_failed}
```

---

## 5. RegularCreateOrderNotifyJob

### 觸發時機
- **每日** `0 06 * * *`（UTC）= 台灣時間 **14:00**
- TW / HK / MY 各自執行

### 職責
> D-3（配送日前 3 天）對信用卡與貨到付款訂單預建通知佇列，並發送。

### 執行流程

```
Step 1: GenerateNotifyQueues(today)
  └─ 查 next_ship_date = today + 3 天的訂單
       ├─ PayType contains "CreditCardOnce" → nq_type = CreditCardOrderNotify
       │                                      PayDateTime = ShippingDate - 2 天
       └─ PayType contains "CashOnDelivery" → nq_type = CashOnDeliveryOrderNotify
                                              PayDateTime = ShippingDate
  └─ 防重複：GetByOrderAndTypeAsync(rgo_pk, type) 存在則跳過
  └─ INSERT notify_queue

Step 2: SendNotificationsAsync
  └─ 查 nq_type IN (CreditCardOrderNotify, CashOnDeliveryOrderNotify)
       AND nq_send_status IN (0, 2)
       AND nq_sended_count < nq_send_limit
       LIMIT 100（各類型各取 100）
  └─ 呼叫 ScmApi V2/RegularOrder/Notify
  └─ 成功 → nq_send_status=1；失敗 → nq_send_status=2，count++
```

### 查詢條件（Step 1）

```sql
-- 查訂單（AES 解密后取資料）
WHERE rgo_status = 1
  AND spi_enable_regular = 1
  AND spi_regular_provider = 'Nine1V2'
  AND rgo_next_ship_date < rgo_pause_date
  AND DATEDIFF(rgo_next_ship_date, today) = 3   ← D-3
```

### nq_content Payload（`RegularOrderNotifyQueuePayloadDto`）

| 欄位 | 說明 |
|------|------|
| `VipMemberId` | 會員序號 |
| `Type` | 通知類型 |
| `ShippingDateTime` | 配送日（yyyy-MM-ddTHH:mm:ss） |
| `PayDateTime` | 信用卡：配送日-2天；COD：配送日 |
| `ShippingProfileTypeDef` | 配送方式代碼 |
| `PayProfileTypeDef` | 付款方式代碼 |
| `SalePageTitle` | 商品名稱 |
| `Qty` | 數量 |
| `DeliveryPeriod` | 配送週期（天） |

---

## 6. RegularOrderFailedNotifyJob

### 觸發時機
- **每日** `0 06 * * *`（UTC）= 台灣時間 **14:00**（與 Notify Job 同時）

### 職責
> D-1（配送日前 1 天）對建單失敗訂單根據錯誤碼分流通知，並發送。

### 執行流程

```
Step 1: GenerateFailedNotifyQueues(today)
  └─ 查 next_ship_date = today + 1 天 且 rgo_create_order_error_code ≠ ''
       ├─ error_code IN [CreditCardBlackList, CreditCardFail]
       │   → nq_type = CreditCardFailedNotify
       └─ 其他 error_code
           → nq_type = CreateFailedNotify
  └─ 防重複：GetByOrderAndTypeAsync(rgo_pk, type) 存在則跳過
  └─ INSERT notify_queue

Step 2: SendNotificationsAsync
  └─ 查 nq_type IN (CreditCardFailedNotify, CreateFailedNotify)
       AND nq_send_status IN (0, 2)
       AND nq_sended_count < nq_send_limit
       LIMIT 100（各類型各取 100）
  └─ 呼叫 ScmApi V2/RegularOrder/Notify
```

### 查詢條件（Step 1）

```sql
WHERE rgo_status = 1
  AND spi_enable_regular = 1
  AND spi_regular_provider = 'Nine1V2'
  AND rgo_next_ship_date < rgo_pause_date
  AND DATEDIFF(rgo_next_ship_date, today) = 1   ← D-1
  AND rgo_create_order_error_code <> ''         ← 有失敗錯誤碼
```

### 錯誤碼分類

| 錯誤碼 | 通知類型 | 意義 |
|--------|---------|------|
| `CreditCardBlackList` | `CreditCardFailedNotify` | 信用卡黑名單 |
| `CreditCardFail` | `CreditCardFailedNotify` | 信用卡刷卡失敗 |
| 其他所有錯誤碼 | `CreateFailedNotify` | 其他建單失敗 |

---

## 7. RegularOrderStockNotifyJob

### 觸發時機
- **每週一** `0 02 * * 1`（UTC）= 台灣時間 **10:00**
- TW / HK / MY 各自執行

### 職責
> 掃描未來 3 週的預計出貨訂單，依商店彙整商品預估出貨量，寫入 StockNotify 並發送給各商店。

### 執行流程

```
Step 1: GenerateStockNotifyQueues(today)
  └─ week = 1, 2, 3
       ├─ start_date = today + week*7
       └─ end_date   = start_date + 6
            → 查 rgo_next_ship_date IN [start, end]
            → 依 (rgo_shop_id, rgo_page_id) GROUP BY 累計 SUM(rgo_qty)
  └─ 整理成每商店一個 nq_content（JSON Array）
  └─ 防重複：GetByShopAndTypeAsync(shop_id, 'StockNotify') 存在則跳過
  └─ INSERT notify_queue（nq_ref_order="0", nq_ref_member="0"）

Step 2: SendStockNotificationsAsync
  └─ 查 nq_type = 'StockNotify'
       AND nq_send_status IN (0, 2)
       AND nq_sended_count < nq_send_limit
       LIMIT 100
  └─ 呼叫 ScmApi V2/RegularOrder/EstimatedQtyNotify（不同 API！）
```

### 查詢條件（Step 1）

```sql
SELECT rgo_shop_id, rgo_page_id, rpd_title, SUM(rgo_qty) AS rgo_qty
FROM regular_order
LEFT JOIN regular_product_define ON rgo_page_id = rpd_page_id AND rgo_shop_id = rpd_shop_id
WHERE rgo_status = 1
  AND spi_regular_provider = 'Nine1V2'    ← 新架構新增
  AND rgo_next_ship_date >= :start_date
  AND rgo_next_ship_date <= :end_date
GROUP BY rgo_shop_id, rgo_page_id         ← PHP 只 GROUP BY rgo_page_id，新版修正
```

### nq_content Payload（`List<StockNotifyItemDto>`）

```json
[
  { "Cycle": 1, "SalePageTitle": "商品A", "SalePageId": 123, "Qty": 3 },
  { "Cycle": 2, "SalePageTitle": "商品A", "SalePageId": 123, "Qty": 5 },
  { "Cycle": 1, "SalePageTitle": "商品B", "SalePageId": 456, "Qty": 2 }
]
```

### 對應 SCM API Request（`RegularOrderEstimatedQtyRequest`）

```json
{
  "ShopId": 4,
  "EstimatedQtyList": [
    { "Cycle": 1, "SalePageTitle": "商品A", "SalePageId": 123, "Qty": 3 }
  ]
}
```

---

## notify_queue 狀態說明

| `nq_send_status` | 意義 |
|:---:|------|
| `0` | 待發送（初始值） |
| `1` | 發送成功 |
| `2` | 發送失敗（等下次重試） |

> 重試上限由 `nq_send_limit` 控制（預設 3 次）。  
> 達到上限但仍失敗 → 不再重試（待清理）。

---

## 不需移植的 PHP Jobs

| PHP Job | 原因 |
|---------|------|
| `sync_translate_housekeeper.php` | PHP 的 i18n Redis 機制，.NET 使用 `IStringLocalizer` / resource file，不依賴此排程 |
| `delete_log_file_housekeeper.php` | .NET 使用 CloudWatch / 集中式 log，不存在 Pod 本地 log 檔 |

---

## 待確認缺口

| 項目 | 說明 | 優先度 |
|------|------|--------|
| **notify_queue 清理** | PHP `notify_queue_housekeeper` 會將昨天已完成的通知備份到 `notify_queue_history` 後刪除。新架構尚未實作，長期下來 `notify_queue` 表會累積舊紀錄。 | 中 |

> 建議做法：加到 `RegularCreateOrderNotifyJob` Step 0 作為前置清理步驟。

---

## CronJob 排程設定位置

```
C:\91APP\regular\worker\cronjob\nine1.regularpurchase.v2.cronjob\
  ├── charts\
  │   ├── regular-create-order-job-daily\           → RegularCreateOrderJob
  │   ├── regular-create-order-notify-job-daily\    → RegularCreateOrderNotifyJob
  │   ├── regular-order-failed-notify-job-daily\    → RegularOrderFailedNotifyJob
  │   ├── regular-order-stock-notify-job-weekly\    → RegularOrderStockNotifyJob
  │   ├── regular-create-order-audit-job-daily\     → RegularCreateOrderAuditJob
  │   ├── regular-order-pending-job-daily-1\        → RegularCheckPendingPaymentJob（MY）
  │   └── regular-order-pending-job-daily-2\        → RegularCheckPendingPaymentJob（MY）
  └── src/Nine1.Regularpurchase.V2.CreateTask/data\ → NMQ Job Name JSON
```
