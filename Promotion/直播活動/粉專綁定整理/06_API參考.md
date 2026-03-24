# 粉專綁定相關 API 參考

## Facebook OAuth 流程 API

| 方法 | 路徑 | 說明 |
|------|------|------|
| GET | `/api/facebook/oauth/authorize?shopId={shopId}` | 產生授權 URL 並導向 Facebook |
| GET | `/api/facebook/oauth/callback?code={code}&state={state}` | Facebook 回調，交換 Token、儲存綁定 |

---

## 粉專綁定管理 API

| 方法 | 路徑 | 說明 |
|------|------|------|
| POST | `/api/shop-page-bindings` | 啟用已授權的粉專（IsEnabled = true）|
| DELETE | `/api/shop-page-bindings/{id}` | 解除綁定（標記 Disconnected）|

---

## 直播場次相關的粉專 API

| 方法 | 路徑 | 說明 |
|------|------|------|
| GET | `/api/live/sessions/{liveSessionId}/shops/{shopId}/facebook-pages` | 取得此場次可用的已綁定粉專清單 |
| GET | `/api/live/sessions/{liveSessionId}/facebook-pages/{pageId}/live-videos` | 透過粉專取得可連結的直播影片清單（LIVE / SCHEDULED）|
| POST | `/api/live/sessions/{liveSessionId}/link-facebook` | 將系統直播場次連結到指定的 FB 直播 |

---

## 後端 Service / Controller 對應

| API | 說明 |
|-----|------|
| `CheckToken` | 確認是否有 Token |
| `OAuthLogin` | 登入 Facebook（後端取得並紀錄 Token）|
| `GetFanClubList` | 取得已綁定、未綁定粉絲團清單（可用 type 區分）|
| `BindingFanClub` | 綁定粉絲團 |
| `UbBindingFanClub` | 解除綁定粉絲團 |
