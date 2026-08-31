# CustomOfflinePayment 付款流程文件

> 來源：`nineyi.webstore.mobilewebmall` codebase 分析  
> 更新：2026-06-25

---

## CustomOffline 是 ATM 嗎？

**不是 ATM**，是兩個不同的 PayProfileType：

| PayProfileType | Enum 值 | 說明 | 虛擬帳號 | 管理方式 |
|---|---|---|---|---|
| `ATM` | `16` | 系統內建 ATM 付款 | ✅ 有 | 系統自動產生 |
| `CustomOfflinePayment` | `1048576` | **其他轉帳方式** | ❌ 無（欄位留空） | 商店後台自訂設定 |

> `CustomOfflinePayment` 是商家在後台填寫轉帳說明（銀行名稱、帳號、備注等），
> 訂單建立後直接顯示給消費者，**不需要任何金流 API 介接**。

---

## 呼叫入口 Call Chain

```
TradesOrderLiteController.CompleteForNewCartV2()
  └─ CreateTradesOrder(context)           // private ActionResult
       └─ PayProcessService.CreateTradesOrder(context)
            └─ ExecuteProcess(context, "CustomOfflinePaymentProcess")
                 └─ // 依序執行下列 26 個 Processor
```

`PayProcessFlowType` 在 `GetPayProcessDataProcessor` 中被設為  
`PayProcessFlowTypeEnum.CustomOfflinePaymentProcess`，  
驅動 Autofac Keyed 注入找到對應的 Processor 清單。

---

## Processor 執行序列（CustomOfflinePaymentProcess）

| # | Processor | 說明 |
|---|---|---|
| 1 | `XSSFilterProcessor` | 檢查 XSS 危險字元 |
| 2 | `ArrangeDataProcessor` | 資料整理 |
| 3 | `ValidateProcessDataProcessor` | 檢查付費流程資料合法性 |
| 4 | `ReserveFirstPurchaseProcessor` | 首購資格預扣 |
| 5 | `CheckValidTemperatureDeliveryTicketProcess` | 檢查溫層商品配送保留編號 |
| 6 | `CalculateShoppingCartProcessor` | 計算購物車金額 |
| 7 | `CloneSalePageGroupListProcessor` | 複製購物車原商品資料（snapshot）|
| 8 | `SplitOrderByQtyProcessor` | 商品拆單處理 |
| 9 | `SplitGiftECouponOrderByQtyProcessor` | 贈品券商品拆單處理 |
| 10 | `ArrangeActivityCodeProcessor` | 讀取外部代碼（Cookie / Redis）|
| 11 | `ArrangeFRCodeProcessor` | 讀取 FRCode（需在 10 之後，後蓋前）|
| 12 | `ValidatePromotionEngineSelectedGiftListProcessor` | 驗促購引擎贈品有效性 |
| 13 | `MapPromotionFreeGiftSalePageProcessor` | 滿額贈活動贈品 → 實際商品資料 |
| 14 | `MapSalePageGiftProcessor` | 買就送商品贈品 → 實際商品資料 |
| 15 | `MapToTradesOrderProcessor` | 購物車資料轉換為訂單資料結構 |
| 16 | `ValidateTradesOrderDataProcessor` | 檢查訂單資料 |
| 17 | `CheckPayProcessCodeProcessor` | 檢查付費流程 token 有效性 |
| 18 | `RedeemCouponProcessor` | 核銷優惠券 |
| **19** | ⭐ **`CreateTradesOrderProcessor`** | **新增訂單到 DB**（詳見下方說明）|
| 20 | `CreateTradesOrderEventProcessor` | 成立訂單後發出 Event（NMQ）|
| **21** | ⭐ **`ConfigureShopCustomOfflinePaymentInfoProcessor`** | **讀取商店自訂轉帳設定**（詳見下方說明）|
| 22 | `RedeemPromotionQuotaProcessor` | 處理活動名額 |
| 23 | `RedeemLoyaltyPointProcessor` | 串接積點系統扣點 |
| 24 | `StoreCreditProcessor` | 購物金付款 |
| **25** | ⭐ **`TransferOrderProcessor`** | **轉單到 ERP** |
| 26 | `AfterOrderProcessor` | 成立訂單後動作（Email、推播等）|

---

### ⭐ Processor 19：CreateTradesOrderProcessor — CustomOffline 專屬邏輯

```csharp
case PayProfileTypeDefEnum.CustomOfflinePayment:
    this.AddAtmAccountForCustomOfflinePayment(
        tradesOrderGroup,
        context.GetTotalPaymentWithoutMultiPayment()
    );
    break;
```

`AddAtmAccountForCustomOfflinePayment` 建立一筆 `AtmAccount` 記錄：

```csharp
atmAccount.AtmAccount_BankCode      = string.Empty;  // 留空
atmAccount.AtmAccount_BankName      = string.Empty;  // 留空
atmAccount.AtmAccount_VirtualAccount = string.Empty; // 留空（不產生虛擬帳號）
atmAccount.AtmAccount_ExpiredDateTime = DateTime.Today.AddDays(8); // N+7 天有效期
```

---

### ⭐ Processor 21：ConfigureShopCustomOfflinePaymentInfoProcessor

```csharp
ShopCustomOfflinePaymentSettingEntity setting =
    _shopPaymentSettingService.GetCustomOfflinePaymentSetting(context.ShoppingCartV2.ShopId);

var multilingualContentSetting =
    _shopPaymentSettingService.GetShopCustomOfflinePaymentSettingWithMultilingualContentCache(
        context.ShoppingCartV2.ShopId, setting, context.MultilingualContentSupport
    );

context.CustomOfflinePaymentInfo = new CustomOfflinePaymentInfoEntity { ... };
```

負責將商店後台設定的轉帳說明（含多語系）寫入 `context.CustomOfflinePaymentInfo`，  
最終回傳給前端顯示。

---

## OrderSlaveFlow 初始狀態

在 `CreateTradesOrderProcessor` 的 switch 中，`CustomOfflinePayment` 只設定了：

```csharp
case PayProfileTypeDefEnum.CustomOfflinePayment:
    orderSlaveFlow.OrderSlaveFlow_StatusForUserDef = nameof(OrderSlaveFlowStatusForUserEnum.WaitingToPay);
    orderSlaveFlow.OrderSlaveFlow_StatusForSCMDef  = nameof(OrderSlaveFlowStatusForSCMEnum.WaitingToPay);
    break;
```

`StatusDef` 未被覆蓋，沿用程式碼第 545 行的預設值：

| 欄位 | 值 | 說明 |
|---|---|---|
| `StatusDef`（系統主狀態）| `WaitingToTrans` | 預設值，未被 switch 覆蓋 |
| `StatusForUserDef`（前台顯示）| `WaitingToPay` | 待付款 |
| `StatusForSCMDef`（後台顯示）| `WaitingToPay` | 待付款 |

---

## 與其他付款方式的流程差異

| 付款方式 | ThirdPartyPayApiProcessor | AuthProcessor | 建單即轉單 | StatusDef 初始 |
|---|---|---|---|---|
| `CustomOfflinePayment` | ❌ | ❌ | ✅ | `WaitingToTrans` |
| `ATM` | ❌ | ❌ | ✅ | `WaitingToTrans` |
| `LinePay / PoyaPay` | ✅ | ❌ | ❌（回調後轉）| `WaitingToPay` |
| `JKOPay` | ✅ | ❌ | ❌（ThirdPartyTrans）| `WaitingToThirdPartyTrans` |
| `CreditCardOnce_Stripe` | ✅ | ✅ | ❌（授權後轉）| `WaitingToAuth` |

---

## Controller 最終回應

CustomOfflinePayment 不符合任何特殊分支（LinePay、Stripe、GlobalPay 等）的條件，
因此走到預設回應：

```csharp
return Json(new ApiResultEntity<object> {
    ReturnCode = "Success",
    Data = context,    // 含 CustomOfflinePaymentInfo（銀行名稱、帳號說明等）
    Message = "SendTradesOrderSuccess"
});
```

---

## 關鍵特點小結

1. **無金流 API 介接**：整個流程不呼叫任何外部付款 API
2. **建單即轉單**：`TransferOrderProcessor` 在同一個 Processor 鏈中執行，不需等待付款回調
3. **AtmAccount 留空**：產生 `AtmAccount` 記錄但所有金流欄位為空，有效期 N+7 天
4. **商店自訂說明**：由 `ConfigureShopCustomOfflinePaymentInfoProcessor` 讀取後台設定並塞入 context
5. **狀態初始為 WaitingToTrans**：與 ATM 相同，後續由人工確認匯款後更新
