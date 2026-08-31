


## task 數量


```sql
SELECT job_name, COUNT(*) as job_count 
FROM task.archive
WHERE "group" in ('91app-live.group1','91app-live.group2','91app-live.group3')
AND task_create_time BETWEEN '2026-03-10 12:30' AND '2026-03-10 14:50'
GROUP BY job_name
```