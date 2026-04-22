# 02 Cache 資料結構

## 重要：兩種 Cache 格式

| | Cart Domain Cache | PayProcessContext Cache |
|---|---|---|
| JSON 格式 | camelCase | **PascalCase（C# 序列化）** |
| Worker 讀取 | ❌ | ✅ **Worker 讀這份** |

Worker 讀的是 **PascalCase** 的 `PayProcessContextEntity`。

---

## 加購品識別：SalePageGroupList

加購品在 `ShoppingCartV2.SalePageGroupList[].SalePageList[]` 的關鍵欄位：

```json
{
  "SalePageId": 1557672,
  "SaleProductSKUId": 2169174,
  "Price": 2,
  "IsPurchaseExtra": false,        ← ❌ 商品加價購才用，滿額加價購是 false
  "IsCartExtraPurchase": true,     ← ✅ 識別滿額加購品的欄位
  "SpecialPriceId": 231056,        ← ✅ 對應活動 price rule 的 key
  "CartExtendInfos": []            ← 空的，不走 CartExtendInfo 機制
}
```

---

## 活動規則：PromotionInfoList

活動資料在 `ShoppingCartV2.PromotionInfoList[]`，
找 `PromotionConditionTypeDef == "CartReachPriceExtraPurchase"` 的那筆：

```json
{
  "Id": 39608,
  "PromotionConditionTypeDef": "CartReachPriceExtraPurchase",
  "IsPromotionMatchCondition": true,
  "CartExtraPurchaseList": [
    {
      "SkuId": 2169174,
      "SpecialPriceId": 231056,
      "Price": 10,          ← 商品原價
      "NewPrice": 2,        ← ✅ 活動設定的加購價（稽核比對用）
      "ConditionId": 1,
      "ConditionPrice": 1,  ← 滿額門檻
      "ConditionPiece": 0
    }
  ]
}
```

---

## 關聯方式

```
SalePage.IsCartExtraPurchase == true
    └─ SalePage.SpecialPriceId (231056)
           └─ PromotionInfoList[PromotionConditionTypeDef="CartReachPriceExtraPurchase"]
                  .CartExtraPurchaseList[SpecialPriceId == 231056].NewPrice
```
