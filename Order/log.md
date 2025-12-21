
## Athena


#### Athena SalesOrder / Get
```sql
SELECT * FROM "hk_prod_osm"."osm_api_nlog" 
WHERE controller = 'SalesOrder'
and date = '2023/04/14'
and action = 'Get'
and field4 like '%TG230414R00028%'
--and requestid = '{"message":"202209262311209082'
```