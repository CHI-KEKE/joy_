
## 📋 目錄
1. [SMS 對應](#sms-對應)
2. [DB](#db)
3. [促購後台對應](#促購後台對應)
4. [GetPlatformTypeDiction](#getplatformtypediction)
5. [PromotionEngineSetting](#promotionenginesetting)
6. [PromotionEngine Rule](#promotionengine-rule)
7. [可視通路邏輯](#可視通路邏輯)
8. [MatchedSalesChannels](#matchedsaleschannels)
9. [S3](#s3)
10. [SalesChannelEnum](#saleschannelenum)
11. [ParsePromotionEngineRuleObject](#parsepromotionengiineruleobject)
12. [Others](#others)

---


## SMS 對應

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
<br>

## DB

- PromotionEnginePlatformTypeEnum(DB)
- SalesChannelEnum(RuleRecord)

<br>
<br>

## 促購後台對應

- Web (官網)
- AppIOS (iOS)
- AppAndroid (Android)
- LocationWizard (店員幫手)
- InStore (門市)

<br>
<br>

## GetPlatformTypeDiction

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
<br>

## PromotionEngineSetting

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
<br>

## PromotionEngine Rule

- MatchedSalesChannels：TargetPlatformTypeList
- VisibleSalesChannels：VisiblePlatformTypeList

<br>
<br>

## 可視通路邏輯

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
<br>

## MatchedSalesChannels

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
<br>

## S3

s3 的 rule 節點有 "MatchedSalesChannels":31

<br>
<br>

## SalesChannelEnum

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

<br>
<br>

## ParsePromotionEngineRuleObject

parse rule 的地方都會有 MatchedSalesChannels

ParsePromotionEngineRuleObject

<br>
<br>

## Others

- 活動進行中不能改通路
- 沒有純官網
- 沒有官網門市