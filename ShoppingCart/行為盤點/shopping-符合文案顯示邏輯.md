# Shopping 購物車商品卡「已符合」文案顯示邏輯

> 對應 Processor：`GetSalepagePromotionDisplayTextProcessor`
> 檔案路徑：`C:\91APP\Shopping\src\BusinessLogic\Nine1.Shopping.BL.Services\CartProcessor\GetSalepagePromotionDisplayTextProcessor.cs`

---

## 一、整體流程總覽

```bash
ExecuteAsync
  │
  ├─ [A] GetSkuWithPromotionList()
  │       → 每個商品 × 每個活動 → 篩選出「有效 pair」
  │       → 每個 pair 帶著 IsMatch（Engine 算好的）
  │
  ├─ [B] ArrangeCartExtraPurchasePromotionDisplayTextAsync()
  │       → 只處理 IsCartExtraPurchase=true 的加購品
  │       → 走另一條路（CartExtraPurchase 類型文案）
  │
  └─ [C] ArrangePromotionDisplayTextAsync()
          │
          ├─ FilteredPromotion(MatchedPromotion)
          │   → 從 SkuWithPromotionList 取出 IsMatch=true 的活動
          │
          └─ GetSalepageDiscountDisplayAsync(MatchedPromotion)
              → 產生 DiscountDisplayList 項目（IsMatched=true）
              → AddRange 到 salepage.DiscountDisplayList
```

---

## 二、前置：PromotionEngine 已算好 IsMatch

Shopping 拿到的 `CartPromotionInfoEntity` 是 Cart 專案算完的：

| 欄位 | 說明 |
|------|------|
| `IsMatch` | Engine 判斷活動是否達門檻（`true` = 已符合） |
| `MatchPromotionSkuDiscounts` | 哪些 SkuId 實際拿到折扣（折扣型活動專用） |
| `IsDisplayInShoppingCart` | 活動本身是否設定在購物車顯示 |
| `ExcludedProducts` | 活動排除的商品 collection tags |
| `IncludedProducts` | 活動圈選的商品 collection tags |

---

## 三、Step A：GetSkuWithPromotionList — 建立商品×活動有效 pair

### 3.1 整個活動先過 IsSkipDisplayText

```csharp
// 以下任一條件成立 → 整個活動跳過，不進入後續流程
isSkip = (promotion.IsMatch == false && (memberTierNotMatch || memberCollectionNotMatch))
       || (promotion.IsDisplayInShoppingCart == false);
```

| 條件 | 說明 |
|------|------|
| `IsDisplayInShoppingCart == false` | 活動設定不在購物車顯示 |
| `IsMatch == false` && 會員等級不符 | 未達門檻且不符合會員等級 → 跳過（連未符合文案也不顯示） |
| `IsMatch == false` && 指定客群不符 | 未達門檻且不在指定客群 → 跳過 |

> ⚠️ 注意：如果 `IsMatch == true`，這條不會跳過，會繼續走。

### 3.2 商品×活動 scope 過濾（targetSalepageList）

```csharp
var targetSalepageList = from salepage in salepageList
    where excludedProducts.Overlaps(salepage.PromotionTags) == false   // 不在排除名單
    where includedProducts.Any() == false || includedProducts.Overlaps(salepage.PromotionTags)  // 在圈選名單
    where (salepage.PromotionFlags.Any(x => x == "PromotionExcludedByCoupon") == false
           || typesToInclude.Contains(promotion.Type))                  // 沒被自選券排除
    select salepage;
```

### 3.3 特殊商品類型過濾

```csharp
// 贈品活動排除虛擬商品
if (AllGift.HasFlag(promotion.Type))
    targetSalepageList = targetSalepageList.Where(x => x.ProductTypeDef != "Virtual");

// 加價購商品過濾【關鍵】
// IsPurchaseExtra=true 的商品，只有在 _purchaseExtraPromotionType 清單內的活動才能通過
if (_purchaseExtraPromotionType.Contains(promotion.Type) == false)
    targetSalepageList = targetSalepageList.Where(x => x.IsPurchaseExtra == false);
```

**`_purchaseExtraPromotionType` 清單（允許 IsPurchaseExtra 商品顯示的活動類型）：**

| 活動類型 | 說明 |
|---------|------|
| `DiscountReachPriceWithFreeGift` | 滿額贈 |
| `DiscountReachPieceWithFreeGift` | 滿件贈 |
| `RewardReachPriceWithPoint` | 舊版滿額固定給點 |
| `RewardReachPriceWithRatePoint` | 舊版滿額依比例給點 |
| `RewardReachPriceWithPoint2` | 新版滿額固定給點 |
| `RewardReachPriceWithRatePoint2` | 新版滿額依比例給點 |
| `RewardReachPriceWithCoupon` | 滿額送券 |
| `CartReachPriceExtraPurchase` | 新版滿額加價購（VSTS615222 新增） |
| `CartReachPieceExtraPurchase` | 新版滿件加價購（VSTS615222 新增） |

### 3.4 每個 pair 的細部過濾

```csharp
// ① 折扣型活動：IsMatch=true 但此 SkuId 不在 MatchPromotionSkuDiscounts → null
//    語意：活動達門檻，但此商品沒有折扣紀錄（可能是其他商品達門檻）
if (_salepageDiscountPromotionType.Contains(promotion.Type)
    && promotion.IsMatch == true
    && promotion.MatchPromotionSkuDiscounts.Any(y => y.SkuId == salepage.SkuId) == false)
    promotion = null;

// ② 優惠碼活動：IsMatch=false → null（優惠碼只顯示有中的）
if (_promoCodeTypeList.Contains(promotion.Type) && promotion.IsMatch == false)
    promotion = null;

// ③ 點加金商品只允許特定活動類型
if (salepage.IsPointsPayPair && !_pointsPayDisplayPromotionTypes.Contains(promotion.Type))
    promotion = null;

// ④ 商品加價購加購品（AddOns Sub）：只允許給點 + 有開開關的活動類型
if (salepage.CartExtendInfoItemTypeDef == "Sub" && HasAddOnsExtendInfo && !addOnsDisplayPromotionTypeList.Contains(promotion.Type))
    promotion = null;

// ⑤ 新版滿額/滿件加價購加購品（IsCartExtraPurchase=true）：白名單過濾
if (salepage.IsCartExtraPurchase && !cartExtraPurchaseDisplayPromotionTypeList.Contains(promotion.Type))
    promotion = null;
```

---

## 四、Step C：FilteredPromotion(MatchedPromotion)

```csharp
case DiscountDisplayTypeEnum.MatchedPromotion:
    promotionList = skuWithPromotion.PromotionList
                                    .Where(x => x.IsMatch)  // ← 就這一條
                                    .ToList();
```

只要 `IsMatch == true`，就進入已符合清單。

---

## 五、Step C 續：GetSalepageDiscountDisplayAsync — 產生文案

```csharp
// 只處理 _salepageDisplayPromotionType 清單內的活動
var filteredCartPromotionList = cartPromotionList.Where(x => _salepageDisplayPromotionType.Contains(x.Type));

foreach (var promotion in filteredCartPromotionList)
{
    // 取得對應 DisplayService（by 活動類型名稱，e.g. CartReachPieceExtraPurchaseDisplayService）
    if (!serviceDic.TryGetValue(promotion.Type, out var displayService)) continue;

    result.Add(new SalepageDiscountDisplayEntity
    {
        DiscountSource = "Promotion",
        DisplayText    = displayService.GetPromotionText(...),
        DisplayType    = DiscountDisplayTypeEnum.MatchedPromotion,
        IsMatched      = true,              // ← 已符合
        PromotionId    = promotion.Id,
        PromotionName  = promotion.Name,
        HasLink        = true
    });
}
```

---

## 六、活動類型範例

### 範例 1：滿件加價購（CartReachPieceExtraPurchase）

**情境：** 購物車內有商品 A（主商品）和商品 B（加購品），活動 40657 已達門檻

```bash
Engine 計算結果：
  promotion.IsMatch = true
  promotion.MatchPromotionSkuDiscounts = [ {SkuId: A}, {SkuId: B} ]

商品 A（普通主商品）：
  Step A: _salepageDiscountPromotionType 包含 CartReachPieceExtraPurchase
    → MatchPromotionSkuDiscounts.Any(y => y.SkuId == A) = true → 保留 ✅
  Step C: IsMatch=true → 進入 MatchedPromotion
  Result: discountDisplayList 有 { DisplayType: MatchedPromotion, IsMatched: true } ✅

商品 A（同時是舊版加購品，IsPurchaseExtra=true）：
  Step A: _purchaseExtraPromotionType 包含 CartReachPieceExtraPurchase（VSTS615222 後）
    → 過濾不執行 → 保留 ✅
  Step A: MatchPromotionSkuDiscounts.Any(y => y.SkuId == A) = true → 保留 ✅
  Step C: IsMatch=true → 進入 MatchedPromotion
  Result: discountDisplayList 有 { DisplayType: MatchedPromotion, IsMatched: true } ✅
```

---

### 範例 2：滿額打折（DiscountReachPriceWithRate）

**情境：** 購物車總金額達 $1000 門檻，活動 IsMatch=true

```bash
Engine 計算結果：
  promotion.IsMatch = true
  promotion.MatchPromotionSkuDiscounts = [ {SkuId: A, TotalDiscount: 50}, {SkuId: B, TotalDiscount: 30} ]

商品 A（一般商品）：
  Step A: MatchPromotionSkuDiscounts.Any(y => y.SkuId == A) = true → 保留 ✅
  Step C: IsMatch=true → MatchedPromotion
  Result: 顯示「已符合 滿 $1000 打9折」✅

商品 C（一般商品，但不在 MatchPromotionSkuDiscounts）：
  Step A: MatchPromotionSkuDiscounts.Any(y => y.SkuId == C) = false → promotion = null ❌
  Result: 此商品不顯示這個活動（活動有中，但此商品沒有折扣紀錄）
```

---

### 範例 3：滿額贈（DiscountReachPriceWithFreeGift）

**情境：** IsMatch=true，贈品是虛擬商品

```bash
商品 A（實體商品）：
  Step A: AllGift.HasFlag(DiscountReachPriceWithFreeGift) = true
    → 排除虛擬商品，但 A 是實體 → 保留 ✅
  Step C: IsMatch=true → MatchedPromotion
  Result: 顯示「已符合 滿$500 送禮品」✅

虛擬商品 V：
  Step A: AllGift.HasFlag → 虛擬商品 → targetSalepageList 被排除 ❌
  Result: 不顯示
```

---

### 範例 4：給點活動（RewardReachPriceWithPoint）

**情境：** IsMatch=true，購物車有一個 IsPurchaseExtra=true 的舊版加購品

```bash
加購品（IsPurchaseExtra=true）：
  Step A: _purchaseExtraPromotionType 包含 RewardReachPriceWithPoint → 過濾不執行 → 保留 ✅
  Step C: IsMatch=true → MatchedPromotion
  Result: 加購品也顯示「已符合 滿$500 贈 100 點」✅
  （設計意圖：加購品也參與給點活動門檻計算，故應顯示）
```

---

## 七、_salepageDiscountPromotionType 清單（需過 MatchPromotionSkuDiscounts 的活動類型）

這些活動類型是「折扣型」，已符合時需確認此 SkuId 有折扣紀錄才顯示：

| 活動類型 | 說明 |
|---------|------|
| `PromoCodeKOLReachPriceAmount/Rate` | KOL 優惠碼 |
| `PromoCodeMGOReachPriceAmount/Rate` | 會員邀請活動 |
| `DiscountNthPieceWithRate/Price/Amount` | 第 N 件打折/固定價/折現 |
| `DiscountReachPieceWithPrice/Rate/Amount` | 任選優惠價/滿件打折/滿件折現 |
| `DiscountReachPriceWithRate/Amount` | 滿額打折/滿額折現 |
| `DiscountReachGroupsPiece` | 紅配綠 |
| `DiscountByMemberWithPrice` | 單品特價 |
| `MultiBuyLowestPriceFree` | 買 X 享 Y 免費 |
| `CartReachPriceExtraPurchase` | 新版滿額加價購 |
| `CartReachPieceExtraPurchase` | 新版滿件加價購 |

---

## 八、最終輸出順序

```bash
salepage.DiscountDisplayList = [
  已符合活動 (MatchedPromotion, IsMatched=true),   ← 本文件範疇
  未符合活動 (MismatchedPromotion, IsMatched=false),
  APP 獨享 (AppOnly),
  無法參與 (PromotionUnavailable)
]
```
