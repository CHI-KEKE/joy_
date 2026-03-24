


https://91appinc.visualstudio.com/DailyResource/_workitems/edit/572807




## 訊息

Hi, 有訂單 MG260121M00009 煩請協助釐清這張單無法通過交易的原因

Endpoint : https://api.apaylater.com/v2/payments

## 失敗的 reuqest / response 如下 

#### request
```json
{
  "ReferenceId": "MG260121M00009",
  "MerchantReferenceId": "MG260121M00009",
  "Currency": "MYR",
  "Amount": 349.00,
  "CallbackUrl": "https://www.travelforall.my/",
  "PaymentResultUrl": "my-com-nineyi-shop-s200111://thirdpartypayconfirm?url=https%3a%2f%2fappservice2.91app.com.my%2fV2%2fPayChannel%2fDefault%2fAtome%2fMG260121M00009%3fshopId%3d200111%26k%3dbe86454b-3ea4-4913-8c5f-b679dcb49cdd%26lang%3den-US",
  "CustomerInfo": {
    "MobileNumber": "+600129916207"
  },
  "ShippingAddress": {
    "CountryCode": "MY",
    "Lines": [
      "PT 60303, Jalan KPB 2, Kawasan Perindustrian Kg. Baru Balakong, 43300, Seri Kembangan, Selangor"
    ],
    "PostCode": ""
  },
  "BillingAddress": {
    "CountryCode": "MY",
    "Lines": [
      "PT 60303, Jalan KPB 2, Kawasan Perindustrian Kg. Baru Balakong, 43300, Seri Kembangan, Selangor"
    ],
    "PostCode": ""
  },
  "Items": [
    {
      "ItemId": "MS260121M000012",
      "Name": "RCBPC Imperial II 28 Inch PP Front Opener Luggage - Expandable+Security zip+TSA - 32890228",
      "Quantity": 1,
      "Price": 349.00
    },
    {
      "ItemId": "MS260121M000013",
      "Name": "TFA CNY Ang Pao",
      "Quantity": 1,
      "Price": 0.00
    }
  ],
  "AdditionalInfo": {
    "ShopName": "Travel For All"
  }
}
```

#### response
{
  "code": "CUSTOMER_SHIPPING_ADDRESS_MISSING",
  "message": "Your request is missing customer shipping address"
}


## 成功的 request , response 如下

#### request
```json
{
  "ReferenceId": "MG250226S00009",
  "MerchantReferenceId": "MG250226S00009",
  "Currency": "MYR",
  "Amount": 512.14,
  "CallbackUrl": "https://www.lining.my/",
  "PaymentResultUrl": "my-com-nineyi-shop-s200057://PayChannelReturn?url=https%3a%2f%2fappservice2.91app.com.my%2fV2%2fPayChannel%2fDefault%2fAtome%2fMG250226S00009%3fshopId%3d200057%26k%3dd7c8ac1b-ec69-4307-8f56-022fd99901a4%26lang%3den-US",
  "CustomerInfo": {
    "MobileNumber": "+600172280715"
  },
  "ShippingAddress": {
    "CountryCode": "MY",
    "Lines": [
      "Lot 5.11.00, Level 5, Pavilion KL, 168, Jalan Bukit Bintang, Kuala Lumpur, Kuala Lumpur"
    ],
    "PostCode": ""
  },
  "BillingAddress": {
    "CountryCode": "MY",
    "Lines": [
      "Lot 5.11.00, Level 5, Pavilion KL, 168, Jalan Bukit Bintang, Kuala Lumpur, Kuala Lumpur"
    ],
    "PostCode": ""
  },
  "Items": [
    {
      "ItemId": "MS250226S000033",
      "Name": "LI-NING MEN'S DILU TRAIL RUNNING SHOES  - BLUE BLACK - ARNU003-15",
      "Quantity": 1,
      "Price": 512.14
    }
  ],
  "AdditionalInfo": {
    "ShopName": "Li Ning"
  }
}
```

#### response

```json
{
  "referenceId": "MG250226S00009",
  "currency": "MYR",
  "amount": 51214,
  "refundableAmount": 51214,
  "paymentTransaction": null,
  "refundTransactions": [],
  "status": "PROCESSING",
  "redirectUrl": "https://gateway.atome.my/en-my/payment/gateway?token=fd3383547abe400faa4c48622795d357",
  "qrCodeUrl": "https://api.apaylater.com/qr/04ea28eb0c5a4430b07de9129be2ef93",
  "appPaymentUrl": "https://app.apaylater.com/entry?q=eyJpZCI6IlBNMjFDMDAzOTAwMDFNRzI1MDIyNlMwMDAwOSIsInR5cGUiOiJQQVlfVE9fUEFZTUVOVCIsInN0YXR1cyI6IkVOQUJMRUQiLCJtZXNzYWdlIjoiIiwiY291bnRyeUNvZGUiOiJNWSIsInF1ZXJ5Ijp7InRva2VuIjoiZmQzMzgzNTQ3YWJlNDAwZmFhNGM0ODYyMjc5NWQzNTcifX0=",
  "qrCodeContent": "https://app.apaylater.com/qr/04ea28eb0c5a4430b07de9129be2ef93",
  "merchantReferenceId": "MG250226S00009",
  "additionalInfo": null
}
```

需要釐清，第一張訂單的 error message : Your request is missing customer shipping address, 原因是地址不合法嗎?




request body
```json
{
  "amount": 349.0,
  "country": "MY",
  "currency": "MYR",
  "tg_code": "MG260121L00008",
  "request_id": "9cd19a29-9598-47f0-9ef0-bb590466cdd5",
  "device": "PC",
  "platform": "Web",
  "extend_info": {
    "mobileNumber": "+601789012346",
    "shippingCountryCode": "MY",
    "shippingAddress": "PT 60303, Jalan KPB 2, Kawasan Perindustrian Kg. Baru Balakong, 43300, Seri Kembangan, Selangor",
    "shippingPostCode": "",
    "billingCountryCode": "MY",
    "billingAddress": "PT 60303, Jalan KPB 2, Kawasan Perindustrian Kg. Baru Balakong, 43300, Seri Kembangan, Selangor",
    "billingPostCode": "",
    "items": [
      {
        "itemId": "MS260121L000027",
        "name": "RCBPC Imperial II 28 Inch PP Front Opener Luggage - Expandable+Security zip+TSA - 32890228",
        "quantity": 1,
        "price": 349.0
      },
      {
        "itemId": "MS260121L000028",
        "name": "TFA CNY Ang Pao",
        "quantity": 1,
        "price": 0.0
      }
    ],
    "callbackUrl": "https://www.travelforall.my/",
    "paymentResultUrl": "https://www.travelforall.my/V2/PayChannel/Default/Atome/MG260121L00008?shopId=200111&k=df3115e6-73c2-455e-ad70-bd4a3ad460ba&lang=en-US",
    "shopName": "Travel For All"
  }
}
```

```json
{
  "amount": 349.00,
  "country": "MY",
  "currency": "MYR",
  "tg_code": "MG260121M00009",
  "request_id": "95713c73-bbc3-468b-9e74-7c9ca9a46af9",
  "device": "Mobile",
  "platform": "iOSApp",
  "shopName": "Travel For All",
  "callbackUrl": "https://www.travelforall.my/",
  "paymentResultUrl": "my-com-nineyi-shop-s200111://thirdpartypayconfirm?url=https%3a%2f%2fappservice2.91app.com.my%2fV2%2fPayChannel%2fDefault%2fAtome%2fMG260121M00009%3fshopId%3d200111%26k%3dbe86454b-3ea4-4913-8c5f-b679dcb49cdd%26lang%3den-US",
  "extend_info": {
    "mobileNumber": "+600129916207",
    "shippingCountryCode": "MY",
    "shippingAddress": "PT 60303, Jalan KPB 2, Kawasan Perindustrian Kg. Baru Balakong, 43300, Seri Kembangan, Selangor",
    "shippingPostCode": "",
    "billingCountryCode": "MY",
    "billingAddress": "PT 60303, Jalan KPB 2, Kawasan Perindustrian Kg. Baru Balakong, 43300, Seri Kembangan, Selangor",
    "billingPostCode": "",

    "items": [
      {
        "itemId": "MS260121M000012",
        "name": "RCBPC Imperial II 28 Inch PP Front Opener Luggage - Expandable+Security zip+TSA - 32890228",
        "quantity": 1,
        "price": 349.00
      },
      {
        "itemId": "MS260121M000013",
        "name": "TFA CNY Ang Pao",
        "quantity": 1,
        "price": 0.00
      }
    ]
  }
}

```


## 特殊

MG250226S00009








[MY]品牌反映當購物車選擇門市自取就無法使用Atome結賬_(200111) Travel For All


- shopId = 200111
- 選擇門市自取 => 無法使用Atome
- salepageId = 323582



```sql
use WebStoreDB

select Shop_SupplierId,*
from Shop(nolock)
where Shop_ValidFlag = 1
and Shop_Id = 200111

select Supplier_SalesMarket,Supplier_SalesCurrency,*
from Supplier(nolock)
where Supplier_ValidFlag = 1
and Supplier_Id = 200080


-- Supplier_SalesCurrency = MYR
-- Supplier_SalesMarke = MY
```


## 成功案例

```bash
{service="prod-payment-middleware"}
|json
| line_format "{{._msg}}"
|_props_ny_pay_type = `Atome`
|_props_RequestPath = `/api/v1.0/pay/Atome/MG260121L00008`
```


```json
{
  "amount": 289.71,
  "country": "MY",
  "currency": "MYR",
  "tg_code": "MG260102A00001",
  "request_id": "2fbebfd0-97ff-48dc-b5cc-7467929ba82f",
  "device": "PC",
  "platform": "Web",
  "extend_info": {
    "mobileNumber": "+600162753159",
    "shippingCountryCode": "MY",
    "shippingAddress": "554 jalan ds 1/9 bandar dataran segar, Port Dickson, Negeri Sembilan",
    "shippingPostCode": "71010",
    "billingCountryCode": "MY",
    "billingAddress": "554 jalan ds 1/9 bandar dataran segar, Port Dickson, Negeri Sembilan",
    "billingPostCode": "71010",
    "items": [
      {
        "itemId": "MS260102A000001",
        "name": "[PWP] HUMMER 50CM TRAVEL BAG-13239940",
        "quantity": 1,
        "price": 1.0
      },
      {
        "itemId": "MS260102A000002",
        "name": "FLYASIA CUBEX ABS Hard Case Luggage 3 IN 1 COMBO SET (16680020-24-28)",
        "quantity": 1,
        "price": 288.0
      }
    ],
    "callbackUrl": "https://www.travelforall.my/",
    "paymentResultUrl": "https://www.travelforall.my/V2/PayChannel/Default/Atome/MG260102A00001?shopId=200111&k=be8e2f17-9eb4-4166-a7f5-38957165ef19&lang=en-US",
    "shopName": "Travel For All"
  }
}
```


## 失敗案例

Atome Response

CUSTOMER_SHIPPING_ADDRESS_MISSING, Your request is missing customer shipping address

Request


```json
{
  "amount": 349.0,
  "country": "MY",
  "currency": "MYR",
  "tg_code": "MG260121L00008",
  "request_id": "9cd19a29-9598-47f0-9ef0-bb590466cdd5",
  "device": "PC",
  "platform": "Web",
  "extend_info": {
    "mobileNumber": "+601789012346",
    "shippingCountryCode": "MY",
    "shippingAddress": "PT 60303, Jalan KPB 2, Kawasan Perindustrian Kg. Baru Balakong, 43300, Seri Kembangan, Selangor",
    "shippingPostCode": "",
    "billingCountryCode": "MY",
    "billingAddress": "PT 60303, Jalan KPB 2, Kawasan Perindustrian Kg. Baru Balakong, 43300, Seri Kembangan, Selangor",
    "billingPostCode": "",
    "items": [
      {
        "itemId": "MS260121L000027",
        "name": "RCBPC Imperial II 28 Inch PP Front Opener Luggage - Expandable+Security zip+TSA - 32890228",
        "quantity": 1,
        "price": 349.0
      },
      {
        "itemId": "MS260121L000028",
        "name": "TFA CNY Ang Pao",
        "quantity": 1,
        "price": 0.0
      }
    ],
    "callbackUrl": "https://www.travelforall.my/",
    "paymentResultUrl": "https://www.travelforall.my/V2/PayChannel/Default/Atome/MG260121L00008?shopId=200111&k=df3115e6-73c2-455e-ad70-bd4a3ad460ba&lang=en-US",
    "shopName": "Travel For All"
  }
}
```

是少 shippingPostCode?? billingPostCode??





## cart 打 mweb 前

context.OldPayProcessContext;

context.OldPayProcessContext.Receiver

selectedReceiver = cart.DisplayReceiver.FirstOrDefault(x => x.ShippingProfileTypeDef == shippingProfileType.ToString());



        var payProcess = context.OldPayProcessContext;
        payProcess.Receiver = new MemberLocationEntity
        {

                        ZipCode = selectedReceiver?.ZipCode,


來自
checkoutContext.Data.Receiver
來自
originalCheckoutEntity
來自 cache



看 deliverySet 的 zipcode 確實沒有資料


200080
200094

在 2023 也有幾筆這種資料



跟 salpage id 無關
2025/12/05 就有遇到過


TradesOrderReceiver_StoreId	 = 1830
TradesOrderReceiver_StoreName = TFA HQ
TradesOrder_ShopId = 200111


TradesOrderSlave_ShopShippingTypeName = MINIMUM 24HRS (SMS Notify once ready)
TradesOrder_ShopShippingTypeId = 200527
TradesOrderThirdPartyPayment_CurrencyTypeDef = RM
TradesOrderReceiver_Address = PT 60303, Jalan KPB 2, Kawasan Perindustrian Kg. Baru Balakong, 43300, Seri Kembangan, Selangor
TradesOrderReceiver_CountryCode = 60
TradesOrderReceiver_ShippingAreaId = 2




## 研究購物車行為



## Webapi 取得門市列表

https://webapi2.91app.com.my/webapi/LocationV2/GetLocationListForPickup?searchKey=&isLocationPickup=true&startIndex=0&maxCount=50&areaType=0&deliveryTypeId=200370&isFilterHiddenLocation=false&lang=en-US&shopId=200017



每個門市會給
```json
{
    "Id": 2741,
    "Domestic": 1,
    "IsDomestic": true,
    "Name": "Julie測國內門市_1",
    "TelPrepend": "",
    "Tel": "",
    "Address": "test12345,123, Ayer Baloi, Johor",
    "CityId": 1,
    "CityName": "Johor",
    "AreaId": 1,
    "AreaName": "Ayer Baloi",
    "Longitude": "121.564101",
    "Latitude": "25.033493",
    "NormalTime": "00:00~22:00",
    "WeekendTime": "10:00~22:00",
    "OperationTime": "",
    "IsAvailableLocationPickup": true,
    "IsEnableRetailStore": false,
    "CountryProfileId": 30
}
```

看起來直接往後走 Delivery-set

而且看起來也不會帶 zipcode

```json
{
    "checkoutUniqueKey": "fb40494c-f6eb-4a18-841a-dce6bdb324f5",
    "checkoutShippingType": {
        "shippingProfileTypeDef": "LocationPickup",
        "shippingAreaId": 140,
        "deliveryTypeIdList": [
            200370
        ]
    },
    "receiver": {
        "addressBookId": 0,
        "addressDetail": "test12345,123, Ayer Baloi, Johor",
        "cellPhone": "1789012346",
        "city": "",
        "country": "",
        "countryCode": "60",
        "countryEnglishName": "",
        "district": "",
        "floor": "",
        "fullAddress": "",
        "fullName": "egregregeooop",
        "note": "",
        "shippingAreaId": 140,
        "shippingAreaIdList": [
            140
        ],
        "shippingProfileTypeDef": "LocationPickup",
        "state": "",
        "storeId": "2741",
        "storeName": "Julie測國內門市_1",
        "tradesOrderReceiverId": 0,
        "zipCode": ""
    }
}
```





##　delivery-set 測試





## myqa 新會員測試


#### delivery-set

```json
{
    "checkoutUniqueKey": "f7b4bc5d-4648-4ae8-bd93-0fead495790d",
    "checkoutShippingType": {
        "shippingProfileTypeDef": "LocationPickup",
        "shippingAreaId": 140,
        "deliveryTypeIdList": [
            189
        ]
    },
    "receiver": {
        "addressBookId": 0,
        "addressDetail": "柔佛州亞逸巴洛伊Jln Pontian - Batu Pahat, Kampung Pulai Sebatang, 82100 Ayer Baloi, Johor Darul Ta'zim, 馬來西亞",
        "cellPhone": "01789012347",
        "city": "",
        "country": "",
        "countryCode": "60",
        "countryEnglishName": "",
        "district": "",
        "floor": "",
        "fullAddress": "",
        "fullName": "AllenLinccc",
        "note": "",
        "shippingAreaId": 140,
        "shippingAreaIdList": [
            140
        ],
        "shippingProfileTypeDef": "LocationPickup",
        "state": "",
        "storeId": "279",
        "storeName": "露西門市",
        "tradesOrderReceiverId": 0,
        "zipCode": ""
    }
}
```





https://www.travelforall.my/V2/TradesOrder/TradesOrderList?shopId=200111





SELECT TOP (1000) *
  FROM [InfoDB].[dbo].[Location]
  where Location_Id in (2752,1830)



```sql
use WebStoreDB

select Shop_SupplierId,*
from Shop(nolock)
where Shop_ValidFlag = 1
and Shop_Id = 200111

select Supplier_SalesMarket,Supplier_SalesCurrency,*
from Supplier(nolock)
where Supplier_ValidFlag = 1
and Supplier_Id = 200080


select TradesOrderThirdPartyPayment_TradesOrderGroupCode,TradesOrderThirdPartyPayment_ResponseMsg,TradesOrderThirdPartyPayment_StatusDef,TradesOrderThirdPartyPayment_TypeDef,TradesOrderThirdPartyPayment_DateTime,*
from TradesOrderThirdPartyPayment(nolock)
where TradesOrderThirdPartyPayment_ValidFlag = 1
and TradesOrderThirdPartyPayment_ShopId= 200111
and TradesOrderThirdPartyPayment_TypeDef = 'Atome'
--and TradesOrderThirdPartyPayment_DateTime < '2026-01-18'
and TradesOrderThirdPartyPayment_DateTime > '2026-01-01'
and TradesOrderThirdPartyPayment_StatusDef in ('Success')
--AND TradesOrderThirdPartyPayment_TradesOrderGroupCode = 'MG260121L00008'

SELECT TradesOrderGroup_Code,*
FROM TradesOrderGroup(NOLOCK)
WHERE TradesOrderGroup_UniqueKey = 'df3115e6-73c2-455e-ad70-bd4a3ad460ba'


select top 100 TradesOrderReceiver_ZipCode,TradesOrderReceiver_CreatedDateTime,*
from TradesOrderReceiver(nolock)
where TradesOrderReceiver_ValidFlag = 1
and TradesOrderReceiver_DeliverTypeDef = 'LocationPickup'
and TradesOrderReceiver_Country = 'Malaysia'
and TradesOrderReceiver_CreatedDateTime > '2025-11-01'
and TradesOrderReceiver_CreatedDateTime < '2025-12-01'

select top 10 *
from TradesOrder(nolock)

select top 10 *
from TradesOrderGroup(nolock)

select TradesOrderGroup_TrackSourceTypeDef,OrderSlaveFlow_TradesOrderGroupCode,TradesOrderReceiver_StoreId,TradesOrderSlave_SalePageId,TradesOrderReceiver_StoreName,TradesOrder_ShopId,TradesOrderThirdPartyPayment_ResponseMsg,TradesOrderThirdPartyPayment_DateTime,TradesOrderThirdPartyPayment_StatusDef,TradesOrderSlave_PayProfileTypeDef,TradesOrderSlave_ShippingProfileTypeDef,*
from TradesOrderSlave(nolock)
inner join TradesOrder(nolock)
on TradesOrder_Id = TradesOrderSlave_TradesOrderId
inner JOIN TradesOrderGroup(nolock)
on TradesOrder_TradesOrderGroupId = TradesOrderGroup_Id
inner join TradesOrderThirdPartyPayment(nolock)
on TradesOrderGroup_Code = TradesOrderThirdPartyPayment_TradesOrderGroupCode
inner join TradesOrderReceiver
on TradesOrder_Id = TradesOrderReceiver_TradesOrderId
inner join OrderSlaveFlow(nolock)
on OrderSlaveFlow_TradesOrderSlaveId = TradesOrderSlave_Id
where TradesOrderSlave_ValidFlag = 1
--and TradesOrderThirdPartyPayment_TradesOrderGroupCode = 'MG251222U00003'
and TradesOrderSlave_PayProfileTypeDef = 'Atome'
and TradesOrderSlave_ShippingProfileTypeDef = 'LocationPickup'
--and TradesOrderThirdPartyPayment_DateTime > '2025-02-27'
and TradesOrderThirdPartyPayment_StatusDef = 'Success'
--and TradesOrderThirdPartyPayment_ResponseMsg = 'CUSTOMER_SHIPPING_ADDRESS_MISSING, Your request is missing customer shipping address'
--and TradesOrderReceiver_StoreId = 1830
--and TradesOrder_ShopId = 200111
--and TradesOrderThirdPartyPayment_DateTime > '2025-01-01'
--and TradesOrderThirdPartyPayment_TradesOrderGroupCode = 'MG260121L00008'
--order by TradesOrder_DateTime desc

select top 10 *
from MemberLocation(nolock)
where MemberLocation_StoreId is not null


use WebStoreDB
select top 1 *
--from location(nolock)

select top 1 ,*
from OrderSlaveFlow(nolock)
```