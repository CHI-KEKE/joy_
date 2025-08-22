# 回收 Jobs 處理指南

## 目錄
1. [PromotionRecycleCouponJob](#1-promotionrecyclecouponjob)
2. [RecycleLoyaltyPointsV2](#2-recycleloyaltypointsv2)
3. [PromotionRecycleOrderDataEntity](#3-promotionrecycleorderdataentity)
4. [邏輯](#4-邏輯)

---

## 1. PromotionRecycleCouponJob

### 1.1 準備 PromotionRewardRequestEntity

```csharp
var promotionRequest = new PromotionRewardRequestEntity
{
    //// 原始主單
    CrmSalesOrderId = request.OriginalCrmSalesOrderId,
    ShopId = request.ShopId,

    //// DdbKeyOrderCode : CrmSalesOrder:CrmSalesOrderId
    TradesOrderGroupCode = request.TradesOrderGroupCode,
    PromotionEngineId = request.PromotionEngineId,
    OrderDateTime = couponReward.OrderTime,

    //// RewardFlow 設定為 Recycle
    RewardFlowEnum = RewardFlowEnum.Recycle,
    PromotionEngineType = request.PromotionEngineType,

    //// 將DDB Detail紀錄的子單 Id 全部傳入
    RelatedOrderSlaveIdList = couponDetails
    .Select(detail => detail.CrmSalesOrderSlaveId)
    .Where(x => x.HasValue)
    .Select(x => x.Value)
    .ToList(),
};

//// 將逆向單本人的子單 Id 也傳入
promotionRequest.RelatedOrderSlaveIdList.Add(request.CrmSalesOrderSlaveId);
```

<br>

### 1.2 確認是否有期間活動

執行以下方法檢查活動狀態：

- `GetPromotionRuleRecordAsync`
- `CheckPromotionCondition`

<br>

### 1.3 GetOrderDataBeforeCalculate

根據 `RewardFlowEnum` 走 `GetCouponRecycleOrderData`

**crmSalesOrderDbData：** 過濾掉勾稽不是自己的訂單 => 投射成一票 OrderSlaveEntity List

**relatedCrmSalesOrderDbData：** 有勾稽這張主單的負向子單 => 整合到剛剛的 OrderSlaveEntity List

<br>

### 1.4 篩選 OrderSlaveEntity 的 IsProcessRecycle

**線下：** 應皆為 False, 表示一定往下執行

**線上：** 檢查是整單取消或退貨完成，就不繼續往下執行（因為給券不回收）

<br>

### 1.5 組成 BasketCalculateRequestEntity 做菜籃計算

執行以下流程：

- `GetPromotionSalepageSku`
- `shopStaticSetting`

**Key：** `IsCalculateRewardCoupon : IsCalculateRewardPoint`

**groupName：** `PromotionReward : LoyaltyPoints`

攤提後執行 `BasketCalculateAsync`

<br>

### 1.6 比對計算結果，更新 DDB

#### 不更新 DDB 條件
`calculateResult.TotalCouponQty == couponReward.TotalCouponQty`

<br>

#### 主表更新邏輯

**當 `calculateResult.TotalCouponQty == 0`：**
- `RewardStatus = Cancel`

**其他情況：**
- `TotalCouponQty = calculateResult.TotalCouponQty`

<br>

#### 子表更新邏輯

**當 `calculateResult.TotalCouponQty > originalCouponQty`：**
- `ShouldGiveCouponQty : + ( calculateResult.TotalCouponQty - originalCouponQty )`

**當 `calculateResult.TotalCouponQty <= originalCouponQty`：**
- `CanceledCouponQty + ( originalCouponQty - calculateResult.TotalCouponQty )`

<br>

---

## 2. RecycleLoyaltyPointsV2

### 2.1 測試資料

以下是 RecycleLoyaltyPointsV2 的測試資料範例：

<br>

**範例 1：退貨事件**
```json
{"Data":"{\"ShopId\":2,\"EventName\":\"Return\",\"TriggerDatetime\":\"2025-07-02T17:53:47.5375643+08:00\",\"OrderCreateDate\":\"2025-07-01T15:00:00.000\",\"PromotionId\":7371,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"PendingRetryCount\":1,\"CrmSalesOrderSlaveId\":828143,\"OrderTypeDefEnum\":\"Others\"}"}

{"Data":"{\"ShopId\":11,\"EventName\":\"Return\",\"TriggerDatetime\":\"2025-08-19T06:33:52.1939309+08:00\",\"OrderCreateDate\":\"2025-08-14T20:30:00\",\"PromotionId\":7954,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"PendingRetryCount\":1,\"CrmSalesOrderSlaveId\":828883,\"OrderTypeDefEnum\":\"Others\"}"}

```

<br>

**範例 2：訂單取消事件**
```json
{"Data":"{\"ShopId\":2,\"TSCode\":\"TS250627P000007\",\"EventName\":\"OrderCancelled\",\"TriggerDatetime\":\"2025-06-27T13:50:41.4426856+08:00\",\"OrderCreateDate\":\"2025-06-27T13:46:18.64\",\"PromotionId\":6541,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":0,\"OrderTypeDefEnum\":\"ECom\"}"}
```

<br>

**範例 3：訂單取消事件（活動 6137）**
```json
{"Data":"{\"ShopId\":2,\"TSCode\":\"TS250721P000003\",\"EventName\":\"OrderCancelled\",\"TriggerDatetime\":\"2025-07-21T13:49:17.5995715+08:00\",\"OrderCreateDate\":\"2025-07-21T13:45:37.183\",\"PromotionId\":6137,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":0,\"OrderTypeDefEnum\":\"ECom\"}"}
```

<br>

**範例 4：退貨事件（重試次數 4）**
```json
{"Data":"{\"ShopId\":2,\"TSCode\":null,\"EventName\":\"Return\",\"TriggerDatetime\":\"2025-06-27T11:56:17.7710489+08:00\",\"OrderCreateDate\":\"2025-06-19T08:05:00\",\"PromotionId\":6156,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"PendingRetryCount\":4,\"CrmSalesOrderSlaveId\":828088,\"OrderTypeDefEnum\":\"Others\"}"}
```

<br>

**範例 5：退貨事件（商店 10230）**
```json
{"Data":"{\"ShopId\":10230,\"TSCode\":null,\"EventName\":\"Return\",\"TriggerDatetime\":\"2025-07-12T07:16:26.3944076+08:00\",\"OrderCreateDate\":\"2025-07-10T22:30:00\",\"PromotionId\":31207,\"PromotionEngineType\":\"RewardReachPriceWithRatePoint2\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":993279,\"OrderTypeDefEnum\":\"Others\"}"}
```

<br>

**範例 6：退貨事件（商店 11）**
```json
{"Data":"{\"ShopId\":11,\"TSCode\":null,\"EventName\":\"Return\",\"TriggerDatetime\":\"2025-07-19T06:33:37.0059135+08:00\",\"OrderCreateDate\":\"2025-07-17T14:30:00\",\"PromotionId\":7532,\"PromotionEngineType\":\"RewardReachPriceWithRatePoint2\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":828289,\"OrderTypeDefEnum\":\"Others\"}"}
```

<br>

### 2.2 Task Data

#### 線下訂單
```json
{
  "ShopId": "2",
  "EventName": "Return",
  "TriggerDatetime": "DateTime.Now",
  "OrderCreateDate": "originOrderSlave.CrmSalesOrderSlaveTradesOrderFinishDateTime",
  "PromotionId": "promotion.Id",
  "PromotionEngineType": "promotion.TypeDef",
  "CrmSalesOrderSlaveId": "orderSlave.CrmSalesOrderSlaveId",
  "OrderTypeDefEnum": "CrmOrderSourceTypeDefEnum.Others"
}
```

<br>

#### 線上訂單
```json
{
	"ShopId": 2,
	"TSCode": "TS250619P000011",
	"EventName": "OrderCancelled",
	"TriggerDatetime": "2025-06-19T13:51:56.7019972+08:00",
	"OrderCreateDate": "2025-06-19T13:47:00.223",
	"PromotionId": 5338,
	"PromotionEngineType": "RewardReachPriceWithRatePoint2",
	"PendingRetryCount": 0,
	"CrmSalesOrderSlaveId": 0,
	"OrderTypeDefEnum": "ECom"
}
```

<br>

### 2.3 錯誤情境

#### 情境一：訂單狀態時序問題

**錯誤訊息：** [Error] TS250813R000004 訂單狀態Finish 不符合預期退點狀態

<br>

**原因：** 雖然是 OrderReturned event 觸發，但 ReturnGoodsOrderSlave 還沒走到 Finish

<br>

#### 情境二：線上通路判斷問題導致回饋異常

**正流程：** 無符合的線上SalesChannel，跳過回饋 => 沒有正常長 couponRecord

<br>

**造成結果：** 逆流程：訂單已付款，但查無給券紀錄 => 查 SalesOrderValid 後發現沒正常長資料噴錯

<br>

---

## 3. PromotionRecycleOrderDataEntity


- OrderCode : CrmSalesOrderOuterOrderCode1~5
- OrderSlaveCode : OuterOrderSlaveCode1~6
- OrderSlaveStatus : Finish
- MemberId : NineYiMemberId


### 實體屬性說明

| 屬性名稱 | 來源 | 說明 |
|---------|------|------|
| **OriginalCrmSalesOrderId** | `crmSalesOrderSlave.CrmSalesOrderId` | 原始主訂單 ID |
| **CrmSalesOrderSlaveId** | `taskData.CrmSalesOrderSlaveId` | 當前任務所屬的子訂單 ID |
| **OrderCode** | `crmOrderCodeStringArr.ToCrmSalesOrderCodeString()` | 主訂單代碼（可能為轉換格式） |
| **DdbKeyOrderCode** | `$"{Prefix}{crmSalesOrderSlave.CrmSalesOrderId}"` | DynamoDB 的主訂單代碼鍵 |
| **OrderSlaveCode** | `crmOrderSlaveCodeArray.ToCrmSalesOrderCodeString()` | 子訂單代碼 |
| **OrderSlaveStatus** | `nameof(SalesOrderSlaveStatusDefEnum.Finish)` | 子訂單狀態，這裡固定為完成 |
| **MemberId** | `crmSalesOrderSlave.NineYiMemberId` | 會員 ID |
| **IsGift** | `false` | 是否為贈品，固定為否 |
| **IsMajor** | `true` | 是否為主要項目，固定為是 |
| **IsSalePageGift** | `false` | 是否為銷售頁贈品，固定為否 |
| **LocationName** | `locationInfo?.LocationName` | 門市名稱，允許為 null |

<br>

---

## 4. 邏輯

### 4.1 線下訂單

執行以下處理流程：

<br>

1. **Lock** - 鎖定處理
2. **根據 OrderCreateDate 取得對應 RuleRecord** - `CrmSalesOrderSlaveTradesOrderFinishDateTime`
3. **判斷通路正確** - `LocationWizard / InStore`
4. **贈品確認** - `(IsGift, IsSalePageGift, IsMajor)`
5. **確認 DDB Record** - `IsDdbDataNeedToRecycle, RewardStatusEnum.Unmatch` 不處理回收
6. **確認訂單狀態** - N/A
7. **依照主表 & 子表狀態判斷行為**
   - 主表 + 子表 Reward 執行退點流程
   - 主表 WaitToReward 執行取消給點流程

<br>

### 4.2 線上訂單

執行以下處理流程：

<br>

1. **Lock** - 鎖定處理
2. **根據 OrderCreateDate 取得對應 RuleRecord** - `SalesOrderSlaveDateTime`
3. **判斷通路正確** - `Web / AppIOS / AppAndroid`

<br>