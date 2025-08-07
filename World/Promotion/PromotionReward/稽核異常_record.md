# 稽核異常記錄文件

## 目錄
1. [DDB已發券序號為空](#1-ddb已發券序號為空)
2. [攤提結果因重算不符預期](#2-攤提結果因重算不符預期)
3. [找不到對應的退貨訂單明細](#3-找不到對應的退貨訂單明細)
4. [線下訂單給點紀錄稽核監控到異常](#4-線下訂單給點紀錄稽核監控到異常)

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

---

## 2. 攤提結果因重算不符預期

### 訊息

```
給點紀錄稽核監控到異常
市場環境: TW-Prod
TG Code: TG250722QA00W6
稽核到下列異常:
應給點數(200)與實際點數(0)不同
活動:472232 攤提結果不符預期
應給點數(60)與實際點數(0)不同
活動:472125 攤提結果不符預期
```

<br>

### 釐清

確認 8 號店 7/16 以後走重算

<br>

**ShopStaticSetting 設定：**
```
ShopStaticSetting_ShopId: 8
ShopStaticSetting_Value: {"ShopIds": [], "SwitchDateTime": "2025-07-16T00:00:00"}
```

<br>

看起來其中兩個 task Id 是為贈品不退點的紀錄：

<br>

**Task 1：** `b9ff4f3e-4723-4b1f-a182-c0f417707a27`
- TS250722QA0020F, IsGift:True, IsSalePageGift:False, IsMajor:False 為贈品，不需要執行退點

<br>

**Task 2：** `01d23f85-a0f7-430c-9a6e-134f8b25c19d`
- IsGift:True, IsSalePageGift:False, IsMajor:False 為贈品，不需要執行退點

<br>

**Task 3：** `21b6ad8c-6af5-4575-9157-cdf036936c1e`
- 重算後整單不滿額，更新回饋狀態為 Cancel

<br>

---

## 3. 找不到對應的退貨訂單明細

### 訊息

```
給券回收紀錄稽核監控到異常
市場環境: HK-Prod
TS250722Q000299
稽核到下列異常:
DDB Detail找不到對應的退貨訂單明細crmSalesOrderSlaveId：0
DDB Keys：34743_TG250722Q00126
```

<br>

### 釐清

看起來是想稽核 detail 是不是在走逆流程時有紀錄到子單，但這邊是線上單在找是否有線下的子單且要 = 0，為誤判

<br>

---

## 4. 線下訂單給點紀錄稽核監控到異常

### 訊息

```
線下訂單給點紀錄稽核監控到異常 應給點數(68)與實際給點點數(56)不同
```

<br>

### 相關討論

<br>

https://91app.slack.com/archives/C7T5CTALV/p1753324426113299

<br>

### 原因

<br>

打菜籃計算給點沒有排除負向單

<br>