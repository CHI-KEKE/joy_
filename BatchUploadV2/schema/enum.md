# Enum / TypeDef 欄位對照

## DB 欄位對照表

| 欄位 | Table | 型別 | 說明 |
|------|-------|------|------|
| `BatchUpload_TypeDef` | `WebStoreDB.dbo.BatchUpload` | `varchar(50)` | 批次類型 |
| `BatchUploadData_TypeDef` | `WebStoreDB.dbo.BatchUploadData` | `varchar(30)` | 子資料批次類型 |
| `BatchUploadMessage_TypeDef` | `WebStoreDB.dbo.BatchUploadMessage` | `varchar(50)` | 訊息批次類型 |
| `BatchUploadAverageTime_TypeDef` | `WebStoreDB.dbo.BatchUploadAverageTime` | `varchar(50)` | 平均時間類型 |
| `BatchUpload_ExecuteTaskType` | `WebStoreDB.dbo.BatchUpload` | `varchar(50)` | 子 Task 類型 |
| `Definition_Code` | `WebStoreDB.dbo.Definition` | `varchar(30)` | 定義代碼（**上限 30 字**） |
| `Job_Name` | `NMQV2DB.dbo.Job` | `varchar(50)` | NMQ Job 名稱 |

<br>

---

## 相關 Enum（程式碼）

- `BatchUploadTypeDefEnum` — 批次類型
- `BatchUploadExecuteTaskTypeEnum` — 子 Task 執行類型

<br>

---

## 各批次類型對應值

| TypeDef | ExecuteTaskType | NMQ JobId |
|---------|----------------|-----------|
| `ModifyRewardPromotionSalePage` | `BatchModifyPromotionSalePageTask` | 334 |
| `BatchModifyPromotionOuterId` | `BatchModifyPromotionOuterIdTask` | 486 |
| `ExportRewardPromotionSalePage` | `ExportRewardPromotionSalePageTask` | 149 |
| `ExportRewardPromotionOuterId` | `ExportRewardPromotionOuterIdTask` | 487 |
| `BatchCrmOrderImport` | `BatchCrmOrderImportTask` | — |
| `BatchCrmMemberImport` | `BatchCrmMemberImportTask` | 290 |

> 新增批次類型時，也需要在 `WebStoreDB.dbo.Definition` 新增對應定義，詳見 [guides/add-new-type.md](../guides/add-new-type.md)。
