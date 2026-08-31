
● 錯誤很清楚：value too long for type character varying(16) — nq_type 欄位 DB 定義是 varchar(16)，但程式塞入的通知類型字串超過 16 字。


● 找到了！兩個 varchar(16) 欄位：

   - NqRefOrder — HasMaxLength(16)  
   - NqCreateUser — HasMaxLength(16)


