# 💳 LoyaltyPoint 維護文件

<br>

## 📖 目錄
  - [🩺 Loyalty 站台 Health Check 方式](#-loyalty-站台-health-check-方式)
<br>

---

## 🩺 Loyalty 站台 Health Check 方式

**loyalty-api**：

<br>

```powershell
Invoke-WebRequest 'http://loyalty-api.internal.hk.91app.io/api/ops/healthcheck' -Proxy 'http://10.32.67.239'
```

<br>

**member**：

<br>

```powershell
Invoke-WebRequest 'http://loyalty-api.internal.hk.91app.io/member/echo' -Proxy 'http://10.32.67.239'
```

<br>

**LoyaltyPointCenterInfo**：

<br>

```powershell
Invoke-WebRequest 'http://loyalty-api.internal.hk.91app.io/loyaltypoint/GetLoyaltyPointCenterInfo/2' -Proxy 'http://10.32.67.239'
```

<br>

---

---
