# regular-create-housekeeper — Step ⑤ check_order_create

## 職責

每日排程的收尾步驟。Step ① 預估了今天要處理幾筆，  
Step ⑤ 將 Step ④ 的實際結果（成功／失敗筆數）回填到 `regular_daily_create` 表。  
**純統計用途，不影響任何訂單狀態。**

---

## 執行流程

```
讀取今天的 regular_daily_create 紀錄（由 Step ① 建立）
        │
        ├── 找不到 → 直接結束（Step ① 未建立則跳過）
        │
        ▼ 找到
        │
撈出今天被 Step ④ 處理過的 detail 清單
        │
        ▼
統計 total / success / failed
        │
        ▼
UPDATE regular_daily_create
  rdc_execute = total
  rdc_success = success
  rdc_failed  = failed
```

---

## 查詢 SQL

```sql
SELECT rod_shop_id, rod_create_order_status
FROM regular_order_detail
WHERE rod_status = 1
  AND rod_ship_sequence > 1                    -- 排除第 1 期（首單由其他排程處理）
  AND rod_daemon_active_time >= :current_date
  AND rod_daemon_active_time < DATE_ADD(:current_date, INTERVAL 1 DAY)
```

> `rod_daemon_active_time` 是 Step ④ 打完 API 後寫入的時間戳，  
> 用這個欄位判斷「今天有被 Step ④ 嘗試處理過」。

---

## 統計邏輯

| 條件 | 計入 |
|---|---|
| `rod_create_order_status = 1` | success++ |
| `rod_create_order_status = 0 or 2` | failed++ |
| 每一筆 | total++ |

---

## `regular_daily_create` 欄位前後變化

| 欄位 | Step ① 寫入 | Step ⑤ 更新 | 說明 |
|---|---|---|---|
| `rdc_date` | 今天日期 | 不動 | |
| `rdc_estimate` | 預估工作量（A+B） | 不動 | Step ① 算的預估值 |
| `rdc_execute` | 0 | ✅ 實際處理筆數 | |
| `rdc_success` | 0 | ✅ 成功筆數 | |
| `rdc_failed` | 0 | ✅ 失敗筆數 | |

---

## 整日資料流示意

```
Step ①  → regular_daily_create 建立
            rdc_estimate = 預估工作量
            rdc_execute  = 0
            rdc_success  = 0
            rdc_failed   = 0

Step ②③④ → 實際處理訂單，更新 detail 與主計畫

Step ⑤  → regular_daily_create 更新
            rdc_execute  = 今天處理筆數
            rdc_success  = 成功筆數
            rdc_failed   = 失敗筆數
```

---

## 注意事項

1. **Step ① 未建立 daily 紀錄時，Step ⑤ 直接跳過**  
   `load_by_date` 找不到當天紀錄則整個函式提前結束，不報錯。

2. **`rod_ship_sequence > 1` 排除首單**  
   第 1 期由 `create_order_housekeeper` 負責，不計入此統計。

3. **純報表用途，不影響功能**  
   即使 Step ⑤ 沒跑或跑失敗，訂單本身完全不受影響，只是 daily 表的數字不會更新。

4. **`rdc_estimate` 與 `rdc_execute` 可能不相等**  
   - `rdc_estimate` 是 Step ① 事前預估（哪些 detail 該被處理）  
   - `rdc_execute` 是 Step ⑤ 事後統計（今天實際被打過 API 的筆數）  
   - 兩者差異可作為排程異常的監控指標
