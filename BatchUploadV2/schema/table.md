# BatchUpload Table 資料結構

## BatchUpload

| 欄位名稱 | 說明 |
|---------|------|
| `BatchUpload_Id` | 主鍵 |
| `BatchUpload_SupplierId` | 廠商（商店）ID |
| `BatchUpload_TypeDef` | 批次類型（對應 Enum） |
| `BatchUpload_FileName` | 上傳的 Excel 檔名 |
| `BatchUpload_StatusDef` | 目前狀態（見 [status-machine.md](./status-machine.md)） |
| `BatchUpload_Code` | 批次作業代碼（如 `BA2405281000001`） |
| `BatchUpload_TotalCount` | 總筆數 |
| `BatchUpload_SuccessCount` | 成功筆數 |
| `BatchUpload_FailCount` | 失敗筆數 |
| `BatchUpload_UploadDateTime` | 上傳時間 |
| `BatchUpload_StartDateTime` | 開始執行時間 |
| `BatchUpload_FinishDateTime` | 完成時間 |
| `BatchUpload_UploadGuid` | 上傳暫存 GUID |
| `BatchUpload_HasDetail` | 是否有錯誤明細 |
| `BatchUpload_FailDownloadPath` | 錯誤明細下載路徑 |
| `BatchUpload_EstimateFinishMinutes` | 預估完成分鐘數 |
| `BatchUpload_AverageSecondsPerRecord` | 平均每筆秒數 |
| `BatchUpload_ExecuteTaskType` | 子 Task 類型（對應 `BatchUploadExecuteTaskTypeEnum`） |
| `BatchUpload_ExportFileDownloadPath` | 匯出檔案下載路徑 |
| `BatchUpload_ShopId` | 商店 ID |
| `BatchUpload_ScheduledDateTime` | 排程執行時間 |

<br>

---

## BatchUploadData

每一列 Excel 對應一筆 BatchUploadData。

| 欄位名稱 | 說明 |
|---------|------|
| `BatchUploadData_Id` | 主鍵 |
| `BatchUploadData_BatchUploadId` | 所屬 BatchUpload |
| `BatchUploadData_SupplierId` | 廠商 ID |
| `BatchUploadData_TypeDef` | 批次類型（`varchar(30)`） |
| `BatchUploadData_TypeDefDesc` | 批次類型說明 |
| `BatchUploadData_StatusDef` | 子資料狀態（見 [status-machine.md](./status-machine.md)） |
| `BatchUploadData_Title` | 標題（如 PromotionId） |
| `BatchUploadData_CreatedUser` | 建立者（`BulkInsertBatchUploadData` 或 `csp_InsertBatchUploadData`） |
| `BatchUploadData_UpdatedUser` | 最後更新者 |

<br>

---

## BatchUploadMessage

子 Task 執行失敗時寫入的錯誤明細。

| 欄位名稱 | 說明 |
|---------|------|
| `BatchUploadMessage_Id` | 主鍵 |
| `BatchUploadMessage_BatchUploadId` | 所屬 BatchUpload |
| `BatchUploadMessage_TypeDef` | 批次類型（`varchar(50)`） |
| `BatchUploadMessage_StatusDef` | 訊息狀態（如 `ValidateFailed`） |
| `BatchUploadMessage_Note` | 錯誤訊息內容 |

<br>

---

## BatchUploadAverageTime

| 欄位名稱 | 說明 |
|---------|------|
| `BatchUploadAverageTime_TypeDef` | 批次類型（`varchar(50)`） |
