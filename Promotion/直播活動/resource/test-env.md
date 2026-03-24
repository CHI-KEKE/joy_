
## Facebook 開發者設定

> 用公司 mail 辦 Facebook 帳號後可以拿到 profileId。

- 開發者後台：https://developers.facebook.com/?no_redirect=1
- Token 驗證 / API 測試工具：https://developers.facebook.com/tools/explorer/

### QA 測試帳號

| 項目 | 值 |
|------|-----|
| 帳號 | kevindeng@91app.com |
| 密碼 | Kd123456 |
| Facebook 測試用帳號 | [Mak-Darren](https://www.facebook.com/people/Mak-Darren/pfbid0HzDjYh5UtYGvzvFP3FPb4WDHcjrufqcPQ6bCx7JPVyQknd5RN8vEWzrfsidZbVoCl/)（需加好友）|

### QA App 憑證

| 項目 | 值 |
|------|-----|
| App Id | 25105850345737877 |
| Secret | 7f4e00100a9c7b26c487930e78644e7f |

---

## 系統 API 清單（Facebook 相關）

| API | 說明 |
|-----|------|
| `CheckToken` | 確認是否有 Token |
| `OAuthLogin` | 登入 Facebook（後端取得並紀錄 Token）|
| `GetFanClubList` | 取得已綁定、未綁定粉絲團清單（可用 type 區分）|
| `BindingFanClub` | 綁定粉絲團 |
| `UbBindingFanClub` | 解除綁定粉絲團 |