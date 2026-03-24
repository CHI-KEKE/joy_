


## task data



{"Data":"{\"ShopId\":2,\"LiveSessionId\":\"LS260312L00002\",\"ReferenceTime\":\"2026-03-12T00:00:00Z\"}"}


## api

"/api/live/comments/sync/LS260323L00001?shopId=200017&referenceTime=2026-03-23T08%3A55%3A26Z\



## 查 Webapi log

```bash
{container="nine1-livebuy-web-api",service="prod-91app-live"}
|json
|= `live/comments/sync`
|json
| line_format "{{._msg}}"
# |_props_RequestId = `0HNIHFFFJTCCQ:00000012`
```