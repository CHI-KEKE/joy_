

## JOB / Task


```sql
select *
from Job(nolock)
where Job_ValidFlag = 1
and Job_Name like '%SmsMessage%'

select Task_Data
from Task(nolock)
where Task_ValidFlag = 1
and Task_JobId = 264
```