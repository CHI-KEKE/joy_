# LoadExcel — LinqToExcel 套件技術細節

BatchUpload 使用 `LinqToExcel` 套件的 `ExcelQueryFactory` 讀取 Excel 檔案。

<br>

---

## 完整流程

### Step 1. 建立 ExcelQueryFactory

```csharp
ExcelQueryFactory excelQueryFactory = new ExcelQueryFactory(fileName);
excelQueryFactory.TrimSpaces = TrimSpacesType.Both;  // 自動修剪字串前後空白
```

<br>

### Step 2. 取得對應介面與 Mapping Definition

```csharp
Type typeFromHandle2 = typeof(IColumnMapping<>);
Type type = typeFromHandle2.MakeGenericType(dataType);
object obj = LifetimeScope.Resolve(type);  // 透過 DI 解析對應物件
```

`IColumnMapping<>` 是泛型介面，程式透過 `MakeGenericType` 產生對應 `dataType` 的型別，再由 DI 容器解析。

<br>

### Step 3. 建立 MappingDefinition

```csharp
Type typeFromHandle3 = typeof(MappingDefinition<>);
Type type2 = typeFromHandle3.MakeGenericType(dataType);
object obj2 = Activator.CreateInstance(type2);
// 建立針對 dataType 的 MappingDefinition，描述 Excel 欄位與類別屬性的對應關係
```

<br>

### Step 4. 執行映射邏輯

```csharp
MethodInfo method = type.GetMethod("MapExcelToEntity");
method.Invoke(obj, new object[1] { obj2 });
```

<br>

### Step 5. 取得對應資訊

```csharp
PropertyInfo property = type2.GetProperty("Mappings");
object value = property.GetValue(obj2);
// Mappings 通常為 Dictionary<string, string>
// Key = Excel 欄位名稱，Value = .NET 屬性名稱
```

<br>

### Step 6. 建立映射至 ExcelQueryFactory

```csharp
foreach (object item in value as IEnumerable)
{
    Type type3 = item.GetType();
    object value2 = type3.GetProperty("Key").GetValue(item);   // Excel 欄位名稱
    object value3 = type3.GetProperty("Value").GetValue(item); // .NET 屬性名稱

    MethodInfo methodInfo = (from i in typeFromHandle.GetMethods()
                             where i.Name == "AddMapping"
                               && i.GetGenericArguments().Count() == 1
                               && i.GetParameters().Count() == 2
                             select i).First();
    methodInfo = methodInfo.MakeGenericMethod(dataType);
    methodInfo.Invoke(excelQueryFactory, new object[2] { value2, value3 });
}
```

<br>

### Step 7. 呼叫 Worksheet 取得資料

```csharp
MethodInfo methodInfo2 = (from i in typeFromHandle.GetMethods()
                          where i.Name == "Worksheet"
                            && i.GetGenericArguments().Count() == 1
                            && i.GetParameters().Count() == 1
                            && i.GetParameters().First().ParameterType == typeof(string)
                          select i).First();
MethodInfo methodInfo3 = methodInfo2.MakeGenericMethod(dataType);
object obj3 = methodInfo3.Invoke(excelQueryFactory, new object[1] { worksheetName });
// 讀取指定工作表（worksheetName）的資料
```

<br>

### Step 8. 回傳

```csharp
return obj3 as IEnumerable<object>;
```

<br>

---

## appSetting

| Key | 預設值 | 說明 |
|-----|--------|------|
| `BatchUpload.Storage.Floder` | `C:\Temp\UploadData` | Excel 檔案暫存根目錄 |

**實際路徑規則：**

```bash
{BatchUpload.Storage.Floder} / {supplierId} / {BatchUpload_UploadGuid}
```
