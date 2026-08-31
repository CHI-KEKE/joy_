## 表用途

`regular_order_detail` 是**定期購計畫明細檔**，每期配送對應一筆記錄。  
首次由 `RegularOrderInitJob` 建立第 1 期（`rod_ship_sequence = 1`）；  
後續每次到期後由 `RegularCreateOrderJob` 新增下一期，並更新 `regular_order_map` 指向最新期。  
收件人資訊（姓名、手機、地址）以加密 `byte[]` 儲存。

### Status 說明

#### `rod_status`（資料狀態）
| 值 | 意義 |
|----|------|
| 1  | 正常 |
| 9  | 刪除 |

#### `rod_create_order_status`（本期建單狀態）
| 值 | 意義 |
|----|------|
| 0  | 初始（尚未建單） |
| 1  | 建單成功 |
| 2  | 建單異常 |

---

```sql
 -- 最新 10 筆明細
 SELECT rod_pk, rod_rgo_pk, rod_tg_code, rod_ts_code, rod_shop_id,
        rod_qty, rod_sale_price, rod_status, rod_create_order_status, rod_create_time
 FROM regular_order_detail
 ORDER BY rod_pk DESC
 LIMIT 10;
 
 -- 依主單 PK 查明細
 SELECT * FROM regular_order_detail
 WHERE rod_rgo_pk = 478;
```