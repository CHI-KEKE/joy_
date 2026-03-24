## 改完要清 redis + browser cache






## GlobalShipping

```sql
-- GlobalShipping : PickCountry,EasyParcelAPIKey
USE WebStoreDB
select *
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_GroupTypeDef = 'GlobalShipping'
and ShopDefault_Key = 'EasyParcelAPIKey'
and ShopDefault_ShopId = 83


--update ShopDefault
--set ShopDefault_NewValue = 'SG',
--ShopDefault_Value = 'SG'
--where ShopDefault_ValidFlag = 1
--and ShopDefault_GroupTypeDef = 'GlobalShipping'
--and ShopDefault_Key = 'PickCountry'
--and ShopDefault_ShopId = 83
```


| 系統 / 模組  | 是放 Machine Config   | 使用情境 / API   
| ---------- | -------------------- | ---------------- |
| **Batch**  | ✅                   | `PayOrder`（查單用途）|
| **NMQ**    | ✅                   | `PayOrder`（查單用途） |
| **OSMAPI** | ✅                   | 未明確看到實際使用情境| 
| **OSMWEB** | ✅                   | Bulk API (這次 release)        | 
| **MWeb**   | ✅                   | `PayOrder`（查單用途）| 
| **ERP**    | ❌                   | —                | 



## EnableOTP

#### 功能路徑

測試店_EMPTYOOO_TEST >> 全通路會員管理 >> 會員登入註冊設定

```sql
use WebStoreDB


select ShopDefault_Key,ShopDefault_Value,ShopDefault_ShopId,*
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_GroupTypeDef = 'MemberAuthentication'
and ShopDefault_ShopId in (200017,200136)
```