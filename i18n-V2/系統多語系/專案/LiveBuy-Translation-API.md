## LiveBuy Translation API

### Controller：`/api/translations`

| Method | 路徑 | Auth | 說明 |
|---|---|---|---|
| GET | `/current-locale` | 需登入 | 回傳當前 thread 的語系代碼 |
| GET | `/language-setting` | 需登入 | 回傳可用語系清單 + 是否自動偵測 |
| GET | `/client-module/{module}/{locale}` | 需登入 | 單一 module 翻譯 key-value |
| POST | `/client-modules/{locale}` | **AllowAnonymous** | 多 module 翻譯 key-value |

### 呼叫鏈
```
TranslationController
  └─ TranslationService
       └─ ITranslationsClient (Nine1.Translation.Client)
            ├─ GetManifest()         → 讀記憶體 manifest
            └─ GetTranslation(m, l)  → 讀記憶體 key-value
```

### 翻譯值輸出轉換（TransformOutputText）

| 原始 | 轉換後 |
|---|---|
| `\r\n` / `\n` | `<br/>` |
| `"` | `'` |
| `{0}`, `{1}` | `{{val0}}`, `{{val1}}` (Angular 格式) |

### 回傳格式
Key 格式為 `{module}.{key}`，例如：
```json
{
  "backend.general.live_session_should_not_be_empty": "直播序號不可為空"
}
```

### 特例：SMS 系統模組翻譯
`GetSystemModuleNamesAsync` / `GetSystemFunctionNamesAsync`
- 來源：S3 (`SmsS3BaseUrl/backend.database.system_module/{locale}.json`)
- 快取：MemoryCache 60 分鐘
- 下載失敗不噴錯，回傳空 dict（不中斷主流程）