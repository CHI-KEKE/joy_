# Facebook OAuth 授權流程

## 授權流程設計

### authorize-api 更新摘要

- 詳細說明 OAuth 授權 URL 的各項參數（`client_id`、`redirect_uri`、`scope`、`state`）
- 補充完整授權流程步驟
- 說明 `state` 參數的用途與格式（CSRF 防護、資料傳遞、HMAC-SHA256 簽章驗證）
- 重新綁定時，前端需清空已選擇的粉專

### callback-api 更新摘要

**三種回應情境：**

| 情境 | 行為 |
|------|------|
| 用戶取消授權（`access_denied`）| 重導並帶 `oauth=cancelled` |
| 其他錯誤 | 重導並帶 `error={訊息}` |
| 授權成功 | 重導並帶 `success=true` |

**授權成功後的處理步驟：**

1. 驗證 `state` 簽章
2. 交換 User Access Token
3. 取得用戶帳號資訊（ID、姓名、大頭貼）
4. 取得粉專列表
5. 確認是否有可用粉專
6. 儲存綁定資料（含用戶資訊）

**新增儲存的用戶資訊欄位：**

- `UserId`
- `UserName`
- `UserPicUrl`

**其他補充：**
- 新增錯誤處理表格：列出各錯誤情境與對應的 HTTP 狀態碼
- 補充 Facebook Graph API `/me` 端點的 JSON 回傳格式範例

---

## Redirect URL 錯誤（QA HK 環境）

QA HK 環境的 Facebook OAuth Redirect URL 設定有誤，需立即修正。

**當時申請的 Scope：**

```
pages_show_list, pages_messaging, pages_read_engagement,
pages_read_user_content, pages_manage_metadata,
pages_manage_engagement, read_insights, public_profile
```

**問題排查方向：** 確認 Redirect URL 是否與 Facebook Developer Console 中設定的允許清單完全吻合（包含協定、domain、路徑）。

---

## Authorize API 規格

### 端點

- **URL**：`GET /api/facebook/oauth/authorize`

### Query Parameters

| 參數名稱 | 型別 | 必填 | 說明 |
|----------|------|------|------|
| `shopId` | string | 是 | 商店 ID |
| `userId` | string | 否 | 使用者 ID |

### Response

**成功（302 Redirect）：** 重導至 Facebook OAuth 授權頁面，URL 包含以下參數：

| 參數 | 說明 |
|------|------|
| `client_id` | Facebook App ID |
| `redirect_uri` | Callback API 的完整網址 |
| `scope` | 請求的權限範圍（`pages_show_list,pages_read_engagement,pages_manage_metadata`）|
| `state` | 加密狀態參數（包含 shopId、userId，以 HMAC-SHA256 簽章，Base64 編碼）|

### Error Responses

| 狀態碼 | 情境 |
|--------|------|
| 400 Bad Request | 參數錯誤（shopId 為空）|
| 302 Redirect | Session 失效，重導至 `/oauth/facebook/callback?error=unauthorized&shopId={shopId}` |

### 授權流程步驟

1. 用戶點擊「綁定 Facebook 帳號」
2. 系統產生 `state` 參數（包含商店資訊並簽章）
3. 重導至 Facebook 授權頁面
4. 用戶登入 Facebook 並同意授權
5. Facebook 重導回 Callback API，帶上授權碼

### State 參數說明

`state` 用於防止 CSRF 攻擊、傳遞商店資訊，並透過簽章確保完整性。

**格式（Base64 編碼）：**

```json
{
  "ShopId": "shop123",
  "UserId": "user456",
  "Timestamp": 1234567890,
  "Signature": "HMAC-SHA256 簽章"
}
```

**注意事項：**
- State 參數有時效性，建議用戶在 **10 分鐘內**完成授權
- 重新綁定時使用相同流程，前端需清空已選擇的粉絲頁
