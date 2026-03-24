# SSO 驗證與角色功能權限

## SSO 驗證 API 規格

| 端點 | 說明 |
|------|------|
| `POST /api/auth/callback` | 交換授權碼 → Access Token → 建立 Redis Session |
| `GET /api/auth/check` | 驗證 shopId / path 的角色功能權限 |
| `POST /api/auth/logout` | 清除 Redis Session 與 Cookie |

**Cookie 安全設定：**
- HTTP-only、Secure、SameSite
- Token 儲存於 Redis，前端不直接處理 Token

---

## 角色功能權限 403 問題

### 問題描述

在系統站立同步後，存取特定頁面出現 `403 NoPermission` 錯誤。

### 確認方向

相關 API 文件位於 `RolePermissions/fetch-role-permissions-api.md`。
確認 PR Merged 後，需補齊對應的情境測試，確保角色與路由的權限判斷涵蓋所有使用場景。
