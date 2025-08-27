# BatchUpload Query 查詢語法

## 目錄
1. [查詢 BatchUpload 進度](#1-查詢-batchupload-進度)
2. [查詢有錯的 Job](#2-查詢有錯的-job)
3. [BatchUploadMessage](#3-batchuploadmessage)
4. [BatchUploadData](#4-batchuploaddata)

<br>

---

## 1. 查詢 BatchUpload 進度

```sql
SELECT top 1 BatchUpload_Code,BatchUpload_StatusDef,*
FROM BatchUpload(nolock)
where BatchUpload_ValidFlag = 1
```

<br>

---

## 2. 查詢有錯的 Job

```sql
select BatchUpload_ShopId,count(*)
from BatchUpload(nolock)
where BatchUpload_ValidFlag = 1
and BatchUpload_StatusDef = 'FinishWithError'
group by BatchUpload_ShopId
```

<br>

---

## 3. BatchUploadMessage

```sql
select *
from BatchUploadMessage(nolock)
where BatchUploadMessage_ValidFlag = 1
and BatchUploadMessage_BatchUploadId = 45518
```

<br>

---

## 4. BatchUploadData

```sql
select *
from BatchUploadData(nolock)
where BatchUploadData_ValidFlag = 1
and BatchUploadData_BatchUploadId = 45518
```

<br>

---