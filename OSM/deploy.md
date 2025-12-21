## Health Check

```powershell
# HK OSM
Invoke-WebRequest 'http://store.91app.hk/ops/healthcheck' -Proxy 'http://10.32.20.53'

# MY OSM Web
Invoke-WebRequest 'http://osm2.91app.com.my/api/health/check' -Proxy 'http://10.1.20.63'


# Auth SSO
Invoke-WebRequest -Uri "http://auth.91app.hk/api/ops/healthcheck" -Proxy 'http://10.32.21.221'
invoke-webrequest -Uri 'http://erp.hk.91app.biz/v2/api/health/check' -Proxy 'http://10.32.21.211/v2/api/health/check'
Test-Connection -ComputerName "SG-HK-SSO3" -Count "1" -Quiet


# ERP
# MY ERP
Invoke-WebRequest -Uri "http://erp2.my.91app.biz/v2/api/Ops/healthcheck" -Proxy "http://10.1.21.105"

# HK ERP - 方式 1
Invoke-WebRequest -Uri "http://erp.hk.91app.biz/Health/Check" -Proxy "http://10.2.18.57"

# HK ERP - 方式 1.2
Invoke-WebRequest -Uri "http://erp.hk.91app.biz/ops/healthcheck" -Proxy "http://10.32.21.211"

# HK ERP - 方式 2
Invoke-WebRequest -Uri 'http://erp.hk.91app.biz/v2/api/health/check' -Proxy 'http://10.32.21.211/v2/api/health/check'

# HK ERP - 方式 1.2 (V2 版本)
Invoke-WebRequest -Uri 'http://erp.hk.91app.biz/v2/api/ops/healthcheck' -Proxy 'http://10.32.21.211/v2/api/ops/healthcheck'


# Expense
invoke-webrequest -Uri http://erp.hk.91app.biz/v2/api/health/check -Proxy http://10.32.25.215/v2/api/health/check
```


## MachineConfig 咬檔問題
   
錯誤訊息：`the process cannot access the file because it is used by another process`


## DbContext 新增後 CrmdbUser 權限問題

**發生時間**：2025-01-23 09:57:24 AM
**問題描述**：有開發者新增了 DbContext，上線時發現 CrmdbUser 沒有權限
**錯誤訊息**：
```
Login failed for user 'crmdbardbuser'
異常類型: System.Data.SqlClient.SqlException
錯誤代碼: 18456 (SQL Server 登入失敗)
```
**影響的相關資訊**：
- Prod.CRMAR
- Prod.CRMARReadOnly
- 相關伺服器：SG-MY-DBLstn1.sg.91app.corp
- 影響系統：NineYi SCM API V2
**討論串**：https://91app.slack.com/archives/G04TVB3KW/p1737598056960449
**當下確認事項**：
- 確認目前線上機器無影響，新程式碼未在線上
- 確認目前各站台 release 步驟進度
- 確認各站台 login user 權限問題排除
- 統控確認上述無誤
**原因釐清**：
- HK / MY 的密碼帶反了
- 目前是請 DBA 直接改成反的
- 因為 REPO 是 CRMDBAR，可能很多 Console 都在用，改不完，維持現狀重新 release
**修正驗證**：
修正後重打 Health Check 成功
```powershell
Invoke-WebRequest 'http://shop2.91app.hk/webapi/ops/HealthCheck' -Proxy 'http://10.32.15.52'
```