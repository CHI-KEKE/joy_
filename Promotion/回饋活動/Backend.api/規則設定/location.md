
## 📋 目錄
1. [舊活動適用門市 API](#舊活動適用門市-api)
2. [TargetLocationTypeDef](#targetlocationtypedef)
3. [IncludeTargetLocationIdList](#includetargetlocationidlist)
4. [PromotionTargetLocationTypeDefEnum](#promotiontargetlocationtypedefenum)
5. [DB](#db)
6. [PromotionTag](#promotiontag)
7. [includedLocationScopes](#includedlocationscopes)

---


## 舊活動適用門市 API

https://sms.qa1.hk.91dev.tw/Api/Location/GetListForAll

<br>
<br>

## TargetLocationTypeDef


- 所有門市 (All)：All
- 指定門市 (Location)：Location

<br>
<br>

## IncludeTargetLocationIdList

<br>

- 所有 (All)：[] (空陣列)
- 指定門市 (Location)：門市 Ids

<br>
<br>

## PromotionTargetLocationTypeDefEnum

<br>

- 所有門市 = All
- 指定門市 = Location

<br>
<br>

## DB

PromotionEngine_TargetLocationTypeDef：活動門市類型

<br>
<br>

## PromotionTag

PromotionTagId：每個 LocationId 對應一個 PromotionTagSlave_TargetTypeId = targetId

<br>
<br>

## includedLocationScopes

- TargetLocationType = All → LocationScopeType = AllLocationScope
- TargetLocationType = Location → TagLocationScope = LocationScopeType + TagId