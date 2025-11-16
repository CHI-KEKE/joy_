
## Request Data 範例

- Channel

```JSON
{
  "Shop": {
    "Id": 2,
    "Tags": [
      "EnableAddOns"
    ]
  },
  "User": {
    "Id": "32905",
    "Tags": [
      "AllUserScope",
      "CrmShopMemberCard:31"
    ],
    "OuterId": "tina20241018",
    "ShopMemberCode": "qDDkYy5/lcEcdXEj9YTfXg=="
  },
  "Shipping": {
    "ShippingProfileTypeDef": "SFLocker",
    "ShippingAreaId": 0,
    "CountryProfileId": 85,
    "LocationId": 0
  },
  "Payment": {
    "PayProfileTypeDef": "CreditCardOnce_Cybersource",
    "MultiPayItems": []
  },
  "Channel": "Web",
  "CurrencyDecimalDigits": 2,
  "SalepageSkuList": [
    {
      "SalepageId": 60366,
      "SkuId": 84072,
      "Price": 10.00,
      "SuggestPrice": 111.00,
      "Qty": 1,
      "Flags": [],
      "OuterId": "",
      "Tags": null,
      "OptionalTypeDef": "",
      "OptionalTypeId": 0,
      "CartExtendInfoItemGroup": 0,
      "CartExtendInfoItemType": "Major",
      "PointsPayPair": null,
      "CartExtendInfos": [],
      "CartId": 52407
    }
  ],
  "FeeList": [
    {
      "Id": 474,
      "Type": "ShippingFee",
      "Price": 0,
      "Payment": 0,
      "ExtendInfo": {
        "ShippingProfileTypeDef": "SFLocker",
        "IsDomesticWeightPricing": false,
        "TemperatureTypeDef": "Normal",
        "ShippingType": "474",
        "ShippingAreaId": 0,
        "IsLocal": true
      }
    }
  ],
  "Promotion": {
    "Code": null,
    "PromoCodePoolGroupId": null,
    "SelectedDesignatePaymentPromotionId": 0
  },
  "CouponSetting": {
    "MultipleRedeem": {
      "Discount": {
        "IsMultiple": true,
        "Qty": 10
      },
      "Gift": {
        "IsMultiple": true,
        "Qty": 9999
      },
      "Shipping": {
        "IsMultiple": false,
        "Qty": 1
      }
    },
    "CouponList": [
      {
        "CouponId": 3290,
        "Tags": null,
        "RedeemStartDateTime": "2025-09-29T00:00:00",
        "RedeemEndDateTime": "2026-09-28T23:59:59",
        "IsSelected": true,
        "GlobalExcludedSalepageIds": null,
        "CouponSlaveId": 75885382,
        "OrderMaxDiscountRate": 0.00,
        "IsOverOrderMaxDiscount": false
      },
      {
        "CouponId": 3290,
        "Tags": null,
        "RedeemStartDateTime": "2025-10-25T00:00:00",
        "RedeemEndDateTime": "2026-10-24T23:59:59",
        "IsSelected": false,
        "GlobalExcludedSalepageIds": null,
        "CouponSlaveId": 75888023,
        "OrderMaxDiscountRate": 0.00,
        "IsOverOrderMaxDiscount": false
      },
      {
        "CouponId": 2307,
        "Tags": null,
        "RedeemStartDateTime": "2025-09-26T00:00:00",
        "RedeemEndDateTime": "2025-11-24T23:59:59",
        "IsSelected": false,
        "GlobalExcludedSalepageIds": null,
        "CouponSlaveId": 75885106,
        "OrderMaxDiscountRate": 0.00,
        "IsOverOrderMaxDiscount": false
      }
    ],
    "Options": {
      "IsVerbose": false,
      "IsCouponPreSelect": false,
      "IncludeRecordDetail": false
    },
    "LoyaltyPoint": {
      "CheckoutPoint": 0,
      "CheckoutDiscountPrice": null,
      "IsSelected": false,
      "IsSetDiscountPrice": false,
      "TotalPoint": 0
    }
  }
}
```