
```sql
select * from "hk_prod_webstore"."webstore_web_nlog"
where date = '2025/06/19'
and controller = 'PayChannel'
and action = 'PayChannelReturn'
and message like '% PayChannel: Cybersource, PayMethod: CreditCardOnce%'
--and requestid = '{"message":"202412211334273896'
limit 100;

select * from "hk_prod_webstore"."webstore_web_nlog"
where date = '2025/08/11'
and controller = 'PayChannel'
and action = 'PayChannelReturn'
--and message like '% PayChannel: PayMe, PayMethod: EWallet, TGCode: TG250811W00083%'
and requestid = '{"message":"202508112041040548'
limit 100;
```
