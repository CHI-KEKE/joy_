# 購物車商品卡 — 各文案及 HighlightTag 資料流確認

> 對應 User Story：#599194 [前台][購物車] 加購品商品卡：加購品加入購物車後的顯示資訊  
> 活動類型：**購物車滿件加價購（CartReachPieceExtraPurchase）**

---

## 一、整體架構

```
Client (Webstore)
    │
    │  GET /api/carts/{cartUniqueKey}
    ▼
╔══════════════════════════════════════════════════════╗
║  Shopping API  CartGetAsync                          ║
║  ProcessorTypeEnum.CartGet 依序執行：                 ║
║                                                      ║
║  Layer 1: CartGetProcessor                           ║
║  Layer 2: ArrangePromotionRuleInfoProcessor          ║
║  Layer 3: GetSalepagePromotionDisplayTextProcessor   ║
║  Layer 4: (parallel) Unreach... / GetDiscount...     ║
║  Layer 5: ArrangeHighlightTagProcessor               ║
╚══════════════════════════════════════════════════════╝
```

---

## 二、各 Layer 資料流說明

### Layer 1：CartGetProcessor

```
Shopping ──POST /api/carts/get──▶ Cart API
                                      │
                           CalculatePromotionProcessor
                                      │
                           ──POST /api/cart-calculate──▶ PromotionFrontend API
                                                              │
                                                   ParsePromotionEngineRuleObject
                                                   （CartReachPieceExtraPurchaseRuleService）
                                                              │
                                                   ──查詢 DB──▶ PromotionEngine Table
                                                              │  PromotionEngineSpecialPrice Table
                                                              │
                                                   ◀── CartExtraPurchaseConditions
                                                       （含 SpecialPriceIdList）──
                                      │
                           回傳 CartEntity（已含）：
                           ┌─ SalepageGroupList
                           │   └─ SalepageEntity
                           │       ├─ IsCartExtraPurchase = true   ← 加購品旗標
                           │       ├─ OptionalTypeId               ← SpecialPrice_Id
                           │       └─ PromotionFlags               ← 含活動排除旗標
                           ├─ PromotionInfoList
                           │   └─ CartPromotionInfoEntity
                           │       ├─ Type = CartReachPieceExtraPurchase
                           │       ├─ IsMatch                      ← 是否符合門檻
                           │       ├─ LackPieceCount               ← 差幾件
                           │       ├─ PurchasedItemPiece           ← 已購件數
                           │       ├─ CartExtraPurchases[]         ← SpecialPriceIdList（本次修正補上）
                           │       └─ CartExtraPurchasePricePairList[]  ← SkuId + SpecialPriceId + ConditionPiece
                           └─ UnreachAmountCartExtraPurchaseSalepageList ← 未達門檻的加購品
```

> **重要**：`CartExtraPurchases[].SpecialPriceIdList` 由本次修正（#599198）在  
> `CartReachPieceExtraPurchaseRuleService.ParsePromotionEngineRuleObject` 中補上，  
> 是 Layer 4 能正確計算「差幾件」的前提。

---

### Layer 2：ArrangePromotionRuleInfoProcessor

```
Shopping ──GET promotionIds──▶ PromotionService（內部 DB）
                                      │
                              補充 promotionInfo：
                              ├─ PicUrl（活動圖片）
                              ├─ Label（活動標籤）
                              └─ PromotionRewardPointRules（給點規則）
```

---

### Layer 3：GetSalepagePromotionDisplayTextProcessor

本 Story 最核心的 Processor，負責產生三類商品卡文案：

#### ① 活動標籤文案（DiscountDisplayTypeEnum.CartExtraPurchase）

```
條件：salepage.IsCartExtraPurchase == true
      promotionInfo.IsMatch == true（已符合門檻）
      promotionInfo.Type == CartReachPieceExtraPurchase

呼叫：
  CartReachPieceExtraPurchaseDisplayService.GetPromotionHintText(
      promotionInfo,
      DiscountDisplayTypeEnum.CartExtraPurchase,
      skuId,
      optionalTypeDef,
      optionalTypeId   ← 對應 SpecialPriceId
  )

邏輯：
  1. 查 CartExtraPurchasePricePairList，找 SkuId + SpecialPriceId 相符的 ConditionPiece
  2. 組合文案格式："{Name}{CumulativePieceThreshold} {N}件"
     例：「活動名稱 累積滿 3件」

輸出：
  salepage.DiscountDisplayList.Add({
      DiscountSource = "CartExtraPurchase",
      DisplayText    = "活動名稱 累積滿 3件",
      DisplayType    = CartExtraPurchase
  })
```

#### ② 已符合提示（DiscountDisplayTypeEnum.MatchedPromotion）綠色 icon

#### ③ 未符合提示（DiscountDisplayTypeEnum.MismatchedPromotion）橘色 icon

```
條件：salepage.IsCartExtraPurchase == true

加購品的活動顯示白名單（預設）：
  ├─ RewardReachPriceWithPoint
  ├─ RewardReachPriceWithRatePoint
  ├─ RewardReachPriceWithPoint2
  ├─ RewardReachPriceWithRatePoint2
  └─ RewardReachPriceWithCoupon

依 PromotionFlags 動態擴充白名單：
  ├─ DiscountReachPriceWithFreeGift（滿額贈）
  │   → 排除條件：PromotionFlags 含 CartExtraPurchaseDiscountReachPriceWithFreeGiftExcluded
  ├─ RegisterReachPiece（滿件登記）
  │   → 排除條件：PromotionFlags 含 CartExtraPurchaseRegisterReachPieceExcluded
  └─ RegisterReachPrice（滿額登記）
      → 排除條件：PromotionFlags 含 CartExtraPurchaseRegisterReachPriceExcluded

邏輯：
  非白名單的活動 → promotion = null（不顯示）
  白名單的活動 → 依 IsMatch 決定 MatchedPromotion / MismatchedPromotion

輸出：
  salepage.DiscountDisplayList.Add({
      DiscountSource = "RegisterReachPiece",
      DisplayText    = "...",
      DisplayType    = MatchedPromotion 或 MismatchedPromotion
  })
```

---

### Layer 4（parallel）：UnreachCartExtraPurchaseDisplayTextProcessor

處理「未達門檻」加購品的文案（活動失效 / 未達件數）：

```
來源清單：
  ├─ ExpiredPromotionSalepageList（活動失效）
  └─ UnreachAmountCartExtraPurchaseSalepageList（未達門檻）
  條件：IsCartExtraPurchase == true

【活動失效】邏輯：
  └─ 預設：PromotionConditionChangedOrExpired（活動條件已變更或過期）
      若 IsMatch == false && 銷售通路為 APP 限定
      → DisplayText = PromotionConditionAppOnly（APP 限定提示）

【未達門檻】邏輯：
  1. 從 PromotionFlags 解析 PromotionEngineId
     格式：{PromotionEngineId}:CartExtraPurchaseSpecialPriceId:{SpecialPrice_Id}
  2. 找 PromotionInfoList 中對應的 CartReachPieceExtraPurchase 活動
  3. 查 CartExtraPurchases.FirstOrDefault(
         x => x.SpecialPriceIdList.Contains(salepage.OptionalTypeId)
     )                           ← 依賴本次修正補上的 SpecialPriceIdList
  4. 計算差額：
     lackPiece = PurchasedItemPiece > 0
         ? Math.Abs(thresholdCondition.ReachPiece - PurchasedItemPiece)
         : piecePromotionInfo.LackPieceCount
  5. DisplayText = string.Format(UnreachPieceThresholdHint, lackPiece)
     例：「再買 2 件即可加購」

【折價券不適用】（永遠顯示）：
  PromotionFlags 含 ECouponExcludedByPromotion
  → DiscountSource = "Coupon"
  → DisplayText = "不適用折價券"
  → DisplayType = Disabled

【點數不適用】（後台設定時顯示）：
  PromotionFlags 含 LoyaltyPointExcluded
  → DiscountSource = "LoyaltyPoint"
  → DisplayText = "不適用點數"
  → DisplayType = Disabled
```

---

### Layer 5：ArrangeHighlightTagProcessor

```
條件：salepage.IsCartExtraPurchase == true

輸出：
  salepage.HighlightTagList.Add({
      TagType  = "CartExtraPurchase",   ← 觸發前端顯示「加價購」紅底白字標籤
      TagValue = "CartExtraPurchase",
      Sort     = 4
  })
```

---

## 三、最終 Response 結構（商品卡欄位）

```json
{
  "SalepageGroupList": [{
    "SalepageList": [{
      "Title": "加購品名稱",
      "Price": 99,
      "IsCartExtraPurchase": true,
      "HighlightTagList": [
        {
          "TagType": "CartExtraPurchase",
          "TagValue": "CartExtraPurchase"
        }
      ],
      "DiscountDisplayList": [
        {
          "DiscountSource": "CartExtraPurchase",
          "DisplayText": "活動名稱 累積滿 3件",
          "DisplayType": "CartExtraPurchase"
        },
        {
          "DiscountSource": "DiscountReachPriceWithFreeGift",
          "DisplayText": "已符合：滿額贈 活動名稱",
          "DisplayType": "MatchedPromotion"
        },
        {
          "DiscountSource": "RegisterReachPiece",
          "DisplayText": "享 活動名稱",
          "DisplayType": "MismatchedPromotion"
        },
        {
          "DiscountSource": "Coupon",
          "DisplayText": "不適用折價券",
          "DisplayType": "Disabled"
        }
      ]
    }]
  }],
  "UnreachAmountCartExtraPurchaseSalepageList": [{
    "DiscountDisplayList": [
      {
        "DiscountSource": "CartExtraPurchase",
        "DisplayText": "再買 2 件即可加購",
        "DisplayType": "MismatchedPromotion"
      },
      {
        "DiscountSource": "Coupon",
        "DisplayText": "不適用折價券",
        "DisplayType": "Disabled"
      }
    ]
  }]
}
```

---

## 四、需求 vs 資料流對應彙整

| 需求顯示項目 | Layer | 關鍵程式位置 | 關鍵資料來源 | 狀態 |
|---|---|---|---|---|
| 「加價購」紅底白字標籤 | Layer 5 | `ArrangeHighlightTagProcessor` | `IsCartExtraPurchase` (from Cart) | ✅ 已實作 |
| 活動標籤文案「滿件加購 + 名稱 + 累積滿 N 件」 | Layer 3 | `CartReachPieceExtraPurchaseDisplayService` | `CartExtraPurchasePricePairList[].ConditionPiece` | ✅ 已實作 |
| 已符合提示（綠色）- 滿額贈 | Layer 3 | `GetSalepagePromotionDisplayTextProcessor` | `PromotionFlags` 排除旗標 + 白名單 | ✅ 已實作 |
| 已符合提示（綠色）- 滿件登記 | Layer 3 | `GetSalepagePromotionDisplayTextProcessor` | `PromotionFlags` 排除旗標 + 白名單 | ✅ 已實作 |
| 已符合提示（綠色）- 滿額登記 | Layer 3 | `GetSalepagePromotionDisplayTextProcessor` | `PromotionFlags` 排除旗標 + 白名單 | ✅ 已實作 |
| 未符合提示（橘色）- 同上三種活動 | Layer 3 | `GetSalepagePromotionDisplayTextProcessor` | `IsMatch` 判斷 | ✅ 已實作 |
| 未達門檻差幾件文案 | Layer 4 | `UnreachCartExtraPurchaseDisplayTextProcessor` | `CartExtraPurchases[].SpecialPriceIdList` | ✅ 本次修正補上（#599198） |
| 不適用折價券提示（永遠顯示） | Layer 4 | `UnreachCartExtraPurchaseDisplayTextProcessor` | `PromotionFlags: ECouponExcludedByPromotion` | ✅ 已實作 |
| 不適用點數提示（後台設定時顯示） | Layer 4 | `UnreachCartExtraPurchaseDisplayTextProcessor` | `PromotionFlags: LoyaltyPointExcluded` | ✅ 已實作 |

---

## 五、關鍵修正說明（#599198）

**問題**：`CartReachPieceExtraPurchaseRuleService.ParsePromotionEngineRuleObject`  
未呼叫 `GetCartExtraPurchaseSpecialPriceListAsync`，導致 `CartExtraPurchases[].SpecialPriceIdList` 永遠為空。

**影響**：Layer 4 的 `UnreachCartExtraPurchaseDisplayTextProcessor` 中：
```csharp
// line 149：SpecialPriceIdList 為空 → thresholdCondition 永遠 null → 文案不顯示
var thresholdCondition = piecePromotionInfo.CartExtraPurchases
    .FirstOrDefault(x => x.SpecialPriceIdList.Contains(salepage.OptionalTypeId));
```

**修正**：補上與滿額版（`CartReachPriceExtraPurchaseRuleService`）相同的邏輯，  
在 `ParsePromotionEngineRuleObject` 中查詢 `PromotionEngineSpecialPrice` table，  
依 `ConditionId` 分組填入每個條件的 `SpecialPriceIdList`。

---

## 六、相關程式碼位置

| 檔案 | 說明 |
|------|------|
| `Shopping/CartGetProcessor.cs` | Layer 1，呼叫 Cart API `api/carts/get` |
| `Shopping/ArrangePromotionRuleInfoProcessor.cs` | Layer 2，補充活動資訊 |
| `Shopping/GetSalepagePromotionDisplayTextProcessor.cs` | Layer 3，活動標籤 + 已/未符合文案 |
| `Shopping/UnreachCartExtraPurchaseDisplayTextProcessor.cs` | Layer 4，未達門檻文案 + 折價券/點數不適用 |
| `Shopping/ArrangeHighlightTagProcessor.cs` | Layer 5，HighlightTag 加購品紅標籤 |
| `Shopping/CartReachPieceExtraPurchaseDisplayService.cs` | 滿件加價購文案組合邏輯 |
| `PromotionFrontend/CartReachPieceExtraPurchaseRuleService.cs` | ParsePromotionEngineRuleObject（本次修正） |
