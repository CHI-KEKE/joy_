# ErrorTask 錯誤處理文件

## 目錄
1. [Lock 失敗](#lock-失敗)
2. [呼叫 ECoupon API失敗](#券不足呼叫-ECoupon-API-失敗)
3. [The JSON value is not in a supported DateTime format](#3-the-json-value-is-not-in-a-supported-datetime-format)
4. [SalepageCollectionException](#4-salepagecollectionexception)
5. [PromotionRecycleCoupon 找不到 DDB 資料](#5-promotionrecyclecoupon-找不到-ddb-資料)
6. [點數中心 API 與 DDB 回收點數紀錄資料不對稱](#6-點數中心-api-與-ddb-回收點數紀錄資料不對稱)

<br>

---

## Lock 失敗

- 佔額與回饋互搶
- 正逆流程互搶
- 稽核正流程互搶

<br>
<br>

## 券不足呼叫 ECoupon API 失敗


- 訊息：優惠券回饋失敗，券數不足，ECouponId:
**主子單狀態：** WaitingToReward
**主子單UpdateReason：** 給券異常，本次已給予...

- 會在活動結束時間後 180 天內持續延遲 bookingTime
- 若經過重算，逆流程可能因為現狀以滿足壓上 Reward


## 呼叫 ECoupon API 失敗

記 log，優惠券回饋失敗，ECouponId:...

<br>
<br>

## 3. The JSON value is not in a supported DateTime format

### 訊息

```
System.FormatException: The JSON value is not in a supported DateTime format.

{
  "Id": "74c109d4-1da1-4678-8e9f-576f08127107",
  "SourceId": "",
  "JobName": "PromotionRewardCoupon",
  "Data": {
    "ShopId": 2,
    "PromotionEngineId": 6716,
    "PromotionEngineType": "RewardReachPriceWithCoupon",
    "OrderTypeDefEnum": "Others",
    "TradesOrderGroupCode": null,
    "CrmSalesOrderId": 329356,
    "OrderDateTime": "2025-05-08 20:00:00.000"
  }
}
```

<br>

### 原因

JSON 欄位內容中 `"OrderDateTime":"2025-05-08 20:00:00.000"` 無法被 System.Text.Json 正確解析為 DateTime
DateTime 少了 T 格式問題

<br>
<br>

## SalepageCollectionException

### 訊息

```
{"errorCode":"SalepageCollectionException","message":"HttpRequestException","data":"Response status code does not indicate success: 400 (Bad Request)."}
```

<br>

### 原因

可以打 saleapge-collection API 看看，掛掉請 arch 協助

<br>
<br>

## PromotionRecycleCoupon 找不到 DDB 資料

https://91app.slack.com/archives/C08BE4B4KQW/p1753756490298899

### 問題分析

<br>

- PromotionRewardLoyaltyPointsDispatcherV2Job 沒有被觸發
- OrderCreated Event 沒看到對應的TG
- OrderFailed 有紀錄
- OrderRecheck 後訂單狀態為 Timeout 觸發了 OrderCancelled

Athena 可以查
```SQL
SELECT * FROM "hk_prod_nmqv3"."archive_event"
WHERE event_name = 'OrderCreated'
AND date = '2025/06/15'
AND event LIKE '%TG250616A00008%'
LIMIT 10;
```

-  SalesOrderGroup
-  invalid => OrderFailed

<br>

### 結論

<br>

DDB 給券應該要檢查訂單狀態做噴錯

<br>
<br>

## 6. 點數中心 API 與 DDB 回收點數紀錄資料不對稱

### 問題描述

<br>

點數回收的 job 執行時，在進行逆流程主邏輯前，會先篩選 detail 資料抓出 "要退的對應子單"，但因為這個篩選是 by TS Code，因此以線下訂單而言，TS_正 + TS_逆都被帶出來往後走，到真正的逆流程退點時，被檢查邏輯所阻擋，造成 "不該退逆向單" 的狀況

<br>

### 問題分析

<br>

- **篩選邏輯問題**: 使用 TS Code 進行篩選時，會同時抓到正向單和逆向單
- **線下訂單特性**: TS_正 + TS_逆 都會被篩選條件帶出
- **檢查邏輯阻擋**: 在逆流程退點時被後續檢查邏輯阻擋
- **錯誤結果**: 造成不該退逆向單的異常狀況

<br>

### 修正方案

<br>

修正為要對應到 `OriginalCrmSalesOrderSlaveId` 才是真正要退的子單

**相關 MR**: https://gitlab.91app.com/commerce-cloud/nine1.promotion/nine1.promotion.worker/-/merge_requests/817

<br>



















## 大量 PromotionRewardCoupon Failed


- 有人負責拉 lock 失敗 tgpromotionId
- 有人處理解lock 語法
- 有人測試redo
- 友人與dba協作


https://91app.slack.com/archives/C3DB30C3T/p1762146085229249

NMQ 底層錯誤

Update inbound event category failed. Exception: System.NullReferenceException: Object reference not set to an instance of an object.


Redeploy worker 解決

EF 錯誤

Execution Timeout Expired.  The timeout period elapsed prior to completion of the operation or the server is not responding.

紀錄下　timeout位置
GetCrmMemberTierInfoAsync

//// 取得會員卡等級
var crmMemberTierInfo = await this._crmMemberRepository.GetCrmMemberTierInfoAsync(shopId, memberInfo.MemberId);



後來 Redploy 解決


解LOCK

UPDATE "TW_Prod_Loyalty_PromotionReward" SET "LockUserName" = '' WHERE "PromotionEngineId_TradesOrderGroupCode" = '8615_TG250901U00011';



CREATE 失敗 => 等待佔額做完


撈活動清單


SELECT *
  FROM [WebStoreDB].[dbo].[PromotionEngine]
  WHERE [PromotionEngine_ValidFlag] = 1
  AND [PromotionEngine_TypeDef] = 'RewardReachPriceWithCoupon'
  AND [PromotionEngine_StartDateTime] <= '2025-11-03 10:00'
  AND [PromotionEngine_EndDateTime] >= '2025-11-03 10:00'




## basketapi 太慢


locationTag 拉 s3
料號 拉 s3



## PromotionRecycleCoupon 查無訂單 DDB 給券資料 505318_CrmSalesOrder:196940881


未來訂單 不會做回饋 但回收沒有阻擋

```sql
select CrmSalesOrderSlave_CalculateMemberTierDateTime,CrmSalesOrderSlave_TradesOrderFinishDateTime,CrmSalesOrderSlave_OriginalCrmSalesOrderSlaveId,*
from CrmSalesOrderSlave(nolock)
where CrmSalesOrderSlave_Id in(672895462,672895465,672895463,672895466)


select CrmSalesOrderSlave_ShopId,CrmSalesOrderSlave_CrmSalesOrderId,CrmSalesOrderSlave_CalculateMemberTierDateTime,CrmSalesOrderSlave_TradesOrderFinishDateTime,CrmSalesOrderSlave_OriginalCrmSalesOrderSlaveId,*
from CrmSalesOrderSlave(nolock)
where CrmSalesOrderSlave_Id in(659445079,
659445080,
659978649,659978650)
```



## 轉單時間差


https://91app.slack.com/archives/C3DB30C3T/p1762221671530419

RecycleLoyaltyPointsDispatcherV2 撈不到 SalesOrderSlave 訂單成立後取消比轉單時間還早


## NMQ 底層錯誤


Update inbound event category failed. Exception: System.NullReferenceException: Object reference not set to an instance of an object.

## BatchAuditLoyaltyPoints 撈 SalesOrderSlave timeout 30 sec

撈 SalesOrderSlave timeout 30 sec
Execution Timeout Expired. The timeout period elapsed prior to completion of the operation or the server is not responding.



## 線下訂單完全沒有紀錄


ddb log 完全沒有正流程紀錄
acive crm 有 建立
batch 有建立
d254e098-6530-490b-82a7-e5b89707d631
"CalculateDate":"2025-11-04T00:00:00+08:00"
撈取 11:03~11:04 訂單
但 7360537 create 時間在  2025-11-04 04:10:07.580 會撈不到


==> 改 CRONJOB 時間




## OccupyDisptacher 比 正流程 Dispatcher 晚做


https://91app.slack.com/archives/C3DB30C3T/p1762409054295869
Ocuupy比Reward晚執行
故佔額活動會遇到無佔額紀錄的互卡問題

OrderPlaced比OrderCreated早1.5秒
但PromotionRewardOccupyDispatcher比PromotionRewardLoyaltyPointsDispatcherV2
晚開始執行
估計是塞車影響
且兩個不同Job，執行時沒有先後順序問題
所以才會被搶先做
暫解:讓兩個分派的量能相同 讓佔額分派量能大於Reward
PromotionRewardOccupyDispatcher
Max Processes:2 -> ​4
PromotionRewardLoyaltyPointsDispatcherV2
Max Processes:5 -> ​2

持續觀察中


如果遇到無佔額紀錄
重新booking 5分鐘後的自己(不能寫死5分鐘，NMQ怪怪的太剛好會變及時)
上限先1次，加入數量判斷機制(放在TaskData

Lock的問題可以在PromotionRewardLoyaltyPointsDispatcherV2Job解
如果活動GroupCode不為空
CreateTask的時後就Booking 五分鐘後

## lock fail


lock fail 一波沒看到特殊 log 的 需要一個個解完會需要花時間處理
- 佔額找不到資料 看是否能 5 min booking 後緩解

## Response status code does not indicate success: 413 (Request Entity Too Large)

因為 QTY 太大包


## 合併帳號導致發券時找不到 


Member https://91app.slack.com/archives/C08BE4B4KQW/p1762653130035569


## RecycleCoupon 想去回收帶有勾跡序號的正向單但找不到資料 


CrmSalesOrderSlave_CrmSalesOrderId : 221052607 , 221052607, 221052953, 221052955, 221052955, 221053270


## 35248_TG251105Z00130 BookingTime與預期不符，時間差異: 24.00 小時，超過允許範圍 1 小時


https://91app.slack.com/archives/C7T5CTALV/p1762362105299559
給點紀錄稽核監控到異常
市場環境: HK-Prod
TG Code: TG251105Z00130
稽核到下列異常:
35248_TG251105Z00130 BookingTime與預期不符，時間差異: 24.00 小時，超過允許範圍 1 小時


DDB
BookingTimeUTC 2025-11-12T16:00:00.0000000Z
OrderTimeUTC 2025-11-05T15:54:52.2055854Z
UpdateTimeUTC 2025-11-05T16:02:14.9945738Z
RewardHistory [{"TotalLoyaltyPoint":428,"GivingPoints":0,"RecyclePoints":0,"RewardStatus":"WaitToReward","RewardTime":"0001-01-01T00:00:00","UpdateUser":"PromotionRewardLoyaltyPointsV2","UpdateReason":"活動給點:35248","UpdateTime":"2025-11-06T00:02:14.9945738+08:00"}]



## 回饋活動 - 線下未來訂單退款JobFail

https://91app.slack.com/archives/C3DB30C3T/p1761901296661149

ShopId:400
釐清後是未來訂單
CrmSalesOrderSlave_CalculateMemberTierDateTime: 2025-10-19 23:59:00.000
CrmSalesOrder_TradesOrderFinishDateTime: 2025-10-30 20:25:00.000
回收會拿原單 TradesOrderFinishDateTime來判斷是否符合活動
活動是 2025-10-29T09:01:00 ~ 2025-11-12T09:00:59.997
所以回收以為中了但其實沒有QQ
TradesOrderFinishDateTime判斷活動是沒錯的 ✅
但要排除未來訂單就比較麻煩🤯


## 回收不到有勾稽的正向單

那個正向單是沙小

> notmal type + original

據說是換貨類型

討論時要問道的重點是 omo 會將這樣的訂單視為哪一種訂單 我們到底應不應該給予回饋?

疑點 : 他們說是換貨 那表示第一個商品應該等於是沒買了 後續又退掉換過去的貨 結論看起來是沒買 才對 但最後又說因為總金額 Summary 起來是正的所以有買

以結果來說 第三張是保留有回饋的 第一是被第二張退掉


常常遇到 TagService 壞掉

確認稽核怎麼整理線下訂單的

AuditCrmOthersOrderPromotionRewardCouponJobName
AuditCrmOthersOrderPromotionRewardLoyaltyPointsV2JobName



PurchaseType	TypeMemo	訂單類型	OriginalCrmSalesOrderId	是否保留
Normal	正向 Memo	NormalType	0 (無勾稽)	✅ 保留
Normal	正向 Memo	NormalType	== crmSalesOrderId (勾稽自己)	✅ 保留
Normal	正向 Memo	NormalType	!= crmSalesOrderId (勾稽他人)	✅ 保留
Normal	負向 Memo	ReturnType	0 (無勾稽)	✅ 保留
Normal	負向 Memo	ReturnType	== crmSalesOrderId (勾稽自己)	✅ 保留
Normal	負向 Memo	ReturnType	!= crmSalesOrderId (勾稽他人)	❌ 排除
Return	正向 Memo	ReturnType	0 (無勾稽)	✅ 保留
Return	正向 Memo	ReturnType	== crmSalesOrderId (勾稽自己)	✅ 保留
Return	正向 Memo	ReturnType	!= crmSalesOrderId (勾稽他人)	❌ 排除
Return	負向 Memo	NormalType	0 (無勾稽)	✅ 保留
Return	負向 Memo	NormalType	== crmSalesOrderId (勾稽自己)	✅ 保留
Return	負向 Memo	NormalType	!= crmSalesOrderId (勾稽他人)	✅ 保留
