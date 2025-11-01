# Athena 文件

## 目錄
1. [webapi 結帳](#1-webapi-結帳)
2. [NMQ Event](#2-nmq-event)
3. [NMQ Tasks](#3-nmq-tasks)
4. [PayChannelReturn](#4-paychannelreturn)

<br>

---

## 1. webapi 結帳

```sql
SELECT * FROM "hk_prod_webstore"."webstore_web_nlog"
where 1 =1
--and message like '%TG250702T00022%'
and requestid = '{"message":"202505302224105708'
and date = '2025/05/30'
limit 1000;
```

<br>

```sql
select * from "hk_prod_webstore"."webstore_web_nlog"
where date = '2024/12/21'
and controller = 'tradesOrderLite'
and action = 'CompleteForNewCart'
and requestid = '{"message":"202412211334273896'
limit 100;
```

<br>

---

## 2. NMQ Event

```sql
SELECT *
FROM "hk_prod_nmqv3"."archive_event"
WHERE event_name = 'OrderCreated'
AND date = '2025/06/15'
AND event LIKE '%TG250616A00008%'
LIMIT 10;
```

<br>

---

## 3. NMQ Tasks

```sql
select * from "nmqv3_hk"."archive_task" limit 10;
```

<br>

---

## 4. PayChannelReturn

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