# GetSalepagePromotionDisplayTextProcessor

> **執行時序**：第 2 步  
> **檔案路徑**：`src/BusinessLogic/Nine1.Shopping.BL.Services/CartProcessor/GetSalepagePromotionDisplayTextProcessor.cs`

---

## 職責

產生商品卡**主要活動文案區塊**，依序處理以下類型並寫入 `SalepageDiscountDisplayEntity`：

| DisplayType | 說明 |
|-------------|------|
| `MatchedPromotion` | 已符合活動 |
| `MismatchedPromotion` | 未符合活動 |
| `AppOnly` | APP 獨享活動提示 |
| `PromotionUnavailable` | 無法參與活動 |
| `CartExtraPurchase` | 購物車加價購加購品活動標籤 |

---

## 實作重點

### 1. 主要流程
```
GetSkuWithPromotionList()            → 建立 商品 ↔ 活動 mapping
FilteredPromotion()                  → 依 DisplayType 篩選各類型活動清單
GetSalepageDiscountDisplayAsync()    → 呼叫各活動 DisplayService 產生文案
ArrangeCartExtraPurchasePromotionDisplayTextAsync() → 加購品文案
```

### 2. 活動 DisplayService 架構
依活動類型動態查找對應的 Service（命名規則：`{TypeName}DisplayService`）：
```csharp
var serviceName = $"{type}DisplayService";
var service = _promotionDisplayServices.FirstOrDefault(x => x.GetType().Name == serviceName);
```
目前有實作 `CartReachPriceExtraPurchaseDisplayService`，**尚未實作** `CartReachPieceExtraPurchaseDisplayService`。

### 3. 加購品文案產生 `ArrangeCartExtraPurchasePromotionDisplayTextAsync`
```csharp
var cartExtraPurchaseSalepageList = context.Data.SalepageGroupList
    .SelectMany(x => x.SalepageList)
    .Where(x => x.IsCartExtraPurchase)
    .ToList();

var cartExtraPurchasePromotionList = context.Data.PromotionInfoList
    .Where(x => x is {
        SourceType: SourceTypeEnum.Promotion,
        Type: PromotionEngineTypeDefEnum.CartReachPriceExtraPurchase  // ← 只取滿額
    })
    .ToList();
```
> ⚠️ **Bug**：Filter 只篩選 `CartReachPriceExtraPurchase`，滿件 `CartReachPieceExtraPurchase` 未被處理。

### 4. `_salepageDiscountPromotionType`（活動類型白名單）
```csharp
//// 購物車滿額加價購
PromotionEngineTypeDefEnum.CartReachPriceExtraPurchase,
//// 購物車滿件加價購
PromotionEngineTypeDefEnum.CartReachPriceExtraPurchase,  // ← Bug: 應為 CartReachPieceExtraPurchase
```

### 5. `_promotionSupportMemberCollectionTypes`（MemberCollection 檢核白名單）
同樣的 Bug 出現在此清單（行 210-212），滿件加價購使用了錯誤的 Enum。

### 6. 購物車加價購加購品活動類型白名單
`IsCartExtraPurchase == true` 的加購品，只允許顯示以下類型的活動文案：
- `RewardReachPriceWithPoint` / `RewardReachPriceWithRatePoint`（給點）
- `RewardReachPriceWithPoint2` / `RewardReachPriceWithRatePoint2`
- `RewardReachPriceWithCoupon`（給券）
- `DiscountReachPriceWithFreeGift`（滿額贈，需無 `CartExtraPurchaseDiscountReachPriceWithFreeGiftExcluded` flag）
- `RegisterReachPiece` / `RegisterReachPrice`（需無對應 Excluded flag）

### 7. SalepageDiscountDisplayEntity 產出欄位
```csharp
new SalepageDiscountDisplayEntity
{
    DiscountSource = "Promotion",
    DisplayText = ...,             // 文案本體
    DisplayTypeDescription = ..., // 區塊標題描述（已符合/未符合等）
    PartialDisplayTypeDescription = ...,
    DisplayType = displayTypeEnum,
    IsMatched = ...,
    PromotionId = ...,             // 跳轉用（優惠碼/單品特價不含）
    PromotionName = ...,           // 活動名稱
    HasLink = ...                  // 是否可點擊跳轉
}
```

### 8. 活動排序（Comparer 使用時機）
`CartPromotionInfoComparer`：
- 主要排序：活動類型（`_promotionTypeSorts` 陣列順序）
- 次要排序：活動開始時間**由新到舊**（`y.StartDateTime.CompareTo(x.StartDateTime)`）
- 末位排序：活動 ID 由大到小

---

## US 599198 需求對應狀態

| 需求 | 狀態 | 說明 |
|------|------|------|
| 已符合活動文案顯示 | ✅ 已實作 | `FilteredPromotion(MatchedPromotion)` |
| 未符合活動文案顯示 | ✅ 已實作 | `FilteredPromotion(MismatchedPromotion)` |
| APP 獨享活動提示 | ✅ 已實作 | `FilteredPromotion(AppOnly)` |
| 無法參與活動提示 | ✅ 已實作 | `FilteredPromotion(PromotionUnavailable)` |
| 活動名稱顯示 | ✅ 已實作 | `SalepageDiscountDisplayEntity.PromotionName` |
| 活動排序（開始時間新到舊） | ✅ 已實作 | `CartPromotionInfoComparer` |
| 點擊活動名稱跳轉活動詳細頁 | ✅ 已實作 | `PromotionId` + `HasLink = true` |
| **滿件加價購** 加購品文案（`CartExtraPurchase`） | ❌ 未處理 | 行 363 Filter 只取 `CartReachPriceExtraPurchase`，需補 `CartReachPieceExtraPurchase` |
| `_salepageDiscountPromotionType` 滿件正確 Enum | ❌ Bug | 行 150 誤用 `CartReachPriceExtraPurchase`，應為 `CartReachPieceExtraPurchase` |
| `_promotionSupportMemberCollectionTypes` 滿件正確 Enum | ❌ Bug | 行 212 同上 |

### 需修改項目
1. **行 363**：補上 `CartReachPieceExtraPurchase` 篩選條件
2. **行 150**：修正 `_salepageDiscountPromotionType` 中滿件的 Enum
3. **行 212**：修正 `_promotionSupportMemberCollectionTypes` 中滿件的 Enum
4. 新增 `CartReachPieceExtraPurchaseDisplayService` 並實作件數門檻文案格式
