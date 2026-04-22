# 購物車滿額加價購 — 定期購降級需求文件

## 需求說明

> **商品本身設定為定期購交期，當它被選為「購物車滿額加價購」活動的加購品並加入購物車時，應以一般交期（Normal）購買，而非定期購（RegularOrder）。**
>
> 同一商品以一般方式加入購物車時，應保持原本的定期購流程不受影響。

---

## 文件索引

| 檔案 | 內容 |
|------|------|
| [01-識別機制.md](./01-識別機制.md) | 系統如何區分「透過加購UI加入」與「一般加入」 |
| [02-加入購物車流程.md](./02-加入購物車流程.md) | 消費者點選加購品到寫入 DB 的完整鏈路 |
| [03-購物車讀取流程.md](./03-購物車讀取流程.md) | 購物車頁載入時 Processor 執行路徑與前端渲染 |
| [04-各層現狀.md](./04-各層現狀.md) | MWeb / Shopping API / Cart API 目前實作狀態 |
| [05-改動建議.md](./05-改動建議.md) | 唯一需要新增的程式碼與原因說明 |

---

## 涉及專案

| 專案 | 路徑 |
|------|------|
| MWeb | `C:\91APP\nineyi.webstore.mobilewebmall` |
| Shopping API | `C:\91APP\Shopping\src\Nine1.Shopping.Web.Api.sln` |
| Cart API | `C:\91APP\Cart\cart2\nine1.cart\src\Nine1.Cart.Web.Api.sln` |
