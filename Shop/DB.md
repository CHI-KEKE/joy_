

## Shop / Supplier

```sql
USE WebStoreDB


SELECT Supplier_SalesMarket,*
FROM Supplier(NOLOCK)

-- 200100, 200099

SELECT *
FROM Shop(NOLOCK)
WHERE Shop_ValidFlag = 1
AND Shop_SupplierId IN (200100, 200099)
```


## password


```sql
use AuthExternalDB

--select *
--from　PasswordHistory
--where PasswordHistory_UsersId = 91596


--update PasswordHistory
--set PasswordHistory_CreatedUser = 'system',
--PasswordHistory_UpdatedUser = 'system',
--PasswordHistory_CreatedDateTime = '2025-10-01 11:24:12.910',
--PasswordHistory_UpdatedDateTime = '2025-10-01 11:24:12.910'
--where PasswordHistory_ValidFlag = 1
--and PasswordHistory_Id = 431

--select *
--from　PasswordHistory
--where PasswordHistory_UsersId = 91596

```


## ShopDefault

```sql
USE WebStoreDB
select *
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_GroupTypeDef = 'GlobalShipping'
and ShopDefault_Key = 'EasyParcelAPIKey'
and ShopDefault_ShopId = 83
```


| 系統 / 模組  | 是放 Machine Config   | 使用情境 / API   
| ---------- | -------------------- | ---------------- |
| **Batch**  | ✅                   | `PayOrder`（查單用途）|
| **NMQ**    | ✅                   | `PayOrder`（查單用途） |
| **OSMAPI** | ✅                   | 未明確看到實際使用情境| 
| **OSMWEB** | ✅                   | Bulk API (這次 release)        | 
| **MWeb**   | ✅                   | `PayOrder`（查單用途）| 
| **ERP**    | ❌                   | —                | 
