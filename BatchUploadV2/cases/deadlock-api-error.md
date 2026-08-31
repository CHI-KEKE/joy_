# 案例：SQL Deadlock 導致大量批次中途中止

## 案例資訊

| 項目 | 值 |
|------|-----|
| 批次 Code | `BA2503211400003` |
| BatchUploadId | 11519 |
| TypeDef | `BatchModifyPromotionOuterId` |
| 發生時間 | 2025-03-21 14:40 |
| 上傳筆數 | 10 萬筆料號 |
| 實際完成 | 約 6 萬筆後中止 |

<br>

---

## 症狀

批次更新料號（10 萬筆），執行至約 6 萬多筆後異常終止，未完成所有資料。

<br>

---

## 根本原因

子 Task 呼叫促購後台（SCMAPIV2）時發生 **SQL Deadlock**：

**API 錯誤 Response：**

```json
{
    "errorCode": "InternalServerError",
    "message": "SqlException",
    "data": "Transaction (Process ID 196) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction."
}
```

多個子 Task 並行執行，對同一批次的促購活動進行大量料號更新時，DB 產生鎖定資源競爭，系統選擇其中一個交易作為死鎖受害者並中止。

<br>

---

## 排查步驟

### Step 1. 確認失敗的 BatchUploadData

```sql
USE WebStoreDB

SELECT *
FROM BatchUploadData (NOLOCK)
WHERE BatchUploadData_ValidFlag = 1
  AND BatchUploadData_BatchUploadId = 11519
  AND BatchUploadData_StatusDef = 'ProcessFailed'
```

**失敗範例資料：**

| 欄位 | 值 |
|------|-----|
| BatchUploadData_Id | 1835340 |
| BatchUploadData_StatusDef | `ProcessFailed` |
| BatchUploadData_UpdatedUser | `BatchUpload` |

**BatchUploadData_Data 內容（含大量料號）：**

```json
{
    "ScheduleTime": "3/21/2025 2:40:00 PM",
    "PromotionId": 6298,
    "OuterIds": ["Z7BkR", "tfu8R", "pQqQC", ...],
    "ModifyStatus": "Add"
}
```

### Step 2. 確認錯誤訊息

```sql
USE WebStoreDB

SELECT BatchUploadMessage_StatusDef,
       BatchUploadMessage_Note
FROM BatchUploadMessage (NOLOCK)
WHERE BatchUploadMessage_ValidFlag = 1
  AND BatchUploadMessage_BatchUploadId = 11519
  AND BatchUploadMessage_StatusDef = 'ValidateFailed'
```

### Step 3. 確認 NMQ 子 Task 狀態

```sql
USE NMQV2DB

-- BatchModifyPromotionOuterIdTask JobId = 486
SELECT *
FROM Task (NOLOCK)
WHERE Task_ValidFlag = 1
  AND Task_JobId = 486
ORDER BY Task_CreatedDatetime DESC
```

<br>

---

## 解法

此案例需由促購後台 DB 層解決 Deadlock 問題（重試機制或調整 Transaction 隔離層級）。

BatchUpload 層的處置：

- 已失敗的 `BatchUploadData_StatusDef = ProcessFailed` 需手動確認是否需要重跑
- 若需重跑，可考慮重設 `BatchUploadData_StatusDef` 為 `ReadyToProcess` 並觸發新 Task

<br>

---

## 相關錯誤判斷位置

```csharp
// 若發生錯誤會 InsertUploadFailedMessageList 至 BatchUploadMessage Table
// BatchUploadMessage_StatusDef = ValidateFailed
// BatchUploadMessage_Note = errorMessage
// BatchUploadData_StatusDef = ProcessFailed
```
