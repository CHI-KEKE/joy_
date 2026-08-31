# BatchUploadMessage 錯誤明細表

**DB：** `WebStoreDB`

## 欄位說明

| 欄位 | 型別 | Nullable | 說明 |
|------|------|----------|------|
| `BatchUploadMessage_Id` | long | ❌ | PK |
| `BatchUploadMessage_BatchUploadId` | long | ❌ | FK → BatchUpload_Id |
| `BatchUploadMessage_BatchUploadDataId` | long | ✅ | FK → BatchUploadData_Id（可對應到具體工作單元） |
| `BatchUploadMessage_TypeDef` | string(50) | ✅ | 批次類型 |
| `BatchUploadMessage_StatusDef` | string | ✅ | 訊息類型，目前固定寫 `ValidateFailed`（即使 ProcessFailed 也是同值，歷史包袱） |
| `BatchUploadMessage_Title` | string | ✅ | 工作單元代表名稱（如 MemberCode） |
| `BatchUploadMessage_Note` | string | ✅ | **錯誤訊息內容** |
| `BatchUploadMessage_CreatedDateTime` | DateTime | ❌ | 建立時間 |
| `BatchUploadMessage_CreatedUser` | string(50) | ✅ | 建立者 |
| `BatchUploadMessage_UpdatedTimes` | byte | ❌ | 更新次數 |
| `BatchUploadMessage_UpdatedDateTime` | DateTime | ❌ | 最後更新時間 |
| `BatchUploadMessage_UpdatedUser` | string(50) | ✅ | 最後更新者 |
| `BatchUploadMessage_ValidFlag` | bool | ❌ | 資料有效旗標 |
| `BatchUploadMessage_Rowversion` | byte[] | ❌ | 樂觀鎖版本號 |

---

## 寫入時機

| 階段 | 寫入時機 | 筆數 |
|------|---------|------|
| InitBatchData（LoadData 期間） | 逐列查核失敗時暫存，主 NMQ 結束前批次寫入 | 每個查核失敗 1 筆 |
| DoValidate | 驗證失敗時暫存，主 NMQ 結束前批次寫入 | 每個 ErrorMessage 1 筆 |
| DoProcess（業務失敗） | 執行失敗時暫存，子 Task 每輪結束時寫入 | 業務邏輯自訂 |
| DoProcess（Exception 達 Retry 上限） | 達上限時暫存，子 Task 每輪結束時寫入 | 固定 1 筆：「資料處理異常，請下載檔案後重新執行」 |

> ⚠️ Exception 尚未達 Retry 上限時**不寫 Message**，資料狀態維持 `ReadyToProcess`。

---

## 常用查詢

```sql
USE WebStoreDB

-- 查某批次所有錯誤明細
SELECT
    BatchUploadMessage_Id,
    BatchUploadMessage_BatchUploadDataId,
    BatchUploadMessage_Title,
    BatchUploadMessage_StatusDef,
    BatchUploadMessage_Note,
    BatchUploadMessage_CreatedDateTime
FROM BatchUploadMessage (NOLOCK)
WHERE BatchUploadMessage_ValidFlag = 1
  AND BatchUploadMessage_BatchUploadId = {BatchUpload_Id}
ORDER BY BatchUploadMessage_Id

-- 查某批次錯誤訊息彙整（依訊息內容分組計數）
SELECT
    BatchUploadMessage_Note,
    COUNT(*) AS cnt
FROM BatchUploadMessage (NOLOCK)
WHERE BatchUploadMessage_ValidFlag = 1
  AND BatchUploadMessage_BatchUploadId = {BatchUpload_Id}
GROUP BY BatchUploadMessage_Note
ORDER BY cnt DESC

-- 查某工作單元的錯誤明細
SELECT *
FROM BatchUploadMessage (NOLOCK)
WHERE BatchUploadMessage_ValidFlag = 1
  AND BatchUploadMessage_BatchUploadDataId = {BatchUploadData_Id}
```
