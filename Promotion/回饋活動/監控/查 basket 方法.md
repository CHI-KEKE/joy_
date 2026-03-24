


## 關鍵字


38227_TG260316Z00104

```bash
{service="prod-promotion-service"} 
|json
|= `38227_TG260316Z00104`
|json
| line_format "{{._msg}}"
```

用 table 看找 taskid


## task id

```bash
{service="prod-promotion-service"} 
|json
| _props_TaskId = `b96e6dcc-c773-4fb3-b6f4-27d7384744c2`
|json
| line_format "{{._msg}}"
```


## 搜尋 basket

Request content: 

Response content:


## ai 拆解

格式轉換專家