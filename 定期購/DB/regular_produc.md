## 表用途

`regular_product` 是**定期購商品設定檔**，記錄商店開啟定期購功能的商品資訊。  
存放商品的配送週期選項（`rpd_ship_period` 為 JSON 格式）、次數限制、付款/配送方法等。  
`RegularOrderInitJob` 建立計畫主檔時會以 `rpd_page_id` 查此表取得 `rpd_pk`。

### Status 說明

#### `rpd_status`（資料狀態）
| 值 | 意義 |
|----|------|
| 1  | 已啟動（上架中） |
| 8  | 停用（已下架） |
| 9  | 刪除 |

#### `rpd_ship_limit_status`（配送次數限制）
| 值 | 意義 |
|----|------|
| 1  | 無限制 |
| 2  | 有限制（搭配 `rpd_ship_limit_count` 使用） |

---

```sql
  -- 最新 10 筆定期購商品
 SELECT rpd_pk, rpd_shop_id, rpd_page_id, rpd_status, rpd_create_time
 FROM regular_product
 ORDER BY rpd_pk DESC
 LIMIT 10;
 
 -- 依商店查
 SELECT * FROM regular_product
 WHERE rpd_shop_id = 10230
 AND rpd_status != 9
 LIMIT 10;
```