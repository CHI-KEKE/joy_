
## bucket

TWQA : `91dev-ap-northeast-1-private-tw-osm`
HKQA : `91dev-ap-southeast-1-private-hk-osm`

## 儲存路徑

`{環境路徑}/Promotion/Reward/{ShopId}/{PromotionEngineId}/{Identity}_{PromotionEngineDateTime:yyMMddHHmmss}.json`

`91dev-ap-southeast-1-private-hk-osm/{this._environment}/Promotion/Reward/{recordData.ShopId}/{recordData.PromotionEngineId}/{recordData.Identity}_{recordData.PromotionEngineDateTime:yyMMddHHmmss}.json`

`91dev-ap-southeast-1-private-hk-osm/QA/Promotion/Reward/2/5977/2_5977_250206114332.json`

<br>

## 商品（料號）資訊

節點位置：`SaleProducts`

| 欄位名 | 說明 |
|-------|------|
| TagId | `PromotionTag ID` |
| `ProductionSkuCode` | 料號（商品 SKU 編碼） |

- `TargetType` = SalePage 且 `IncludeProductSkuOuterIdList` 有資料
- 或 `ExcludeTargetType` = SalePage 且 `ExcludeProductSkuOuterIdList` 有資料

<br>

## 門市資訊

節點位置：`Locations`

| 欄位名 | 說明 |
|-------|------|
| `TagId` | 門市標籤 ID |
| `LocationId` | 指定門市 ID |

生效條件：
- `TargetLocationType` = Location
- 且 `IncludeTargetLocationIdList` 有資料

| 類型 | 條件 | Scope 類型 |
|------|------|------------|
| 所有門市 | `TargetLocationType` = All | `AllLocationScope` |
| 指定門市 | TargetLoc`ationType = Location | `TagLocationScope` + TagId |

<br>

## Sample

```json
{
  "Identity": "2:5783",
  "ShopId": 2,
  "PromotionEngineId": 5783,
  "PromotionEngineDateTime": "2025-02-06T11:43:32.5625219+08:00",
  "StartDateTime": "2025-02-07T00:00:00",
  "EndDateTime": "2025-02-09T00:00:00",
  "Rule": "{\"TypeFullName\":\"NineYi.Msa.Promotion.Rule.RewardReachPriceWithPoint2\",\"Id\":5783,\"Name\":\"Api 新增給點活動0206_Local_測支援料號_00001\",\"Enabled\":true,\"Description\":\"xxxxxxxxxxxxxxxxxxx\",\"Since\":\"2025-02-07T00:00:00\",\"Until\":\"2025-02-09T00:00:00\",\"UpdatedAt\":\"2025-02-06T11:43:35.2962914+08:00\",\"Cyclable\":true,\"Accumulated\":false,\"IncludedProductScopes\":[{\"ProductScopeType\":\"NineYi.Msa.Tagging.TagProductScope\",\"Tag\":\"Collection:f_278128551560613120\"},{\"ProductScopeType\":\"NineYi.Msa.Tagging.TagProductScope\",\"Tag\":\"4980\"}],\"ExcludedProductScopes\":null,\"IncludeRegionScopes\":[{\"RegionScopeType\":\"NineYi.Msa.Promotion.Engine.AllCountryRegionScope\"}],\"MatchedUserScopes\":[{\"UserScopeType\":\"NineYi.Msa.Promotion.Engine.AllUserScope\"}],\"VisibleUserScopes\":[{\"UserScopeType\":\"NineYi.Msa.Promotion.Engine.AllUserScope\"}],\"MatchedSalesChannels\":31,\"VisibleSalesChannels\":31,\"IncludedLocationScopes\":[{\"LocationScopeType\":\"NineYi.Msa.Promotion.Engine.AllLocationScope\"}],\"IsLimitedAddOnsPurchaseQty\":false,\"Thresholds\":{\"CrmShopMemberCard:5\":{\"ReachPricePointPairs\":[{\"ReachPrice\":100.0,\"Point\":10.0}]},\"CrmShopMemberCard:30\":{\"ReachPricePointPairs\":[{\"ReachPrice\":200.0,\"Point\":20.0}]}],\"PointUntil\":{\"UntilType\":1,\"AfterDays\":15,\"UntilYearOffset\":0,\"UntilYearMonth\":12,\"FixedDate\":\"0001-01-01T00:00:00\"}}",
  "MemberTier": null,
  "SaleProducts": [
    {
      "TagId": 4980,
      "SalePageId": 0,
      "SaleProductSkuId": 0,
      "ProductionSkuCode": "AAA"
    },
    {
      "TagId": 4980,
      "SalePageId": 0,
      "SaleProductSkuId": 0,
      "ProductionSkuCode": "CCC"
    }
  ],
  "Locations": null,
  "PromotionType": "RewardReachPriceWithPoint2",
  "RewardPointConditions": [
    {
      "SourceTypeDef": "ECom",
      "DeliveryMethod": "Home",
      "StatusDef": "WaitingToShipping",
      "RewardDays": 0
    },
    {
      "SourceTypeDef": "ECom",
      "DeliveryMethod": "Oversea",
      "StatusDef": "WaitingToShipping",
      "RewardDays": 0
    },
    {
      "SourceTypeDef": "ECom",
      "DeliveryMethod": "LocationPickup",
      "StatusDef": "WaitingToShipping",
      "RewardDays": 0
    },
    {
      "SourceTypeDef": "Others",
      "DeliveryMethod": null,
      "StatusDef": null,
      "RewardDays": 0
    }
  ],
  "IncludeCollectionIds": [
    "Collection:f_278128551560613120"
  ],
  "ExcludeCollectionIds": null
}
```