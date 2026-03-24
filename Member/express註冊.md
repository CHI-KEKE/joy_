## osm 設定路徑

LilyTestssssss
全通路會員管理
會員登入註冊設定

## 前台註冊時多打得 api

https://emptyoo.91app.com.my/webapi/MemberRegister/CreateExpressMemberRegister?lang=en-US&shopId=200017



```sql
use WebStoreDB


select ShopDefault_Key,ShopDefault_Value,ShopDefault_ShopId,ShopDefault_UpdatedUser,*
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_GroupTypeDef = 'MemberAuthentication'
and ShopDefault_ShopId in (200017,200136)
```