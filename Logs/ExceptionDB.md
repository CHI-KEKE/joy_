

## Slack

https://91app.slack.com/archives/G06A3GDC7/p1767593383010459

## 異常訪問 IP 

84.17.37.77
202.165.70.78
202.165.70.44
42.200.139.90
156.146.45.14

## 錯誤

前台 Web API & 後台 OsmWeb， 引發 404 錯誤


## 前台寫 elmah 過濾規則

- 前台 elmah 規則可以在各專案的 Web.config, 這裡會記下某些路徑發生 404 的時候要記錄 log 目前包含 /Tmp, /Scripts, /img, /Content, /V2

- /V2 會針對常被攻擊的排除掉一些常常被攻擊的路徑

- /WebAPI 發生 404 錯誤，僅排除 `favicon.ico` 路徑

![404_log_rule](https://raw.githubusercontent.com/CHI-KEKE/pics/refs/heads/main/Log/404rule_1.png)


![404_log_rule2](https://raw.githubusercontent.com/CHI-KEKE/pics/refs/heads/main/Log/404rule_2.png)

