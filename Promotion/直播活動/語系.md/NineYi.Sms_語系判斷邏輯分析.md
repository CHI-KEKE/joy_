# NineYi.Sms 語系判斷邏輯完整分析

> **分析日期**：2026-03-10  
> **專案**：NineYi.Sms  
> **說明**：本文件完整記錄 NineYi.Sms 後台系統顯示語系的判斷流程、依據與相關設定。  
> **最後更新**：補充 `Web.config <globalization>` 為瀏覽器語系讀取的實際來源。

---

## 目錄

1. [整體架構概覽](#整體架構概覽)
2. [根源：Web.config globalization 設定](#根源webconfigglobalization-設定)
3. [第一層：LocaleFilter（每個請求皆執行）](#第一層localefilter每個請求皆執行)
4. [第二層：使用者個人語系偏好 Filter（全域 Action Filter）](#第二層使用者個人語系偏好-filter全域-action-filter)
5. [第三層：RestrictLocaleAttribute（特定功能限定）](#第三層restrictlocaleattribute特定功能限定)
6. [優先順序總結](#優先順序總結)
7. [關鍵 AppSetting 設定](#關鍵-appsetting-設定)
8. [支援的語系清單](#支援的語系清單)
9. [相關檔案位置總覽](#相關檔案位置總覽)
10. [語系資料儲存與快取](#語系資料儲存與快取)

---

## 整體架構概覽

NineYi.Sms 語系決策採用**四階段流程**，以 `Web.config` 的 `<globalization>` 設定為根源，再依序經過三層覆蓋：

```
HTTP Request 進來
      │
      ▼
┌─────────────────────────────────────────────────────┐
│  【根源】Web.config: <globalization uiCulture="auto:zh-TW" />  │
│  ASP.NET Framework 自動讀取 Accept-Language Header   │
│  → 初始化 CultureInfo.CurrentUICulture              │
│  → 若 Header 不存在，預設 zh-TW                      │
└─────────────────────────────────────────────────────┘
      │
      ▼
┌─────────────────────────────────────────────────┐
│  第一層：Application_BeginRequest → LocaleFilter │  ← 每個 Request 必執行
│  來源：讀取上一步的 CurrentUICulture              │
│  作用：對應到 Translation.Locales 支援清單        │
└─────────────────────────────────────────────────┘
      │
      ▼
┌─────────────────────────────────────────────────┐
│  第二層：GetUserPreferLangAttribute（全域 Filter）│  ← 可覆蓋第一層
│  來源：AWS DynamoDB 使用者個人設定               │
│  條件：商店有啟用此功能 & 非前端設定 Accept-Language │
└─────────────────────────────────────────────────┘
      │
      ▼
┌─────────────────────────────────────────────────┐
│  第三層：RestrictLocaleAttribute（特定 Action）   │  ← 可進一步限制
│  來源：特定 AppSetting（功能專屬語系設定）          │
│  條件：Controller Action 有標記此 Attribute       │
└─────────────────────────────────────────────────┘
      │
      ▼
  最終語系確定 → 載入對應 .resx 資源檔 → 頁面顯示
```

---

## 根源：Web.config `<globalization>` 設定

### 這才是讀取瀏覽器語系的真正位置

**檔案位置**：`WebSite/WebSite/Web.config` 第 36 行

```xml
<globalization culture="auto:zh-TW" uiCulture="auto:zh-TW" />
```

### `auto:zh-TW` 語法說明

| 部分 | 說明 |
|------|------|
| `auto` | 讓 ASP.NET Framework **自動**讀取每個 Request 的 `Accept-Language` HTTP Header，並設定 `Thread.CurrentThread.CurrentUICulture` |
| `:zh-TW` | **Fallback**：若瀏覽器未送 Accept-Language Header 或無法解析，預設使用 `zh-TW` |

### 運作機制

這是 **ASP.NET Framework 內建功能**，不需要自己寫任何程式碼去讀取 Header。ASP.NET 在處理每個 Request 時，會在 Pipeline 的早期階段自動完成以下動作：

```
瀏覽器送出 Request
   └─ HTTP Header: Accept-Language: zh-TW,en;q=0.9,en-US;q=0.8

ASP.NET 接收 Request（globalization auto 設定生效）
   └─ Thread.CurrentThread.CurrentCulture   = CultureInfo("zh-TW")
   └─ Thread.CurrentThread.CurrentUICulture = CultureInfo("zh-TW")
                                              ↑ 這就是 LocaleFilter() 讀到的值
```

> **重點**：`LocaleFilter()` 中的 `CultureInfo.CurrentUICulture.Name` 之所以能反映瀏覽器語系，完全依賴這一行 `Web.config` 設定，而非任何自寫的程式碼。

### 各情境下的初始化結果

| 瀏覽器 Accept-Language | globalization 初始化結果 |
|------------------------|--------------------------|
| `zh-TW` | `CultureInfo("zh-TW")` |
| `en-US,en;q=0.9` | `CultureInfo("en-US")` |
| `zh-HK` | `CultureInfo("zh-HK")` |
| （無 Header） | `CultureInfo("zh-TW")`（使用 fallback `auto:zh-TW`） |

---

## 第一層：LocaleFilter（每個請求皆執行）

### 觸發時機

`Application_BeginRequest`，每一個 HTTP Request 進來時必定執行。

### 檔案位置

```
WebSite/WebSite/Global.asax.cs → LocaleFilter() 方法
```

### 執行邏輯

```csharp
private void LocaleFilter()
{
    var configService = new NineYi.Common.Utility.Config.ConfigService();

    // 1. 讀取市場支援語系清單（AppSetting）
    var rawAvailableLanguages = configService.GetAppSetting("Translation.Locales") ?? string.Empty;
    // 2. 讀取市場預設語系（AppSetting）
    var rawDefaultLanguages = configService.GetAppSetting("Translation.DefaultLocale") ?? string.Empty;

    // 3. 取得目前 CultureInfo（由 ASP.NET 從瀏覽器 Accept-Language Header 初始化）
    var locale = CultureInfo.CurrentUICulture.Name;
    var lang = locale.Split('-').FirstOrDefault();  // 取語言前綴，如 "zh-TW" → "zh"

    var availableLanguages = rawAvailableLanguages
        .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

    // 4. 三層比對邏輯
    var targetLanguage =
        availableLanguages.FirstOrDefault(l => l == locale) ??              // 精確比對
        availableLanguages.FirstOrDefault(l => l.StartsWith($"{lang}-")) ?? // 語言前綴比對
        rawDefaultLanguages;                                                   // 預設語系 fallback

    // 5. 設定最終語系
    CultureInfo.CurrentUICulture = CultureInfo.CreateSpecificCulture(targetLanguage);
}
```

### 語系來源說明

`LocaleFilter()` 讀取的 `CultureInfo.CurrentUICulture` 是由 **`Web.config` 的 `<globalization uiCulture="auto:zh-TW" />`** 設定所初始化，ASP.NET Framework 在 Request Pipeline 早期即自動從 `Accept-Language` Header 完成設定。

`LocaleFilter()` 的職責是將這個值「**對應到系統支援的語系清單**」，三步驟如下：

| 步驟 | 比對方式 | 範例 |
|------|----------|------|
| 1 | 精確比對 | 瀏覽器 `zh-TW` → 系統有 `zh-TW` → ✅ 採用 |
| 2 | 語言前綴比對 | 瀏覽器 `zh-HK` → 系統有 `zh-TW`（zh 開頭）→ ✅ 採用 `zh-TW` |
| 3 | 預設語系 fallback | 瀏覽器 `ja-JP` → 系統無對應 → 使用 `Translation.DefaultLocale` |

---

## 第二層：使用者個人語系偏好 Filter（全域 Action Filter）

### 觸發時機

每個 Controller Action 執行前，作為全域 Filter 自動套用。

### 檔案位置

```
WebSite/WebSite/Extensions/ActionFilters/GetUserPreferLangAttribute.cs     ← MVC
WebSite/WebSite/Extensions/ActionFilters/GetUserPreferLangApiAttribute.cs  ← Web API
```

### 登記位置

```
WebSite/WebSite/App_Start/FilterConfig.cs   ← MVC 全域 Filter 登記
WebSite/WebSite/App_Start/WebApiConfig.cs   ← Web API 全域 Filter 登記
```

### 完整執行流程

```
OnActionExecuting 觸發
      │
      ▼
[中止條件 1] QueryString 含 IsSetAcceptLanguage=true？
      │ YES → 直接結束，保留第一層語系（前端主動指定 Accept-Language）
      │ NO ↓
      ▼
[中止條件 2] 使用者是否已登入？
      │ NO → 直接結束
      │ YES ↓
      ▼
[中止條件 3] 從 JWT Claims 取得 UserData，解析 SupplierId + UserId
      │ 失敗 → 直接結束
      │ 成功 ↓
      ▼
[開關判斷] AppSetting: Multilingual.UserPreferLanguage.EnabledShopIds
          取 User 登入的第一個 ShopId（ShopId 最小值）判斷
      │ 此商店未在白名單 → 直接結束
      │ 此商店在白名單 ↓
      ▼
呼叫 GetUserPreferLanguage(supplierId, userId)
      │
      ▼
先查 Redis Cache（Key: GetDDBAuthExternalUserInfo-20250424001-{supplierId}-{userId}）
      │ Cache Hit → 直接返回快取值
      │ Cache Miss ↓
      ▼
查詢 AWS DynamoDB（Table: UserPreferLanguage 相關 Table）
      │ 有資料 → 回傳 DynamoDB["UserPreferLanguage"] 欄位值
      │ 無資料 → 回傳 AppSetting Translation.DefaultLocale
      ▼
preferLang 為空？
      │ YES → 直接結束（不覆蓋語系）
      │ NO ↓
      ▼
設定 CultureInfo.CurrentUICulture = preferLang（覆蓋第一層語系）
MVC 版本額外設定 ViewBag.UserPreferLang = preferLang
```

### `IsSetAcceptLanguage` 參數說明

當前端（或 API 呼叫方）在 QueryString 帶上 `IsSetAcceptLanguage=true` 時，代表**前端已自行設定 Accept-Language Header**，系統應尊重前端設定，不套用使用者個人語系偏好。

```
GET /Api/SomePage?IsSetAcceptLanguage=true
```

### `Multilingual.UserPreferLanguage.EnabledShopIds` 格式

此 AppSetting 使用 `|` 分隔三個區段：

```
{全域開關}|{指定商店ID清單（逗號分隔）}|{商店ID範圍清單（dash分隔範圍，逗號分隔多組）}
```

| 設定值 | 效果 |
|--------|------|
| `true\|none\|none` | 全部商店開啟 |
| `false\|none\|none` | 全部商店關閉 |
| `true\|8,233\|none` | 只有 ShopId 8 和 233 開啟 |
| `true\|8,233\|100-120` | ShopId 8、233 及 100~120 開啟 |

### AWS DynamoDB 資料結構

| 欄位名稱 | 說明 |
|----------|------|
| `SupplierId_UserId` | 主鍵（格式：`{SupplierId}_{UserId}`） |
| `UserPreferLanguage` | 使用者偏好語系（如 `en-US`、`zh-TW`） |
| `SupplierId` | 客戶序號 |
| `UserId` | 使用者序號 |
| `UpdateDateTime` | 最後更新時間 |
| `UpdateUser` | 最後更新操作人員 |

### Redis 快取設定

| 項目 | 值 |
|------|-----|
| ServiceName | `UserInfo` |
| TypeName | `UserPreferLanguage` |
| Key 格式 | `GetDDBAuthExternalUserInfo-20250424001-{supplierId}-{userId}` |
| TTL | **240 分鐘（4 小時）** |

> ⚠️ 當使用者修改語系偏好後，系統會立即呼叫 `_cacheService.Remove()` 清除快取，確保語系立即生效。

---

## 第三層：RestrictLocaleAttribute（特定功能限定）

### 觸發時機

僅在特定 Controller Action 標記了 `[RestrictLocale]` 或 `[APIRestrictLocale]` 時才執行。

### 檔案位置

```
WebSite/WebSite/Extensions/ActionFilters/RestrictLocaleAttribute.cs       ← MVC
WebSite/WebSite/Extensions/ActionFilters/APIRestrictLocaleAttribute.cs    ← Web API
```

### 使用方式

```csharp
// 在 Action 上標記，傳入兩個 AppSetting key
[RestrictLocale("Export.AvailableLocales", "Export.DefaultLocale")]
public ActionResult ExportReport() { ... }
```

### 執行邏輯

與第一層 `LocaleFilter` 完全相同的比對邏輯，但語系來源改為**特定功能的 AppSetting**：

1. 讀取 `key` 指定的 AppSetting（允許語系清單）
2. 讀取 `defaultValue` 指定的 AppSetting（預設語系）
3. 依序精確比對 → 前綴比對 → fallback 預設語系
4. 若最終語系為空，拋出 `ApplicationException("Detect locales for export failed.")`

### 使用場景

主要用於**匯出功能**等需要限制特定語系的場景（例如某功能只支援 `zh-TW`），與全域語系設定隔離，互不干擾。

---

## 優先順序總結

### 完整初始化順序（修正版）

```
① Web.config: <globalization uiCulture="auto:zh-TW" />
     ASP.NET 自動讀取 Accept-Language Header
     → 設定 CultureInfo.CurrentUICulture
          （若 Header 不存在 → 預設 zh-TW）
             │
             ▼
② Global.asax.cs: Application_BeginRequest → LocaleFilter()
     讀取 CultureInfo.CurrentUICulture（由①決定）
     → 對應到 Translation.Locales 支援清單
          精確比對 → 前綴比對 → Translation.DefaultLocale
             │
             ▼
③ Action Filter: GetUserPreferLangAttribute（全域，每個 Action 前）
     條件：商店啟用 & 無 IsSetAcceptLanguage=true
     → 從 DynamoDB 取個人設定
     → 覆蓋 CultureInfo.CurrentUICulture
             │
             ▼
④ RestrictLocaleAttribute（僅特定 Action）
     讀取功能專屬 AppSetting 語系清單
     → 進一步限制/覆蓋語系
```

### 優先序對照表
|--------|----------|----------|------|
| **最高** | 商店已啟用個人語系偏好 & QueryString **無** `IsSetAcceptLanguage=true` | **AWS DynamoDB 使用者個人設定** | 無資料時 fallback 到 `Translation.DefaultLocale` |
| **中** | QueryString 有 `IsSetAcceptLanguage=true`，或商店未啟用個人語系偏好 | **HTTP Accept-Language Header** → 對應 `Translation.Locales` | 找不到對應語系時 fallback 到 `Translation.DefaultLocale` |
| **最低** | 以上皆無法解析，或 Accept-Language 不在支援清單中 | **`Translation.DefaultLocale`**（強制 fallback） | 無法解析時拋出 `ApplicationException` |

### 決策流程圖（簡化版）

```
Request 進來
    │
    ▼
LocaleFilter()
  Accept-Language → 對應 Translation.Locales → 無匹配 → Translation.DefaultLocale
    │
    ▼（Action Filter）
IsSetAcceptLanguage=true?
  YES ──────────────────────────────► 使用 LocaleFilter 結果
  NO
    │
    ▼
Multilingual.UserPreferLanguage 功能啟用？（依 ShopId 判斷）
  NO ───────────────────────────────► 使用 LocaleFilter 結果
  YES
    │
    ▼
DynamoDB 有設定值？
  YES → 使用個人偏好語系  ──────────► 覆蓋 LocaleFilter 結果
  NO  → Translation.DefaultLocale ──► 覆蓋 LocaleFilter 結果
```

---

## 關鍵 AppSetting 設定

### 全域語系設定

| AppSetting Key | 說明 | 範例值 |
|----------------|------|--------|
| `Translation.Locales` | 此市場支援的語系清單（逗號分隔） | `zh-TW,en-US` |
| `Translation.DefaultLocale` | 此市場的預設語系（唯一值） | `zh-TW` |
| `Translation.LocalesForFetch` | 翻譯 API 抓取語系清單 | `zh-TW,en-US` |
| `Translation.RootFolder` | 翻譯檔案根目錄路徑 | （路徑字串） |

### 個人語系偏好功能開關

| AppSetting Key | 說明 | 範例值 |
|----------------|------|--------|
| `Multilingual.UserPreferLanguage.EnabledShopIds` | 啟用個人語系偏好功能的商店白名單 | `true\|8,233\|none` |

### 設定檔位置

- **開發環境**：`WebSite/WebSite/Web.Debug.config`（或 `ConnectionStrings.Debug.MY.config`）
- **正式/測試環境**：透過 `Web.config` 的 `appSettings` 區段或外部設定檔

---

## 支援的語系清單

語系清單**硬寫在** `UserService.GetAvailableLanguages()` 方法中：

```csharp
var allLanguages = new List<LanguageEntity>()
{
    new LanguageEntity { Code = "zh-TW", Name = "中文 (繁體)" },
    new LanguageEntity { Code = "en-US", Name = "English (US)" },
    new LanguageEntity { Code = "zh-HK", Name = "中文 (香港)" },
    new LanguageEntity { Code = "ms-MY", Name = "Bahasa Melayu" }
};
```

實際**顯示給使用者選擇**的語系，由 `Translation.Locales` AppSetting 過濾決定。市場預設語系（`Translation.DefaultLocale`）會排序在第一位。

| Code | 名稱 | 備註 |
|------|------|------|
| `zh-TW` | 中文 (繁體) | 台灣市場預設 |
| `en-US` | English (US) | 英文版 |
| `zh-HK` | 中文 (香港) | 香港市場 |
| `ms-MY` | Bahasa Melayu | 馬來西亞市場 |

---

## 相關檔案位置總覽

### Action Filter（語系判斷核心）

| 檔案 | 說明 | 套用範圍 |
|------|------|----------|
| `WebSite/WebSite/Extensions/ActionFilters/GetUserPreferLangAttribute.cs` | MVC 使用者個人語系偏好 Filter | 全域（MVC） |
| `WebSite/WebSite/Extensions/ActionFilters/GetUserPreferLangApiAttribute.cs` | Web API 使用者個人語系偏好 Filter | 全域（Web API） |
| `WebSite/WebSite/Extensions/ActionFilters/RestrictLocaleAttribute.cs` | MVC 特定功能語系限制 | 單一 Action |
| `WebSite/WebSite/Extensions/ActionFilters/APIRestrictLocaleAttribute.cs` | Web API 特定功能語系限制 | 單一 Action |

### 全域設定

| 檔案 | 說明 |
|------|------|
| **`WebSite/WebSite/Web.config`** | **`<globalization uiCulture="auto:zh-TW" />`：讀取瀏覽器 Accept-Language 的根源** |
| `WebSite/WebSite/Global.asax.cs` | `Application_BeginRequest` → `LocaleFilter()` |
| `WebSite/WebSite/App_Start/FilterConfig.cs` | MVC 全域 Filter 登記（GetUserPreferLangAttribute） |
| `WebSite/WebSite/App_Start/WebApiConfig.cs` | Web API 全域 Filter 登記（GetUserPreferLangApiAttribute） |
| `WebSite/WebSite/App_Start/AutofacConfig.cs` | DI 容器登記（GetUserPreferLangAttribute、GetUserPreferLangApiAttribute） |

### 業務邏輯層

| 檔案 | 說明 |
|------|------|
| `BusinessLogic/Services/Users/IUserService.cs` | `IsUserPreferLanguage()` 及 `GetUserPreferLanguage()` 介面定義 |
| `BusinessLogic/Services/Users/UserService.cs` | 語系相關業務邏輯實作（DynamoDB 查詢、快取管理、商店白名單判斷） |

### 翻譯設定

| 檔案 | 說明 |
|------|------|
| `CrossLayer/Translations/TranslationSettings.cs` | 讀取 `Translation.Locales`、`Translation.DefaultLocale`、`Translation.LocalesForFetch` |
| `CrossLayer/Translations/TranslationModule.cs` | Autofac 翻譯相關 DI 模組 |

### 資源檔（.resx）

| 路徑 | 說明 |
|------|------|
| `CrossLayer/Resources/` | 所有多語系資源檔（約 160+ 個 .resx） |
| `CrossLayer/Resources/{模組}/*.resx` | 英文版資源檔（預設） |
| `CrossLayer/Resources/{模組}/*.zh-TW.resx` | 繁體中文版資源檔 |

---

## 語系資料儲存與快取

### 使用者語系偏好的讀寫流程

#### 寫入（使用者修改語系偏好）

```
使用者在後台修改語系設定
    │
    ▼
UserService.SetAuthExternalUserInfo()
    ├─ 確認商店已啟用個人語系偏好功能
    ├─ 取得 Shop 的 SupplierId
    ├─ 呼叫 DynamoDB PutItem（寫入/更新）
    └─ 呼叫 Redis CacheService.Remove() 清除快取（立即生效）
```

#### 讀取（每次 Action 執行前）

```
GetUserPreferLangAttribute.OnActionExecuting()
    │
    ▼
UserService.GetUserPreferLanguage(supplierId, userId)
    │
    ▼
Redis Cache 查詢
    ├─ Cache Hit → 直接返回（不查 DDB）
    └─ Cache Miss
           │
           ▼
       AWS DynamoDB GetItem
           ├─ 有 UserPreferLanguage 欄位 → 返回語系值
           └─ 無資料 → 返回 AppSetting Translation.DefaultLocale
```

### 多市場架構注意事項

- 每個部署市場（TW、MY、HK 等）有**獨立的** AppSetting 設定（`Translation.Locales`、`Translation.DefaultLocale`）
- 語系功能的商店白名單（`Multilingual.UserPreferLanguage.EnabledShopIds`）也是**各市場獨立設定**
- 資源檔（.resx）是**共用**的，同一個系統支援多語系顯示

---

*文件由 GitHub Copilot 自動分析 NineYi.Sms 原始碼產生*
