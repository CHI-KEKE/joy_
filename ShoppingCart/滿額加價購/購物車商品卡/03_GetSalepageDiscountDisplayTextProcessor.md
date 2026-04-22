# GetSalepageDiscountDisplayTextProcessor

> **執行時序**：第 3 步（與其他 Processor 並行）  
> **檔案路徑**：`src/BusinessLogic/Nine1.Shopping.BL.Services/CartProcessor/GetSalepageDiscountDisplayTextProcessor.cs`

---

## 職責

處理商品卡**折價券相關**的不適用文案，以及購物車中無法參與結帳的商品（無交集商品）固定文案。  
所有文案均為 `DisplayType = Disabled`（灰色純提示）。

---

## 實作重點

### 1. 處理範圍
- `SalepageGroupList` 中的一般商品（排除贈品券商品、商品加價購加購品）
- `UnMappingCheckoutSalepageList`（無交集商品）
- `UnMappingGroupedCheckoutSalepageList`（分群無交集商品）

### 2. 商品加價購加購品跳過
```csharp
if (salePage.CartExtendInfoItemTypeDef == nameof(CartExtendInfoItemTypeEnum.Sub) &&
    salePage.CartExtendInfos.Any(y => y.RuleTypeDef == nameof(CartExtendInfoRuleTypeDefEnum.AddOnsSalepageExtraPurchase)))
{
    continue;
}
```
> 舊版商品加價購（AddOns）的加購品不在此處理。

### 3. 折價券不適用判斷邏輯（一般商品）
```
isPromoCodeSelected && isPromotionMutuallyExclusive
  → "已優先使用優惠碼，無法再享折扣活動優惠"

isExcludePromotionByCoupon
  → "已優先使用折價券，無法再享折扣活動優惠"

hasCoupon:
  isExcludeCouponDiscount || salePage.IsPointsPayPair
    → "不適用折價券"
  isExcludeCouponByPromotion && selectedCouponId == 0
    → "已享有折扣，與折價券不可合併使用"
  !isApplicableSelectedCoupon && selectedCouponId > 0
    → "不適用目前選擇的折價券"
```

### 4. 舊版滿額加價購加購品（`IsPurchaseExtra`）
```csharp
if (salePage.IsPurchaseExtra)
{
    //// 不適用折價券
    text = GetSalepageDiscountDisplayText.NotFitCoupons;
}
```
此判斷在最後覆蓋，確保加購品一定顯示「不適用折價券」。

### 5. Flag 說明
| Flag | 說明 |
|------|------|
| `ECouponExcludedByPromotion` | 活動排除折價券（促購引擎設定） |
| `GlobalSalepageExcludedByCoupon` | 商品本身設定排除折價券 |
| `GlobalExclusive` | 活動互斥 |
| `$matched_coupon:{slaveId}` | 商品有使用到的折價券 ID |

---

## US 599198 需求對應狀態

| 需求 | 狀態 | 說明 |
|------|------|------|
| 舊版加價購（`IsPurchaseExtra`）不適用折價券 | ✅ 已實作 | 行 124-128 |
| **新版購物車加價購（`IsCartExtraPurchase`）活動範圍品** 顯示「不適用折價券」 | ❌ 未處理 | 滿額/滿件加價購活動本身不可與折價券合併，但範圍品未加判斷 |
| **新版購物車加價購（`IsCartExtraPurchase`）加購品** 顯示「不適用折價券」 | ❌ 未處理 | 加購品跳過邏輯只排除舊版 AddOns，新版加購品仍會跑進主流程，但未補充折價券不適用判斷 |

### 需修改項目
在適當位置補上以下判斷（類似 `IsPurchaseExtra` 的做法）：

```csharp
// 範圍品：符合新版購物車加價購活動（IsCartExtraPurchase 為加購品，範圍品需另外用 PromotionFlags 判斷）
// 加購品：IsCartExtraPurchase == true

if (salePage.IsCartExtraPurchase)
{
    //// 購物車滿額/滿件加價購加購品 不適用折價券
    text = GetSalepageDiscountDisplayText.NotFitCoupons;
}
```

> 範圍品的判斷方式需確認是否透過 `PromotionFlags` 包含特定加價購活動旗標來識別，  
> 或由上游計算層補充 `IsCartExtraPurchaseRangeProduct` 之類的屬性。
