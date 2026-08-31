# 問題 5：InvalidCastException - DBNull 無法轉換為 Byte[]

## 錯誤訊息
```
System.InvalidCastException: Unable to cast object of type 'System.DBNull' to type 'System.Byte[]'.
  at System.Data.Common.DbDataReader.GetFieldValue[T](Int32 ordinal)
  at MySqlConnector.MySqlDataReader.GetFieldValue[T](Int32 ordinal) in /_/src/MySqlConnector/MySqlDataReader.cs:line 433
  at lambda_method21(Closure, QueryContext, DbDataReader, Int32[])\
  at Microsoft.EntityFrameworkCore.Query.Internal.FromSqlQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
```

## 根本原因
在 SELECT 查詢中，當敏感欄位的值為 `NULL` 時（未被加密），`AES_DECRYPT(NULL, key)` 仍回傳 `NULL`，導致 MySQL 回傳 `DBNull`。

而 EF Core 無法將 `DBNull` 轉換為非 nullable 的 `byte[]` 型別（Entity 定義敏感欄位為 `byte[]` 而非 `byte[]?`），導致拋出異常。

### 問題場景
```sql
SELECT 
    CAST(AES_DECRYPT(rod_receiver_name, '...') AS BINARY) AS rod_receiver_name
    --   當 rod_receiver_name 本身為 NULL 時
    --   AES_DECRYPT(NULL, ...) 仍回傳 NULL
    --   → MySQL 回傳 DBNull
    --   → EF Core 無法轉換為 byte[]
FROM regular_order_detail
WHERE rod_receiver_name IS NULL  -- 或其他導致該欄位為 NULL 的條件
```

### Entity 定義（期望非 nullable）
```csharp
public byte[] RodReceiverName { get; set; } = null!;
//      ^^^^^ 非 nullable，無法容納 NULL 值
```

## 解決方案

### ❌ 舊做法（不要用）
```sql
CAST(AES_DECRYPT(rod_receiver_name, '...') AS BINARY) AS rod_receiver_name
-- 當 rod_receiver_name 為 NULL 時，結果仍為 NULL，無法轉換
```

### ✅ 新做法：使用 COALESCE 將 NULL 轉為空值
```sql
CAST(COALESCE(AES_DECRYPT(rod_receiver_name, '...'), '') AS BINARY) AS rod_receiver_name
--   ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
--   如果 AES_DECRYPT 回傳 NULL，改為回傳空字符串 ''
--   再 CAST 為 BINARY，保證回傳二進制（不是 DBNull）
```

## 涉及欄位
所有敏感欄位（定義為非 nullable `byte[]`）：
- `RodReceiverName` - 收件人姓名
- `RodReceiverMobile` - 收件人電話
- `RodReceiverAddress` - 收件人地址
- `RodInvoiceEmail` - 發票信箱

## 修復策略

### L168-182 修復前後對比
```diff
- CAST(AES_DECRYPT(rod.rod_receiver_name, '...') AS BINARY) AS rod_receiver_name,
+ CAST(COALESCE(AES_DECRYPT(rod.rod_receiver_name, '...'), '') AS BINARY) AS rod_receiver_name,

- CAST(AES_DECRYPT(rod.rod_receiver_mobile, '...') AS BINARY) AS rod_receiver_mobile,
+ CAST(COALESCE(AES_DECRYPT(rod.rod_receiver_mobile, '...'), '') AS BINARY) AS rod_receiver_mobile,

- CAST(AES_DECRYPT(rod.rod_receiver_address, '...') AS BINARY) AS rod_receiver_address,
+ CAST(COALESCE(AES_DECRYPT(rod.rod_receiver_address, '...'), '') AS BINARY) AS rod_receiver_address,

- CAST(AES_DECRYPT(rod.rod_invoice_email, '...') AS BINARY) AS rod_invoice_email,
+ CAST(COALESCE(AES_DECRYPT(rod.rod_invoice_email, '...'), '') AS BINARY) AS rod_invoice_email,
```

## 流程驗證
1. 若 `rod_receiver_name` 本身為 NULL（未加密）
2. `AES_DECRYPT(NULL, key)` 回傳 NULL
3. `COALESCE(NULL, '')` 回傳 `''`（空字符串）
4. `CAST('' AS BINARY)` 回傳二進制零長度值（而非 DBNull）
5. EF Core 正確接收並映射為空的 `byte[]`
6. C# 層解碼時會得到空字符串 `""`

## 修復後的驗證
Build succeeded ✅

## 相關注意事項
- 該修復在問題 4 中已實施（L168-182）
- COALESCE 也可用 IFNULL 替代，效果相同
- 空 byte[] 在 C# 解碼後變成 `""` 空字符串
