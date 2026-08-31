
---

## NMQ Job ID 對照表

| 環境 | `BatchUploadProcess`（主 NMQ） | `BatchUploadTask`（標準子 Task） |
|------|-------------------------------|--------------------------------|
| TW_PROD | 49 | 50 |
| TW_QA   | 48 | 51 |
| HK_PROD | 49 | 50 |
| HK_QA   | 49 | 50 |
| MY_PROD | 49 | 50 |
| MY_QA   | 49 | 50 |


## 常用查詢

```sql
USE NMQV2DB

-- 查詢特定 BatchUpload 相關的所有 Task（含主 NMQ 與子 Task）
SELECT *
FROM Task (NOLOCK)
WHERE Task_ValidFlag = 1
  AND Task_Data LIKE '%{BatchUpload_Id}%'
ORDER BY Task_CreatedDatetime DESC

-- 查詢特定 Job 的最近 Task（用 JobId，速度較快）
SELECT TOP 20 *
FROM Task (NOLOCK)
WHERE Task_ValidFlag = 1
  AND Task_JobId = {JobId}
ORDER BY Task_CreatedDatetime DESC

-- 查詢目前 Pending 中的 BatchUpload Task（主 NMQ）
SELECT *
FROM Task (NOLOCK)
WHERE Task_ValidFlag = 1
  AND Task_JobId = 49   -- BatchUploadProcess
  AND Task_StatusDef = 'Pending'
ORDER BY Task_ScheduledDatetime

-- 查詢所有 Job 清單（補充未知 JobId）
USE NMQV2DB
SELECT Job_Id, Job_Name, Job_Description
FROM Job (NOLOCK)
WHERE Job_ValidFlag = 1
  AND Job_Name LIKE '%BatchUpload%'
ORDER BY Job_Name

-- 查詢特定 Task 的 Data 內容（找 BatchUploadId）
SELECT Task_Id, Task_JobId, Task_StatusDef, Task_Data, Task_CreatedDatetime
FROM Task (NOLOCK)
WHERE Task_ValidFlag = 1
  AND Task_Id = {Task_Id}
```
