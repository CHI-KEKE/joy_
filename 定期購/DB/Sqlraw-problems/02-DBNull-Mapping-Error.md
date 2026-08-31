# 問題 2：DBNull 映射錯誤

## 錯誤訊息
```
System.InvalidOperationException: The provider doesn't have a store type mapping 
for properties of type 'DBNull'.
```

## 根本原因
當使用 EF Core 的參數化查詢（如 `FromSqlRaw(sql, @p0, @p1)`）時，若某個參數值為 `null`，EF Core 需要推斷該參數的 SQL 類型。

**問題**：EF Core 無法自動判斷 `null` 對應的欄位類型（例如是 `INT` 還是 `VARCHAR`），導致生成 `DBNull` 類型的參數，而 MySQL 驅動無法處理。

### 具體場景
```csharp
// 傳入 null 值給 nullable 欄位
var parameters = new object?[] {
    entity.RodCreateOrderStatus,  // 可能是 null，型別是 sbyte?
    entity.RodCreateOrderErrorCode, // 可能是 null，型別是 string?
};
var sql = "... {0} ... {1} ...";
_dbContext.Database.FromSqlRaw(sql, parameters);
```

當這些值為 `null` 時，EF Core 無法判斷應該傳送什麼類型的參數。

## 解決方案

### ❌ 舊做法（不要用）
```csharp
var parameters = new object?[] {
    entity.RodCreateOrderStatus,  // null 會導致錯誤
    entity.RodCreateOrderErrorCode, // null 會導致錯誤
};
```

### ✅ 新做法：在 C# 層提前處理 null，轉換為 SQL 字面量
```csharp
// 方式 1：使用三元運算子 + 字符串插值
var sql = $@"
...
{(entity.RodCreateOrderStatus.HasValue ? entity.RodCreateOrderStatus.Value : 0)} AS status,
{(entity.RodCreateOrderErrorCode != null ? $"'{entity.RodCreateOrderErrorCode.Replace("'", "''")}'" : "NULL")} AS error_code,
...
";

// 方式 2：預先構建字符串變數
var statusStr = entity.RodCreateOrderStatus.HasValue 
    ? entity.RodCreateOrderStatus.Value.ToString() 
    : "NULL";
var sql = $"... {statusStr} ...";
```

**關鍵原則**：
- 對於 nullable 值型別（`int?`, `sbyte?`）：用 `.HasValue ? .Value : defaultValue`
- 對於 nullable 引用型別（`string?`）：用 `!= null ? value : "NULL"`
- 在 SQL 層面用 `NULL` 字面量表示 NULL，而非傳參

## 涉及檔案
- `RegularOrderDetailRepository.cs` - `InsertRangeAsync()` (L38-113)
  - `RodCreateOrderStatus` (sbyte?)
  - `RodCreateOrderErrorCode` (string?)

## 修復策略演變
1. ❌ 先試用 `@p0, @p1` 語法 → 仍然失敗（DBNull 問題未解決）
2. ❌ 再試用 `MySqlParameter` → 命名空間衝突（項目用 Pomelo，不是 MySql.Data）
3. ✅ 最終：完全避免參數化，所有值直接內嵌為字符串

## 修復後的驗證
Build succeeded ✅
