# NMQ 執行流程與子 Task 機制

## 整體流程概覽

```bash
主 NMQ (BatchUploadProcess)
  └─ LoadData / DoValidate / CreateBatchUploadData
  └─ 發動「一個」子 Task（帶著 ProcessCount）
           ↓
子 Task (BatchUploadTaskProcess 或自訂 Worker)
  └─ GetReadyToProcessDataList → 撈 N 筆 ReadyToProcess 的 BatchUploadData
  └─ DoBatchProcess → foreach 逐筆執行
  └─ 執行完後 → 再查 DB，還有 ReadyToProcess 嗎？
       ├─ 有 → re-queue 自己（建立新的相同子 Task，delay N 秒）
       └─ 沒有 → 更新主 BatchUpload 狀態為 Finish，結束
```

---

## 主 NMQ 只發動一個子 Task

- 主 NMQ **不會**一次建立多個並行子 Task
- 只建立「第一個」子 Task，後續的 re-queue 由子 Task 自己負責
- `ProcessCount` 控制每輪子 Task 處理幾筆（AppSetting：`BatchUpload.{TypeDef}.TaskProcessCount`，預設 `"10"`）

---

## 子 Task 的 re-queue 機制

子 Task 執行完一批後，**不是被外部觸發**，而是**自己重新建立一個新的子 Task 訊息**丟回 NMQ Queue。

```bash
子 Task [第1輪: 撈10筆 → 跑完]
    └─ re-queue ──→ 子 Task [第2輪: 撈10筆 → 跑完]
                        └─ re-queue ──→ 子 Task [第3輪: 撈0筆]
                                            └─ 沒資料 → Finish
```

每次 re-queue 都是全新的 Task 訊息，因此：
- 每次都要重新查 DB 確認還有沒有 `ReadyToProcess` 資料
- 每次 re-queue 都有 delay（AppSetting：`BatchUpload.BookingTime.DelaySeconds`，預設 `"10"` 秒）

### 累積時間試算（1000 筆 × 1 秒/筆）

| 項目 | 值 |
|------|---|
| 總筆數 | 1000 筆 |
| ProcessCount | 10 筆/次 |
| 每筆 DoProcess | 1 秒 |
| re-queue delay | 10 秒 |
| **總計** | **100 × (10 + 10) = 2000 秒 ≈ 33 分鐘** |

---

## 子 Task 名稱由 ExecuteTaskType 決定

主 NMQ 在發動子 Task 時，Job 名稱**不固定**，由 `BatchUpload_ExecuteTaskType` 欄位決定：

```csharp
// BatchUploadProcess.cs
if (Enum.TryParse<BatchUploadExecuteTaskTypeEnum>(
    this.BatchUploadEntity.BatchUpload_ExecuteTaskType, out batchUploadExecuteTaskType))
{
    taskHelper.CreateTask(batchUploadExecuteTaskType.ToString(), taskData, bookingTime);
}
else
{
    // 預設
    taskHelper.CreateTask(BatchUploadExecuteTaskTypeEnum.BatchUploadTask.ToString(), taskData, bookingTime);
}
```

### 已知子 Task 類型

| Task 名稱 | 對應場景 | Worker 類別 |
|-----------|---------|------------|
| `BatchUploadTask` | 標準大量上傳（預設） | `BatchUploadTaskProcess` |
| `SalesOrderDataExportTask` | 訂單資料匯出 | 自訂 Worker（不走標準流程） |
| 其他自訂 Task | 有特殊執行邏輯 | 各自獨立實作 |

`ExecuteTaskType` 在 SMS 端由 `GetBatchUploadExecuteTaskType()` 的 switch case 決定（100+ case）。

---

## 標準子 Task vs 自訂子 Task

### 標準（BatchUploadTask）
走 `BatchUploadTaskProcess` 框架：
```
GetReadyToProcessDataList
  → DoBatchProcess (foreach DoProcess)
  → 判斷是否 re-queue
  → 更新 Finish 狀態
```

### 自訂（如 SalesOrderDataExportTask）
- 完全獨立的 Worker 類別
- 不一定有 re-queue 機制
- 不一定走 `BatchUploadBaseService<T>` 那一套
- 需額外在 NMQ 端建立 Worker Class 並完成 DI 註冊

---

## 新增批次類型時的注意事項

若新批次需要**自訂執行邏輯**（非標準上傳流程），需額外：

1. 在 `BatchUploadExecuteTaskTypeEnum` 加入新 Task 名稱
2. 在 SMS `GetBatchUploadExecuteTaskType()` 加入對應 case
3. 在 SCMNMQ 建立對應的 Task Worker Class
4. 在 NMQ 端完成 Worker 的 DI 註冊

這幾步在現有開發 guide 中容易被遺漏。

---

## 相關 AppSetting 一覽

| AppSetting Key | 預設值 | 說明 |
|---------------|--------|------|
| `BatchUpload.{TypeDef}.TaskProcessCount` | `"10"` | 每輪子 Task 處理筆數 |
| `BatchUpload.BookingTime.DelaySeconds` | `"10"` | 子 Task re-queue 等待秒數 |
| `BatchUpload.{TypeDef}.TaskExecuted.ExpireSeconds` | `"50"` | IsBatchTaskExecuted 防重複執行的有效期 |
| `BatchUpload.Supplier.JobLimit` | `"10"` | Quota 上限（最多幾個 InProcess） |
| `BatchUpload.JobLimit.DelaySeconds` | `"120"` | Quota 超限時的 re-queue 等待秒數 |
