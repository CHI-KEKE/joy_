# LackOfPiece 再湊件數資料流

> 適用活動類型：`CartReachPieceExtraPurchase`（滿件加價購）

---

## 背景說明

Engine 的 `CartExtraPurchaseRuleBase.HintInfo` 使用 `LackOfPiece` 作為「差幾件達門檻」的欄位名稱，
與其他滿件活動（DiscountReachPiece* 等）使用的 `LackPieceCount` **命名不同**。

Cart 的 `HintInfo` 原本只有 `LackPieceCount`，導致 JSON 反序列化時靜默丟棄 `LackOfPiece`，
造成 `LackPieceCount` 永遠為 0，最終文案顯示「再湊 0 件」。

---

## 修復方式

| 檔案 | 異動 |
|------|------|
| `Cart/.../Responses/HintInfo.cs` | 新增 `public int LackOfPiece { get; set; }` 接收 Engine 回傳的 JSON key |
| `Cart/.../CalculatePromotionProcessor.cs` line 669 | 當 `promotionType == CartReachPieceExtraPurchase` 時，改用 `hintState.LackOfPiece` 映射至 `promotionInfo.LackPieceCount` |

---

## 完整資料流

### ① Engine（in-process in PromotionFrontendAPI）

```
CartReachPieceExtraPurchase.MismatchedProcess()
└─ 建立 DisplayHintInstruction {
     PromotionRuleId = {活動ID}
     SourceType      = ExtraPurchase
     State = CartExtraPurchaseRuleBase.HintInfo {
       LackOfPiece        = 3   ← 距門檻差幾件
       PurchasedItemPiece = 2   ← 目前已購件數
       LackOfPrice        = 0   ← 滿件不使用金額
       PurchasedItemPrice = 0   ← 同上
     }
   }
```

> `DisplayHintInstruction.State` 宣告型別為 `object`，序列化時以 runtime type 輸出所有欄位。

---

### ② PromotionFrontendAPI → HTTP Response JSON

```json
{
  "PromotionInstructionList": [
    {
      "PromotionRuleId": 123,
      "SourceType": "ExtraPurchase",
      "State": {
        "LackOfPiece": 3,
        "PurchasedItemPiece": 2,
        "LackOfPrice": 0,
        "PurchasedItemPrice": 0
      }
    }
  ]
}
```

> 使用 `System.Text.Json` 序列化。

---

### ③ Cart 接收並反序列化

```
PromotionHttpClient.ParseToType<CalculateResponseEntity>()
└─ System.Text.Json.Deserialize（PropertyNameCaseInsensitive = true）

HintInstruction.State → HintInfo {
  LackOfPiece        = 3   ✅ 新增欄位，正確接收
  PurchasedItemPiece = 2   ✅ 名稱吻合，一直正確
  LackPieceCount     = 0   （其他活動用，此場景 JSON 無此 key，維持預設值）
}
```

---

### ④ Cart CalculatePromotionProcessor（line 669）

```csharp
// 滿件加價購 Engine 回傳 LackOfPiece（命名與其他活動不同）
promotionInfo.LackPieceCount =
    promotionType == CartReachPieceExtraPurchase
        ? hintState.LackOfPiece          // = 3  ✅
        : promotionType != DiscountReachGroupsPiece
            ? hintState.LackPieceCount   // 其他一般滿件活動
            : hintState.Groups.Values    // 紅配綠
                .Where(x => x.GroupsLimitCycleCount == 0)
                .Sum(x => x.NextCycleLakeItemsCount);

promotionInfo.PurchasedItemPiece = hintState.PurchasedItemPiece; // = 2  ✅
```

**Cart 傳給 Shopping 的 `CartPromotionInfoEntity`：**

```
LackPieceCount     = 3
PurchasedItemPiece = 2
Type               = CartReachPieceExtraPurchase
```

---

### ⑤ Shopping 組出文案

#### 情境一：主商品卡（未達門檻）— `MismatchedPromotion`

```
CartReachPieceExtraPurchaseDisplayService.GetLackOfCondition()
└─ return promotionInfo.LackPieceCount.ToString()  →  "3"

DisplayTypeDescription = string.Format("再湊 {0} 件", "3")
→ "再湊 3 件"  ✅
```

---

#### 情境二：加購品卡（未達門檻）— `UnreachCartExtraPurchaseDisplayTextProcessor`

```
else 分支（滿件）
條件：PurchasedItemPiece > 0 || LackPieceCount > 0  →  true（LackPieceCount = 3）

threshold  = thresholdCondition.ReachPiece  （門檻件數，例如 5）
totalPiece = promotionInfo.PurchasedItemPiece  （= 2）
lackPiece  = threshold - totalPiece  = 5 - 2 = 3

UnreachPieceThresholdHint = "再湊 3 件享加價購優惠"  ✅
```

---

#### 情境三：加購品卡（已達門檻）— `CartExtraPurchase`

```
CartReachPieceExtraPurchaseDisplayService.GetPromotionHintText(CartExtraPurchase)
└─ CartExtraPurchasePricePairList.FirstOrDefault(skuId, specialPriceId)
   .ConditionPiece  →  N

→ "{活動名稱}累積滿件 N件"  ✅
```

---

## 欄位命名對照表

| 層級 | 欄位名稱 | 說明 |
|------|---------|------|
| Engine `CartExtraPurchaseRuleBase.HintInfo` | `LackOfPiece` | 差幾件（Engine 私有命名，與其他活動不同）|
| Engine 其他滿件活動 HintInfo | `LackPieceCount` | 差幾件（其他活動統一命名）|
| Cart `HintInfo` | `LackOfPiece` | 橋接欄位，接收 Engine JSON |
| Cart `HintInfo` | `LackPieceCount` | 其他活動共用欄位 |
| Cart `CartPromotionInfoEntity` | `LackPieceCount` | 統一儲存差距件數，Shopping 消費 |
| Cart `HintInfo` | `PurchasedItemPiece` | 已購件數（命名一致，無問題）✅ |
| Cart `CartPromotionInfoEntity` | `PurchasedItemPiece` | 已購件數，傳至 Shopping ✅ |
