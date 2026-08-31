# 常用 SQL 查詢

## 目錄

- [查詢有錯誤的作業](#查詢有錯誤的作業)
- [BatchUpload 基本查詢](#batchupload-基本查詢)
- [BatchUploadMessage 查詢](#batchuploadmessage-查詢)
- [BatchUploadData 查詢](#batchuploaddata-查詢)
- [NMQ Task 查詢](#nmq-task-查詢)
- [插入新 BatchUpload Type Definition](#插入新-batchupload-type-definition)

<br>

---

## NMQ Jobs 清單

| Job 名稱 | JobId | NMQ Path |
|----------|-------|----------|
| `BatchUploadProcess`（主流程） | 49 | `NineYi.SCM.Frontend.NMQV2.BatchUpload.BatchUploadProcess` |
| `BatchModifyPromotionSalePageTask` | 334 | `NineYi.SCM.Frontend.NMQV2.BatchUpload.BatchModifyPromotionSalePageTaskProcess` |
| `BatchModifyPromotionOuterIdTask` | 486 | — |
| `ExportRewardPromotionSalePageTask` | 149 | — |
| `ExportRewardPromotionOuterIdTask` | 487 | — |
| `BatchCrmOrderImportTask` | — | — |
| `BatchCrmMemberImportTask` | 290 | — |

<br>

---

## 查詢有錯誤的作業

```sql
USE WebStoreDB

SELECT BatchUpload_ShopId, COUNT(*) AS ErrorCount
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_StatusDef = 'FinishWithError'
GROUP BY BatchUpload_ShopId
ORDER BY ErrorCount DESC
```

<br>

---

## BatchUpload 基本查詢

```sql
USE WebStoreDB

-- 查詢特定作業
SELECT *
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_Id = 45518

-- 查詢廠商所有 InProcess 作業
SELECT *
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_SupplierId = 1111
  AND BatchUpload_StatusDef = 'InProcess'
```

<br>

---

## BatchUploadMessage 查詢

```sql
USE WebStoreDB

SELECT *
FROM BatchUploadMessage (NOLOCK)
WHERE BatchUploadMessage_ValidFlag = 1
  AND BatchUploadMessage_BatchUploadId = 45518
```

<br>

---

## BatchUploadData 查詢

```sql
USE WebStoreDB

SELECT *
FROM BatchUploadData (NOLOCK)
WHERE BatchUploadData_ValidFlag = 1
  AND BatchUploadData_BatchUploadId = 45518

-- 查詢失敗的子資料
SELECT *
FROM BatchUploadData (NOLOCK)
WHERE BatchUploadData_ValidFlag = 1
  AND BatchUploadData_BatchUploadId = 45518
  AND BatchUploadData_StatusDef = 'ProcessFailed'
```

<br>

---

## NMQ Task 查詢

```sql
USE NMQV2DB

-- 查詢特定 BatchUpload 相關 Task
SELECT *
FROM Task (NOLOCK)
WHERE Task_ValidFlag = 1
  AND Task_Data LIKE '%45518%'
ORDER BY Task_CreatedDatetime DESC

-- 查詢特定 Job 的 Task（速度較快）
SELECT *
FROM Task (NOLOCK)
WHERE Task_ValidFlag = 1
  AND Task_JobId = 334   -- 換成對應 JobId
ORDER BY Task_CreatedDatetime DESC

-- 查詢批次異動商品頁子 Task 狀態（JobId = 334）
USE NMQV2DB
SELECT *
FROM Task (NOLOCK)
WHERE Task_ValidFlag = 1
  AND Task_JobId = 334
ORDER BY Task_CreatedDatetime DESC
```

<br>

---

## 插入新 BatchUpload Type Definition

```sql
USE WebStoreDB;

DECLARE @batchUploadType VARCHAR(50) = 'BatchModifyPromotionOuterId',
        @batchUploadTypeDesc NVARCHAR(50) = N'批次更新給點活動料號',
        @vsts VARCHAR(20) = 'VSTS466779',
        @now DATETIME = GETDATE();

-- Insert
INSERT INTO dbo.Definition
(
    Definition_TableName, Definition_ColumnName, Definition_Code,
    Definition_Desc, Definition_Note, Definition_Sort,
    Definition_CreatedDateTime, Definition_CreatedUser,
    Definition_UpdatedTimes, Definition_UpdatedDateTime,
    Definition_UpdatedUser, Definition_ValidFlag
)
SELECT TOP 1
    Definition_TableName, Definition_ColumnName,
    @batchUploadType, @batchUploadTypeDesc, Definition_Note,
    Definition_Sort + 10, @now, @vsts, 0, @now, @vsts, 1
FROM dbo.Definition WITH (NOLOCK)
WHERE Definition_ValidFlag = 1
  AND Definition_TableName = 'BatchUpload'
  AND Definition_ColumnName = 'BatchUpload_TypeDef'
ORDER BY Definition_Sort DESC

-- Verify
SELECT TOP 1 *
FROM dbo.Definition WITH (NOLOCK)
WHERE Definition_ValidFlag = 1
  AND Definition_TableName = 'BatchUpload'
  AND Definition_ColumnName = 'BatchUpload_TypeDef'
  AND Definition_Code = @batchUploadType
ORDER BY Definition_Sort DESC
```
