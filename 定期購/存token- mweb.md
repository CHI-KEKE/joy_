# Razer Card Token 儲存機制（mweb）

## 概述

定期購使用 Razer 信用卡付款，**第一次下單跳付款頁**，付款成功後將 Razer 回傳的 `card_token` 存入定期購資料，後續定期建單時直接用 token 做 server-to-server 扣款，不需再跳頁。

---

## 第一次下單：Token 儲存流程

```
消費者結帳（含定期購商品）
    ↓
ThirdPartyProcess flow:
  ThirdPartyPayApiProcessor
    → ProcessPayment → PaymentMiddlewareService.PaymentRequest
    → RazerPayChannelService.GetPayExtendInfo（IsRegularOrderAutoGen = false）
      → 帶卡號/CVV → Razer 回傳 Redirect URL
    → PaymentStatus.Redirect → IsFlowFinished = true → flow 停住
    ↓
消費者在 Razer 付款頁刷卡
    ↓
Razer redirect 回 mweb Return URL
    ↓
FinishPayment()
  → PaymentMiddlewareService.QueryPayment()
    → 查詢 Razer 付款結果
    → RazerPayChannelService.AppendCardInfo()        ← creditcard_token 寫入 context
       context.ThirdPartyPaymentInfo.ExtendInfo["creditcard_token"] = <Razer token>
    ↓
ReturnCode == Success → ThirdPartyFinishProcess flow:
  1. CreateGlobalInvoiceProcessor
  2. TransferOrderProcessor       → 轉單 NMQ task
  3. CreateRegularOrderProcessor  ← 存 token 在這裡
     └─ L249: market == "MY" && PayProfileType == CreditCardOnce_Razer
        extendInfo["credit_card_token"] = context.ThirdPartyPaymentInfo.ExtendInfo["creditcard_token"]
        extendInfo["credit_card_user_email"] = ...
        extendInfo["credit_card_user_fullname"] = billing_name
        → 存入 Regular Order DB
  4. AfterOrderProcessor
```

### 關鍵程式位置

| 步驟 | 檔案 | 說明 |
|---|---|---|
| Token 寫入 context | `RazerPayChannelService.cs` L447-474 `AppendCardInfo` | Razer Query 結果回傳後，把 `creditcard_token` 寫入 context |
| Token 存入 DB | `CreateRegularOrderProcessor.cs` L249-255 | MY + CreditCardOnce_Razer 分支，取 context 的 token 存進定期購 extendInfo |
| AppendCardInfo 呼叫點 | `PaymentMiddlewareService.cs` L208 | QueryPayment 後呼叫 |
| ThirdPartyFinishProcess | `TradesOrderPaymentService.cs` L437 | 付款成功後執行的 flow |

---

## 後續定期建單：Token 付款流程

```
RegularCreateOrderJob（定期購排程）
    ↓
mweb API: SubscriptionOrderAutoGen
    ↓
GetPayProcessContextForRegularOrder
  → GetProcessContextForRegularOrderPay
  → context.IsRegularOrderAutoGen = true
  → context.RegularOrderCheckoutInfo = 從 DB 讀出的 token 資料
    ↓
CreateTradesOrder → ThirdPartyProcess flow:
  ThirdPartyPayApiProcessor
    → ProcessPayment → PaymentMiddlewareService.PaymentRequest
    → RazerPayChannelService.GetPayExtendInfo（IsRegularOrderAutoGen = true）
      → L160: payInfo["card_token"] = checkOutInfo.CreditCardToken  ← 使用存好的 token
      → L169: 不帶卡號，改帶 card_token
      → 打 PaymentMiddleware → Razer token charge（不跳頁）
    ↓
Razer 非同步處理 token 扣款
  → PaymentStatus.Pending → OrderSlaveFlow_StatusDef = WaitingToPay
  → flow 繼續 → TransferOrderProcessor → 轉單 NMQ task
    ↓
API 回傳 OrderSlaveFlowStatus = WaitingToPay（Razer 確認中）
```

### 關鍵程式位置

| 步驟 | 檔案 | 說明 |
|---|---|---|
| token 讀入 payInfo | `RazerPayChannelService.cs` L160-174 `GetPayExtendInfo` | IsRegularOrderAutoGen=true 分支，帶 card_token |
| Pending 狀態處理 | `TradesOrderPaymentService.cs` L330-348 | Razer 非同步 → WaitingToPay → flow 繼續 |
| 轉單 NMQ task 建立 | `TransferOrderProcessor.cs` L62-92 | 建立 TransferOrderToERP task |

---

## 兩種 Razer 付款路徑對照

| 情境 | GetPayExtendInfo | Razer 回應 | IsFlowFinished | 後續 |
|---|---|---|---|---|
| 一般消費者結帳 | 帶卡號/CVV | Redirect URL | `true` → flow 停住 | 消費者付款後 Return URL → ThirdPartyFinishProcess |
| **定期購自動建單** | 帶 card_token | Pending/Success | `false` → flow 繼續 | 直接 → TransferOrderProcessor → 轉單 |

---

## SalesOrder 狀態追蹤

定期購 Razer token 扣款後的轉單 CSP（`csp_TradesOrderTransToSalesOrderWithFlow_Mall`）：

- L193 `@sourcePayTypes` 包含 `CreditCardOnce_Razer`
- L1030: `SalesOrderSlave_StatusDef = WaitingThirdPartyPaymentCheck`（等 Razer 確認）
- Razer webhook 確認後 → `WaitingToShipping`

**PHP Pending Job `availableStatus = ["WaitingToShipping", "ConfirmedToShipping", "Finish"]` 的設計假設**：  
Pending Job 執行時（約建單後 30 分鐘），Razer webhook 已完成，SalesOrder 狀態已推進到 WaitingToShipping。

### 已知風險

若 Razer webhook 延遲 或 NMQ 轉單尚未完成，Pending Job 查 SCM 時：
- `TotalCount = 0`（SalesOrder 不存在）→ PHP 標 **CreditCardFail**
- `OrderStatus = WaitingThirdPartyPaymentCheck` → PHP 標 **CreditCardFail**

兩種情況在 PHP 的行為都是錯誤標記失敗。.NET 版本需決定是否改善此行為。
