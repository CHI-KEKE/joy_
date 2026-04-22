# Cybersource — Pay 付款流程分析

## 核心架構：Secure Acceptance（表單導向）

Cybersource 的付款模式與 Stripe 根本不同。  
PMW **不直接呼叫 Cybersource 付款 API**，而是組好一份簽名表單，讓瀏覽器直接 POST 到 Cybersource 的頁面完成付款。

```
前台 → POST /Pay → PMW → 回傳 FormPostAction（含簽名表單欄位）
                           ↓
                       瀏覽器直接 POST → Cybersource Secure Acceptance URL
                                          ↓
                                     Cybersource 完成付款後 redirect 回前台
```

---

## Pay 流程詳解

### Step 1：讀取設定（從 Header 取得認證資訊）

`CybersourceConfiguration` 從 request header 取得以下欄位：

| Header 欄位 | 用途 |
|------------|------|
| `x-merchant-id` | 商戶 ID（REST API 用） |
| `x-secret-key` | REST API 簽名密鑰 |
| `x-merchant-key-id` | REST API Key ID |
| `x-profile-id` | Secure Acceptance Profile ID |
| `x-profile-access-key` | Secure Acceptance Access Key |
| `x-profile-secret` | Secure Acceptance 簽名密鑰 |

另從 config 取得：
- `_N1CONFIG:Plugins:Cybersource:HttpClient:PaymentUrl` — Cybersource Secure Acceptance 頁面 URL

---

### Step 2：組建表單欄位（CybersourcePaymentFormEntity）

| 欄位名稱 | 來源 | 說明 |
|---------|------|------|
| `access_key` | `x-profile-access-key` | Secure Acceptance 識別用 |
| `profile_id` | `x-profile-id` | Profile 識別 |
| `transaction_uuid` | `request.ExtendInfo.UniqueKey` | 防重複提交的唯一識別碼（50字元內） |
| `signed_date_time` | `DateTime.UtcNow` | 格式：`yyyy-MM-dd'T'HH:mm:ss'Z'` |
| `locale` | `request.ExtendInfo.Lang` | 語系（如 `zh-tw`） |
| `transaction_type` | 固定值 `"sale"` | 授權 + 請款一次完成 |
| `reference_number` | `request.TradesOrderGroupCode` | 訂單號，對應 TG Code |
| `amount` | `request.Amount` | 金額 |
| `currency` | `request.Currency` | 幣別 |
| `signed_field_names` | 所有欄位 key 的逗號串接 | 告知 Cybersource 哪些欄位被簽名 |
| `unsigned_field_names` | 空字串 | 本實作無未簽名欄位 |

---

### Step 3：HMAC-SHA256 簽名（SecureAcceptanceHelper）

簽名演算法：

```
待簽名字串 = signed_field_names 中列出的欄位，每筆格式為 "key=value"，逗號串接
簽名 = HMAC-SHA256(待簽名字串, x-profile-secret)，Base64 編碼
```

**簽名驗證機制**：
- 組表單時：PMW 簽名後加入 `signature` 欄位，一起送到 Cybersource
- 收 callback 時（QueryPayment Form-Data 路徑）：PMW 驗證 Cybersource 回傳的 `signature` 是否一致

---

### Step 4：回傳 FormPostAction

PMW 不回傳付款結果，而是回傳：

```json
{
  "ReturnCode": "2003",
  "RequiredAction": "FormPost",
  "ActionUrl": "https://testsecureacceptance.cybersource.com/pay",
  "FormData": {
    "access_key": "...",
    "profile_id": "...",
    "transaction_uuid": "...",
    "signed_field_names": "access_key,profile_id,...",
    "signed_date_time": "2026-01-01T00:00:00Z",
    "locale": "zh-tw",
    "transaction_type": "sale",
    "reference_number": "TG123456",
    "amount": "1000",
    "currency": "HKD",
    "signature": "<hmac-sha256-base64>"
  }
}
```

> **ReturnCode `2003`（WaitingToPay）是 Pay 階段的正常結果，代表「請前台引導用戶至付款頁」**。

---

## Request ExtendInfo 欄位

```csharp
public class PaymentRequestExtendInfo
{
    public string Lang { get; set; }       // 語系，如 "zh-tw"
    public string UniqueKey { get; set; }  // 防重複提交的唯一識別碼
}
```

---

## 特殊設計說明

### 為什麼 Pay 不呼叫任何 API？

Cybersource Secure Acceptance 的設計哲學是：
> 付款頁面由 Cybersource 托管，PMW 只負責組建合法的跳轉表單。

優點：
- 卡號等敏感資訊完全不經過商戶系統（PMW、前台）
- 減少 PCI DSS 合規範圍

缺點：
- 付款結果需透過 Cybersource redirect 或 webhook 告知，有同步延遲問題

### transaction_type 固定為 `"sale"`

`sale = authorization + capture`，即授權與請款一次完成，不走先授權後請款的兩階段流程。

---

## 環境 URL 對照

| 環境 | PaymentUrl |
|------|-----------|
| Test | `https://testsecureacceptance.cybersource.com/pay` |
| Production | `https://secureacceptance.cybersource.com/pay` |
