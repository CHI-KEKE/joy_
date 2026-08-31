
```sql
SELECT * FROM "hk_prod_webstore"."webstore_web_nlog"
where controller = 'PayChannel'
and action = 'InternalFinishPayment'
and date = '2025/02/01'
limit 6000;
```