USE NMQV2DB
GO

-- Job_Id	Job_Name
-- 112	SendTemplateMailPriorityLow

SELECT
	Task_Status
   ,COUNT(1)
FROM dbo.Task WITH (NOLOCK)
WHERE Task_ValidFlag = 1
AND Task_CreatedDatetime BETWEEN '2025/08/30 00:00:00' AND '2025/08/30 23:59:59'
AND Task_JobId = 112
GROUP BY Task_Status

--SELECT
--	COUNT(1)
--FROM dbo.Task WITH (NOLOCK)
--WHERE Task_ValidFlag = 1
--AND Task_CreatedDatetime BETWEEN '2025/07/31 00:00:00' AND '2025/07/31 23:59:59'
--AND Task_Data LIKE N'%折價券到期通知信%'
--AND Task_JobId = 112


--SELECT
--TOP 10
--	*
--FROM dbo.Task WITH (NOLOCK)
--WHERE Task_ValidFlag = 1
--AND Task_CreatedDatetime BETWEEN '2025/07/30 00:00:00' AND '2025/07/30 23:59:59'
--AND Task_JobId = 112
--AND Task_Data LIKE N'%折價券到期通知信%'
--ORDER BY Task_Id DESC

--{"Sender":null,"Receivers":null,"Subject":"折價券到期通知信","Content":null,"TemplateName":"ECouponExpireSoon","TemplateData":"10795,4187435,275349061,"}


SELECT
	DATEADD(MINUTE, DATEDIFF(MINUTE, 0, Task_BookingTime), 0) AS MinuteInterval
   ,COUNT(1) AS TotalCount
FROM dbo.Task WITH (NOLOCK)
WHERE Task_ValidFlag = 1
AND Task_CreatedDatetime BETWEEN '2025/08/30 00:00:00' AND '2025/08/30 23:59:59'
AND Task_JobId = 112
AND Task_Status = 'Switched'
GROUP BY DATEADD(MINUTE, DATEDIFF(MINUTE, 0, Task_BookingTime), 0)
ORDER BY MinuteInterval;



--SELECT
--	DATEADD(MINUTE, DATEDIFF(MINUTE, 0, Task_CreatedDatetime), 0) AS MinuteInterval
--   ,COUNT(1) AS TotalCount
--FROM dbo.Task WITH (NOLOCK)
--WHERE Task_ValidFlag = 1
--AND Task_CreatedDatetime BETWEEN '2025/07/30 00:00:00' AND '2025/07/30 23:59:59'
--AND Task_JobId = 112
----AND Task_Status = 'Switched'
--GROUP BY DATEADD(MINUTE, DATEDIFF(MINUTE, 0, Task_CreatedDatetime), 0)
--ORDER BY MinuteInterval;



