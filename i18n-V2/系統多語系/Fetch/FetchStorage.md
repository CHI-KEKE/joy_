
Manifest 存在哪裡，取決於 FetchStorageProvider 是哪個：

正式環境  → FetchStorageProvider = RedisStorageProvider  → manifest 在 Redis
本機開發  → FetchStorageProvider = FileStorageProvider   → manifest 在本機磁碟

兩者是互斥的，程式只會選其中一個當作 FetchStorage