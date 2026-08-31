```sql
SELECT * FROM "my_prod_webstore"."webstore_web_nlog"
where date ='2026/01/12'
and controller = 'PayLite'
and action = 'RequestPayProcessUrlV3'
--and message like'%shopId=200093%'
--and requestid = '{"message":"202601131028431336'
--and level = 'Error'
```