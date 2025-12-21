```sql
USE NMQV2DB

SELECT *
FROM Job(NOLOCK)
WHERE Job_ValidFlag = 1
AND Job_Name = 'BatchUpload'

select *
from Task(nolock)
where Task_ValidFlag = 1
--and Task_JobId = 50
and Task_Data like '%ExportRewardPromotionSalePage%'
order by Task_CreatedDatetime desc


use NMQV2DB

select *
from Job(nolock)
where Job_ValidFlag = 1
and Job_Name = 'BatchModifyPromotionSalePageTask'
```