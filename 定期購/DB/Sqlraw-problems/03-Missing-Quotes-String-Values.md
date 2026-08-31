# 問題 3：SQL 語法錯誤 - 缺少引號

## 錯誤訊息
```
You have an error in your SQL syntax; check the manual that corresponds to your 
MySQL server version for the right syntax to use near ',
'',
'',
'',
'',
AES_ENCRYPT(IFNULL('',''),'58445f13945144e' at line 32
```

## 生成的錯誤 SQL 片段
```sql
...
AES_ENCRYPT(IFNULL('fffffffffffffff',''),'58445f13945144e8b8d7032e746639b6'),
,           ← 多出一個逗號，說明前面的值沒有被正確填入
'',
...
```

## 根本原因
將非 nullable `string` 型別的值直接內嵌到 SQL 中，但**未加引號**。

### 錯誤的做法
```csharp
// RodReceiverCountry 是非 nullable string，但被直接插入
var sql = $@"... {entity.RodReceiverCountry}, ...";
// 生成：... Malaysia, ...  （Malaysia 是無效的 SQL 標識符）

// RodReceiverShop 也是非 nullable string，同樣問題
var sql = $@"... {entity.RodReceiverShop}, ...";
// 生成：...  , ...  （空值導致多出逗號）
```

### 受影響的欄位
從模型定義查出的非 nullable string 欄位：
- `RodReceiverCountry` - 國家編碼（必填）
- `RodReceiverShop` - 配送門市（必填）
- `RodReceiverShopCode` - 門市代碼（nullable，已正確處理）
- `RodReceiverShopAddress` - 門市地址（nullable，已正確處理）
- 其他：`RodTgCode`, `RodTsCode`, `RodCreateUser`, `RodUpdateUser` 等

## 解決方案

### ❌ 舊做法（不要用）
```csharp
var sql = $@"... {entity.RodReceiverCountry}, {entity.RodReceiverShop}, ...";
// 生成：... Malaysia, Home, ...  → SQL 語法錯誤
```

### ✅ 新做法：為所有字符串加引號並進行 SQL 轉義
```csharp
var sql = $@"... 
    '{entity.RodReceiverCountry.Replace("'", "''")}',
    '{entity.RodReceiverShop.Replace("'", "''")}',
    ...
";
// 生成：...
//     'Malaysia',
//     'Home',
//     ...
// → 正確的 SQL
```

### SQL 轉義規則
在 MySQL 中，字符串內的單引號必須加倍：
```csharp
// 輸入：John's Store
// 轉義：John''s Store
// SQL：'John''s Store'
var escaped = value.Replace("'", "''");
var sqlValue = $"'{escaped}'";
```

## 涉及檔案
- `RegularOrderDetailRepository.cs` - `InsertRangeAsync()`
  - L84：`RodReceiverCountry`（第 15 個值）
  - L90：`RodReceiverShop`（第 22 個值）

## 修復策略
1. 識別所有非 nullable 字段：`string RodXxx { get; set; } = null!;`
2. 為這些字段套用引號和轉義：`'{field.Replace("'", "''")}'`
3. 對於 nullable 字段保持原策略：`{(field != null ? $"'{field.Replace("'", "")}'" : "NULL")}`

## 修復後的驗證
Build succeeded ✅
