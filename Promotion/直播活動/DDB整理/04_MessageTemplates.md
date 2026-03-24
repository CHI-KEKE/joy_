# DynamoDB：MessageTemplates（LiveSessions 子結構）

## 儲存位置

`MessageTemplates` 不是獨立的 Table，而是 **LiveSessions 表中的一個 JSON 字串欄位**。

```
LiveSessions Table
  └── MessageTemplates (String, JSON)
        ├── CartMessage
        ├── ProductRecommendation
        ├── DrawMessage（抽獎開獎）
        └── WinnerMessage（中獎私訊）
```

---

## 讀取路徑（Redis → DynamoDB）

Worker 處理留言時，先查 Redis，miss 才讀 DynamoDB：

```
Redis Key:  live_session:{liveSessionId}:templates
TTL:        180 秒

miss → 從 DynamoDB 的 LiveSessions 表讀取 MessageTemplates 欄位
     → 寫回 Redis 快取
```

程式碼路徑：`BL.Services/MessageTemplateService.cs` L54

---

## 四種模板說明

### 1. CartMessage（加入購物車訊息）

觸發時機：粉絲留言關鍵字下單，系統建立購物車後透過 Facebook 私訊通知。

```json
{
  "CartMessage": {
    "Message": "感謝您於{{LIVE_NAME}}活動購物!{{PRODUCT_INFO}}商品加入購物車連結:{{ADD_TO_CART_LINK}}",
    "ButtonText": "加入購物車"
  }
}
```

| 變數 | 說明 |
|------|------|
| `{{LIVE_NAME}}` | 直播活動名稱 |
| `{{PRODUCT_INFO}}` | 商品名稱 + 購買規格與數量（各項目以 `\n` 分隔，末尾 TrimEnd）|
| `{{ADD_TO_CART_LINK}}` | 加入購物車連結 URL |

**組裝邏輯：**
```
sb.AppendLine(商品名稱)
sb.AppendLine(SKU規格 - $價格 x數量)
→ .TrimEnd() 移除末尾空白
```

**實際輸出：**
```
感謝您於Amy的直播活動購物!
潮流T恤
S規格 - $200 x1
商品加入購物車連結:
https://cart.url
```

---

### 2. ProductRecommendation（推薦商品訊息）

觸發時機：主播在直播間點擊「推薦商品」，系統發 Facebook 公開留言。

```json
{
  "ProductRecommendation": {
    "Message": "{{LIVE_NAME}}推薦商品{{PRODUCT_INFO_AND_KEYWORDS}}下單規則留言「關鍵字+數量」"
  }
}
```

| 變數 | 說明 |
|------|------|
| `{{LIVE_NAME}}` | 直播活動名稱 |
| `{{PRODUCT_INFO_AND_KEYWORDS}}` | 商品名稱 + 各 SKU 規格、價格與關鍵字（各項目以 `\n` 分隔）|

**組裝邏輯：**
```
格式：商品名稱\nSKU1 - $價格 | 關鍵字：K1\nSKU2 - $價格 | 關鍵字：K2
程式碼：string.Join("\n", lines)
```

**實際輸出：**
```
Amy的直播推薦商品
潮流T恤
S - $200 | 關鍵字：S1
M - $220 | 關鍵字：M1
下單規則留言「關鍵字+數量」
```

---

### 3. DrawMessage（抽獎開獎公告）

觸發時機：主辦者執行開獎，系統發 Facebook 公開留言。

```json
{
  "DrawMessage": {
    "Message": "{{ACTIVITY_NAME}}開獎結果公佈！恭喜得獎者：{{WINNER_LIST}}"
  }
}
```

| 變數 | 說明 |
|------|------|
| `{{ACTIVITY_NAME}}` | 抽獎活動名稱 |
| `{{WINNER_LIST}}` | 得獎者名單（頓號 `、` 串接在同一行）|

**組裝邏輯：**
```
string.Join("、", winners)
```

**實際輸出：**
```
🎊夏日抽獎開獎結果公佈！🏆恭喜得獎者：Amy、Bob、Cindy
```

程式碼路徑：`BL.Services/LotteryWinnerNotificationService.cs` L157

---

### 4. WinnerMessage（中獎私訊）

觸發時機：開獎後，系統對每位得獎者個別發送 Facebook 私訊。

```json
{
  "WinnerMessage": {
    "Message": "恭喜{{WINNER_NAME}}獲得{{ACTIVITY_NAME}}獎品！優惠券代碼：{{COUPON_CODE}}"
  }
}
```

| 變數 | 說明 |
|------|------|
| `{{ACTIVITY_NAME}}` | 抽獎活動名稱 |
| `{{WINNER_NAME}}` | 得獎者名稱 |
| `{{COUPON_CODE}}` | 優惠券代碼 |

程式碼路徑：`BL.Services/LotteryWinnerNotificationService.cs` L250

---

## 換行設計原則

```
┌──────────────────────────────────────────┐
│ 前端模板字串                              │
│ → 前後換行由模板中的 \n 位置控制           │
│ → 使用者在後台可自行編輯                  │
└──────────────────────────────────────────┘
                    ↓ 存入 DynamoDB
┌──────────────────────────────────────────┐
│ 後端清單型變數內容                         │
│ {{PRODUCT_INFO_AND_KEYWORDS}} → \n 分隔  │
│ {{PRODUCT_INFO}} → \n 分隔，末尾 TrimEnd │
│ {{WINNER_LIST}} → 頓號串接，單行          │
│ → 清單內部換行由後端組裝邏輯控制           │
└──────────────────────────────────────────┘
```

> **結論：後端不主動在清單變數前後加換行，排版完全由前端模板的 `\n` 位置決定。**

---

## 關鍵程式碼位置

| 功能 | 檔案 | 行號 |
|------|------|------|
| 讀取模板（Redis/DDB）| `BL.Services/MessageTemplateService.cs` | L54 |
| 組裝商品資訊（CartMessage）| `BL.Services/NotificationService.cs` | L121 |
| 替換加車訊息變數 | `BL.Services/NotificationService.cs` | L199 |
| 組裝推薦商品資訊 | `BL.Services/LiveSessionService.cs` | L1721 |
| 替換推薦商品變數 | `BL.Services/LiveSessionService.cs` | L1786 |
| 開獎訊息發送 | `BL.Services/LotteryWinnerNotificationService.cs` | L157 |
| 中獎私訊發送 | `BL.Services/LotteryWinnerNotificationService.cs` | L250 |

---

## 來源檔案

- `訊息文案/訊息文案運作說明.md`
- `LiveSession_Table.md`（MessageTemplates JSON 實際資料）
