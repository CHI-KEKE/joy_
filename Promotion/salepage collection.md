
## salepage collection

#### case 1

<br>

```
2025-09-26T03:23:42.4021843Z [Information] Start processing HTTP request "POST" https://promotion-api-frontend-internal.qa1.hk.91dev.tw/api/basket-calculate

2025-09-26T03:23:42.8605746Z [Error] 回收活動回饋優惠券 9230_CrmSalesOrder:330481 發生錯誤System.Net.Http.HttpRequestException: Response status code does not indicate success: 500 (Internal Server Error).

2025-09-26T03:23:42.8602800Z [Information] Response content: {"errorCode":"SalepageCollectionException","message":"HttpRequestException","data":"Response status code does not indicate success: 400 (Bad Request)."}
```

<br>

## 問題排查步驟

<br>

- 到 shoppingcart-loki 搜尋 salepage-service
- 打 POST {{host}}api/salepage-collections:match 測試

<br>

## 相關 API 文件

<br>

[Tag API Swagger](https://tag-api.qa1.my.91dev.tw/swagger/index.html)
[Salepage Collection SWAGGER](https://salepage-service-api-internal.qa1.hk.91dev.tw/swagger/index.html#/)




## Salepage Collections Match Log

#### 🔗 API 請求記錄
```
Start processing HTTP request \"POST\" https://salepage-service-api-internal.qa1.hk.91dev.tw/api/salepage-collections:match
.GetMatchedSalepageCollectionFromCustomRuleAsync() 回傳結果為空
```


## Case1 - 線下訂單前台 basket-calculate fail 原因

0. 10093_CrmSalesOrder:330553
1. 去 Loki-Log
2. Request Path : /api/basket-calculate
3. msg : 2 Basket Data: {"PromotionRuleId":0,"PromotionRuleIds":[10093]

可以查到該筆商店 & 活動 Id 的 Request Data
```json
2 Basket Data: {"PromotionRuleId":0,"PromotionRuleIds":[10093],"PromotionRules":[{"Id":10093,"Type":"RewardReachPriceWithPoint2","Rule":"{\u0022TypeFullName\u0022:\u0022NineYi.Msa.Promotion.Rule.RewardReachPriceWithPoint2\u0022,\u0022Id\u0022:10093,\u0022Name\u0022:\u0022[Julie\u6E2C]\u7DDA\u4E0B\u8A02\u55AE\u6EFF100\u7D66100\u9EDE\u0022,\u0022Enabled\u0022:true,\u0022Description\u0022:\u0022\u0022,\u0022Since\u0022:\u00222025-11-18T19:00:00\u002B08:00\u0022,\u0022Until\u0022:\u00222025-11-30T00:00:59.997\u002B08:00\u0022,\u0022UpdatedAt\u0022:\u00222025-11-18T17:36:08.3467211\u002B08:00\u0022,\u0022Cyclable\u0022:true,\u0022Accumulated\u0022:false,\u0022IncludedProductScopes\u0022:[{\u0022ProductScopeType\u0022:\u0022NineYi.Msa.Tagging.TagProductScope\u0022,\u0022Tag\u0022:\u0022Collection:f_381497820496017920\u0022},{\u0022ProductScopeType\u0022:\u0022NineYi.Msa.Tagging.TagProductScope\u0022,\u0022Tag\u0022:\u0022OuterIdTag:9965\u0022}],\u0022ExcludedProductScopes\u0022:null,\u0022IncludeRegionScopes\u0022:[{\u0022RegionScopeType\u0022:\u0022NineYi.Msa.Promotion.Engine.AllCountryRegionScope\u0022}],\u0022MatchedUserScopes\u0022:[{\u0022UserScopeType\u0022:\u0022NineYi.Msa.Promotion.Engine.AllUserScope\u0022}],\u0022VisibleUserScopes\u0022:[{\u0022UserScopeType\u0022:\u0022NineYi.Msa.Promotion.Engine.AllUserScope\u0022}],\u0022MatchedSalesChannels\u0022:24,\u0022VisibleSalesChannels\u0022:31,\u0022IncludedLocationScopes\u0022:[{\u0022LocationScopeType\u0022:\u0022NineYi.Msa.Promotion.Engine.AllLocationScope\u0022}],\u0022IsLimitedAddOnsPurchaseQty\u0022:false,\u0022IsBirthdayMonthEnabled\u0022:false,\u0022Thresholds\u0022:{\u0022AllUserScope\u0022:{\u0022ReachPricePointPairs\u0022:[{\u0022ReachPrice\u0022:100.0,\u0022Point\u0022:100.0}],\u0022MaximumPoints\u0022:null},\u0022PointUntil\u0022:{\u0022UntilType\u0022:5,\u0022AfterDays\u0022:0,\u0022UntilYearOffset\u0022:0,\u0022UntilYearMonth\u0022:12,\u0022FixedDate\u0022:\u00229999-12-31T23:59:59.9999999\u0022}}"}],"CalculateDateTime":"2025-11-19T10:00:00","PromotionSourceType":0,"IsMemberCollectionRequired":true,"Shop":{"Id":0,"Tags":[]},"User":{"Id":"34508","Tags":["AllUserScope","CrmShopMemberCard:5"],"OuterId":"5600034580","ShopMemberCode":"I7cS2/lqgTzUqEeCOdSEZQ=="},"Shipping":{"ShippingProfileTypeDef":null,"ShippingAreaId":0,"CountryProfileId":0,"LocationId":57},"Payment":{"PayProfileTypeDef":null,"MultiPayItems":[]},"Channel":"InStore","CurrencyDecimalDigits":2,"SalepageSkuList":[{"SalepageId":0,"SkuId":0,"Price":200,"SuggestPrice":0,"Qty":1,"Flags":[],"OuterId":"case1","Tags":null,"OptionalTypeDef":"","OptionalTypeId":0,"CartExtendInfoItemGroup":829637,"CartExtendInfoItemType":"TradesOrderSlaveId","PointsPayPair":null,"CartExtendInfos":[],"CartId":0},{"SalepageId":0,"SkuId":0,"Price":-200,"SuggestPrice":0,"Qty":1,"Flags":[],"OuterId":"case1","Tags":null,"OptionalTypeDef":"","OptionalTypeId":0,"CartExtendInfoItemGroup":829639,"CartExtendInfoItemType":"TradesOrderSlaveId","PointsPayPair":null,"CartExtendInfos":[],"CartId":0}],"FeeList":[],"Promotion":{"Code":null,"PromoCodePoolGroupId":null,"SelectedDesignatePaymentPromotionId":0},"CouponSetting":{"MultipleRedeem":{"Discount":{"IsMultiple":false,"Qty":1},"Gift":{"IsMultiple":true,"Qty":9999},"Shipping":{"IsMultiple":false,"Qty":1}},"CouponList":[],"Options":{"IsVerbose":false,"IsCouponPreSelect":null,"IncludeRecordDetail":true},"LoyaltyPoint":{"CheckoutPoint":0,"CheckoutDiscountPrice":null,"IsSelected":false,"IsSetDiscountPrice":false,"TotalPoint":0}}
```

```json
{
  "PromotionRuleId": 0,
  "PromotionRuleIds": [10093],
  "PromotionRules": [
    {
      "Id": 10093,
      "Type": "RewardReachPriceWithPoint2",
      "Rule": {
        "TypeFullName": "NineYi.Msa.Promotion.Rule.RewardReachPriceWithPoint2",
        "Id": 10093,
        "Name": "[Julie測]線下訂單滿100送100點",
        "Enabled": true,
        "Description": "",
        "Since": "2025-11-18T19:00:00+08:00",
        "Until": "2025-11-30T00:00:59.997+08:00",
        "UpdatedAt": "2025-11-18T17:36:08.3467211+08:00",
        "Cyclable": true,
        "Accumulated": false,
        "IncludedProductScopes": [
          {
            "ProductScopeType": "NineYi.Msa.Tagging.TagProductScope",
            "Tag": "Collection:f_381497820496017920"
          },
          {
            "ProductScopeType": "NineYi.Msa.Tagging.TagProductScope",
            "Tag": "OuterIdTag:9965"
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
        "MatchedSalesChannels": 24,
        "VisibleSalesChannels": 31,
        "IncludedLocationScopes": [
          {
            "LocationScopeType": "NineYi.Msa.Promotion.Engine.AllLocationScope"
          }
        ],
        "IsLimitedAddOnsPurchaseQty": false,
        "IsBirthdayMonthEnabled": false,
        "Thresholds": {
          "AllUserScope": {
            "ReachPricePointPairs": [
              {
                "ReachPrice": 100.0,
                "Point": 100.0
              }
            ],
            "MaximumPoints": null
          },
          "PointUntil": {
            "UntilType": 5,
            "AfterDays": 0,
            "UntilYearOffset": 0,
            "UntilYearMonth": 12,
            "FixedDate": "9999-12-31T23:59:59.9999999"
          }
        }
      }
    }
  ],
  "CalculateDateTime": "2025-11-19T10:00:00",
  "PromotionSourceType": 0,
  "IsMemberCollectionRequired": true,
  "Shop": {
    "Id": 0,
    "Tags": []
  },
  "User": {
    "Id": "34508",
    "Tags": ["AllUserScope", "CrmShopMemberCard:5"],
    "OuterId": "5600034580",
    "ShopMemberCode": "I7cS2/lqgTzUqEeCOdSEZQ=="
  },
  "Shipping": {
    "ShippingProfileTypeDef": null,
    "ShippingAreaId": 0,
    "CountryProfileId": 0,
    "LocationId": 57
  },
  "Payment": {
    "PayProfileTypeDef": null,
    "MultiPayItems": []
  },
  "Channel": "InStore",
  "CurrencyDecimalDigits": 2,
  "SalepageSkuList": [
    {
      "SalepageId": 0,
      "SkuId": 0,
      "Price": 200,
      "SuggestPrice": 0,
      "Qty": 1,
      "Flags": [],
      "OuterId": "case1",
      "Tags": null,
      "OptionalTypeDef": "",
      "OptionalTypeId": 0,
      "CartExtendInfoItemGroup": 829637,
      "CartExtendInfoItemType": "TradesOrderSlaveId",
      "PointsPayPair": null,
      "CartExtendInfos": [],
      "CartId": 0
    },
    {
      "SalepageId": 0,
      "SkuId": 0,
      "Price": -200,
      "SuggestPrice": 0,
      "Qty": 1,
      "Flags": [],
      "OuterId": "case1",
      "Tags": null,
      "OptionalTypeDef": "",
      "OptionalTypeId": 0,
      "CartExtendInfoItemGroup": 829639,
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

4. 線下訂單 salepageId = 0, 打 salepage collection 應該就不中而已
5. 料號用 OuterId 看
6. 2025-11-20T03:09:59.6913186Z [Information] Response content: {"errorCode":"SalepageCollectionException","message":"HttpRequestException","data":"Response status code does not indicate success: 400 (Bad Request)."}
7. 