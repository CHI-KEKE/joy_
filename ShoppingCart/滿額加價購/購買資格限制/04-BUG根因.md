# BUG 根因分析

## 根因定位

**位置**：`ArrangeUnmatchPurchasesQualificationProcessor.RemoveItems()`

```csharp
// 問題程式碼（L77 附近）
var items = group.SalepageList
    .Where(s => s.PromotionFlags.Contains(
        nameof(PromotionFlagDefEnum.RestrictedPurchasesNotMeet)))
    .ToList();
// ❌ 沒有排除 IsCartExtraPurchase == true 的滿額加購品
```

---

## 為什麼加購品會有 `RestrictedPurchasesNotMeet` Flag？

`CalculatePromotionProcessor` 呼叫 Promotion Engine API 時，傳入的是**整個購物車的商品清單**，包含加購品。Promotion Engine 針對每個商品的購買資格做檢查，不區分商品是否為加購品身份，因此加購品也會被打上此 flag。

```
Promotion Engine 的視角：
  商品 A → 有購買資格限制 + 當前會員不符資格
         → 打上 RestrictedPurchasesNotMeet                ← 無論何種身份
         
Cart Domain 的視角（現行）：
  有 RestrictedPurchasesNotMeet flag → 全部移出主清單  ← 沒考慮加購品身份
```

---

## `SalepageEntity.IsCartExtraPurchase` 已存在

`SalepageEntity`（Cart Domain 內的核心 Entity）有 `IsCartExtraPurchase` 計算屬性：

```csharp
// SalepageEntity.cs
public bool IsCartExtraPurchase
{
    get => ArrangeIsCartExtraPurchase();
}

private bool ArrangeIsCartExtraPurchase()
{
    result = OptionalTypeDef is
        nameof(SalePageOptionalTypeEnum.CartReachPriceExtraPurchase) or
        nameof(SalePageOptionalTypeEnum.CartReachPieceExtraPurchase) &&
        OptionalTypeId > 0;
    return result;
}
```

**這個屬性就在 `ArrangeUnmatchPurchasesQualificationProcessor` 能使用的同一個 Entity 上**，只是目前 `RemoveItems()` 沒有用到它。

---

## 為什麼其他層不能/不應修

| 層 | 原因 |
|----|------|
| Promotion Engine | 不應改：PE 的職責是檢核資格本身，不應知道「加購品」的業務例外 |
| `CalculatePromotionProcessor` | 不應改：它只是把 PE 的 flags 傳回，不應過濾 |
| Checkout（`MergeRequestAndCartContextProcessor`） | 上游修好後自動正確，不需改 |
| 前端（MWeb） | 上游修好後不再收到此 flag，自動正確，不需改 |

---

## 加購品選購清單（Modal）另行說明

`ExtraPurchaseProductList.tsx` 中已有：
```tsx
<ThemeCoreProductCard
    ...
    isRestricted={false}   ← 已硬碼，Modal 不顯示限制標籤
    ...
/>
```
AC 1 對 Modal 的部分**已符合**，無需修改。需要修改的是購物車內已加入的加購品。
