Npgsql 無法辨識 sbyte? 型別（MySQL tinyint → C# sbyte，但 PostgreSQL 不支援 sbyte）




為什麼 sbyte 會造成這個錯誤？

 Npgsql.NpgsqlParameter.ResolveTypeInfo → 無法解析型別

Npgsql（PostgreSQL 的 .NET driver）不支援 sbyte 作為參數型別。它的支援清單裡有 bool、short、int、long，但沒有 sbyte。

-----------------------------------------------------------

其他那些修改（HasColumnType("tinyint")、HasDefaultValueSql("'0'")）是不是必要的？

必要，但不是這次 runtime 錯誤的直接原因：

┌─────────────────────────────┬────────────────────────────┐
│ 修改項目                    │ 影響                       │
├─────────────────────────────┼────────────────────────────┤
│ sbyte? → short?             │ ✅ 直接修復本次 runtime    │
│                             │ 錯誤                       │
├─────────────────────────────┼────────────────────────────┤
│ HasColumnType("tinyint")    │ 影響 Migration / Schema    │
│ 移除                        │ 生成，若不移除在 dotnet ef │
│                             │ migrations add 或 schema   │
│                             │ validate 時會出錯          │
├─────────────────────────────┼────────────────────────────┤
│ HasDefaultValueSql("'0'") → │ 影響 DDL 語法，'0'         │
│ "0"                         │ 是字串，PostgreSQL 的 int  │
│                             │ 欄位 default 要寫 0        │
└─────────────────────────────┴────────────────────────────┘