# Meta Graph API Rate Limit 三層架構

Meta API 的 Rate Limit 分為三個層級，依據直播場景的實際影響程度分析如下：

---

## 第一層：每秒速率（App-Level Rate Limit）⚠️ 主要瓶頸

### 什麼是 App-Level？

App-Level 指的是以「我們的 Facebook App（即 91APP 在 Meta Developer 上註冊的應用程式）」為單位計算的速率限制。無論是哪間商店、哪個用戶觸發的動作，只要是透過同一個 App ID 呼叫 Meta API，全部共用同一個配額。

> 類比：整棟大樓只有一條網路線（250 req/s），不管是 1 樓還是 10 樓的人在用，總頻寬是固定的。

### 在直播場景中的具體意義

以下所有呼叫，全部共用同一個 250 req/s 的 App 配額：

| 呼叫來源 | API 類型 |
|----------|----------|
| 顧客留言觸發關鍵字 → 系統發加車私訊 | Meta Messenger API |
| 系統 polling 抓取直播留言 | Meta Graph API |
| 主播推薦商品 → 系統發公開留言 | Meta Graph API |

### 上限與情境估算

- **上限：250 req/s**
- 這是真正的瓶頸。每秒超過 250 次呼叫即會被節流。
- 情境估算：3,000 人同時觸發 `A9+1`，最快需要 12 秒，實際延遲估計約 **30～60 秒**。

---

## 第二層：每小時總量（User Token Rate Limit）✅ 通常不是問題

### 什麼是 User Token？

User Token（使用者存取權杖）是在 OAuth 授權流程中，由商家管理員授權後產生的 Token，代表「這位使用者允許我們的 App 代表他操作 Facebook」。

### 在 91APP 直播系統中的使用時機

| 使用時機 | 說明 |
|----------|------|
| OAuth 授權時 | 取得商家管理員的 User Token，用來換取各粉專的 Page Access Token |
| 讀取商家管理的粉專清單 | 呼叫 `/me/accounts` 取得商家可管理的所有粉專 |

> User Token 取得粉專列表後，後續的直播操作（留言、發私訊、訂閱 Webhook）都會改用 **Page Access Token**，而不再繼續使用 User Token。

### Rate Limit 特性

- 每個 User Token 有**獨立的**每小時呼叫配額（不與其他使用者共用）
- 在一般直播場景下，系統對 User Token 的呼叫量極少（僅 OAuth 階段使用），每日總量通常**不是瓶頸**

---

## 第三層：Spam 判定風險 ⚠️ 需注意發送策略

在短時間內對大量用戶批次發送**相同內容**（例如統一推播加車連結），有被 Facebook 判定為 **spam** 的風險。

建議：
- 避免在同一秒內對數百人發送完全相同的訊息
- 可在訊息中加入個人化變數（如顧客名稱、商品名稱）以降低 spam 觸發機率
