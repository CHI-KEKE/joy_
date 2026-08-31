# regular_create_housekeeper — Part 2：失敗延展 & rod 狀態欄位說明

> 文件版本：v1.0  
> 更新日期：2026-05-22  

---

## 1. Step ② `update_fail_order_shipping_date()` — 失敗訂單延展

### 觸發情境

某一期的 detail 建單失敗（API 打了 2 次都沒成功），配送日已經過了。

```
5/31  Step ④ 打 API → 失敗 → rod_create_order_status = 2
6/1   Step ④ 重試   → 失敗 → rod_create_order_status = 2
6/2   配送日到，仍未成功建單
6/3   CronJob 執行 → Step ② 觸發
```

### 觸發條件（SQL）

```sql
WHERE rgo_status = 1               -- 計畫啟用中
  AND spi_enable_regular = 1       -- 商店有開定期購
  AND rgo_create_order_status = 2  -- 上一次建單失敗
  AND rgo_next_ship_date < today   -- 配送日已過
```

### 做的兩件事

**第 1 件：把失敗的 detail 標記為廢棄**

```sql
UPDATE regular_order_detail SET rod_status = 2
WHERE rod_rgo_pk = :rgo_pk
  AND rod_status = 1
  AND rod_create_order_status = 2
  AND rod_ship_date < today
```

白話：「這期已經來不及了，這筆 detail 作廢。」

**第 2 件：把主計畫的下次出貨日往前推一個週期**

```
rgo_next_ship_date = rgo_next_ship_date + ship_period
rgo_in_schedule    = 0（解鎖，允許下次觸發 Step ③）
```

白話：「這期跳過，下次等下一個週期再試。」

### 完整時間軸

```
5/31  打 API 失敗 → rod_create_order_status = 2
6/1   重試失敗   → rod_create_order_status = 2
6/2   配送日，rgo_next_ship_date = 6/2，rgo_create_order_status = 2

6/3   Step ②：
       ├─ rod（6/2 那期）→ rod_status = 2（廢棄）
       └─ rgo_next_ship_date = 7/2（6/2 + 30天），rgo_in_schedule = 0

6/30  Step ③：DATEDIFF(7/2, 6/30) = 2 → 正常建立下一期 detail
6/30  Step ④：打 API 建下一期訂單
```

---

## 2. 跳過某期後，總期數會少嗎？

**是的，這期就是真的跳過，消費者少收一次貨。**

### 原因：ship_count 只在成功時遞增

```
第 1 期 API 成功 → rgo_ship_count = 1
第 2 期 API 失敗 → rgo_ship_count 維持 1（不變）
第 3 期 API 成功 → rgo_ship_count = 3（= rod_ship_sequence）
```

### 計畫完結條件

```
rgo_ship_count >= rgo_ship_limit_count（且 limit > 0）
```

以 `limit = 3` 為例，第 2 期失敗後：

```
第 1 期成功：ship_count = 1
第 2 期失敗：跳過
第 3 期成功：ship_count = 3 >= 3 → 計畫完結 → rgo_status = 3
```

**結果：消費者實際只收到 2 次貨，但系統認定計畫「已完成」。**

> ⚠️ 這是現行 PHP 系統的設計，屬於已知缺陷。  
> 新版 .NET Core 規劃時可考慮是否補償（例如：失敗期不計入 ship_count，讓消費者補收一次）。

---

## 3. rod 的兩個狀態欄位說明

`regular_order_detail` 有**兩個**不同意義的狀態欄位，很容易混淆：

### `rod_create_order_status` — 建單結果

| 值 | 意義 |
|----|------|
| `0` | 待建單（detail 已建立，尚未打 API） |
| `1` | 建單成功（已有真實 mweb 訂單） |
| `2` | 建單失敗（API 回傳失敗或例外） |

### `rod_status` — 這筆 detail 的生命週期

| 值 | 意義 | 由誰設定 |
|----|------|---------|
| `1` | 有效（正常進行中） | 建立時預設 |
| `2` | 廢棄（配送日已過且失敗） | Step ② |
| `3` | 待付款 Pending | Step ④（API 回傳 WaitingToPay） |

### 兩個欄位的組合狀態

| rod_status | rod_create_order_status | 意義 |
|------------|------------------------|------|
| `1` | `0` | 有效，等待打 API 建單 |
| `1` | `1` | 有效，建單成功 |
| `1` | `2` | 有效，建單失敗（配送日未過，還有重試機會） |
| `2` | `2` | 廢棄（配送日已過，不再重試） |
| `3` | `1` | 待付款（建單成功但訂單待付款） |

### Step ④ 撈取條件

```sql
WHERE rod_status = 1                      -- 有效的
  AND rod_create_order_status IN (0, 2)   -- 待建單 或 失敗可重試
  AND rod_ship_date > today               -- 配送日未過（還來得及）
  AND DATEDIFF(rod_ship_date, today) <= 2 -- 配送日在 2 天內
```

---

## 4. 一句話總結 Step ②

> **這期建單失敗且配送日已過 → 廢棄這期的 detail → 把主計畫的下次出貨日推進一個週期，繼續下一期。**
