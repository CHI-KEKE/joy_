# Promotion FrontEnd 文件

## 目錄
1. [Domain](#1-domain)
2. [購物車計算](#2-購物車計算)
3. [菜籃計算](#3-菜籃計算)

<br>

---

## 1. Domain

測試環境 API 網址：

<br>

```
https://promotion-api-frontend-internal.qa1.hk.91dev.tw
```

<br>

---

## 2. 購物車計算

API 端點：

<br>

```
/api/cart-calculate
```

<br>

---

## 3. 菜籃計算

### Request Body 範例

注意：Qty 會被拆出來，-1 也會被改成 1

<br>

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

<br>