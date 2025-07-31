# Query 文件

## 目錄
1. [ShopEtlFlow](#1-shopetlflow)
2. [ShopEtlFlowStep](#2-shopetlflowstep)
3. [EtlService](#3-etlservice)
4. [EtlFlowTask](#4-etlflowtask)

<br>

---

## 1. ShopEtlFlow

<br>

```sql
use EtlDB

select *
from ShopEtlFlow(nolock)
where ShopEtlFlow_Name = 'Flow_CheckThirdPartyPaymentOrders'
```

<br>

```sql
select *
from ShopEtlFlowStep(nolock)
where ShopEtlFlowStep_ShopEtlFlowId = 188
```

<br>

---

## 2. ShopEtlFlowStep

<br>

```sql
use EtlDB

select *
FROM ShopEtlFlow(nolock)
inner join ShopEtlFlowStep(nolock)
on ShopEtlFlow_Id = ShopEtlFlowStep_ShopEtlFlowId
where ShopEtlFlow_ValidFlag = 1
and ShopEtlFlow_Id = 188
```

<br>

---

## 3. EtlService

<br>

```sql
select *
from EtlService(nolock)
where EtlService_ValidFlag = 1
and EtlService_Id = 219
```

<br>

---

## 4. EtlFlowTask

<br>

```sql
use EtlDB

select *
from EtlFlowTask(nolock)
where EtlFlowTask_ValidFlag = 1
and EtlFlowTask_ShopEtlFlowId = 188
and EtlFlowTask_StatusUpdatedDateTime > '2023-01-01'
--and EtlFlowTask_StatusUpdatedDateTime < '2024-01-01'
order by EtlFlowTask_StatusUpdatedDateTime
--and EtlFlowTask_CreatedDateTime BETWEEN '2024-03-23' and '2024-03-24'
```

<br>