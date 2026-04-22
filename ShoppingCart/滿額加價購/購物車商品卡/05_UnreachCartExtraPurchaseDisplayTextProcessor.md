# UnreachCartExtraPurchaseDisplayTextProcessor

> **執行時序**：第 3 步（與其他 Processor 並行）  
> **檔案路徑**：`src/BusinessLogic/Nine1.Shopping.BL.Services/CartProcessor/UnreachCartExtraPurchaseDisplayTextProcessor.cs`

---

## 職責

專門處理「購物車滿額/滿件加價購活動**加購品**」的未達門檻或無法購買的提示文案：

| 情境 | 文案類型 |
|------|---------|
| 活動失效 / 過期 | `Disabled`（灰色文案） |
| APP 限定通路不符合 | `Disabled`（灰色文案） |
| 金額**未達門檻** | `MismatchedPromotion`（未符合活動） |

---

## 實作重點

### 1. 入口條件
```csharp
if (context.Data.ExpiredPromotionSalepageList.Any(x => x.IsCartExtraPurchase) == false
    && context.Data.UnreachAmountCartExtraPurchaseSalepageList.Any(x => x.IsCartExtraPurchase) == false)
{
    return Task.CompletedTask;
}
```
只處理 `IsCartExtraPurchase == true` 的商品。

### 2. BuildCartPromotionEngineMapping：建立 CartId ↔ PromotionEngineId 對應
```csharp
// PromotionFlags 格式：{PromotionEngineId}:CartExtraPurchaseSpecialPriceId:{SpecialPriceId}
const string SplitKey = $":{nameof(PromotionFlagEnum.CartExtraPurchaseSpecialPriceId)}:";
```
從 PromotionFlags 解析出活動 ID，建立 `Dictionary<long, long>`（CartId → PromotionEngineId）。

### 3. 無法購買清單（`ExpiredPromotionSalepageList`）
```csharp
foreach (var salepage in context.Data.ExpiredPromotionSalepageList.Where(x => x.IsCartExtraPurchase))
{
    var displayText = PromotionConditionChangedOrExpired;  // 預設：活動條件異動或已結束

    if (/* 找到活動 */ && /* LackSalesChannel 包含 APP 通路 */)
    {
        displayText = PromotionConditionAppOnly;  // APP 限定
    }

    salepage.DiscountDisplayList.Add(new SalepageDiscountDisplayEntity
    {
        DisplayType = DiscountDisplayTypeEnum.Disabled,
        DisplayText = displayText
    });
}
```

### 4. 未達金額門檻清單（`UnreachAmountCartExtraPurchaseSalepageList`）
```csharp
foreach (var salepage in context.Data.UnreachAmountCartExtraPurchaseSalepageList.Where(x => x.IsCartExtraPurchase))
{
    var promotionInfo = context.Data.PromotionInfoList
        .FirstOrDefault(x => x.Id == promotionEngineId && x is { PurchasedItemPrice: > 0, LackOfPrice: > 0 });

    // 從 CartExtraPurchases 找到此加購品對應的條件
    var thresholdCondition = promotionInfo.CartExtraPurchases
        .FirstOrDefault(x => x.SpecialPriceIdList.Contains(salepage.OptionalTypeId));

    if (thresholdCondition != null)
    {
        var threshold = thresholdCondition.ReachPrice;  // ← 只看 ReachPrice（金額）
        var diff = (int)Math.Abs(threshold - totalAmount);
        displayText = string.Format(UnreachAmountThresholdHint, diff);

        salepage.DiscountDisplayList.Add(new SalepageDiscountDisplayEntity
        {
            DisplayType = DiscountDisplayTypeEnum.MismatchedPromotion,
            DisplayText = displayText
        });
    }
}
```

### 5. 文案對應
| Key | 文案（預設） |
|-----|------------|
| `PromotionConditionChangedOrExpired` | 活動條件異動或已結束 |
| `PromotionConditionAppOnly` | 此活動僅限 APP 使用 |
| `UnreachAmountThresholdHint` | 還差 {0} 元 |

---

## US 599198 需求對應狀態

| 需求 | 狀態 | 說明 |
|------|------|------|
| 加購品活動失效/過期 → 顯示預設文案 | ✅ 已實作 | `PromotionConditionChangedOrExpired` |
| 加購品 APP 限定通路不符 → 顯示 APP 限定文案 | ✅ 已實作 | `PromotionConditionAppOnly` |
| 加購品滿額未達門檻 → 顯示差距**金額**提示 | ✅ 已實作 | `UnreachAmountThresholdHint` + `ReachPrice` |
| 加購品**滿件**未達門檻 → 顯示差距**件數**提示 | ❌ 未處理 | 目前只看 `thresholdCondition.ReachPrice`，`ReachPiece` 完全沒處理 |

### 需修改項目

```csharp
// 目前只處理滿額（ReachPrice）
var threshold = thresholdCondition.ReachPrice;

// 需依活動類型分支：
if (promotionInfo.Type == PromotionEngineTypeDefEnum.CartReachPieceExtraPurchase)
{
    // 滿件：顯示件數差距
    var pieceDiff = thresholdCondition.ReachPiece - (int)/* 已購件數 */;
    displayText = string.Format(UnreachPieceThresholdHint, pieceDiff);
}
else
{
    // 滿額：顯示金額差距（現有邏輯）
    var diff = (int)Math.Abs(threshold - totalAmount);
    displayText = string.Format(UnreachAmountThresholdHint, diff);
}
```

> `CartExtraPurchaseConditionEntity.ReachPiece` 已有定義，只需在此補充件數分支邏輯。  
> 同時需確認**已購件數**的資料來源（`PurchasedItemPrice` 是金額，件數可能需另外從 promotionInfo 取得）。
