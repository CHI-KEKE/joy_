

## s3 拉 postcode 清單維度

https://s3-ap-northeast-1.amazonaws.com/91dev-ap-northeast-1-qa-mweb/MY/QA/StaticFile/Address/AdministrativeRegion_140.json


Malaysia CountryProfileId = 30

每一個 object 裡面長這樣
```json
{
    "CountryProfileAliasCode": "MY",
    "ShippingAreaAliasCode": "malaysia",
    "State": "Johor",
    "City": "Ayer Baloi",
    "District": "",
    "Postcode": "82100",
    "StateAliasCode": "",
    "Sort": 10,
    "ShippingAreaId": 140 //Malaysia
}
```

因為有拆出 postcode 維度