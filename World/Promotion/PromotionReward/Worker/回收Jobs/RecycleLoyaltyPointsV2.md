

## RecycleRequestEntity

TSCode
EventName
PromotionEngineType


ShopId
TSCode
EventName
TriggerDatetime
OrderCreateDate
PromotionId
PromotionEngineType


ShopId
TSCode
this.EventName = "Return";
TriggerDatetime
OrderCreateDate = originOrderSlave.CrmSalesOrderSlaveTradesOrderFinishDateTime;
PromotionId
PromotionEngineType
CrmSalesOrderSlaveId
this.OrderTypeDefEnum = CrmOrderSourceTypeDefEnum.Others;



## GetRecycleDataAsync


#### Ecom

SalesOrderSlave + SalesOrder + SalesOrderGroup

OrderCode = salesOrderGroup.SalesOrderGroupTradesOrderGroupCode,
DdbKeyOrderCode = salesOrderGroup.SalesOrderGroupTradesOrderGroupCode,
OrderSlaveStatus = salesOrderSlave.SalesOrderSlaveStatusDef,
OrderSlaveCode = salesOrderSlave.SalesOrderSlaveTradesOrderSlaveCode,
MemberId = salesOrderGroup.SalesOrderGroupMemberId,
IsGift = salesOrderSlave.SalesOrderSlaveIsGift,
IsMajor = salesOrderSlave.SalesOrderSlaveIsMajor,
IsSalePageGift = salesOrderSlave.SalesOrderSlaveIsSalePageGift,
OrderCreateDateTime = salesOrderGroup.SalesOrderGroupDateTime


#### Others

shopId + returnCrmSalesOrderSlaveId (退單本人)

CrmSalesOrderSlaves + CrmMembers


CrmSalesOrderId = originalCrmSalesOrderSlave.CrmSalesOrderSlaveCrmSalesOrderId,
ReturnCrmSalesOrderId = returnCrmSalesOrderSlave.CrmSalesOrderSlaveCrmSalesOrderId,
NineYiMemberId = crmMember.CrmMemberNineYiMemberId,
CrmMemberId = crmMember.CrmMemberId,
ReturnCrmSalesOrderSlaveId = returnCrmSalesOrderSlave.CrmSalesOrderSlaveId,
OuterOrderSlaveCode1 = originalCrmSalesOrderSlave.CrmSalesOrderSlaveOuterOrderSlaveCode1,
OuterOrderSlaveCode2 = originalCrmSalesOrderSlave.CrmSalesOrderSlaveOuterOrderSlaveCode2,
OuterOrderSlaveCode3 = originalCrmSalesOrderSlave.CrmSalesOrderSlaveOuterOrderSlaveCode3,
OuterOrderSlaveCode4 = originalCrmSalesOrderSlave.CrmSalesOrderSlaveOuterOrderSlaveCode4,
OuterOrderSlaveCode5 = originalCrmSalesOrderSlave.CrmSalesOrderSlaveOuterOrderSlaveCode5,
OuterOrderSlaveCode6 = originalCrmSalesOrderSlave.CrmSalesOrderSlaveOuterOrderSlaveCode6,
OriginalCrmSalesOrderId = returnCrmSalesOrderSlave.CrmSalesOrderSlaveOriginalCrmSalesOrderId,
OriginalCrmSalesOrderSlaveId = returnCrmSalesOrderSlave.CrmSalesOrderSlaveOriginalCrmSalesOrderSlaveId,
ReturnQty = returnCrmSalesOrderSlave.CrmSalesOrderSlaveQty,
ReturnPrice = returnCrmSalesOrderSlave.CrmSalesOrderSlavePrice,
ReturnTotalPayment = returnCrmSalesOrderSlave.CrmSalesOrderSlaveTotalPayment,



OriginalCrmSalesOrderId = crmSalesOrderSlave.CrmSalesOrderId,
OriginalCrmSalesOrderSlaveId = crmSalesOrderSlave.OriginalCrmSalesOrderSlaveId,
CrmSalesOrderId = crmSalesOrderSlave.ReturnCrmSalesOrderId,
CrmSalesOrderSlaveId = taskData.CrmSalesOrderSlaveId,
OrderCode = CrmSalesOrderOuterOrderCode1~5 用底線串起
DdbKeyOrderCode = $"{OthersCrmSalesOrderPromotionRewardCalculateService.CrmSalesOrderCodePrefix}{crmSalesOrderSlave.CrmSalesOrderId}",
OrderSlaveCode = OuterOrderSlaveCode1~6 用底線串起,
OrderSlaveStatus = nameof(SalesOrderSlaveStatusDefEnum.Finish),
MemberId = crmSalesOrderSlave.NineYiMemberId,
IsGift = false,
IsMajor = true,
IsSalePageGift = false,
LocationName = locationInfo?.LocationName


## ProcessRecycleDataAsync

#### getRuleRecord


#### IsPromotionRewardCalculateSwitchAsync (看重算)

#### IsValidPromotionSaleChannel (檢查通路)

**Ecom**

promotion.Rule.MatchedSalesChannels = Web / AppIOS / AppAndroid

**Others**

LocationWizard / InStore

#### IsGift / IsSalePageGift / IsMajor (檢查贈品)


#### 取得 Record


#### IsDdbDataNeedToRecycle


**Ecom**

檢查主單
CheckSalesOrderCheckIsValidAsync

Valid / Invalid / Checking

檢查子單

無 detail &　Unmatch

**Others**

無 record
無 detail &　Unmatch


####　IsOrderStatusCorrectAsync (檢查訂單狀態是否正確)


**Ecom**

Event = OrderCancelled / OrderSlaveStatus = SalesOrderSlaveStatusDefEnum.Cancel
Event = OrderReturned / OrderSlaveStatus = Finish


**Others**

線下訂單匯入的時候 = 訂單處理完成
todo: 是否需要確認退貨Qty、totalPayment 是否與原勾稽資料相同?

true



#### 檢查 MemberId



#### FilterPromotionRewardDetail (取得/過濾需要回收點數的 DDB資料)

**Ecom**

i => i.TradesOrderSlaveCode == taskData.TSCode


**Others**

i => i.CrmSalesOrderSlaveId == orderData.OriginalCrmSalesOrderSlaveId



## 主單 Reward, 子單 NoReward || Recycle || Cancel

為不給點狀態，取消回收

## 主單 Reward, 子單 Reward

開始執行{record.PromotionEngineId_TradesOrderGroupCode}退點


#### ProcessRecyclePointAsync

線下 TG : CrmSalesOrder:200160020
線下 TS : 來自 orderSlave.OrderSlaveCode (OrderSlaveEntity) => OuterOrderSlaveCode1~6組成


loyaltyPointApiRequest


VipMemberId = shopMemberCode,
ShopMemberCode = shopMemberCode,
ShopId = shopId,
OrderId = this.GenerateCode(orderData.OrderCode, record.PromotionEngineId), //// orderData.OrderCode 就是 TG Code , 線下 : CrmSalesOrderOuterOrderCode1~5底線組成, 這邊最後會 OrderCode|PromotionId
EventType = nameof(LoyaltyPointEventTypeEnum.Add)


取得TransactionCode

找不到會 => 查無點數中心給點紀錄


**檢查是否已經有還點紀錄** 


GetRecycleOrderIdAsync

線上訂單格式：{tgCode}|{promotionId}|{serialNumber}
門市訂單格式：{orderCode}|{promotionId}_{transactionCode}|{serialNumber}

=> 已有回收紀錄，不須進行回收點數
=> InsufficientPoints
=> 會員點數餘額為0點數中心也不會有Api扣點紀錄
=> 點數中心 API 與 DDB 回收點數紀錄資料不對稱

**檢查tsRewardDetail 狀態**

Status != nameof(RewardStatusEnum.Reward)

=> 給點子單狀態異常

**線上訂單回收點數不能包含在攤提清單中**

amor => amor.TradesOrderSlaveCode == orderData.OrderSlaveCode

=> {orderTypeDescription}還點異常，訂單不應被納入重算：{orderData.OrderSlaveCode}

**recyclePoint**


== 0

應退點數為0點，不打點數中心，純紀錄DDB

發動 點鐘recycle


餘額不足紀錄
recycleResult.ResultCode == "api_0000" || recycleResult.ResultCode == "api_0021"
記 s3RecordData
firstTsDetail.InsufficientPoints = s3RecordData.InsufficientPoints

會員點數餘額為0
firstTsDetail.InsufficientPoints = recyclePoint


最後壓主(RecyclePoints)子單(tsRewardDetail 要被退的單 detail)


## 主單 Unmatch / Cancel / MatchWithoutQuota


## 主單 WaitToReward / Occupy

重算
記退單


**record.TotalLoyaltyPoint <= recyclePoint**

RewardStatus = Cancel
TotalLoyaltyPoint = 0

detail
Status = Cancel
釋放佔額


部分點數需回收 -> 扣除後更新
record.TotalLoyaltyPoint -= recyclePoint



