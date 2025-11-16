
1. [通路類型對應表](#1-通路類型對應表)
2. [資料存放位置](#2-資料存放位置)
3. [促購後台類型](#3-促購後台類型)
4. [平台類型字典 GetPlatformTypeDiction](#4-平台類型字典-getplatformtypediction)
5. [促購引擎設定 PromotionEngineSetting](#5-促購引擎設定-promotionenginesetting)
6. [促購引擎規則 PromotionEngine Rule](#6-促購引擎規則-promotionengine-rule)
7. [可視通路邻輯](#7-可視通路邏輯)
8. [匹配銷售通路 MatchedSalesChannels](#8-匹配銷售通路-matchedsaleschannels)
9. [S3 儲存](#9-s3-儲存)
10. [銷售通路列舉 SalesChannelEnum](#10-銷售通路列舉-saleschannelenum)
11. [規則解析](#11-規則解析)
12. [其他規則限制](#12-其他規則限制)

---

## 1. 通路類型對應表

SMS 使用 ChannelType 溝通通路，促購後台拆解如下

<br>

| 通路類型 | Web | AppAndroid | AppIOS | LocationWizard | InStore |
|---------|-----|------------|--------|----------------|---------|
| APP/官網/門市 (All) | ✓ | ✓ | ✓ | ✓ | ✓ |
| APP/門市 (AppAndInStore) |  | ✓ | ✓ | ✓ | ✓ |
| APP (App) |  | ✓ | ✓ |  |  |
| 官網/App (WebAndApp) | ✓ | ✓ | ✓ |  |  |
| 門市 (InStore) |  |  |  | ✓ | ✓ |

<br>

## 2. 資料存放位置

- PromotionEnginePlatformTypeEnum(DB)
- SalesChannelEnum(RuleRecord)

## 3. 促購後台類型

- Web (官網)
- AppIOS (iOS)
- AppAndroid (Android)
- LocationWizard (店員幫手)
- InStore (門市)

<br>

## 4. 平台類型字典 GetPlatformTypeDiction

**適用**
- isWeb
- isAndroid
- isiOS
- isInStore
- isLocationWizard

<br>

**可見**
- isWebVisible
- isAndroidVisible
- isiOSVisible
- isLocationWizardVisible

<br>

## 5. 促購引擎設定 PromotionEngineSetting

適用：
- PromotionEngineSetting_IsWeb
- PromotionEngineSetting_IsAndroidApp
- PromotionEngineSetting_IsiOSApp
- PromotionEngineSetting_IsLocationWizard
- PromotionEngineSetting_IsInStore

<br>

可見：
- PromotionEngineSetting_IsWebVisible
- PromotionEngineSetting_IsAndroidAppVisible
- PromotionEngineSetting_IsiOSAppVisible
- PromotionEngineSetting_IsLocationWizardVisible

<br>

## 6. 促購引擎規則 PromotionEngine Rule

<br>

- MatchedSalesChannels：TargetPlatformTypeList
- VisibleSalesChannels：VisiblePlatformTypeList

<br>

## 7. 可視通路邏輯

<br>

有 APP：
- isWebVisible
- isAndroidVisible
- isiOSVisible

<br>

門市：
- isWebVisible
- isLocationWizardVisible
- isAndroidVisible
- isiOSVisible

<br>

## 8. 匹配銷售通路 MatchedSalesChannels

```json
"matchedSalesChannels": [
    "Web",
    "AppIOS",
    "AppAndroid",
    "LocationWizard",
    "InStore"
]
```

<br>

## 9. S3 儲存

s3 的 rule 節點有 "MatchedSalesChannels":31

<br>

## 10. 銷售通路列舉 SalesChannelEnum

```csharp
[Flags]
public enum SalesChannelEnum
{
    Web = 1,
    AppIOS = 2,
    AppAndroid = 4,
    LocationWizard = 8,
    InStore = 0x10
}
```

## 11. 規則解析

parse rule 的地方都會有 MatchedSalesChannels

ParsePromotionEngineRuleObject

## 12. 其他規則限制

- 活動進行中不能改通路
- 沒有純官網
- 沒有官網門市