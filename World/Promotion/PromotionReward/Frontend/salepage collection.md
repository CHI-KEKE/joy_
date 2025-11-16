
## 計算過程發生錯誤 - salepage collection

### 錯誤日誌範例

<br>

```
2025-09-26T03:23:42.4021843Z [Information] Start processing HTTP request "POST" https://promotion-api-frontend-internal.qa1.hk.91dev.tw/api/basket-calculate

2025-09-26T03:23:42.8605746Z [Error] 回收活動回饋優惠券 9230_CrmSalesOrder:330481 發生錯誤System.Net.Http.HttpRequestException: Response status code does not indicate success: 500 (Internal Server Error).

2025-09-26T03:23:42.8602800Z [Information] Response content: {"errorCode":"SalepageCollectionException","message":"HttpRequestException","data":"Response status code does not indicate success: 400 (Bad Request)."}
```

<br>

### 問題排查步驟

<br>

- 到 shoppingcart-loki 搜尋 salepage-service
- 打 POST {{host}}api/salepage-collections:match 測試

<br>

### 相關 API 文件

<br>

[Tag API Swagger](https://tag-api.qa1.my.91dev.tw/swagger/index.html)
[Salepage Collection SWAGGER](https://salepage-service-api-internal.qa1.hk.91dev.tw/swagger/index.html#/)
