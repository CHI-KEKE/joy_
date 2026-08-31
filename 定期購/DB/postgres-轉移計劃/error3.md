EF Core 在讀取 RETURNING 欄位時，C# model 有 bool 型別但 PostgreSQL 欄位是 smallint。看 insert 的 RETURNING：mbi_pk, mbi_login_count, mbi_notify — mbi_notify 是 bool 但 DB 是 smallint。


● 全部 bool? 都來自 MySQL tinyint(1)，PostgreSQL DB 欄位是 smallint，Npgsql 不做自動轉換。

  最快修法：在 DBContext 為這些欄位加 value converter（bool? ↔ short），這樣 model 語意不變，DB 讀寫都正確。



  修復摘要：

 - 原因：MySQL tinyint(1) → PostgreSQL smallint，但 C# model 用 bool?，Npgsql 不做自動轉換
 - 修法：在 OnModelCreating 定義 ValueConverter<bool?, short?> 並套用到所有 bool? 欄位： - MbiNotify, RgoInSchedule, RgoIsCancel, RgoIsComplete, RpdBuySetting（主 DBContext）
 - SpiCancelRegular, SpiEnableEcoupon, SpiEnableRegular, SpiStatus, SpiSuspendRegular（Extension）
 - 順帶修掉 extension 檔殘留的 HasColumnType("datetime") 和 quoted 數值 default

現在可以重新測試 InitJob，INSERT member_info 這步應該可以過了。