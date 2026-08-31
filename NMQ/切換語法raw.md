use NMQV2DB



---- === 取得 GORUPID / JOBID ===

declare @jobId int;

-- jobid = 77
select @jobId = Job_Id
from Job(nolock)
where Job_ValidFlag = 1
and Job_Name = 'SalesOrderDataExportTask'


PRINT(@jobId)

declare @jobGroupIdforQA9 INT;

--jobGroupID = 11
select @jobGroupIdforQA9 = JobGroup_Id
from JobGroup(nolock)
WHERE JobGroup_Name = 'QA9'

PRINT(@jobGroupIdforQA9)


---- === 復活 JOBGROUPMAPPING!! TO QA9 !!!

SELECT *
FROM JobGroupMapping
WHERE JobGroupMapping_JobId = @jobId

SELECT JobGroup_Name,JobGroupMapping_JobGroupId,Job_Name,*
FROM JobGroupMapping
INNER JOIN 
JobGroup(NOLOCK)
ON JobGroup_Id = JobGroupMapping_JobGroupId
INNER JOIN Job(NOLOCK)
ON Job_Id = JobGroupMapping_JobId
Where JobGroupMapping_JobId = @jobId
--AND JobGroupMapping_Id = 610


UPDATE JobGroupMapping
SET JobGroupMapping_ValidFlag = 0
Where JobGroupMapping_JobId = @jobId
AND JobGroupMapping_ValidFlag = 1



SELECT JobGroup_Name,JobGroupMapping_JobGroupId,Job_Name,JobGroupMapping_ValidFlag,*
FROM JobGroupMapping
INNER JOIN 
JobGroup(NOLOCK)
ON JobGroup_Id = JobGroupMapping_JobGroupId
INNER JOIN Job(NOLOCK)
ON Job_Id = JobGroupMapping_JobId
Where JobGroupMapping_JobId = 77
--AND JobGroupMapping_Id = 610



--============== 讓 JOBSWITCH 消失 !!!

use NMQV2DB



---- === 取得 GORUPID / JOBID ===

declare @jobId2 int;

-- jobid = 77
select @jobId2 = Job_Id
from Job(nolock)
where Job_ValidFlag = 1
and Job_Name = 'SalesOrderDataExportTask'


SELECT *
FROM JobSwitch
Where JobSwitch_JobId = @jobId2


Update dbo.JobSwitch
SET JobSwitch_ValidFlag =1
Where JobSwitch_JobId = @jobId2



SELECT *
FROM JobSwitch
Where JobSwitch_JobId = @jobId2


------------ == 總攬!!!
use NMQV2DB
select Job_Name,JobSwitch_JobId,JobSwitch_ValidFlag,JobGroupMapping_JobId,JobGroup_Name,JobGroupMapping_ValidFlag,*
from JobGroupMapping
inner join JobGroup
on JobGroup_Id = JobGroupMapping_JobGroupId
inner join JobSwitch
on JobGroupMapping_JobId = JobSwitch_JobId
INNER JOIN Job
ON Job_Id = JobSwitch_JobId
where JobGroupMapping_JobId = 77
--and JobGroup_Name = 'QA9'



--===== 還原回 v3
--use NMQV2DB
--declare @jobId3 int;
--DECLARE @jobGroupIdforQA31 INT;

-- jobid = 77
select @jobId3 = Job_Id
from Job(nolock)
where Job_ValidFlag = 1
and Job_Name = 'SalesOrderDataExportTask'

use NMQV2DB
declare @jobId3 int;



SELECT *
FROM JobGroupMapping
WHERE JobGroupMapping_JobId = 77

SELECT JobGroup_Name,JobGroupMapping_JobGroupId,Job_Name,*
FROM JobGroupMapping
INNER JOIN 
JobGroup(NOLOCK)
ON JobGroup_Id = JobGroupMapping_JobGroupId
INNER JOIN Job(NOLOCK)
ON Job_Id = JobGroupMapping_JobId
Where JobGroupMapping_JobId = @jobId3
--AND JobGroupMapping_Id = 610


--jobGroupID = 11
DECLARE @jobGroupIdforQA31 INT;
select @jobGroupIdforQA31 = JobGroup_Id
from JobGroup(nolock)
WHERE JobGroup_Name = 'QA31'

SELECT *
FROM JobGroup(NOLOCK)
WHERE JobGroup_Name = 'QA31'


UPDATE JobGroupMapping
SET JobGroupMapping_JobGroupId = @jobGroupIdforQA31,JobGroupMapping_ValidFlag = 0
Where JobGroupMapping_JobId = 77
AND JobGroupMapping_ValidFlag = 1



SELECT JobGroup_Name,JobGroupMapping_JobGroupId,Job_Name,JobGroupMapping_ValidFlag,*
FROM JobGroupMapping
INNER JOIN 
JobGroup(NOLOCK)
ON JobGroup_Id = JobGroupMapping_JobGroupId
INNER JOIN Job(NOLOCK)
ON Job_Id = JobGroupMapping_JobId

Where JobGroupMapping_JobId = 77
--AND JobGroupMapping_Id = 610



SELECT *
FROM JobSwitch
Where JobSwitch_JobId = @jobId3


Update dbo.JobSwitch
SET JobSwitch_ValidFlag =1
Where JobSwitch_JobId = @jobId3



SELECT *
FROM JobSwitch
Where JobSwitch_JobId = @jobId3



--====確認
USE NMQV2DB
DECLARE @JJ INT = 77;

SELECT *
FROM JobSwitch
Where JobSwitch_JobId = @JJ



SELECT *
FROM JobGroupMapping
WHERE JobGroupMapping_JobId = @JJ



use WebStoreDB

select top 5 TradesOrderSlave_ItemType,*
from TradesOrderSlave(nolock)
where TradesOrderSlave_ItemType = 'C'