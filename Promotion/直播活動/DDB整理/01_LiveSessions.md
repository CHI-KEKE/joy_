# DynamoDB Table：LiveSessions

## 設定

| 項目 | 值 |
|------|-----|
| Config Key | `_N1CONFIG:DynamoDB:TableNames:LiveSessions` |
| HK QA Table 名稱 | `HK_QA_91APPLive_LiveSessions` |

---

## 欄位說明

| 欄位 | 類型 | 說明 | 範例 |
|------|------|------|------|
| `LiveSessionId` (PK) | String | 直播場次 ID | `"LS251229L0000D"` |
| `ShopId` | Number | 商店 ID | `2` |
| `Title` | String | 直播活動名稱 | `"1229安博關鍵字"` |
| `Status` | String | 場次狀態 | `"Scheduled"` / `"Live"` / `"Ended"` |
| `Platform` | String | 平台 | `"Facebook"` |
| `PlatformLiveId` | String | 連結的 FB 直播影片 ID | `""` |
| `PlatformPageId` | String | 連結的 FB 粉專 ID | `""` |
| `PlatformPostId` | String | FB 貼文 ID（用於留言監聽）| `""` |
| `StartTime` | String | 預計開始時間（ISO 8601）| `"2025-12-30T00:00:00.0000000"` |
| `LiveHostName` | String | 直播主名稱 | `""` |
| `EffectPeriod` | String (JSON) | 活動有效期間 | `{"StartTime":"...","EndTime":"..."}` |
| `MessageTemplates` | String (JSON) | 訊息文案模板（見下方說明）| |
| `Remark` | String | 備註 | `""` |
| `ValidFlag` | Boolean | 是否有效 | `true` |
| `CreatedAt` | String | 建立時間（ISO 8601）| `"2025-12-29T16:59:45.6418952+08:00"` |
| `CreatedUser` | String | 建立者 | `"User"` |

### 連結 FB 直播後新增/更新的欄位

連結 Facebook 直播（`POST /api/live/sessions/{id}/link-facebook`）成功後，以下欄位會被寫入：

| 欄位 | 說明 |
|------|------|
| `PlatformLiveId` | FB LiveVideoId |
| `PlatformPageId` | FB PageId |
| `PlatformPostId` | FB PostId（用於後續留言，若為原生影片則用 VideoId）|
| `LinkedAt` | 連結時間（UTC）|
| `LiveStartTime` | 直播開始時間（UTC）|
| `LiveTitle` | FB 直播標題 |
| `LiveStatus` | 轉換後的系統狀態 |
| `LiveEmbedHtml` | FB 提供的 Embed HTML（前端顯示直播畫面用）|
| `LivePermalinkUrl` | FB 直播永久連結 |
| `LiveDescription` | FB 直播描述 |

---

## 實際資料範例（JSON）

```json
{
  "LiveSessionId": { "S": "LS251229L0000D" },
  "CreatedAt": { "S": "2025-12-29T16:59:45.6418952+08:00" },
  "CreatedUser": { "S": "User" },
  "EffectPeriod": {
    "S": "{\"StartTime\":\"2025-12-30T00:00:00\",\"EndTime\":\"9999-12-31T23:59:59Z\"}"
  },
  "LiveHostName": { "S": "" },
  "MessageTemplates": {
    "S": "{\"CartMessage\":{\"Message\":\"感謝您於{{LIVE_NAME}}活動購物!{{PRODUCT_INFO}}商品加入購物車連結:{{ADD_TO_CART_LINK}}\",\"ButtonText\":\"加入購物車\"},\"ProductRecommendation\":{\"Message\":\"{{LIVE_NAME}}推薦商品{{PRODUCT_INFO_AND_KEYWORDS}}下單規則留言「關鍵字+數量」\"}}"
  },
  "Platform": { "S": "Facebook" },
  "PlatformLiveId": { "S": "" },
  "PlatformPageId": { "S": "" },
  "Remark": { "S": "" },
  "ShopId": { "N": "2" },
  "StartTime": { "S": "2025-12-30T00:00:00.0000000" },
  "Status": { "S": "Scheduled" },
  "Title": { "S": "1229安博關鍵字" },
  "ValidFlag": { "BOOL": true }
}
```

---

## Status 狀態說明

| 狀態 | 說明 |
|------|------|
| `Scheduled` | 已排程，尚未開播 |
| `Live` | 直播中 |
| `Ended` | 已結束 |

---

## MessageTemplates 結構

`MessageTemplates` 是一個 JSON 字串，嵌在 LiveSessions 表內，包含三種訊息模板：

```json
{
  "CartMessage": {
    "Message": "感謝您於{{LIVE_NAME}}活動購物!{{PRODUCT_INFO}}商品加入購物車連結:{{ADD_TO_CART_LINK}}",
    "ButtonText": "加入購物車"
  },
  "ProductRecommendation": {
    "Message": "{{LIVE_NAME}}推薦商品{{PRODUCT_INFO_AND_KEYWORDS}}下單規則留言「關鍵字+數量」"
  },
  "DrawMessage": {
    "Message": "（抽獎開獎公告模板，含 {{ACTIVITY_NAME}}、{{WINNER_LIST}}）"
  },
  "WinnerMessage": {
    "Message": "（中獎私訊模板，含 {{ACTIVITY_NAME}}、{{WINNER_NAME}}、{{COUPON_CODE}}）"
  }
}
```

> 詳細變數替換邏輯見 `訊息文案/訊息文案運作說明.md`，Redis cache 見 `04_MessageTemplates.md`。

---

## 來源檔案

- `LiveSession_Table.md`
- `粉專綁定整理/07_連結FB直播.md`（LiveLinkEntity 欄位段落）
