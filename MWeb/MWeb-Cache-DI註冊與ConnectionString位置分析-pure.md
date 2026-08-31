# MWeb 快取服務 DI 註冊與 Connection String 位置分析

## 01 情境概述

前一份文件追到 `SearchService.GetShopCategorySalePageList` 會透過 `DataCacheService` 存取 Redis 快取。這份文件接著往下挖：`DataCacheService` 這個物件實際上是怎麼被組裝出來的，它連的 Redis 位址又藏在哪一個設定檔裡。

在 .NET 專案裡，程式碼裡看到的每一個服務物件，通常不是自己 `new` 出來的，而是由一個叫做依賴注入容器（這裡用的是 Autofac）的機制在程式啟動時統一組裝好，用到的時候再自動塞進建構子裡。`DataCacheService` 也是這樣，前一份文件裡看到的 `this._dataCacheService.GetRedisCacheData(...)`，這個 `_dataCacheService` 其實是啟動當下由 Autofac 幫忙組出來的一個實例，裡面已經先接好了要用哪一組 Redis 連線。

這份文件要回答三個問題：`DataCacheService` 在哪一段程式碼被註冊進容器、註冊的時候幫它接了哪些 Redis 連線、以及這些連線字串實際的值放在哪個設定檔裡，不同環境（QA、Debug、正式站）之間又差在哪。

**核心類別：** `NineYi.WebStore.Frontend.BLV2.DataCache.DataCacheService`，實作 `IDataCacheService`，是所有 Redis 快取存取的統一入口。

**組裝地點：** Autofac 模組 `CacheModule`（`Frontend\BLV2\Modules\CacheModule.cs`），負責建立六組 Redis 連線並注入 `DataCacheService`。

**連線字串：** 用 `{Execmode}.Redis.Cache` 這種格式的 key，從 `ConnectionStrings*.config` 讀出來，正式環境的真實值不會出現在版控裡。

## 02 DataCacheService 是誰

`DataCacheService` 的檔案位置是 `C:\91APP\nineyi.webstore.mobilewebmall\WebStore\Frontend\BLV2\DataCache\DataCacheService.cs`，它是整個 mweb 生態系裡所有需要「先查 Redis，查不到再算一次並存回去」這種快取模式的共用實作，前一份文件分析的 `GetRedisCacheData` 就是它公開的方法之一。

它的建構子長這樣，注入了兩個 `ICacheService`，分別對應不同的用途：

```csharp
// DataCacheService.cs
public DataCacheService(
    ICacheService cacheService,
    ITranslationClient translationClient,
    IConfigService configService,
    [WithKey("FileCache")] ICacheService fileCacheService)
{
    this._cacheService = cacheService;
    this._translationClient = translationClient;
    this._configService = configService;
    this._fileCacheService = fileCacheService;

    this._isCacheWithLocale = bool.Parse(this._configService.GetAppSetting("Cache.Locale.Enable", "false"));
}
```

| 建構子參數 | 用途 |
| --- | --- |
| `cacheService` | 一般 Redis 快取服務，供 `GetRedisCacheData` 使用，也就是 `SearchService` 快取商品清單所走的那一組。 |
| `fileCacheService`（帶 `"FileCache"` key） | 另一組獨立的 Redis 快取服務，供 `GetRedisLargeCacheData` 使用，命名上是給比較大量的資料做快取用。 |
| `translationClient` | 取得目前語系代碼，用在把 Key 加上語系後綴的 `AppendLocale` 方法。 |
| `configService` | 讀取 `Cache.Locale.Enable` 這個設定，決定是否要幫每個 Key 加上語系後綴。 |

**重點：** `DataCacheService` 本身不知道 Redis 主機在哪裡，它只認得 `ICacheService` 這個介面。真正決定連到哪一台 Redis 的工作，是在下一節的 `CacheModule` 完成的。

## 03 從啟動到組裝：DI 註冊鏈路

以實際會觸發快取邏輯的 WebAPI 這個應用程式為例，從程式啟動到 `DataCacheService` 被組裝完成，會經過三層：

**第一層，App 啟動：`WebAPI\App_Start\AutofacConfig.cs`**

`AutofacConfig.Initialize()` 建立一個空的 `ContainerBuilder`，先註冊控制器、設定讀取、資料庫連線等基礎服務，接著呼叫 `builder.RegisterProxyService("NineYi.WebStore.Frontend.BLV2")`，把整個 BLV2 這個共用商業邏輯組件的內容納進來。

**第二層，共用模組入口：`Frontend\BLV2\Modules\ServiceModule.cs`**

BLV2 這個組件裡，商業邏輯服務的註冊集中在 `ServiceModule`。它的 `Load` 方法第 133 行有這一行：

```csharp
builder.RegisterModule(new CacheModule());
```

也就是說，只要 `ServiceModule` 被載入，`CacheModule` 就會跟著一起被載入。

**第三層，真正組裝的地方：`Frontend\BLV2\Modules\CacheModule.cs`**

`CacheModule.Load()` 讀取 Redis 連線字串、建立六組 `ConnectionMultiplexer`，再一路包成 `ICacheProvider`、`ICacheService`，最後把其中兩組 `ICacheService` 餵給 `DataCacheService` 並註冊成 `IDataCacheService`。細節在下一節整理。

**換句話說：** 每一個會用到 `ISearchService`、`IDataCacheService` 之類 BLV2 商業邏輯服務的 ASP.NET 應用程式（例如 WebAPI），都要在自己的啟動流程裡引用同一份 `ServiceModule`／`CacheModule`，才能拿到能正常運作的 Redis 快取物件。

## 04 CacheModule 裡到底裝了什麼

`CacheModule.Load()` 一開始會依序讀出六把 Redis 連線字串，任何一把找不到都會直接丟例外讓應用程式啟動失敗，避免帶著一個沒接好的快取服務跑上線：

| 連線字串 Key 後綴 | Autofac Key | 用途 |
| --- | --- | --- |
| `.Redis.Cache` | （不加 Key，預設） | 一般 Redis 快取，`SearchService` 分類商品清單快取走的就是這一組。 |
| `.Redis.Data` | `"Data"` | 會員登入狀態等使用者資料快取，由 `UserRedisDataHelper` 使用。 |
| `.Redis.Data2` | `"Data2"` | 與 `Data` 分流用的第二組使用者資料快取。 |
| `.Redis.LineUp` | `"LineUp"` | 排隊機制（搶購/售完通知等情境）專用的 Redis。 |
| `.Redis.FileCache` | `"FileCache"` | `DataCacheService` 建構子裡那個 `fileCacheService` 參數用的連線，供 `GetRedisLargeCacheData` 使用。 |
| `.Redis.Sequence` | `"Sequence"` | 序號產生器專用的 Redis。 |

每一把連線字串都會先包成 `ConnectionMultiplexer`（單例），再包成 `IDatabase`，接著包成 `RedisCacheProvider`（`ICacheProvider`），最後才包成 `CacheService`（`ICacheService`）。整條包裝鏈大概是：

`ConnectionString` → `ConnectionMultiplexer` → `IDatabase` → `ICacheProvider` → `ICacheService`

最後這一段，就是把包好的 `ICacheService` 餵給 `DataCacheService` 並註冊成 `IDataCacheService`：

```csharp
// CacheModule.cs：註冊 DataCacheService
builder.RegisterType<DataCacheService>()
       .WithParameter(new ResolvedParameter(
          (pi, ctx) => pi.ParameterType == typeof(ICacheService) && pi.Name == "cacheService",
          (pi, ctx) => ctx.Resolve<ICacheService>()))
       .WithParameter(new ResolvedParameter(
          (pi, ctx) => pi.ParameterType == typeof(ICacheService) && pi.Name == "fileCacheService",
          (pi, ctx) => ctx.ResolveKeyed<ICacheService>("FileCache")))
       .As<IDataCacheService>();
```

**容易混淆的地方：** 整個模組其實包了不只一組 Redis，但 `SearchService` 分類商品清單快取只走最普通、沒有額外 Key 標記的那一組（`.Redis.Cache`）。其他像會員資料、排隊機制、序號產生，各自都是獨立的 Redis 連線，彼此互不影響。

### Cache Key Prefix 與過期時間怎麼決定

除了連線本身，`CacheModule` 還決定了兩件事：所有 Key 前面要不要加一段固定前綴、以及 Redis 物件的預設過期時間（TTL）。

```csharp
// CacheModule.cs：取得 KeyPrefix
var cacheKeyPrefix = ConfigurationManager.AppSettings.Get(mode + ".Cache.KeyPrefix");

// CacheModule.cs：取得預設過期時間，預設 86400 秒（1天）
var key = $"{mode}.Cache.DefaultExpire";
int.TryParse(ConfigurationManager.AppSettings.Get(key), out defaultExpire);
```

**這個 TTL 跟前一份文件的一分鐘不是同一件事：** 這裡設定的是 `RedisCacheProvider` 沒被特別指定時的預設過期時間（預設一天）。`SearchService` 呼叫 `GetRedisCacheData` 時有另外指定 `expirationSecond = 60`，這個呼叫端指定的值會蓋掉這裡的預設值，所以分類商品清單快取實際只活一分鐘。

## 05 Connection String 讀取規則

`CacheModule.Load()` 讀連線字串的方式，是先讀一個叫做 `Execmode` 的 AppSetting，再拿這個值當前綴，去組出實際要查的 Key：

```csharp
// CacheModule.cs
var mode = ConfigurationManager.AppSettings["Execmode"];
var redisConfig = ConfigurationManager.ConnectionStrings[mode + ".Redis.Cache"];
if (redisConfig == null)
{
    throw new ApplicationException($"Redis ConnectionString不存在, Key: {mode + ".Redis.Cache"}");
}
```

也就是說，實際要在 `web.config` 的 `<connectionStrings>` 區塊裡找的名稱，長得像 `Dev.Redis.Cache`、`Prod.Redis.Cache` 這種「環境代號 + `.Redis.Cache`」的組合，六組連線分別對應 `.Redis.Cache`、`.Redis.Data`、`.Redis.Data2`、`.Redis.LineUp`、`.Redis.FileCache`、`.Redis.Sequence`。

**找不到就直接掛掉：** 六組連線字串只要有任何一組讀不到，`CacheModule.Load()` 就會丟出 `ApplicationException`，代表這個應用程式在啟動階段就會失敗，不會等到真的有人呼叫快取邏輯才發現設定漏掉。

## 06 各環境設定檔實際位置

連線字串的實際內容，放在 WebAPI 專案底下一系列以 `ConnectionStrings` 開頭、依環境命名的 `web.config` 轉換檔案（XDT Transform），路徑都在 `C:\91APP\nineyi.webstore.mobilewebmall\WebStore\WebAPI\` 這個資料夾下：

| 檔案 | 對應環境 | 內容狀態 |
| --- | --- | --- |
| `ConnectionStrings.config` | 基礎預設值（Dev） | 有實際內容，例如 `Dev.Redis.Cache` 指向一組 QA 用的 AWS ElastiCache 網址。 |
| `ConnectionStrings.Debug.HK.config` | 本機除錯，模擬 HK 市場 | 有實際內容，`Dev.Redis.Cache` 指向 HK QA 專用的 Redis 主機。 |
| `ConnectionStrings.Debug.MY.config` | 本機除錯，模擬 MY 市場 | 有實際內容，指向 MY QA 專用的 Redis 主機。 |
| `ConnectionStrings.Debug.PX.config` | 本機除錯，其他市場 | 有實際內容。 |
| `ConnectionStrings.QA.config` | 共用 QA 站 | 整段 `xdt:Transform="RemoveAll"`，代表這個環境的真實值不寫在這份檔案裡。 |
| `ConnectionStrings.Release.config` / `ConnectionStrings.PP.config` / `ConnectionStrings.LoadTest.config` | 其他非正式環境 | 同樣是轉換檔，實際值視部署流程而定。 |
| `ConnectionStrings.Prod.config` | 正式環境 | 整段 `xdt:Transform="RemoveAll"`，見下一節說明。 |

以 `ConnectionStrings.Debug.HK.config` 為例，六組 Redis 連線裡其實只看得到五組（少了 `.Redis.Sequence`），內容大致如下：

```xml
<add name="Dev.Redis.Cache" connectionString="hk-qa-backend-redis-1.xxxx.apse1.cache.amazonaws.com:6379,ssl=false,......"/>
<add name="Dev.Redis.Data" connectionString="hk-qa-backend-redis-1.xxxx.apse1.cache.amazonaws.com:6379,ssl=false,......"/>
<add name="Dev.Redis.Data2" connectionString="hk-qa-backend-redis-1.xxxx.apse1.cache.amazonaws.com:6379,ssl=false,......"/>
<add name="Dev.Redis.LineUp" connectionString="hk-qa-backend-redis-1.xxxx.apse1.cache.amazonaws.com:6379,ssl=false,......"/>
<add name="Dev.Redis.FileCache" connectionString="hk-qa-backend-redis-1.xxxx.apse1.cache.amazonaws.com:6379,ssl=false,......"/>
```

**觀察：** 在這個 HK 除錯環境裡，五組連線字串全部指到同一台 Redis 主機，只是邏輯上用不同的 Key 前綴（`.Redis.Cache`、`.Redis.Data` 等）區分用途，並非每組都各自有獨立主機，這在測試環境是常見的簡化做法。

## 07 正式環境的秘密去哪了

`ConnectionStrings.Prod.config` 跟 `ConnectionStrings.QA.config` 的內容都只有這幾行：

```xml
<connectionStrings xmlns:xdt="http://schemas.microsoft.com/XML-Document-Transform">
  <add xdt:Transform="RemoveAll"/>
</connectionStrings>
```

`xdt:Transform="RemoveAll"` 的意思是，在套用這份轉換檔之後，`<connectionStrings>` 整個區塊會先被清空，再由後續的步驟填入真正的值。也就是說，正式環境（以及共用 QA 站）實際使用的 Redis 連線字串，並不會以明碼寫進這兩個 repo（`nineyi.webstore.mobilewebmall`、`nineyi.configuration`）裡的任何一份檔案。

**推論，真正的值從哪裡來：** 依照這兩個檔案清空後留白的設計，合理推測正式環境的連線字串是在部署流程（CI/CD）階段，由機密管理機制（例如 Secrets Manager、Vault，或部署管線自行注入的環境變數）另外寫進最終部署的 `web.config`，而不是走一般的原始碼版控。這一層目前不在 `nineyi.webstore.mobilewebmall` 或 `nineyi.configuration` 這兩個可檢視的 repo 範圍內，需要有部署管線或機密管理系統的存取權限才能進一步確認實際內容。

**對照前面幾份文件的意義：** 如果要實際確認 HK、MY 兩個市場在 WebAPI 這個應用程式裡，是不是真的連到同一台或不同台 Redis，光看這兩個原始碼 repo 是看不到答案的，需要往部署管線或正式環境的機密管理系統再往下查。

## 08 mweb 自己這邊呢

前面幾節都是以 WebAPI 這個獨立應用程式為例，因為前一份文件已經確認消費者實際看到的商品清單，最終是打進 WebAPI 的 `ShopCategoryLiteController.GetSalePageList`。那 mweb 本身（`MobileWebMallV2` 專案）的 `OfficialController.SalePageCategory` 也會直接呼叫 `SearchService`，它是不是也走同一套 `CacheModule` 註冊鏈路？

檢查 `MobileWebMallV2` 專案自己的 `App_Start\AutofacConfig.cs`，目前沒有看到跟 WebAPI 相同的 `RegisterProxyService("NineYi.WebStore.Frontend.BLV2")` 呼叫，這個專案底下也沒有找到任何 `ConnectionStrings*.config` 這類獨立檔案，`web.config` 本身也沒有 `<connectionStrings>` 區塊。

**尚待確認：** 這代表 `MobileWebMallV2` 專案組裝 `ISearchService`／`IDataCacheService` 的方式，跟 WebAPI 不完全一樣，但目前的程式碼搜尋還無法確定它實際上是透過什麼路徑拿到可用的 Redis 連線（可能是另一種尚未追到的註冊慣例，也可能實際部署時這個專案的 `web.config` 另外被合併進了一份包含連線字串的設定）。這一段留待下次針對 `MobileWebMallV2` 專案的 Autofac 啟動流程再深入分析，目前不確定的部分不在這份文件裡下定論。

---

整理自 nineyi.webstore.mobilewebmall（BLV2 / WebAPI）程式碼分析
