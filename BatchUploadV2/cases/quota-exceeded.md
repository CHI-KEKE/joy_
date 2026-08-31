# 案例：廠商批次 Job 超過上限，作業卡在待執行

## 案例資訊

| 項目 | 值 |
|------|-----|
| 反映者 | Nancy |
| 票號 | #10230 |
| 批次 Code | `BA2503191400002` |
| BatchUploadId | 45518 |
| SupplierId | 1111 |
| TypeDef | `BatchCrmMemberImport` |
| FileName | `有品編的會員.xlsx` |

<br>

---

## 症狀

匯入其他交易資料後，作業一直停在「待執行」，未進入處理。

<br>

---

## 根本原因

廠商（SupplierId = 1111）同時有 **10 筆** BatchUpload 在 `InProcess` 狀態，已達系統上限（`BatchUpload.Supplier.JobLimit` 預設 10），新作業無法取得執行資格，被系統重新排隊（delay 120 秒），但因為前面 10 筆一直未完成，導致無限排隊。

**NMQ Log 關鍵訊息：**

```bash
目前 job 數 10, 最多執行 job 限制 10
目前 job 數 10, 超過最多執行 job 限制 10, 發動 Task 重新排隊
```

<br>

---

## 排查步驟

### Step 1. 確認 BatchUpload 基本資訊

```sql
USE WebStoreDB

SELECT BatchUpload_Id, BatchUpload_SupplierId, BatchUpload_TypeDef,
       BatchUpload_FileName, BatchUpload_StatusDef
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_Id = 45518
```

### Step 2. 確認該廠商卡住的作業

```sql
USE WebStoreDB

SELECT *
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_SupplierId = 1111
  AND BatchUpload_StatusDef = 'InProcess'
```

### Step 3. 查 NMQ 是否有排隊中的 Task

```sql
USE NMQV2DB

-- BatchCrmMemberImportTask ==> JobId = 290
SELECT *
FROM Task (NOLOCK)
WHERE Task_ValidFlag = 1
  AND Task_Data LIKE '%45518%'
ORDER BY Task_CreatedDatetime DESC
```

> 查不到或查太慢時，改用 JobId 或 Datetime 縮小範圍。

### Step 4. 確認 Log

在 NMQ Log 中搜尋：

```bash
目前 job 數 10, 最多執行 job 限制 10
```

<br>

---

## 解法

### 方法 A：壓掉卡住的舊作業（手動）

```sql
USE WebStoreDB

-- 先確認有哪些
SELECT *
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_SupplierId = 1111
  AND BatchUpload_StatusDef = 'InProcess'

-- 壓掉特定一筆
UPDATE BatchUpload
SET BatchUpload_ValidFlag = 0
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_SupplierId = 1111
  AND BatchUpload_StatusDef = 'InProcess'
  AND BatchUpload_Id = 44856

-- 驗證
SELECT *
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_SupplierId = 1111
  AND BatchUpload_StatusDef = 'InProcess'
```

### 方法 B：使用 ERP 小工具釋放上限

```bash
https://erp.qa.91dev.tw/v2/DynamicReport/List?report=BatchUploadRepository-QueryOrUpdateBatchUpload
```

<br>

---

## 相關查詢

```sql
USE WebStoreDB

-- 查訊息
SELECT *
FROM BatchUploadMessage (NOLOCK)
WHERE BatchUploadMessage_ValidFlag = 1
  AND BatchUploadMessage_BatchUploadId = 45518

-- 查子資料
SELECT *
FROM BatchUploadData (NOLOCK)
WHERE BatchUploadData_ValidFlag = 1
  AND BatchUploadData_BatchUploadId = 45518
```
