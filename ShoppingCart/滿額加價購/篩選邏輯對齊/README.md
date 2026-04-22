# Story #582440：篩選邏輯對齊 — 核心結論索引

## Story 標題
[滿額加價購][加價購商品] 對照商品加價購的加購頁的篩選邏輯，檢核滿額加價購的加購列表頁與 P1 加購區

## 結論摘要

| 項目 | 結論 |
|------|------|
| 主要修改位置 | SPL `CartExtraPurchaseService.BuildPromotionEntitiesAsync()` |
| 兩個入口是否共用 | ✅ 是，改一處即修正兩個入口 |
| 新增排除：組合商品 | ✅ 可修，欄位 `IsSalePageBundle` 已存在 |
| 新增排除：商品頁關閉 | ✅ 可修，欄位 `IsClosed`（或 `StatusDef`）已存在 |
| 新增排除：FB 分享商品 | ⚠️ `IsShareToBuy` 欄位不存在於 SPL response，需需求端確認處理方式 |
| CartAddOns 一致性問題 | ⚠️ CartAddOns 本身也未排除 Bundle/ShareToBuy，需確認 Story 是否要求一併修正 |

## 文件清單

| 文件 | 說明 |
|------|------|
| [01-需求說明.md](01-需求說明.md) | Story AC 與功能背景 |
| [02-識別機制.md](02-識別機制.md) | 各排除條件欄位來源與識別方式 |
| [03-完整流程追蹤.md](03-完整流程追蹤.md) | 兩個入口的呼叫路徑 |
| [04-BUG根因.md](04-BUG根因.md) | 現行排除邏輯缺口 |
| [05-改動建議.md](05-改動建議.md) | 修改方式與 Task 清單 |

## 關聯
- **Parent Feature**: #564617
- **Sprint**: MoneyLogistic\Sprint 192
- **Area**: MoneyLogistic\Commerce 5
