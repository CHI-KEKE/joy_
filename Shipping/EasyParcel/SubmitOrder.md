## GlobalShipping API

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

## 行為

SubmitOrder => PayOrder
salesOrderSlaveIds => 建立 ShippingOrderSlave

shippingOrderSlave

receiver 都是從 SalesOrderReceiver 拿  

- SalesOrderReceiver_Address
- SalesOrderReceiver_Country

他會把 address 切成 4 塊, 不知道新加坡這樣切可不可行


## 錯誤紀錄


```bash
EasyParcel MPSubmitOrder Fail,MessageNow:Please fill in the details in English only.

Type  : System.Exception
Message : EasyParcel MPSubmitOrder Fail,MessageNow:Please fill in the details in English only.
Time : 11/27/2025 11:32:56 AM
Json :

System.Exception: 成立EasyParcel訂單失敗,EasyParcel MPSubmitOrder Error,[response]：{"Version":{"Major":1,"Minor":1,"Build":-1,"Revision":-1,"MajorRevision":-1,"MinorRevision":-1},"Content":{"Headers":[{"Key":"Content-Type","Value":["application/json"]},{"Key":"Expires","Value":["Thu, 19 Nov 1981 08:52:00 GMT"]}]},"StatusCode":200,"ReasonPhrase":"OK","Headers":[{"Key":"Transfer-Encoding","Value":["chunked"]},{"Key":"Connection","Value":["keep-alive"]},{"Key":"Vary","Value":["Accept-Encoding"]},{"Key":"Pragma","Value":["no-cache"]},{"Key":"Cache-Control","Value":["no-store, must-revalidate, no-cache"]},{"Key":"Date","Value":["Thu, 27 Nov 2025 03:32:55 GMT"]}],"RequestMessage":{"Version":{"Major":1,"Minor":1,"Build":-1,"Revision":-1,"MajorRevision":-1,"MinorRevision":-1},"Content":{"ObjectType":"<>f__AnonymousType115`27[[System.S
```


## Receiver


#### 後來發現 SalesOrderReciever 還是 Malaysia

TradesOrderReicer.Country => MemberLocation.CountryEnglishName
=> 會去拉過去訂單的 localReceiver

C:\91APP\Shopiing\shopping2\nine1.shopping\src\BusinessLogic\Nine1.Shopping.BL.Services\CheckoutCreateProcessor\GetLastOrderInfoProcessor.cs

MergeReceiverFromTradesOrder

#### mweb - ArrangeDataProcessor


C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\BLV2\PayProcesses\Processors\ArrangeDataProcessor.cs

ArrangeReceiver



## MPPayOrder

```json
{
  "access": "Success",
  "result": {
    "status": [
      {
        "orderno": "EI-0FPRR",
        "messagenow": "Fully Paid", /// 可譨會有大小寫
        "parcel": [
          {
            "parcelno": "EP-NIBG6",
            "awb": null,
            "awb_id_link": "https:\/\/demo.connect.easyparcel.sg\/?ac=AWBLabel&id=RVAtMjNEd3FMN0llIw%3D%3D",
            "tracking_url": "https:\/\/easyparcel.rocks\/sg\/en\/track\/details\/?courier=Mxpress&awb="
          }
        ]
      }
    ]
  },
  "api_status": "Success",
  "error_code": "0",
  "error_remark": ""
}
```