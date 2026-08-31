# regular_create_housekeeper — 邏輯說明

> PHP 檔案：`admin/daemon/regular_create_housekeeper.php`  
> 對應排程：每天台灣時間 10:15（`15 02 * * *`）  
> pg_id：4  

---

## 一句話摘要

**定期購最核心的排程**，負責第 2 期以後的訂單自動化。每天定時執行五個步驟：統計預估、失敗延展、建立佔位、打 API 建單、更新統計。

---

## 背景知識：兩張核心表的關係

```
regular_order（主計畫）              regular_order_detail（每一期配送紀錄）
────────────────────                ─────────────────────────────────────
一個訂閱 = 一筆 rgo                  每一期 = 一筆 rod

rgo_ship_period     = 30（天）        rod_ship_sequence = 1  ← 第1期（首單）
rgo_ship_limit_count= 6（共幾期）      rod_ship_sequence = 2  ← 第2期（此排程負責）
rgo_ship_date       = 本期配送日      rod_ship_date = 這期要出貨的日期
rgo_next_ship_date  = 下期配送日      rod_create_order_status = 0/1/2
rgo_ship_count      = 已完成幾期
```

**第 1 期**（首單）由結帳流程建立，`rod_create_order_status = 1`，此排程不會碰它。  
**第 2 期以後**全部由此排程每天觸發。

---

## 兩個關鍵常數

| 常數 | 值 | 用途 |
|------|----|------|
| `GENERATE_NEW_ORDER_DIFF_DAYS` | 2 | 提前幾天 INSERT detail 佔位 |
| `CALL_API_AND_GET_NEW_ORDER_IN_DAYS` | 2 | 配送日前幾天內去打 API |

---

## 執行流程

```
main()
  ├─ ① estimate_order_count()              統計預估（純管理用，不影響業務）
  ├─ ② update_fail_order_shipping_date()   失敗延展（廢棄過期失敗期，推進週期）
  ├─ ③ generate_regular_order_detail()     建立佔位 detail（只 INSERT DB，不打 API）
  ├─ ④ take_order_api()                    打 API 建真實訂單
  └─ ⑤ check_order_create()               統計今日成單結果
```

---

## 各步驟詳細說明

---

### Step ①：estimate_order_count() — 統計預估

**純管理後台的統計步驟，不影響任何業務流程。**

今天若 `regular_daily_create` 還沒有今日紀錄，就計算今天預計要做多少工作量並寫入。

**預估 = A + B：**

| | 查的表 | 問的問題 | 對應步驟 |
|-|--------|---------|---------|
| A | `regular_order_detail` | 今天要打幾次 API？（detail 已存在但 status=0/2） | Step ④ |
| B | `regular_order` | 今天要新建幾筆 detail？（next_ship_date 剛好是今天+2天） | Step ③ |

寫入 `regular_daily_create`：

```
rdc_estimate = A + B
rdc_execute / rdc_success / rdc_failed = 0（等 Step ⑤ 填）
```

---

### Step ②：update_fail_order_shipping_date() — 失敗延展

**情境：** 某一期建單失敗（API 打了 2 天都沒成功），配送日已過。

**觸發條件（查 `regular_order`）：**
```sql
WHERE rgo_status = 1               -- 計畫啟用中
  AND spi_enable_regular = 1
  AND rgo_create_order_status = 2  -- 主計畫層級標記失敗（由 Step ④ 設定）
  AND rgo_next_ship_date < today   -- 配送日已過
```

> ⚠️ `rgo_create_order_status = 2` 是由 **Step ④ 打 API 失敗時**設定的，不是 Step ② 設定的。

**做兩件事：**

1. 廢棄已過期的失敗 detail：
```sql
UPDATE regular_order_detail SET rod_status = 2
WHERE rod_rgo_pk = ? AND rod_status = 1
  AND rod_create_order_status = 2 AND rod_ship_date < today
```

2. 主計畫推進一個週期：
```
rgo_next_ship_date = rgo_next_ship_date + ship_period
rgo_in_schedule    = 0（解鎖）
```

**結果：** 這期跳過，等下一個週期繼續嘗試。消費者這期不會收到貨。

> ⚠️ 跳過的期數不計入 `rgo_ship_count`，若 `ship_limit_count = 3` 且第 2 期失敗，消費者實際只收 2 次貨但計畫會標記完成，屬於現行設計缺陷。

---

### Step ③：generate_regular_order_detail() — 建立佔位 detail

**只做 DB INSERT，不呼叫任何外部 API。**

**觸發條件（查 `regular_order`）：**
```sql
WHERE DATEDIFF(rgo_next_ship_date, today) = 2  -- 下次配送日剛好是 2 天後
  AND rgo_status = 1
  AND rgo_in_schedule = 0        -- 尚未進入排程
  AND rgo_ship_period > 0        -- 非一次性訂單
  AND rgo_next_ship_date < rgo_pause_date
  AND (rgo_ship_limit_count = 0 OR rgo_ship_count < rgo_ship_limit_count)
  AND spi_enable_regular = 1
```

**做的事：**
- 更新主計畫：`rgo_in_schedule = 1`（防止重複建立）
- INSERT `regular_order_detail`：
  - `rod_ship_sequence = rgo_ship_count + 1`（下一期期數）
  - `rod_ship_date = rgo_next_ship_date`（配送日）
  - `rod_create_order_status = 0`（待建單，還沒有真實訂單）
  - 收件人、發票等資訊直接從主計畫複製過來

---

### Step ④：take_order_api() — 打 API 建真實訂單

**真正建立 mweb 訂單的步驟。**

**觸發條件（查 `regular_order_detail`）：**
```sql
WHERE rod_status = 1                         -- 有效
  AND rod_create_order_status IN (0, 2)      -- 待建單 or 失敗可重試
  AND DATEDIFF(rod_ship_date, today) <= 2    -- 配送日在 2 天內
  AND rod_ship_date > today                  -- 配送日未過
  AND spi_enable_regular = 1
  AND rgo_status <> 3
```

**呼叫的 API：** `Webapi_TradesOrderV2/SubscriptionOrderAutoGen`

**三種結果：**

| 結果 | 條件 | 對 rod 的動作 | 對 rgo 的動作 |
|------|------|-------------|-------------|
| **成功** | Status=Success, FlowStatus≠WaitingToPay | status=1，填入 tg_code/ts_code | 推進 ship_date、next_ship_date、ship_count；`in_schedule=0` |
| **Pending** | Status=Success, FlowStatus=WaitingToPay | status=3 | 不更新（等 pending 排程處理） |
| **失敗** | Status≠Success 或例外 | status=2，填入 error_code | `rgo_create_order_status=2`（供 Step ② 偵測） |

**計畫完結條件：**
```
rgo_ship_count >= rgo_ship_limit_count（且 limit > 0）
→ rgo_is_complete = 1，rgo_status = 3（完結）
```

**每次呼叫 API 都會 INSERT `create_regular_log`** 記錄完整 API 回傳內容，供日後 debug。

---

### Step ⑤：check_order_create() — 統計今日結果

查詢今天所有處理過的 detail，統計成功/失敗數，更新 `regular_daily_create`：

```
rdc_execute = 今日處理總筆數
rdc_success = rod_create_order_status = 1 的筆數
rdc_failed  = 其餘筆數
```

---

## rod_status 與 rod_create_order_status 對照

| rod_status | rod_create_order_status | 意義 |
|------------|------------------------|------|
| `1` | `0` | 有效，等待打 API |
| `1` | `1` | 有效，建單成功 |
| `1` | `2` | 有效，建單失敗（配送日未過，還有重試機會） |
| `2` | `2` | 廢棄（配送日已過，Step ② 標記） |
| `3` | `1` | 待付款 Pending |

---

## 涉及資料表

| 資料表 | 操作 | 說明 |
|--------|------|------|
| `regular_order` | SELECT + UPDATE | 主計畫：推進日期、in_schedule、完結判斷 |
| `regular_order_detail` | SELECT + INSERT + UPDATE | 每期紀錄：建立佔位、更新 API 結果 |
| `shop_info` | JOIN | 過濾 `spi_enable_regular = 1` 的商店 |
| `regular_daily_create` | INSERT + UPDATE | 每日統計 |
| `create_regular_log` | INSERT | 每次 API 呼叫的詳細紀錄 |

---

## 翻寫 .NET Core 注意事項

1. **查詢條件加上 `spi_regular_provider = 'Nine1'`**：現行 PHP 未過濾，新版必須加，避免與 PHP CronJob 重複處理。
2. **RegularOrderProvider 改為 `"Nine1"`**：呼叫 `SubscriptionOrderAutoGen` 時的參數。
3. **`rgo_in_schedule` 防重複機制**：Step ③ 設 1，Step ④ 成功後設 0，必須保留。
4. **失敗延展邏輯**：Step ② 的日期計算必須完整實作。
5. **Pending 狀態**：`rod_status = 3`，需由 `regular-order-pending-housekeeper` 後續處理。
6. **`create_regular_log`**：每次 API 呼叫都要記錄，不可省略。
7. **兩個常數**：`GENERATE_NEW_ORDER_DIFF_DAYS = 2`、`CALL_API_AND_GET_NEW_ORDER_IN_DAYS = 2` 需對應到新版設定檔。
