-- MG260617L00008

  SELECT * FROM regular_order
 WHERE rgo_pk  = 262
 
 
  SELECT COUNT(*) as affected_rows 
 FROM regular_order 
 WHERE rgo_product_style_id IS NULL 
    OR rgo_product_style_id = '' 
    OR rgo_product_style_id = '0';
  
  
 DELETE FROM regular_order 
 WHERE rgo_product_style_id IS NULL 
    OR rgo_product_style_id = '' 
    OR rgo_product_style_id = '0';
 
 SELECT * FROM regular_order_detail
 -- where rod_create_time > '2026-06-18'
 WHERE rod_tg_code  = 'MG260618S00001'
-- WHERE rod_rgo_pk  = 263;
 
    UPDATE regular_order_detail
 SET 
     rod_rgo_pk = 262
 WHERE rod_tg_code  = 'MG260618K00002'
 
 
  SELECT * FROM regular_order
 WHERE rgo_source_tg = 'MG260618P00003'
 
 select * from regular_order
 where rgo_create_time > '2026-06-18'
 

  
UPDATE regular_order
SET rgo_next_ship_date = DATE_ADD(CURDATE(), INTERVAL 2 DAY),
    rgo_ship_period = 1,
    rgo_update_user = 'TEST',
    rgo_update_time = NOW()
WHERE rgo_pk = 291;
 
 
  SELECT * FROM regular_order_detail
  WHERE rod_rgo_pk  = 291;
  
  
   UPDATE regular_order_detail
 SET 
     rod_create_order_status = 2,
     rod_create_order_date = DATE_SUB(CURDATE(), INTERVAL 1 DAY),
     rod_update_user = 'TEST',
     rod_update_time = NOW(),
     rod_ship_date = DATE_SUB(CURDATE(), INTERVAL 1 DAY)
 WHERE rod_pk = 4992;
   
   
   UPDATE regular_order_detail
 SET 
     rod_create_order_date = '2026-06-18',
     rod_update_user = 'TEST',
     rod_update_time = NOW()
where rod_create_time > '2026-06-18'
   
 
 UPDATE regular_order
 SET 
     rgo_next_ship_date = DATE_SUB(CURDATE(), INTERVAL 1 DAY),
     rgo_update_user = 'TEST',
     rgo_update_time = NOW(),
     rgo_create_order_status = 2
 WHERE rgo_pk = 256;
  
  
  
  
 
   SELECT * FROM regular_order
 WHERE rgo_source_tg = 'MG260617S00001'
 
 
 
 

 UPDATE regular_order
 SET rgo_next_ship_date = ADDDATE(CURDATE(), 2),
     rgo_ship_period = 1,
     rgo_update_user = 'TEST',
     rgo_update_time = NOW()
 WHERE rgo_pk = 256;
 
   SELECT * FROM regular_order
 WHERE rgo_source_tg = 'MG260617M00001'
 
 
  SELECT *
 FROM regular_order
 WHERE rgo_create_time >= '2026-06-17' 
   AND rgo_create_time < '2026-06-18'
 ORDER BY rgo_create_time DESC;
 
 
SELECT 
     *
 FROM shop_info
 WHERE spi_shopid  = 4
 
 
 
  SELECT 
     rod_pk,
     rod_rgo_pk,
     HEX(rod_receiver_mobile) AS encrypted_hex,
     LENGTH(rod_receiver_mobile) AS byte_length,
     rod_create_time
 FROM regular_order_detail
 WHERE rod_pk IN (4989, 4991, 4992)
 ORDER BY rod_pk;
  
  
  
select *
from create_regular_log
where crl_error_code  = 'Other'
and crl_create_time > '2026-06-18 16:00'