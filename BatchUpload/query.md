## BatchUpload

```sql
SELECT top 1 BatchUpload_Code,BatchUpload_StatusDef,*
FROM BatchUpload(nolock)
where BatchUpload_ValidFlag = 1
```

<br>


## 查詢有錯的 Job

```sql
select BatchUpload_ShopId,count(*)
from BatchUpload(nolock)
where BatchUpload_ValidFlag = 1
and BatchUpload_StatusDef = 'FinishWithError'
group by BatchUpload_ShopId
```

<br>

##  BatchUploadMessage

```sql
select *
from BatchUploadMessage(nolock)
where BatchUploadMessage_ValidFlag = 1
and BatchUploadMessage_BatchUploadId = 45518
```

<br>

## BatchUploadData

```sql
select *
from BatchUploadData(nolock)
where BatchUploadData_ValidFlag = 1
and BatchUploadData_BatchUploadId = 45518
```

<br>

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
    Definition_TableName,
    Definition_ColumnName,
    Definition_Code,
    Definition_Desc,
    Definition_Note,
    Definition_Sort,
    Definition_CreatedDateTime,
    Definition_CreatedUser,
    Definition_UpdatedTimes,
    Definition_UpdatedDateTime,
    Definition_UpdatedUser,
    Definition_ValidFlag
)
SELECT TOP 1
    Definition_TableName,
    Definition_ColumnName,
    @batchUploadType,
    @batchUploadTypeDesc,
    Definition_Note,
    Definition_Sort + 10,
    @now,
    @vsts,
    0,
    @now,
    @vsts,
    1
FROM dbo.Definition WITH(NOLOCK)
WHERE Definition_ValidFlag = 1
    AND Definition_TableName = 'BatchUpload'
    AND Definition_ColumnName = 'BatchUpload_TypeDef'
ORDER BY Definition_Sort DESC
  
-- Verify
SELECT TOP 1 *
FROM dbo.Definition WITH(NOLOCK)
WHERE Definition_ValidFlag = 1
    AND Definition_TableName = 'BatchUpload'
    AND Definition_ColumnName = 'BatchUpload_TypeDef'
    AND Definition_Code = @batchUploadType
ORDER BY Definition_Sort DESC
 
use WebStoreDB
-- Verify
SELECT *
FROM dbo.Definition WITH(NOLOCK)
WHERE Definition_ValidFlag = 1
    AND Definition_TableName = 'BatchUpload'
    AND Definition_ColumnName = 'BatchUpload_TypeDef'
    --AND Definition_Code = @batchUploadType
    AND Definition_Code = 'ModifyRewardPromotionSalePage'
ORDER BY Definition_Sort DESC
```