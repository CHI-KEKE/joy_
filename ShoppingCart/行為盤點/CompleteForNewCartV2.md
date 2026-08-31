# CompleteForNewCartV2 呼叫鏈路分析

> 分析日期：2026-05-18
> 涉及專案：Shopping → Cart → Webstore (mobilewebmall)

---

## 一、完整呼叫鏈路

```
使用者在結帳頁填完資料，按下「確認訂購」
        ↓
[Shopping] POST /api/checkout/Complete
→ CheckoutController.Complete()
    ├─ AntiForgery Token 驗證
    ├─ _checkOutService.SendCompleteRequestAsync(request)
    │       ├─ ExceededTimesLimiter 防重複打
    │       ├─ 還原個資遮罩（UnmaskOrderCompleteRequest）
    │       ├─ Email 格式驗證
    │       └─ POST api/checkouts/complete ──────────────────────┐
    └─ 移除 AntiForgery Token（成功後）                          │
                                                                  ↓
                                              [Cart] POST api/checkouts/complete
                                              → CheckoutController.Complete()
                                              → ICheckOutService.SendCompleteRequest()
                                                  ├─ 檢查 CheckoutEntity 是否過期（Redis）
                                                  ├─ CheckoutOrderComplete Processor Pipeline:
                                                  │   1. MergeRequestAndOldPayProcessContextProcessor
                                                  │   2. CalculateShoppingCartPayPriceLimitProcessor
                                                  │   3. GetPayDataProcessor
                                                  │   4. UpdateOrClearCacheBeforeComplete
                                                  ├─ CheckStoreIdValid（驗證門市合法性）
                                                  └─ POST tradesOrderLite/CompleteForNewCartV2 ──────┐
                                                     ?v=0&shopId={shopId}&lang={lang}               │
                                                     &uniquekey={uniqueKey}                         │
                                                                                                     ↓
                                                                                  [Webstore]
                                                                        TradesOrderLiteController
                                                                              .CompleteForNewCartV2()
                                                                        → CreateTradesOrder(context)
                                                                            ├─ 內部 IP 驗證
                                                                            ├─ 會員登入驗證
                                                                            ├─ 防重複申請（頻率限制）
                                                                            ├─ UniqueKey 狀態驗證
                                                                            ├─ _payProcessService.CreateTradesOrder()
                                                                            └─ 依付款類型決定回傳格式
```

**關鍵檔案位置：**
- Shopping Controller：`Nine1.Shopping.Web.Api/Controllers/CheckoutController.cs`
- Shopping Service：`Nine1.Shopping.BL.Services/Checkout/CheckoutService.cs`
- Cart Service：`Nine1.Cart.BL.Services/CheckOut/CheckOutService.cs`
- Cart Processors 目錄：`Nine1.Cart.BL.Services/Processor/Checkout/Complete/`
- Webstore Controller：`WebStore/WebAPI/Controllers/TradesOrderLiteController.cs`

---

## 二、Cart 的 CheckoutOrderComplete Processor Pipeline 詳解

| 順序 | Processor | 說明 |
|------|-----------|------|
| 1 | `MergeRequestAndOldPayProcessContextProcessor` | 從 Redis 取出 CheckoutEntity → deep copy → 重新計算購物車 → 驗證訂單資料 → 把新 cart entity 轉換成舊 `OldPayProcessContextEntity` |
| 2 | `CalculateShoppingCartPayPriceLimitProcessor` | 依付款方式檢查金額上限/下限 |
| 3 | `GetPayDataProcessor` | Assign 配送方式、付款方式、付款 Flow 類型、取得商店 Gateway |
| 4 | `UpdateOrClearCacheBeforeComplete` | 清除會員首購資格快取（避免短時間重進車拿到快取的首購資格） |

---

## 三、CalculateShoppingCartPayPriceLimitProcessor 金額驗證對照表

| 付款方式 | 驗證規則 |
|----------|---------|
| ATM | 總金額不得超過 `AtmLimitPrice` 上限設定值 |
| Stripe（信用卡/ApplePay/GooglePay） | 總金額不得低於 HK$4 / US$0.55（依 SalesMarket） |
| KPay | 總金額不得低於 HK$2 |
| Razer / AsiaPay 系列（MY） | 總金額不得低於 RM 1 |

> `OtherMultiPaymentTotalAmount`（多元支付其他金額）會從總金額中扣除後再比較。

---

## 四、Webstore `CreateTradesOrder` 內部驗證流程

```
CreateTradesOrder(context)
    │
    ├─ 1. IsFromCompany() 內部 IP 驗證
    │     └─ 非公司IP → 直接回傳 IpNotAllowed，不執行成單
    │
    ├─ 2. MemberManager.GetMemberIdentity() 會員登入確認
    │     └─ 未登入 → 更新排隊狀態為 Error，回傳 UnknownError
    │
    ├─ 3. CreateTradesOrderLimitValid() 防重複申請（頻率限制）
    │
    ├─ 4. UniqueKey 狀態驗證（依商店開關決定是否啟用）
    │     └─ 票券不存在或狀態不是 CheckOut → 拋出 PayProcessUniqueKeyException
    │
    ├─ 5. _payProcessService.CreateTradesOrder(context) 實際成立訂單
    │
    └─ 6. 依 PayProfileType 決定回傳格式：
          ├─ Is3DSecure（Stripe/CheckoutDotCom/KPay）→ ReturnCode: "WaitingFor3d"（含 ThirdPartyPaymentInfo）
          ├─ Is3DSecure（其他）→ ReturnCode: "WaitingFor3d"
          ├─ 需跳轉第三方（LinePay/CathayPay/JKOPay/icashPay/EasyWallet/Razer 系列等）
          │     → ReturnCode: "PayUrlIsNotNull"，Data: 第三方付款 URL
          └─ 一般付款（信用卡/ATM/CVS/貨到付款等）→ ReturnCode: "API0001"
```

---

## 五、Cart 呼叫 Webstore 的 Request Body

Cart 將 `OldPayProcessContextEntity` 整包 POST 給 Webstore。

此物件是由新版 `CheckoutEntity`（從 Redis 取出）轉換而來，包含：

| 欄位 | 說明 |
|------|------|
| `MemberId` | 會員 ID |
| `UniqueKey` | k 值（即 CheckoutUniqueKey） |
| `ShoppingCartV2` | 完整購物車資料（商品、金流、物流、折扣等） |
| `PayProfileType` | 付款方式（信用卡/ATM/CVS…） |
| `ShippingProfileType` | 配送方式 |
| `PayProcessFlowType` | 付款流程類型（決定 Webstore 走哪條成單路徑） |
| `RememberCreditCardNo` | 是否記住信用卡 |
| `Email` / `UserMemo` | 訂購人資訊 |
| `UnloginId` | 非登入 ID（for 追蹤） |
| `IsSkipReplaceMaskPersonalData` | 跳過遮罩還原（內部服務成單，資料已是原始值） |

---

## 六、V1 vs V2 差異

| 版本 | Action | Request 讀取方式 | 差異 |
|------|--------|----------------|------|
| V1 | `CompleteForNewCart` | `[FromJson]` 參數綁定 | 舊方式，長 Request 會遇到 maxJsonLength 問題 |
| V2 | `CompleteForNewCartV2` | `Request.ToTypedObject<>()` 手動反序列化 | 解決 maxJsonLength 問題 |

兩者都呼叫同一個 `private CreateTradesOrder(...)` 方法，邏輯完全一致。

---

## 七、Cart 端後處理（成單後）

```
_webStoreWebApiClient.PostAsJsonAsync(CompleteForNewCartV2) 回傳
    │
    ├─ HTTP 狀態碼 ≠ 200 → 拋出 CheckoutCompleteException (OrderCompleteHttpStatusNotOk)
    │
    ├─ UpdateCheckoutUniqueKeyAsync() 
    │     → 處理 3DS 或重新結帳時 k 值需更換的情境
    │
    └─ HandleReturnCode(apiResponseEntity, checkoutPayType)
          → 依付款方式對 ReturnCode 做後處理後，回傳給 Shopping
```

---

## 八、與 RequestPayProcessUrlV3 的關係

| 時間點 | API | 目的 |
|--------|-----|------|
| 進入結帳頁前 | `RequestPayProcessUrlV3` | 建立 k 值、存 PayProcessContext、取得付款流程 URL |
| 使用者填完資料按確認 | `CompleteForNewCartV2` | 帶著 k 值組裝的 OldPayProcessContext 實際成立訂單 |

> `k` 值（UniqueKey）是兩次呼叫的共同識別鍵，貫穿整個結帳流程。
