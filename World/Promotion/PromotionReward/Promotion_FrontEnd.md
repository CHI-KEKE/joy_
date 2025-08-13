# Promotion FrontEnd 文件

## 目錄
1. [Domain](#1-domain)
2. [購物車計算](#2-購物車計算)
3. [菜籃計算](#3-菜籃計算)
4. [生日壽星貼標](#4-生日壽星貼標)
5. [加價購](#5-加價購)
6. [回饋活動加入全新的活動類型需實作](#6-回饋活動加入全新的活動類型需實作)

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

---

## 4. 生日壽星貼標

### DDB Table

<br>

**Table Name:** HK_QA_OSM_MemberCollectionMapping

<br>

**Key:** System_BirthdayMonth_5

<br>

**說明:** 該 shop 的 12 個 Birthday CollectionIds

<br>

**組成:** Name / Month / CollectionId

<br>

### 程式碼路徑

<br>

```
C:\91APP\Promotion\frontEnd\nine1.promotion.web.api.frontend\src\BusinessLogic\Nine1.Promotion.BL.Services\Rules\Repositories\PromotionRuleRepository.cs
```

<br>

### GetRuleInfoListAsync

<br>

以活動是否有 "rule.IsBirthdayMonthEnabled" 來確認是否為當月壽星

<br>

promotionCollectionId + birthdayCollectionId + memberId 送去打 memberCollection

<br>

有 match 的會貼標在 context.Calculate.User.Tags

<br>

壽星要置換成純文字 CurrentBirthdayMonth

<br>

### 時間判斷

<br>

- **cart-calculate:** context.Cart.Now.Month
- **basket-calculate:** basketCalculateRequest.CalculateDateTime.Value

<br>

### MatchedUserScopes 範例

<br>

```json
"MatchedUserScopes": [
  {
    "UserScopeType": "NineYi.Msa.Promotion.Engine.AllUserScope"
  },
  {
    "UserScopeType": "NineYi.Msa.Tagging.TagUserScope",
    "Tag": "CurrentBirthdayMonth"
  }
]
```

<br>

### 引擎判斷邏輯

<br>

引擎會看 IsBirthdayMonthEnabled + CurrentBirthdayMonth有貼才中

<br>

---

## 5. 加價購

### 5.1 Request 處理流程

**request 進來**

<br>

```csharp
entity.SalepageSkuList
```

<br>

**mapping ProcessContext**

<br>

```csharp
salePageSkuItemList.Add(
    new SalepageSkuItemEntity
    {
        Index = index++,
        SalepageId = salePage.SalepageId,
        SkuId = salePage.SkuId,
        Price = salePage.Price,
        Payment = salePage.Price,
        // ...
    });
```

<br>

**建立 CreateShoppingCartContext**

<br>

```csharp
foreach (var item in context.SalepageSkuItemList)
{
    var productItem = new ProductItem
    {
        Id = item.SalepageId,
        SkuId = item.SkuId,
        ListPrice = item.Payment,
        // ...
    };
}
```

<br>

**引擎作法**

<br>

```csharp
public bool Purchase(long id, ProductItem item, ISet<string> flags = null)
{
    PurchasedItem purchasedItem = new PurchasedItem(id, item, flags)
    {
        SalePrice = item.ListPrice,
        // ...
    };
}
```

<br>

### 5.2 商品範例

**加購品：** 60393 貓腿

<br>

**主商品：** 62227 有 SKU

<br>

**當下的 Request Data**

<br>

- **主商品：** 4.5 * 14 = 63，Flag：`"AddOnsSalepageMajor"`
- **加購品：** 6.66，Flags：`"AddOnsSalepageSub"`

<br>

### 5.3 完整 Request 範例

```json
{
  "Shop": {
    "Id": 11,
    "Tags": ["EnableAddOns"]
  },
  "User": {
    "Id": "33502",
    "Tags": [
      "AllUserScope",
      "CrmShopMemberCard:24",
      "FirstPurchase"
    ],
    "OuterId": null,
    "ShopMemberCode": "97+Gy73RMbUYz5ZqI4EuEA=="
  },
  "Shipping": {
    "ShippingProfileTypeDef": "Home",
    "ShippingAreaId": 0,
    "CountryProfileId": 85,
    "LocationId": 0
  },
  "Payment": {
    "PayProfileTypeDef": "TwoCTwoP"
  },
  "Channel": "AppIOS",
  "CurrencyDecimalDigits": 2,
  "SalepageSkuList": [
    {
      "SalepageId": 62227,
      "SkuId": 86642,
      "Price": 4.5,
      "SuggestPrice": 10000.0,
      "Qty": 14,
      "Flags": ["AddOnsSalepageMajor"],
      "OuterId": "123",
      "Tags": null,
      "OptionalTypeDef": "",
      "OptionalTypeId": 0,
      "CartExtendInfoItemGroup": 1748912653812,
      "CartExtendInfoItemType": "Major",
      "PointsPayPair": null,
      "CartExtendInfos": [
        {
          "RuleTypeDef": "AddOnsSalepageExtraPurchase",
          "RuleId": 10000050,
          "RelatedItemCartIds": [45514],
          "RelatedSubItemCount": 0
        }
      ],
      "CartId": 45513
    },
    {
      "SalepageId": 60393,
      "SkuId": 84109,
      "Price": 6.66,
      "SuggestPrice": 100.0,
      "Qty": 1,
      "Flags": ["AddOnsSalepageSub"],
      "OuterId": "",
      "Tags": null,
      "OptionalTypeDef": "",
      "OptionalTypeId": 0,
      "CartExtendInfoItemGroup": 1748912653812,
      "CartExtendInfoItemType": "Sub",
      "PointsPayPair": null,
      "CartExtendInfos": [
        {
          "RuleTypeDef": "AddOnsSalepageExtraPurchase",
          "RuleId": 10000050,
          "RelatedItemCartIds": [],
          "RelatedSubItemCount": 0
        }
      ],
      "CartId": 45514
    }
  ],
  "FeeList": [
    {
      "Id": 221,
      "Type": "ShippingFee",
      "Price": 0,
      "Payment": 0,
      "ExtendInfo": {
        "ShippingProfileTypeDef": "Home",
        "IsDomesticWeightPricing": false,
        "TemperatureTypeDef": "Normal",
        "ShippingType": "221",
        "ShippingAreaId": 84,
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
      "IsCouponPreSelect": true,
      "IncludeRecordDetail": false
    },
    "LoyaltyPoint": {
      "CheckoutPoint": 0,
      "CheckoutDiscountPrice": 0,
      "IsSelected": false,
      "IsSetDiscountPrice": false,
      "TotalPoint": 0.0
    }
  }
}
```

<br>

### 5.4 Collection 資訊

**62227：**

<br>

- `"Collection:d_320231983465890560"`
- `"Collection:d_320479663883691264"`

<br>

**60393：**

<br>

- `"Collection:d_320231983465890560"`
- `"Collection:d_320479663883691264"`

<br>

### 5.5 排除邏輯實作

**RuleInitProcess**

<br>

```csharp
/// <summary>
/// The RuleInitProcess
/// </summary>
public override void RuleInitProcess()
{
    this.ExclusiveTags ??= new HashSet<string>();
    this.ExclusiveTags.Add(FlagConstants.AddOnsSalepageSub);
    this.ExclusiveTags.Add(FlagConstants.CouponExcludedByOrder);
}
```

<br>

也就是說促購前台有一步 LoadRules，每個活動 Init 流程實作需要排除哪些 Flag 的商品

<br>

**PurchasedItems 排除機制**

<br>

PurchasedItems 會需要把該 Tags vs Flag 交集有結果要排除掉

<br>

```csharp
/// <summary>
/// Gets the PurchasedItems
/// </summary>
public IEnumerable<PurchasedItem> PurchasedItems =>
    this._currentRule == null ?
        this._purchasedItems :
        this._purchasedItems.Where(x => ((this._currentRule.ExclusiveTags ?? new HashSet<string>()).Union(this.ExclusiveTags))
                                       .Intersect(x.Flags).Any() == false
                                        && (this._currentRule.GlobalExcludedProductIds == null ? true : this._currentRule.GlobalExcludedProductIds.Contains(x.Product.Id) == false)
                                   );
```

<br>

---

## 6. 回饋活動加入全新的活動類型需實作

新增回饋活動類型時需要實作的檔案列表：

<br>

- **PromotionConditionTypeEnum.cs**
- **PromotionEngineTypeDefEnum.cs**
- **PromotionEngineForCartEntity.cs**
- **PromotionEngineRuleEntity.cs**
- **新增 PromotionRewardCouponRuleEntity.cs**
- **套件升級**
- **ProcessRepository.cs**
- **S3LocationRepository.cs**
- **PromotionEngineService.cs**
- **新增 RewardReachPriceWithCouponRuleService.cs**
- **PromotionRuleRepository.cs**
- **Program.cs**
- **PromotionTagOuterIdRepository.cs**
- **S3OuterIdRepository.cs**

<br>