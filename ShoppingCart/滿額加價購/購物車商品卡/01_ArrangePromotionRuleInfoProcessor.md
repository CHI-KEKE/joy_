# ArrangePromotionRuleInfoProcessor

> **執行時序**：第 1 步（在所有文案 Processor 之前）  
> **檔案路徑**：`src/BusinessLogic/Nine1.Shopping.BL.Services/CartProcessor/ArrangePromotionRuleInfoProcessor.cs`

---

## 職責

整理購物車活動清單的**基礎資料**，包含：
- 依活動類型排序 (`PromotionTypeComparer`)
- 從 Promotion API 補充每筆活動的附加資訊（圖片 URL、標籤、給點規則、給券規則）
- 處理指定金物流活動的 `IsMatch` 修正
- 彙總給點活動總點數（`TotalPromotionRewardPoints`）
- 彙總給券活動回饋資訊（`PromotionRewardCouponInfoList`）

---

## 實作重點

### 1. 活動排序
```csharp
cartData.PromotionInfoList = cartData.PromotionInfoList
    .OrderBy(x => x.Type, new PromotionTypeComparer());
```
- 使用 `PromotionTypeComparer` 做類型層級排序
- 同一類型的細部排序（新到舊）由下游 `CartPromotionInfoComparer` 在產生文案時處理

### 2. 補充活動資料
從 Promotion API 批次取得活動明細後逐筆補充：
```csharp
promotionInfo.PicUrl = picUrl;
promotionInfo.PromotionRewardPointRules = promotion.PromotionRewardPointRules;
promotionInfo.PromotionRewardCouponRules = promotion.PromotionRewardCouponRules;
promotionInfo.Label = promotion.Label;
promotionInfo.TargetMemberTypeDef = promotion.TargetMemberTypeDef;
```

### 3. 指定金物流活動 IsMatch 修正
若活動有指定金物流 (`PayShippingTargetType != None`) 且 `IsMatch == false`，
則以 `IsMatchExceptPayShipping()` 重新判斷（排除金物流條件的符合狀態）：
```csharp
if (isSpecifiedPayShipping && promotionInfo.IsMatch == false)
{
    promotionInfo.IsMatch = promotionInfo.MatchCondition.IsMatchExceptPayShipping();
}
```

### 4. 給券活動回饋資訊
對 `RewardReachPriceWithCoupon` 類型活動，呼叫 Coupon API 取得券面文字、折數等詳細資訊。

---

## US 599198 需求對應狀態

| 需求 | 狀態 | 說明 |
|------|------|------|
| 活動排序（同一類型依開始時間新到舊） | ✅ 基礎資料準備完成 | 此 Processor 做類型層級排序，細部時間排序由 `CartPromotionInfoComparer` 在文案產生時完成 |
| 跳轉活動詳細頁所需活動資料 | ✅ 補充 `PicUrl`、`Label` | `PromotionId` 從計算層取得，此處補充圖片與標籤 |
| 滿額/滿件加價購活動資料補充 | ✅ 無需特殊處理 | 活動類型為 `CartReachPriceExtraPurchase` / `CartReachPieceExtraPurchase`，統一補充 |

### 備註
此 Processor 本身**不需修改**，US 599198 的缺口在下游 Processor。
