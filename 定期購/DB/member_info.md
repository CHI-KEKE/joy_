## 表用途

`member_info` 是**定期購系統的會員資料快取表**，從 OSM（Order System Management）同步而來。  
`RegularOrderInitJob` 建立計畫時，以 `(mbi_shopid, mbi_mbid)` 查詢此表取得內部 PK，  
若找不到則先 INSERT 一筆新記錄。  
`mbi_mobile`、`mbi_email`、`mbi_nickname` 以加密 `byte[]` 儲存。

### Status 說明

#### `mbi_status`（會員狀態）
| 值 | 意義 |
|----|------|
| 0  | 未啟動 |
| 1  | 已啟動（正常） |
| 8  | 停權 |
| 9  | 刪除 |

#### `mbi_notify`（推播狀態）
| 值         | 意義 |
|-----------|------|
| 0 / false | 未啟動 |
| 1 / true  | 已啟動 |

---

```sql
 -- 最新 10 筆會員資料
 SELECT *
 FROM member_info
 ORDER BY mbi_pk DESC
 LIMIT 10;
 
 -- 依手機查（注意欄位可能加密）
 SELECT * FROM member_info
 WHERE mbi_shopid = 12488
 LIMIT 10;
```