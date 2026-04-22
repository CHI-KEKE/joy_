# PromotionFrontend 補上滿件加價購取 SpecialPrice 前因後果

## 背景

在驗證 Shopping → Cart → PromotionFrontend → PromotionEngine 完整路徑能否正確組合購物車商品卡文案時，發現滿件加價購（`CartReachPieceExtraPurchase`）的「未達門檻提示文案」永遠顯示不出來的 Bug。

---

## 問題根源

### 現象

Shopping `UnreachCartExtraPurchaseDisplayTextProcessor.cs` 在處理滿件加價購未達門檻時，會以加購品的 `OptionalTypeId`（即 `SpecialPriceId`）去查對應的門檻條件：

```csharp
// UnreachCartExtraPurchaseDisplayTextProcessor.cs line 149
var thresholdCondition = piecePromotionInfo.CartExtraPurchases
    .FirstOrDefault(x => x.SpecialPriceIdList.Contains(salepage.OptionalTypeId));
```

查到 `thresholdCondition` 後，再計算差距件數：

```csharp
var lackPiece = piecePromotionInfo.PurchasedItemPiece > 0
    ? Math.Abs(thresholdCondition.ReachPiece - piecePromotionInfo.PurchasedItemPiece)
    : piecePromotionInfo.LackPieceCount;
// → 顯示「再買 N 件即可加購」文案
```

### 根本原因

`CartExtraPurchaseConditionEntity.SpecialPriceIdList` **永遠是空清單**，導致 `thresholdCondition` 永遠是 `null`，文案靜默不顯示。

追溯資料流：

```
PromotionFrontend CartReachPieceExtraPurchaseRuleService.ParsePromotionEngineRuleObject
  → CartExtraPurchaseConditions[*].SpecialPriceIdList = []（未填）
  ↓ PromotionEngineService.cs line 377
  → Cart GetPromotionEngineResponseEntity.CartExtraPurchaseConditions
  ↓ Cart CalculatePromotionProcessor.cs line 646
  → promotionInfo.CartExtraPurchases = promotion.CartExtraPurchaseConditions
  ↓ Cart api/carts/get 回傳 CartEntity
  → Shopping PromotionInfoList[*].CartExtraPurchases[*].SpecialPriceIdList = []
  → 查不到 thresholdCondition → 文案不顯示 ❌
```

### 對比：滿額版為何正常

`CartReachPriceExtraPurchaseRuleService.ParsePromotionEngineRuleObject` 有額外呼叫 `GetCartExtraPurchaseSpecialPriceListAsync`，從 DB 的 `PromotionEngineSpecialPrice` table 取得各 ConditionId 對應的加購品特價 ID 清單，填入 `SpecialPriceIdList`：

```csharp
// CartReachPriceExtraPurchaseRuleService.cs（滿額版，已正確實作）
var promotionEngineId = (int)JObject.Parse(jsonRule)["Id"];
var specialPriceMap = this.GetCartExtraPurchaseSpecialPriceListAsync(promotionEngineId).GetAwaiter().GetResult();

foreach (var condition in conditions)
{
    specialPriceMap.TryGetValue(conditionId, out var specialPriceIdList);
    item.SpecialPriceIdList = specialPriceIdList ?? new List<long>();  // ← 滿額版有這行
}
```

**滿件版原本沒有對應邏輯**，導致 `SpecialPriceIdList` 永遠為空。

---

## 為何滿件加價購也適用同樣邏輯

### 1. 同一張 DB 資料表

`CartExtraPurchaseProcessGroup.ModifyRuleByCustomMethodAsync` 對 **滿額與滿件一視同仁**，都從 `PromotionEngineSpecialPrice` table 取加購品資料：

```csharp
// CartExtraPurchaseProcessGroup.cs line 133
var rules = this._promotionEngine.Rules.Values
    .Where(r => this._allTypeFullName.Contains(r.TypeFullName))  // 包含兩種類型
    .Cast<CartExtraPurchaseRuleBase>()
```

每筆 `PromotionEngineSpecialPrice` 都有：
- `PromotionEngineSpecialPrice_Id` → 即加購品的 `SpecialPriceId`
- `PromotionEngineSpecialPrice_ConditionId` → 對應哪個門檻條件

### 2. `OptionalTypeId` 就是 `SpecialPriceId`

```csharp
// CartExtraPurchaseProcessGroup.cs line 175-176
var specialPrice = cspGetPromotionEngineSpecialPriceData.FirstOrDefault(c =>
    c.PromotionEngineSpecialPrice_Id == item.OptionalTypeId &&   // OptionalTypeId = SpecialPriceId
    c.PromotionEngineSpecialPrice_SaleProductSKUId == item.SkuId);
```

Shopping 用 `salepage.OptionalTypeId` 查 `SpecialPriceIdList`，這個 key 的來源就是 `PromotionEngineSpecialPrice_Id`，兩邊完全對應。

---

## 修正內容

**檔案：** `CartReachPieceExtraPurchaseRuleService.cs`（PromotionFrontend）

### 修正前

```csharp
public CartReachPieceExtraPurchaseRuleService()  // 無依賴注入
{
}

public PromotionEngineRuleEntity ParsePromotionEngineRuleObject(string jsonRule)
{
    // ...
    foreach (var condition in conditions)
    {
        CartExtraPurchaseConditionEntity item = new CartExtraPurchaseConditionEntity()
        {
            ConditionId = (int)condition.ConditionId,
            ReachPrice  = (decimal)condition.ReachPrice,
            ReachPiece  = (int)condition.ReachPiece
            // ← SpecialPriceIdList 未填，預設空清單
        };
    }
}
```

### 修正後

```csharp
public CartReachPieceExtraPurchaseRuleService(
    IPromotionEngineRepository promotionEngineRepository,
    ISpecialPriceRepository specialPriceRepository)
{
    this._promotionEngineRepository = promotionEngineRepository;
    this._specialPriceRepository    = specialPriceRepository;
}

public PromotionEngineRuleEntity ParsePromotionEngineRuleObject(string jsonRule)
{
    // ...
    var promotionEngineId = (int)JObject.Parse(jsonRule)["Id"];
    var specialPriceMap   = this.GetCartExtraPurchaseSpecialPriceListAsync(promotionEngineId)
                                .GetAwaiter().GetResult();   // ← 補上 DB 查詢

    foreach (var condition in conditions)
    {
        int conditionId = (int)condition.ConditionId;
        specialPriceMap.TryGetValue(conditionId, out var specialPriceIdList);

        CartExtraPurchaseConditionEntity item = new CartExtraPurchaseConditionEntity()
        {
            ConditionId       = conditionId,
            ReachPrice        = (decimal)condition.ReachPrice,
            ReachPiece        = (int)condition.ReachPiece,
            SpecialPriceIdList = specialPriceIdList ?? new List<long>()   // ← 正確填入
        };
    }
}

// ← 補上與滿額版相同的私有方法
private async Task<Dictionary<int, List<long>>> GetCartExtraPurchaseSpecialPriceListAsync(int promotionEngineId)
{
    var promotionEngine = await this._promotionEngineRepository.GetAsync(promotionEngineId);
    if (promotionEngine == null) return new Dictionary<int, List<long>>();

    var specialPriceData = await this._specialPriceRepository.CspGetPromotionEngineSpecialPriceDataAsync(
        nameof(CspGetPromotionEngineSpecialPriceDataQueryTypeEnum.PromotionEngine),
        null, null,
        string.Join(',', promotionEngineId),
        promotionEngine.PromotionEngine_ShopId);

    return specialPriceData
        .GroupBy(x => x.PromotionEngineSpecialPrice_ConditionId)
        .ToDictionary(g => g.Key, g => g.Select(x => x.PromotionEngineSpecialPrice_Id).ToList());
}
```

---

## 修正後的完整資料流

```
DB PromotionEngineSpecialPrice（含 ConditionId）
  ↓ GetCartExtraPurchaseSpecialPriceListAsync（補上後）
  ↓ CartExtraPurchaseConditionEntity.SpecialPriceIdList = [101, 102, ...]（正確填入）
  ↓ Cart → Shopping PromotionInfoList.CartExtraPurchases
  ↓ Shopping UnreachCartExtraPurchaseDisplayTextProcessor
      CartExtraPurchases.FirstOrDefault(x => x.SpecialPriceIdList.Contains(OptionalTypeId))
      → 找到 thresholdCondition.ReachPiece
      → lackPiece = |ReachPiece - PurchasedItemPiece| 或 LackPieceCount
      → 顯示「再買 N 件即可加購」文案 ✅
```

---

## 同步更新測試

**檔案：** `CartReachPieceExtraPurchaseRuleServiceTest.cs`

| 新增測試 | 說明 |
|---------|------|
| `ParsePromotionEngineRuleObject_ShouldPopulateSpecialPriceIdList` | 驗證各 ConditionId 的 `SpecialPriceIdList` 正確從 DB 填入 |
| `ParsePromotionEngineRuleObject_WhenPromotionEngineNotFound_SpecialPriceIdListShouldBeEmpty` | 驗證 PromotionEngine 不存在時 `SpecialPriceIdList` 回傳空清單（防護邏輯）|

所有測試結果：**22 個全部通過** ✅
