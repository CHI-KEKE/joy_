
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
```