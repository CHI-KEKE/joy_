# 🎯 InfoModule 主題推薦模組

<br>

## 📖 目錄
  - [📝 功能概述](#-功能概述)
  - [🔗 OSM 相關 API](#-osm-相關-api)
  - [🌐 內容多語系設定](#-內容多語系設定)
  - [📄 前台多語系 API](#-前台多語系-api)
  - [🏷️ Attribute 設定](#️-attribute-設定)
  - [📋 詳細頁實作](#-詳細頁實作)
  - [💾 快取機制](#-快取機制)
  - [🔧 程式碼實作](#-程式碼實作)
  - [📎 S3 HTML 連結](#-s3-html-連結)

<br>

---

## 📝 功能概述

InfoModule 是用來做主題推薦的功能模組，包含三種不同類型的內容：

<br>

- **圖文模組** - 提供圖片與文字的組合內容
- **影音模組** - 提供視頻與音頻內容
- **相簿模組** - 提供相片集合展示

<br>

---

## 🔗 OSM 相關 API

**模組清單**：

<br>

```
https://sms.qa1.hk.91dev.tw/InfoModule/List?shopId=2
```

<br>

**檢查模組啟用狀態**：

<br>

```
https://sms.qa1.hk.91dev.tw/Api/InfoModule/IsModuleEnabled
```

<br>

**新增影音模組**：

<br>

```
https://sms.qa1.hk.91dev.tw/Api/InfoModule/CreateVideo
```

<br>

**新增圖文模組**：

<br>

```
https://sms.qa1.hk.91dev.tw/Api/InfoModule/CreateArticle
```

<br>

**新增相簿模組**：

<br>

```
https://sms.qa1.hk.91dev.tw/Api/InfoModule/CreateAlbum
```

<br>

---

## 🌐 內容多語系設定

**三個模組的多語系識別**：

<br>

- **圖文模組**：`infomodulearticle`
- **影音模組**：`infomodulevideo` 
- **相簿模組**：`infomodulealbum`

<br>

**顯示用多語系 Key**：

<br>

- `multilingual_module_infomodulearticle`
- `multilingual_module_infomodulevideo`
- `multilingual_module_infomodulealbum`

<br>

---

## 📄 前台多語系 API

**清單頁 API**：

<br>

```
/webapi/InfoModule/geteditorpicklist
/webapi/InfoModule/getarticlelist
/webapi/InfoModule/getalbumlist
/webapi/InfoModule/getvideolist
```

<br>

**詳細頁 API 範例**：

<br>

```
https://www.sasa.com.hk/Video/Detail/251
```

<br>

---

## 🏷️ Attribute 設定

**清單頁 Attribute 設定**：

<br>

```csharp
[MultilingualContent(MultilingualModuleTypeEnum.InfoModuleArticle)]
[MultilingualContent(isIdColumn:true)]
[MultilingualContent(targetColumn: "SubTitle")]
[MultilingualContent(targetColumn: "Introduction")]
[MultilingualContent(targetColumn: "Title")]
```

<br>

**詳細頁 Attribute 設定**：

<br>

```csharp
[MultilingualContent(MultilingualModuleTypeEnum.InfoModuleAlbum)]
[MultilingualContent(isIdColumn:true)]
[MultilingualContent(targetColumn: "SubTitle")]
[MultilingualContent(targetColumn: "Introduction")]
[MultilingualContent(targetColumn: "Title")]
```

<br>

---

## 📋 詳細頁實作

**檔案位置**：

<br>

```
C:\91APP\MWeb2\nineyi.webstore.mobilewebmall\WebStore\Frontend\MobileWebMall\Views\Shop\AlbumDetail.cshtml
```

<br>

**Entity 模型**：

<br>

```csharp
ShopAlbumDetailForFrontendEntity
```

<br>

---

## 💾 快取機制

**快取 Key 建立**：

<br>

```csharp
cacheKey.ServiceName = "InfoModule";
cacheKey.TypeName = string.Format("{0}-{1}", nameof(GetAlbumDetail), "2018091915");
cacheKey.Key = key;
```

<br>

---

## 🔧 程式碼實作

**取得相簿詳細資料**：

<br>

```csharp
ShopAlbumDetailForFrontendEntity shopAlbumEntity = this._infoModuleService.GetAlbumDetailData(albumId, cleanCache);
```

<br>

**取得多語系支援設定**：

<br>

```csharp
MultilingualContentSupportEntity multilingualContentSupportEntity = this.ViewBag.MultilingualContentSupport;
```

<br>

**準備快取 Key**：

<br>

```csharp
var cacheKey = new CacheKeyEntity()
{
    ServiceName = "InfoModule",
    TypeName = string.Format("{0}-{1}", "GetAlbumDetail", "2018091915"),
    Key = albumId.ToString()
};
```

<br>

**置換多語系內容**：

<br>

```csharp
shopAlbumEntity = MultilingualContentHelper.GetReplaceContentWithMultilingualContentCache(
    this._lifetimeScope, 
    shopAlbumEntity, 
    cacheKey, 
    multilingualContentSupportEntity, 
    MultilingualModuleTypeEnum.InfoModuleAlbum, 
    cleanCache);
```

<br>

---

## 📎 S3 HTML 連結

**HTML 內容多語系連結**：

<br>

```csharp
/// <summary>
/// 備註內容多語系 html 連結
/// </summary>
[MultilingualContent("Introduction")]
public string IntroductionMulHtmlUrl { get; set; }
```

<br>

**前端資料存取**：

<br>

```javascript
window.nineyi.ServerData
```

<br>
