# Facebook OAuth 與權限問題整理

## 問題核心

OAuth 權限問題的核心是：**粉絲頁三個必要權限（`CREATE_CONTENT` / `MODERATE` / `MESSAGING`）在授權後沒有主動檢查與告知**，導致用戶在直播時才發現功能異常。目前正在推進前後端補齊這段驗證邏輯。

---

## 文件索引

| 檔案 | 說明 |
|------|------|
| [01_oauth授權流程.md](./01_oauth授權流程.md) | OAuth 授權流程設計、Redirect URL 錯誤處理、Authorize API 完整規格 |
| [02_sso與角色權限.md](./02_sso與角色權限.md) | SSO 驗證 API 規格、角色功能權限 403 問題 |
| [03_token過期機制.md](./03_token過期機制.md) | PageAccessToken 過期規則、立即失效情境、Data Access 90 天週期 |
| [04_粉絲頁權限.md](./04_粉絲頁權限.md) | 三個必要權限說明、根本原因分析、前後端改善建議 |
