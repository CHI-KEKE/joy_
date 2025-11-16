
1. [舊活動適用門市 API](#1-舊活動適用門市-api)
2. [目標門市類型定義 TargetLocationTypeDef](#2-目標門市類型定義-targetlocationtypedef)
3. [包含目標門市ID清單 IncludeTargetLocationIdList](#3-包含目標門市id清單-includetargetlocationidlist)
4. [促購目標門市類型列舉 PromotionTargetLocationTypeDefEnum](#4-促購目標門市類型列舉-promotiontargetlocationtypedefenum)
5. [促購引擎資料庫 PromotionEngine DB](#5-促購引擎資料庫-promotionengine-db)
6. [促購標籤 PromotionTag](#6-促購標籤-promotiontag)
7. [規則 - 包含門市範圍 Rule - includedLocationScopes](#7-規則---包含門市範圍-rule---includedlocationscopes)

---

## 1. 舊活動適用門市 API

https://sms.qa1.hk.91dev.tw/Api/Location/GetListForAll


## 2. 目標門市類型定義 TargetLocationTypeDef


- 所有門市 (All)：All
- 指定門市 (Location)：Location

<br>

## 3. 包含目標門市ID清單 IncludeTargetLocationIdList

<br>

- 所有 (All)：[] (空陣列)
- 指定門市 (Location)：門市 Ids

<br>

## 4. 促購目標門市類型列舉 PromotionTargetLocationTypeDefEnum

<br>

- 所有門市 = All
- 指定門市 = Location

<br>

## 5. 促購引擎資料庫 PromotionEngine DB

<br>

PromotionEngine_TargetLocationTypeDef：活動門市類型

<br>

## 6. 促購標籤 PromotionTag

<br>

PromotionTagId：每個 LocationId 對應一個 PromotionTagSlave_TargetTypeId = targetId

<br>

## 7. 規則 - 包含門市範圍 Rule - includedLocationScopes

- TargetLocationType = All → LocationScopeType = AllLocationScope
- TargetLocationType = Location → TagLocationScope = LocationScopeType + TagId