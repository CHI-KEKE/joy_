## 新增 Key 要改這裡

```bash
C:\91APP\SCMAPIV2\nineyi.scm.apiv2\CrossLayer\Translations\i18n\NineYi.Scm.ApiV2\{module}\{locale}.json
```

## 本機 File Cache**（`FileStorageProvider` 讀取位置）

```bash
C:\Files\i18n\NineYi.Scm.ApiV2\{module}\{locale}.json
```

## S3

```bash
https://s3-ap-northeast-1.amazonaws.com/91dev-ap-northeast-1-translation-service/NineYi.Scm.ApiV2/{locale}/{module}.json
```


## 問題：新增 Key 後顯示 Key 值而非翻譯


### 原因

TranslationClient **不直接讀 Repo 的 i18n JSON**，而是讀 File Cache（本機）或 Redis（QA）。

- **本機**：`C:\Files\i18n\NineYi.Scm.ApiV2\` 若不存在或版本舊，就 Fetch S3
- **S3 沒有新 Key** → Fetch 回來也沒有 → 回傳 Key 值本身

### 本機 Fix

把 Repo 的 i18n 複製到本機 File Cache 路徑：

```powershell
Copy-Item -Path "C:\91APP\SCMAPIV2\nineyi.scm.apiv2\CrossLayer\Translations\i18n\NineYi.Scm.ApiV2\*" `
          -Destination "C:\Files\i18n\NineYi.Scm.ApiV2\" `
          -Recurse -Force
```

複製後**重啟 IIS / IIS Express**，讓 TranslationClient 重新載入快取。

### QA Fix

需將更新後的 JSON 上傳到 S3，再讓 Redis Cache 失效。
通常透過 **Jenkins / CI 部署 pipeline** 中的 translation deploy step 執行。