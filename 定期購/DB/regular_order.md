## 表用途

`regular_order` 是**定期購計畫主檔**，每個訂閱計畫對應一筆記錄。  
會員結帳時由 `RegularOrderInitJob` 寫入，後續排程（`RegularCreateOrderJob`）以  
`rgo_next_ship_date` 與 `rgo_status` 決定是否觸發該期建單。

### Status 說明

#### `rgo_status`（資料狀態）
| 值 | 意義 |
|----|------|
| 1  | 進行中（正常訂閱） |
| 2  | 暫停 |
| 3  | 終止（會員取消） |
| 9  | 刪除 |

#### `rgo_create_order_status`（最新一期建單狀態）
| 值 | 意義 |
|----|------|
| 1  | 建單成功 |
| 2  | 建單異常 |

#### `rgo_ship_limit_status`（配送次數限制）
| 值 | 意義 |
|----|------|
| 1  | 無限制 |
| 2  | 有限制（搭配 `rgo_ship_limit_count` 使用） |

#### `rgo_in_schedule`（排程鎖）
| 值         | 意義 |
|-----------|------|
| 0 / false | 可執行產單 |
| 1 / true  | 排程正在執行中（防止重複觸發） |

---

## 查詢 Script

```sql
-- 最新 10 筆計畫主檔
SELECT rgo_pk, rgo_source_tg, rgo_source_ts, rgo_shop_id, rgo_member_id,
       rgo_ship_period, rgo_next_ship_date, rgo_status, rgo_create_order_status
FROM regular_order
ORDER BY rgo_pk DESC
LIMIT 10;

-- 查特定商店的進行中計畫
SELECT * FROM regular_order
WHERE rgo_shop_id = 10230
AND rgo_status = 1
ORDER BY rgo_pk DESC
LIMIT 20;
```
