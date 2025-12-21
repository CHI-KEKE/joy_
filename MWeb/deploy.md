
## CDN 確認語法

```sql
SELECT ShopStaticSetting_Value FROM WebStoreDB.dbo.ShopStaticSetting (NOLOCK)
WHERE 1 = 1
AND ShopStaticSetting_ShopId = 0
AND ShopStaticSetting_ValidFlag = 1
AND ShopStaticSetting_GroupName = 'CssJsCdn'
AND ShopStaticSetting_Key = 'EnabledStatus'
```

## ⚠️ 部署異常紀錄

#### Step 1 - Prepare first machine 錯誤

錯誤描述：
```
An error occurred when executing task 'Step 1 - Prepare first machine'
目前流量是80 20 不能重跑!!
目前HK 前台Release 第一步發生異常，疑似是ASG 啟動有問題，但流量部分已經切換
可以協助幫忙回復配置，我們這邊可能要重新執行第一步
```

詳細錯誤訊息
```
Step 1 - Prepare first machine
powershell.exe : Cake.exe : An error occurred when executing task 'Step 1 - Prepare first machine'.
At D:\ws\workspace\.webstore.mobilewebmall_master_4@tmp\durable-9d460c1d\powershellWrapper.ps1:3 char:1
+ & powershell -NoProfile -NonInteractive -ExecutionPolicy Bypass -Comm ...
+ ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    + CategoryInfo          : NotSpecified: (Cake.exe : An e...first machine'.:String) [], RemoteException
    + FullyQualifiedErrorId : NativeCommandError
```

<br>

####　AWS ASG Launch Template 過多

MWeb 第二步，Launch Template 過多未刪，停在 Sync First Branch, 請 INFRA 恢復流量並移除舊 template

<br>
<br>

## Health Check API

```powershell
(1) Invoke-WebRequest 'http://shop2.91app.hk/v2/ops/HealthCheck' -Proxy 'http://10.32.10.52'
(2) Invoke-WebRequest 'http://shop2.91app.hk/ops/HealthCheck' -Proxy 'http://10.32.10.52'
(3) Invoke-WebRequest 'http://shop2.91app.hk/webapi/ops/HealthCheck' -Proxy 'http://10.32.10.52'
(4) Invoke-WebRequest 'http://shop2.91app.hk/admin/check' -Proxy 'http://10.32.10.52'
(5) Invoke-WebRequest 'http://shop2.91app.hk/webapi/auth/islogin' -Proxy 'http://10.32.10.52'
(6) Invoke-WebRequest 'http://shop2.91app.hk/v2/Official/BrandStory' -Proxy 'http://10.32.10.52'

## mweb21
Invoke-WebRequest 'http://shop2.9lapp.hk/webapi/ops/HealthCheck' -Proxy 'http://10.32.15.52'

## mweb22
Invoke-WebRequest 'http://shop2.9lapp.hk/webapi/ops/HealthCheck' -Proxy 'http://10.32.16.52'
```