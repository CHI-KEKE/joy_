- Qty 會被拆出來，-1 也會被改成 1
- 只會帶一個 ruleId

```json
{
  "PromotionRuleId": 0,
  "PromotionRuleIds": [7286],
  "PromotionRules": [
    {
      "Id": 7286,
      "Type": "RewardReachPriceWithPoint2",
      "Rule": "{\"TypeFullName\":\"NineYi.Msa.Promotion.Rule.RewardReachPriceWithPoint2\",\"Id\":7286,\"Name\":\"回饋給點測試改的結果ALLEN\",\"Enabled\":true,\"Description\":\"回饋給點測試改的結果ALLEN\",\"Since\":\"2025-06-24T17:00:00+08:00\",\"Until\":\"2025-06-28T00:00:59.997+08:00\",\"UpdatedAt\":\"2025-06-24T15:33:24.443296+08:00\",\"Cyclable\":true,\"Accumulated\":false,\"IncludedProductScopes\":[{\"ProductScopeType\":\"NineYi.Msa.Promotion.Engine.AllProductScope\"}],\"ExcludedProductScopes\":null,\"IncludeRegionScopes\":[{\"RegionScopeType\":\"NineYi.Msa.Promotion.Engine.AllCountryRegionScope\"}],\"MatchedUserScopes\":[{\"UserScopeType\":\"NineYi.Msa.Promotion.Engine.AllUserScope\"}],\"VisibleUserScopes\":[{\"UserScopeType\":\"NineYi.Msa.Promotion.Engine.AllUserScope\"}],\"MatchedSalesChannels\":31,\"VisibleSalesChannels\":31,\"IncludedLocationScopes\":[{\"LocationScopeType\":\"NineYi.Msa.Promotion.Engine.AllLocationScope\"}],\"IsLimitedAddOnsPurchaseQty\":false,\"Thresholds\":{\"AllUserScope\":{\"ReachPricePointPairs\":[{\"ReachPrice\":10.0,\"Point\":6.0}]}},\"PointUntil\":{\"UntilType\":1,\"AfterDays\":10,\"UntilYearOffset\":0,\"UntilYearMonth\":12,\"FixedDate\":\"0001-01-01T00:00:00\"}}"
    }
  ],
  "CalculateDateTime": "2025-06-24T17:34:26.1891521+08:00",
  "PromotionSourceType": 0,
  "Shop": {
    "Id": 0,
    "Tags": []
  },
  "User": {
    "Id": "33671",
    "Tags": [
      "AllUserScope",
      "CrmShopMemberCard:24"
    ],
    "OuterId": "5500033674",
    "ShopMemberCode": "zoPVY9eLYe0vnBaSGUJETA=="
  },
  "Shipping": {
    "ShippingProfileTypeDef": null,
    "ShippingAreaId": 0,
    "CountryProfileId": 0,
    "LocationId": 35
  },
  "Payment": {
    "PayProfileTypeDef": null
  },
  "Channel": "InStore",
  "CurrencyDecimalDigits": 2,
  "SalepageSkuList": [
     {
      "SalepageId": 0,
      "SkuId": 0,
      "Price": 8,
      "SuggestPrice": 0,
      "Qty": 1,
      "Flags": [],
      "OuterId": "",
      "Tags": null,
      "OptionalTypeDef": "",
      "OptionalTypeId": 0,
      "CartExtendInfoItemGroup": 828042,
      "CartExtendInfoItemType": "TradesOrderSlaveId",
      "PointsPayPair": null,
      "CartExtendInfos": [],
      "CartId": 0
    },
    {
      "SalepageId": 0,
      "SkuId": 0,
      "Price": 7,
      "SuggestPrice": 0,
      "Qty": 1,
      "Flags": [],
      "OuterId": "",
      "Tags": null,
      "OptionalTypeDef": "",
      "OptionalTypeId": 0,
      "CartExtendInfoItemGroup": 828040,
      "CartExtendInfoItemType": "TradesOrderSlaveId",
      "PointsPayPair": null,
      "CartExtendInfos": [],
      "CartId": 0
    },
    {
      "SalepageId": 0,
      "SkuId": 0,
      "Price": 6,
      "SuggestPrice": 0,
      "Qty": 1,
      "Flags": [],
      "OuterId": "",
      "Tags": null,
      "OptionalTypeDef": "",
      "OptionalTypeId": 0,
      "CartExtendInfoItemGroup": 828038,
      "CartExtendInfoItemType": "TradesOrderSlaveId",
      "PointsPayPair": null,
      "CartExtendInfos": [],
      "CartId": 0
    },
    {
      "SalepageId": 0,
      "SkuId": 0,
      "Price": -6,
      "SuggestPrice": 0,
      "Qty": 1,
      "Flags": [],
      "OuterId": "",
      "Tags": null,
      "OptionalTypeDef": "",
      "OptionalTypeId": 0,
      "CartExtendInfoItemGroup": 828037,
      "CartExtendInfoItemType": "TradesOrderSlaveId",
      "PointsPayPair": null,
      "CartExtendInfos": [],
      "CartId": 0
    }
  ],
  "FeeList": [],
  "Promotion": {
    "Code": null,
    "PromoCodePoolGroupId": null,
    "SelectedDesignatePaymentPromotionId": 0
  },
  "CouponSetting": {
    "MultipleRedeem": {
      "Discount": {
        "IsMultiple": false,
        "Qty": 1
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
    "CouponList": [],
    "Options": {
      "IsVerbose": false,
      "IsCouponPreSelect": null,
      "IncludeRecordDetail": true
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

```json
{
  "promotionRules": [
    {
      "id": 34656,
      "type": "RewardReachPriceWithRatePoint2",
      "rule": {
        "TypeFullName": "NineYi.Msa.Promotion.Rule.RewardReachPriceWithRatePoint2",
        "Id": 34656,
        "Name": "(新制)滿10訂單完成後4天贈120%，最多贈2345點",
        "Enabled": true,
        "Description": "(新制)滿10訂單完成後4天贈120%，最多贈2345點",
        "Since": "2025-07-17T18:00:00+08:00",
        "Until": "2025-09-06T00:00:59.997+08:00",
        "UpdatedAt": "2025-07-17T17:40:45.9425521+08:00",
        "Cyclable": false,
        "Accumulated": false,
        "IncludedProductScopes": [
          {
            "ProductScopeType": "NineYi.Msa.Promotion.Engine.AllProductScope"
          }
        ],
        "ExcludedProductScopes": null,
        "IncludeRegionScopes": [
          {
            "RegionScopeType": "NineYi.Msa.Promotion.Engine.AllCountryRegionScope"
          }
        ],
        "MatchedUserScopes": [
          {
            "UserScopeType": "NineYi.Msa.Promotion.Engine.AllUserScope"
          }
        ],
        "VisibleUserScopes": [
          {
            "UserScopeType": "NineYi.Msa.Promotion.Engine.AllUserScope"
          }
        ],
        "MatchedSalesChannels": 31,
        "VisibleSalesChannels": 31,
        "IncludedLocationScopes": [
          {
            "LocationScopeType": "NineYi.Msa.Promotion.Engine.AllLocationScope"
          }
        ],
        "IsLimitedAddOnsPurchaseQty": false,
        "Thresholds": {
          "AllUserScope": {
            "ReachPriceRatePointPairs": [
              {
                "ReachPrice": 10.0,
                "Rate": 1.2
              }
            ],
            "MaximumPoints": 2345.0
          },
          "PointUntil": {
            "UntilType": 2,
            "AfterDays": 0,
            "UntilYearOffset": 0,
            "UntilYearMonth": 8,
            "FixedDate": "0001-01-01T00:00:00"
          },
          "PointCalculateType": 1
        }
      }
    }
  ],
  "promotionSourceType": "Promotion",
  "channel": "InStore",
  "promotionRuleIds": [
    34656
  ],
  "options": {
    "isVerbose": false,
    "isCouponPreSelect": null,
    "includeRecordDetail": true
  },
  "calculateDateTime": "2025-08-28T18:17:00+08:00",
  "shipping": {
    "shippingProfileTypeDef": null,
    "shippingAreaId": 0,
    "countryProfileId": 0,
    "locationId": 9
  },
  "isMemberCollectionRequired": false,
  "user": {
    "id": "4190299",
    "tags": [
      "AllUserScope",
      "CrmShopMemberCard:10"
    ],
    "outerId": "5104190236",
    "shopMemberCode": "6bX6x2/YuowTUrdlVsHVSQ=="
  },
  "currencyDecimalDigits": 2,
  "salepageSkuList": [
    {
      "salepageId": 0,
      "skuId": 0,
      "price": 50,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All02",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85663282,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": 50,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All02",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85663282,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": 50,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All02",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85663282,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": 50,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All02",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85663282,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": 50,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All02",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85663282,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": -50,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All02",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85767130,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": -50,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All02",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85767130,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": -50,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All02",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85767130,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": -50,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All02",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85767130,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": -50,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All02",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85767130,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": 25,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All01",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85663281,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": 25,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All01",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85663281,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": 25,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All01",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85663281,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": 25,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All01",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85663281,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": 25,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All01",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85663281,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": 25,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All01",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85663281,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": -25,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All01",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85767129,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": -25,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All01",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85767129,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": -25,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All01",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85767129,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": -25,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All01",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85767129,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    },
    {
      "salepageId": 0,
      "skuId": 0,
      "price": -25,
      "suggestPrice": 0,
      "qty": 1,
      "flags": [],
      "outerId": "All01",
      "tags": null,
      "optionalTypeDef": "",
      "optionalTypeId": 0,
      "cartExtendInfoItemGroup": 85767129,
      "cartExtendInfoItemType": "TradesOrderSlaveId",
      "pointsPayPair": null,
      "isExcludedLoyaltyPoint": false
    }
  ]
}

```