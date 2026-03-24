
https://sms.qa1.hk.91dev.tw/Api/Shop/GetShopList

response

```json
{
    "Status": "Success",
    "Data": [
        {
            "Key": 2,
            "Value": "測試店_香港難波萬🕊️"
        }
    ],
    "ErrorMessage": null,
    "TimeStamp": "2026-02-12T16:52:54.9459994+08:00"
}
```


```csharp
var data = this._userService.GetCurrentUser().ShopList;

var result = data.Select(i => new KeyValuePairEntity
{
    Key = i.ShopId,
    Value = i.ShopName
});
```