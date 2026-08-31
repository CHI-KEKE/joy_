
## Slack

https://91app.slack.com/archives/C3DB30C3T/p1772084423609039


## error

檢查週期：2026-02-26 13:30:17-2026-02-26 13:40:17MobileWebMall Elmah Notification@group MobileWebMall 發生過多 Error ,請盡速確認 :smile:ExceptionMassageERROR|Key(selling_start) was not found in module(frontend.sale_page), locale(zh-HK).|Count630



## appsetting

AppSettings.Prod.TW.config 沒有 zh-HK 的設定


Translation.LocalesForFetch


## shopDeault


```sql
use WebStoreDB
select *
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_GroupTypeDef = 'Language'
--and ShopDefault_Key in ('Default','IsEnableAutoDetectLanguage')
and ShopDefault_ShopId = 17
```

## 原因


TW PROD Appsetting 沒有 zh-HK, 但 shopDefault group = Language, key = Contract 有,前台取得時出現大量錯誤


三個 Key 讀出來後各自的用途：

 Key = Default   → shopCrossBorder.DefaultLanguage         ← 影響 CurrentUICulture
 Key = Setting   → shopCrossBorder.AvailableLanguages      ← 影響 CurrentUICulture
 Key = Contract  → shopCrossBorder.ContractLanguages       ← 在這個 repo 只存不用

語系交集邏輯只用 Setting，不用 Contract：

 var finalAvailableLangs = allLangsFromConfig   // ← MSA Config: all-languages
     .Where(le => shopCrossBorder.AvailableLanguages.Any(l => le.Lang == l));  // ← Key=Setting




