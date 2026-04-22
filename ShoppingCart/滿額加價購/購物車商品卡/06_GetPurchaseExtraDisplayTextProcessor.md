# GetPurchaseExtraDisplayTextProcessor（舊版）

> **執行時序**：第 3 步（與其他 Processor 並行）  
> **檔案路徑**：`src/BusinessLogic/Nine1.Shopping.BL.Services/CartProcessor/UnreachAmountPurchaseExtraDisplayTextProcessor.cs`

---

## 職責

處理**舊版**滿額加價購（`SourceType = ExtraPurchase`）商品的未達門檻提示文案。  
此為舊版活動機制，與 US 599198 所針對的新版購物車加價購（`CartReachPriceExtraPurchase` / `CartReachPieceExtraPurchase`）**不同**。

---

## 實作重點

### 1. 判斷依據：SourceType
```csharp
var instruction = context.Data.PromotionInfoList
    .FirstOrDefault(x => x.SourceType == SourceTypeEnum.ExtraPurchase && x.IsMatch == false);
```
- 使用 `SourceTypeEnum.ExtraPurchase` 識別舊版加價購
- 新版購物車加價購使用 `SourceTypeEnum.Promotion`

### 2. 寫入 UnreachAmountPurchaseExtraSalePageList
```csharp
foreach (var salepage in context.Data.UnreachAmountPurchaseExtraSalePageList)
{
    salepage.DiscountDisplayList.Add(new SalepageDiscountDisplayEntity
    {
        DiscountSource = SourceTypeEnum.ExtraPurchase.ToString(),
        DisplayText = string.Format(UnreachAmountThresholdHint, (int)Math.Abs(instruction.LackOfPrice)),
        DisplayType = DiscountDisplayTypeEnum.MismatchedPromotion
    });
}
```

### 3. 也更新 UnReachPurchaseExtraAmount
```csharp
context.Data.UnReachPurchaseExtraAmount = Math.Max(0,
    context.Data.PromotionInfoList
        .FirstOrDefault(x => x.SourceType == SourceTypeEnum.ExtraPurchase)?.LackOfPrice
    ?? context.Data.PurchaseExtraAmountThreshold);
```
此數值為頁面頂部「再買 $XXX 可加價購」提示用。

---

## 新舊版加價購對照

| 維度 | 舊版（ExtraPurchase） | 新版（CartReachPriceExtraPurchase / CartReachPieceExtraPurchase） |
|------|----------------------|---------------------------------------------------------------|
| SourceType | `ExtraPurchase` | `Promotion` |
| 商品標記 | `IsPurchaseExtra = true` | `IsCartExtraPurchase = true` |
| 未達門檻清單 | `UnreachAmountPurchaseExtraSalePageList` | `UnreachAmountCartExtraPurchaseSalepageList` |
| 失效清單 | — | `ExpiredPromotionSalepageList`（`IsCartExtraPurchase` filter） |
| 處理 Processor | `GetPurchaseExtraDisplayTextProcessor` | `UnreachCartExtraPurchaseDisplayTextProcessor` |

---

## US 599198 需求對應狀態

| 需求 | 狀態 | 說明 |
|------|------|------|
| US 599198 相關修改 | ⬛ 無需修改 | 此 Processor 處理的是舊版加價購，與 US 599198 目標（新版購物車加價購）無關 |

### 備註
此 Processor 僅作為**對照參考**，US 599198 的邏輯修改不涉及此 Processor。  
新版購物車加價購的對應 Processor 為 `UnreachCartExtraPurchaseDisplayTextProcessor`。
