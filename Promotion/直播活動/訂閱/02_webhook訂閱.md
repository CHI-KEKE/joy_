# Webhook 訂閱機制

## 訂閱 API

Facebook 要求在綁定粉專後，主動呼叫以下 API，才能讓 Facebook 將 Webhook 事件推送到我們的系統：

```
POST /{page-id}/subscribed_apps
```

若未呼叫此 API，即使 OAuth 授權完成，Facebook 也不會主動推送任何事件（留言、互動、直播狀態等）。

---

## 必要的訂閱欄位

訂閱時需明確指定 `subscribed_fields` 參數，目前直播功能需要的欄位如下：

| 欄位 | 用途 |
|------|------|
| `feed` | 頁面貼文與留言事件 |
| `live_videos` | 直播影片狀態變更 |
| `message_reactions` | 訊息互動（按讚、表情）|
| `messages` | 私訊（Messenger）事件 |
| `messaging_postbacks` | 訊息按鈕回呼事件 |

---

## 目前訂閱流程（現況）

```
呼叫 POST /{page-id}/subscribed_apps
    ↓
FB 回傳 error code 200（權限不足）
    ↓
系統拋出 FACEBOOK_PAGE_INSUFFICIENT_PERMISSION
    ↓
整個訂閱失敗
用戶只看到錯誤訊息，但不知道缺少哪個權限
```

**問題：** 系統目前對三個必要權限（`CREATE_CONTENT` / `MODERATE` / `MESSAGING`）皆未主動檢查，只在訂閱失敗後才回傳通用錯誤。

---

## 失敗案例

**店家：Frank's cloud store（PageId: 336245326240878）**

綁定 2 號店時出現以下錯誤：

```
FACEBOOK_WEBHOOK_SUBSCRIBE_FAILED
Facebook webhook 訂閱失敗，PageId: 336245326240878；PageName: Frank's cloud store；請稍後再試或聯繫客服
```

根本原因：粉專管理員的帳號缺少必要的 Facebook 頁面權限（`MODERATE` 或 `MESSAGING`），導致訂閱 API 呼叫被 Facebook 拒絕。

---

## 連結直播時的訂閱判斷

連結直播時，系統應新增以下判斷：

- 確認該粉絲專頁是否已成功訂閱 Webhook
- 若 `WebhookSubscribed = false`，主動觸發重新訂閱
- 若重新訂閱失敗，需明確提示使用者缺少哪個權限
