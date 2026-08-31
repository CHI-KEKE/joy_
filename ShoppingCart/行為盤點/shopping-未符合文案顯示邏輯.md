# Shopping 購物車商品卡「未符合」文案顯示邏輯

> 對應 Processor：`GetSalepagePromotionDisplayTextProcessor`
> 檔案路徑：`C:\91APP\Shopping\src\BusinessLogic\Nine1.Shopping.BL.Services\CartProcessor\GetSalepagePromotionDisplayTextProcessor.cs`

---

## 一、整體流程總覽

```bash
ExecuteAsync
  │
  ├─ [A] GetSkuWithPromotionList()
  │       → 每個商品 × 每個活動 → 篩選出「有效 pair」
  │       → 每個 pair 帶著 IsMatch=false（Engine 算好的）
  │       → 如果 IsMatch=false 但 IsExcluded=true → 走「無法參與」，不走「未符合」
  │
  └─ [C] ArrangePromotionDisplayTextAsync()
          │
          ├─ FilteredPromotion(MismatchedPromotion)
          │   → 從 SkuWithPromotionList 取出 IsMatch=false、IsExcluded=false、IsAppOnly=false 的活動
          │   → 再套用自選券、優惠碼互斥等條件
          │
          └─ GetSalepageDiscountDisplayAsync(MismatchedPromotion)
              → 產生 DiscountDisplayList 項目（IsMatched=false）
              → AddRange 到 salepage.DiscountDisplayList
```

---

## 二、前置：PromotionEngine 算出 IsMatch=false

Shopping 拿到的 `CartPromotionInfoEntity` 中：

| 欄位 | 說明 |
|------|------|
| `IsMatch = false` | Engine 判斷活動**未達門檻** |
| `IsExcluded` | 被獨立活動（GlobalExclusive）排除 → 走「無法參與」，不走「未符合」 |
| `IsAppOnly` | APP 獨享 → 走「APP 獨享」，不走「未符合」 |
| `LackOfRegion` | 缺少地區條件 → 不顯示未符合文案 |
| `LackSalesChannel` | 缺少銷售通路（線下通路限定活動） |

---

## 三、Step A：GetSkuWithPromotionList — 商品×活動有效 pair 篩選

> 此 Step 與「已符合」流程相同，以下只說明與「未符合」相關的差異。

### 3.1 IsSkipDisplayText — 整個活動跳過的情況

```csharp
isSkip = (promotion.IsMatch == false && (memberTierNotMatch || memberCollectionNotMatch))
       || (promotion.IsDisplayInShoppingCart == false);
```

| 條件 | 未符合影響 |
|------|-----------|
| `IsMatch == false` && 會員等級不符 | **整個活動跳過**，連未符合文案都不顯示 |
| `IsMatch == false` && 指定客群不符 | **整個活動跳過** |
| `IsDisplayInShoppingCart == false` | **整個活動跳過** |

> 設計意圖：如果這個人根本不符合會員條件，連「未達門檻」都不需要提示他。

### 3.2 商品 scope 過濾 — 同「已符合」邏輯

- `PromotionTags` 不在 `ExcludedProducts` 中
- `PromotionTags` 在 `IncludedProducts` 中（或活動未圈選）
- 沒有 `PromotionExcludedByCoupon` flag（或是給點活動）

### 3.3 加價購商品過濾 — 同「已符合」邏輯

```csharp
if (_purchaseExtraPromotionType.Contains(promotion.Type) == false)
    targetSalepageList = targetSalepageList.Where(x => x.IsPurchaseExtra == false);
```

### 3.4 pair 細部：IsExcluded 貼標（獨立活動互斥）

```csharp
// 商品有 GlobalExclusive flag 且活動 IsMatch=false 且活動 IsIncompatible=true
if (isPromotionMutuallyExclusive && promotion.IsMatch == false && promotion.IsIncompatible)
    promotion.IsExcluded = true;
```

`IsExcluded=true` 的活動後續進入 **PromotionUnavailable**，不進入 MismatchedPromotion。

### 3.5 優惠碼活動：IsMatch=false → null

```csharp
if (_promoCodeTypeList.Contains(promotion.Type) && promotion.IsMatch == false)
    promotion = null;
```

優惠碼活動（KOL、MGO）只顯示有中的，未符合不顯示。

---

## 四、Step C：FilteredPromotion(MismatchedPromotion)

```csharp
case DiscountDisplayTypeEnum.MismatchedPromotion:
    promotionList = skuWithPromotion.PromotionList
        .Where(x => x.IsMatch == false   // 未達門檻
                 && x.IsAppOnly == false // 非 APP 獨享
                 && x.IsExcluded == false // 非被獨立活動排除
                 && x.LackOfRegion != true) // 非缺少地區
        .ToList();
```

### 4.1 線下通路隱藏活動過濾

```csharp
var hiddenPromotionIds = promotionList
    .Where(promotion => IsOfflineChannelHiddenDisplayPromotions(promotion))
    .Select(promotion => promotion.Id);

promotionList = promotionList.Where(p => !hiddenPromotionIds.Contains(p.Id)).ToList();
```

`LackSalesChannel` 包含線下通路（`LocationWizard` 或 `InStore`）的特定活動類型 → 不顯示未符合。

### 4.2 自選券排除

```csharp
// 若使用者用了「排除型」自選券 → 只顯示給點/滿額贈活動的未符合文案
if (isExcludeByCoupon)
{
    var typesToInclude = new [] {
        RewardReachPriceWithPoint, RewardReachPriceWithRatePoint,
        RewardReachPriceWithCoupon, RewardReachPriceWithPoint2, RewardReachPriceWithRatePoint2
        // (若未啟用新版滿額贈也加入 DiscountReachPriceWithFreeGift)
    };
    promotionList = promotionList.Where(x => typesToInclude.Contains(x.Type)).ToList();
}
```

### 4.3 優惠碼互斥活動（isExcludedByPromoCode）

```csharp
// 使用了優惠碼，且優惠碼與折扣活動互斥
// → MismatchedPromotion 只留「無法設定獨立計算」的活動（給點/信用卡折扣/登記）
// → 其他折扣活動從 MismatchedPromotion 移到 PromotionUnavailable
if (isExcludedByPromoCode)
    promotionList = promotionList.Where(x => _incompatibleCalculateTypes.Contains(x.Type)).ToList();
```

**`_incompatibleCalculateTypes` 清單（優惠碼互斥時仍留在未符合的活動）：**

| 活動類型 | 說明 |
|---------|------|
| `RewardReachPriceWithPoint` | 舊版滿額固定給點 |
| `RewardReachPriceWithRatePoint` | 舊版滿額依比例給點 |
| `RewardReachPriceWithPoint2` | 新版滿額固定給點 |
| `RewardReachPriceWithRatePoint2` | 新版滿額依比例給點 |
| `RewardReachPriceWithCoupon` | 滿額送券 |
| `PayTypeReachPriceWithAmount` | 指定信用卡滿額折現 |
| `RegisterReachPiece` | 滿件登記 |
| `RegisterReachPrice` | 滿額登記 |

---

## 五、Step C 續：GetSalepageDiscountDisplayAsync — 產生文案

```csharp
// 只處理 _salepageDisplayPromotionType 清單內的活動
var filteredCartPromotionList = cartPromotionList.Where(x => _salepageDisplayPromotionType.Contains(x.Type));

foreach (var promotion in filteredCartPromotionList)
{
    result.Add(new SalepageDiscountDisplayEntity
    {
        DiscountSource = "Promotion",
        DisplayText    = displayService.GetPromotionText(...),   // 例：「再買 $200 打9折」
        DisplayType    = DiscountDisplayTypeEnum.MismatchedPromotion,
        IsMatched      = false,             // ← 未符合
        PromotionId    = promotion.Id,
        PromotionName  = promotion.Name,
        HasLink        = true
    });
}
```

---

## 六、四種 DisplayType 的完整對應

| DisplayType | IsMatch | IsExcluded | IsAppOnly | 顯示文案 |
|------------|---------|-----------|----------|---------|
| `MatchedPromotion` | `true` | - | - | 已符合（綠色） |
| `MismatchedPromotion` | `false` | `false` | `false` | 未符合（灰色，距離提示） |
| `AppOnly` | `false` | `false` | `true` | APP 獨享 |
| `PromotionUnavailable` | `false` | `true` | - | 無法參與（互斥） |

---

## 七、活動類型範例

### 範例 1：滿件加價購（CartReachPieceExtraPurchase）- 未達門檻

**情境：** 活動需要購買 3 件，目前只有 1 件

```bash
Engine 計算結果：
  promotion.IsMatch = false
  promotion.LackOfPiece = 2  （還差 2 件）

IsSkipDisplayText:
  IsMatch=false 但會員等級符合 → 不跳過 ✅

FilteredPromotion(MismatchedPromotion):
  IsMatch=false ✅ IsAppOnly=false ✅ IsExcluded=false ✅
  → 進入 MismatchedPromotion

GetSalepageDiscountDisplayAsync:
  DisplayText = "再加 2 件可加購優惠商品"
  IsMatched = false

Result:
  discountDisplayList 有 { DisplayType: MismatchedPromotion, IsMatched: false }
  → 購物車顯示「再加 2 件可加購優惠商品」✅
```

---

### 範例 2：滿額打折（DiscountReachPriceWithRate）- 未達門檻

**情境：** 活動需要 $1000，目前購物車 $600

```bash
Engine 計算結果：
  promotion.IsMatch = false
  promotion.LackOfPrice = 400  （還差 $400）

IsSkipDisplayText: 通過

FilteredPromotion(MismatchedPromotion):
  IsMatch=false ✅, IsAppOnly=false ✅, IsExcluded=false ✅
  → 進入 MismatchedPromotion

GetSalepageDiscountDisplayAsync:
  DisplayText = "再買 $400 打9折"
  IsMatched = false

Result: 顯示「再買 $400 打9折」✅
```

---

### 範例 3：滿額打折 + 使用了互斥型優惠碼

**情境：** 使用者輸入了排除折扣活動的優惠碼，購物車未達活動門檻

```bash
isExcludedByPromoCode = true

FilteredPromotion(MismatchedPromotion):
  → 套用互斥過濾：只留 _incompatibleCalculateTypes 清單
  → DiscountReachPriceWithRate 不在清單 → 從 MismatchedPromotion 移除
  → 改到 PromotionUnavailable（由優惠碼互斥邏輯處理）

Result: 「未符合」不顯示此活動；改顯示為「無法參與（與優惠碼互斥）」
```

---

### 範例 4：滿額贈 + 會員等級不符

**情境：** 活動限指定會員等級（黃金卡以上），使用者是一般會員，且 IsMatch=false

```bash
IsSkipDisplayText:
  promotion.IsMatch = false
  memberTierNotMatch = true（等級不符）
  → isSkip = true → 整個活動跳過 ❌

Result: 完全不顯示此活動（已符合和未符合文案都不顯示）
       （設計意圖：對不符合等級的會員，連「你快達到了」的提示都不出現）
```

---

### 範例 5：APP 獨享活動

**情境：** IsMatch=false 且 IsAppOnly=true（僅 APP 可用）

```bash
FilteredPromotion(MismatchedPromotion):
  IsMatch=false ✅ 但 IsAppOnly=true ❌ → 被排除

FilteredPromotion(AppOnly):
  IsMatch=false ✅ IsAppOnly=true ✅ IsExcluded=false ✅
  → 進入 AppOnly

Result: salepage.DiscountDisplayList 有 AppOnly 項目（「共 1 個活動 APP 限定」）
       不顯示在 MismatchedPromotion 位置
```

---

### 範例 6：給點活動 + 使用了排除型自選券

**情境：** 使用者使用排除型自選券，購物車未達給點門檻

```bash
isExcludeByCoupon = true

FilteredPromotion(MismatchedPromotion):
  → 套用自選券過濾：只留 typesToInclude（給點類活動）
  → RewardReachPriceWithPoint 在清單 → 保留 ✅

Result: 仍顯示「再買 $200 送 50 點」的未符合文案
       （設計意圖：給點活動不受自選券互斥影響）
```

---

## 八、登記活動（RegisterReachPiece / RegisterReachPrice）的特殊邏輯

```csharp
// IsSkipDisplayText 中有額外判斷
var isSkip = registerList.Contains(promotion.Type) && promotion.DisplaySetting != "All";
```

- 登記活動的 `DisplaySetting` 若不是 `"All"` → 跳過，不顯示任何文案
- `DisplaySetting == "All"` → 正常走後續流程

---

## 九、最終輸出順序

```bash
salepage.DiscountDisplayList = [
  已符合活動 (MatchedPromotion, IsMatched=true),
  未符合活動 (MismatchedPromotion, IsMatched=false),  ← 本文件範疇
  APP 獨享 (AppOnly),
  無法參與 (PromotionUnavailable)
]
```

> 顯示優先順序：已符合 → 未符合 → APP 獨享 → 無法參與
