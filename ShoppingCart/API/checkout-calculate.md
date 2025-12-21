
## path

http://cart-api-internal.qa.91dev.tw/api/checkouts/create



## Request Body

```json
{
  "IsEnableEdm": true,
  "CartUniqueKey": "ea407daa-74a7-4a21-af00-dcdbf8ae0ff9",
  "DisplayReceiver": [
    {
      "AddressBookId": 6503,
      "ShippingProfileTypeDef": "Home",
      "FullName": "Robin",
      "CellPhone": "0955325352",
      "CountryCode": "886",
      "ZipCode": "221",
      "AddressDetail": "321fdsfsdfdsf",
      "City": "新北市",
      "State": "",
      "District": "汐止區",
      "Country": "Taiwan",
      "ShippingAreaId": 1,
      "ShippingAreaIdList": [1],
      "StoreId": "",
      "StoreName": "",
      "FullAddress": "新北市汐止區321fdsfsdfdsf",
      "Note": ""
    },
    {
      "AddressBookId": 6503,
      "ShippingProfileTypeDef": "CashOnDelivery",
      "FullName": "Robin",
      "CellPhone": "0955325352",
      "CountryCode": "886",
      "ZipCode": "221",
      "AddressDetail": "321fdsfsdfdsf",
      "City": "新北市",
      "State": "",
      "District": "汐止區",
      "Country": "Taiwan",
      "ShippingAreaId": 1,
      "ShippingAreaIdList": [1],
      "StoreId": "",
      "StoreName": "",
      "FullAddress": "新北市汐止區321fdsfsdfdsf",
      "Note": ""
    },
    {
      "AddressBookId": 0,
      "ShippingProfileTypeDef": "Family",
      "FullName": "羅賓",
      "CellPhone": "0955325352",
      "CountryCode": "886",
      "ZipCode": "",
      "AddressDetail": "台北市松山區八德路二段400號",
      "City": "",
      "State": "",
      "District": "",
      "Country": "Taiwan",
      "ShippingAreaId": 1,
      "ShippingAreaIdList": [1],
      "StoreId": "011699",
      "StoreName": "全家鑫德店"
    },
    {
      "AddressBookId": 0,
      "ShippingProfileTypeDef": "FamilyPickup",
      "FullName": "羅賓",
      "CellPhone": "0955325352",
      "CountryCode": "886",
      "ZipCode": "",
      "AddressDetail": "台北市松山區八德路二段400號",
      "City": "",
      "State": "",
      "District": "",
      "Country": "Taiwan",
      "ShippingAreaId": 1,
      "ShippingAreaIdList": [1],
      "StoreId": "011699",
      "StoreName": "全家鑫德店"
    }
  ],
  "DisplayCreditCard": {
    "PaymentInfoList": [
      {
        "PayProfile": "CreditCardOnce",
        "CreditCardInfo": {
          "CardBrand": "VISA",
          "IssuerBankCode": "807",
          "Issuer": "永豐商業銀行",
          "FirstSix": "405865",
          "LastFour": "8812",
          "CountryCode": "TW",
          "CardCode": "204D70C41B8022D96469936FD8C2C5C18C3C9C3BCC0F3DB9B6A4C053FB1FBC5B",
          "ExpiryDate": "0926"
        },
        "ExtendInfo": {
          "identity": "791DCA95A5A6C3F60AF6DBCBEC4E759550470B6ED00E95093398200ADF4EF89E"
        }
      }
    ]
  },
  "IsTradesOrderMemo": false,
  "DefaultCreditCardGatewayType": "Nine1Payment",
  "MemberId": 748560,
  "Auth": "od\\u002BXMJjXjeNCLaky\\u002B2jWL5qimAiN8EzOX1LpW9PxGl5O0v3Ow9Y0Rn9xKex3iQEXzQHOUUh0NjeKd1Z5Lyg/0cmPXeG6eZfC5eiUL73OaZKzOL754gXY6fYPfUJhqEFpb1XjctY9BemrvkE7PSDvAcbBMtyQd1rIbyp\\u002BZtXjDODuegelE6Uq2CxbfQnKnlcWlqfNg8jVpD42Ok\\u002BHlKOgDMwVk2fX3QqcbeAxzg9mAcsQzu9BhFu8APC4cGOZdUweMCrlevFJSaLmsOiec7pCF4Ufa9CYQcFj...",
  "AuthSameSite": "od\\u002BXMJjXjeNCLaky\\u002B2jWL5qimAiN8EzOX1LpW9PxGl5O0v3Ow9Y0Rn9xKex3iQEXzQHOUUh0NjeKd1Z5Lyg/0cmPXeG6eZfC5eiUL73OaZKzOL754gXY6fYPfUJhqEFpb1XjctY9BemrvkE7PSDvAcbBMtyQd1rIbyp\\u002BZtXjDODuegelE6Uq2CxbfQnKnlcWlqfNg8jVpD42Ok\\u002BHlKOgDMwVk2fX3QqcbeAxzg9mAcsQzu9BhFu8APC4cGOZdUweMCrlevFJSaLmsOiec7pCF4Ufa9CYQcFj...",
  "UAuth": "Xt6PF3DOOCQJ4hA/f5nh5tL5xuSOz/qMCctbSxa/FWUegra\\u002BgxnK0uMBVXIZ/rjJaHgJpfaznODMh1V51wf/\\u002BU7zL5mZaCCWIIkmOcuQNgw=",
  "UAuthSameSite": "Xt6PF3DOOCQJ4hA/f5nh5tL5xuSOz/qMCctbSxa/FWUegra\\u002BgxnK0uMBVXIZ/rjJaHgJpfaznODMh1V51wf/\\u002BU7zL5mZaCCWIIkmOcuQNgw=",
  "Currency": "TWD",
  "UnloginId": "ba4f9444-c18e-43cd-baf1-b0cc59c7a879",
  "FMICFromType": ""
}
```

## Controllers

"ControllerShortName": "Checkout",
"ActionShortName": "Create",
"RequestPath": "/api/checkout/create",


"ControllerShortName": "Checkout",
"ActionShortName": "SetDelivery",
"RequestPath": "/api/checkout/delivery-set"
