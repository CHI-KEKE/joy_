翻譯人員更新了 zh-HK 語系的「PayProfile」


## Publish API

- 更新 S3 上的 backend.definition.PayProfile:zh-HK.json
- 更新 S3 上的 i18n.manifest（新的 ETag）

<br>
<br>

## Redis

可能因為 Publish 通知，更新了 Redis 中的 manifest key

Translation:QA1MY:i18n:NineYi.WebStore.MallAndApi:i18n.manifest
```json
"backend.definition.PayProfile|en-US": {
    "Name": "backend.definition.PayProfile|en-US",
    "Module": "backend.definition.PayProfile",
    "Locale": "en-US",
    "ETag": "\"9e111dff5ef9266e3ae81f6477ed42bc\"",
    "IsSuccessful": true,
    "Message": "Updated.",
    "PublishedAt": "2025-10-28T12:04:58+08:00"
},
```

<br>
<br>

## Fetch API 被觸發

- 比對 manifest ETag → 發現不一致
- 從 S3 抓下新的 zh-HK.json
- 寫回 Redis

Translation:QA1MY:i18n:NineYi.WebStore.MallAndApi:backend.definition.PayProfile:zh-HK.json
```json
{
    "PayProfile_TypeDef_Aftee": "AFTEE先享後付",
    "PayProfile_TypeDef_AliPayHK_EftPay": "AlipayHK",
    "PayProfile_TypeDef_ApplePay": "Apple Pay",
    "PayProfile_TypeDef_ATM": "ATM",
    "PayProfile_TypeDef_Atome": "Atome",
    "PayProfile_TypeDef_Boost_AsiaPay": "Boost",
    "PayProfile_TypeDef_CashOnDelivery": "貨到付款",
    "PayProfile_TypeDef_CathayPay": "信用卡付款",
    "PayProfile_TypeDef_CreditCardInstallment": "信用卡分期付款"
}
```

回傳最新的翻譯資料給前端

<br>
<br>

## 一次取用時

因為 Redis 已更新，不用再從 S3 抓
註記 : 如果資料的 Redis 被刪掉，會造成 fetch 沒有用，因為 etag 不變