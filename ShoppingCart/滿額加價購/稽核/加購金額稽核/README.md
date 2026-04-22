# 加購金額稽核 — 實作規劃索引

## 稽核目標
驗證購物車 Cache 內的「滿額加價購」加購品價格，是否等於 Promotion API 活動設定的加購價。

## 稽核流程概述
1. 從 Cache 找出所有加購品（`IsCartExtraPurchase == true`）
2. 查詢下單當下在期的滿額加價購活動 IDs
3. 用活動 IDs + 加購品 SalePageIds 查出活動設定的加購價
4. 比對 Cache 的價格 vs 活動設定價格

## 文件清單

| 檔案 | 說明 |
|---|---|
| [01-流程總覽.md](./01-流程總覽.md) | 完整稽核流程與各 Step 說明 |
| [02-Model異動.md](./02-Model異動.md) | 需新增的 C# Model 欄位與新 Entity |
| [03-PromotionClient擴充.md](./03-PromotionClient擴充.md) | `INine1PromotionClient` 需新增的兩個方法 |
| [04-Processor實作.md](./04-Processor實作.md) | `CartExtraPurchaseAuditProcessor` 完整實作邏輯 |
| [05-註冊與設定.md](./05-註冊與設定.md) | DI 註冊與 `AuditProcessorDefinitionCenter` 異動 |
