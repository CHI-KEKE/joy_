

## Workgroup

**nmqv3_hk：** 要使用 workgroup UPD-UPD2-B2E6
**nmqv3_my：** 要使用 workgroup UPD-UPD2-B2E8

<br>

## my archive_task

```sql
SELECT * FROM "nmqv3_my"."archive_task"
--where 1 =1
--and controller = 'tradesOrderLite'
--and action = 'CompleteForNewCart'
--and date = '2025/06/24'
--and message like '%MG250624J00008%'
where job_name = 'OnlineBankingRazerRefundRequestFinish'
--and requestid = '{"message":"202506160110276106'
and date = '2025/07/14'
and id = 'd0b8de76-683b-4ed7-8a00-232ea1f90623'
limit 100;
```

![alt text](./image-13.png)


## hk archive_task

```sql
select * from "nmqv3_hk"."archive_task" limit 10;
```

## view_worker_log

```sql
SELECT *
FROM "view_worker_log"
WHERE date = '2025/12/04'
--and _msg like '%cs:line 53%'
and _hid = 'SG-MY-NMQ1'
AND json_extract(_props, '$.taskid') = CAST('4b6f5b04-2807-4bd8-9847-970b81756cf9' as JSON)
limit 10;
```