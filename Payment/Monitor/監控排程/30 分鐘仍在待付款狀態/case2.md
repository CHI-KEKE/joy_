

## Case2

```sql
select * from "hk_prod_webstore"."webstore_web_nlog"
where date = '2025/11/03'
and controller = 'PayChannel'
and action = 'PayChannelReturn'
and message like '%TG251103B00024%'
--and requestid = '{"message":"202508112041040548'
limit 100;
```

沒東西~ 已確認語法正確


```sql
select * from "hk_prod_webstore"."webstore_web_nlog"
where date = '2025/11/03'
--and controller = 'tradesOrderLite'
--and action = 'CompleteForNewCart'
--and message = '%TG251103B00024%'
limit 100;
```

沒東西~ 已確認語法正確

幹
```sql
select * from "hk_prod_webstore"."webstore_web_nlog"
where date = '2025/11/02'
and controller = 'tradesOrderLite'
and action = 'CompleteForNewCartV2'
--and message LIKE '%TG251103B00024%'
and requestid like '%{"message":"202511030132390014%'
limit 100;
```

正常完成 wtp


確認 paychannelreturn
```sql
SELECT * FROM "hk_prod_webstore"."webstore_web_nlog"
where controller = 'PayChannel'
and action = 'PayChannelReturn'
and date = '2025/11/02'
and message like '%TG251103B00024%'
limit 6000;
```

==> 沒東西

internal finish
```sql
SELECT * FROM "hk_prod_webstore"."webstore_web_nlog"
where controller = 'PayChannel'
and action = 'InternalFinishPayment'
and date = '2025/11/02'
and message like '%TG251103B00024%'
limit 6000;
```