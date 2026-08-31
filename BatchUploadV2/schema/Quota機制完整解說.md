# BatchUpload Quota 機制完整解說

> 對應程式碼：`BatchUploadTaskProcess.DoJob`（子 Task 執行入口）、`BatchUploadRepository.GetBatchUploadProcesseIdList`（DA 層）

---

## 答案：Quota 計算的是主 BatchUpload 筆數

Quota 查的是 **`BatchUpload` 主表**，條件為同廠商 (`SupplierId`) 且狀態為 `InProcess`。
不是子 Task 數量，也不是 `BatchUploadData` 筆數。

```csharp
// BatchUploadRepository.cs（DA 層實作）
var list = (from batchUpload in context.BatchUpload.Valids()
            where batchUpload.BatchUpload_SupplierId == supplierId
               && batchUpload.BatchUpload_StatusDef == "InProcess"
            select batchUpload).ToList();
```

> 注意：使用 `ReadUncommitted` isolation level（等同 `NOLOCK`），為避免 Quota 查詢被其他事務阻塞。

---

## 完整判斷邏輯

```csharp
// BatchUploadTaskProcess.DoJob（子 Task 每次執行時都會跑這段）
int jobLimit = AppSetting("BatchUpload.Supplier.JobLimit", "10");  // 預設 10

List<long> processIdList = GetBatchUploadProcesseIdList(supplierId);
// → 撈同廠商所有 BatchUpload_StatusDef == 'InProcess' 的 BatchUpload_Id 清單

int currJobCount = processIdList.Count;

if (processIdList.Contains(BatchUploadEntity.BatchUpload_Id) == false
    && currJobCount >= jobLimit)
{
    // 延遲排隊（預設 120 秒後重新 re-queue）
    var delaySeconds = AppSetting("BatchUpload.JobLimit.DelaySeconds", "120");
    DateTime bookingTime = DateTime.Now.AddSeconds(delaySeconds);
    this.CreateNMQTask(taskInfo, bookingTime);
    return;
}
```

### 關鍵條件：`processIdList.Contains(this.BatchUpload_Id) == false`

| 情況 | 結果 |
|------|------|
| 這批自己的 ID **不在** InProcess 清單（第一輪子 Task，BatchUpload 還是 `WaitingToProcess`），且 count ≥ jobLimit | ❌ re-queue，120 秒後再試 |
| 這批自己的 ID **已在** InProcess 清單（第二輪以後的子 Task，BatchUpload 已是 `InProcess`） | ✅ 直接繼續，**不受 Quota 限制** |

---

## 時序圖

```
BatchUpload A（第一輪子 Task 進來，BatchUpload_StatusDef = WaitingToProcess）
  ↓
  processIdList = [B, C, D, ...]（其他廠商批次的 Id）
  A 的 Id 不在 list 裡 → 檢查 count >= 10？
    YES → re-queue +120s → 結束本次 Task
    NO  → UpdateStatus(WaitingToProcess → InProcess)
          → A 的 Id 進入 InProcess list
          → 開始執行第一組子資料

BatchUpload A（第二輪以後子 Task 進來，BatchUpload_StatusDef = InProcess）
  ↓
  processIdList = [A, B, C, D, ...]
  A 的 Id 在 list 裡 → Contains == true
  → 跳過 Quota 判斷，直接執行下一組子資料
```

---

## 從 DB 確認目前 Quota 狀況

### 1. 查某廠商目前佔了幾個 Quota

```sql
USE WebStoreDB

SELECT COUNT(*) AS 目前佔用數
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_SupplierId = @SupplierId
  AND BatchUpload_StatusDef = 'InProcess'
```

### 2. 看是哪幾批在佔著 Quota（程式碼中的 processIdList）

```sql
SELECT BatchUpload_Id,
       BatchUpload_Code,
       BatchUpload_TypeDef,
       BatchUpload_FileName,
       BatchUpload_StartDateTime,
       DATEDIFF(MINUTE, BatchUpload_StartDateTime, GETDATE()) AS 已執行分鐘數
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_SupplierId = @SupplierId
  AND BatchUpload_StatusDef = 'InProcess'
ORDER BY BatchUpload_StartDateTime ASC
```

### 3. 確認是否有新批次被卡在排隊（Quota 不夠導致）

```sql
SELECT BatchUpload_Id,
       BatchUpload_Code,
       BatchUpload_TypeDef,
       BatchUpload_ScheduledDateTime,
       BatchUpload_UploadDateTime
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_SupplierId = @SupplierId
  AND BatchUpload_StatusDef = 'WaitingToProcess'
  AND BatchUpload_ScheduledDateTime IS NULL  -- 非預約批次卡住才是 Quota 問題
ORDER BY BatchUpload_UploadDateTime ASC
```

---

## Quota 鎖死的根本原因

```
正常流程：
  InProcess → 執行完畢 → Finish / FinishWithError → 狀態離開 InProcess → Quota 釋放

鎖死流程：
  InProcess → 子 Task 卡住（NMQ 異常 / Deadlock / 未捕捉例外）
            → 狀態永遠停在 InProcess
            → 新批次每 120 秒 re-queue 一次，永遠進不來
```

**判斷是否鎖死：**
執行上方 SQL 2，看 `已執行分鐘數` 異常大（幾小時以上）的批次，即為佔住 Quota 的根源。

---

## AppSetting 設定值

| Key | 預設值 | 說明 |
|-----|--------|------|
| `BatchUpload.Supplier.JobLimit` | `10` | 同廠商最多同時 InProcess 的 BatchUpload 數 |
| `BatchUpload.JobLimit.DelaySeconds` | `120` | 超過 Quota 時 re-queue 延遲秒數 |
