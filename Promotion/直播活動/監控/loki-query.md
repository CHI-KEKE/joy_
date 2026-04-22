
## RequestPath

/api/live/sessions/active

/api/webhooks/facebook





## key world

```bash
{service="hk-qa-91app-live"}
|json
|= `XDX7lfYESQ0CHew=`
|json
| line_format "{{._msg}}"
# |_props_RequestId = `0HNIHFFFJTCCQ:00000012`
```


## 算數量分布


```bash
{service="prod-91app-live"}
|json
|=`Started new worker process,`
|_props_JobName =`SyncLiveCommentStatus`
|json
| line_format "{{._msg}}"
```


## websocket CollectionId

```bash
{service="hk-qa-91app-live"}
|json
|= `ZvjyHdMGSQ0CHgA=`
|json
| line_format "{{._msg}}"
# |_props_RequestId = `0HNIHFFFJTCCQ:00000012`
```

## websocket 關鍵字

```bash
## 發送訊息到 WebSocket 發生錯誤
## 處理 WebSocket 連線中斷失敗
{service="hk-qa-91app-live"}
|json
|= `完成發送訊息到 ConnectionId`
|json
| line_format "{{._msg}}"
# |_props_RequestId = `0HNIHFFFJTCCQ:00000012`
```


## websocket requestid

```bash
{service="hk-qa-91app-live"} | json | container = "nine1-livebuy-web-api"
|~ `.*.*` 
|~ "0HNJQIKGKLROT:0000001D"
| Job =~ `.*.*`
| _msg =~ `.*.*`
| line_format "{{._msg}}"
```

## 查 Webapi log

```bash
{container="nine1-livebuy-web-api",service="prod-91app-live"}
|json
|= `live/comments/sync`
|json
| line_format "{{._msg}}"
# |_props_RequestId = `0HNIHFFFJTCCQ:00000012`


{container="nine1-livebuy-web-api",service="prod-91app-live"}
|json
# |= `找不到相符的用戶`
|_props_RequestId=`0HNKEJ8Q3HNNE:00000006`
|json
| line_format "{{._msg}}"
# |_props_RequestId = `0HNIHFFFJTCCQ:00000012`
```

## by jobName

```bash
{service="prod-91app-live"}
|json
|=`91APP`
# |_props_JobName =`SyncLiveCommentStatus`
|_props_TaskId=`9d3c432b-39cc-4b69-bbac-52de555fb09b`
|json
| line_format "{{._msg}}"


{service="prod-91app-live"}
|json
|=`OrderCreate`
|=`::Received data`
|=`HttpReferer`
|_props_JobName =`SyncLiveCommentStatus`
|json
| line_format "{{._msg}}"
```


## 僅呈現特定格式

```bash
{service="prod-91app-live"}
|json
|=`OrderCreate`
|=`::Received data`
|_props_JobName =`SyncLiveCommentStatus`
|json
| line_format "{{._msg}}"
| regexp `\\"HttpReferer\\":\\"?(?P<HttpReferer>[^\\",}]*)`
| line_format "HttpReferer: {{.HttpReferer}}"


{service="prod-91app-live"}
|json
|=`OrderCreate`
|=`::Received data`
|_props_JobName =`SyncLiveCommentStatus`
|json
| line_format "{{._msg}}"
| regexp `\\"HttpReferer\\":\\"?(?P<HttpReferer>[^\\",}]*)`
| line_format "HttpReferer: {{.HttpReferer}}"
|=`91APP`
```


## live status sync


```bash
{container="nine1-livebuy-web-api",service="prod-91app-live"}
|json
|= `LS260407L00003/sync-live-status`
|json
| line_format "{{._msg}}"
# |_props_RequestId = `0HNIHFFFJTCCQ:00000012`



{container="nine1-livebuy-web-api",service="prod-91app-live"}
|json
|= `LS260408L00003/sync-live-status`
|=`ViewerCount 更新。LiveSessionId: \"LS260408L00003\"`
|json
| line_format "{{._msg}}"
# |_props_RequestId = `0HNIHFFFJTCCQ:00000012`
```