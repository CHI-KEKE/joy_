# APP 獨賣商品機制分析

## 概念釐清

> ⚠️ **兩個不同的 APP 獨享概念，容易混淆**

| 概念 | 欄位/判斷 | 對象 | 說明 |
|------|-----------|------|------|
| **商品的 APP 獨賣** | `SalePage.SalePage_IsAPPOnly` | 商品本身 | 商品被設定為只在 APP 上賣 |
| **活動的 APP 獨享** | `PromotionEngineEntity.IsAppOnly` (computed) | 促購活動 | 活動通路設定中 Web=false、iOS=true、Android=true |

---

## 商品的 APP 獨賣（`IsAPPOnly`）

### 資料位置

| 層 | 位置 | 欄位 |
|----|------|------|
| DB | `WebStore/DA/WebStoreDBV2/Tables/SalePage.cs` L75 | `SalePage_IsAPPOnly (Nullable<bool>)` |
| Entity | `WebStore/Frontend/BE/SalePage/SalePageEntity.cs` L276 | `IsAPPOnly` |

### 控制機制

**不是後端權限阻擋，而是「讓商品不出現在入口」：**

| 入口 | 機制 |
|------|------|
| FTS 搜尋列表 | `SearchIsAPPOnlyTypeEnum`（`True / False / All`）參數過濾 |
| 熱銷排行 | 硬碼 `IsAPPOnly = false`，直接排除 |
| 直連 URL | **完全沒有阻擋**，知道網址可直接進入 |

### 唯一真正的阻擋

```csharp
// PromotionEngineController.cs L903
if (promoCodeResult.IsAppOnly == true && userClientTrack.IsApp == false)
{
    // 阻擋非 APP 使用者使用 PromoCode
}
```
只有 **PromoCode 兌換**時才有真正的後端阻擋。

---

## 活動的 APP 獨享（`PromotionEngineEntity.IsAppOnly`）

### 計算邏輯

```csharp
// PromotionEngineEntity.cs
public bool IsAppOnly
{
    get
    {
        //// 只判斷線上通路 APP/Web
        //// 小幫手/門市視為線下通路，不加入 APP 獨享的判斷
        return this.IsWeb == false && (this.IsAndroidApp && this.IsiOSApp);
    }
}
```

即：**Web 關閉 + iOS 開啟 + Android 開啟 = APP 獨享活動**

### 後端過濾（活動列表）

```csharp
// PromotionEngineService.cs
private bool IsVisiblePromotion(PromotionEngineEntity entity, UserClientTrackSourceTypeEnum source)
{
    switch (source)
    {
        case Web:     return entity.IsWebVisible;
        case iOSApp:  return entity.IsiOSAppVisible;
        case Android: return entity.IsAndroidAppVisible;
        case LocationWizard: return entity.IsLocationWizardVisible;
    }
}

// 活動列表查詢時
list = list.Where(x => this.IsVisiblePromotion(x, searchCriteria.Source));
```

Web 消費者在**活動列表頁**看不到 APP Only 的活動。

### 前端使用（活動詳細頁菜籃）

```tsx
// basketAction.tsx
const isShowAppDownload = (): boolean => {
    return (
        props.isAPPOnlyPromotion &&
        (/* 會員條件 */)
    );
};
// isAPPOnlyPromotion = true → 顯示「下載 APP」按鈕（取代加入購物車）
```

```tsx
// promotionCard.tsx
{props.isAPPOnlyPromotion && (
    // 顯示 APP Only 標籤
)}
```

---

## `SalePageDisplayTagEnum` 標籤優先順序

```csharp
// [Flags]
public enum SalePageDisplayTagEnum
{
    MemberOnly       = 1,    // 最高優先
    AppOnlyPromotion = 2,    // 活動 APP 獨享（不是商品 IsAPPOnly）
    ExclusiveLimit   = 4,
    RegularOrder     = 8,
    BuyFreeGift      = 16,
    FreeShipping     = 32,
    PurchaseExtra    = 64,
    OverseasDelivery = 128,
}
```

> 注意：`AppOnlyPromotion` 是**活動**的 APP 獨享標籤，不是商品本身的 `IsAPPOnly`。
