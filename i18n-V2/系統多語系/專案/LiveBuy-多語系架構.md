## LiveBuy WebAPI 多語系架構

### 使用套件
`Nine1.Translation.Client` v1.1.0

### 資料來源
翻譯資料存放在 **AWS S3**，App 啟動時一次性下載到記憶體。

```
S3 bucket: 91dev-ap-northeast-1-translation-service
資料夾:    Nine1.Livebuy/
ServiceBaseUrl:   https://s3-ap-northeast-1.amazonaws.com/.../Nine1.Livebuy
S3RestApiBaseUrl: https://91dev-ap-northeast-1-translation-service.s3.amazonaws.com
```

### 設定檔 (_N1CONFIG:TranslationSetting)

| Key | 說明 |
|---|---|
| `ServiceBaseUrl` | S3 翻譯資料根路徑 (Nine1.Livebuy) |
| `S3RestApiBaseUrl` | S3 REST API base |
| `SmsS3BaseUrl` | SMS 系統模組翻譯路徑 (NineYi.Sms) |
| `DefaultLocale` | 預設語系，例如 `zh-HK` |
| `Locales` | 允許的語系清單 |
| `ProjectName` | `Nine1.Livebuy` |
| `Modules` | 翻譯模組清單（見下方） |

### 支援語系
`en-US` / `zh-TW` / `zh-HK` / `ms-MY` / `zh-CN` / `ja-JP` / `th-TH`

### Modules 清單
```
backend.general
frontend.common
frontend.live_events
frontend.stream_settings
```

共 4 modules × 7 locales = **28 個 JSON 檔**，啟動時從 S3 全部下載。

### 啟動流程
```
Program.cs
  ├─ builder.Services.RegisterTranslationsClient(configuration)   ← DI 註冊
  └─ app.Services.RegisterTranslationsClient()                    ← 觸發 Fetch，下載 S3
```