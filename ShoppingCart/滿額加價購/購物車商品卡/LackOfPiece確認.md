# LackOfPiece 命名差異確認與本輪異動清單

## 問題背景

Cart 專案的 `HintInfo` 接收 Engine 回傳的 JSON，其中件數相關欄位名稱在不同活動間不一致。

---

## Engine 端欄位命名確認

| 活動類型 | Engine HintInfo 欄位名稱 | 檔案 |
|---|---|---|
| CartReachPieceExtraPurchase（滿件加價購） | **`LackOfPiece`** | `CartExtraPurchaseRuleBase.cs` line 108 |
| RegisterReachPiece | `LackPieceCount` | `RegisterReachPiece.cs` line 161 |
| DiscountReachPieceWithAmount | `LackPieceCount` | `DiscountReachPieceWithAmount.cs` |
| DiscountReachPieceWithPrice | `LackPieceCount` | `DiscountReachPieceWithPrice.cs` |
| DiscountReachPieceWithRate | `LackPieceCount` | `DiscountReachPieceWithRate.cs` |
| DiscountNthPieceWith* | `LackPieceCount` | 各 NthPiece 檔案 |
| DiscountReachGroupsPiece（紅配綠） | 無 piece 欄位（用 `CycleLimitCount`） | `DiscountReachGroupsPiece.cs` |

**結論：只有 `CartExtraPurchaseRuleBase`（滿額/滿件加價購共用基底）使用 `LackOfPiece`，其餘所有件數活動均用 `LackPieceCount`。這是 Engine 端的歷史命名不一致。**

---

## 資料流修正

### 問題
Cart 的 `HintInfo` 只有 `LackPieceCount`，JSON 反序列化時 `LackOfPiece` key 找不到對應欄位 → 靜默丟棄 → 永遠是 0 → 文案顯示「再湊  件」（空白）。

### 修正方式
1. Cart `HintInfo` 補上 `LackOfPiece` 欄位接收 Engine 的 JSON key
2. `CalculatePromotionProcessor` 先跑原始 `LackPieceCount` 賦值邏輯，再對 `CartReachPieceExtraPurchase` 做覆寫

```
Engine JSON: { "LackOfPiece": 3 }
     ↓
Cart HintInfo.LackOfPiece = 3
     ↓
promotionInfo.LackPieceCount = 3  （覆寫）
     ↓
Shopping GetLackOfCondition() → return promotionInfo.LackPieceCount.ToString()
     ↓
文案：「再湊 3 件」✅
```

---

## 本輪所有程式碼異動位置

### Cart 專案

#### 1. HintInfo.cs — 新增 LackOfPiece 欄位
```
C:\91APP\Cart\cart2\nine1.cart\src\BusinessLogic\Nine1.Cart.BL.BE\Promotion\Responses\HintInfo.cs
```
- **line 48**：新增 `public int LackOfPiece { get; set; }`
- 用途：接收 Engine `CartExtraPurchaseRuleBase.HintInfo.LackOfPiece` 的 JSON key

#### 2. CalculatePromotionProcessor.cs — 覆寫 LackPieceCount
```
C:\91APP\Cart\cart2\nine1.cart\src\BusinessLogic\Nine1.Cart.BL.Services\Processor\Promotion\CalculatePromotionProcessor.cs
```
- **line 668–676**：原始 `LackPieceCount` 邏輯不動，之後針對 `CartReachPieceExtraPurchase` 用 `hintState.LackOfPiece` 覆寫

```csharp
//// 紅配綠將僅第一層缺少件數加總
promotionInfo.LackPieceCount = promotionType != PromotionEngineTypeDefEnum.DiscountReachGroupsPiece ?
                hintState.LackPieceCount :
                hintState.Groups.Values.Where(x => x.GroupsLimitCycleCount == 0).Sum(x => x.NextCycleLakeItemsCount);
//// 滿件加價購 Engine 回傳欄位為 LackOfPiece（命名與其他活動不同），覆寫
if (promotionType == PromotionEngineTypeDefEnum.CartReachPieceExtraPurchase)
{
    promotionInfo.LackPieceCount = hintState.LackOfPiece;
}
```

---

### Shopping 專案

#### 3. PromotionFlagEnum.cs — 新增 CartExtraPurchaseMainProduct
```
C:\91APP\Shopping\src\BusinessLogic\Nine1.Shopping.BL.BE\Promotion\Enum\PromotionFlagEnum.cs
```
- 新增 `CartExtraPurchaseMainProduct` enum 值
- 用途：在 display text 處理流程中標記滿額/滿件加價購的主商品（活動範圍品）

#### 4. GetSalepagePromotionDisplayTextProcessor.cs — 標記主商品 flag
```
C:\91APP\Shopping\src\BusinessLogic\Nine1.Shopping.BL.Services\CartProcessor\GetSalepagePromotionDisplayTextProcessor.cs
```
- `ArrangePromotionDisplayTextAsync` 的 `skuWithPromotionList` loop 中（`var salepage = this.GetSalePage(...)` 之後）
- 若該 SKU 的 `PromotionList` 含有 `CartReachPriceExtraPurchase` 或 `CartReachPieceExtraPurchase`，則對 salepage 加上 `CartExtraPurchaseMainProduct` flag

```csharp
if (skuWithPromotion.PromotionList.Any(p =>
        p.Type is PromotionEngineTypeDefEnum.CartReachPriceExtraPurchase
               or PromotionEngineTypeDefEnum.CartReachPieceExtraPurchase))
{
    salepage.PromotionFlags.Add(nameof(PromotionFlagEnum.CartExtraPurchaseMainProduct));
}
```

> **設計說明**：`PromotionFlags` 是既有的 per-salepage 字串集合，用於 display text 流程中標記商品特性（同 `ECouponExcludedByPromotion`、`GlobalExclusive` 等的慣例）。不直接改 `SalepageEntity` 屬性是為了避免污染資料實體。

#### 5. GetSalepageDiscountDisplayTextProcessor.cs — 主商品顯示「不適用折價券」
```
C:\91APP\Shopping\src\BusinessLogic\Nine1.Shopping.BL.Services\CartProcessor\GetSalepageDiscountDisplayTextProcessor.cs
```
- 新增 `using Nine1.Shopping.BL.BE.Promotion.Enum`
- 在 `SalepageGroupList` 迴圈末端新增判斷：

```csharp
//// 購物車滿額/滿件加價購主商品永遠不適用折價券
if (salePage.PromotionFlags.Contains(nameof(PromotionFlagEnum.CartExtraPurchaseMainProduct)))
{
    text = GetSalepageDiscountDisplayText.NotFitCoupons;
}
```

> **注意**：`GetSalepagePromotionDisplayTextProcessor` 在 pipeline 中先執行，flag 已設好，此 processor 才讀取。

#### 6. UnreachCartExtraPurchaseDisplayTextProcessor.cs — 加購品折價券/點數提示
```
C:\91APP\Shopping\src\BusinessLogic\Nine1.Shopping.BL.Services\CartProcessor\UnreachCartExtraPurchaseDisplayTextProcessor.cs
```
- 新增 `using Nine1.Shopping.Common.Translations.Backend.Service.CartProcessor`
- Expired loop 與 Unreach loop 各新增：
  - `ECouponExcludedByPromotion` flag → 顯示「不適用折價券」（`GetSalepageDiscountDisplayText.NotFitCoupons`）
  - `LoyaltyPointExcluded` flag → 顯示「不適用點數折抵」（`GetLoyaltyPoints.LoyaltyPointExecluded`）

---

### Promotion FrontendAPI 專案

#### 7. CartReachPieceExtraPurchaseRuleServiceTest.cs — 修正測試 Build 錯誤
```
C:\91APP\Promotion\frontEnd\nine1.promotion.web.api.frontend\src\Test\Nine1.Promotion.BL.Services.Test\PromotionEngines\CartReachPieceExtraPurchaseRuleServiceTest.cs
```
- 原因：`CartReachPieceExtraPurchaseRuleService` 建構子新增了 `IPromotionEngineRepository`、`ISpecialPriceRepository` 參數，測試未更新
- 修正：
  - 新增 `using NSubstitute`、repository namespace
  - 類別層級新增 NSubstitute mock fields
  - `GetTarget()` 改傳入兩個 mock

```csharp
private readonly IPromotionEngineRepository _promotionEngineRepository = Substitute.For<IPromotionEngineRepository>();
private readonly ISpecialPriceRepository _specialPriceRepository = Substitute.For<ISpecialPriceRepository>();

private CartReachPieceExtraPurchaseRuleService GetTarget()
{
    return new CartReachPieceExtraPurchaseRuleService(_promotionEngineRepository, _specialPriceRepository);
}
```
