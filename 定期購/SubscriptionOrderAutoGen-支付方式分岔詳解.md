# SubscriptionOrderAutoGen API —— 支付方式分岔詳解

> 本文釐清 mweb `SubscriptionOrderAutoGen` API 如何根據付款方式區分信用卡扣款與貨到付款

**關鍵發現：** 分岔發生在 **PayProfileType 層級**，貨到付款根本不會進入 Razer 支付流程

---

## 一、API 完整調用流程圖

```
┌─────────────────────────────────────────────────────────────────┐
│  PHP 定期購排程 (regular_create_housekeeper)                      │
│  呼叫 mweb TradesOrderV2/SubscriptionOrderAutoGen API            │
└────────────────────────────┬────────────────────────────────────┘
                             │
                             ▼
         ┌───────────────────────────────────────┐
         │ TradesOrderV2Controller               │
         │ .SubscriptionOrderAutoGen()           │
         │ 📍 mweb 前台 API 入口                │
         └───────────────────┬───────────────────┘
                             │
                             ▼
    ┌────────────────────────────────────────────┐
    │ TradesOrderService                         │
    │ .GetPayProcessContextForRegularOrder()     │
    │ 📍 讀取首單資訊與付款 token                │
    │    (RegularOrderCheckoutInfo)              │
    └────────────────┬─────────────────────────┘
                     │
                     ▼
  ┌──────────────────────────────────────────────────┐
  │ PayProcessService                                │
  │ .GetProcessContextForRegularOrderPay()           │
  │ 📍 設定 IsRegularOrderAutoGen = true             │
  │    設定 RegularOrderCheckoutInfo = token/null    │
  │    執行 "GetPayProcess" 流程                     │
  └──────────┬───────────────────────────────────────┘
             │
             ▼
  ╔══════════════════════════════════════════════════════╗
  ║  🔄 【分岔點】GetPayProcessDataProcessor.Process()   ║
  ║  📍 根據 PayProfileType 決定支付流程類型            ║
  ║     (CreditCardProcess vs CashOnDeliveryProcess)    ║
  ╚══════════════════════════════════════════════════════╝
             │
        ┌────┴──────┐
        │            │
        ▼            ▼
    ┌─────────┐  ┌─────────────────┐
    │信用卡   │  │貨到付款         │
    │Payment  │  │CashOnDelivery   │
    │Process  │  │Process          │
    └────┬────┘  └────┬────────────┘
         │             │
         ▼             ▼
 ┌──────────────┐  ┌────────────────┐
 │ RazerPay     │  │ 跳過支付流程    │
 │ ChannelSvc   │  │ 直接建訂單      │
 │ (扣款)       │  │ (Pending)      │
 └──────┬───────┘  └────┬───────────┘
        │                │
        ▼                ▼
   ✅ Success/    ⏳ WaitingToPay
   ⏳ Pending     (消費者線下付款)
```

---

## 二、支付方式分岔邏輯 (核心代碼)

### 📍 **分岔點位置**

| 層級 | 檔案 | 行號 | 功能 |
|------|------|------|------|
| **入口** | `TradesOrderV2Controller.cs` | - | 接收 API 請求 |
| **準備上下文** | `PayProcessService.cs` | L412-446 | 設定 IsRegularOrderAutoGen / RegularOrderCheckoutInfo |
| **✨ 決定支付流程類型** | **`GetPayProcessDataProcessor.cs`** | **L232-257** | ← 🔑 **主要分岔點** |
| **執行支付** | `PayProcessService.CreateTradesOrder()` | - | 根據流程類型執行相應邏輯 |

---

### 🔍 **分岔點代碼詳解**

#### **第 1 步：讀取付款方式**

**檔案：** `GetPayProcessDataProcessor.cs` L162

```csharp
public void Process(PayProcessContextEntity context)
{
    var shoppingCartV2 = context.ShoppingCartV2;
    
    // ← 在這裡從購物車讀取首單時選擇的付款方式
    string selectedPayType = shoppingCartV2.SelectedCheckoutPayTypeGroup.StatisticsTypeDef;
    // e.g., "CreditCardOnce_Razer" 或 "CashOnDelivery"
    
    // 解析為 PayProfileTypeDefEnum
    context.PayProfileType = (PayProfileTypeDefEnum)Enum.Parse(...);
```

---

#### **第 2 步：🌳 根據 PayProfileType 分岔**

**檔案：** `GetPayProcessDataProcessor.cs` L232-257

```csharp
// 🔑 【核心分岔點】根據支付方式決定流程
switch (context.PayProfileType)
{
    // ✅ 【信用卡路徑】
    case PayProfileTypeDefEnum.CreditCardOnce:
    case PayProfileTypeDefEnum.CreditCardInstallment:
        context.PayProcessFlowType = PayProcessFlowTypeEnum.CreditCardProcess;
        // → 後續會呼叫 RazerPayChannelService，進行 server-to-server 扣款
        var creditCardGateway = GetCreditCardGateway(context.ShoppingCartV2.ShopId);
        context.CreditCardGatewayType = ...;
        break;

    // ❌ 【貨到付款路徑】
    case PayProfileTypeDefEnum.CashOnDelivery:
        context.PayProcessFlowType = PayProcessFlowTypeEnum.CashOnDeliveryProcess;
        // → 後續 **不會** 呼叫 RazerPayChannelService，直接建訂單 Pending
        break;

    // 其他支付方式 (ATM、超取、LinePay 等)
    case PayProfileTypeDefEnum.ATM:
        context.PayProcessFlowType = PayProcessFlowTypeEnum.AtmProcess;
        break;
    
    case PayProfileTypeDefEnum.Family:  // 7-11
    case PayProfileTypeDefEnum.SevenEleven:
    case PayProfileTypeDefEnum.HiLife:
    case PayProfileTypeDefEnum.OKmart:
        context.PayProcessFlowType = PayProcessFlowTypeEnum.ShopProcess;
        break;
    
    // ... 更多支付方式
}
```

---

## 三、信用卡 vs 貨到付款 完整流程對比

### 📊 **完整對比表**

| 環節 | 信用卡（CreditCardOnce_Razer） | 貨到付款（CashOnDelivery） |
|------|-------------------------------|------------------------|
| **API 入口** | `SubscriptionOrderAutoGen()` | `SubscriptionOrderAutoGen()` |
| **PayProfileType** | `CreditCardOnce_Razer` | `CashOnDelivery` |
| **PayProcessFlowType** | `CreditCardProcess` | `CashOnDeliveryProcess` |
| **RegularOrderCheckoutInfo** | ✅ 包含 `CreditCardToken`（首單儲存） | ❌ 為 `null`（首單不需要） |
| **支付通道** | ✅ 執行 `RazerPayChannelService` | ❌ 跳過支付通道 |
| **扣款方式** | Server-to-Server（用 token） | 消費者線下付款（不扣款） |
| **API 回應** | `WaitingToPay`（Razer 確認中）或 `Success` | `WaitingToPay`（等消費者付款） |
| **後續處理** | `RegularCheckPendingPaymentJob` 查詢付款狀態 | 消費者收貨時線下付款 |

---

## 四、信用卡流程詳解

```
CreditCardProcess Flow
    │
    ├─ IsRegularOrderAutoGen = true
    ├─ PayProfileType = CreditCardOnce_Razer
    └─ RegularOrderCheckoutInfo.CreditCardToken = "Razer_Token_XXXX"
                        │
                        ▼
         ┌──────────────────────────────────┐
         │ RazerPayChannelService           │
         │ .GetPayExtendInfo()              │
         │ 📍 代碼位置: L160-175            │
         └──────────────────────────────────┘
                        │
                        ▼
    ┌─────────────────────────────────────────────────────┐
    │ if (IsRegularOrderAutoGen && RegularOrderCheckoutInfo != null)
    │ {
    │     payInfo["card_token"] = token;  ← 帶首單儲存的 token
    │     payInfo["card_user_email"] = ...;
    │     payInfo["card_user_fullname"] = ...;
    │     // ← 不帶卡號！（只在首單時帶卡號）
    │ }
    └─────────────────┬───────────────────────────────────┘
                      │
                      ▼
        ┌───────────────────────────────────┐
        │ PaymentMiddleware                 │
        │ → Razer Server-to-Server API      │
        │   (token charge)                  │
        └───────────────┬───────────────────┘
                        │
                   ┌────┴─────────┐
                   │              │
                   ▼              ▼
            ┌─────────────┐  ┌─────────────────┐
            │ Success     │  │ Pending         │
            │ (立即確認)  │  │ (非同步確認中)  │
            │ 訂單成功    │  │ 訂單建立但待確認│
            └─────────────┘  └─────────────────┘
```

---

## 五、貨到付款流程詳解

```
CashOnDeliveryProcess Flow
    │
    ├─ IsRegularOrderAutoGen = true
    ├─ PayProfileType = CashOnDelivery
    └─ RegularOrderCheckoutInfo = null  ← 無 token
                        │
                        ▼
         ┌──────────────────────────────────┐
         │ 【跳過 RazerPayChannelService】   │
         │ GetPayExtendInfo() 只寫入：      │
         │   payInfo["customer_name"] = ...│
         │   (無任何支付信息)              │
         └──────────────────────────────────┘
                        │
                        ▼
        ┌───────────────────────────────────┐
        │ PaymentMiddleware                 │
        │ → 不呼叫 Razer                    │
        │   直接進入後續訂單建立流程        │
        └───────────────┬───────────────────┘
                        │
                        ▼
            ┌─────────────────────────────────┐
            │ OrderSlaveFlowStatus =          │
            │    WaitingToPay                 │
            │ (不是支付確認中，是等消費者    │
            │  線下付款時提示 Pending)       │
            └─────────────────────────────────┘
                        │
                        ▼
        ┌───────────────────────────────────┐
        │ 訂單已建立 (rod_status = 3)       │
        │ 消費者收貨時線下付款              │
        └───────────────────────────────────┘
```

---

## 六、代碼堆棧（調用順序）

### **信用卡調用堆棧**

```
[mweb] TradesOrderV2Controller.SubscriptionOrderAutoGen()
  └─> TradesOrderService.GetPayProcessContextForRegularOrder()
        └─> ShoppingCartV2Service.GetShoppingCartContextForRegularOrder()
        └─> PayProcessService.GetProcessContextForRegularOrderPay()
              └─> PayProcessService.ExecuteProcess("GetPayProcess")
                    └─> GetPayProcessDataProcessor.Process()
                          └─> 【分岔】設定 PayProcessFlowType = CreditCardProcess
                    └─> 【支付流程】ThirdPartyPayApiProcessor
                          └─> RazerPayChannelService.GetPayExtendInfo()
                                └─> payInfo["card_token"] = token ✅
                          └─> PaymentMiddleware → Razer
  └─> PayProcessService.CreateTradesOrder(payProcessContext)
        └─> TransferOrderProcessor (轉單)
```

### **貨到付款調用堆棧**

```
[mweb] TradesOrderV2Controller.SubscriptionOrderAutoGen()
  └─> TradesOrderService.GetPayProcessContextForRegularOrder()
        └─> ShoppingCartV2Service.GetShoppingCartContextForRegularOrder()
        └─> PayProcessService.GetProcessContextForRegularOrderPay()
              └─> PayProcessService.ExecuteProcess("GetPayProcess")
                    └─> GetPayProcessDataProcessor.Process()
                          └─> 【分岔】設定 PayProcessFlowType = CashOnDeliveryProcess
                    └─> 【支付流程】RazerPayChannelService.GetPayExtendInfo()
                          └─> payInfo 只有 customer_name（無 token）❌
  └─> PayProcessService.CreateTradesOrder(payProcessContext)
        └─> TransferOrderProcessor (轉單，等消費者線下付款)
```

---

## 七、為什麼兩個通知 Job 必須分開？

### 📋 **定期購 D-3 通知時機**

**都是在距配送日 3 天前觸發**，但業務含義完全不同：

```
D-3 時間點
    │
    ├─ 【信用卡】CreditCardOrderNotify
    │  └─ 業務含義：「親愛的客戶，3 天後我們將從您的信用卡扣款」
    │  └─ 觸發條件：rgo_pay_type 包含 'CreditCardOnce'
    │  └─ 建單情況：step④ 已打 API，訂單已建立（可能 Pending 或 Success）
    │
    └─ 【貨到付款】CashOnDeliveryOrderNotify
       └─ 業務含義：「親愛的客戶，3 天後商品將到達，請準備現金」
       └─ 觸發條件：rgo_pay_type 不包含 'CreditCardOnce'
       └─ 建單情況：step④ 已打 API，訂單建立但 WaitingToPay（等線下付款）
```

### ✅ **分開 Job 的優勢**

| 優勢 | 說明 |
|------|------|
| **查詢效率** | 各自獨立查詢不同 pay_type 的訂單，避免無謂的篩選邏輯 |
| **業務清晰** | Job 名稱直接反映業務含義（扣款 vs 到貨） |
| **獨立調整** | 未來若需改變時機（如信用卡改 D-2），只需調整一個 Job |
| **錯誤隔離** | 一個失敗不影響另一個通知發送 |

---

## 八、總結

### 🎯 **核心結論**

1. **分岔發生在 GetPayProcessDataProcessor.Process() 方法**
   - 根據 `PayProfileType` 設定 `PayProcessFlowType`
   - 信用卡 → `CreditCardProcess`
   - 貨到付款 → `CashOnDeliveryProcess`

2. **信用卡和貨到付款進入不同的支付流程**
   - 信用卡：執行 `RazerPayChannelService.GetPayExtendInfo()` → 帶 token → Razer 扣款
   - 貨到付款：跳過支付通道 → 只寫基本收件人資訊 → 直接建訂單 Pending

3. **API 回應都是 WaitingToPay，但含義不同**
   - 信用卡：代表「Razer 確認中」（支付非同步處理）
   - 貨到付款：代表「等消費者線下付款」（無支付流程）

4. **兩個通知 Job 必須分開**
   - `RegularOrderCreditCardNotifyJob`：D-3 扣款提醒
   - `RegularOrderCashOnDeliveryNotifyJob`：D-3 到貨提醒

---

## 九、相關文件參考

- **mweb 代碼**：`c:\91APP\nineyi.webstore.mobilewebmall`
  - 控制器：`WebStore\WebAPI\Controllers\TradesOrderV2Controller.cs`
  - 服務層：`WebStore\Frontend\BLV2\PayProcesses\PayProcessService.cs`
  - 分岔點：`WebStore\Frontend\BLV2\PayProcesses\Processors\GetPayProcessDataProcessor.cs` L232-257
  - 支付通道：`WebStore\Frontend\BLV2\PayChannel\RazerPayChannelService.cs` L160-175

- **PHP 舊架構參考**：`C:\Users\Allen Lin\Desktop\joy_2\joy_\定期購`
  - 建單流程：`scheduled-jobs\regular-create-housekeeper.md`
  - 通知流程：`scheduled-jobs\create-notify-housekeeper.md`
