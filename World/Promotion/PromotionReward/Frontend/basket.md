

## 📋 目錄
1. [請求實體 BasketCalculateRequestEntity](#1-請求實體-basketcalculaterequestentity)
2. [資料轉換流程](#2-資料轉換流程)
   - [轉換成 SalepageSkuItemList](#轉換成-salepageskuitemlist)
   - [組成 ProcessContext](#組成-processcontext)
   - [取得菜籃群組 GetBasketGroup](#取得菜籃群組-getbasketgroup)
   - [建立群組上下文 CreateGroupContext](#建立群組上下文-creategroupcontext)
3. [計算處理流程 Calculate](#3-計算處理流程-calculate)
   - [更新處理規則清單 UpdateProcessRuleListAsync](#更新處理規則清單-updateprocessrulelistasync)
   - [更新商品項目清單 UpdateSalepageSkuItemListAsync](#更新商品項目清單-updatesalepageskuitemlistasync)
   - [更新門市標籤 UpdateLocationTagsAsync](#更新門市標籤-updatelocationtagsasync)
   - [載入規則並修改優先級 LoadRuleAndModifyPriority](#載入規則並修改優先級-loadruleandmodifypriority)
   - [建立購物車上下文 CreateShoppingCartContext](#建立購物車上下文-createshoppingcartcontext)
   - [執行購買 Purchase](#執行購買-purchase)
4. [範例資料](#4-範例資料)
   - [菜籃 Request Body JSON](#菜籃-request-body-json)
   - [菜籃 Response 回應](#菜籃-response-回應)

---

## 1. 請求實體 BasketCalculateRequestEntity

### 🔑 主要欄位

| 欄位 | 說明 |
|------|------|
| **PromotionRuleId** | 單一活動規則ID |
| **PromotionRuleIds** | 多個活動規則ID清單 |
| **PromotionRules** | 點數活動規則（需要以訂單當下版本為主，從外部傳入） |
| **CalculateDateTime** | 計算時間（點數活動需要以訂單當下時間為準） |
| **PromotionSourceType** | 促銷來源類型：Promotion / Coupon / LoyaltyPoint / ExtraPurchase / FeePromotion |

### 👤 使用者資訊 (User)

| 欄位 | 說明 |
|------|------|
| **Id** | 會員ID (MemberId) |
| **Tags** | 會員標籤（首購、會員等級） |
| **OuterId** | 會員外部編號 (VipMember上的) |
| **Channel** | 通路 |
| **CurrencyDecimalDigits** | 幣別小數位數 |

### 🛒 商品清單 (SalepageSkuList)

```json
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
}
```

---

## 2. 資料轉換流程

### 轉換成 SalepageSkuItemList

根據 Request.Qty 組成 SalepageSkuItemEntity List

#### 🏷️ 基礎欄位 (Base)
- **Index** - 索引
- **SalepageId** - 商品頁ID
- **SkuId** - SKU ID
- **OuterId** - 外部編號
- **CartId** - 購物車ID

#### 💰 價格欄位 (Price)
- **Price** = Price
- **Payment** = Price

#### 🏪 標籤欄位 (Tags)
- **Tags** - 標籤
- **Flags** - 旗標

#### 💎 點數加金 (PointsPayPair)
- **PointsPayPair** - 點數支付配對

#### ⚙️ 特殊邏輯欄位
- **OptionalTypeDef** - 可選類型定義
- **OptionalTypeId** - 可選類型ID
- **CartExtendInfoItemGroup** - 購物車擴展資訊群組
- **CartExtendInfoItemType** - 購物車擴展資訊類型
- **CartExtendInfos** - 購物車擴展資訊

### 組成 ProcessContext

| 欄位 | 說明 |
|------|------|
| **Calculate** | 放入 request entity |
| **SalepageSkuItemList** | 商品SKU清單 |
| **ShopId** | 商店ID |

### 取得菜籃群組 GetBasketGroup

🔄 **回傳**: `PromotionBasketGroup`

### GetInstance

因為
var useCustomRule = basketCalculateRequest.PromotionRules?.Any() == true;

🔄 **回傳**: `CustomRuleBasketGroup`

### 建立群組上下文 CreateGroupContext

#### 📊 GroupContext 組成
| 欄位 | 說明 |
|------|------|
| **ShopId** | 商店ID |
| **Group** | PromotionBasketGroup |
| **ProcessRuleList** | 全活動類型都拿 |
| **Calculate** | request entity |
| **SalepageSkuItemList** | 商品清單 |
| **Shipping** | 運送資訊 |


---

## 3. 計算處理流程 Calculate

### 更新處理規則清單 UpdateProcessRuleListAsync

#### 🔍 GetRuleInfoListAsync 處理步驟

1️⃣ **取得活動ID清單**
   - `Ids = 指定商品活動 Id + 指定料號活動 Id + 全站活動 Id`

2️⃣ **處理會員集合 (MemberCollection)**
   - 當月壽星解析Rule，取得當下時間並取得對應 `birthdayCollectionId`
   - 加入 MemberCollection
   - 打 match，若有中就改成 `CurrentBirthdayMonth` 貼在 UserTag

3️⃣ **設定規則清單**
   - 拿剛剛撈的 Ids 設定 `_rulesList`
   - 格式：`RuleEntity(i.PromotionEngineId, this.SourceType, i.TypeDef, i.Rule, i.PayProfileTypeDef)`

📌 **條件**: 商品與 promotion 有交集到才 `UpdateProcessRule` => 設定 `context.ProcessRuleList.RuleList`

### 更新商品項目清單 UpdateSalepageSkuItemListAsync

🏷️ **處理邏輯**: 商品頁、料號有交集的就貼 Tags

### 更新門市標籤 UpdateLocationTagsAsync

📍 **處理流程**:
- 取得 `context.Shipping.LocationId`
- 設定 `context.Shipping.LocationTags = locationTags.ToArray()`
- 比對 S3 的 Locations 資料：
  - `LocationId`
  - `TagId`

### 載入規則並修改優先級 LoadRuleAndModifyPriority

#### 🔄 LoadRules 流程
```csharp
var ruleList = context.ProcessRuleList.SelectMany(i => i.RuleList).ToList();
_promotionEngine.LoadRules(RuleLoader.AssemblyFullName, ruleList.Select(i => i.Rule).ToList());
```

📊 **引擎設定**:
- 在引擎設定 `Rules[promotionRuleBase.Id] = promotionRuleBase`
- 型別: `public IDictionary<long, PromotionRuleBase> Rules { get; private set; } = new Dictionary<long, PromotionRuleBase>();`

#### ⚡ 更新規則 Priority
- `context.ProcessRuleList.Single(i => i.Name == type).Priority => _promotionEngine.Rules.Values`

### 建立購物車上下文 CreateShoppingCartContext

| 參數 | 設定值 |
|------|--------|
| **userContext** | `new UserContext(context.Calculate.User.Id, context.Calculate.User.Tags)` |
| **locationContext** | `new LocationContext(context.Shipping.LocationId, context.Shipping.LocationTags, isLocationAlwaysMatch)` (線上訂單) |
| **Channel** | `context.Calculate.Channel` |
| **CurrencyDecimalDigits** | `context.Calculate.CurrencyDecimalDigits` |

### 執行購買 Purchase

🛒 **資料處理**: 打資料倒進 `_purchasedItems`

#### 📦 ProductItem 結構
```csharp
productItem
- Id = item.SalepageId
- SkuId = item.SkuId
- ListPrice = item.Payment
- Tags = item.Tags
```

#### 💰 計算邏輯
- `ISet<string> flags = item.Flags.ToHashSet()`
- `TotalPrice += purchasedItem.SalePrice` (SalePrice = item.ListPrice)

---

## 4. 範例資料

### 菜籃 Request Body JSON

📌 **重要說明**:
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



### 菜籃 Response 回應

#### ✅ 符合活動條件 (中活動)
```json
{
  "code": "Success",
  "data": {
    "requestId": "b1338774-07e6-4d0d-9d10-e26d7e7e4e52",
    "salepageSkuList": [
      {
        "promotionDiscount": 0,
        "promoCodeDiscount": 0,
        "designatePaymentPromotionDiscount": 0,
        "couponDiscount": 0,
        "loyaltyPointDiscount": 0,
        "additionalDiscount": 0,
        "totalDiscount": 0,
        "totalPrice": 250,
        "totalPayment": 250,
        "tags": [],
        "loyaltyPointRedeemPoint": 0,
        "salepageId": 0,
        "skuId": 0,
        "price": 50,
        "suggestPrice": 0,
        "qty": 5,
        "flags": [
          "$matched_promotion:34656"
        ],
        "outerId": null,
        "optionalTypeDef": "",
        "optionalTypeId": 0,
        "cartExtendInfoItemGroup": 85663282,
        "cartExtendInfoItemType": "TradesOrderSlaveId",
        "pointsPayPair": null,
        "cartExtendInfos": [],
        "cartId": 0
      },
      {
        "promotionDiscount": 0,
        "promoCodeDiscount": 0,
        "designatePaymentPromotionDiscount": 0,
        "couponDiscount": 0,
        "loyaltyPointDiscount": 0,
        "additionalDiscount": 0,
        "totalDiscount": 0,
        "totalPrice": 150,
        "totalPayment": 150,
        "tags": [],
        "loyaltyPointRedeemPoint": 0,
        "salepageId": 0,
        "skuId": 0,
        "price": 25,
        "suggestPrice": 0,
        "qty": 6,
        "flags": [
          "$matched_promotion:34656"
        ],
        "outerId": null,
        "optionalTypeDef": "",
        "optionalTypeId": 0,
        "cartExtendInfoItemGroup": 85663281,
        "cartExtendInfoItemType": "TradesOrderSlaveId",
        "pointsPayPair": null,
        "cartExtendInfos": [],
        "cartId": 0
      },
      {
        "promotionDiscount": 0,
        "promoCodeDiscount": 0,
        "designatePaymentPromotionDiscount": 0,
        "couponDiscount": 0,
        "loyaltyPointDiscount": 0,
        "additionalDiscount": 0,
        "totalDiscount": 0,
        "totalPrice": -150,
        "totalPayment": -150,
        "tags": [],
        "loyaltyPointRedeemPoint": 0,
        "salepageId": 0,
        "skuId": 0,
        "price": -25,
        "suggestPrice": 0,
        "qty": 6,
        "flags": [
          "$matched_promotion:34656"
        ],
        "outerId": null,
        "optionalTypeDef": "",
        "optionalTypeId": 0,
        "cartExtendInfoItemGroup": 85767129,
        "cartExtendInfoItemType": "TradesOrderSlaveId",
        "pointsPayPair": null,
        "cartExtendInfos": [],
        "cartId": 0
      }
    ],
    "promotionInstructionList": [],
    "promotionRecordList": [
      {
        "purchasedItemSkuIds": [
          0
        ],
        "sourceType": "Promotion",
        "amount": 0,
        "point": 300,
        "gifts": null,
        "couponItems": null,
        "group": "Merged",
        "id": "PR-1069",
        "notes": null,
        "promotionRuleId": 34656,
        "purchasedItemIds": [
          17,
          16,
          15,
          14,
          13,
          12,
          11,
          10,
          9,
          8,
          7,
          6,
          5,
          4,
          3,
          2,
          1
        ],
        "instructions": null,
        "needAmortization": false,
        "feeItemIds": null,
        "serialNumber": null,
        "redeemPoint": 0,
        "subItemAmount": 0,
        "subItemRedeemPoint": 0
      }
    ],
    "promotionRecordDetailList": [
      {
        "cartId": 0,
        "amount": 0,
        "point": -30,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 17,
        "purchasedItemSkuId": 0,
        "serialNumber": "85767129",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": -30,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 16,
        "purchasedItemSkuId": 0,
        "serialNumber": "85767129",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": -30,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 15,
        "purchasedItemSkuId": 0,
        "serialNumber": "85767129",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": -30,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 14,
        "purchasedItemSkuId": 0,
        "serialNumber": "85767129",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": -30,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 13,
        "purchasedItemSkuId": 0,
        "serialNumber": "85767129",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": -30,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 12,
        "purchasedItemSkuId": 0,
        "serialNumber": "85767129",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": 30,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 11,
        "purchasedItemSkuId": 0,
        "serialNumber": "85663281",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": 30,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 10,
        "purchasedItemSkuId": 0,
        "serialNumber": "85663281",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": 30,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 9,
        "purchasedItemSkuId": 0,
        "serialNumber": "85663281",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": 30,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 8,
        "purchasedItemSkuId": 0,
        "serialNumber": "85663281",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": 30,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 7,
        "purchasedItemSkuId": 0,
        "serialNumber": "85663281",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": 60,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 6,
        "purchasedItemSkuId": 0,
        "serialNumber": "85663282",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": 60,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 5,
        "purchasedItemSkuId": 0,
        "serialNumber": "85663282",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": 60,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 4,
        "purchasedItemSkuId": 0,
        "serialNumber": "85663282",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": 60,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 3,
        "purchasedItemSkuId": 0,
        "serialNumber": "85663282",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": 60,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 2,
        "purchasedItemSkuId": 0,
        "serialNumber": "85663282",
        "redeemPoint": 0
      },
      {
        "cartId": 0,
        "amount": 0,
        "point": 60,
        "group": "Merged",
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemId": 1,
        "purchasedItemSkuId": 0,
        "serialNumber": "85663282",
        "redeemPoint": 0
      }
    ],
    "promotionRecordDetailSummaryList": [
      {
        "cartId": 0,
        "amount": 0,
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "purchasedItemSkuId": 0,
        "qty": 0,
        "purchasedItemIds": []
      }
    ],
    "promotionRecordFeeDetailSummaryList": [],
    "ruleList": [
      {
        "ruleId": 34656,
        "sourceType": "LoyaltyPoint",
        "typeDef": "RewardReachPriceWithRatePoint2"
      }
    ],
    "serviceErrorList": [],
    "customizedInfo": {
      "orderMaxDiscountList": []
    },
    "loyaltyPoint": null,
    "user": {
      "id": "4190299",
      "tags": [
        "AllUserScope",
        "CrmShopMemberCard:10"
      ],
      "outerId": "5104190236",
      "shopMemberCode": "6bX6x2/YuowTUrdlVsHVSQ=="
    }
  },
  "_lvl": "Information",
  "_srctx": "Nine1.Promotion.Console.Common.Utils.Handler.LoggingHandler",
  "_lt": "Common",
  "_hid": "promotion-console-nmqv3worker-group4-864d7cb5d9-db2cb",
  "_props": {
    "HttpMethod": "POST",
    "Uri": "https://promotion-api-frontend-internal.hk.91app.io/api/basket-calculate"
  }
}
```

#### ❌ 不符合活動條件 (沒中活動)
```json
{
  "code": "Success",
  "data": {
    "requestId": "4cc55808-6f81-43be-83f9-d1f06921789e",
    "salepageSkuList": [
      {
        "promotionDiscount": 0,
        "promoCodeDiscount": 0,
        "designatePaymentPromotionDiscount": 0,
        "couponDiscount": 0,
        "loyaltyPointDiscount": 0,
        "additionalDiscount": 0,
        "totalDiscount": 0,
        "totalPrice": 250,
        "totalPayment": 250,
        "tags": [],
        "loyaltyPointRedeemPoint": 0,
        "salepageId": 0,
        "skuId": 0,
        "price": 50,
        "suggestPrice": 0,
        "qty": 5,
        "flags": [],
        "outerId": null,
        "optionalTypeDef": "",
        "optionalTypeId": 0,
        "cartExtendInfoItemGroup": 85663282,
        "cartExtendInfoItemType": "TradesOrderSlaveId",
        "pointsPayPair": null,
        "cartExtendInfos": [],
        "cartId": 0
      },
      {
        "promotionDiscount": 0,
        "promoCodeDiscount": 0,
        "designatePaymentPromotionDiscount": 0,
        "couponDiscount": 0,
        "loyaltyPointDiscount": 0,
        "additionalDiscount": 0,
        "totalDiscount": 0,
        "totalPrice": -250,
        "totalPayment": -250,
        "tags": [],
        "loyaltyPointRedeemPoint": 0,
        "salepageId": 0,
        "skuId": 0,
        "price": -50,
        "suggestPrice": 0,
        "qty": 5,
        "flags": [],
        "outerId": null,
        "optionalTypeDef": "",
        "optionalTypeId": 0,
        "cartExtendInfoItemGroup": 85767130,
        "cartExtendInfoItemType": "TradesOrderSlaveId",
        "pointsPayPair": null,
        "cartExtendInfos": [],
        "cartId": 0
      },
      {
        "promotionDiscount": 0,
        "promoCodeDiscount": 0,
        "designatePaymentPromotionDiscount": 0,
        "couponDiscount": 0,
        "loyaltyPointDiscount": 0,
        "additionalDiscount": 0,
        "totalDiscount": 0,
        "totalPrice": 150,
        "totalPayment": 150,
        "tags": [],
        "loyaltyPointRedeemPoint": 0,
        "salepageId": 0,
        "skuId": 0,
        "price": 25,
        "suggestPrice": 0,
        "qty": 6,
        "flags": [],
        "outerId": null,
        "optionalTypeDef": "",
        "optionalTypeId": 0,
        "cartExtendInfoItemGroup": 85663281,
        "cartExtendInfoItemType": "TradesOrderSlaveId",
        "pointsPayPair": null,
        "cartExtendInfos": [],
        "cartId": 0
      },
      {
        "promotionDiscount": 0,
        "promoCodeDiscount": 0,
        "designatePaymentPromotionDiscount": 0,
        "couponDiscount": 0,
        "loyaltyPointDiscount": 0,
        "additionalDiscount": 0,
        "totalDiscount": 0,
        "totalPrice": -150,
        "totalPayment": -150,
        "tags": [],
        "loyaltyPointRedeemPoint": 0,
        "salepageId": 0,
        "skuId": 0,
        "price": -25,
        "suggestPrice": 0,
        "qty": 6,
        "flags": [],
        "outerId": null,
        "optionalTypeDef": "",
        "optionalTypeId": 0,
        "cartExtendInfoItemGroup": 85767129,
        "cartExtendInfoItemType": "TradesOrderSlaveId",
        "pointsPayPair": null,
        "cartExtendInfos": [],
        "cartId": 0
      }
    ],
    "promotionInstructionList": [
      {
        "promotionRuleId": 34656,
        "sourceType": "Promotion",
        "state": {
          "fulfilment": {
            "reachPrice": 10,
            "rate": 1.2
          },
          "lackOfPrice": 10,
          "lackSalesChannel": 0
        }
      }
    ],
    "promotionRecordList": [],
    "promotionRecordDetailList": [],
    "promotionRecordDetailSummaryList": [],
    "promotionRecordFeeDetailSummaryList": [],
    "ruleList": [
      {
        "ruleId": 34656,
        "sourceType": "LoyaltyPoint",
        "typeDef": "RewardReachPriceWithRatePoint2"
      }
    ],
    "serviceErrorList": [],
    "customizedInfo": {
      "orderMaxDiscountList": []
    },
    "loyaltyPoint": null,
    "user": {
      "id": "4190299",
      "tags": [
        "AllUserScope",
        "CrmShopMemberCard:10"
      ],
      "outerId": "5104190236",
      "shopMemberCode": "6bX6x2/YuowTUrdlVsHVSQ=="
    }
  }
}

```