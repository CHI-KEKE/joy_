
## 機器

D:\Files\Log\SMS\NLog\202601\20260120\Shop
`D:\Files\Log\SMS\NLog\202512\20251202\GlobalShipping`
`E:\log\ny-log\Common\NLog\SMS`

## Athena LOG

#### 取得商品選項

```sql
SELECT * 
FROM "tw_prod_osm"."osm_web_nlog" 
WHERE date = '2025/01/16'
    and controller = 'SalePage'
    and action = 'GetSalePageSku'
    and message like '%31502348%'
    -- and requestid like '%202501161528480463%'
limit 100;
```

#### 上傳圖片

```sql
SELECT * 
FROM "tw_prod_osm"."osm_web_nlog" 
WHERE date = '2025/01/16'
    and controller = 'Image'
    -- and action = 'UploadImage'
    -- and message like '%31502348%'
    -- and requestid like '%202501161528480463%'
limit 100;
```

#### 更新商品選項

```sql
SELECT * 
FROM "tw_prod_osm"."osm_web_nlog" 
WHERE date = '2025/01/16'
    -- and longdate >= '2025-01-16 15:24:12.9832'
    -- and controller = 'SalePage'
    -- and action = 'UpdateSalePageInfoWithSku'
    -- and message like '%10454276%'
    and requestid like '%202501161525254407%'
limit 100;
```