
## 訂單列表 API

https://mytestmina.shop.qa1.my.91dev.tw/webapi/MemberTradesOrder/GetList?shopId=83&startIndex=0&maxCount=5&lang=zh-TW



## Rewrite


```csharp
  <rule name="MemberTradesOrder Swtich to Lite" stopProcessing="true">
    <match url="webapi\/MemberTradesOrder\/(GetList|GetArchiveList|GetMemberPromptList)$"/>
    <action type="Rewrite" url="webapi/MemberTradesOrderLite/{R:1}" appendQueryString="true" />
  </rule>
```


## 相關節點

`"OrderSlaveFlowShippingOrderSlaveOuterCode"`: "EI-0FPRR"

TradesOrderGroupList >> TradesOrderList >> TradesOrderSlaveList >> OrderSlaveFlowShippingOrderSlaveOuterCode


`"CurrentStatusQueryUrl"`: "/V2/TradesOrder/RedirectToEasyParcel?orderNo=EI-0FPRR&shopId=83"


## 資料寫入的地方

後台出貨完成後建立 HomeDelivery ShippingOrder

```csharp
var createEntity = new Scm.Api.Models.ShippingOrders.HomeDeliveryShippingOrderCreateEntity
{
    TMCode = entity.TradesOrderCode,
    ShippingOrderCode = grpPack.Key.OuterCode,
    ForwarderDef = grpPack.Key.ForwarderDef,
    ShopId = entity.ShopId,
    TSCodeList = grpPack.Select(i => i.TradesOrderSlaveCode).ToList(),
    ShippingType = entity.ShippingType
};
```

## 怎麼判斷要換那些訂單?

1. OrderSlaveFlowShippingOrderForwarderDef 

```csharp
var forwarderList = this.DefinitionService.GetDefinitionList("ShippingOrder", "ShippingOrder_ForwarderDef", cleanCache);
var forwarder = forwarderList.Where(i => i.Code == paramsEntity.OrderSlaveFlowShippingOrderForwarderDef).FirstOrDefault();

/// <summary>
/// 判斷是否為 EasyParcel (MY)
/// </summary>
/// <param name="forwarder">DefinitionV2Entity</param>
/// <returns>是否為 EasyParcel</returns>
private bool IsEasyParcel(BE.DefinitionV2.DefinitionV2Entity forwarder)
{
    if (SettingHelper.DefaultCountry == "MY" && forwarder.Code == "9")
    {
        return true;
    }
    else
    {
        return false;
    }
}
```

2. ShippingProfileTypeDef == home
3. salesmarket = sg or my
4. defaultMarket = "MY"