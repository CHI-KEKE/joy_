# 滿額加價購 × 買就送互斥 — 分析文件

> 對應 User Story：[#596793](https://91appinc.visualstudio.com/MoneyLogistic/_workitems/edit/596793)  
> 最後更新：2026-03-28　｜　基於最新 MWeb / Shopping / Cart API 程式碼分析

---

## 需求一句話

> 當商品以「滿額加價購」加購品身分進入購物車時，即使該商品在後台設有「買就送」贈品設定，系統也**不應**分配或顯示任何贈品。同一商品以一般方式購買時，買就送行為**不受影響**。

---

## 文件索引

| 文件 | 內容 |
|------|------|
| [01-需求說明.md](./01-需求說明.md) | Story 596793 需求、AC、業務背景 |
| [02-識別機制.md](./02-識別機制.md) | IsPrimary / IsCartExtraPurchase / IsSubAddOns 三者關係與差異 |
| [03-買就送現況流程.md](./03-買就送現況流程.md) | 現有 GetShoppingCartSalePageGiftProcessor 完整流程與 BUG 說明 |
| [04-各層現狀.md](./04-各層現狀.md) | Cart API / Shopping API / MWeb 各層現狀 |
| [05-改動建議.md](./05-改動建議.md) | 唯一需要的程式碼修改（4 行）與影響範圍 |

---

## 涉及專案

| 專案 | 路徑 |
|------|------|
| Cart API | `C:\91APP\Cart\cart2\nine1.cart\src\Nine1.Cart.Web.Api.sln` |
| Shopping API | `C:\91APP\Shopping\src\Nine1.Shopping.Web.Api.sln` |
| MWeb | `C:\91APP\nineyi.webstore.mobilewebmall` |

---

## 核心結論

| 項目 | 結論 |
|------|------|
| BUG 根因 | `GetShoppingCartSalePageGiftProcessor` 只排除 `IsPrimary == false`，未排除 `IsCartExtraPurchase == true` |
| 影響範圍 | 滿額/滿件加購品若商品頁有買就送設定，購物車會顯示贈品、結帳會分配贈品 |
| 修改位置 | **Cart API `GetShoppingCartSalePageGiftProcessor.cs`，新增 4 行** |
| 其他層 | Shopping API、MWeb 均**不需修改** |
