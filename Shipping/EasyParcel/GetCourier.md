
## GlobalShipping

https://sms.qa1.my.91dev.tw/Api/GlobalShipping/GetCourier

```json
{
    "SalesOrderId": "15247",
    "TotalWeight": 0.005,
    "PickupDate": "2025-11-27T03:30:53.338Z"
}
```

```csharp
//// pick_country / send_country
var country = this._shopRepository.GetShopDefault(shopId, "GlobalShipping", "PickCountry")
    .ShopDefault_Value;

//// api key
var item = this._shopRepository.GetShopDefault(shopId, "GlobalShipping", "EasyParcelAPIKey");

//// pickZipcODE -- 既有 17700 -> 536828
var pickCode = this._shopRepository.GetShopDefault(shopId, "GlobalShipping", "PickZipCode")
    .ShopDefault_Value;

//// country zipCode 檔案要放在機器上 => pickState
string filePath = Path.Combine(this._configService.GetAppSetting("File.Path"), "Data", "Addresses", $"{country}.json");

<add key="QA.File.Path" value="\\SG-MY-QA1-SCM2\Storage" xdt:Transform="Insert" />

//my prod
<add key="Prod.File.Path" value="\\SG-Filer1\Storage2" xdt:Transform="Insert" />

<add key="Prod.File.Path" value="\\SG-HK-Filer1\Storage" xdt:Transform="Insert" />

<add key="Prod.File.Path" value="\\nineyi.corp\storage" xdt:Transform="Insert" />

//// sendCode
SalesOrderReceiver_ZipCode

//// sendState
ar sendState = this._globalZipCodeService.GetStateCode(country, sendCode);

//// easy authentication config
this._authentication = configService.GetAppSetting("EasyParcel.Authentication");
```
