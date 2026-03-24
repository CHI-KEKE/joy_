

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