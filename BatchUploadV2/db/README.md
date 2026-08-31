# BatchUpload DB 文件索引

## 資料庫對應

| 表名 | 所在 DB | 說明 |
|-----|---------|------|
| `BatchUpload` | `WebStoreDB` | 批次作業主檔 |
| `BatchUploadData` | `WebStoreDB` | 每個工作單元的執行狀態與資料 |
| `BatchUploadMessage` | `WebStoreDB` | 錯誤明細訊息 |
| `BatchUploadAverageTime` | `WebStoreDB` | 各批次類型的平均執行時間統計 |
| `Definition` | `WebStoreDB` | 批次類型定義（下拉選單來源） |
| `Task` | `NMQV2DB` | NMQ 工作排程記錄 |

## 文件清單

- [BatchUpload.md](./BatchUpload.md) — 主表欄位與常用查詢
- [BatchUploadData.md](./BatchUploadData.md) — 工作佇列表欄位與常用查詢
- [BatchUploadMessage.md](./BatchUploadMessage.md) — 錯誤明細表欄位與常用查詢
- [BatchUploadAverageTime.md](./BatchUploadAverageTime.md) — 平均時間統計表
- [Definition.md](./Definition.md) — 批次類型定義查詢
- [NMQ-Task.md](./NMQ-Task.md) — NMQ Task 查詢
