



## ⏰ 查詢耗時方法

### 1.1 語法

**基本查詢語法**：

```
{service="prod-payment-middleware"}
| json
|="Root=1-68f0b7b7-1cf9e63362ee8de07ac67bc3"
#|="End processing HTTP request"
#| json
#| line_format "{{._msg}}"
```

<br>

**介面操作參考**
_props_ElapsedMilliseconds
_props_RequestPath

![alt text](./image.png)





## 9. Dashboard

https://monitoring-dashboard.91app.io/d/Wc3ALtv4k/payment-middleware-monitor?orgId=2&refresh=1m

<br>

![alt text](./Img/image-9.png)

<br>