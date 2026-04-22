# 02 — Model 異動

## 異動檔案清單

| 檔案路徑 | 異動類型 |
|---|---|
| `ShoppingCartClientSalePageEntity.cs` | 新增欄位 |
| `ShoppingCartClientSalePagePromotionEntity.cs` | 新增欄位 |
| `CartExtraPurchaseInfoEntity.cs` | 新建 Entity |

---

## 1. ShoppingCartClientSalePageEntity

**路徑：** `src/Nine1.Commerce.Console.BL/Model/PayProcessContext/ShoppingCartClientSalePageEntity.cs`

新增以下兩個欄位：

```csharp
/// <summary>
/// 是否為滿額加價購加購品
/// </summary>
public bool IsCartExtraPurchase { get; set; }

/// <summary>
/// 加價購特殊價格序號（對應活動設定的 SpecialPrice.Id）
/// </summary>
public long SpecialPriceId { get; set; }
```

**Redis 對應欄位（PascalCase cache）：**
```json
{
  "IsCartExtraPurchase": true,
  "SpecialPriceId": 231056
}
```

---

## 2. ShoppingCartClientSalePagePromotionEntity

**路徑：** `src/Nine1.Commerce.Console.BL/Model/PayProcessContext/ShoppingCartClientSalePagePromotionEntity.cs`

新增以下欄位：

```csharp
/// <summary>
/// 滿額加價購商品清單（PromotionConditionTypeDef == "CartReachPriceExtraPurchase" 時存在）
/// </summary>
public List<CartExtraPurchaseInfoEntity> CartExtraPurchaseList { get; set; }
```

**Redis 對應欄位：**
```json
{
  "PromotionId": 39608,
  "PromotionConditionTypeDef": "CartReachPriceExtraPurchase",
  "CartExtraPurchaseList": [
    {
      "SkuId": 123,
      "SpecialPriceId": 231056,
      "Price": 100,
      "NewPrice": 2,
      "ConditionId": 1,
      "ConditionPrice": 500,
      "ConditionPiece": 0
    }
  ]
}
```

---

## 3. CartExtraPurchaseInfoEntity（新建）

**路徑：** `src/Nine1.Commerce.Console.BL/Model/PayProcessContext/CartExtraPurchaseInfoEntity.cs`

```csharp
namespace Nine1.Commerce.Console.BL.Model.PayProcessContext
{
    /// <summary>
    /// 滿額加價購活動加購品資訊
    /// </summary>
    public class CartExtraPurchaseInfoEntity
    {
        /// <summary>
        /// 商品選項序號
        /// </summary>
        public long SkuId { get; set; }

        /// <summary>
        /// 加價購特殊價格序號
        /// </summary>
        public long SpecialPriceId { get; set; }

        /// <summary>
        /// 原價
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 活動設定加購價
        /// </summary>
        public decimal NewPrice { get; set; }

        /// <summary>
        /// 門檻序號
        /// </summary>
        public int ConditionId { get; set; }

        /// <summary>
        /// 門檻金額
        /// </summary>
        public decimal ConditionPrice { get; set; }

        /// <summary>
        /// 門檻件數
        /// </summary>
        public int ConditionPiece { get; set; }
    }
}
```

> **注意：** `CartExtraPurchaseInfoEntity` 中的 `NewPrice` 是 Cache 快照，本次稽核**不使用**它作為比對來源（避免 cache 自己比自己），而是以 Promotion API 回傳的 `TypeValue` 為準。
