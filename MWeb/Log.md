## 機器 Log

`E:\log\ny-log\Common\NLog\WebApi`
`D:\QA\NineYi\WebAPI.WebStore`

## ATHENA


#### tradesOrderLite / CompleteForNewCart
```sql
select * from "hk_prod_webstore"."webstore_web_nlog"
where date = '2024/12/23'
and controller = 'tradesOrderLite'
and action = 'CompleteForNewCart'
and requestid = '{"message":"202412230856540938'
limit 100;
```

#### PayChannel / InternalFinishPayment

```sql
SELECT * FROM "hk_prod_webstore"."webstore_web_nlog"
where controller = 'PayChannel'
and action = 'InternalFinishPayment'
and date = '2025/02/01'
limit 6000;
```


#### Scan / GetChannelSkuInfo
```sql
--SELECT * FROM "hk_prod_webstore"."webstore_web_nlog" limit 10;
--PM3284010007

--select * from "hk_prod_webstore"."webstore_web_nlog"
--where date = '2025/11/11'
--and message like'%PM3284010007%'
--limit 100;

--40 : {"message":"202511111840409830
--10 : {"message":"202511111810297541

select * from "hk_prod_webstore"."webstore_web_nlog"
where date = '2025/11/11'
and controller = 'Scan'
and action = 'GetChannelSkuInfo'
and requestid = '{"message":"202511111840409830'
--and message like'%PM3284010007%'
limit 100;
```


## IIS LOG

####　查看 NLog 設定檔的位置

IIS --> MQweb2 --> QA.MobileWebMall --> WebAPI --> Explore --> 可以找到 Nlog.config


#### 位置

C:\inetpub\logs\LogFiles\W3SVC4
E:\log\ny-log\Common\IISLog\W3SVC1


#### 工具

https://www.finalanalytics.com/




