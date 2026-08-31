# BatchUpload 主表

**DB：** `WebStoreDB`

## 欄位說明

| 欄位 | 型別 | Nullable | 說明 |
|------|------|----------|------|
| `BatchUpload_Id` | long | ❌ | PK |
| `BatchUpload_SupplierId` | int | ❌ | 廠商（Supplier）ID |
| `BatchUpload_ShopId` | long | ✅ | 商店 ID |
| `BatchUpload_TypeDef` | string | ❌ | 批次類型（對應 `BatchUploadTypeDefEnum` 字串名稱） |
| `BatchUpload_ExecuteTaskType` | string | ✅ | 子 Task 類型（對應 `BatchUploadExecuteTaskTypeEnum`） |
| `BatchUpload_StatusDef` | string | ❌ | 目前狀態（見狀態機說明） |
| `BatchUpload_Code` | string | ✅ | 批次作業代碼（如 `BA2405281000001`） |
| `BatchUpload_FileName` | string | ✅ | 上傳的 Excel 檔名 |
| `BatchUpload_UploadGuid` | string | ✅ | 上傳暫存 GUID，對應實體檔案路徑 |
| `BatchUpload_TotalCount` | int | ❌ | 總筆數（DoValidate 後寫入） |
| `BatchUpload_SuccessCount` | int | ❌ | 成功筆數 |
| `BatchUpload_FailCount` | int | ❌ | 失敗筆數 |
| `BatchUpload_HasDetail` | bool | ✅ | 是否有錯誤明細（有 BatchUploadMessage 時為 true） |
| `BatchUpload_FailDownloadPath` | string | ✅ | 錯誤明細下載路徑 |
| `BatchUpload_ExportFileDownloadPath` | string | ✅ | 匯出檔案下載路徑（匯出型批次使用） |
| `BatchUpload_EstimateFinishMinutes` | int | ✅ | 預估完成分鐘數 |
| `BatchUpload_AverageSecondsPerRecord` | int | ✅ | 平均每筆秒數（由 BatchUploadAverageTime 查出） |
| `BatchUpload_UploadDateTime` | DateTime | ❌ | 上傳時間 |
| `BatchUpload_StartDateTime` | DateTime | ✅ | 開始執行時間 |
| `BatchUpload_FinishDateTime` | DateTime | ✅ | 完成時間 |
| `BatchUpload_ScheduledDateTime` | DateTime | ✅ | 預約排程執行時間（null 表示立即執行） |
| `BatchUpload_Note` | string | ✅ | 備註 |
| `BatchUpload_CreatedDateTime` | DateTime | ❌ | 建立時間 |
| `BatchUpload_CreatedUser` | string | ✅ | 建立者 |
| `BatchUpload_UpdatedTimes` | byte | ❌ | 更新次數 |
| `BatchUpload_UpdatedDateTime` | DateTime | ❌ | 最後更新時間 |
| `BatchUpload_UpdatedUser` | string | ✅ | 最後更新者 |
| `BatchUpload_ValidFlag` | bool | ❌ | 資料有效旗標（查詢時務必加此條件） |
| `BatchUpload_Rowversion` | byte[] | ❌ | 樂觀鎖版本號 |

---

## 狀態說明（BatchUpload_StatusDef）

| 狀態 | 說明 |
|------|------|
| `WaitingToUpload` | 建立後等待上傳 |
| `Uploading` | 上傳中 |
| `WaitingToProcess` | 上傳完成，等待 NMQ 執行 |
| `InProcess` | 主 NMQ 或子 Task 執行中 |
| `Finish` | 全部成功完成 |
| `FinishWithError` | 完成但有部分失敗 |
| `FinishWithUnExpectedError` | 非預期錯誤中止 |
| `ScheduleAbortedWithErrors` | 預約批次驗證失敗中止 |

---

## 常用查詢

```sql
USE WebStoreDB

-- 查詢特定 BatchUpload
SELECT *
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_Id = {BatchUpload_Id}

-- 查詢某廠商所有 InProcess 作業（確認是否塞車）
SELECT *
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_SupplierId = {SupplierId}
  AND BatchUpload_StatusDef = 'InProcess'
ORDER BY BatchUpload_UploadDateTime

-- 查詢某廠商最近 N 筆批次記錄
SELECT TOP 20 *
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_SupplierId = {SupplierId}
ORDER BY BatchUpload_Id DESC

-- 查詢有錯誤的批次（各廠商錯誤數排行）
SELECT BatchUpload_SupplierId, COUNT(*) AS ErrorCount
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_StatusDef = 'FinishWithError'
GROUP BY BatchUpload_SupplierId
ORDER BY ErrorCount DESC

-- 查詢卡住的批次（InProcess 超過 2 小時）
SELECT *
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_StatusDef = 'InProcess'
  AND BatchUpload_StartDateTime < DATEADD(HOUR, -2, GETDATE())
ORDER BY BatchUpload_StartDateTime

-- 查詢某批次類型的執行時間統計
SELECT
    BatchUpload_TypeDef,
    COUNT(*) AS TotalCount,
    AVG(DATEDIFF(SECOND, BatchUpload_StartDateTime, BatchUpload_FinishDateTime)) AS AvgSeconds,
    MAX(DATEDIFF(SECOND, BatchUpload_StartDateTime, BatchUpload_FinishDateTime)) AS MaxSeconds
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_StatusDef IN ('Finish', 'FinishWithError')
  AND BatchUpload_StartDateTime IS NOT NULL
  AND BatchUpload_FinishDateTime IS NOT NULL
  AND BatchUpload_TypeDef = '{TypeDef}'
GROUP BY BatchUpload_TypeDef
```
