# PageAccessToken 過期機制

## 基本規則

透過以下 API 取得的 PageAccessToken **本身無過期時間（永久有效）**：

```csharp
var url = $"{this.settings.GraphApiVersion}/me/accounts" +
    "?fields=id,name,access_token,picture.width(80).height(80){url,height,width,is_silhouette}" +
    $"&access_token={userAccessToken}";
```

---

## 會立即失效的情境

即使 Token 本身永久有效，以下情況發生時 Facebook 會**立即**註銷該 Token：

| 情境 | 說明 |
|------|------|
| 授權用戶更改了個人 Facebook 密碼 | 所有該帳號相關 Token 全部失效 |
| 用戶被移除粉絲頁管理權限 | 失去對該粉絲頁的操作授權 |
| 用戶在 Facebook 設定中移除了應用程式授權 | 主動撤銷授權 |
| Facebook 偵測到應用程式有異常資安風險 | 強制重置所有 Token |

---

## Data Access 到期週期

Facebook 的「資料存取權限到期日」指的是 **Data Access** 的週期性到期機制，與 PageAccessToken 是否有效無關：

- 到期週期為 **90 天**，依授權用戶最後一次活躍時間計算
- 若 App 有 90 天未使用某個 Permission，該 Permission 也可能單獨過期
- 只要用戶持續活躍使用，一般不會遇到過期問題

> Token 本身不過期，但授權用戶的 Data Access 權限可能因長期未活躍而失效，兩者需分開理解。
