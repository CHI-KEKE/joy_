
-- regular_order
SELECT rgo_product_name ,
rgo_member_id,
rgo_shop_id ,
		rgo_pause_date ,
		rgo_status ,
		rgo_create_order_status ,
		rgo_source_tg ,
		rgo_source_ts ,
		rgo_next_ship_date ,
		* from regular_order
inner join shop_info si 
	on si.spi_shopid  = rgo_shop_id 
inner join member_info mi 
	on rgo_member_id  = mi.mbi_mbid 
where rgo_source_ts   in ('MS260626U000001','MS260626T000002','MS260626T000001','MS260626S000014','MS260626S000013','MS260626S000012','MS260626S000011','MS260626S000010','MS260626S000009','MS260626S000008')
ORDER BY rgo_pk desc;
   
SELECT
    rod.rod_tg_code,
    rod.rod_ts_code,
    ro.rgo_product_name ,
    rod.rod_create_order_status,
    rod.rod_create_order_error_code,
    *
FROM regular_order_detail AS rod
INNER JOIN regular_order AS ro
    ON rod.rod_rgo_pk = ro.rgo_pk
WHERE rod.rod_ts_code  in ('MS260626U000001','MS260626T000002','MS260626T000001','MS260626S000014','MS260626S000013','MS260626S000012','MS260626S000011','MS260626S000010','MS260626S000009','MS260626S000008')
ORDER BY rod.rod_pk DESC
LIMIT 10;


select *
from regular_order ro 


select *
from regular_order_detail rod 
where rod.rod_tg_code  = 'MG260702L00002'
order by rod.rod_create_time desc

   
-- member_info
SELECT *
FROM member_info
inner join shop_info si 
	on mbi_shopid  = si.spi_shopid 
where mbi_mbid  = 99000136
ORDER BY mbi_pk desc
LIMIT 10;

-- regular_product
SELECT * FROM regular_product
ORDER BY rpd_pk desc
LIMIT 10;


select *
from notify_queue nq
where nq.nq_ref_member  = '99000136'


update notify_queue
set nq_send_status = 0
where nq_pk = 100001

delete from notify_queue 
where nq_pk = 100000

delete from member_info 
where mbi_pk = 100000


update regular_order ro 
set rgo_next_ship_date  = '2026-06-28'
where ro.rgo_pk  = 100003


select si.spi_enable_regular ,si.spi_regular_provider ,*
from shop_info si 