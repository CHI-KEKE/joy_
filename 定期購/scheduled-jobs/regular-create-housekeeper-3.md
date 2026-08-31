# regular-create-housekeeper — Step ③ generate_regular_order_detail

## 職責

在配送日兩天前，為「即將到期的主計畫」建立一筆佔位 detail，  
並鎖定主計畫防止重複建立。**純 DB 操作，不打任何 API。**

---

## 觸發條件（SQL）

```sql
SELECT rgo_pk FROM regular_order
LEFT JOIN shop_info ON rgo_shop_id = spi_shopid
WHERE spi_enable_regular = 1
  AND rgo_status = 1                         -- 訂閱中
  AND rgo_in_schedule = 0                    -- 未鎖定（可建 detail）
  AND rgo_ship_period > 0
  AND rgo_next_ship_date < rgo_pause_date    -- 未暫停
  AND (rgo_ship_limit_count = 0 OR rgo_ship_count < rgo_ship_limit_count)  -- 未達期數上限
  AND DATEDIFF(rgo_next_ship_date, :current_date) = 2  -- 距離下次配送日剛好 2 天
```

---

## 執行流程

```
撈出符合條件的 rgo_pk 清單
        │
        ▼ foreach 每一筆
        │
        ├─ 更新主計畫 regular_order
        │    rgo_in_schedule = 1
        │    rgo_create_order_date = today
        │
        └─ INSERT 一筆 regular_order_detail
             （資料全部從主計畫複製）
```

---

## INSERT detail 欄位說明

| 欄位 | 來源 | 說明 |
|---|---|---|
| `rod_txid` | 新產生 UUID | 這期的 transaction ID |
| `rod_rgo_pk` | `rgo_pk` | FK 對應主計畫 |
| `rod_shop_id` | `rgo_shop_id` | |
| `rod_member_id` | `rgo_member_id` | |
| `rod_tg_code` | **`''`（空）** | 尚無真實訂單，Step ④ 成功後填入 |
| `rod_ts_code` | **`''`（空）** | 尚無真實訂單，Step ④ 成功後填入 |
| `rod_page_id` | `rgo_page_id` | |
| `rod_qty` | `rgo_qty` | |
| `rod_sale_price` | `rgo_sale_price` | |
| `rod_pay_type` | `rgo_pay_type` | |
| `rod_receiver_*` | `rgo_receiver_*` | 收件人資訊完整複製（快照） |
| `rod_invoice_*` | `rgo_invoice_*` | 發票資訊完整複製（快照） |
| `rod_ship_type` | `rgo_ship_type` | |
| `rod_ship_sequence` | `rgo_ship_count + 1` | 下一期的期數 |
| `rod_ship_date` | `rgo_next_ship_date` | 此期配送日 |
| `rod_create_order_date` | today | |
| `rod_create_order_status` | `0` | 待建單（Step ④ 尚未打 API） |
| `rod_status` | `1` | 有效 |
| `rod_create_user` | `'DAEMON'` | |

---

## rgo_in_schedule 狀態說明

| 值 | 意義 | 由誰設定 |
|---|---|---|
| `0` | 待塞 detail，可被 Step ③ 撈取 | 初始值 / Step ④ 成功後 / Step ② 延展後 |
| `1` | detail 已建，等 Step ④ 打 API | Step ③ |

```
rgo_in_schedule = 0
        │
        ▼ Step ③（DATEDIFF = 2）
        │  INSERT detail，主表 in_schedule → 1
        ▼
rgo_in_schedule = 1
        │
        ├── Step ④ API 成功 ──→ in_schedule → 0（進入下一期）
        ├── Step ④ API 失敗 ──→ in_schedule 維持 1（隔天重試）
        └── Step ② 過期失敗 ──→ in_schedule → 0（廢棄這期，推進週期）
```

---

## 注意事項

1. **收件人資訊是快照**：建立 detail 當下複製，消費者中途改地址不影響本期，下期才會抓新的。
2. **`rod_tg_code` / `rod_ts_code` 初始為空**：真實訂單號碼要等 Step ④ 打 API 成功後才寫入。
3. **不重複保護**：`rgo_in_schedule = 1` 確保同一期只會建一筆 detail，即使 Step ③ 隔天再跑也不會重複（因為 DATEDIFF 已不等於 2）。
