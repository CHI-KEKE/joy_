# Config 技術參考

## 機器路徑

| 項目 | 路徑 |
|------|------|
| AppSettings.config | `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Config\AppSettings.config` |
| Config 目錄 | `C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Config` |


myqa worker config : C:\worker


<br>

---

## Config 對應機器的方法

各機器與 Config 的對應關係定義在：

https://bitbucket.org/nineyi/nineyi.configuration/src/master/serverMaps.json

<br>

---

## DB 定義的 Config

從資料庫讀取 Config 值：

```sql
csp_GetConfigDBAppSettingValue
```
