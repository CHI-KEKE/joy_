# delete-log-file-housekeeper — 邏輯說明

> PHP 檔案：`admin/daemon/delete_log_file_housekeeper.php`
> 對應排程：未在 Rancher 找到（可能尚未部署或已整合到其他排程）
> pg_id：7（與 stock-estimate-housekeeper 共用同一 pg_id！）

---

## 一句話摘要

刪除 `/webhome/logs/web/api/` 目錄下 30 天前的 log 檔案，是純粹的維護性 job，與定期購業務邏輯無關。

---

## 執行流程

```
main()
├── 計算 30 天前的日期（date("Ymd", strtotime("- 30 days"))）
├── glob("/webhome/logs/web/api/*_YYYYMMDD*") → 找出符合日期的檔案
└── foreach del_file
    └── unlink() → 刪除檔案
```

---

## 刪除規則

| 項目 | 值 |
|---|---|
| 目錄 | `/webhome/logs/web/api/` |
| 保留天數 | 30 天 |
| 檔案名格式 | `*_YYYYMMDD*`（日期嵌在檔名中） |

---

## 涉及資料表

> 無，此 job 只操作檔案系統，不讀寫 DB。

---

## 外部依賴

| 依賴 | 說明 |
|---|---|
| 本地檔案系統 | 只刪除 Pod 內的 log 檔，不涉及外部服務 |

---

## ⚠️ pg_id = 7 重複問題

```
stock-estimate-housekeeper → pg_id = 7
delete-log-file-housekeeper → pg_id = 7  ← 相同！
```
> 兩支 job 共用同一個 `daemon_list` 開關（pg_id=7），代表關閉其中一支會同時關閉另一支。這是 PHP 版本的已知問題。

---

## 翻寫 .NET Core 注意事項

1. **很可能不需要移植**：.NET Core 版本的 log 通常集中到 CloudWatch / Elasticsearch / Loki 等集中式 log 系統，不再存放在 Pod 本地，此 job 可能直接廢棄。
2. **pg_id 衝突**：新版若需保留，應給予獨立的 pg_id，不與 stock-estimate 共用。
3. **log 保留策略**：改用集中式 log 後，保留天數由 log 系統設定控制，不需要另寫排程。
