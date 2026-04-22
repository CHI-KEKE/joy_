# ArrangeHighlightTagProcessor

> **執行時序**：第 4 步（最後執行，所有文案 Processor 完成後）  
> **檔案路徑**：`src/BusinessLogic/Nine1.Shopping.BL.Services/CartProcessor/ArrangeHighlightTagProcessor.cs`

---

## 職責

為購物車中**每一個商品頁（SalepageEntity）**整理並產生 `HighlightTagList`，  
即商品卡左上角的各種標籤（溫層、交期、活動、虛擬商品、點數、加價購等）。

---

## 實作重點

### 1. 處理對象（全部商品類型）
```
SalepageGroupList          → 一般商品（已勾選）
UnMappingCheckoutSalepageList        → 無交集商品
UnMappingGroupedCheckoutSalepageList → 分群無交集商品
UnCheckedSalepageGroupList           → 未勾選商品
UnreachAmountPurchaseExtraSalePageList     → 舊版加價購未達門檻
UnmatchPurchasesQualificationSalePageList → 未符合購買資格
ExpiredPromotionSalepageList              → 活動失效商品
UnreachAmountCartExtraPurchaseSalepageList → 新版購物車加價購未達門檻
```

### 2. 標籤排序
```csharp
var tagTypeSort = new Dictionary<HighlightTagTypeEnum, int>()
{
    { TemperatureTypeDef,       0 },  // 溫層
    { SaleProductShippingTypeDef, 0 }, // 交期
    { PromotionSettingLabel,    1 },  // 商店客製活動標籤
    { VirtualProduct,           2 },  // 虛擬商品
    { PointsPayPair,            3 },  // 點數兌換
    { PurchaseExtra,            4 },  // 舊版加價購加購品
    { CartExtraPurchase,        4 },  // 新版加價購加購品（同序）
    { BookingPickupDate,        5 },  // 指定到貨日
};
```

### 3. 各標籤加入條件

| 標籤 | 加入條件 |
|------|---------|
| `TemperatureTypeDef` | 永遠加入（預設 "Normal"） |
| `SaleProductShippingTypeDef` | `ShippingTypeDef.HasValue == true` |
| `PromotionSettingLabel` | `PromotionSettingLabelList.Any()` |
| `BookingPickupDate` | `IsEnabledBookingPickupDate == true` |
| `VirtualProduct` | `ProductTypeDef == "Virtual"` |
| `PointsPayPair` | `PointsPayPair != null && PairsPoints > 0` |
| `PurchaseExtra` | `IsPurchaseExtra == true`（舊版加價購加購品） |
| `CartExtraPurchase` | `IsCartExtraPurchase == true`（新版加價購加購品） |

### 4. PurchaseExtra vs CartExtraPurchase
```csharp
//// 滿額加價購（舊版）
if (salePage.IsPurchaseExtra == true)
{
    highlightTags.Add(new HighlightTagEntity { TagType = "PurchaseExtra", ... });
}

//// 購物車滿額/滿件加價購活動的加購品（新版）
if (salePage.IsCartExtraPurchase)
{
    highlightTags.Add(new HighlightTagEntity { TagType = "CartExtraPurchase", ... });
}
```

---

## US 599198 需求對應狀態

| 需求 | 狀態 | 說明 |
|------|------|------|
| 加購品顯示 `CartExtraPurchase` 標籤 | ✅ 已實作 | `IsCartExtraPurchase == true` → 加入標籤 |
| 未達門檻加購品（`UnreachAmountCartExtraPurchaseSalepageList`）也處理標籤 | ✅ 已實作 | 行 95-101，此清單也走 `GetHighlightTagList()` |
| 活動失效加購品（`ExpiredPromotionSalepageList`）也處理標籤 | ✅ 已實作 | 行 86-92 |
| 滿額/滿件加價購活動範圍品的標籤 | ✅ 無需新增 | 範圍品本身不是加購品，不需 `PurchaseExtra` 或 `CartExtraPurchase` 標籤 |

### 備註
此 Processor **不需修改**，US 599198 的標籤需求已由現有邏輯覆蓋。
