# GetLoyaltyPointExcludedDisplayTextProcessor

> **執行時序**：第 3 步（與其他 Processor 並行）  
> **檔案路徑**：`src/BusinessLogic/Nine1.Shopping.BL.Services/CartProcessor/GetLoyaltyPointExcludedDisplayTextProcessor.cs`

---

## 職責

偵測商品的 `PromotionFlags` 是否包含 `LoyaltyPointExcluded` 旗標，  
若有則在商品卡加入「不適用點數折抵」提示文案（`DisplayType = Disabled`）。

---

## 實作重點

### 1. 核心邏輯
```csharp
const string loyaltyPointExecludedflag = "LoyaltyPointExcluded";

foreach (var salePage in salepageList)
{
    if (salePage.PromotionFlags.Any(x => x.Contains(loyaltyPointExecludedflag)) == false)
    {
        continue;
    }

    salePage.DiscountDisplayList.Add(new SalepageDiscountDisplayEntity
    {
        DiscountSource = SourceTypeEnum.LoyaltyPoint.ToString(),
        DisplayText = GetLoyaltyPoints.LoyaltyPointExecluded,  // "不適用點數折抵"
        DisplayType = DiscountDisplayTypeEnum.Disabled
    });
}
```

### 2. 處理範圍
- 僅處理 `SalepageGroupList` 中的一般商品（已勾選商品）
- 未勾選商品、加購品未達門檻清單**不在**處理範圍內

### 3. Flag 設定來源
`LoyaltyPointExcluded` 旗標由 **Cart 計算層**（`nine1.cart` 專案）在計算時打入商品的 `PromotionFlags`。  
條件是：活動設定了「不能使用點數」，且該商品屬於此活動的範圍品。

### 4. 文案
| Key | 文案（預設） |
|-----|------------|
| `GetLoyaltyPoints.LoyaltyPointExecluded` | 不適用點數折抵 |

---

## US 599198 需求對應狀態

| 需求 | 狀態 | 說明 |
|------|------|------|
| 滿件加價購後台設定「不能使用點數」→ 活動範圍品顯示「不適用點數折抵」 | ⚠️ 機制存在，但需確認 | Processor 機制正確，關鍵在 Cart 計算層是否對滿件加價購的**範圍品**打 `LoyaltyPointExcluded` flag |
| 滿件加價購後台設定「不能使用點數」→ **加購品**顯示「不適用點數折抵」 | ⚠️ 機制存在，但需確認 | 同上，需確認 Cart 計算層是否對**加購品**也打 `LoyaltyPointExcluded` flag |
| 滿件加價購未設定「不能使用點數」→ 不顯示 | ✅ 邏輯正確 | Flag 不存在則不加入文案 |

### 待確認事項
1. **Cart 計算層**（`nine1.cart`）在處理 `CartReachPieceExtraPurchase` 時，是否對範圍品和加購品打 `LoyaltyPointExcluded` flag？
2. 若 Cart 計算層**尚未實作**此 flag 邏輯，則需在 Cart 專案補充，Shopping 層此 Processor **不需修改**。
3. 若 Cart 計算層**已實作**，則此 Processor 對滿件加價購天然支援，無需修改。

### 確認方向
- 查閱 `nine1.cart` 中處理 `CartReachPieceExtraPurchase` 的計算邏輯
- 搜尋 `LoyaltyPointExcluded` flag 在 Cart 專案中的設定位置
- 確認此 flag 是否有覆蓋加購品（`IsCartExtraPurchase`）的商品
