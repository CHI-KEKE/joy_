# Facebook OAuth 授權流程

## 流程起點

前端頁面：`/stream-settings` (StreamSettingsPage.tsx)

---

## 步驟 1：使用者點擊「新增粉絲專頁」或「帳號登入」

---

## 步驟 2：前端呼叫後端 OAuth Authorize API

```
GET /api/facebook/oauth/authorize?shopId={shopId}
```

---

## 步驟 3：後端產生 State 並導向 Facebook

```csharp
[HttpGet("authorize")]
public IActionResult Authorize([FromQuery] string shopId, [FromQuery] string? userId = null)
{
    //// 1. 驗證 shopId
    if (string.IsNullOrWhiteSpace(shopId))
    {
        throw new ApplicationApiException(...);
    }
    
    //// 2. 產生安全的 State 參數（包含 shopId、userId、timestamp、nonce）
    var state = this.GenerateSecureState(shopId, userId);
    
    //// 3. 組合 Facebook OAuth 授權網址
    var authorizeUrl = QueryHelpers.AddQueryString(
        "https://www.facebook.com/{version}/dialog/oauth",
        new Dictionary<string, string?>
        {
            ["client_id"] = this.facebookAppSettings.AppId,
            ["redirect_uri"] = this.facebookAppSettings.RedirectUri,  // ← 回調網址
            ["state"] = state,                                         // ← 安全參數
            ["scope"] = "pages_manage_posts,pages_read_engagement,..." // ← 權限範圍
        });
    
    //// 4. 302 重導向至 Facebook 授權頁面
    return this.Redirect(authorizeUrl);
}
```

**負責元件**：`FacebookOAuthController`
- 產生 Facebook 授權 URL
- 接收 `/oauth/callback`
- 驗證 CSRF state
- 串起整個授權流程

---

## 步驟 4：使用者在 Facebook 授權頁面操作

瀏覽器跳轉到 Facebook OAuth Dialog：

```
https://www.facebook.com/v24.0/dialog/oauth?
  client_id={AppId}
  &redirect_uri={系統回調網址}
  &state={加密的State}
  &scope=pages_manage_posts,pages_read_engagement,...
```

- Facebook 登入畫面（如果未登入）
- 授權確認頁面：顯示「91APP Livebuy 想要存取您的粉絲專頁」
- 選擇粉專：使用者勾選要授權的粉絲專頁

---

## 步驟 5：使用者同意授權，Facebook 回調系統

Facebook 導回系統指定的 `redirect_uri`：

```
{系統網域}/api/facebook/oauth/callback?
  code={授權碼}
  &state={原本的State}
```

若使用者拒絕授權：

```
{系統網域}/api/facebook/oauth/callback?
  error=access_denied
  &error_description=...
  &state={原本的State}
```

---

## 步驟 6：後端處理 Facebook 回調

```csharp
[HttpGet("callback")]
public async Task<IActionResult> Callback([FromQuery] FacebookOAuthCallbackRequest request)
{
    //// 1. 檢查是否有錯誤（使用者拒絕授權）
    if (string.IsNullOrWhiteSpace(request.Error) == false)
    {
        throw new ApplicationApiException(...);
    }
    
    //// 2. 驗證 State 參數（防止 CSRF 攻擊）
    var stateData = this.ValidateAndExtractStateData(request.State);
    var shopId = stateData.ShopId;
    var userId = stateData.UserId;
    
    //// 3. 用授權碼交換 User Access Token
    FacebookUserTokenResponse userToken = 
        await this.facebookGraphApiService.ExchangeCodeForTokenAsync(request.Code);
    
    //// 4. 用 User Access Token 取得粉專列表及 Page Access Token
    List<FacebookPageAccountResponse> pageAccounts = 
        await this.facebookGraphApiService.GetPageAccountsAsync(userToken.AccessToken);
    
    //// 5. 儲存粉專綁定資料到 DynamoDB
    await this.SaveBindingsAsync(shopId, userId, userToken, pageAccounts);
    
    //// 6. 導回前端回調頁面，顯示成功訊息
    var redirectUrl = "/oauth/facebook/callback?shopId={shopId}&oauth=success";
    return this.Redirect(redirectUrl);
}
```

---

## 步驟 7：儲存粉專綁定到 DynamoDB

```csharp
private async Task SaveBindingsAsync(
    string shopId, string userId,
    FacebookUserTokenResponse? userToken,
    IReadOnlyCollection<FacebookPageAccountResponse>? pageAccounts)
{
    //// 遍歷每個粉專，建立綁定記錄
    foreach (var page in pageAccounts)
    {
        var entity = new ShopPageBinding
        {
            //// Partition Key
            ShopId = shopId,
            
            //// Sort Key
            Platform = "Facebook",
            PlatformPageId = page.PageId,
            
            //// 粉專資訊
            PageName = page.PageName,
            PageAccessToken = page.PageAccessToken,  // ← 重要！Page Access Token
            PicUrl = page.Picture?.Data?.Url,
            
            //// 狀態資訊
            BindingStatus = BindingStatusEnum.Active,
            IsEnabled = false,  // ← 預設為 false，需手動啟用
            WebhookSubscribed = false,
            
            //// 權限資訊
            TokenExpiresAt = tokenExpiresAt,
            UserAccessToken = userToken.AccessToken,
            UserIdList = JsonSerializer.Serialize(new[] { userId }),
            
            //// 時間戳記
            CreatedBy = userId,
            UpdatedBy = userId,
        };
        
        //// 呼叫 ShopPageBindingService 儲存到 DynamoDB
        await this.shopPageBindingService.AddBindingAsync(entity);
    }
}
```

---

## 步驟 8：後端嘗試訂閱 Webhook

```
1. 前端導使用者去 Facebook
2. Facebook 給後端 code
3. 後端換 User Token
4. 拿粉專清單 + Page Token
5. Page Token 存 Secrets Manager
6. 嘗試訂閱 Webhook
7. 建立 DynamoDB 綁定記錄
8. 導回前端
```

> **重要**：Webhook 失敗 ≠ 綁定失敗 → 狀態標記為 `WebhookPending`

---

## 步驟 9：前端接收成功並刷新

- 後端導回：`/oauth/facebook/callback?oauth=success&shopId={shopId}`
- 前端 `FacebookCallbackPage`：顯示「授權成功」、通知主視窗（`postMessage`）、1.5 秒後關閉彈窗
- 主視窗 `StreamSettingsPage`：接收 postMessage、顯示 Toast「帳號登入成功」、刷新粉專列表
