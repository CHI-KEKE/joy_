

[管理介面](https://translation.qa.91dev.tw/web/NineYi.WebStore.MallAndApi/search?=%E3%80%81)


1. Publish API 會將資料推送到 S3 儲存
2. 專案系統都是從 S3 拉取語系資料
3. 中間透過 Redis 快取機制提升效能




## 快取層級機制

In Memory Cache ⇒ Redis Cache ⇒ 直接回傳 Key 值

<br>

## 環境同步機制

透過 Jenkins 工具可將 QA 環境的多語系內容直接同步到其他環境

<br>

## 專案整合方式

<br>

- **SDK 名稱**：Translations Client
- **前端 API**：提供 Fetch API 讓瀏覽器拉取最新語系資料
- **程式碼產生**：執行 Auto Code Generation 從 S3 抓取資料轉換成 .cs 檔案

<br>

## S3

[S3](https://s3-ap-northeast-1.amazonaws.com/91dev-ap-northeast-1-translation-service/NineYi.Sms/backend.definition.BatchUpload/zh-TW.json)



<br>

## Fetch

確認系統是否真的有執行 Fetch 操作，需檢查以下檔案的 ETag 更新狀況
C:\Files\i18n\NineYi.WebStore.MallAndApi/i18n.manifest
只有當此檔案的 ETag 有更新時，系統才會重新進行 Fetch 操作


[HKQA_MWeb_shop2](https://shop2.shop.qa1.hk.91dev.tw/webapi/translations/fetch)
[MYQA_MWeb_moiicrm](https://moiicrm.shop.qa1.my.91dev.tw/webapi/translations/fetch)
[SMS](https://sms.dev.91dev.tw/api/translations/fetch)

<br>

## Debug API

可以確認 Remote / Fetch / Project / Module 到底少了哪一段

[MWEB](https://moiicrm.shop.qa1.my.91dev.tw/webapi/translations/debug/backend.definition.PayProfile.PayProfile_TypeDef_CreditCardInstallment_Razer/zh-TW)

[SMS](https://sms.pp.91dev.tw/api/translations/debug/backend.entity.promotion_engine_sale_page_ranking.ranking/zh-TW)
