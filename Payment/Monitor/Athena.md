## Tips

- date 是 UTC 時間要比實際要早一點
- 要一個個測試是否語法有差異, EX : LIKE, Action 切換,requestid vs message

## CompleteForNewCart

```sql
SELECT * FROM "hk_prod_webstore"."webstore_web_nlog"
where 1 =1
--and message like '%TG250702T00022%'
and requestid = '{"message":"202505302224105708'
and date = '2025/05/30'
limit 1000;
```
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

<br>

## PayChannelReturn

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

<br>

## InternalFinishPayment

```sql
SELECT * FROM "hk_prod_webstore"."webstore_web_nlog"
where controller = 'PayChannel'
and action = 'InternalFinishPayment'
and date = '2025/02/01'
limit 6000;
```

## RequestPayProcessUrlV3

```sql
SELECT * FROM "my_prod_webstore"."webstore_web_nlog"
where date ='2026/01/12'
and controller = 'PayLite'
and action = 'RequestPayProcessUrlV3'
--and message like'%shopId=200093%'
--and requestid = '{"message":"202601131028431336'
--and level = 'Error'
```