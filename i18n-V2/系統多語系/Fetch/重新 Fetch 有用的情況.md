S3 上的翻譯檔內容是對的，但本機/Redis 的版本是舊的或損壞的：

S3 有正確翻譯 → Fetch 拉下來 → 寫入 Redis → InMemory 過期後讀到新值 → 正常