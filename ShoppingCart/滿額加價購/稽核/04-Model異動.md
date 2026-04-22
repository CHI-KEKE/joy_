# 04 Model 異動

以下欄位**已存在 Redis Cache**，但 NMQv3Worker 的 C# Model 尚未定義，
反序列化時會被忽略，需要補齊。

---

## ShoppingCartClientSalePageEntity

路徑：`src/Nine1.Commerce.Console.BL/Model/PayProcessContext/ShoppingCartClientSalePageEntity.cs`

| 欄位 | 型別 | 說明 |
|------|------|------|
| `IsCartExtraPurchase` | `bool` | 識別是否為滿額加購品 |
| `SpecialPriceId` | `long` | 對應 CartExtraPurchaseList 的 key |

---

## ShoppingCartClientSalePagePromotionEntity

路徑：`src/Nine1.Commerce.Console.BL/Model/PayProcessContext/ShoppingCartClientSalePagePromotionEntity.cs`

| 欄位 | 型別 | 說明 |
|------|------|------|
| `CartExtraPurchaseList` | `List<CartExtraPurchaseInfoEntity>` | 活動的加購品價格規則清單 |

---

## 新增 CartExtraPurchaseInfoEntity（新檔案）

路徑：`src/Nine1.Commerce.Console.BL/Model/PayProcessContext/CartExtraPurchaseInfoEntity.cs`

| 欄位 | 型別 | 說明 |
|------|------|------|
| `SkuId` | `long` | 加購品 SKU |
| `SpecialPriceId` | `long` | 對應 salepage 的 SpecialPriceId |
| `Price` | `decimal` | 商品原價 |
| `NewPrice` | `decimal` | **活動設定的加購價（稽核比對用）** |
| `ConditionId` | `int` | 條件 ID |
| `ConditionPrice` | `decimal` | 滿額門檻金額 |
| `ConditionPiece` | `int` | 滿件門檻數量 |
