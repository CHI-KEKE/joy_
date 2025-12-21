

## VSTS

https://91appinc.visualstudio.com/G11n/_workitems/edit/546605

調整 Easy Parcel API 參數支援新加坡出貨
訂單管理 > 宅配訂單管理 > 訂單出貨 > Easy Parcel 出貨



##　easyParcel Account


#### Sandbox 帳號

Demo URL：https://demo.connect.easyparcel.sg/
API Key：可同時用於測試環境和正式環境
DEMO Auth Key: mgZBJbN5ns

#### Prod 帳號

https://easyparcel.com/sg/
ID: williamteoh@exabytes.com
Password: 91App@2025
Live URL：https://connect.easyparcel.sg/
API Key：EP-23DwqL7Ie
Live Auth Key: VsSiP6LzsT


## 後台 - 依據日期取得物流商

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

//// 從 SalesOrderReceiver 拿 receiver

use ERPDB

select *
from SalesOrderReceiver(nolock)
where SalesOrderReceiver_ValidFlag = 1
and SalesOrderReceiver_SalesOrderId = 15247;


select SalesOrderGroup_Id,*
from SalesOrderGroup(nolock)
where SalesOrderGroup_ValidFlag = 1
and SalesOrderGroup_TradesOrderGroupCode = 'MG251126U00005'

select *
from SalesOrder(nolock)
where SalesOrder_ValidFlag = 1
and SalesOrder_SalesOrderGroupId = 15252

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






#### 出貨完成

https://sms.qa1.my.91dev.tw/Api/GlobalShipping/CreateShippingOrder
```json
{
    "CollectDate": "2025/11/27",
    "Content": "回饋活動好難",
    "ServiceCode": "EP-CS0ADJ",
    "Value": 61.7,
    "Weight": 0.005,
    "PackingList": [
        {
            "TradesOrderSlaveCode": "MS251126U000005",
            "SalesOrderSlaveId": 23535,
            "ShippingType": 0
        }
    ],
    "SalesOrderId": "15247",
    "ShopId": 4,
    "TradesOrderCode": "MM251126U00005"
}
```



**錯誤1**


EasyParcel MPSubmitOrder Fail,MessageNow:Please fill in the details in English only.

Type  : System.Exception
Message : EasyParcel MPSubmitOrder Fail,MessageNow:Please fill in the details in English only.
Time : 11/27/2025 11:32:56 AM
Json :

System.Exception: 成立EasyParcel訂單失敗,EasyParcel MPSubmitOrder Error,[response]：{"Version":{"Major":1,"Minor":1,"Build":-1,"Revision":-1,"MajorRevision":-1,"MinorRevision":-1},"Content":{"Headers":[{"Key":"Content-Type","Value":["application/json"]},{"Key":"Expires","Value":["Thu, 19 Nov 1981 08:52:00 GMT"]}]},"StatusCode":200,"ReasonPhrase":"OK","Headers":[{"Key":"Transfer-Encoding","Value":["chunked"]},{"Key":"Connection","Value":["keep-alive"]},{"Key":"Vary","Value":["Accept-Encoding"]},{"Key":"Pragma","Value":["no-cache"]},{"Key":"Cache-Control","Value":["no-store, must-revalidate, no-cache"]},{"Key":"Date","Value":["Thu, 27 Nov 2025 03:32:55 GMT"]}],"RequestMessage":{"Version":{"Major":1,"Minor":1,"Build":-1,"Revision":-1,"MajorRevision":-1,"MinorRevision":-1},"Content":{"ObjectType":"<>f__AnonymousType115`27[[System.S



shippingOrderSlave

receiver 都是從SalesOrderReceiver 拿  

- SalesOrderReceiver_Address
- SalesOrderReceiver_Country

他會把 address 切成 4 塊, 不知道新加坡這樣切可不可行

## 後來發現 SalesOrderReciever 還是 Malaysia

TradesOrderReicer.Country => MemberLocation.CountryEnglishName
=> 會去拉過去訂單的 localReceiver

C:\91APP\Shopiing\shopping2\nine1.shopping\src\BusinessLogic\Nine1.Shopping.BL.Services\CheckoutCreateProcessor\GetLastOrderInfoProcessor.cs

MergeReceiverFromTradesOrder




## mweb - ArrangeDataProcessor


C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\BLV2\PayProcesses\Processors\ArrangeDataProcessor.cs

ArrangeReceiver



## EasyParcel 按鈕會不會出現

C:\91APP\NineYi.Sms\WebSite\WebSite\Controllers\Apis\GlobalShippingController.cs

var shop = this._userService.GetCurrentUser().ShopList.First(s => s.ShopId == shopId);
var key = this._shopRepository.GetShopDefault(shop.ShopId, "GlobalShipping", "EasyParcelAPIKey")?.ShopDefault_Value;
return new Dictionary<string, object>
{
    { "EnableGlobalShipping", string.IsNullOrEmpty(key) == false }
};

            有 key 應該就會有










## keywords


- SubmitOrderAddonEntity
- SubmitOrderBulkResponseEntity
- SubmitOrderResultEntity



目前 SubmitOrder 已知的 新舊 API 差異 (跟三方金物流存的資料有關係, 還有實際測試送貨會有關係)
商店連絡電話會檢查前綴 +65
新加坡多的必填節點
Pick Unit
提貨點資訊現在是存 ShopDefault
PickContact
PickAddress
PickZipCode
Pick Unit   => 沒有這個, 我會從 PickAddress 找,  沒就帶 "N/A" 送去 easyParcel
Send Unit
送貨點資訊會拿 SalesOrderReceiver 的資料表的資料, 目前沒特別存 Send Unit 的資料,我會從  send address 找, 沒就 "N/A"  送去 easyParcel




Easy-Parcel 跨境配送要綁卡片


TW - HH

TW 環境 MY 銷售市場 只能使用國家地區配送

TW -> MY


Easy-Parcel 目前只有 MY 環境開放, 因此 HH 長不出來
MY 就限制境內配送



## 取得配或編號


### NMQ

MPPayOrder

this.MailDataEntity.OuterCode = mpPayOrderEntity.result.status[0].parcel[0].awb;


### OSM

