
## IsValidForRecycleAsync - couponReward 沒資料

`回收活動回饋優惠券 {ddbPk} 不符合回收條件，將不進行後續處理`


#### 狀況 1

- OrderTypeDefEnum = ECom
- SalesOrder.CheckIsValid = Valid

throw new PromotionRewardFailAndReleaseLockableException("訂單已付款，但查無給券紀錄")

#### 狀況 2

- OrderTypeDefEnum = ECom
- SalesOrder.CheckIsValid = Invalid

log1 : 取消付款，不回收優惠券
log2 回收活動回饋優惠券 {ddbPk} 不符合回收條件，將不進行後續處理
回 false finish

#### 狀況 3

- OrderTypeDefEnum = ECom
- SalesOrder.CheckIsValid = Checking

log1 : 付款處理中，不回收優惠券
log2 回收活動回饋優惠券 {ddbPk} 不符合回收條件，將不進行後續處理
回 false finish


#### 其他狀況

- 不論線上線下

```csharp
//// 之前遇到因為正向單帶有勾稽單號而且還勾別人導致正向單沒被記錄到,因此逆向單找不到這個既就噴錯,後來討論後是讓他要被納入計算!!
throw new PromotionRewardFailAndReleaseLockableException($"查無訂單 DDB 給券資料 {ddbPk}, 請確認關鍵字 [原始單號帶有勾稽單號]");
```





## 準備 PromotionRewardRequestEntity

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

## 確認是否有期間活動

執行以下方法檢查活動狀態：

- `GetPromotionRuleRecordAsync`
  - OrderTime > PromotionEngineRuleRecord_PromotionEngineDateTime 最近一次
- `CheckPromotionCondition`
  - Rule.MatchedSalesChannels vs onlineOrderChannel / offlineOrderChannel

<br>








## GetOrderDataBeforeCalculate

根據 `RewardFlowEnum` 走 `GetCouponRecycleOrderData`

#### 線下訂單

```csharp
//// 以下為正向單子單整理
//// 包含 : 一般正向子單, 有勾稽的正向子單, 沒有勾稽的負向子單, 勾稽自己的負向子單
//// 排除 : 勾稽其他訂單的負向子單, 亂勾的負向單

//// 以下為負向單整理
//// relatedCrmSalesOrder = 同天不同單勾稽子單
```
```csharp
//// OrderSlaveEntity

OrderSlaveId = order.CrmSalesOrderSlaveId,
OrderSlaveCode = this.GetCrmOrderCodeString(order.OuterOrderSlaveCode1, order.OuterOrderSlaveCode2, order.OuterOrderSlaveCode3, order.OuterOrderSlaveCode4,
    order.OuterOrderSlaveCode5, order.OuterOrderSlaveCode6),
OrderGroupCode = $"{CrmSalesOrderCodePrefix}{order.CrmSalesOrderId}",
CrmSalesOrderGroupJoinCode = this.GetCrmOrderCodeString(order.OuterOrderCode1, order.OuterOrderCode2, order.OuterOrderCode3, order.OuterOrderCode4,
    order.OuterOrderCode5),
CrmSalesOrderSlaveCode1 = order.OuterOrderSlaveCode1,
OrderStatus = "Finish", //// 線下訂單都是完成的
OrderSlaveStatusDef = "Finish", //// 線下訂單都是完成的
OrderSlaveStatusUpdatedDateTime = order.CrmSalesOrderTradesOrderCreateDateTime,
CrmMemberId = order.CrmMemberId,
CrmShopMemberCardId = order.CrmShopMemberCardId,
TrackSourceTypeDef = order.TrackSourceTypeDef,
SkuId = 0, //// 線下訂單沒有 skuId
SalepageId = 0, //// 線下訂單沒有 SalepageId
OuterId = order.OuterProductSkuCode,
Qty = order.Qty,
Price = order.Price,
SuggestPrice = 0, //// 線下訂單沒有建議價格
TotalPayment = order.TotalPayment,
//// 線下訂單即使PurchaseType 如果是 Gift 也需要帶入計算
IsGift = false,
IsSalePageGift = false,
IsMajor = true,
OrderGroupDateTime = order.CrmSalesOrderTradesOrderCreateDateTime,
ShopId = order.ShopId,
OrderId = order.CrmSalesOrderId,
SalesOrderSlaveItemType = string.Empty, //// 線下訂單沒有商品頁群組
SalesOrderSlaveItemGroup = 0, //// 線下訂單沒有商品頁群組
IsCalculatePoint = order.IsCalculatePoint,
IsCalculateCoupon = order.IsCalculateCoupon,
LocationId = order.LocationId

//// + relatedCrmSalesOrderDbData
```


#### 線上訂單

- OrderPlaced : WebStore
- OrderCreated : ERP

```csharp
//// OrderSlaveEntity

OrderSlaveId = salesOrderSlave.SalesOrderSlaveTradesOrderSlaveId,
OrderGroupCode = orderSlaveFlow.OrderSlaveFlowTradesOrderGroupCode,
OrderSlaveCode = orderSlaveFlow.OrderSlaveFlowTradesOrderSlaveCode,
OrderStatus = orderSlaveFlow.OrderSlaveFlowStatusDef,
MemberId = orderSlaveFlow.OrderSlaveFlowMemberId,
CrmShopMemberCardId = salesOrderGroup.SalesOrderGroupCrmShopMemberCardId ?? 0,
TrackSourceTypeDef = salesOrderGroup.SalesOrderGroupTrackSourceTypeDef,
SalepageId = salesOrderSlave.SalesOrderSlaveSalePageId,
SkuId = salesOrderSlave.SalesOrderSlaveSaleProductSkuid,
OuterId = salesOrderSlave.SalesOrderSlaveSaleProductSkuouterId,
Qty = salesOrderSlave.SalesOrderSlaveQty,
Price = salesOrderSlave.SalesOrderSlavePrice,
SuggestPrice = salesOrderSlave.SalesOrderSlaveSuggestPrice ?? 0,
TotalPayment = salesOrderSlave.SalesOrderSlaveTotalPayment ?? 0,
OrderSlaveStatusDef = salesOrderSlave.SalesOrderSlaveStatusDef,
OrderSlaveStatusUpdatedDateTime = salesOrderSlave.SalesOrderSlaveStatusUpdatedDateTime,
IsGift = salesOrderSlave.SalesOrderSlaveIsGift,
IsMajor = salesOrderSlave.SalesOrderSlaveIsMajor,
IsSalePageGift = salesOrderSlave.SalesOrderSlaveIsSalePageGift ?? false,
OrderGroupDateTime = salesOrderGroup.SalesOrderGroupDateTime,
ShopId = salesOrderGroup.SalesOrderGroupShopId ?? 0,
OrderId = salesOrderSlave.SalesOrderSlaveSalesOrderId,
SalesOrderSlaveItemType = salesOrderSlave.SalesOrderSlaveItemType,
SalesOrderSlaveItemGroup = salesOrderSlave.SalesOrderSlaveItemGroup,
IsProcessRecycle = orderSlaveFlow.OrderSlaveFlowCancelOrderSlaveId.HasValue ||
                    (orderSlaveFlow.OrderSlaveFlowReturnGoodsOrderSlaveIsClosed.GetValueOrDefault() &&
                    orderSlaveFlow.OrderSlaveFlowReturnGoodsOrderSlaveStatusDef == "Finish") ||
                    orderSlaveFlow.OrderSlaveFlowSalesOrderSlaveStatusDef == "Cancel" ||
                    orderSlaveFlow.OrderSlaveFlowSalesOrderSlaveStatusDef == "Fail",
```


## 篩選 OrderSlaveEntity 的 IsProcessRecycle

#### 線下

應皆為 False, 表示一定往下執行

#### 線上

檢查是整單取消或退貨完成，就不繼續往下執行（因為給券不回收）

<br>

## 組成 BasketCalculateRequestEntity 做菜籃計算

- `GetPromotionSalepageSku`
  - `shopStaticSetting`
    - Key `IsCalculateRewardCoupon / IsCalculateRewardPoint`
    - groupName `PromotionReward : LoyaltyPoints`

```csharp
SalepageId = orderSlave.SalepageId,
SkuId = orderSlave.SkuId,
Price = orderSlave.Qty / Math.Abs(orderSlave.Qty) * basePrice,
Qty = 1, //// Qty一定要是正數
Flags = this.GetSalepageSkuFlags(checkIsCalculateReward, promotionEngineType, orderSlave.IsCalculateCoupon, orderSlave.IsCalculatePoint),
OuterId = orderSlave.OuterId,
SuggestPrice = orderSlave.SuggestPrice,
OptionalTypeDef = string.Empty, //// TODO: 不影響給活動給點 暫不處理
OptionalTypeId = 0, //// TODO: 不影響給活動給點 暫不處理
PointsPayPair = null, //// TODO: 不影響給活動給點 暫不處理
CartExtendInfoItemType = "TradesOrderSlaveId", //// 線下訂單還是要加上這個值才會有正確的結果
CartExtendInfoItemGroup = orderSlave.OrderSlaveId

if (basePrice == 0)
{
salepageSkus.Add(new SalepageSkuEntity
{
    SalepageId = orderSlave.SalepageId,
    SkuId = orderSlave.SkuId,
    Price = basePrice,
    Qty = 0,
    Flags = this.GetSalepageSkuFlags(checkIsCalculateReward, promotionEngineType, orderSlave.IsCalculateCoupon, orderSlave.IsCalculatePoint),
    OuterId = orderSlave.OuterId,
    SuggestPrice = orderSlave.SuggestPrice,
    OptionalTypeDef = string.Empty, //// TODO: 不影響給活動給點 暫不處理
    OptionalTypeId = 0, //// TODO: 不影響給活動給點 暫不處理
    PointsPayPair = null, //// TODO: 不影響給活動給點 暫不處理
    CartExtendInfoItemType = "TradesOrderSlaveId", //// 線下訂單還是要加上這個值才會有正確的結果
    CartExtendInfoItemGroup = orderSlave.OrderSlaveId
});
}

//// GetSalepageSkuFlags

promotionEngineType == PromotionEngineTypeDefEnum.RewardReachPriceWithCoupon && isCalculateCoupon == false
=> FlagConstants.CouponExcludedByOrder

if ((promotionEngineType == PromotionEngineTypeDefEnum.RewardReachPriceWithPoint2 ||
        promotionEngineType == PromotionEngineTypeDefEnum.RewardReachPriceWithRatePoint2) &&
    isCalculatePoint == false)

=> FlagConstants.PointExcludedByOrder


//// 排序
    var sortKey = this.CrmSalesOrderSlaveSortKey(orderSlave.CrmSalesOrderGroupJoinCode, orderSlave.CrmSalesOrderSlaveCode1);
    if (sortKeySalePageDataMapping.TryAdd(sortKey, salepageSkus) == false)
    {
        sortKeySalePageDataMapping[sortKey].AddRange(salepageSkus);
    }
}

//// 線上訂單是由購物車加入順序來排序，線下訂單是由訂單子單序號排序，在攤體的時候要反過來牌才對
List<SalepageSkuEntity> result = sortKeySalePageDataMapping
.OrderByDescending(i => i.Key)
.SelectMany(i => i.Value)
.ToList();


//// remainder 攤提
var remainder = orderSlave.TotalPayment - (basePrice * orderSlave.Qty);

//// 攤提餘數
var minPrice = (decimal)Math.Pow(10, -digits);
var index = 0;
while (remainder > 0)
{
    var sku = salepageSkus[index];
    sku.Price += minPrice;
    remainder -= minPrice;
    index++;
    index = index >= salepageSkus.Count ? 0 : index; //// 超過 Count 就攤下一輪
}
```

<br>

## ConvertLoyaltyPointsResultEntity

#### AmortizeLoyaltyPoints

- loop orderSlaves
  - 看 calculateResult.PromotionRecordDetailList 內的 item.SerialNumber == orderSlave.OrderSlaveId 的
  - order by PurchasedItemId
  - 這個 篩選且排序後的集合再 select 成 PromotionRewardLoyaltyPointsAmortizationEntity

- 有中且回 details 的
```csharp
TradesOrderSlaveCode = orderSlave.OrderSlaveCode,
Seq = seq,
OrderSlaveStatusDef = orderSlave.OrderSlaveStatusDef,
OrderSlaveStatusUpdatedDateTime = orderSlave.OrderSlaveStatusUpdatedDateTime,
LoyaltyPoint = detail.Point,
IsRewardPoint = true,
CrmSalesOrderSlaveId = orderSlave.OrderSlaveId
```
- 沒中的
```csharp
TradesOrderSlaveCode = orderSlave.OrderSlaveCode,
Seq = 0,
CrmSalesOrderSlaveId = orderSlave.OrderSlaveId,
OrderSlaveStatusDef = orderSlave.OrderSlaveStatusDef,
OrderSlaveStatusUpdatedDateTime = orderSlave.OrderSlaveStatusUpdatedDateTime,
LoyaltyPoint = 0,
IsRewardPoint = false
```

- 全部重排 Seq
```csharp
//// 依據 TradesOrderSlaveCode 分組, 處理 seq
var orderSlaveCodeGroups = result.GroupBy(x => x.TradesOrderSlaveCode);
foreach (var group in orderSlaveCodeGroups)
{
    group.Select((entity, index) =>
    {
        entity.Seq = index;
        return entity;
    }).ToList();
}
```

## GetRewardConditionSetting

#### Ecom - 指定日期

```csharp
condition.RewardTimingType == nameof(RewardTimingTypeEnum.ByDateTime)
condition.RewardDateTime.HasValue

=> bookingTime = condition.RewardDateTime.Value;
```

#### Ecom - 立即給

```csharp
condition.StatusDef == SalesOrderSlaveStatusDefEnum.WaitingToShipping

=> bookingTime = orderTime;
```

#### Ecom - N 天後

```csharp
bookingTime = days > 0 ? orderTime.Date.AddDays(days) : orderTime.AddDays(days)
```

#### Others - 指定日期

```csharp
crmOrderCondition.RewardTimingType == nameof(RewardTimingTypeEnum.ByDateTime)
rewardDateTime = crmOrderCondition.RewardDateTime.Value;
```

#### Others - N 天後

```csharp
rewardDays > 0

rewardDateTime = orderTime.Date.AddDays(rewardDays);
```

#### Others - 未來訂單

```csharp
//// 未來訂單要立即給點
rewardDateTime = orderTime > DateTime.Now ? DateTime.Now : orderTime.AddDays(rewardDays);
```

## 比對計算結果，更新 主子表 DDB

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



## 券回收找不到資料 原因是原始子單本身也有勾稽資料 

- 要把這種單也加入回饋
- 回收時要 log 下來