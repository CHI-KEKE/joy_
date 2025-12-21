使用 LinqToExcel 套件的 ExcelQueryFactory 來執行 Excel 讀取操作

#### 建立 ExcelQueryFactory

<br>

```csharp
// 使用 LinqToExcel 套件建立 excelQueryFactory，並指定要讀取的 fileName
ExcelQueryFactory excelQueryFactory = new ExcelQueryFactory(fileName);

// 設定 TrimSpaces 為 Both，以自動修剪字串前後的空白
Type typeFromHandle = typeof(ExcelQueryFactory);

// typeFromHandle 用於後續透過反射取得方法
excelQueryFactory.TrimSpaces = TrimSpacesType.Both;
```

<br>

#### 取得對應介面與對應定義 (Mapping Definition)


```csharp
Type typeFromHandle2 = typeof(IColumnMapping<>);
Type type = typeFromHandle2.MakeGenericType(dataType);
object obj = LifetimeScope.Resolve(type);
```

IColumnMapping<> 是一個泛型介面，程式透過 MakeGenericType 產生對應 dataType 的型別。使用 LifetimeScope.Resolve(type) 來解析出對應的物件，這通常是 Dependency Injection (DI) 容器的操作。

<br>

#### 建立 MappingDefinition

```csharp
Type typeFromHandle3 = typeof(MappingDefinition<>);
Type type2 = typeFromHandle3.MakeGenericType(dataType);
object obj2 = Activator.CreateInstance(type2);

// 建立出針對 dataType 的 MappingDefinition，用來描述 Excel 欄位與類別屬性的對應關係
```

<br>

#### 執行映射邏輯

```csharp
MethodInfo method = type.GetMethod("MapExcelToEntity");
method.Invoke(obj, new object[1] { obj2 });
```

<br>

#### 取得對應資訊

<br>

```csharp
PropertyInfo property = type2.GetProperty("Mappings");
object value = property.GetValue(obj2);
// 取得 Mappings 屬性：
// Mappings 是 obj2 中的欄位對應資訊，通常是 Dictionary<string, string> 格式，Key 是 Excel 欄位，Value 是 .NET 屬性
```

<br>

#### 建立映射至 ExcelQueryFactory

```csharp
foreach (object item in value as IEnumerable)
{
    Type type3 = item.GetType();
    object value2 = type3.GetProperty("Key").GetValue(item);
    object value3 = type3.GetProperty("Value").GetValue(item);
    
    MethodInfo methodInfo = (from i in typeFromHandle.GetMethods()
                             where i.Name == "AddMapping" && i.GetGenericArguments().Count() == 1 && i.GetParameters().Count() == 2
                             select i).First();
    methodInfo = methodInfo.MakeGenericMethod(dataType);
    methodInfo.Invoke(excelQueryFactory, new object[2] { value2, value3 });
}

// 逐一從 Mappings 中取出每個項目 (Excel 欄位與屬性對應)
// 使用反射找到 AddMapping 方法，並指定 dataType
// 呼叫 AddMapping(value2, value3)：
// value2：Excel 欄位名稱
// value3：.NET 類別的屬性名稱
```

<br>

#### 呼叫 Worksheet 取得資料

<br>

```csharp
MethodInfo methodInfo2 = (from i in typeFromHandle.GetMethods()
                          where i.Name == "Worksheet" && i.GetGenericArguments().Count() == 1 && i.GetParameters().Count() == 1 && i.GetParameters().First().ParameterType == typeof(string)
                          select i).First();
MethodInfo methodInfo3 = methodInfo2.MakeGenericMethod(dataType);
object obj3 = methodInfo3.Invoke(excelQueryFactory, new object[1] { worksheetName });

// 透過反射取得泛型的 Worksheet<T> 方法
// 呼叫 Worksheet<T>(worksheetName)，讀取指定工作表的資料
```

<br>

#### 回傳

<br>

```csharp
return obj3 as IEnumerable<object>;
// 將讀取到的資料轉型為 IEnumerable<object>，以便後續使用
```