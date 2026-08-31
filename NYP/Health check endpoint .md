
專案要配置 Health check endpoint 否則在 rancher 的 recent event 會看到：

```
Readiness probe failed: Get "http://10.50.231.179:50350/_hc": dial tcp 10.50.231.179:50350: connect: connection refused
```