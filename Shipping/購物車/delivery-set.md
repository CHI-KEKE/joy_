## API


https://lilychuang2.shop.qa1.my.91dev.tw/shopping/api/checkout/delivery-set?lang=zh-TW&shopId=4


## payload

```json
{
    "checkoutUniqueKey": "6f5e19d2-c0bb-4a26-bbd5-4d1ef5977d2e",
    "checkoutShippingType": {
        "shippingProfileTypeDef": "Home",
        "shippingAreaId": 140,
        "deliveryTypeIdList": [
            420
        ]
    },
    "receiver": {
        "addressBookId": 799,
        "shippingProfileTypeDef": "Home",
        "fullName": "AllenLin meets",
        "cellPhone": "123456789", /////
        "countryCode": "60", /////
        "zipCode": "26900", /////
        "addressDetail": "fffffffffffffff", /////
        "city": "Bandar Tun Abdul Razak", /////
        "state": "Pahang", /////
        "district": "", /////
        "country": "", /////
        "countryEnglishName": null, /////
        "shippingAreaId": 140, /////
        "shippingAreaIdList": [ /////
            140
        ],
        "storeId": "",
        "storeName": "",
        "floor": null,
        "fullAddress": "fffffffffffffff, Bandar Tun Abdul Razak, Pahang", /////
        "note": "",
        "latitude": null,
        "longitude": null
    }
}
```


## Oversea 需要還原 ShippingArea

- 如果 `ShippingProfileTypeDef` = Oversea
- 海外配送的配送方式因為沒有 `ShippingArea` 的概念，在 `SetDelivery` 先做還原

就是把 cache 的 `selectedShippingAreaId` 塞進 `request.CheckoutShippingType.ShippingAreaId`


## 打 cart API delivery-set


#### 從 cache 拿 checkoutTypeEntity.DisplayShippingTypeList 作比對檢查

- ShippingProfileTypeDef = Home
  - DisplayShippingTypeList 是否有指定的 ShippingProfileTypeDef / ShippingAreaId
  - 其他
  - DisplayShippingTypeList 是否有指定的 ShippingProfileTypeDe

#### 購物車計算

PayCalculateAsync


