# create_order_housekeeper — Queue 消費邏輯說明

> 分析日期：2026-05-21  
> 檔案：`admin\daemon\create_order_housekeeper.php`  
> 觸發：Kubernetes CronJob，每 1 分鐘執行一次

---

## 一、定位

這支 CronJob 是 **order_create_queue 的消費者**，負責將佇列中的首單通知轉換為真正的定期購計畫。

---

## 二、消費數量與順序

### 一次消費多少筆？

**最多 100 筆**（由 `take_queue_job($token, $limit = 100)` 決定）

```sql
UPDATE order_create_queue
SET    ocq_occupy_id = :token
WHERE  ocq_occupy_id = ''
  AND  ocq_status = 0
ORDER  BY ocq_create_time ASC
LIMIT  100
```

### 消費順序？

**先進先出（FIFO）**，以 `ocq_create_time ASC` 排序，越早進 queue 的越先被處理。

### 如何避免多個 instance 搶同一筆？

使用 **Optimistic Lock（樂觀鎖）** 機制：

```
Step 1: 產生唯一 UUID token
Step 2: 用 UPDATE ... WHERE ocq_occupy_id = '' 原子性地「佔領」一批資料
         → 同時只有一個 instance 能成功寫入相同 token
Step 3: 再用 SELECT WHERE ocq_occupy_id = token 取出被自己鎖定的資料
Step 4: 處理完後更新 status = 1 or 8
```

> UPDATE 是原子操作，多個 instance 同時執行時，只有一個能鎖到同一批資料，自然避免重複消費。

---

## 三、完整處理流程

```
main()
  │
  ├─ Step 1: 產生 UUID token
  │
  ├─ Step 2: take_queue_job(token)
  │    UPDATE order_create_queue SET ocq_occupy_id = token
  │    WHERE ocq_occupy_id = '' AND ocq_status = 0
  │    ORDER BY ocq_create_time ASC LIMIT 100
  │    → 回傳 false（沒有待處理資料）→ 直接結束
  │    → 回傳 true（成功鎖定）→ 繼續
  │
  ├─ Step 3: get_queue_job(token)
  │    SELECT * FROM order_create_queue
  │    WHERE ocq_occupy_id = token AND ocq_status = 0
  │
  └─ Step 4: foreach 每筆 queue item：
       │
       ├─ 解析 ocq_data（JSON）
       │    取得 ShopId, TGCode(tg_id), TSCodeList
       │
       ├─ 呼叫 91APP V2/SalesOrder/Get(shopId, tgId)
       │    → 取得訂單完整資料清單
       │    → 篩選只處理在 TSCodeList 內的 TS
       │
       ├─ foreach 每筆 TS：
       │    ├─ 防重複：load_by_tg_and_ts(TG, TS) → 若已存在跳過
       │    │
       │    ├─ 呼叫 91APP V2/RegularOrder/Get(shopId, TSCode)
       │    │    → 取得定期購設定（配送週期、收件人等）
       │    │
       │    ├─ 驗證 OrderFrequency != 0（0 = 一次性訂單，非定期購）
       │    │
       │    ├─ 查詢 regular_product（商品定期購設定是否存在）
       │    │
       │    ├─ Upsert member_info（會員資料）
       │    │
       │    ├─ BEGIN TRANSACTION
       │    │    ├─ INSERT regular_order（主計畫）
       │    │    │    rgo_ship_count = 1
       │    │    │    rgo_ship_date = 首單日 + 2天
       │    │    │    rgo_next_ship_date = 首單日 + 週期 + 2天
       │    │    │    rgo_status = 1（進行中）
       │    │    │
       │    │    └─ INSERT regular_order_detail（第 1 期明細）
       │    │         rod_ship_sequence = 1
       │    │         rod_ship_date = 首單日 + 2天
       │    │         rod_create_order_status = 1（首單已建立）
       │    │
       │    └─ COMMIT / ROLLBACK
       │
       ├─ 成功 → ocq_status = 1
       └─ 失敗 → ocq_status = 8
              queue.update()
```

---

## 四、建立的資料說明

### regular_order（主計畫）

| 欄位 | 值 | 說明 |
|------|-----|------|
| `rgo_ship_count` | `1` | 首期，已配送 1 次 |
| `rgo_ship_date` | `首單日 + 2天` | 本次（第 1 期）配送日 |
| `rgo_next_ship_date` | `首單日 + 週期 + 2天` | 下次配送日 |
| `rgo_status` | `1` | 進行中 |
| `rgo_ship_limit_status` | `MaxDeliveryCount > 2 ? 2 : 1` | 有無次數上限 |
| `rgo_pause_date` | `2099-12-31` | 初始暫停日（遠未來）|
| `rgo_create_user` | `DAEMON` | 系統建立標記 |

### regular_order_detail（第 1 期明細）

| 欄位 | 值 | 說明 |
|------|-----|------|
| `rod_ship_sequence` | `1` | 第 1 期 |
| `rod_ship_date` | `首單日 + 2天` | 本期配送日 |
| `rod_create_order_status` | `1` | 建單成功（首單已存在）|
| `rod_status` | `1` | 正常 |

> **重要：** 第 1 期 `rod_create_order_status = 1` 是因為首單訂單（TG/TS）已由 mweb 結帳建立，不需再呼叫 API 建單。

---

## 五、Queue 狀態轉換

```
寫入時：status=0, occupy_id=''
         ↓ take_queue_job
取走時：status=0, occupy_id='{UUID}'
         ↓ 處理
成功：  status=1
失敗：  status=8
```

| status | 意義 | 後續動作 |
|--------|------|---------|
| `0` | 待處理 | CronJob 下次觸發時可能再被取走（若 occupy_id 仍空）|
| `1` | 已成功 | 不再處理 |
| `8` | 失敗 | 不自動重試，需人工確認 |

---

## 六、配送日期計算公式

| 項目 | 計算公式 |
|------|---------|
| 首期配送日（`rgo_ship_date`）| `首單日（FirstOrderDate）+ 2天` |
| 下次配送日（`rgo_next_ship_date`）| `首單日 + 配送週期（OrderFrequency）+ 2天` |
| 常數 `2天` | `Gb_Config::$GENERATE_NEW_ORDER_DIFF_DAYS = 2` |

---

## 七、關鍵檔案索引

| 角色 | 路徑 |
|------|------|
| CronJob 主程式 | `C:\91APP\regular\n1-regularpurchase-nkp\src\...\admin\daemon\create_order_housekeeper.php` |
| Queue Model（鎖定/取出）| `C:\91APP\regular\n1-regularpurchase-nkp\src\...\models\order_create_queue.php` |
| Queue Base Model（SQL）| `C:\91APP\regular\n1-regularpurchase-nkp\src\...\models\base\order_create_queue_base.php` |
