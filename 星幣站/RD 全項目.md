## 範例檔

- 新增商品頁
- 批次出貨
- 國家地區配送範例檔

## DB

- PayProfileCurrency
- ShopStaticSetting
- ShippingArea
- 新加坡銀行
- 新增配送商資料
- CountryProfile_Enabled

## Config 確認

#### mweb

GlobalShipping.Singapore.Url
OverseaShipping.LocalArea.Singapore.ShippingAreaId


#### SMS

- Shipping.DisableWeightPricing config
- EasyParcel.Singapore.Authentication
- GlobalShipping.Singapore.Url

#### Shopping

- OverseaShipping:SingaporeArea.ShippingAreaId（[Shopping] ShippingAreaId Config??）
- OverseaShipping:SingaporeArea.CountryCode
- SalesMarketAdministrativeRegionFilePath:Singapore

#### Cart

- SalesMarketAdministrativeRegionFilePath:Singapore

## ShopDefault

"GlobalShipping", "PickCountry"
"GlobalShipping", "PickAddress"
"GlobalShipping", "PickZipCode"
"GlobalShipping", "PickCity"


## machine config

#### MWeb, SMS

EasyParcel.Singapore.Authentication

## json

- shopping
- cart
- MWeb


## easyparcel

- EasyParcel 帳戶設定確認（綁卡、topup）
- shopDefault Country、PickZipCode（SG 國內）- EasyParcel 使用

##　優化

- 改一個相容 CODE：unitRegex.Match(shippingLocationInfo.PickAddress);（防呆？）
- AddressHelper 改 enum
