## 表用途

`order_create_queue` 是**建單任務佇列**，定期購排程建立實際訂單時，  
會先將任務寫入此表，再由 Worker 依序取出呼叫訂單 API。  
`ocq_occupy_id` 用於分散式鎖，避免多 Worker 同時搶同一筆任務。

### Status 說明

#### `ocq_status`（任務狀態）
| 值 | 意義 |
|----|------|
| 0  | 未建立（初始狀態，等待 Worker 處理） |
| 1  | 建立成功 |
| 8  | 建立異常（需人工確認） |
| 9  | 刪除 |

---

## 查詢 Script

```sql
-- 最新 20 筆佇列任務
SELECT ocq_pk, ocq_tgid, ocq_txid, ocq_occupy_id, ocq_status, ocq_create_time
FROM order_create_queue
ORDER BY ocq_pk DESC
LIMIT 20;

-- 查異常任務
SELECT * FROM order_create_queue
WHERE ocq_status = 8
ORDER BY ocq_pk DESC
LIMIT 20;

-- 查未處理任務（積壓監控）
SELECT COUNT(*) AS pending_count
FROM order_create_queue
WHERE ocq_status = 0;
```
