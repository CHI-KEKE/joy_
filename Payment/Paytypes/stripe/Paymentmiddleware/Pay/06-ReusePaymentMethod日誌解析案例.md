# 06 - ReusePaymentMethod 付款日誌解析案例

## 概述

本文件以一筆真實的付款日誌為例，說明如何判斷一筆 Stripe 交易的類型，以及系統是否執行了「記住信用卡」或「重用已記住的信用卡」流程。

---

## 案例資訊

| 欄位 | 值 |
|------|-----|
| TG Code | `TG260416M00075` |
| Request Path | `POST /api/v1.0/pay/CreditCardOnce_Stripe/{TGCode}` |
| PayMethod | `CreditCardOnce` |
| PaymentFlow | `DirectCharge` |
| SubAccount | `acct_1EzmjGHfnYtXGyLl` |
| Amount | 23.00 HKD |
| Customer | `cus_Symc9G6952QtoS`（name: `5_23000`） |
| PaymentMethod | `pm_1S2p3NHfnYtXGyLlhdKi7zs0` |
| 最終狀態 | `succeeded` ✅ |

---

## 完整執行流程分析

### Step 1：進入 `StripePlugin.Pay()`

```
payMethod = "CreditCardOnce"
→ 非 MobileWallet，進入 ReusePaymentMethod 判斷邏輯
```

### Step 2：分支判斷（程式碼 112~124 行）

```csharp
// 判斷是否有Token，有 -> 用Token結帳、無 -> 判斷是否記住新用卡
if (PaymentMethod != null && CustomerId != null)
    → ReusePaymentMethodPaymentIntentProcess  ← ✅ 本次走此路徑
else if (IsReusePaymentMethod == true)
    → RememberPaymentMethodProcess
else
    → strategy.Pay()（一般明碼結帳）
```

Request 帶入了 `payment_method` + `customer_id`，因此走 **ReusePaymentMethodPaymentIntentProcess**。

### Step 3：`CustomersSearchAsync` 查詢

```
GET /v1/customers/search?query=name:"5_23000"
→ 找到 1 筆：cus_Symc9G6952QtoS
```

`5_23000` = `{ShopId}_{MemberId}`，為 mweb 傳入的值。

### Step 4：`ReusePaymentMethodPaymentIntentProcess.Process()`

```csharp
// 驗證 CustomerId 存在於 search 結果中
if (customersSearch.data.Any(x => x.id == context.CustomerId))
    → PaymentIntentAsync(request, setupFutureUsage: null)
```

直接使用已存的 Token 建立 PaymentIntent，**不設定 `setup_future_usage`**。

### Step 5：建立 PaymentIntent

```
POST /v1/payment_intents
→ 狀態：succeeded
→ amount_received: 2300 (= 23.00 HKD)
→ setup_future_usage: null
```

---

## 這筆交易有記住信用卡嗎？

**➜ 否。這筆交易使用的是「已記住的信用卡」，本次並未重新儲存。**

### 判斷依據

| 指標 | 說明 | 結論 |
|------|------|------|
| Customer Search 找到 1 筆 | `cus_Symc9G6952QtoS` 已存在 | 此會員之前已記住過卡 |
| `payment_method` 由 Request 帶入 | 非本次建立的新 PM | 使用既有 Token |
| `setup_future_usage: null` | 未設定未來重用意圖 | 本次不觸發儲卡行為 |
| 無 `POST /v1/payment_methods` 呼叫 | 未建立新 PaymentMethod | — |
| 無 `POST /v1/payment_methods/{id}/attach` 呼叫 | 未綁定到 Customer | — |
| 無 `POST /v1/customers` 呼叫 | 未建立新 Customer | — |

---

## 三種付款路徑比較

| 路徑 | 觸發條件 | Stripe API 呼叫 | 結果 |
|------|----------|-----------------|------|
| **一般明碼結帳** | 無 PM、不記卡 | `CreatePaymentMethod` → `CreatePaymentIntent` | 不儲卡 |
| **RememberPaymentMethodProcess** | `IsReusePaymentMethod=true` 且無既有 PM | `CreatePaymentMethod` → `PaymentIntent(setup_future_usage: off_session)` → `Attach` 或 `CreateCustomer` | **首次儲卡** |
| **ReusePaymentMethodPaymentIntentProcess** | 帶入既有 `payment_method` + `customer_id` | `CustomersSearch` → `PaymentIntent(setup_future_usage: null)` | **使用已儲卡** |

> ✅ 本案例 = 第三種路徑

---

## 使用的信用卡資訊

| 欄位 | 值 |
|------|-----|
| 品牌 | Visa |
| 類型 | Debit |
| 末四碼 | `3237` |
| 到期日 | 04/2028 |
| 發卡國 | TW（台灣） |
| 3DS | 無（off-session 已驗證） |
| authorization_code | `161710` |
| network_transaction_id | `586106139135591` |

---

## Response 摘要

```json
{
  "return_code": "0000",
  "return_message": "Success",
  "transaction_id": "pi_3TMgy5HfnYtXGyLl0Lb41l12",
  "extend_info": {
    "payment_intent_id": "pi_3TMgy5HfnYtXGyLl0Lb41l12",
    "charge_id": "ch_3TMgy5HfnYtXGyLl0x4jrYLJ",
    "payment_method": "pm_1S2p3NHfnYtXGyLlhdKi7zs0",
    "customer_id": "cus_Symc9G6952QtoS"
  }
}
```

---

## 相關程式碼位置

| 元件 | 路徑 |
|------|------|
| 付款主流程 | `src/Plugins/NineYi.PaymentMiddleware.Plugins.Stripe/StripePlugin.cs` |
| DirectCharge 策略 | `.../StripePaymentFlowStrategies/DirectChargePaymentFlowStrategy.cs` |
| 重用 Token 流程 | `.../ReusePaymentMethodFlow/ReusePaymentMethodPaymentIntentProcess.cs` |
| 首次記卡流程 | `.../ReusePaymentMethodFlow/RememberPaymentMethodProcess.cs` |
| mweb ExtendInfo 組裝 | `WebStore/Frontend/BLV2/PayChannel/StripePayChannelService.cs` |
