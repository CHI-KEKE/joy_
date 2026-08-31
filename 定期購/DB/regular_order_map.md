## 表用途

`regular_order_map` 是**計畫與最新期數的對應索引表**，每個 `regular_order` 固定只有一筆記錄。  
永遠指向該計畫「最新一期」的 `regular_order_detail`。  
主要用途：PHP 查詢會員訂閱列表時 JOIN 此表，避免在 `regular_order_detail` 做 subquery 撈最新期，提升效能。  
`rom_rgo_pk` 有 UNIQUE constraint；每次建立新期後即以 UPSERT 更新 `rom_rod_pk`。

> 此表**無 status 欄位**。

---

```sql
 -- ① regular_order_map：確認表結構 + 幾筆範例
 SELECT rom_pk, rom_rgo_pk, rom_rod_pk
 FROM regular_order_map
 LIMIT 10;

 -- ② 完整 JOIN（計畫 → 最新期數）：模擬 PHP 查會員訂閱列表的方式
 SELECT
     rgo.rgo_pk,
     rgo.rgo_source_tg,
     rgo.rgo_source_ts,
     rgo.rgo_shop_id,
     rgo.rgo_member_id,
     rgo.rgo_ship_period,
     rgo.rgo_status,
     rod.rod_pk,
     rod.rod_ship_sequence,
     rod.rod_ship_date,
     rod.rod_create_order_status
 FROM regular_order_map rom
 LEFT JOIN regular_order rgo ON rom.rom_rgo_pk = rgo.rgo_pk
 LEFT JOIN regular_order_detail rod ON rom.rom_rod_pk = rod.rod_pk
 WHERE rgo.rgo_shop_id = <your_shop_id>
 ORDER BY rgo.rgo_first_create_date DESC
 LIMIT 10;

 -- ③ 確認 mapping 是否真的只有一筆 per rgo_pk（唯一性驗證）
 SELECT rom_rgo_pk, COUNT(*) AS cnt
 FROM regular_order_map
 GROUP BY rom_rgo_pk
 HAVING cnt > 1;
 -- 預期：0 rows（每個計畫只有一筆 mapping）

 -- ④ 找一個真實的首單範例（rod_ship_sequence = 1）看完整欄位
 SELECT
     rgo.rgo_pk,
     rgo.rgo_source_tg,
     rgo.rgo_source_ts,
     rgo.rgo_rpd_pk,
     rgo.rgo_ship_date,
     rgo.rgo_next_ship_date,
     rgo.rgo_ship_period,
     rgo.rgo_ship_count,
     rgo.rgo_create_order_status,
     rgo.rgo_status,
     rgo.rgo_create_user,
     rod.rod_pk,
     rod.rod_ship_sequence,
     rod.rod_ship_date,
     rod.rod_create_order_status,
     rom.rom_pk
 FROM regular_order rgo
 LEFT JOIN regular_order_detail rod ON rod.rod_rgo_pk = rgo.rgo_pk AND rod.rod_ship_sequence = 1
 LEFT JOIN regular_order_map rom ON rom.rom_rgo_pk = rgo.rgo_pk
 WHERE rgo.rgo_create_user = 'DAEMON'
 ORDER BY rgo.rgo_pk DESC
 LIMIT 5;

 -- ⑤ 確認 regular_order_map table schema（看有沒有其他欄位）
 DESCRIBE regular_order_map;
```