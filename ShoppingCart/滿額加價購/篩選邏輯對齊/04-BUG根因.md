# 04 BUG 根因

## 問題定位

**根因**：SPL `CartExtraPurchaseService.BuildPromotionEntitiesAsync()` 在過濾加購品時，
缺少對「組合商品」與「商品頁關閉」的排除邏輯。

## 現行過濾邏輯（BuildPromotionEntitiesAsync，Line 639 附近）

```csharp
// 現有排除條件
var validSalepages = salepageList
    .Where(x => x.IsSoldOut == false)          // ✅ 已排除：售完
    // Story #596792 修正後新增：
    // .Where(x => x.SellingStartDateTime <= DateTime.Now)
    // .Where(x => x.SellingEndDateTime >= DateTime.Now)
    .ToList();
```

## 缺少的排除條件

| # | 條件 | 欄位 | 狀態 |
|---|------|------|------|
| 1 | 組合商品 | `IsSalePageBundle == true` 需排除 | ❌ 未實作 |
| 2 | 商品頁關閉 | `IsClosed == true` 需排除 | ❌ 未實作 |
| 3 | FB 分享後購買 | `IsShareToBuy`（欄位不存在） | ⚠️ 待確認 |

## CartAddOns 對照基準（SalepageExtraPurchaseService）

CartAddOns 已實作的排除：
```csharp
// SalepageExtraPurchaseService（商品加價購，對照基準）
salepages = salepages
    .Where(x => x.IsClosed == false)           // ✅ 商品頁開啟
    .Where(x => IsSellingTimeValid(x))          // ✅ 販售時間有效
    .Where(x => x.IsInvisible == false)         // ✅ 不隱形
    .ToList();
// 注意：CartAddOns 也沒有排除 IsSalePageBundle 和 IsShareToBuy
```

## 影響範圍

| 影響點 | 說明 |
|--------|------|
| 加購品列表頁 | 可能顯示組合商品或已關閉商品頁的商品 |
| P1 加購區 | 同上（同一 SPL Service 回傳） |
| 結帳成功率 | 使用者點擊已關閉或組合商品後，結帳可能失敗 |
| 品牌形象 | 顯示不應出現的商品，影響商家信任度 |

## 遺留問題（需需求端確認）

### 問題一：AC 要求超出對照基準
- **Story AC** 明確要求排除「組合商品」和「FB 分享後購買商品」
- **CartAddOns（對照基準）** 本身也未排除這兩項
- 若 Story 要求「100% 對照 CartAddOns」，則這兩項都不需改；但 AC 又明確列出需排除
- **需確認**：是否要求滿額加價購超越現有基準？若是，CartAddOns 是否也需一起修正？

### 問題二：IsShareToBuy 欄位缺失
- SPL `GetSalepageListByIdsResponse` 中無 `IsShareToBuy` 欄位
- 即使要實作排除，也需 SPL 端先新增此欄位或提供過濾參數
- **需確認**：IsShareToBuy 排除方案（新增欄位 / 查詢參數 / 暫不處理）
