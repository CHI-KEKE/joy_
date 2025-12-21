

## 機器

E:/log/ny-log/Commin/NLog/ScmApiV2/NLog/202505/20250515/EcouponLite/Dispatch/16

- `202505`：年月資料夾（2025年5月）
- `20250515`：具體日期資料夾（2025年5月15日）
- `EcouponLite/Dispatch/16`：功能模組和詳細分類路徑


## Elmah


http://elmahdashboard.91app.hk/Log/Details/38860fdb-750d-44dd-a2b9-bb6f2fc4e1b0

http://elmahdashboard.91app.hk/?Page=1&Pagesize=500&App=NineYi.ScmApiV2&Type=System.Data.SqlClient.SqlException&StartTime=11%2F30%2F2023+10%3A50&EndTime=11%2F30%2F2023+10%3A55

相關 Token：`30671037`



## Athena

https://ap-southeast-1.console.aws.amazon.com/athena/home?region=ap-southeast-1#/query-editor/history/31275af0-b8cb-4f3e-af32-cc57ba3b6176

https://ap-southeast-1.console.aws.amazon.com/athena/home?region=ap-southeast-1#/query-editor/history/4a679630-e999-43b8-a820-b86b8368bce9

#### 特定 API 時間區間查詢

```sql
SELECT *
FROM "tw_prod_osm"."osm_api_nlog"
WHERE DATE BETWEEN '2024/11/11' AND '2024/11/12'
AND longdate >= '2024-11-12 08:00'
AND longdate < '2024-11-12 09:00'
and controller = 'SalesOrder'
and action = 'GetListByCustomerlizedFilter'
```

#### 查詢 token 對應數量

```sql
SELECT
    regexp_extract(cs_uri_query, 't=([^&]+)', 1) AS t_value,
    count(*) AS cnt
FROM "hk_prod_osm"."osm_api_iislog"
WHERE date_parse("date", '%Y/%m/%d') = DATE '2025-11-11'
  AND cs_uri_stem = '/scm/V2/SalePage/UpdateSellingQty'
  AND cs_uri_query LIKE 't=%'
  AND "time" > '05:50:00'
GROUP BY 1
ORDER BY cnt DESC;
```


## Application Insights

https://portal.azure.com/#view/AppInsightsExtension/DetailsV2Blade/ComponentId~/{"SubscriptionId"%3A"a23bf8eb-057f-4ad3-83ad-e1002205aa29"%2C"ResourceGroup"%3A"91APP-HK-OSMAPI"%2C"Name"%3A"Prod-91APP-HK-OSMAPI"%2C"LinkedApplicationType"%3A0%2C"ResourceId"%3A"%252Fsubscriptions%252Fa23bf8eb-057f-4ad3-83ad-e1002205aa29%252FresourceGroups%252F91APP-HK-OSMAPI%252Fproviders%252Fmicrosoft.insights%252Fcomponents%252FProd-91APP-HK-OSMAPI"%2C"ResourceType"%3A"microsoft.insights%252Fcomponents"%2C"IsAzureFirst"%3Afalse}/DataModel~/{"eventId"%3A"34dcb91a-8e92-11ee-8bc4-00224859dfb4"%2C"timestamp"%3A"2023-11-29T08%3A34%3A35.498Z"%2C"cacheId"%3A"1daac255-b44e-464f-b95f-33679e916b7c"%2C"eventTable"%3A"requests"}

#### 大量失敗時，ApplicationInsights 查看失敗詳情

<br>

```
https://portal.azure.com/#@91app.biz/resource/subscriptions/a23bf8eb-057f-4ad3-83ad-e1002205aa29/resourceGroups/91APP-HK-OSMAPI/providers/microsoft.insights/components/Prod-91APP-HK-OSMAPI/failures
```



## CloudWatch


區間流量異常監控


當 API 出現區間流量異常時，可以查閱 CloudWatch 的 SUMREQUEST 指標來分析流量模式：

![alt text](./image-11.png)




## 商店錯誤使用案例

**API 路徑**：`V1/SalePage/SubmitMain`
**發生時間**：02/21 15:18:32.203 ~ now
**商店資訊**：ShopId: 50 (ZAKURA櫻花薈)
**錯誤 Request 範例**：
```json
{
  "salepage": {
    "MirrorCategoryIdList": [],
    "MirrorShopCategoryIdList": [],
    "Title": "Curel珂潤 輕透清爽防曬乳液SPF3030ml",
    "ApplyType": "一般",
    "HasSku": false,
    "SkuList": [],
    "SuggestPrice": 180.0,
    "Price": 180.0,
    "Cost": 1.0,
    "OnceQty": 5,
    "Qty": 0,
    "OuterId": "4901301274335",
    "SafetyStockQty": 3,
    "ShopId": 50,
    "CategoryId": 0,
    "ShopCategoryId": 0,
    "Status": "Hidden"
  }
}
```

```
ShopCategoryId => 'Shop Category Id' should not be empty.
ShopCategoryId, MirrorShopCategoryIdList => 商店類目或分身類目不存在
```

請按照 API 文件，將 Request 中的 `"MirrorCategoryIdList":[]` 改為 `"MirrorCategoryIdList":null`