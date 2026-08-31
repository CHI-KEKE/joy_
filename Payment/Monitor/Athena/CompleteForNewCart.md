
## CompleteForNewCart

```sql
SELECT * FROM "hk_prod_webstore"."webstore_web_nlog"
where 1 =1
--and message like '%TG250702T00022%'
and requestid = '{"message":"202505302224105708'
and date = '2025/05/30'
limit 1000;
```



## CompleteForNewCartV2

```sql
select * from "hk_prod_webstore"."webstore_web_nlog"
where date = '2024/12/21'
and controller = 'tradesOrderLite'
and action = 'CompleteForNewCart'
and requestid = '{"message":"202412211334273896'
limit 100;

select * from "hk_prod_webstore"."webstore_web_nlog"
where date = '2025/11/02'
and controller = 'tradesOrderLite'
and action = 'CompleteForNewCartV2'
--and message LIKE '%TG251103B00024%'
and requestid like '%{"message":"202511030132390014%'
limit 100;
```