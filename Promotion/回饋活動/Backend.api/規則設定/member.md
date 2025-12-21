## 📋 目錄
1. [MemberCollection](#membercollection)
2. [環境設定是否走新版 MemberCollection 開關](#環境設定是否走新版-membercollection-開關)
3. [DB](#db)
4. [SetMatchedUserScopes](#setmatcheduserscopes)

---

## MemberCollection

確認白名單設定, 活動對象為全店則需指定 MemberCollectionId = -1

<br>

```csharp
//// 允許指定客群活動類型，且活動對象為全店，則將MemberCollectionId 設為-1(不指定)，其餘空值為不支援
if (entity.TargetMemberTypeDef == nameof(PromotionEngineTargetMemberTypeDefEnum.All) &&
            this._enableMemberCollectionPromotionType.Contains(entity.TypeDef))
{
     entity.MemberCollectionId = "-1";
}
```

目前已實作支援客群的活動類型：DiscountByMemberWithPrice

<br>
<br>

## 環境設定是否走新版 MemberCollection 開關

<br>

- Table：ShopStaticSetting
- Group：PromotionEngine
- Key：SupportMemberCollectionShop

<br>

範例：HK_QA：2|none，前面為白名單，後面是黑名單

<br>
<br>

## DB

<br>

- PromotionEngine_TargetMemberTypeDef = MemberCollection
- PromotionEngine_UpdatedUser = SyncPromotionTagToCollection
- PromotionEngine_DisplaySetting = Member / All ??
- PromotionEngine_MemberCollectionId = 93be0653-4a61-4d8d-9a24-530d992675fa(範例)(entity.MemberCollectionId ?? string.Empty;)

<br>

## SetMatchedUserScopes

<br>
<br>

```csharp
//// 設定活動對象範圍
ruleService.SetMatchedUserScopes(entity.TargetMemberType, entity.CrmShopMemberCardIds, entity.MemberCollectionId);
```