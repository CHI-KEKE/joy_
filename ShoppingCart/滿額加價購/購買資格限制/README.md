# 滿額加價購 × 購買資格限制互斥分析

> Story #596789

## 文件索引

| 檔案 | 說明 |
|------|------|
| [01-需求說明.md](./01-需求說明.md) | Story #596789 需求內容與 AC |
| [02-購買資格限制機制.md](./02-購買資格限制機制.md) | 購買資格限制（RestrictedPurchasesByMember）運作原理 |
| [03-完整流程追蹤.md](./03-完整流程追蹤.md) | 從加購品加入購物車到結帳的各層流程 |
| [04-BUG根因.md](./04-BUG根因.md) | 問題根源與各層影響分析 |
| [05-改動建議.md](./05-改動建議.md) | 最小改動方案與實作位置 |

## 問題摘要

**商品 A 同時設有「購買資格限制」，且被加入滿額加價購活動作為加購品。一般會員（不符資格者）在達成門檻後，仍因購買資格檢核而被阻擋加購或結帳。**

- `ArrangeUnmatchPurchasesQualificationProcessor` 把帶有 `RestrictedPurchasesNotMeet` flag 的商品全部移出購物車主清單
- 沒有例外處理 `IsCartExtraPurchase == true` 的滿額加購品
- 前端顯示「購買資格限制」標籤，結帳拋出 `UnmatchPurchaseQualificationSalePageOnly`

## 修改層

```
只需改 Cart Domain（nine1.cart）
└── ArrangeUnmatchPurchasesQualificationProcessor.cs
    └── RemoveItems()
        └── 新增 && s.IsCartExtraPurchase == false 條件
```
