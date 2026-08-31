# BatchUploadData 工作佇列表

**DB：** `WebStoreDB`

## 欄位說明

| 欄位 | 型別 | Nullable | 說明 |
|------|------|----------|------|
| `BatchUploadData_Id` | long | ❌ | PK |
| `BatchUploadData_BatchUploadId` | long | ❌ | FK → BatchUpload_Id |
| `BatchUploadData_SupplierId` | int | ❌ | 廠商 ID |
| `BatchUploadData_TypeDef` | string(50) | ❌ | 批次類型（同主表） |
| `BatchUploadData_StatusDef` | string(30) | ❌ | 此工作單元的執行狀態（見下方） |
| `BatchUploadData_Title` | string(500) | ✅ | 工作單元代表名稱（如 MemberCode、TradesOrderCode），方便人工識別 |
| `BatchUploadData_Data` | string(1000) | ✅ | **此工作單元的完整資料，JSON 序列化後存入** |
| `BatchUploadData_CreatedDateTime` | DateTime | ❌ | 建立時間 |
| `BatchUploadData_CreatedUser` | string(50) | ✅ | 建立者（`BulkInsertBatchUploadData` 或 `csp_InsertBatchUploadData`） |
| `BatchUploadData_UpdatedTimes` | byte | ❌ | 更新次數，**Exception 時被借用為 Retry 計數器** |
| `BatchUploadData_UpdatedDateTime` | DateTime | ❌ | 最後更新時間 |
| `BatchUploadData_UpdatedUser` | string(50) | ✅ | 最後更新者（執行 NMQ 的上傳者） |
| `BatchUploadData_ValidFlag` | bool | ❌ | 資料有效旗標 |
| `BatchUploadData_Rowversion` | byte[] | ❌ | 樂觀鎖版本號 |

> ⚠️ `BatchUploadData_Data` 欄位長度為 varchar(1000)，若 JSON 過長可能被截斷。

---

## 狀態說明（BatchUploadData_StatusDef）

| 狀態 | 說明 | 下一步 |
|------|------|--------|
| `ReadyToValidate` | LoadData 寫入後的初始狀態 | 主 NMQ DoValidate 處理 |
| `ValidateFailed` | DoValidate 驗證失敗 | 終止，不進子 Task |
| `ReadyToProcess` | 驗證通過，等待子 Task | 子 Task 撈取執行 |
| `ProcessFailed` | 業務執行失敗 或 Exception 達 Retry 上限 | 終止 |
| `Finish` | 執行成功 | 終止 |

---

## 分組邏輯（Excel 列數 vs BatchUploadData 筆數）

**一列 Excel ≠ 一筆 BatchUploadData**，由各 Service 的分組 Key 決定：

| 模式 | 說明 | 例子 |
|------|------|------|
| 多列合成一筆 | 同 Key 的多列合併 | 黑名單以 MemberCode 分組 |
| 每列各自獨立 | 1 Excel 列 = 1 筆 | SalePage 相關批次 |
| 匯出型 | 0 列 Excel = 1 筆（存查詢條件） | 訂單匯出批次 |

---

## 常用查詢

```sql
USE WebStoreDB

-- 查某批次所有工作單元的狀態分布
SELECT BatchUploadData_StatusDef, COUNT(*) AS cnt
FROM BatchUploadData (NOLOCK)
WHERE BatchUploadData_ValidFlag = 1
  AND BatchUploadData_BatchUploadId = {BatchUpload_Id}
GROUP BY BatchUploadData_StatusDef

-- 查所有工作單元明細
SELECT *
FROM BatchUploadData (NOLOCK)
WHERE BatchUploadData_ValidFlag = 1
  AND BatchUploadData_BatchUploadId = {BatchUpload_Id}
ORDER BY BatchUploadData_Id

-- 查失敗的工作單元
SELECT
    BatchUploadData_Id,
    BatchUploadData_Title,
    BatchUploadData_StatusDef,
    BatchUploadData_UpdatedTimes,
    BatchUploadData_Data
FROM BatchUploadData (NOLOCK)
WHERE BatchUploadData_ValidFlag = 1
  AND BatchUploadData_BatchUploadId = {BatchUpload_Id}
  AND BatchUploadData_StatusDef IN ('ValidateFailed', 'ProcessFailed')

-- 查還有多少筆待執行
SELECT COUNT(*) AS PendingCount
FROM BatchUploadData (NOLOCK)
WHERE BatchUploadData_ValidFlag = 1
  AND BatchUploadData_BatchUploadId = {BatchUpload_Id}
  AND BatchUploadData_StatusDef = 'ReadyToProcess'

-- 查正在重試中的工作單元（Exception 但尚未達 RetryTimes）
SELECT
    BatchUploadData_Id,
    BatchUploadData_Title,
    BatchUploadData_UpdatedTimes,
    BatchUploadData_UpdatedDateTime
FROM BatchUploadData (NOLOCK)
WHERE BatchUploadData_ValidFlag = 1
  AND BatchUploadData_BatchUploadId = {BatchUpload_Id}
  AND BatchUploadData_StatusDef = 'ReadyToProcess'
  AND BatchUploadData_UpdatedTimes > 0
```
