# 05 - calculated_statement_descriptor 顯示錯誤帳號名稱

## 問題描述

付款成功後，Stripe 回傳的 `calculated_statement_descriptor` 值與實際商店不符。

**案例**：5 號店的交易，`calculated_statement_descriptor` 卻顯示 `"SHOPID2 STANDARD"`。

---

## 問題根因

### `calculated_statement_descriptor` 由誰決定？

Stripe 的 `calculated_statement_descriptor` **不是由程式碼設定的**，而是由 Stripe 後台根據以下規則自動計算：

```
calculated_statement_descriptor
  = PaymentIntent 中的 statement_descriptor（若有傳入）
    OR 子帳號（Connected Account）在 Stripe Dashboard 設定的 statement_descriptor
```

---

### 各付款流程的行為差異

| 付款流程 | 程式碼是否傳入 statement_descriptor | 說明 |
|----------|--------------------------------------|------|
| **DirectCharge** | ❌ **不傳入** | Stripe 自動從子帳號 Dashboard 設定讀取 |
| **DestinationCharge** | ✅ **主動傳入** | 先呼叫 `GET /v1/accounts/{id}`，讀取 `settings.payments.statement_descriptor` 後塞入 PaymentIntent |

### DirectCharge 的程式碼（無設定）

```csharp
// DirectChargePaymentFlowStrategy.PaymentIntentAsync()
IDictionary<string, object> paymentIntentRequest = new Dictionary<string, object>
{
    { "payment_method", ... },
    { "amount", amount },
    { "currency", ... },
    { "confirmation_method", "automatic" },
    { "confirm", "true" },
    { "application_fee_amount", ... },
    { "return_url", ... },
    // ⚠️ 沒有 statement_descriptor
};
```

### DestinationCharge 的程式碼（主動設定）

```csharp
// DestinationChargePaymentFlowStrategy.cs
private async Task AppendStatementDescriptorAsync(
    string subAccount, IDictionary<string, object> paymentIntentRequest)
{
    //// VSTS279367 取得子帳號資料，將 statement_descriptor 塞入以便讓銀行顯示
    var account = await this._stripeHttpClient.GetAccountAsync(subAccount);
    var descriptor = account?.settings?.payments?.statement_descriptor;
    if (descriptor is not null)
    {
        paymentIntentRequest.Add("statement_descriptor", descriptor);
    }
}
```

---

## 案例還原

```
商店 ShopId = 5
  ↓
mweb StripePayChannelService.GetShopStripeSetting(shopId=5)
  → StripeSubAccount = "acct_1EzmjGHfnYtXGyLl"
  → StripeAccountType = Standard → payment_flow = "DirectCharge"
  ↓
Payment Middleware POST /v1/payment_intents
  → 送往子帳號 acct_1EzmjGHfnYtXGyLl（DirectCharge）
  → 未帶 statement_descriptor
  ↓
Stripe 自動讀取 acct_1EzmjGHfnYtXGyLl 的 Dashboard 設定
  → Settings → Business settings → Statement descriptor = "SHOPID2 STANDARD"
  ↓
calculated_statement_descriptor = "SHOPID2 STANDARD"  ← 顯示錯誤名稱
```

---

## 問題結論

| 項目 | 說明 |
|------|------|
| 是程式碼 Bug 嗎？ | ❌ **不是**，DirectCharge 本身設計上不傳 statement_descriptor |
| 問題根本原因 | Stripe Connected Account `acct_1EzmjGHfnYtXGyLl` 的 **Statement Descriptor** 在 Stripe Dashboard 被設定為 `SHOPID2 STANDARD`（帳號名稱設定錯誤） |
| 91APP ShopId 與 Stripe Statement Descriptor 的關係 | **完全獨立**，ShopId 是 91APP 內部編號，Stripe 帳號名稱是 Stripe 端的 Dashboard 設定，兩者不會自動對應 |

---

## 修正方式

**需由 Stripe Dashboard 端修正，程式碼無需異動。**

1. 登入 Stripe Dashboard
2. 切換到 Connected Account `acct_1EzmjGHfnYtXGyLl`
3. 前往：**Settings → Business settings → Public details**
4. 修改 **Statement descriptor** 為正確的商店名稱
5. 儲存後，後續交易的 `calculated_statement_descriptor` 即會反映新值

> ⚠️ 注意：修改後只影響**新建立**的 PaymentIntent，不會回溯更新歷史交易的 statement descriptor。

---

## DestinationCharge 是否有同樣問題？

**理論上不會**，因為 `DestinationCharge` 會在建立 PaymentIntent 前主動呼叫 `GET /v1/accounts/{id}` 取得子帳號的 `settings.payments.statement_descriptor` 並傳入，所以即使 Connected Account 帳號名稱有誤，只要 payments statement_descriptor 有正確設定，就不受影響。

但若 `account.settings.payments.statement_descriptor` 也設定錯誤，DestinationCharge 同樣會有顯示問題。

---

## 相關程式碼位置

| 元件 | 路徑 |
|------|------|
| DirectCharge（無設定） | `src/Plugins/.../StripePaymentFlowStrategies/DirectChargePaymentFlowStrategy.cs` |
| DestinationCharge（主動設定） | `src/Plugins/.../StripePaymentFlowStrategies/DestinationChargePaymentFlowStrategy.cs` |
| 帳號類型判斷 | `WebStore/Frontend/BE/ShopDefault/StripeSettingsEntity.cs` → `GetStripePaymentFlow()` |
| 商店 Stripe 設定取得 | `WebStore/Frontend/BLV2/ShopDefault/ShopDefaultService.cs` → `GetShopStripeSetting()` |
