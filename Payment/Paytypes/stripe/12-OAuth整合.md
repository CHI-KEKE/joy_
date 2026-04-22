## 11. OAuth 整合與前台認證

### 11.1 概述

Stripe OAuth 整合提供安全的帳戶連接機制，讓商家可以透過標準的 OAuth 2.0 流程將其 Stripe 帳戶與平台系統連接。

### 11.2 OAuth 控制器

#### 11.2.1 控制器基本資訊

**控制器名稱**：`StripeOAuthController`

**主要功能**：
- 處理 OAuth 授權流程
- 管理 Authorization Code 兌換
- 處理 Access Token 取得

#### 11.2.2 OAuth 流程架構

```
商家 → Stripe授權頁面 → 同意授權 → 回調平台 → 取得Access Token → 完成連接
```

### 11.3 Token 交換 API

#### 11.3.1 API 端點資訊

**端點 URL**：
```
https://connect.stripe.com/oauth/token
```

**HTTP 方法**：`POST`

#### 11.3.2 請求參數

**必要參數**：

| 參數名稱 | 說明 | 範例值 |
|----------|------|--------|
| **client_secret** | 應用程式密鑰 | `sk_live_...` |
| **code** | 授權代碼 | 從回調 URL 取得的 code |
| **grant_type** | 授權類型 | `authorization_code` |

#### 11.3.3 完整 API 呼叫格式

```http
POST https://connect.stripe.com/oauth/token
Content-Type: application/x-www-form-urlencoded

client_secret={secret}&code={code}&grant_type=authorization_code
```

### 11.4 OAuth 整合流程

#### 11.4.1 授權流程步驟

| 步驟 | 動作 | 說明 |
|------|------|------|
| **1** | 導向授權頁面 | 將商家導向 Stripe OAuth 授權頁面 |
| **2** | 商家授權** | 商家在 Stripe 頁面上同意授權 |
| **3** | 接收回調 | 平台接收帶有 authorization code 的回調 |
| **4** | 交換 Token | 使用 code 向 Stripe 請求 access token |
| **5** | 儲存連接資訊 | 將取得的帳戶資訊儲存到系統中 |

#### 11.4.2 回應處理

**成功回應範例**：
```json
{
  "access_token": "sk_live_...",
  "scope": "read_write",
  "livemode": true,
  "token_type": "bearer",
  "refresh_token": "rt_...",
  "stripe_user_id": "acct_...",
  "stripe_publishable_key": "pk_live_..."
}
```

**回應欄位說明**：
| 欄位 | 說明 | 用途 |
|------|------|------|
| **access_token** | 存取令牌 | 用於 API 呼叫的認證 |
| **stripe_user_id** | Stripe 帳戶 ID | Connected Account 識別碼 |
| **stripe_publishable_key** | 公開金鑰 | 前端 SDK 使用 |

### 11.5 安全考量

#### 11.5.1 安全措施

| 安全項目 | 實作方式 | 重要性 |
|----------|----------|--------|
| **HTTPS 強制** | 所有 OAuth 端點使用 HTTPS | 防止資料洩露 |
| **State 參數** | 使用隨機 state 防止 CSRF | 驗證請求來源 |
| **Token 加密** | 加密儲存 access token | 保護敏感資料 |

#### 11.5.2 錯誤處理

**常見錯誤**：
- **invalid_grant**：authorization code 無效或過期
- **invalid_client**：client_secret 錯誤
- **access_denied**：商家拒絕授權

### 11.6 整合測試

#### 11.6.1 測試檢查項目

| 測試項目 | 驗證重點 | 預期結果 |
|----------|----------|----------|
| **授權流程** | 完整的 OAuth 流程 | 成功取得 access token |
| **錯誤處理** | 各種錯誤情況 | 適當的錯誤訊息和處理 |
| **安全驗證** | State 參數和 HTTPS | 安全機制正常運作 |

---

---

## 12. 信用卡付款流程
