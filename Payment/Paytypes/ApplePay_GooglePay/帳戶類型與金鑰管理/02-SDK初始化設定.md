# PublishableKey — SDK 初始化設定

## 核心功能

**publishableKey** 是 Stripe SDK 與 Stripe 後台互動的公開金鑰，用於識別商家身份並啟用付款功能。

| 功能項目 | 說明 |
|----------|------|
| **商家身份驗證** | 確認你是哪一個商家（Merchant） |
| **付款設定載入** | 載入商家的付款選項設定（Apple Pay、信用卡等） |
| **付款操作執行** | 產生 PaymentMethod、Token、PaymentIntent |

---

## Apple Pay 設定（iOS Swift）

必須在 App **啟動時**完成設定，才能使用 Stripe API 功能。

```swift
// 使用 publishableKey 配置 SDK，讓應用程式能與 Stripe API 通訊
StripeAPI.defaultPublishableKey = "pk_test_..."
```

---

## Google Pay 設定（React Native）

有兩種初始化方式：

```javascript
// 方法 1：使用 StripeProvider 組件
<StripeProvider publishableKey="pk_test_...">
  <PaymentScreen />
</StripeProvider>

// 方法 2：使用 initStripe 方法
initStripe({
  publishableKey: 'pk_test_...'
});
```

---

## 安全性說明

| 金鑰類型 | 用途 | 安全等級 | 使用位置 |
|----------|------|----------|----------|
| **Publishable Key** | 前端 SDK 初始化 | 公開安全 | 客戶端應用程式 |
| **Secret Key** | 後端 API 呼叫 | 高度機密 | 伺服器端 |

- publishableKey 可安全地在前端程式碼中使用
- 不會洩露敏感的商家資訊
- 僅能執行客戶端允許的操作
