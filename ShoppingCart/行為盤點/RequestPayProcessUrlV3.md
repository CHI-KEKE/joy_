# RequestPayProcessUrlV3 呼叫鏈路分析

> 分析日期：2026-05-18
> 涉及專案：Shopping → Cart → Webstore (mobilewebmall)

---

## 一、完整呼叫鏈路

```bash
前台使用者點「前往結帳」
        ↓
[Shopping] POST /api/checkout/Create
→ CheckoutController.Create()
→ ICheckOutService.CheckoutCreateAsync()
→ CheckoutCreateProcessorLayers:
    1. GetLastOrderInfoProcessor           // 取上一筆訂單資訊
    2. CheckoutCreateApiProcessor  ────────────────────────────┐
    3. AfterCheckoutCreateProcessor                            │ POST api/checkouts/create
    4. GetImmutableCheckoutDataProcessor                       │
    5. CachePartialCheckoutDataProcessor                       │
                                                               ↓
                                                   [Cart] POST api/checkouts/create
                                                   → CheckoutController.Create()
                                                   → ICheckOutService.Create()
                                                   → CheckoutCreateProcessorLayers:
                                                       1. GetCartAndCalculateProcessor
                                                       2. ThirdPartyValidateProcessor
                                                       3. CalculatePaymentCreditCheckProcessor
                                                       4. MergeRequestAndCartContextProcessor
                                                       5. CreateCheckoutGetGiftCardDisplayListProcessor
                                                       6. CreateCheckoutCalculateGiftCardSelectionProcessor
                                                       7. DoPayProcessFlowProcessor ─────────────────────┐
                                                                                                          │ POST
                                                                                                          ↓
                                                                                          [Webstore]
                                                                                  PayLite/RequestPayProcessUrlV3
                                                                                  ?v=0&shopId={shopId}&lang=zh-TW
```

**關鍵檔案位置：**
- Shopping Controller：`Nine1.Shopping.Web.Api/Controllers/CheckoutController.cs`
- Shopping Processor：`Nine1.Shopping.BL.Services/CheckoutCreateProcessor/CheckoutCreateApiProcessor.cs`
- Cart Controller：`Nine1.Cart.Web.Api/Controllers/CheckoutController.cs`
- Cart Processor：`Nine1.Cart.BL.Services/Processor/Checkout/Create/DoPayProcessFlowProcessor.cs`
- Webstore Controller：`WebStore/WebAPI/Controllers/PayLiteController.cs`

---

## 二、為什麼要在 CheckoutCreate 這步呼叫？

`RequestPayProcessUrlV3` **不只是吐一個 URL**，同時完成多件重要工作：

| 工作 | 說明 |
|------|------|
| ✅ 登入確認 | 若未登入直接拋錯 |
| ✅ 防重複攻擊 | `ExceededTimesLimiter` 檢查同一會員短時間重複打的次數 |
| ✅ 贈品庫存檢查 | 檢查「買就送」與「滿額贈」贈品是否售完 |
| ✅ 超重檢查 | 若因贈品導致超重，先移除滿額贈品 |
| ✅ 排隊系統 | 取得排隊票券（LineUp V1/V2），決定能否進入結帳 |
| ✅ 產生 UniqueKey | `Guid.NewGuid()` 產生本次結帳唯一識別鍵（**k 值**） |
| ✅ 儲存 PayProcessContext | 把整份購物車資料存入 Webstore Session/Cache（`_userDataHelper.Save`） |
| ✅ 決定付款 domain | 依 `UserClientTrack`（App版本/渠道）與 `ShopId` 決定目標 domain |

> **白話**：在這步建立「這次結帳的通行證（k 值）」，後面所有環節都靠它串起來。

---

## 三、回傳的 URL 都一樣嗎？

**每次都不同！** 路徑由**排隊狀態**決定，domain 也會因渠道/商店而異：

| 情境 | URL 格式 |
|------|---------|
| 正常可結帳 | `{domain}/V2/Pay?k={uniqueKey}&shopId={shopId}` |
| 排隊中（V1） | `{domain}/V2/Pay/Landing?k={uniqueKey}&shopId={shopId}` |
| 排隊中（V2） | `{domain}/V2/Pay/LandingV2?k={uniqueKey}&shopId={shopId}` |

**ReturnCode 說明：**

| ReturnCode | 意義 | Cart 的處理 |
|------------|------|-------------|
| `API0001` | 正常，繼續往下 | 寫入 Redis，回傳 URL + k |
| `API0006` | 贈品售完，但不影響運費，可繼續結帳 | 移除售完贈品後，寫入 Redis，回傳 URL + k |
| `API0007` | 贈品售完且影響運費，不可繼續 | 拋出 `SalePageGiftOutOfStock` Exception，導回購物車 P1 |

---

## 四、k 值（UniqueKey）的生命週期

這個 `k` 值是整個後續流程的核心鑰匙：

```bash
Webstore 產生 uniqueKey = Guid.NewGuid()
         │
         ├─→ 儲存 PayProcessContextEntity（含整份購物車資料）到 Webstore Session
         │
         └─→ 組成 URL：{domain}/V2/Pay?k={uniqueKey}&shopId=...
                  │
                  ↓（Cart 從 URL query string 取出 k）
         Cart 用 k 將 CheckoutEntity 寫入 Redis
                  │
         ┌────────┼───────────────────────────────────────────┐
         │        │                                           │
  前端拿 URL     Shopping AfterCheckoutCreateProcessor       後續 CheckoutComplete
  導頁進入        用 k 呼叫 api/checkouts/get                 帶入 CheckoutUniqueKey = k
  付款流程        取回完整 checkout 資料供前端顯示             → Cart 從 Redis 讀出 CheckoutEntity
  /V2/Pay?k=...                                              → 建立訂單
```

---

## 五、V2 vs V3 差異

`RequestPayProcessUrlV3` 與 `RequestPayProcessUrlV2` 幾乎相同，差異只在於 **Request 的讀取方式**：

| 版本 | Request 讀取方式 | 原因 |
|------|----------------|------|
| V2 | `[FromJson]` 參數綁定 | 舊方式 |
| V3 | `Request.ToTypedObject<...>()` 手動反序列化 | 解決 `[FromJson]` 遇到超長購物車資料時的 `maxJsonLength` 問題 |

兩者都呼叫同一個 `private RequestPayProcessUrl(...)` 方法，邏輯完全一致。

---

## 六、RetailStoreExpress 特殊路徑

Shopping 專案中另有 `RetailStoreExpressService` 也有 CheckoutCreate 的呼叫，為門市快速結帳（快速代客結帳）的特殊路徑，非一般購物車主流程。
