


## 系統設置偏好語系服務的方式


C:\91APP\NineYi.Sms\WebSite\WebSite\Global.asax.cs

```csharp
Application_Start()
GlobalConfiguration.Configure(WebApiConfig.Register);


//// WebApi 取得使用者偏好語系
var apiGetUserPreferLangAttribute = System.Web.Mvc.DependencyResolver.Current.GetService(typeof(GetUserPreferLangApiAttribute));
config.Filters.Add(apiGetUserPreferLangAttribute as IFilter);


GetUserPreferLangApiAttribute


preferLang = this._userService.GetUserPreferLanguage(userEntity.SupplierId, userEntity.UsersId);

//// DDB 取得使用者資訊
var response = this.GetDDBAuthExternalUserInfo(supplierId, userId);

//// 取得使用者語系偏好設定
string lang = string.Empty;
if (response.Item.Any() &&
    string.IsNullOrEmpty(response.Item["UserPreferLanguage"].S) == false)
{
    lang = response.Item["UserPreferLanguage"].S;
}
else
{
    //// 若使用者未設定語系偏好，則使用市場預設語系
    lang = this._configService.GetAppSetting("Translation.DefaultLocale");
}
```





## 瀏覽器語系切換 - ddb


https://sms.qa1.hk.91dev.tw/Api/User/GetUserInfo?ShopId=2&UserId=91273


```csharp
private GetItemResponse GetDDBAuthExternalUserInfo(long supplierId, long userId)
{
    string partitionKey = $"{supplierId}_{userId}";
    var requestItem = new GetItemRequest
    {
        TableName = this._userDynamoDBTableName,
        Key = new Dictionary<string, AttributeValue>()
        {
            { "SupplierId_UserId", new AttributeValue { S = partitionKey } }
        }
    };

    return this._dynamoDBService.GetItem(requestItem);
}
```


## 經過 translation client 拿語系結果的方法


```csharp
public string GetLocale()
{
    return CultureInfo.CurrentUICulture.Name;
}
```



## request start 動作

```csharp
/// <summary>
/// Application_BeginRequest
/// </summary>
/// <param name="sender">object</param>
/// <param name="e">EventArgs</param>
protected void Application_BeginRequest(object sender, EventArgs e)
{
    //// Nlog Key
    HttpContext.Current.Items["NLogRequestId"] = string.Format("{0:yyyyMMddHHmmssffff}", DateTime.Now);
    HttpContext.Current.Items["RequestId"] = Guid.NewGuid();

    //// 根據 Locales 設定 CurrentUICulture
    this.LocaleFilter();
}
```