Manifest 就是一張「版本紀錄表」，記錄每個翻譯檔上次下載時的 ETag（S3 的檔案指紋）。

```json
 // i18n.manifest 內容長這樣
 {
   "frontend.home|zh-TW":      { "Module": "frontend.home",     "Locale": "zh-TW", "ETag": "\"abc123\"" },
   "frontend.checkout|zh-TW":  { "Module": "frontend.checkout", "Locale": "zh-TW", "ETag": "\"def456\"" },
   "frontend.home|en-US":      { "Module": "frontend.home",     "Locale": "en-US", "ETag": "\"xyz789\"" }
 }
```

--------------------------------------------------------------------------------------------------------------------------------

## 用途：避免重複下載沒有變動的檔案

每次 /api/translations/fetch 被呼叫時，流程是：

```bash
 manifest 記錄的 ETag        S3 上現在的 ETag        結果
 ────────────────────────    ───────────────────     ──────────────────
 "abc123"              ==    "abc123"            →   跳過，不下載
 "def456"              !=    "def999"            →   重新下載，更新 ETag
 null（第一次）               "xyz789"            →   下載，寫入 ETag
```

--------------------------------------------------------------------------------------------------------------------------------

Manifest 是翻譯檔的 ETag 版本紀錄，讓 Fetch 只下載「有異動」的檔案，其他的直接跳過。

就跟 Git 的 commit hash 概念一樣，只是 ETag 是 S3 給的，用來判斷檔案內容有沒有變。