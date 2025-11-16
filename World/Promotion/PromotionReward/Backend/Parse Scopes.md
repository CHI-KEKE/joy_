

## 📋 目錄
1. [rule json 內容範例](#1-rule-json-內容範例)
2. [Thresholds](#2-thresholds)
3. [PromotionEngineService access rule 內容的方法](#3-promotionengineservice-access-rule-內容的方法)
4. [Item1：productScopePairs](#4-item1productscopepairs)
5. [Item2：userScopePairs](#5-item2userscopepairs)
6. [IsBirthdayMonthEnabled](#6-isbirthdaymonthenabled)
7. [Item3：rewardPointRules / rewardEcouponRules](#7-item3rewardpointrules--rewardecouponrules)
8. [Item4：rewardPointPeriodType](#8-item4rewardpointperiodtype)
9. [Item5：calculateTypeDef](#9-item5calculatetypedef)
10. [Item6：locationScopePairs](#10-item6locationscopepairs)

---

## 1. rule json 內容範例

**回饋給券**
```json
{
   "TypeFullName":"NineYi.Msa.Promotion.Rule.RewardReachPriceWithCoupon",
   "Id":9884,
   "Name":"[AutoTest]新增滿額回饋優惠券1762251152",
   "Enabled":true,
   "Description":"",
   "Since":"2025-11-05T00:00:00+08:00",
   "Until":"2025-12-04T23:59:59.997+08:00",
   "UpdatedAt":"2025-11-04T18:13:11.406475+08:00",
   "Cyclable":false,
   "Accumulated":false,
   "IncludedProductScopes":[
      {
         "ProductScopeType":"NineYi.Msa.Tagging.TagProductScope",
         "Tag":"Collection:d_376433713915106048"
      },
      {
         "ProductScopeType":"NineYi.Msa.Tagging.TagProductScope",
         "Tag":"OuterIdTag:9695"
      }
   ],
   "ExcludedProductScopes":null,
   "IncludeRegionScopes":[
      {
         "RegionScopeType":"NineYi.Msa.Promotion.Engine.AllCountryRegionScope"
      }
   ],
   "MatchedUserScopes":[
      {
         "UserScopeType":"NineYi.Msa.Tagging.TagUserScope",
         "Tag":"CrmShopMemberCard:5"
      }
   ],
   "VisibleUserScopes":[
      {
         "UserScopeType":"NineYi.Msa.Promotion.Engine.AllUserScope"
      }
   ],
   "MatchedSalesChannels":31,
   "VisibleSalesChannels":31,
   "IncludedLocationScopes":[
      {
         "LocationScopeType":"NineYi.Msa.Tagging.TagLocationScope",
         "Tag":9696
      }
   ],
   "IsLimitedAddOnsPurchaseQty":false,
   "IsBirthdayMonthEnabled":false,
   "Thresholds":{
      "CrmShopMemberCard:5":{
         "ReachPriceCouponPairs":[
            {
               "ReachPrice":100.0,
               "Coupons":[
                  {
                     "Id":"3902",
                     "Qty":1
                  }
               ]
            }
         ],
         "MaximumCouponQty":null
      }
   }
}
```

**回饋給點 (壽星)**
```json
{
   "TypeFullName":"NineYi.Msa.Promotion.Rule.RewardReachPriceWithPoint2",
   "Id":9361,
   "Name":"Jacky當月壽星活動",
   "Enabled":true,
   "Description":"",
   "Since":"2025-10-01T16:49:00+08:00",
   "Until":"2025-10-05T00:00:59.997+08:00",
   "UpdatedAt":"2025-10-01T14:36:44.0577894+08:00",
   "Cyclable":false,
   "Accumulated":false,
   "IncludedProductScopes":[
      {
         "ProductScopeType":"NineYi.Msa.Promotion.Engine.AllProductScope"
      }
   ],
   "ExcludedProductScopes":null,
   "IncludeRegionScopes":[
      {
         "RegionScopeType":"NineYi.Msa.Promotion.Engine.AllCountryRegionScope"
      }
   ],
   "MatchedUserScopes":[
      {
         "UserScopeType":"NineYi.Msa.Promotion.Engine.AllUserScope"
      },
      {
         "UserScopeType":"NineYi.Msa.Tagging.TagUserScope",
         "Tag":"CurrentBirthdayMonth"
      }
   ],
   "VisibleUserScopes":[
      {
         "UserScopeType":"NineYi.Msa.Promotion.Engine.AllUserScope"
      }
   ],
   "MatchedSalesChannels":7,
   "VisibleSalesChannels":31,
   "IncludedLocationScopes":[
      {
         "LocationScopeType":"NineYi.Msa.Promotion.Engine.AllLocationScope"
      }
   ],
   "IsLimitedAddOnsPurchaseQty":false,
   "IsBirthdayMonthEnabled":true,
   "Thresholds":{
      "AllUserScope":{
         "ReachPricePointPairs":[
            {
               "ReachPrice":1000.0,
               "Point":10.0
            }
         ],
         "MaximumPoints":null
      }
   },
   "PointUntil":{
      "UntilType":1,
      "AfterDays":100,
      "UntilYearOffset":0,
      "UntilYearMonth":12,
      "FixedDate":"0001-01-01T00:00:00"
   }
}
```


## 2. Thresholds

**全部給券**
```json
"Thresholds": {
  "AllUserScope": {
    "ReachPriceCouponPairs": [
      {
        "ReachPrice": 1.0,
        "Coupons": [
          {
            "Id": "2702",
            "Qty": 1
          }
        ]
      }
    ],
    "MaximumCouponQty": null
  }
}
```

<br>

**指定會員等級給券 (目前不支援多等級有不同門檻或一次多券)**
```json
"Thresholds": {
  "CrmShopMemberCard:5": {
    "ReachPriceCouponPairs": [
      {
        "ReachPrice": 100.0,
        "Coupons": [
          {
            "Id": "2212",
            "Qty": 1
          }
        ]
      }
    ]
  },
  "CrmShopMemberCard:32": {
    "ReachPriceCouponPairs": [
      {
        "ReachPrice": 100.0,
        "Coupons": [
          {
            "Id": "2212",
            "Qty": 1
          }
        ]
      }
    ]
  }
}
```

<br>

## 3. PromotionEngineService access rule 內容的方法

ParsePromotionEngineRuleObject


## 4. Item1：productScopePairs

<br>

- <IncludedProductScopes,TagId>
- <ExcludedProductScopes,TagId>

<br>

## 5. Item2：userScopePairs

<br>

matchedCrmShopMemberCardIds,visibleCrmShopMemberCardIds 只取 Tag 有 CrmShopMemberCard: 開頭
- <MatchedUserScopes,matchedCrmShopMemberCardIds>
- <VisibleUserScopes,visibleCrmShopMemberCardIds>

這邊不處理壽星 & AllUserScopes

## 6. IsBirthdayMonthEnabled

當月壽星活動會在 rule 的MatchedUserScopes 有多一個 ，以規則來說它就是一個交集(可能與會員等級併用)

- UserScopeType = TagUserScope
- Tag = UserTagConstants.CurrentBirthdayMonth

<br>

## 7. Item3：rewardPointRules  / rewardEcouponRules

就是 Thresholds for loop 拆出來的資料

**rewardPointRules**

不同等級可以設不同點數數量的門檻(滿多少送多少)
- CrmShopMemberCardId
- Condition (ReachPricePointPairs.ReachPrice)
- Point
- MaxRewardPoint

**rewardEcouponRules**

其實只能選一個門檻一張券，且也只有一種券
- CrmShopMemberCardId
- firstLevelRule
- EcouponId
- Qty
- MaxRewardCouponQty


<br>

## 8. Item4：rewardPointPeriodType

```JSON
"PointUntil":{
    "UntilType":1, // AfterDays = 1 / UntilTheEndOfYear / SameWithMemberAccountLevel / FixedDate / Permanent
    "AfterDays":100,
    "UntilYearOffset":0,
    "UntilYearMonth":12,
    "FixedDate":"0001-01-01T00:00:00"
}
```

<br>

## 9. Item5：calculateTypeDef

- Merged
- Independent

<br>

## 10. Item6：locationScopePairs

- <IncludedLocationScopes,TagId>