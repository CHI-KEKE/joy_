# Coupon 文件

## 目錄
1. [為什麼主檔 UsingEndatetime 時間很怪](#1-為什麼主檔-usingendatetime-時間很怪)
2. [CouponAPI InsertCoupon](#2-couponapi-insertcoupon)
3. [卡太久券過期](#3-卡太久券過期)

<br>

---

## 1. 為什麼主檔 UsingEndatetime 時間很怪

![alt text](./image-6.png)

<br>

因為不是預產券，所以其實要看子檔發送當下的時間作為 StartDatetime，+ 10 天作為 EndDatetime

<br>

---

## 2. CouponAPI InsertCoupon

### Payload

<br>

```json
{
    "Id": 0,
    "VerificationToken": "",
    "ShopId": 2,
    "Name": "1",
    "Description": "1",
    "PurposeDef": "Marketing",
    "TypeDef": "Code",
    "StartDateTime": "2025-06-12T16:00:00.000Z",
    "EndDateTime": "2099-12-31T15:59:00.000Z",
    "UsingStartDateTime": "2025-06-12T04:00:00.000Z",
    "UsingEndDateTime": "2025-06-13T04:00:00.000Z",
    "DiscountPrice": 1,
    "TotalDiscountPrice": 100,
    "TotalQty": 1,
    "QtyLimit": 100,
    "Code": null,
    "IsSingleCode": true,
    "CreatedDateTime": "2025-06-12T02:25:33.331Z",
    "CreatedUser": "",
    "IsAppDrawOut": true,
    "IsWebDrawOut": true,
    "IsLocationWizardDrawOut": false,
    "HasGenerateCode": false,
    "Status": "Unstarted",
    "MaxDiscountLimit": 70,
    "HasUsingMinPrice": false,
    "UsingMinPrice": 0,
    "IsOnline": true,
    "IsOffline": false,
    "BarCodeTypeDef": "",
    "IsUsingCustomVerificationCode": false,
    "CustomVerificationCodeNumber": 0,
    "IsSingleCustomVerificationCode": false,
    "SingleCustomVerificationCode": "",
    "ECouponCustomVerificationCodeSourceList": [],
    "ExchangePointCost": 0,
    "DiscountTypeDef": "ByPrice",
    "DiscountPercent": 1,
    "ECouponPromotionTagMapping": null,
    "TicketDisplayText": "1",
    "OuterPromotionCode": "",
    "Image": null,
    "CustomInfo": null,
    "ECouponMemberTier": null,
    "UseDurationType": "Relative",
    "UseAfterDay": 0,
    "UseDurationDay": 10,
    "IsOfferBySystem": true,
    "CustomSingleCode": "",
    "IsUsingCustomSingleCode": false,
    "SalePageIdList": [],
    "IsExchangeLocation": false,
    "ExchangeLocationIdList": [],
    "CalculateTypeDef": "Promotion",
    "PieceThreshold": null,
    "PromotionShippingMappingList": null,
    "MultiMonthECoupon": null,
    "IsFromDuplicate": false,
    "SourceDuplicateECouponId": 0,
    "RemindCount": 0,
    "CatalogIds": [],
    "ECouponCustomId": 0,
    "IsOuter": false,
    "OuterUrl": "",
    "IsShowTotalQty": false,
    "MaxQtyPerDispatch": 0,
    "MaxDispatchCount": 0,
    "IsTransferable": false,
    "MemberCollectionIdList": [],
    "MemberCollectionDisplayName": "",
    "OrderMaxDiscountRate": 0.5,
    "IsVisibleOnSalePage": false,
    "SalePagePromoDisplayName": "",
    "IsStackable": false,
    "CombinedPromotions": null,
    "ECouponModes": [
        "PromotionReward"
    ]
}
```

<br>

---

## 3. 卡太久券過期

發券若被退貨卡住太久可能會導致拿到券不能使用，因為排程執行時間會一直往後壓，導致 Coupon 的 UsingEndDatetime 過期

<br>