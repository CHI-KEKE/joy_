# Definition 表（批次類型定義）

**DB：** `WebStoreDB`

## 用途

`Definition` 是全系統共用的「代碼定義表」，BatchUpload 使用它來定義批次類型的顯示名稱與排序，作為前台下拉選單的資料來源。

批次相關的 Definition 條件：
- `Definition_TableName = 'BatchUpload'`
- `Definition_ColumnName = 'BatchUpload_TypeDef'`

---

## 關鍵欄位（BatchUpload 相關）

| 欄位 | 說明 |
|------|------|
| `Definition_Code` | 批次類型字串名稱，**必須與 C# `BatchUploadTypeDefEnum` 成員名稱完全一致** |
| `Definition_Desc` | 前台顯示名稱（中文），下拉選單顯示此值 |
| `Definition_Sort` | 排序，值越小越前面 |
| `Definition_TableName` | 固定為 `'BatchUpload'` |
| `Definition_ColumnName` | 固定為 `'BatchUpload_TypeDef'` |
| `Definition_ValidFlag` | 資料有效旗標 |

---

## 常用查詢

```sql
USE WebStoreDB

-- 查詢所有已定義的批次類型（下拉選單資料）
SELECT
    Definition_Code,
    Definition_Desc,
    Definition_Sort
FROM Definition (NOLOCK)
WHERE Definition_ValidFlag = 1
  AND Definition_TableName = 'BatchUpload'
  AND Definition_ColumnName = 'BatchUpload_TypeDef'
ORDER BY Definition_Sort

-- 查詢特定批次類型是否已定義
SELECT *
FROM Definition (NOLOCK)
WHERE Definition_ValidFlag = 1
  AND Definition_TableName = 'BatchUpload'
  AND Definition_ColumnName = 'BatchUpload_TypeDef'
  AND Definition_Code = '{TypeDef}'
```

---

## 新增批次類型 Definition（INSERT 範本）

```sql
USE WebStoreDB

DECLARE @batchUploadType    VARCHAR(50)   = 'BatchCreateBlackList',   -- 與 C# Enum 名稱完全一致
        @batchUploadTypeDesc NVARCHAR(50) = N'批次匯入黑名單',          -- 前台顯示名稱
        @vsts               VARCHAR(20)   = 'VSTS123456',              -- VSTS 編號
        @now                DATETIME      = GETDATE();

-- INSERT（Sort 自動接在目前最大值 + 10）
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

-- 驗證插入結果
SELECT TOP 1 *
FROM dbo.Definition WITH (NOLOCK)
WHERE Definition_ValidFlag = 1
  AND Definition_TableName = 'BatchUpload'
  AND Definition_ColumnName = 'BatchUpload_TypeDef'
  AND Definition_Code = @batchUploadType
ORDER BY Definition_Sort DESC
```

> ⚠️ `Definition_Code` 必須與 C# `BatchUploadTypeDefEnum` 的成員名稱字串完全一致，否則下拉選單比對失敗。
