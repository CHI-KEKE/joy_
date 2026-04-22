# 回饋活動 — Basket 計算完整流程

## 整體鏈路

```
CalculateController.CartCalculateAsync(request)
    └── CalculateService.CalculateShoppingCartAsync(shopId, request)
            ├── CreateProcessContextAsync()   ← 建立 ProcessContext（商品攤平、Shipping 初始化）
            └── CalculateByProcessGroupAsync()
                    └── 依 Priority 執行各 Group
                            └── BottomProcessGroup.CalculateAsync()  ← 回饋活動在這裡
```

---

## BottomProcessGroup.CalculateAsync 執行步驟

```
1. UpdateProcessRuleListAsync()
   └── PromotionRuleRepository.GetRuleInfoListAsync(context)
       ├── DB 查有效活動 Id（全店 + SalePage）
       ├── GetPromotionEngineDataListAsync()   ← Redis 快取活動 Rule
       ├── GetMemberCollectionTagsAsync()      ← 掃描需要 MemberCollection 的活動
       ├── SetMemberCollectionToUserTagAsync() ← 打 Member Collection API，tag 注入到 User
       └── 壽星邏輯（IsBirthdayMonthEnabled）
           ├── DynamoDB 取當月 CollectionId
           ├── 打 Member Collection API match
           └── 符合 → User.Tags 加入 "CurrentBirthdayMonth"

2. UpdateSalepageSkuItemListAsync()
   ├── GetSalepageInfoListAsync()
   │   └── 打 Collection API，取各 SalepageId 符合的 CollectionId → 貼到商品 Tags
   └── GetOuterIdTagsAsync()
       └── 比對商品 OuterId，符合 → 貼 PromotionTagId 到商品 Tags

3. UpdateLocationTagsAsync()           ← BottomProcessGroup 是空實作！
   （見門市情境說明）

4. LoadRuleAndModifyPriority()         ← 引擎載入 Rule，設定 Priority

5. ModifyRuleByCustomMethodAsync()     ← 自訂調整（BottomGroup 有贈品相關處理）

6. EngineCalculate()
   ├── CreateShoppingCartContext()     ← 組裝引擎所需 context
   │   ├── UserContext(User.Id, User.Tags)
   │   ├── RegionContext(CountryProfileId, ShippingAreaId, CountryTags)
   │   ├── LocationContext(LocationId, LocationTags, isLocationAlwaysMatch)
   │   ├── ShoppingCartContext(channel, ...)
   │   ├── 商品逐件加入 Purchase()
   │   ├── SetPaymentType()
   │   └── SetShippingType()
   └── _promotionEngine.ProcessPromotion(cart)   ← 引擎執行比對

7. UpdateByPromotionEngineResult()    ← 結果寫回 context
```

---

## Basket（菜籃）API 計算差異

菜籃計算走的是 `PromotionBasketGroup`，和一般訂單計算的主要差異：

| 項目 | 一般訂單（BottomProcessGroup） | 菜籃（PromotionBasketGroup） |
|------|-------------------------------|------------------------------|
| Rule 來源 | 所有有效活動 | 只取 `request.PromotionRuleIds` 指定的活動 |
| 金流 PaymentTypes | 活動 Rule 原始設定 | **強制清空 null** |
| 物流 ShippingTypes | 活動 Rule 原始設定 | **強制清空 null** |
| 地區 IncludeRegionScopes | 活動 Rule 原始設定 | **強制清空 null** |
| 通路 MatchedSalesChannels | 活動 Rule 原始設定 | **不清空**，維持比對 |
| 門市 LocationTags | 空（UpdateLocationTagsAsync 空實作） | 空（同左） |

> 菜籃計算因為無法得知金流/物流/地區，所以清空這三個維度讓活動可以被預覽。
> 通路維度刻意保留，避免菜籃顯示不應出現的活動。

---

## 計算結果結構

回饋活動計算完後，每個活動會有一筆 `PromotionRecord`：

```csharp
{
    PromotionEngineId: int,
    IsMatch: bool,           // 是否達成條件
    LackAmount: decimal,     // 距離門檻還差多少金額
    // Lack* 欄位（在 CartPromotionInfoEntity 層面組裝）
}
```

`CartPromotionInfoEntity` 中的 Lack 欄位：

| 欄位 | 說明 |
|------|------|
| `LackOfPrice` | 距離滿額門檻還差多少錢 |
| `LackOfUserScope` | 會員資格不符（等級/Collection/壽星） |
| `LackOfProduct` | 指定商品不在範圍（Collection/料號） |
| `LackSalesChannel` | 通路不符 |
| `LackOfRegion` | 地區不符 |
| `IsDisplayInShoppingCart` | 是否在購物車顯示（CustomField1） |
| `Quota` | 張數限制（CustomField2） |
