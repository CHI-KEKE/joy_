# notify_queue_housekeeper

> PHP 檔案：`admin/daemon/notify_queue_housekeeper.php`
> 對應排程：台灣時間 08:00~00:59 每 5 分鐘（`*/5 00-16 * * *`）
> pg_id：6

## 職責

將 `create_notify_housekeeper` 建好的 `notify_queue` 實際呼叫 API 發送出去，  
並清理昨天已完成的舊紀錄。

---

## 整體流程

```
main()
  │
  ├─ move_notify_to_history()   清理昨天以前的已完成紀錄
  └─ go_notify(100)             撈 queue 發送通知（每次最多 100 筆）
```

---

## Step 1：清理舊紀錄（move_notify_to_history）

```
條件：nq_create_time < 今天 00:00
      AND (nq_send_status = 1 OR nq_sended_count >= nq_send_limit)

1. backup_old_notify()  → INSERT INTO notify_queue_bak（備份）
2. remove_old_notify()  → DELETE FROM notify_queue（清除）
```

> 今天建的 queue 不清除，確保當天排程不重複發送。  
> 超過重試上限視同放棄，也一併清除。

---

## Step 2：發送通知（go_notify）

### 觸發 SQL

```sql
SELECT * FROM notify_queue
WHERE (nq_send_status = 0 OR nq_send_status = 2)   -- 待發 or 失敗重試
  AND nq_sended_count < nq_send_limit               -- 未超過重試上限（預設 3 次）
  AND (nq_send_time IS NULL OR nq_send_time <= now)
ORDER BY nq_priority ASC
LIMIT 100
```

### 發送結果處理

| 結果 | nq_send_status | nq_sended_count |
|---|:---:|:---:|
| 成功（`result = 'Success'`） | `1` | +1 |
| 失敗 | `2` | +1 |

---

## nq_send_status 狀態說明

| 值 | 意義 |
|---|---|
| `0` | 待發送（create_notify_housekeeper 建立時的初始值） |
| `1` | 發送成功 |
| `2` | 發送失敗（等下次重試） |

---

## 兩排程串接流程

```
create_notify_housekeeper（pg_id=5）
  └─ INSERT notify_queue
       nq_send_status = 0，nq_send_limit = 3

notify_queue_housekeeper（pg_id=6）
  ├─ 清理昨天已完成舊紀錄（備份 → 刪除）
  └─ 撈 status IN (0,2) 且 sended_count < send_limit
       ├── 成功 → nq_send_status = 1
       └── 失敗 → nq_send_status = 2，sended_count++
                   sended_count = 3 → 下次清理時歸檔放棄
```

---

## 詳細邏輯

### go_notify(send_limit=100)
```
foreach notify:
    if nq_type == 'StockNotify':
        → api.V2_RegularOrder_EstimatedQtyNotify()
    else:
        → api.V2_RegularOrder_Notify()

    result == 'Success':
        nq_send_status = 1, nq_sended_count++, nq_send_time = now
    else:
        nq_send_status = 2, nq_sended_count++, nq_send_time = now
```

---

## 通知類型與對應 API

| nq_type | API | 說明 |
|---|---|---|
| `CashOnDeliveryOrderNotify` | `V2_RegularOrder_Notify` | 貨到付款配送通知 |
| `CreditCardOrderNotify` | `V2_RegularOrder_Notify` | 即將刷卡通知 |
| `CreditCardFailedNotify` | `V2_RegularOrder_Notify` | 刷卡失敗通知 |
| `CreateFailedNotify` | `V2_RegularOrder_Notify` | 建單失敗通知 |
| `StockNotify` | `V2_RegularOrder_EstimatedQtyNotify` | 庫存預估通知（不同 API！） |

---

## 涉及資料表

| 資料表 | 操作 | 說明 |
|---|---|---|
| `notify_queue` | SELECT + UPDATE | 取未發送通知、更新發送狀態 |
| `notify_queue_history` | INSERT | 備份已發送的舊通知 |

---

## 外部 API 呼叫

| API | 用途 |
|---|---|
| `V2_RegularOrder_Notify` | 發送一般定期購通知 |
| `V2_RegularOrder_EstimatedQtyNotify` | 發送庫存預估通知（StockNotify 專用） |

---

## 翻寫 .NET Core 注意事項

1. **批次上限 100 筆**：每次最多處理 100 筆，若通知量大可能要多次執行才能清空佇列，新版可考慮動態調整或改為 event-driven。
2. **StockNotify 用不同 API**：`nq_type == 'StockNotify'` 需分流到不同 API，翻寫時不能統一處理。
3. **send_status = 2 不重試**：失敗後 status 設為 2，目前沒有 retry 邏輯（依靠 `nq_send_limit` 欄位控制），新版需確認 retry 策略。
4. **history 備份**：搬到 history 的邏輯新版可考慮改為 soft delete 或 TTL 機制。
