# Webhook 訂閱

## 訂閱時機

OAuth 完成後，後端嘗試呼叫 Facebook Graph API 為粉專訂閱 Webhook。

```
POST https://graph.facebook.com/v24.0/{pageId}/subscribed_apps
  ?subscribed_fields=feed,live_videos,message_reactions,messages,messaging_postbacks
  &access_token={PageAccessToken}
```

> **Webhook 失敗 ≠ 綁定失敗**。訂閱失敗時，BindingStatus 標記為 `WebhookPending`，不影響粉專資料存入。

---

## 綁定前置條件與錯誤情境

| 情境 | 結果 |
|------|------|
| 無粉專 | 綁定失敗（`Retrieve page accounts failed for shop`）|
| 有粉專，有內容權限 | 綁定成功，Webhook 訂閱成功 |
| 有粉專，無內容權限 | 綁定成功，但 Webhook 訂閱失敗 |

### 部分權限定義

- **工作權限**：粉專員工層級
- **管理權限**：完整管理員層級（訂閱 Webhook 需要此層級）

---

## Webhook 訂閱失敗案例

### 錯誤一：權限不足（Forbidden）

```bash
Failed to subscribe webhook for page: "169441722919446".
Status: Forbidden
Response: {
  "error": {
    "message": "(#200) User does not have sufficient administrative permission for this action on this page.
               If the page business requires Two Factor Authentication,
               the user also needs to enable Two Factor Authentication.",
    "type": "OAuthException",
    "code": 200,
    "fbtrace_id": "AbHjns2-dP2DDHRzLFT1R3c"
  }
}
```

**原因**：授權者對該粉專沒有足夠的管理員權限，或粉專所屬商業帳號要求啟用雙重驗證。

---

### 錯誤二：重試後仍失敗

```
Webhook subscription returned false for page "169441722919446" on attempt 1. Retrying in 2 seconds.
```

系統會自動重試，若重試後仍失敗，拋出例外：

```
Nine1.Livebuy.Common.Utils.ServerEntity.Entity.ApplicationApiException:
Facebook webhook 訂閱失敗，PageId: 169441722919446；PageName: Hong Kong Demo；請稍後再試或聯繫客服

at Nine1.Livebuy.BL.Services.FacebookPageBindings.ShopPageBindingService.CreateBindingAsync(...)
   in /src/BusinessLogic/.../ShopPageBindingService.cs:line 159
```

---

## 訂閱的 Webhook 事件欄位

| 欄位 | 說明 |
|------|------|
| `feed` | 粉專動態（貼文、留言）|
| `live_videos` | 直播狀態變更 |
| `message_reactions` | 訊息表情回應 |
| `messages` | 私訊 |
| `messaging_postbacks` | Messenger 回傳事件 |
