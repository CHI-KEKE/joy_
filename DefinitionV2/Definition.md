


### 📋 批次作業 Definition 設定

**範例：BatchUpload_TypeDef_BatchModifyPromotionSalePage**

<br>

**專案資訊**：

<br>

- **Epic**：回饋活動_02
- **Project Name**：sms
- **Page Name**：tw
- **功能描述**：批次更新折扣活動商品

<br>

**程式碼實作**：

<br>

```csharp
// 取得 BatchUpload TypeDef 描述
var batchUploadTypeDesc = this._definitionService.GetDefinitions(
    Common.Utility.DefinitionsEf6.Models.DatabaseEnum.WebStore, 
    "BatchUpload", 
    "BatchUpload_TypeDef")
    .Where(d => d.Definition_Code == batchUploadTypeEnum.ToString())
    .Select(d => d.Definition_Desc)
    .FirstOrDefault()
    ?.Replace(' ', '_');
```

<br>

**多語系模組設定**：

<br>

```csharp
// 組合多語系模組名稱
string module = string.Join(".", "backend.definition", tableName); 
// 結果：backend.definition.BatchUpload

// 組合多語系 key
string key2 = string.Join("_", item.Definition_ColumnName, item.Definition_Code);
// Column：BatchUpload_TypeDef
// Code：對應的 type
// 結果：BatchUpload_TypeDef_{對應的type}

// 取得翻譯內容
item.Definition_Desc = _translationClient.GetString(module, key2, item.Definition_Desc);
// Desc：取得資料庫中的中文描述
```

<br>

**設定說明**：

<br>

| 項目 | 值 | 說明 |
|------|-----|------|
| **TableName** | BatchUpload | Definition 表格名稱 |
| **ColumnName** | BatchUpload_TypeDef | Definition 欄位名稱 |
| **Code** | BatchModifyPromotionSalePage | Definition 程式碼 |
| **Module** | backend.definition.BatchUpload | 多語系模組路徑 |
| **Key** | BatchUpload_TypeDef_{Code} | 多語系索引鍵 |
| **Desc** | 批次更新折扣活動商品 | 中文描述（從資料庫取得） |
