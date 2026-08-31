# 問題 1：FormatException in FromSqlRaw

## 錯誤訊息
```
System.FormatException: Input string was not in a correct format. 
Failure to parse near offset 1210. Expected an ASCII digit.
  at System.Text.ValueStringBuilder.AppendFormatHelper(IFormatProvider provider, String format, ReadOnlySpan`1 args)
  at System.String.FormatHelper(IFormatProvider provider, String format, ReadOnlySpan`1 args)
  at Microsoft.EntityFrameworkCore.Query.QuerySqlGenerator.GenerateFromSql(FromSqlExpression fromSqlExpression)
```

## 根本原因
使用 `FromSqlRaw(sql, params)` 時，EF Core 的 `QuerySqlGenerator` 會在傳送給 MySQL 驅動前進行 **C# String.Format()** 格式化。

位置占位符 `{0}`, `{1}` 被解釋為 C# 格式化指令。當 SQL 字符串中包含複雜表達式（如嵌套的 `IFNULL()` 或 `AES_ENCRYPT()`）時，會有大量 `{}` 被誤認為格式化佔位符，導致格式化失敗。

### 錯誤發生的位置
- offset 1210：SQL 字符串中接近 `AES_ENCRYPT` 函數的位置
- 具體例子：
  ```sql
  AES_ENCRYPT(IFNULL({0},''),'{dbSalt}')
  ```
  當 `{dbSalt}` 被解釋為占位符時，格式化器無法找到對應的參數，導致失敗

## 解決方案

### ❌ 舊做法（不要用）
```csharp
var sql = "... AES_ENCRYPT(IFNULL({0},''),'{dbSalt}') ...";
_dbContext.Database.FromSqlRaw(sql, receiverName);
```

### ✅ 新做法：改用 ExecuteSqlRawAsync + 字符串插值
```csharp
var sql = $@"
... 
AES_ENCRYPT(IFNULL('{receiverName.Replace("'", "''")}',''),'{dbSalt}')
...
";
await _dbContext.Database.ExecuteSqlRawAsync(sql, cancellationToken);
```

**區別**：
- `FromSqlRaw()` 是 DbSet 層 API，會經過 `QuerySqlGenerator` 進行格式化
- `ExecuteSqlRawAsync()` 是 Database 層 API，跳過格式化，直接將字符串傳給 MySQL 驅動

## 涉及檔案
- `RegularOrderDetailRepository.cs` - `InsertRangeAsync()` 和 `GetDetailsForApiCallAsync()`
- `RegularOrderRepository.cs` - `InsertAsync()`
- `MemberInfoRepository.cs` - `InsertAsync()`

## 相關技術細節
EF Core 的 SQL 處理流程：
1. `FromSqlRaw()` → 調用 `QuerySqlGenerator.GenerateFromSql()`
2. `QuerySqlGenerator` 執行 `String.Format(sql, parameters)` 
3. 當 SQL 中有 `{}` 時，嘗試將其解釋為格式化指令
4. 若 `{}` 內容不是有效的參數索引，拋出 `FormatException`

## 修復後的驗證
Build succeeded ✅
