# 回收 Jobs 處理指南

## 目錄
1. [PromotionRecycleCouponJob](#1-promotionrecyclecouponjob)

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

<br><br>