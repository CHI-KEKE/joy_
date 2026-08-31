# regular_create_housekeeper — Part 0：整體概覽 & Step ① estimate_order_count

> 文件版本：v1.0  
> 更新日期：2026-05-22  

---

## 1. 整體概覽

這支 CronJob 每天執行一次，負責**自動幫所有定期購計畫建立下一期的真實訂單**。

共 5 個步驟依序執行：

```
main()
  ├─ ① estimate_order_count()             預估今日建單數（寫統計表，不影響業務）
  ├─ ② update_fail_order_shipping_date()  上期建單失敗 → 廢棄該期，推進至下一週期
  ├─ ③ generate_regular_order_detail()    產生新一期 detail 佔位紀錄（status=0）
  ├─ ④ take_order_api()                   呼叫 API 建立真實訂單
  └─ ⑤ check_order_create()              統計今日成單結果，更新統計表
```

### 執行前置條件

```php
$pg_id = 4;
if (!$this->can_running($pg_id)) exit;  // 系統設定停止時不執行
$this->update_daemon_time($pg_id);       // 更新最後執行時間
```

---

## 2. 涉及的資料表

| 表名 | 用途 |
|------|------|
| `regular_order` | 定期購主計畫（一個訂閱一筆） |
| `regular_order_detail` | 每一期配送紀錄（每期一筆） |
| `shop_info` | 商店設定，用於過濾有開啟定期購的商店 |
| `regular_daily_create` | 每日統計表（Step ① 和 ⑤ 使用） |
| `create_regular_log` | 每次 API 呼叫的詳細紀錄（Step ④ 使用） |

---

## 3. ShopId 篩選機制

**所有查詢都不指定特定 ShopId**，而是透過 `shop_info` JOIN 做條件篩選：

```sql
LEFT JOIN shop_info ON rgo_shop_id = spi_shopid
WHERE spi_enable_regular = 1   -- 只處理有開啟定期購的商店
```

### ⚠️ 重要：`spi_regular_provider` 欄位存在但現行未使用

`shop_info` 表有 `spi_regular_provider`（值為 `GameBall` 或 `Nine1`），但現行 PHP CronJob 的所有查詢**均未過濾此欄位**，代表目前會處理所有 provider 的商店。

**新版 .NET Core 排程必須加上此條件，避免與 PHP CronJob 重複處理：**
```sql
AND spi_regular_provider = 'Nine1'
```

---

## 4. Step ① `estimate_order_count()` — 預估今日建單數

### 做什麼

在每天**第一次執行**時，預估今天整體的工作量，寫入 `regular_daily_create` 統計表。  
Step ⑤ 執行完後會回來填入實際結果。

**純統計用途，不影響任何業務流程。**

### 執行條件

```php
if (!$regular_daily_create->load_by_date($this->current_date)) {
    // 今天還沒有統計紀錄才執行，避免重複寫入
}
```

### 預估邏輯

```
total_estimate = A + B

A = 查 regular_order_detail（Step ④ 今天要打幾次 API）
B = 查 regular_order（Step ③ 今天要新建幾筆 detail）
```

#### A：查 `regular_order_detail`（Step ④ 的工作量）

```sql
SELECT rod_pk, rod_rgo_pk
FROM regular_order_detail
LEFT JOIN regular_order ON rgo_pk = rod_rgo_pk
LEFT JOIN shop_info ON rod_shop_id = spi_shopid
WHERE spi_enable_regular = 1
  AND rgo_status <> 3                          -- 計畫未完結
  AND rod_status = 1                           -- detail 有效
  AND rod_create_order_status IN (0, 2)        -- 待建單 or 失敗可重試
  AND DATEDIFF(rod_ship_date, today) <= 2      -- 配送日在 2 天內
  AND rod_ship_date > today                    -- 配送日未過
```

**問的問題：** 「今天 Step ④ 要打幾次 API？」  
→ 這些 detail **已經存在**（前幾天 Step ③ 建好），今天要去打 API 建真實訂單。

#### B：查 `regular_order`（Step ③ 的工作量）

```sql
SELECT rgo_pk
FROM regular_order
LEFT JOIN shop_info ON rgo_shop_id = spi_shopid
WHERE spi_enable_regular = 1
  AND rgo_status = 1                           -- 計畫啟用中
  AND rgo_in_schedule = 0                      -- 不在排程中
  AND rgo_ship_period > 0                      -- 非一次性
  AND rgo_next_ship_date < rgo_pause_date      -- 未暫停
  AND (rgo_ship_limit_count = 0 OR rgo_ship_count < rgo_ship_limit_count)
  AND DATEDIFF(rgo_next_ship_date, today) = 2  -- 下次配送日剛好是 2 天後
```

**問的問題：** 「今天 Step ③ 要新建幾筆 detail？」  
→ 這些主計畫**還沒有新 detail**，今天 Step ③ 會為它們建立佔位紀錄。

### 寫入 `regular_daily_create`

| 欄位 | 值 |
|------|----|
| `rdc_date` | 今天日期 |
| `rdc_estimate` | A + B（預估總數） |
| `rdc_execute` | `0`（等 Step ⑤ 填） |
| `rdc_success` | `0`（等 Step ⑤ 填） |
| `rdc_failed` | `0`（等 Step ⑤ 填） |

### 一句話總結

> **Step ① 是純統計步驟，預估今天要建幾張單，供管理後台顯示，不影響任何業務流程。**
