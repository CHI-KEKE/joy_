https://sms.qa1.hk.91dev.tw/api/translations/GetClientModuleLocale/CommerceCloud.Index/zh-HK

frontend.cloud.commerce_cloud.homepage.agent_management: "客服管理"


C:\91APP\NineYi.Sms\WebSite\WebSite\Controllers\Apis\TranslationController.cs

line 352


## Purpose

取得前端需要的多語系資料

## steps


- 取得系統預設的記憶體快取實例
- 建立快取鍵值,格式為: `{CachePrefix}/GetClientModuleLocale/{pageName}/{locale}`
- 取得頁面需要的模組清單
```csharp
List<string> bundlePageModules = this._translationSettings.ClientModuleSetting.ClientModules
    .Where(page => page.Name == pageName)
    .FirstOrDefault()?.Modules;

List<string> defaultModules = this._translationSettings.ClientModuleSetting.DefaultModules;

// {
//   "DefaultModules": ["Common", "Error", "Validation"],
//   "ClientModules": [
//     {
//       "Name": "OrderList",
//       "Modules": ["Order", "Payment"]
//     }
//   ]
// }
```
- 合併模組清單
```csharp
var bundleModules = defaultModules.Concat(bundlePageModules);

// 結果: ["Common", "Error", "Order", "Payment"]
```

- 查詢多語系資料

```csharp
var data = this._translationClient.GetManifest()
    .Select(x => x.Value)
    .Where(m => bundleModules.Any(x => x == m.Module) && m.Locale == locale)
    .SelectMany(m => (this._translationClient.GetTranslation(m.Module, m.Locale) ?? new Dictionary<string, string>())
        .Select(t => new KeyValuePair<string, string>($"{m.Module}.{t.Key}", t.Value)))
    .Select(kvp => new KeyValuePair<string, string>(kvp.Key, this.TransformOutputText(kvp.Value)))
    .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

```

  - 6.1 取得翻譯清單 (Manifest)
```csharp
this._translationClient.GetManifest().Select(x => x.Value)

// IReadOnlyDictionary<string, I18nManifestItem> GetManifest();
// public class I18nManifestItem
// {
//     public static readonly string MessageUpdated = "Updated.";

//     public static readonly string MessageNoChange = "No Change.";

//     private string _originalETag;

//     public string Name => Module + "|" + Locale;

//     public string Module { get; set; }

//     public string Locale { get; set; }

//     public string ETag { get; set; }

//     public bool IsSuccessful { get; set; }
// GetManifest() 回傳所有可用的翻譯模組清單
// 每個模組包含: 模組名稱、語系、版本等資訊
```
- 6.2 篩選需要的模組和語系
```csharp
.Where(m => bundleModules.Any(x => x == m.Module) && m.Locale == locale)

// 只保留合併後清單中的模組
// 只保留符合指定語系的資料
```

6.3 取得每個模組的翻譯內容
```csharp
.SelectMany(m => (this._translationClient.GetTranslation(m.Module, m.Locale) ?? new Dictionary<string, string>())
    .Select(t => new KeyValuePair<string, string>($"{m.Module}.{t.Key}", t.Value)))

// GetTranslation() 取得該模組的所有翻譯鍵值對
// 將鍵值加上模組前綴: {Module}.{Key} → 例如 Order.Title
// 如果取得失敗,使用空字典避免錯誤
```

6.4 轉換輸出文字格式
```csharp
.Select(kvp => new KeyValuePair<string, string>(kvp.Key, this.TransformOutputText(kvp.Value)))

// 呼叫 TransformOutputText() 處理翻譯文字
// 這個方法會做三件事:
// 將換行符號轉換為 <br/>
// 將雙引號轉換為單引號
// 將變數格式從 {0} 轉換為 Angular 格式 {{val0}}
```

6.5 轉換為字典
```csharp
.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)


// {
//   "Common.Save": "儲存",
//   "Order.Title": "訂單清單",
//   "Order.Status": "狀態"
// }
```



