## Npgsql 預設把 DateTime 對應到 timestamptz（帶時區），要求 Kind=Utc

Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone'

原因：Npgsql 預設把 DateTime 對應到 timestamptz（帶時區），要求 Kind=Utc。但系統裡的 DateTime 都是 Kind=Unspecified（對應 MySQL 的 datetime 不帶時區）。

-----------------------------------------------------------

解法有兩種，我先跟你說明再動手：

┌─────────┬─────────────────────────┬──────────────────────┐
│ 方案    │ 做法                    │ 適用性               │
├─────────┼─────────────────────────┼──────────────────────┤
│ A（推薦 │ 開啟                    │ 最簡單，一行解決，讓 │
│ ）      │ Npgsql.EnableLegacyTime │ Npgsql 接受          │
│         │ stampBehavior           │ Kind=Unspecified     │
│         │                         │ ，對應 timestamp     │
│         │                         │ without time zone    │
│         │                         │ ，完全符合原本 MySQL │
│         │                         │ datetime 的語意      │
├─────────┼─────────────────────────┼──────────────────────┤
│ B       │ 全程式碼改              │ 改動幾十個地方，容易 │
│         │ DateTime.SpecifyKind(.. │ 出錯，且語意跟 MySQL │
│         │ ., DateTimeKind.Utc)    │ 不同                 │
├─────────┼─────────────────────────┼──────────────────────┤
│ C       │ DBContext 所有 DateTime │ 也可以，但量多       │
│         │ 欄位加                  │                      │
│         │ .HasColumnType("timesta │                      │
│         │ mp without time zone")  │                      │
└─────────┴─────────────────────────┴──────────────────────┘

方案 A 是 Npgsql 官方建議的「MySQL/舊系統遷移」做法，加在 Program.cs 最前面：

 AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

這樣 Npgsql 就會把 DateTime 當成 timestamp without time zone，Kind=Unspecified 完全合法，行為跟 MySQL datetime 一致。