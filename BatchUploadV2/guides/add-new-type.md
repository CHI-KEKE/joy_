# 新增批次類型 SOP

## 總覽

新增一個 BatchUpload 批次類型，需異動以下六個層面：

1. [Enum 定義](#1-enum-定義)
2. [新增權限](#2-新增權限)
3. [DB Definition 新增定義](#3-db-definition-新增定義)
4. [LanguageTool 新增語系資料](#4-languagetool-新增語系資料)
5. [BatchUploadService 調整](#5-batchuploadservice-調整)
6. [UI 調整（TypeScript）](#6-ui-調整typescript)

<br>

---

## 1. Enum 定義

需在以下兩個 Enum 新增對應值：

- `BatchUploadTypeDefEnum`
- `BatchUploadExecuteTaskTypeEnum`

> ⚠️ 注意各欄位的 varchar 長度限制（見 [核心架構 Enum 欄位對照](../核心架構.md#6-enum--typedef-欄位對照)）

<br>

---

## 2. 新增權限

```csharp
BatchUploadService.GetBatchUploadPermissionList()
```

PR 參考：http://bitbucket.org/nineyi/nineyi.sms/pull-requests/35898/overview

<br>

---

## 3. DB Definition 新增定義

Table：`WebStoreDB.dbo.Definition`

- `Definition_TableName` = `BatchUpload`
- `Definition_ColumnName` = `BatchUpload_TypeDef`
- `Definition_Code` 長度上限：**30 字**

**SQL 範本：**

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

<br>

---

## 4. LanguageTool 新增語系資料

- Project：`NineYi.Sms`
- Module：`backend.definition.BatchUpload`

<br>

---

## 5. BatchUploadService 調整

需在以下位置新增對應批次類型：

**新增批次作業類型**

```csharp
BatchUploadExecuteTaskTypeEnum.DeliveryBatchPrintTranBill[Forwarder][DeliveryType]Task
```

**調整 switch**

- `PrintBookingNoteBySearchCondition()`
- `GetBatchUploadExecuteTaskType()`

<br>

---

## 6. UI 調整（TypeScript）

檔案位置：

```bash
NineYi.Sms\WebSite\WebSite\TypeScripts\Modules\BatchUpload\batchUploadList.controller.ts
```

**下載按鈕顯示邏輯**

```typescript
IsShowDownload()
```

**下載檔案副檔名**

```typescript
SendDownloadRequest()
```

<br>

---

## NMQ 語法（新增 BatchUpload NMQ Job）

PR 參考：https://bitbucket.org/nineyi/nineyi.database.operation/pull-requests/21347/overview
