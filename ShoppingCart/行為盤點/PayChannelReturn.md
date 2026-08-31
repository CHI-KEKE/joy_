# PayChannelReturn 鏈路分析（mweb 前台）

> 分析日期：2026-05-18
> 涉及位置：MobileWebMallV2 前台 + BLV2 Business Logic

---

## 一、PayChannelReturn 在整個結帳流程中的位置

```bash
用戶結帳
    ↓
[CartCreate] → RequestPayProcessUrlV3
    ↓（取得付款流程 URL，存入 PayProcessContextEntity / Session）
[CompleteForNewCartV2] → 成立訂單
    ↓（需跳轉第三方的付款方式，回傳 ReturnCode: "PayUrlIsNotNull"，Data: 第三方付款URL）
前端導至第三方付款頁
    ↓（e.g. LinePay / CathayPay / Razer / KPay / Stripe 3DS）
用戶在第三方完成付款
    ↓（第三方 redirect 回前台）
[PayChannelReturn] ← 本文件的分析起點
    ↓
呼叫 PaymentMiddleware QueryPayment
    ↓（依結果）
成功 → 轉單、觸發後續 Processor → 導至付款完成頁
失敗 → 取消訂單 → 導回購物車
逾時 → 退款/取消 → 導至訂單列表
```

---

## 二、進入 PayChannelReturn 的 URL 格式

`PayChannelReturnUrl` 由 `PayChannelHelper.GetPayChannelReturnUrl()` 統一產生。

### 一般格式（多數付款方式）

```bash
/V2/PayChannel/{payMethod}/{payChannel}/{tgCode}
?shopId={shopId}&k={uniqueKey}&lang={locale}
```

### SwiftPass（BoCPay_SwiftPass，非 APP）特殊格式

```bash
/V2/PayChannel/{payMethod}/{payChannel}/{tgCode}/{shopId}/{uniqueKey}/{locale}
```

### payMethod / payChannel 的來源


由 `PayProfileType` 以 `_` 切分：

| PayProfileType | payMethod | payChannel |
|----------------|-----------|------------|
| `LinePay` | Default | LinePay |
| `CreditCardOnce_Razer` | CreditCardOnce | Razer |
| `BoCPay_SwiftPass` | BoCPay | SwiftPass |
| `EWallet_PayMe` | EWallet | PayMe |

---

## 三、Route 設定（RouteConfig.cs）

```csharp
// 標準格式
"PayChannel/{payMethod}/{payChannel}/{tgCode}"
→ controller: PayChannel, action: PayChannelReturn

// SwiftPass 特殊格式
"PayChannel/{payMethod}/{payChannel}/{tgCode}/{shopId}/{k}/{lang}"
→ controller: PayChannel, action: PayChannelReturn

// POST 轉導（部分付款方式用 POST 回來）
"PayChannel/ReturnPost/{payMethod}/{payChannel}"
→ controller: PayChannel, action: PayChannelReturnPost
```

> `PayChannelReturnPost` 會先將 Response Form 轉換，再自動 submit 到 `PayChannelReturn`。

---

## 四、PayChannelController.PayChannelReturn 流程

```csharp
[RequireLoginGoLoginPage]          // 必須登入
[RequireLoginGoLoginPageV2(Order=2)]
[RequireReactThemeAttribute(Order=2)]
[RequireOfficialTheme(Order=3)]
public ActionResult PayChannelReturn(string payMethod, string payChannel,
                                     string tgCode, string k,
                                     IDictionary<string, string> extendData = null)
```

1. 取得 `shopId`（ViewBag）、`memberIdentity`（登入驗證）
2. 呼叫 `_tradesOrderPaymentService.FinishPayment(...)` → 取得 `paymentResult`
3. 依 ReturnCode 決定導頁：

| ReturnCode | 結果 |
|------------|------|
| `Success` | Redirect `/V2/Pay/Finish/?k={k}&shopId={shopId}` |
| `Failed` / `Expired` | View `PayChannelCallback`，ErrorRedirectUrl → 購物車 |
| `WaitingToPay`（其他） | View `PayChannelCallback`，ErrorRedirectUrl → 訂單列表 |

**Exception 處理：**

| Exception | 導頁 `err` 參數 |
|-----------|----------------|
| `TradesOrderThirdPartyPaymentException` | `/V2/TradesOrder/TradesOrderList?err=OrderProcessing` |
| `DuplicateRequestException` | `/V2/TradesOrder/TradesOrderList?err=DuplicateRequest` |
| 其他 `Exception` | `/V2/TradesOrder/TradesOrderList?err=PaymentException` |

---

## 五、FinishPayment 核心流程詳解

```bash
FinishPayment(shopId, memberId, payChannel, payMethod, tgCode, k, queryString, isFromWorker)
    │
    ├─ 1. GetPayProcessContext(shopId, memberId, tgCode, k)
    │         └─ 從 UserDataHelper (Session/Cache) 載入 PayProcessContextEntity
    │            （k 為 null → 用 tgCode 查 DB 取 UniqueKey 再載入）
    │            （載入失敗 → 拋 GetPayProcessDataProcessorException）
    │
    ├─ 2. 查詢 TradesOrderThirdPartyPayment（DB）
    │         ├─ 已是 Success → 直接回傳 Success（防止跨 APP 重複導頁）
    │         └─ 非 WaitingToPay → 拋 TradesOrderThirdPartyPaymentException
    │
    ├─ 3. _paymentMiddlewareService.QueryPayment()
    │         └─ 呼叫 PaymentMiddleware 查詢第三方付款結果
    │            回傳 ReturnCode：Success / Failed / Expired / WaitingToPay / PayChannelError / Exception
    │
    ├─ 4. IPayChannelService = _payChannelServiceResolver.Resolve(payProfileType)
    │         └─ 依付款類型選擇對應的 PayChannelService（KPay / Razer / EftPay / Atome...）
    │
    └─ 5. 依 ReturnCode 分支處理（見下表）
```

---

## 六、ReturnCode 分支處理明細

### ✅ Success（付款成功）

```Bash
EnsureRequestNotDuplicated(memberId, k)        // 防重複（Redis TTL 保護）
    ↓
payChannelService.GetTradesOrderThirdPartyPaymentUpdateEntityOnReturnSuccess()
    ↓
UpdateThirdPartyPaymentWithRetry()             // 更新 DB 第三方支付記錄（Retry 機制）
    ↓
UpdateOrderSlaveFlowWithRetry(
    status: WaitingToTrans,                    // 後台狀態
    statusForScm: Hide,                        // SCM 隱藏
    statusForUser: OrderProcessing             // 前台顯示「訂單處理中」
)
    ↓
ThirdPartyFinishProcess Processors（依序執行）：
    1. CreateGlobalInvoiceProcessor    → 建立國際版發票
    2. TransferOrderProcessor          → 轉單到 ERP
    3. CreateRegularOrderProcessor     → 定期購設定
    4. AfterOrderProcessor             → 成立訂單後動作（通知、NMQ等）
    ↓
payChannelService.DoActionOnReturnSuccess()    // 各 PayChannel 自訂收尾動作
```

### ⏰ Expired（付款逾時）

```bash
EnsureRequestNotDuplicated(memberId, k)
    ↓
DoProcessOnReturnForTimeoutTradesOrder():
    ├─ 更新 ThirdPartyPayment → Timeout
    └─ CancelTradeOrder()：取消訂單、回收 PromoCode 名額、退點數...
```

### ❌ Failed（付款失敗）

```bash
EnsureRequestNotDuplicated(memberId, k)
    ↓
若 payChannelService.IsRequiredCancelPaymentRequestOnReturnFailed() == true
    → _paymentMiddlewareService.CancelPayment()  // 通知 PaymentMiddleware 取消
    ↓
CancelTradeOrder(status: Fail)：取消訂單
```

### ⏳ WaitingToPay（僅限 Worker 呼叫）

```bash
計算是否已超時（依 PayChannel 設定的 TimeoutTimeInSeconds）
    ├─ 已超時 → 走逾時流程（同 Expired）
    └─ 未超時 → 略過，由 Console/Worker 後續處理
```

### 其他（WaitingToPay 非 Worker / PayChannelError / Exception）

```bash
記 Log，回傳 WaitingToPay ReturnCode
→ Controller 導至訂單列表（OrderProcessing）
→ 後台 Worker 或 Console 輪詢處理
```

---

## 七、防重複機制（EnsureRequestNotDuplicated）

```csharp
int requestInterval = AppSetting["PayChannel.FinishPayment.RequestInterval"];
bool isDuplicate = _duplicateRequestService.IsDuplicateRequest(
    "PayChannelFinishPayment",
    TimeSpan.FromSeconds(requestInterval),
    $"{memberId}",
    k
);
// isDuplicate == true → 拋 DuplicateRequestException
```

> Controller 捕捉到 `DuplicateRequestException` 後，導至訂單列表顯示「處理中」訊息，不重複處理訂單。

---

## 八、PayChannelReturnPost（特殊 POST 回調）

部分付款方式（如 Atome、QFPay）會以 HTTP POST 方式回調，走不同路由：

```bash
POST PayChannel/ReturnPost/{payMethod}/{payChannel}
    ↓
PayChannelReturnPost()
    ├─ _payChannelServiceResolver.Resolve(payChannel)
    ├─ payChannelService.ProcessResponseFormAfterPayment()
    │     → 將 POST Form 轉換成 QueryString ExtendData
    └─ 產生 auto-submit HTML Form
         → 自動 POST 至 PayChannelReturn 的 RedirectUrl（含 extendData）
```

---

## 九、整體狀態機（ThirdPartyPayment 狀態流轉）

```bash
[WaitingToPay]
    │
    ├─ QueryPayment = Success  → [Success]  → 轉單 ERP → 前台顯示訂單處理中
    ├─ QueryPayment = Failed   → [Fail]     → 取消訂單 → 退款/退點
    ├─ QueryPayment = Expired  → [Timeout]  → 取消訂單 → 退款/退點
    └─ QueryPayment = Waiting  → [維持 WaitingToPay] → Worker 定時輪詢
```

---

## 十、關鍵檔案位置

| 類型 | 路徑 |
|------|------|
| Controller | `MobileWebMallV2/Controllers/PayChannelController.cs` |
| Route 設定 | `MobileWebMallV2/App_Start/RouteConfig.cs` → `MapPayChannelReturn()` |
| 核心 Service | `BLV2/ThirdPartyPay/TradesOrderPaymentService.cs` |
| URL 產生 Helper | `BLV2/PayChannel/PayChannelHelper.cs` |
| Processor 定義 | `BLV2/Modules/PayProcessProcessorModule.cs` → `RegisterThirdPartyFinishProcess()` |
| 抽象 PayChannel | `BLV2/PayChannel/AbstractPayChannelService.cs` |
