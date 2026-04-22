# Feature Toggle 控制鏈

## 一、滿額加價購（CartReachPriceExtraPurchase）的 Feature Toggle

### Toggle Key
```
EnableCartReachPriceExtraPurchase
```

### 控制範圍
- **ShopStaticSetting**（依商店設定）
- 存取介面：`PromotionEngineShopStaticSettingKeyEnum.EnableCartReachPriceExtraPurchase`

### 啟用後的效果
選擇促購活動頁面的「**購物車活動**」分類中，顯示「**滿額加價購**」按鈕。

---

## 二、商品加價購（AddOnsSalepageExtraPurchase）的 Feature Toggle

### Toggle Key
```
EnableAddOns
```

### 控制範圍
- **ShopStaticSetting**（PromotionEngine 區段）
- 透過 API `GET /Api/PromotionEngine/IsShowAddOnsSalepageExtraPurchase` 回傳

### 啟用後的效果
選擇促購活動頁面顯示「**商品加價購**」按鈕。

---

## 三、列入折扣活動開關（僅商品加價購適用）

### Toggle Key
```
IsEnabledAddOnsThresholdEligible
```

### 說明
- 控制商品加價購表單中「列入折扣活動（IsAddOnsThresholdEligible）」欄位是否顯示
- 支援全店設定（shopId=0 作為預設值），單店設定優先
- API：`GET /Api/PromotionEngine/IsEnabledAddOnsThresholdEligible`

---

## 四、前端讀取方式

```typescript
// selectActivityForm.controller.ts
this.PromotionEngineService.getShopStaticSettings({
    shopId: this.ShopId,
    keys: [
        'EnableMultiBuyLowestPriceFree',
        'EnableCartReachPriceExtraPurchase',  // ← 滿額加價購 Toggle
    ],
})
```

---

## 五、Toggle 判斷流程

```
頁面載入
  ↓
呼叫 getShopStaticSettings API
  ↓
找到 Key = 'EnableCartReachPriceExtraPurchase'
  ↓ Enable = true
this.IsEnableCartReachPriceExtraPurchase = true
  ↓
CartPromotionViewModel.PromotionList 加入「滿額加價購」按鈕
```
