# 13 - 商店與 Stripe 帳號的關係

## 概念總覽

> **91APP 是 Stripe 的「平台帳號」，每間商店在 Stripe 都有自己的「子帳號（Connected Account）」。**  
> 付款時，91APP 用自己的 Platform Key + 商店的 `acct_xxxxx`，代替商店向 Stripe 發起交易。

---

## 帳號結構

```
Stripe 世界
│
├── 🏢 91APP Platform Account（主帳號）
│       └── 持有 sk_live_xxxx（平台自己的 Secret Key）
│               │
│               ├── acct_AAA  ← 商店 A 的子帳號（Standard）
│               ├── acct_BBB  ← 商店 B 的子帳號（Standard）
│               ├── acct_CCC  ← 商店 C 的子帳號（Custom）
│               └── acct_DDD  ← 商店 D 的子帳號（Custom）
```

每間 91APP 商店對應一個 Stripe 子帳號（`acct_xxxxx`），
這個對應關係存在資料庫 `ShopDefault` 表中。

---

## 兩種帳號類型

### 🔵 Standard 帳號（大型商店，例如 SASA）

| 項目 | 說明 |
|------|------|
| 帳號所有人 | 商店自己申請、自己擁有 |
| 費率 | 商店自行與 Stripe 談判 |
| 付款流程 | **DirectCharge** — 錢直接進商店子帳號 |
| API Key | 91APP 平台的 `StandardAcctLiveSecretKey`（所有 Standard 共用） |
| 商店區分方式 | `Stripe-Account: acct_xxx` Header（每間不同） |

```
使用者付款
    → 91APP 平台 Key（Authorization）+ Stripe-Account: acct_商店 Header
    → 錢直接進 商店子帳號
    → 91APP 從中收 application_fee
```

---

### 🟠 Custom 帳號（小型商店，由 91APP 在 OSM 代開）

| 項目 | 說明 |
|------|------|
| 帳號所有人 | 91APP 代替商店建立 |
| 費率 | 由平台統一設定 |
| 付款流程 | **DestinationCharge** — 錢先進平台，再 Transfer 給商店 |
| API Key | 91APP 平台的 `CustomAcctLiveSecretKey`（所有 Custom 共用） |
| 商店區分方式 | Body 的 `transfer_data[destination]: acct_xxx`（每間不同） |

```
使用者付款
    → 91APP 平台 Key（Authorization）
    → 錢進 91APP 平台主帳號
    → Transfer → 商店子帳號（transfer_data[destination]）
    → 91APP 從中收 application_fee
```

---

## 為什麼 Standard 商店（自己的帳號）也用 91APP 的 Key？

### 關鍵：OAuth 授權

Standard 商店雖然帳號是自己的，但在接入 91APP 結帳前，必須完成一次 **OAuth 授權**：

```
1. 商店登入 OSM 後台
       ↓
2. 點擊「連結 Stripe 帳號」
       ↓
3. 跳轉 Stripe 官網授權頁面：
   「是否允許 91APP 平台代為處理你的付款？」
       ↓
4. 商店老闆點「同意」
       ↓
5. Stripe 回傳 OAuth 結果：
   {
     "access_token": "sk_live_商店自己的Key",  ← 91APP 不存這個
     "stripe_user_id": "acct_xxxxx",           ← 只存這個進 ShopDefault
     "stripe_publishable_key": "pk_live_..."
   }
       ↓
6. 91APP 只記錄 acct_xxxxx，access_token 直接丟棄
```

OAuth 完成後，Stripe 內部就記下了：
> **「acct_xxxxx 這個商店，已授權 91APP 平台帳號代為操作」**

---

## `sk_live_...` 有兩種，長得一樣但身份不同

這是最容易混淆的地方：

```
91APP 平台帳號的 Key：  sk_live_AbcXXXXXXXXXXXXX  ← config 裡的 StandardAcctLiveSecretKey
商店 OAuth 後的 Key：   sk_live_XyzYYYYYYYYYYYYY  ← OAuth access_token（91APP 不存）
```

**格式完全相同，但代表不同帳號，意義完全不一樣。**

### config 裡的 `StandardAcctLiveSecretKey` 是哪一把？

**是 91APP 自己的 Stripe Platform Account 的 Key，與任何商店無關。**  
它寫死在 `AppSettings.config` 裡，所有 Standard 商店的交易都用這同一把 Key。

---

## 為什麼不直接用商店的 access_token 來打 API？

| | 用商店自己的 `access_token` | 用平台 Key + Stripe-Account Header（現行做法）|
|---|---|---|
| 需要存什麼 | 每間商店各存一把 `sk_live_...` | 只存 `acct_xxxxx`（簡單字串）|
| 管理複雜度 | 高（N 間商店 = N 把 Key，還需處理 refresh）| 低（一把 Key 管全部）|
| Key 洩漏風險 | 高（影響單一商店）| 相對集中管理 |
| 91APP 的選擇 | ❌ | ✅ |

---

## 付款時 Stripe 的驗證邏輯

```
91APP 打 API：
  Authorization: Bearer sk_live_平台Key   → 「我是 91APP 平台」
  Stripe-Account: acct_商店xxxxx          → 「我要代表這間商店操作」
          ↓
Stripe 驗證：
  1. sk_live_平台Key 是否合法？ → ✅
  2. acct_商店xxxxx 是否已授權給這個平台？ → ✅（OAuth 時已同意）
          ↓
  放行，錢進 acct_商店xxxxx
```

---

## DB 設定對應（ShopDefault 表）

| DB Key | 說明 | 哪種帳號使用 |
|--------|------|------------|
| `StripeAccountType` | `Standard` 或 `Custom` | 決定走哪條流程 |
| `StripeSubAccount` | Standard 子帳號 `acct_xxx` | Standard |
| `StripeCustomSubAccount` | Custom 子帳號 `acct_xxx`（正式）| Custom |
| `StripeCustomTestSubAccount` | Custom 子帳號 `acct_xxx`（測試）| Custom |
| `EnableCustomDate` | Custom 帳號啟用日期，未到則降級為 Standard 流程 | Custom |

### 查詢語法

```sql
SELECT ShopDefault_Key, ShopDefault_NewValue
FROM ShopDefault (NOLOCK)
WHERE ShopDefault_ValidFlag = 1
  AND ShopDefault_ShopId = @shopId
  AND ShopDefault_Key IN (
    'StripeAccountType',
    'StripeSubAccount',
    'StripeCustomSubAccount',
    'EnableCustomDate'
  )
```

---

## EnableCustomDate 降級機制

商店 DB 設定 `StripeAccountType = Custom`，但若 `EnableCustomDate` 尚未到達，系統執行期**自動降級為 Standard**：

```
DB: StripeAccountType = "Custom", EnableCustomDate = "2025-06-01"
今天 = 2025-05-01（未到）
          ↓
執行期實際值: StripeAccountType = "Standard"
             payment_flow = "DirectCharge"
```

**用途**：讓商店可以提前在 DB 設定好 Custom 帳號資訊，到指定日期才自動切換，不需要當天手動改 DB。

---

## 完整關係圖

```
ShopId（91APP 內部編號）
    │
    ▼ GetShopStripeSetting(shopId)
ShopDefault 表
    ├── StripeAccountType  = "Standard" or "Custom"
    ├── StripeSubAccount   = "acct_xxxxx"（Standard 用）
    ├── StripeCustomSubAccount = "acct_yyyyy"（Custom 用）
    └── EnableCustomDate   = "YYYY-MM-DD"（Custom 啟用日）
    │
    ▼
StripeSettingsEntity
    ├── GetSubAccount()        → acct_xxxxx（依類型選擇）
    └── GetStripePaymentFlow() → "DirectCharge" or "DestinationCharge"
    │
    ├─── Standard → DirectCharge
    │       Authorization: sk_live_平台StandardKey（所有 Standard 共用）
    │       Stripe-Account: acct_xxxxx（每間商店不同）
    │       → 錢直接進商店子帳號
    │
    └─── Custom → DestinationCharge
            Authorization: sk_live_平台CustomKey（所有 Custom 共用）
            Body: transfer_data[destination] = acct_yyyyy（每間商店不同）
            → 錢進平台 → Transfer → 商店子帳號
```

---

## 相關程式碼位置

| 元件 | 路徑 |
|------|------|
| 帳號設定實體 | `WebStore/Frontend/BE/ShopDefault/StripeSettingsEntity.cs` |
| 商店設定取得 | `WebStore/Frontend/BLV2/ShopDefault/ShopDefaultService.cs` |
| Key 選擇邏輯 | `WebStore/Frontend/BLV2/PayChannel/StripePayChannelService.cs` → `GetStripeApiKey()` |
| Stripe API Client | `src/Plugins/.../HttpClients/IStripeHttpClient.cs` |
| OAuth 授權流程 | `WebStore/Frontend/BLV2/...StripeOAuthController` |
