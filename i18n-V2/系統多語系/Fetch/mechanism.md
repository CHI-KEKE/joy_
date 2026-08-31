## 環境差異

┌────────────────────┬───────────────────────────────────────────────┬─────────────────────────────────┐
│                    │ 本機開發                                      │ 正式 / 測試環境                 │
├────────────────────┼───────────────────────────────────────────────┼─────────────────────────────────┤
│ 判斷條件           │ Translation.RootFolder == "Translation:Dev"   │ 其他值                          │
├────────────────────┼───────────────────────────────────────────────┼─────────────────────────────────┤
│ FetchStorage       │ 本機磁碟（FileStorageProvider）               │ Redis（RedisStorageProvider）   │
├────────────────────┼───────────────────────────────────────────────┼─────────────────────────────────┤
│ manifest 位置      │ 磁碟檔案                                      │ Redis Key                       │
├────────────────────┼───────────────────────────────────────────────┼─────────────────────────────────┤
│ 翻譯 JSON 位置     │ 磁碟檔案                                      │ Redis Key                       │
└────────────────────┴───────────────────────────────────────────────┴─────────────────────────────────┘

--------------------------------------------------------------------------------------------------------------------------------

## 完整呼叫鏈

 GET /api/translations/fetch
```bash
 GET /api/translations/fetch
 │
 ├─ [Part A] _translationClient.Fetch()
 │    │
 │    ├─ SemaphoreSlim.WaitAsync()
 │    │   └─ 防止同時多個 Fetch 並發執行，確保同一時間只有一個在跑
 │    │
 │    ├─ Step 1：讀取本機 manifest（ETag 版本紀錄）
 │    │   ├─ 正式  → 從 Redis 讀  key: {RootFolder}:i18n:NineYi.WebStore:i18n.manifest
 │    │   └─ 本機  → 從磁碟讀  path: {FilePath}/i18n/NineYi.WebStore/i18n.manifest
 │    │
 │    ├─ Step 2：查詢 S3 現在有哪些檔案（CheckI18nManifestItem）
 │    │   └─ GET https://{bucket}.s3.amazonaws.com/?list-type=2&prefix=NineYi.WebStore/
 │    │       └─ 解析 XML 回傳，建立 { "NineYi.WebStore/frontend.home/zh-TW.json" → ETag } 字典
 │    │
 │    ├─ Step 3：比對 manifest vs S3，找出需要更新的項目
 │    │   ├─ manifest ETag == S3 ETag  → NoChange，跳過
 │    │   └─ ETag 不同 or null         → 加入待下載清單
 │    │
 │    ├─ Step 4：並行下載有更新的翻譯 JSON（FetchProcess，每個 module+locale 各一個）
 │    │   └─ GET {S3_BASE}/{module}/{locale}.json
 │    │       Header: If-None-Match: {舊 ETag}
 │    │       │
 │    │       ├─ 200 OK   → 反序列化 JSON，寫入 FetchStorage，更新 manifest ETag
 │    │       │              正式：寫 Redis  key: {RootFolder}:i18n:NineYi.WebStore:{module}:{locale}.json
 │    │       │              本機：寫磁碟   path: {FilePath}/i18n/NineYi.WebStore/{module}/{locale}.json
 │    │       ├─ 304      → 無變更，NoChange
 │    │       └─ 其他     → 記錄錯誤，繼續處理其他項目
 │    │
 │    ├─ Step 5：更新 manifest（寫回 FetchStorage）
 │    │   ├─ 正式：寫 Redis  key: {RootFolder}:i18n:NineYi.WebStore:i18n.manifest
 │    │   └─ 本機：寫磁碟   path: {FilePath}/i18n/NineYi.WebStore/i18n.manifest
 │    │
 │    └─ SemaphoreSlim.Release()
 │
 └─ [Part B] CreateBundleFile(locale)  ← 每個 locale 各執行一次
      │
      ├─ 讀取所有 NeedBundle=true 的 module 翻譯
      │   （從 FetchStorage 讀，正式走 Redis，本機走磁碟）
      │
      ├─ 合併所有 module，key 加上 module 前綴
      │   "submit" in "frontend.checkout"  →  "frontend.checkout.submit"
      │
      ├─ 文字轉換
      │   "\n"   → "<br/>"
      │   '"'    → "'"
      │   "{0}"  → "{{val0}}"   （Angular 變數格式）
      │
      └─ 寫出前端大檔（同樣走 FetchStorage）
          正式：Redis  key: {RootFolder}:i18n:NineYi.WebStore:frontend_{locale}.json
          本機：磁碟   path: {FilePath}/i18n/NineYi.WebStore/frontend_{locale}.json
```

--------------------------------------------------------------------------------------------------------------------------------


## Fetch 完成後，Runtime GetString() 的讀取順序

```bash
 GetString(module, key)
 │
 ├─ L1：InMemory Cache（MemoryCache.Default）
 │   ├─ 命中（60 秒內）→ 直接回傳
 │   └─ 未命中 ↓
 │
 ├─ L2：FetchStorage
 │   ├─ 正式 → Redis StringGet
 │   └─ 本機 → 讀磁碟 JSON
 │       ↓ 找到就寫入 InMemory，快取 60 秒
 │
 └─ L3：BackupStorage（Fallback，永遠是本機磁碟）
     └─ FetchStorage 讀失敗時才用
         ↓ 找到就寫入 InMemory，快取 60 秒
```

--------------------------------------------------------------------------------------------------------------------------------

## Redis Key 結構（正式環境）

```bash
 manifest:
 {Translation.RootFolder}:i18n:NineYi.WebStore:i18n.manifest
 
 翻譯 JSON:
 {Translation.RootFolder}:i18n:NineYi.WebStore:{module}:{locale}.json
 
 範例:
 Translation:WebStore:i18n:NineYi.WebStore:frontend.home:zh-TW.json
 Translation:WebStore:i18n:NineYi.WebStore:frontend.checkout:en-US.json
```

--------------------------------------------------------------------------------------------------------------------------------

## 總結

┌─────────────────────────┬────────────────────────────────────────────────┐
│ 設計                    │ 目的                                           │
├─────────────────────────┼────────────────────────────────────────────────┤
│ ETag + manifest         │ 只下載有變動的翻譯，避免無謂流量               │
├─────────────────────────┼────────────────────────────────────────────────┤
│ SemaphoreSlim           │ 防止同時多個 Fetch 競爭寫入                    │
├─────────────────────────┼────────────────────────────────────────────────┤
│ 並行下載                │ 多個 module 同時拉，加速 Fetch                 │
├─────────────────────────┼────────────────────────────────────────────────┤
│ Redis 作為 FetchStorage │ 多台機器共享同一份翻譯資料，不用每台都各自下載 │
├─────────────────────────┼────────────────────────────────────────────────┤
│ BackupStorage（磁碟）   │ Redis 掛掉時的保底，避免翻譯完全失效           │
├─────────────────────────┼────────────────────────────────────────────────┤
│ InMemory 60 秒快取      │ 減少 Redis 讀取次數，高流量下不會每次打 Redis  │
└─────────────────────────┴────────────────────────────────────────────────┘