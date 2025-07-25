# 稽核異常記錄文件

## 目錄
1. [DDB已發券序號為空](#1-ddb已發券序號為空)

<br>

---

## 1. DDB已發券序號為空

### 訊息

```
@oversea_brd [MY][Prod]
優惠券紀錄稽核異常
ServiceName: AuditPromotionRewardCouponService
ShopId: 200009
異常項目:
DDBKey:8444_MG250722L00007，ECouponId:222755，MemberId:549049 DDB已發券序號為空
```

<br>

### Task與訂單

**Task 查詢：**
```
{service="prod-promotion-service"}
|json
| _props_TaskId = `2a5661bb-6e78-4d4a-850e-b05b4c6c4435`
```

<br>

**DDB Key：**
```
8444_MG250722L00007
```

<br>

### 釐清

看起來是正常發送 3 張券，但 `"GivenCouponSlaveIdList":[]` 導致稽核覺得有問題

<br>

1. 實際上問題卡在 scmapiv2 無法從 Grafana 查 api log 確認 Dispatch API 是不是真的沒有回 SlaveIdList

2. GivenCouponSlaveIdList 的紀錄是新上的程式碼才會記錄的 因此稽核會誤判

<br>

無法從 db 去判斷到底有沒有給到對應的人 => 可以 by MemberId

<br>

### 查詢語法

```sql
use WebStoreDB

select ECoupon_Code,ECoupon_Modes,ECouponSlave_MemberId,*
from ECoupon(nolock)
inner join ECouponSlave(nolock)
on ECoupon_Id = ECouponSlave_ECouponId
where ECoupon_ValidFlag = 1
and ECoupon_ShopId = 200009
and ECoupon_Id = 222755
```

<br>