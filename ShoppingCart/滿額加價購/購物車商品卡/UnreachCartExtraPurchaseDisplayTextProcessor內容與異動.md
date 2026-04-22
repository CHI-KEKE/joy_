# UnreachCartExtraPurchaseDisplayTextProcessor 內容與異動

**檔案路徑**：`Shopping\src\BusinessLogic\Nine1.Shopping.BL.Services\CartProcessor\UnreachCartExtraPurchaseDisplayTextProcessor.cs`

---

## 職責

處理「**加購品本身**」的商品卡文案，涵蓋以下情境：
- 活動失效 / APP 限定 → 無法購買文案
- 未達門檻（滿額 / 滿件）→ 差額提示文案

> **只處理加購品**，不處理主商品。  
> 主商品文案由 `GetSalepagePromotionDisplayTextProcessor` 負責。

---

## 處理的清單來源

| 清單 | 由誰填入 | 內容 |
|---|---|---|
| `ExpiredPromotionSalepageList` | Cart `ArrangeCartExtraPurchaseUnreachThresholdProcessor` | 活動失效 / APP限定 的加購品 |
| `UnreachAmountCartExtraPurchaseSalepageList` | Cart `ArrangeCartExtraPurchaseUnreachThresholdProcessor` | 未達門檻的加購品（滿額 + 滿件） |

兩個清單都加上 `.Where(x => x.IsCartExtraPurchase)` 雙重過濾，確保只處理加購品。

---

## 文案類型總覽

| # | 文案 | DisplayType | 來源清單 |
|---|---|---|---|
| 1 | `本商品因活動條件變更，目前無法加購` | `Disabled` | `ExpiredPromotionSalepageList` |
| 2 | `本商品為 App 限定商品，請至 App 購買` | `Disabled` | `ExpiredPromotionSalepageList` |
| 3 | `再湊 $N 享加價購優惠` | `MismatchedPromotion` | `UnreachAmountCartExtraPurchaseSalepageList` |
| 4 | `再湊 N件 享加價購優惠` | `MismatchedPromotion` | `UnreachAmountCartExtraPurchaseSalepageList` |

文案 1/2 為「無法購買」狀態；文案 3/4 為「可達成但尚未達成」狀態。

---

## 執行流程

### 清單一：ExpiredPromotionSalepageList（文案 1 / 2）

```
foreach salepage in ExpiredPromotionSalepageList.Where(IsCartExtraPurchase):
  │
  ├─ 預設文案 = "本商品因活動條件變更，目前無法加購"
  │
  ├─ BuildCartPromotionEngineMapping() → promotionEngineId
  │     ├─ 查不到 → 使用預設文案（文案 1）
  │     └─ 查到 → 取 promotionInfo
  │                 ├─ promotionInfo == null → 使用預設文案（文案 1）
  │                 └─ IsMatch == false && LackSalesChannel 含 AppAndroid|AppIOS
  │                       → 文案 2："本商品為 App 限定商品，請至 App 購買"
  │
  └─ 寫入 salepage.DiscountDisplayList（DisplayType = Disabled）
```

### 清單二：UnreachAmountCartExtraPurchaseSalepageList（文案 3 / 4）

```
foreach salepage in UnreachAmountCartExtraPurchaseSalepageList.Where(IsCartExtraPurchase):
  │
  ├─ BuildCartPromotionEngineMapping() → promotionEngineId
  │
  ├─ [滿額 filter] PurchasedItemPrice > 0 && LackOfPrice > 0
  │     └─ 命中 →
  │           thresholdCondition = CartExtraPurchases
  │                                  .FirstOrDefault(SpecialPriceIdList.Contains(optionalTypeId))
  │           差額 = Math.Abs(thresholdCondition.ReachPrice - PurchasedItemPrice)
  │           → 文案 3："再湊 $N 享加價購優惠"（DisplayType = MismatchedPromotion）
  │
  └─ [滿件 filter] Type == CartReachPieceExtraPurchase
                   && (PurchasedItemPiece > 0 || LackPieceCount > 0)
        └─ 命中 →
              thresholdCondition = CartExtraPurchases
                                     .FirstOrDefault(SpecialPriceIdList.Contains(optionalTypeId))
              lackPiece = PurchasedItemPiece > 0
                  ? Math.Abs(thresholdCondition.ReachPiece - PurchasedItemPiece)  ← 精確
                  : LackPieceCount                                                  ← fallback
              → 文案 4："再湊 N件 享加價購優惠"（DisplayType = MismatchedPromotion）
```

> **if/else 的判斷維度說明**：loop 是「加購品維度」，每個加購品透過 `CartId → mapping → promotionEngineId` 找到**唯一一筆** `promotionInfo`。if/else 是針對「這個加購品所屬的那個活動」做類型分支，不存在同一加購品同時屬於滿額與滿件的情況，因此 else 是安全的。

---

## 本次異動內容（T-Bug2）

### 異動前問題

原本 `UnreachAmountCartExtraPurchaseSalepageList` 分支只有一個 filter：

```csharp
var promotionInfo = context.Data.PromotionInfoList.FirstOrDefault(
    x => x.Id == promotionEngineId && x is { PurchasedItemPrice: > 0, LackOfPrice: > 0 });
```

滿件加價購的 `PurchasedItemPrice` 和 `LackOfPrice` 永遠為 0 → filter 永遠 `null` → 整筆 `continue` → **加購品完全沒有文案**。

### 異動後修復

| 項目 | 修改內容 |
|---|---|
| filter 邏輯 | 拆成 if（滿額）/ else（滿件）兩個分支；兩者天然互斥（同一加購品只屬於一個活動，一個活動只有一種類型） |
| 滿件 filter | `Type == CartReachPieceExtraPurchase && (PurchasedItemPiece > 0 \|\| LackPieceCount > 0)` |
| 差額計算 | `PurchasedItemPiece > 0 ? \|ReachPiece - PurchasedItemPiece\| : LackPieceCount` |
| 格式串 | 新增 `UnreachPieceThresholdHint`（7 種語系） |
| 重構 | `ExtractPromotionEngineId()` 抽成獨立 private method |

### `lackPiece` fallback 精確度

| 活動結構 | `lackPiece` 來源 | 正確性 |
|---|---|---|
| 單一門檻 | `LackPieceCount` | ✅ 精確 |
| 多門檻，此加購品屬最低門檻 | `LackPieceCount` | ✅ 精確 |
| 多門檻，此加購品屬更高門檻 | `LackPieceCount`（低估） | ⚠️ 近似 |
| NuGet 2.25.1 後（任何結構） | `\|ReachPiece - PurchasedItemPiece\|` | ✅ 精確 |

---

## BuildCartPromotionEngineMapping 說明

從 PromotionFlags 解析 `promotionEngineId`，建立 `cartId → promotionEngineId` 的 mapping。

```
PromotionFlags 格式："{PromotionEngineId}:CartExtraPurchaseSpecialPriceId:{SpecialPrice_Id}"
例如：             "101:CartExtraPurchaseSpecialPriceId:99"
                    ↑ promotionEngineId = 101
```

解析邏輯由 `ExtractPromotionEngineId()` 負責（本次重構抽出），解析失敗回傳 0 並略過該商品。
