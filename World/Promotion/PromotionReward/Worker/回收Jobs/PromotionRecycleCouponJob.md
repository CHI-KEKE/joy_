## 1. PromotionRecycleCouponJob


#### IsValidForRecycleAsync

`回收活動回饋優惠券 {ddbPk} 不符合回收條件，將不進行後續處理`

- couponReward 沒資料 & OrderTypeDefEnum = ECom & SalesOrder.CheckIsValid = Valid => throw new PromotionRewardFailAndReleaseLockableException("訂單已付款，但查無給券紀錄")

- couponReward 沒資料 & OrderTypeDefEnum = ECom & SalesOrder.CheckIsValid = Invalid =>

log1 : 取消付款，不回收優惠券
log2 回收活動回饋優惠券 {ddbPk} 不符合回收條件，將不進行後續處理
回 false finish

- couponReward 沒資料 & OrderTypeDefEnum = ECom & SalesOrder.CheckIsValid = Checking =>

log1 : 付款處理中，不回收優惠券
log2 回收活動回饋優惠券 {ddbPk} 不符合回收條件，將不進行後續處理
回 false finish



- 其他情況會 => throw new PromotionRewardFailAndReleaseLockableException($"查無訂單 DDB 給券資料 {ddbPk}")




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

### 1.6 比對計算結果，更新 主子表 DDB

#### 數量一致的情況

`calculateResult.TotalCouponQty == couponReward.TotalCouponQty`

仍需更新以記錄退貨單


#### 優惠券數量不一致，更新 DDB 資料

一定會複寫
couponReward.TotalCouponQty = calculateResult.TotalCouponQty

`calculateResult.TotalCouponQty == 0 && couponReward.GivenCouponQty == 0`

逆流程重新計算後優惠券應給數量為 0, 且目前未給任何優惠券，狀態更新為 {nameof(RewardDetailStatusEnum.Cancel)}


`calculateResult.TotalCouponQty <= couponReward.GivenCouponQty`

逆流程重新計算後優惠券應給數量為 {calculateResult.TotalCouponQty}，目前已給優惠券數量為 {couponReward.GivenCouponQty}，已不須補發，狀態更新為 {nameof(RewardDetailStatusEnum.Reward)}

<br>


### 1.7 比對計算結果，更新道具表 DDB


#### 重算後應給券數量變少 & calculateResult.TotalCouponQty > couponReward.GivenCouponQty

更新取消數量 cancelledQty

正流程還會繼續給券 
cancelledQty = originalCouponQty - calculateResult.TotalCouponQty


#### 重算後應給券數量變少 & calculateResult.TotalCouponQty < couponReward.GivenCouponQty

更新取消數量 cancelledQty

正流程不繼續給券 
cancelledQty = originalCouponQty - couponReward.GivenCouponQty

---


#### 重算後應給券數量變多

重算後應給券數量變多, 則更新應給數量
ShouldGiveCouponQty = calculateResult.TotalCouponQty - originalCouponQty

