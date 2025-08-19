# Member Query 查詢語法

## 目錄
1. [Member / CrmMember 互查](#1-member--crmmember-互查)

<br>

---

## 1. Member / CrmMember 互查

### 1.1 VipMember 查詢

```sql
SELECT *
FROM dbo.VipMember WITH(NOLOCK)
WHERE VipMember_ShopId = 4
AND VipMember_CellPhone LIKE '%88887766%'
```

<br>

### 1.2 CrmMember 查詢

```sql
Select *
from dbo.CrmMember WITH(NOLOCK)
WHERE CrmMember_NineYiVipMemberId = 1440
and CrmMember_ShopId = 4
And CrmMember_ValidFlag = 1
```

<br>

---