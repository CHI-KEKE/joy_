# regular-create-housekeeper — Step ④ take_order_api

## 職責

對「已建 detail、尚未成功建單」的每期紀錄，呼叫 API 真實建立訂單，  
並依據結果更新 detail 與主計畫的狀態。**這是整個排程最核心的一步。**

---

## 觸發條件（SQL）

```sql
SELECT rod_pk, rod_rgo_pk
FROM regular_order_detail
LEFT JOIN regular_order ON rgo_pk = rod_rgo_pk
LEFT JOIN shop_info ON rod_shop_id = spi_shopid
WHERE spi_enable_regular = 1
  AND rgo_status <> 3                          -- 訂閱未完成
  AND rod_status = 1                           -- detail 有效（非廢棄、非 Pending）
  AND rod_create_order_status IN (0, 2)        -- 待建單 or 失敗重試
  AND DATEDIFF(rod_ship_date, today) <= 2      -- 距配送日 2 天以內
  AND rod_ship_date > today                    -- 配送日尚未過期
```

> **與 Step ③ 的差異**：Step ③ 只在「剛好 2 天」觸發一次；  
> Step ④ 在「2 天以內」每天都會嘗試，所以失敗後隔天會自動重試。

---

## 呼叫的 API

`Webapi_TradesOrderV2_SubscriptionOrderAutoGen`

### Request Body

| 欄位 | 來源 |
|---|---|
| `ShopId` | `rod_shop_id` |
| `MemberId` | `rod_member_id` |
| `SalePageId` | `rod_page_id` |
| `SaleProductSKUId` | `rgo_product_style_id` |
| `Qty` | `rod_qty` |
| `Price` | `rod_sale_price` |
| `RegularOrderId` | `rgo_source_id` |
| `ExpectShippingDate` | `rod_ship_date`（本期配送日） |
| `NextExpectShippingDate` | `rod_ship_date + ship_period`（下期配送日） |
| `ShippingProfileTypeDef` | `rod_ship_type` |
| `PayProfileType` | `rod_pay_type` |
| `ReceiverName/Mobile/Address/...` | `rod_receiver_*` |
| `InvoiceTitle/No/Email` | `rod_invoice_*` |
| `RegularOrderProvider` | `"GameBall"`（固定值） |
| `TraceId` | `rod_txid`（防重複用的 UUID） |

---

## 執行流程

```
撈出符合條件的 detail 清單
        │
        ▼ foreach 每一筆
        │
        ├─ 載入主計畫 regular_order
        ├─ 載入 regular_order_detail
        │
        ▼
   組裝 Request Body
        │
        ▼
   呼叫 SubscriptionOrderAutoGen API
        │
        ├──────────────────────────────────────────────────┐
        │ 例外 / 連線失敗                                   │
        ▼                                                  │
   create_status = false                                   │
   error_code = 'GenerateOrderFailed'                      │
        │                                                  │
        │ API 回傳 Status != "Success"                      │
        ▼                                                  │
   create_status = false                                   │
   error_code = StatusCause（API 給的錯誤代碼）             │
        │                                                  │
        └──────────────────────────────────────────────────┘
        │ API 回傳 Status = "Success"
        │
        ├─ OrderSlaveFlowStatus = "WaitingToPay"？
        │       YES → isPending = true
        │       NO  → isPending = false
        ▼
   寫入 create_regular_log（每次都寫，成功失敗都記）
        │
        ├──── create_status = false（失敗）
        │           ↓ 見「失敗」區塊
        │
        ├──── isPending = true（Pending）
        │           ↓ 見「Pending」區塊
        │
        └──── 成功且非 Pending
                    ↓ 見「成功」區塊
```

---

## 三種結果詳細說明

### ❌ 失敗

```
detail 更新：
  rod_create_order_status = 2
  rod_create_order_error_code = error_code
  rod_daemon_active_time = now

主計畫更新：
  rgo_create_order_status = 2
  rgo_create_order_error_code = error_code
  rgo_in_schedule 維持 1（隔天 Step ④ 繼續重試）

mapping：不寫
```

---

### ⏳ Pending（已建單，等待付款）

```
detail 更新：
  rod_tg_id / rod_tg_code / rod_ts_code = API 回傳的訂單號
  rod_create_order_status = 1
  rod_status = 3（Pending 狀態）
  rod_daemon_active_time = now

主計畫更新：
  rgo_in_schedule 維持 1（不推進，交給 pending_housekeeper 處理）

mapping：不寫
```

---

### ✅ 成功

```
detail 更新：
  rod_tg_id / rod_tg_code / rod_ts_code = API 回傳的訂單號
  rod_create_order_status = 1
  rod_create_order_error_code = ''
  rod_daemon_active_time = now

主計畫更新：
  rgo_in_schedule = 0（解鎖，等下一期）
  rgo_ship_date = rgo_next_ship_date（本期配送日）
  rgo_next_ship_date = rgo_ship_date + ship_period（下期配送日）
  rgo_ship_count = rod_ship_sequence（已完成幾期）
  rgo_create_order_status = 1
  rgo_create_order_error_code = ''

  若 rgo_ship_count >= rgo_ship_limit_count（且 limit > 0）：
    rgo_is_complete = 1
    rgo_complete_time = now
    rgo_status = 3（訂閱完成）

mapping：寫入 regular_order_map（rgo_pk → rod_pk）
```

---

## 狀態流轉圖

```
                    ┌─────────────────────┐
                    │  rod_create_order   │
                    │     status = 0      │  ← Step ③ 建 detail 時
                    │  rod_status = 1     │
                    │  rgo_in_schedule = 1│
                    └──────────┬──────────┘
                               │ Step ④ 呼叫 API
                               ▼
              ┌────────────────┴─────────────────┐
              │                                  │
         API 失敗                            API 成功
              │                                  │
              ▼                    ┌─────────────┴──────────────┐
   rod_create_order_status = 2     │                            │
   rgo_create_order_status = 2  Pending                      正常成功
   rgo_in_schedule 維持 1          │                            │
   （隔天繼續重試）                 ▼                            ▼
              │           rod_status = 3          rod_create_order_status = 1
              │           rod_create_order = 1    rgo_in_schedule = 0
              │           rgo_in_schedule 維持 1  rgo_ship_date 推進
              │           （交給 pending 排程）    rgo_ship_count++
              │                    │
              │                    │ pending_housekeeper 確認付款
              │                    ▼
              │           rod_status = 1
              │           rgo_in_schedule = 0
              │           rgo_ship_date 推進
              │
              │ 若配送日過期（Step ② 偵測）
              ▼
   rod_status = 2（廢棄）
   rgo_next_ship_date + ship_period（跳過這期）
   rgo_in_schedule = 0
```

---

## `regular_order_map` 的作用

Step ④ 成功後會呼叫 `regular_order_mapping(rgo_pk, rod_pk)`，  
維護一張 `regular_order_map` 對照表，記錄「主計畫 → 最新一期 detail」。

```
regular_order_map：
  rom_rgo_pk  →  rom_rod_pk
  （主計畫 PK）   （最新成功 detail PK）
```

- 若對應已存在且 rod_pk 不同 → **UPDATE**（更新為最新一期）
- 若對應不存在 → **INSERT**

> Pending 狀態不寫入，因為付款尚未確認，不算正式完成。

---

## 注意事項

1. **`rod_create_order_status IN (0, 2)` 代表會重試失敗**  
   失敗後 `status = 2`，隔天 Step ④ 仍會撈到這筆繼續打，直到配送日過期才由 Step ② 廢棄。

2. **`TraceId = rod_txid`（防重複）**  
   每筆 detail 有唯一 UUID，API 端以此做冪等判斷，避免網路異常導致重複建單。

3. **`RegularOrderProvider = "GameBall"`（硬編碼）**  
   新版 .NET Core 實作時需改為動態判斷，Nine1 商戶應傳 `"Nine1"`。

4. **成功後主計畫日期推進邏輯**  
   `rgo_ship_date ← rgo_next_ship_date`（本期）  
   `rgo_next_ship_date ← 新 ship_date + ship_period`（下期）  
   此時 `rgo_next_ship_date` 已是下下期的日期，Step ③ 將在距此日 2 天前觸發。
