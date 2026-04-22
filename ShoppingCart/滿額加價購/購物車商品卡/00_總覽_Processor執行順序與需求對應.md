# 購物車 P1 商品卡文案 Processor 總覽

> **對應 User Story**：[599198] [前台][購物車] P1活動商品商品卡：顯示活動資訊、符合活動提示icon、引導至活動詳細頁  
> **活動類型聚焦**：`CartReachPriceExtraPurchase`（購物車滿額加價購）、`CartReachPieceExtraPurchase`（購物車滿件加價購）

---

## Processor 執行順序（CartGetProcessorLayers / CalculateProcessorLayers 共用）

```
┌─────────────────────────────────────────────────────────────┐
│ Step 1  ArrangePromotionRuleInfoProcessor                   │
│         整理活動基礎資料（排序、補充圖片/給點/給券規則）           │
├─────────────────────────────────────────────────────────────┤
│ Step 2  GetSalepagePromotionDisplayTextProcessor            │
│         產生主要活動文案（已符合/未符合/APP獨享/無法參與/加購品）   │
├─────────────────────────────────────────────────────────────┤
│ Step 3  [並行執行以下所有 Processors]                         │
│  ├─ GetSalepageDiscountDisplayTextProcessor                 │
│  │    折價券相關不適用文案                                     │
│  ├─ UpdateLoyaltyPointDisplayStatusProcessor                │
│  │    更新點數顯示狀態                                        │
│  ├─ GetInsufficientPointsSalepageDisplayTextProcessor       │
│  │    點數不足點加金商品提示                                   │
│  ├─ GetPurchaseExtraDisplayTextProcessor（舊版）             │
│  │    舊版滿額加價購未達門檻文案                                │
│  ├─ GetDesignatePaymentPromotionDisplayTextProcessor        │
│  │    指定金流活動文案                                        │
│  ├─ GetLoyaltyPointExcludedDisplayTextProcessor             │
│  │    點數折抵排除（不適用點數折抵）文案                         │
│  ├─ GetUnmatchSalepageBundleDisplayTextProcessor            │
│  │    組合商品不符合規則提示                                   │
│  ├─ GetInStoreOnlySalepageDisplayTextProcessor              │
│  │    門市限定商品提示                                        │
│  └─ UnreachCartExtraPurchaseDisplayTextProcessor            │
│       新版購物車加價購加購品未達門檻/失效/APP限定文案             │
├─────────────────────────────────────────────────────────────┤
│ Step 4  ArrangeHighlightTagProcessor                        │
│         整理商品卡標籤（溫層/交期/虛擬/點數/加價購等）            │
└─────────────────────────────────────────────────────────────┘
```

---

## 各 Processor 對 US 599198 需求影響摘要

| Processor | 對應需求 | 狀態 |
|-----------|---------|------|
| `ArrangePromotionRuleInfoProcessor` | 活動排序基礎資料 | ✅ 無需修改 |
| `GetSalepagePromotionDisplayTextProcessor` | 活動名稱/排序/跳轉/滿件加購品文案 | ❌ **需修改 3 處** |
| `GetSalepageDiscountDisplayTextProcessor` | 折價券不適用（範圍品+加購品） | ❌ **需修改** |
| `GetLoyaltyPointExcludedDisplayTextProcessor` | 點數不適用提示 | ⚠️ **需確認 Cart 計算層 flag** |
| `UnreachCartExtraPurchaseDisplayTextProcessor` | 滿件未達門檻件數提示 | ❌ **需補件數邏輯** |
| `GetPurchaseExtraDisplayTextProcessor` | 舊版加價購（無關） | ⬛ 無需修改 |
| `ArrangeHighlightTagProcessor` | 加購品標籤 | ✅ 無需修改 |

---

## 需求 → Processor 對應矩陣

| US 599198 需求 | 主要 Processor | 狀態 |
|--------------|--------------|------|
| 活動資訊區塊（有資料才顯示、無資料隱藏） | `GetSalepagePromotionDisplayTextProcessor` | ✅ 已實作 |
| 活動名稱正確顯示（`PromotionName`） | `GetSalepagePromotionDisplayTextProcessor` | ✅ 已實作 |
| 同一類型活動依開始時間新到舊排序 | `GetSalepagePromotionDisplayTextProcessor`（`CartPromotionInfoComparer`） | ✅ 已實作 |
| 點擊活動名稱跳轉活動詳細頁（`PromotionId` + `HasLink`） | `GetSalepagePromotionDisplayTextProcessor` | ✅ 已實作 |
| **滿件加價購**加購品文案（`CartExtraPurchase`） | `GetSalepagePromotionDisplayTextProcessor` | ❌ 行 363 未篩選滿件 |
| **滿件加價購**未達門檻件數提示 | `UnreachCartExtraPurchaseDisplayTextProcessor` | ❌ 只有金額，缺件數 |
| 活動範圍品「不適用折價券」提示 | `GetSalepageDiscountDisplayTextProcessor` | ❌ 無 `IsCartExtraPurchase` 判斷 |
| 加購品「不適用折價券」提示 | `GetSalepageDiscountDisplayTextProcessor` | ❌ 無 `IsCartExtraPurchase` 判斷 |
| 範圍品「不適用點數折抵」（後台設定） | `GetLoyaltyPointExcludedDisplayTextProcessor` | ⚠️ 需確認 Cart flag |
| 加購品「不適用點數折抵」（後台設定） | `GetLoyaltyPointExcludedDisplayTextProcessor` | ⚠️ 需確認 Cart flag |

---

## 需新增/修改的檔案清單

| # | 類型 | 檔案 | 說明 |
|---|------|------|------|
| 1 | 修改 | `GetSalepagePromotionDisplayTextProcessor.cs` | 行 363 補 `CartReachPieceExtraPurchase`；行 150/212 修正 Enum Bug |
| 2 | 新增 | `CartReachPieceExtraPurchaseDisplayService.cs` | 滿件加價購 DisplayService，實作件數門檻文案 |
| 3 | 修改 | `GetSalepageDiscountDisplayTextProcessor.cs` | 補 `IsCartExtraPurchase` 折價券不適用邏輯 |
| 4 | 確認/修改 | `nine1.cart` 計算層 | 確認是否對滿件加價購範圍品/加購品打 `LoyaltyPointExcluded` flag |
| 5 | 修改 | `UnreachCartExtraPurchaseDisplayTextProcessor.cs` | 補滿件未達門檻的件數差距提示邏輯 |
