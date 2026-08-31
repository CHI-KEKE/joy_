




https://91appinc.visualstudio.com/DailyResource/_workitems/edit/648389


payload 查詢

Samuel Luo [UPD] 羅恩力
 
upd-hd-dailyresource
Jul 24th at 11:19 AM
@updhd 團隊監控到優品選取得庫存API有被限速器阻擋
發現優品選於2026/7/24 09:49打了 218次 V1/SalePage/GetStock，
VSTS中提供6次被限速API Payload，再麻煩與商店確認~ 謝謝~
Product Backlog Item 648389: [TW-Prod][Scm-Api-Service] 3 分鐘前 SCM API 出現 Rate Limit (429)


```sql
SELECT *
FROM "tw_prod_osm"."osm_api_nlog"
WHERE date = '2026/07/24'
  AND controller = 'SalePageLite'
  AND action = 'GetStock'
  AND (
       field3 LIKE '%b7b752a1273e8bf59cc49bf35a6c66f5ed1e495e1632884acbaf1547522764c6bb5aa47325c8177b9410cf7110a7e9c5947bc6b13a77ddda762670e461867f04%'
    OR field3 LIKE '%9e1f62a0f27d1000f64916106ac8861b71b01f652258dd244e11bbd62e6f663a5a22772cbcdeef877d78a9c00de07770e642d46ef69c878a2491405083a25681%'
    OR field3 LIKE '%6df915ce5054cd675dabdbad6f5d6ef447822be485deae0a9c843c1f2097f7a500b9bae17fc9300d6781f3060e8a9f5d1ba425f181b2b2d1e229a94c4efbc7a9%'
    OR field3 LIKE '%869668acba19a7fe59e5acfef357c0ba846989ceef53db20839a288ed7f8387954d886e5e7a78a41817aa563b47904ecce849d5a6db3ee83742c196b1e57f6f2%'
    OR field3 LIKE '%0b1b8535605cd1454ca5a0f780d2f93d6389a108be14a00a17fcfe0ece041d511f4350c06f8db0a97866b812afca07c858ca6961d2716ca5ed9c8c128e6cbe71%'
    OR field3 LIKE '%3ea58b51d7ca0ae32e5918134d29644c086fdad274327de8139cab58d2b72cb737bd92ffac900ca1929e322849e2c3876affb9f07c557c802288620d0a409395%'
  );
```


https://91app.slack.com/archives/C3DB30C3T/p1784857893348339

優品選 /scm/V1/SalePage/GetStock 打太快

```sql
SELECT * FROM "tw_prod_osm"."osm_api_iislog"
where date = '2026/07/24'
and time >= '01:20'
and time < '01:59'
and sc_status IN ('429')
limit 1000
```


店家09：49 共打了218次/scm/V1/SalePage/GetStock

```sql
SELECT COUNT(*) FROM "tw_prod_osm"."osm_api_iislog"
where date = '2026/07/24'
and time >= '01:48'
and time < '01:50'
and sc_status IN ('429') and cs_uri_query like '%94197152%'
```



觀察 429


https://91app.slack.com/archives/G06A3GDC7/p1762838710746399


```sql
select *
from "tw_prod_osm"."osm_api_iislog"
where date = '2025/11/11'
and cs_uri_stem like '%GetStock%'
and cs_uri_query like 't=89589328'an
and sc_status = '429'
limit 1;
```

康是美SalePage/GetStock無觀察到429錯誤，Elmah亦無異常

目前已釐清為全店config太早上線
導致康是美10/15起先吃到全店設定(上限200)，10/22上SKA設定(上限600)後無再發生429 Error


https://91app.slack.com/archives/C3DB30C3T/p1767828814959539

觀察到KIPLING有429回應
```sql
SELECT * FROM "tw_prod_osm"."osm_api_iislog"
where date = '2026/01/07'
and time >= '23:20'
and time < '23:33'
and sc_status IN ('429')
limit 1
```
時間撈單KIPLING於2026/1/7 07:30:33打了 206次 V1/SalePage/GetMain




https://91app.slack.com/archives/C3DB30C3T/p1768174414795169

觀察到KIPLING有超過限速上限(200)。
2026/1/12 7:30:48打221次
2026/1/12 7:30:54打206次
2026/1/12 7:31:00打211次
更新於同一筆VSTS中

```sql
SELECT *
FROM "tw_prod_osm"."osm_api_nlog"
WHERE date = '2026/01/07'
  AND controller = 'SalePageLite'
  AND action = 'GetMain'
  AND (
       field3 LIKE '%8d0a1728908461ce72e6ab45a2b3211dfbd1f59c2121835b3d049aa553bcf6da502279c52d74abdf768416b02193f97054b91e6e3eda83e00578c7a92aed2803%'
    OR field3 LIKE '%845d9e6fe90224949ab928309b26fa504c8135a6e954ca0d8a6714d8d87c5a83560980389a1b88f3bc8efa2648180185a16c0aca31a7de59504b13a3e31d2409%'
    OR field3 LIKE '%355a1ae28aca155c531ddffc9ca12157873352cc66356a65d48f4aef0c0308a02c31ffa61581424361d11f3f3dc384a0d6ab9f4e792c8d0f5e1621f5fc37a249%'
    OR field3 LIKE '%2f51387dfbc903af19730a2f89e50eadea18f87e318bbacb1db15b911aa29f15378001f4b9c804249628757b3033b20db511b578ca170e902eac06f1c8aee038%'
    OR field3 LIKE '%476a02a8f4f5379971bb7700cacd0d17c3ba21d3a669e021c9713c3086aa7547bfc4a42126a653407d8a39fbdc5e1a88cc6f8e5a92b74ea2ae72deea676454de%'
    OR field3 LIKE '%9d25e5e84a3b4b4954f53fd43b9280416405f376390edaeb5fe00c81862c8db417ca7c8459f1b376e51af89de3611b8e4baa777e0be7b5ee2024ac702d0496c6%'
  );
```

  
測試api 



https://91app.slack.com/archives/G04TVB3KW/p1758080162195229?thread_ts=1758076322.604679&cid=G04TVB3KW



搜尋限速器完成,接下來看查 rate limit







## 算數量


```sql
SELECT COUNT(*)  FROM "hk_prod_osm"."osm_api_iislog"
WHERE cs_uri_stem = '/scm/V1/SalePage/GetStock'
and date = '2026/08/12'
and time >= '10:20'
and time < '10:30'
--and sc_status  in ('429')
limit 10;
```



查 request count

```sql
SELECT
    date,
    time,
    COUNT(*) AS request_count
FROM "hk_prod_osm"."osm_api_iislog"
WHERE cs_uri_stem = '/scm/V1/SalePage/GetStock'
  AND date > '2026/08/01'
  and date < '2026/08/14'
--  AND time >= '10:20'
--  AND time < '10:30'
GROUP BY date, time
ORDER BY request_count DESC
LIMIT 10;


SELECT
    date,
    time,
    COUNT(*) AS request_count
FROM "hk_prod_osm"."osm_api_iislog"
WHERE cs_uri_stem = '/scm/V1/SalePage/GetMain'
  AND date > '2026/07/01'
  and date < '2026/08/14'
--  AND time >= '10:20'
--  AND time < '10:30'
GROUP BY date, time
ORDER BY request_count DESC
LIMIT 10;




SELECT
    date,
    time,
    COUNT(*) AS request_count
FROM "hk_prod_osm"."osm_api_iislog"
WHERE cs_uri_stem = '/scm/V1/SalePage/GetMain'
  AND date > '2026/05/01'
  and date < '2026/08/14'
--  AND time >= '10:20'
--  AND time < '10:30'
GROUP BY date, time
ORDER BY request_count DESC
LIMIT 10;



SELECT
    date, 
    time,
    regexp_extract(cs_uri_query, '(^|&)t=([^&]+)', 2) AS token,
    COUNT(*) AS request_count
FROM "hk_prod_osm"."osm_api_iislog"
WHERE cs_uri_stem = '/scm/V1/SalePage/GetMain'
  AND date > '2026/05/01'
  AND date < '2026/08/14'
GROUP BY regexp_extract(cs_uri_query, '(^|&)t=([^&]+)', 2),date, time
ORDER BY request_count DESC
limit 10;
```