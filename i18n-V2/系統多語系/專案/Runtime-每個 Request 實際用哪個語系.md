## DetectCrossBorderDataForWeb() 會按優先順序決定語系

優先順序（由高到低）：
 
```bash
 1. QueryString  ?lang=en-US
         ↓ 沒有
 2. Cookie       _lang=en-US
         ↓ 沒有
 3. GetShopDefaultLangTag()   ← ⭐ 這裡有商店設定介入
         ↓ 沒有
 4. Accept-Language Header（瀏覽器帶的）
         ↓ 都沒有
 5. AppSettings DefaultLocale（最終 fallback）
```


## GetShopDefaultLangTag 做什麼

 ```csharp
 var langTag = qsLangTag
     ?? cookieLangTag
     ?? this.GetShopDefaultLangTag(shopId, acceptedLangTag, cleanCache)  // ← 查商店設定
     ?? acceptedLangTag
     ?? defaultLangFromConfig;
 ```

這一步會去查商店的跨境設定（ShopCrossBorder），包含

 - 商店啟用的可用語系清單（AvailableLanguages）
 - 商店的預設語系（DefaultLanguage）
 - 是否啟用自動偵測語系（IsEnableAutoDetectLanguage）


最後還會做一步取交集：

 // AppSettings 設定的語系  ∩  商店開放的語系
 var finalAvailableLangs = allLangsFromConfig
     .Where(le => shopCrossBorder.AvailableLanguages.Any(l => le.Lang == l));


## shopDefault


```sql
use WebStoreDB
select *
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_GroupTypeDef = 'Language'
and ShopDefault_Key in ('Default','IsEnableAutoDetectLanguage')
and ShopDefault_ShopId = 17
```