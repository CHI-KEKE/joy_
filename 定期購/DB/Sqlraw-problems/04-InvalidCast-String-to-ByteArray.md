# 問題 4：InvalidCastException - String 無法轉換為 Byte[]

## 錯誤訊息
```
System.InvalidCastException: Unable to cast object of type 'System.String' to type 'System.Byte[]'.
  at System.Data.Common.DbDataReader.GetFieldValue[T](Int32 ordinal)
  at MySqlConnector.MySqlDataReader.GetFieldValue[T](Int32 ordinal) in /_/src/MySqlConnector/MySqlDataReader.cs:line 433
  at lambda_method21(Closure, QueryContext, DbDataReader, Int32[])\
  at Microsoft.EntityFrameworkCore.Query.Internal.FromSqlQueryingEnumerable`1.AsyncEnumerator.MoveNextAsync()
```

## 根本原因
在 SELECT 查詢中，敏感欄位被 `AES_DECRYPT()` 解密後，使用了錯誤的 SQL CAST 類型導致 MySQL 回傳**字符串**，而 EF Core Entity 的敏感欄位定義為 `byte[]`，導致型別不符。

### 錯誤的 SQL
```sql
SELECT 
    CAST(AES_DECRYPT(rod_receiver_name, '58445f13...') AS CHAR) AS rod_receiver_name,
    --                                                   ^^^^ 
    --                                            CHAR → MySQL 回傳 string
    ...
FROM regular_order_detail
```

### Entity 定義（期望 byte[]）
```csharp
public byte[]? RodReceiverName { get; set; }
//      ^^^^^
//      期望二進制，但 MySQL 回傳了 string
```

### 發生位置
`RegularOrderDetailRepository.GetDetailsForApiCallAsync()` 執行於 `RegularCreateOrderJob.EstimateOrderCountAsync()` (Step①)

## 解決方案

### ❌ 舊做法（不要用）
```sql
-- CAST AS CHAR 會導致 MySQL 回傳字符串
CAST(AES_DECRYPT(rod_receiver_name, '...') AS CHAR) AS rod_receiver_name
```

### ✅ 新做法：使用 CAST AS BINARY 確保回傳二進制
```sql
CAST(AES_DECRYPT(rod_receiver_name, '...') AS BINARY) AS rod_receiver_name
--                                         ^^^^^^
--                                    BINARY → MySQL 回傳 varbinary
```

## 涉及欄位
所有敏感欄位（定義為 `byte[]?`）：
- `RodReceiverName` - 收件人姓名
- `RodReceiverMobile` - 收件人電話
- `RodReceiverAddress` - 收件人地址
- `RodInvoiceEmail` - 發票信箱

## 修復策略

### L168-182 修復前後對比
```diff
- CAST(AES_DECRYPT(rod.rod_receiver_name, '...') AS CHAR) AS rod_receiver_name,
+ CAST(AES_DECRYPT(rod.rod_receiver_name, '...') AS BINARY) AS rod_receiver_name,

- CAST(AES_DECRYPT(rod.rod_receiver_mobile, '...') AS CHAR) AS rod_receiver_mobile,
+ CAST(AES_DECRYPT(rod.rod_receiver_mobile, '...') AS BINARY) AS rod_receiver_mobile,

- CAST(AES_DECRYPT(rod.rod_receiver_address, '...') AS CHAR) AS rod_receiver_address,
+ CAST(AES_DECRYPT(rod.rod_receiver_address, '...') AS BINARY) AS rod_receiver_address,

- CAST(AES_DECRYPT(rod.rod_invoice_email, '...') AS CHAR) AS rod_invoice_email,
+ CAST(AES_DECRYPT(rod.rod_invoice_email, '...') AS BINARY) AS rod_invoice_email,
```

## 流程驗證
1. MySQL 執行 `AES_DECRYPT(encrypted_blob, key)` 回傳 `BLOB/BINARY`
2. `CAST(... AS BINARY)` 保證結果為二進制
3. EF Core 接收二進制數據，正確映射到 `byte[]` 屬性
4. 後續在 C# 層用 `Encoding.UTF8.GetString(bytes)` 解碼為字符串

## 修復後的驗證
Build succeeded ✅
