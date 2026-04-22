

"Cannot
insert duplicate key row in object 'dbo.LoyaltyPointTransactionInfo' with unique index 'CIX_LoyaltyPointTransactionId'. The duplicate key value is
(6500344).


USE LoyaltyDB
GO

SELECT TOP 100
	*
FROM dbo.LoyaltyPointTransactionInfo WITH (NOLOCK)
WHERE LoyaltyPointTransactionInfo_LoyaltyPointTransactionId IN (6500344)
AND LoyaltyPointTransactionInfo_ValidFlag = 1

SELECT TOP 10
	*
FROM dbo.LoyaltyPointTransactionRecord WITH (NOLOCK)
WHERE LoyaltyPointTransactionRecord_LoyaltyPointTransactionId IN (6500344)
AND LoyaltyPointTransactionRecord_ValidFlag = 1


資料有寫入 (edited) 

到 SG-MY-NMQ2 查 log 的語法:

PS D:\log\ny-log\Common\NMQv3Worker> ls | ? { $_.LastAccessTime -gt [datetime]::parse("2025/4/30 08:00:00") } | Select-String "d1a45f57-34a6-4e89-b5e3-4e89aecad695"[8:49 AM]archive