## 🔐 CI/CD NuGet 設定

若站台本身跑 CI/CD 時沒有 NuGet 的帳號密碼導致 Build 不過
需要在相關程式設定檔中加入 NuGet 認證資訊

**設定檔案**：
```
nine1.utility.auditcollection.gitlab-ci.yml
```

**環境變數設定**：

```yaml
variables:
    FEED_ENDPOINTS: '{"endpointCredentials": [{"endpoint":"$NINEYI_NUGET_GROUP", "username":"$NYP_AM_USER", "password":"$NYP_AM_PASSWORD"},{"endpoint":"$NINEYI_NUGET_RELEASE", "username":"$NYP_AM_USER", "password":"$NYP_AM_PASSWORD"},{"endpoint":"$NINEYI_NUGET_DEVELOP", "username":"$NYP_AM_USER", "password":"$NYP_AM_PASSWORD"}]}'
```

- `$NINEYI_NUGET_GROUP`：NuGet Group 端點
- `$NINEYI_NUGET_RELEASE`：NuGet Release 端點  
- `$NINEYI_NUGET_DEVELOP`：NuGet Develop 端點
- `$NYP_AM_USER`：NuGet 使用者名稱變數
- `$NYP_AM_PASSWORD`：NuGet 密碼變數