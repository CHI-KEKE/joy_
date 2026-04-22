# PurchasedItemPiece 欄位說明

## 用途

**「使用者目前累積符合活動的商品件數」**，對應既有的 `PurchasedItemPrice`（已購金額）的件數版本。

| 欄位 | 類型 | 語意 |
|---|---|---|
| `PurchasedItemPrice`（既有） | `decimal` | 符合活動條件的商品已購**金額** |
| `PurchasedItemPiece`（新增） | `int` | 符合活動條件的商品已購**件數** |

---

## 新增位置

| 層 | 檔案 | 變更 |
|---|---|---|
| Cart BE | `Nine1.Cart.BL.BE\Promotion\Responses\HintInfo.cs` | 新增 `public int PurchasedItemPiece { get; set; }` |
| Cart BE | `Nine1.Cart.BL.BE\Promotion\CartPromotionInfoEntity.cs` | 新增 `public int PurchasedItemPiece { get; set; }` |
| Cart Service | `Nine1.Cart.BL.Services\Processor\Promotion\CalculatePromotionProcessor.cs` line 687 | `promotionInfo.PurchasedItemPiece = hintState.PurchasedItemPiece` |
| Shopping BE | `Nine1.Shopping.BL.BE\Promotion\CartPromotionInfoEntity.cs` | 新增 `public int PurchasedItemPiece { get; set; }` |

---

## 資料流

```
【PromotionFrontendAPI — Engine NuGet】
  CartReachPieceExtraPurchase.MismatchedProcess()
    └─ hintState.PurchasedItemPiece = 已購符合活動條件的件數
       (⚠️ 待 NuGet 2.25.1，目前回傳 0)

【Cart — CalculatePromotionProcessor】
  line 686: promotionInfo.PurchasedItemPrice = hintState.PurchasedItemPrice  (既有)
  line 687: promotionInfo.PurchasedItemPiece = hintState.PurchasedItemPiece  (新增)
    → 序列化為 JSON 傳給 Shopping

【Shopping — CartPromotionInfoEntity】
  反序列化接收 PurchasedItemPiece

【Shopping — UnreachCartExtraPurchaseDisplayTextProcessor】
  line 130: var lackPiece = piecePromotionInfo.PurchasedItemPiece > 0
                ? Math.Abs(thresholdCondition.ReachPiece - PurchasedItemPiece)  ← 精確
                : LackPieceCount                                                  ← fallback
```

---

## 消費點

目前 Shopping 專案中 `PurchasedItemPiece` **只有一個消費點**：

| 檔案 | 行為 |
|---|---|
| `UnreachCartExtraPurchaseDisplayTextProcessor.cs` line 130 | 計算加購品還差幾件才能達到所屬門檻，產生「再湊 N件 享加價購優惠」文案 |

---

## 為什麼需要這個欄位（而非直接用 `LackPieceCount`）

`LackPieceCount` = 距**最近門檻**還差幾件，是整個活動層級的**單一數字**。

當活動有**多門檻**時（例如滿 3 件可買加購品 A、滿 5 件可買加購品 B）：

| 購物車現況 | `LackPieceCount` | 加購品 A 正確文案 | 加購品 B 正確文案 |
|---|---|---|---|
| 已購 2 件 | 1（距最近門檻 3） | 再湊 **1** 件 ✅ | 再湊 **3** 件 ✅ |

若只用 `LackPieceCount`，加購品 B 會錯誤顯示「再湊 **1** 件」（低估）。

`PurchasedItemPiece` 讓每個加購品可自行計算：
```
lackPiece = ReachPiece（該條件門檻） - PurchasedItemPiece（已購件數）
```

---

## 現階段行為（NuGet 2.25.1 前）

- Engine `hintState.PurchasedItemPiece` 尚未填值，傳入值為 **0**
- `UnreachCartExtraPurchaseDisplayTextProcessor` 判斷 `PurchasedItemPiece > 0` 為 false
- **自動 fallback 至 `LackPieceCount`**，單一門檻活動結果精確，多門檻活動為近似值
- 現有邏輯**不受影響**，零破壞性風險

---

## NuGet 2.25.1 升版後

Engine 填入 `PurchasedItemPiece` → `PurchasedItemPiece > 0` 判斷通過 → 自動切換至精確計算，**不需要額外程式碼修改**。
