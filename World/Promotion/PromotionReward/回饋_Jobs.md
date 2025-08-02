# 回饋 Jobs 文件

## 目錄
1. [PromotionRewardCoupon](#promotionrewardcoupon)
2. [PromotionRewardLoyaltyPointsV2](#promotionrewardloyaltypointsv2)
3. [ValidTypeMemos](#validtypememos)
4. [線下訂單資料流程](#4-線下訂單資料流程)
5. [開關](#5-開關)

<br>

---

## PromotionRewardCoupon

### 餵資料測試

以下是 PromotionRewardCoupon 的測試資料範例：

<br>

**範例 1：基本訂單資料**
```json
{"Data":"{\"ShopId\":11,\"TradesOrderGroupCode\":null,\"CrmSalesOrderId\":329444,\"OrderDateTime\":\"2025-05-22T18:30:00\",\"PromotionEngineId\":6886,\"PromotionEngineType\":\"RewardReachPriceWithCoupon\",\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\"}"}
```

<br>

**範例 2：包含會員資料**
```json
{"Data":"{\"ShopId\":11,\"TradesOrderGroupCode\":null,\"CrmSalesOrderId\":329444,\"OrderDateTime\":\"2025-05-22T18:30:00\",\"PromotionEngineId\":6886,\"PromotionEngineType\":\"RewardReachPriceWithCoupon\",\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\",\"MemberId\":123,\"S3Key\":\"abc\"}"}
```

<br>

**範例 3：電商訂單**
```json
{"Data":"{\"ShopId\":2,\"TradesOrderGroupCode\":\"TG250625T00005\",\"CrmSalesOrderId\":0,\"OrderDateTime\":\"2025-06-25T17:45:52.8943886+08:00\",\"PromotionEngineId\":7307,\"PromotionEngineType\":null,\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"ECom\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

---

## PromotionRewardLoyaltyPointsV2

### 餵資料測試

以下是 PromotionRewardLoyaltyPointsV2 的測試資料範例：

<br>

**範例 1：電商訂單資料**
```json
{"Data":"{\"ShopId\":2,\"TradesOrderGroupCode\":\"TG250627P00006\",\"CrmSalesOrderId\":0,\"OrderDateTime\":\"2025-06-27T13:47:44.9064825+08:00\",\"PromotionEngineId\":6137,\"PromotionEngineType\":null,\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"ECom\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

**範例 2：CRM 銷售訂單**
```json
{"Data":"{\"ShopId\":2,\"TradesOrderGroupCode\":\"CrmSalesOrder:329599\",\"CrmSalesOrderId\":329599,\"OrderDateTime\":\"2025-06-30T17:00:00+08:00\",\"PromotionEngineId\":7371,\"PromotionEngineType\":null,\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

**範例 3：電商訂單（商店 11）**
```json
{"Data":"{\"ShopId\":11,\"TradesOrderGroupCode\":\"TG250629S00001\",\"CrmSalesOrderId\":0,\"OrderDateTime\":\"2025-06-29T16:58:04.8305713+08:00\",\"PromotionEngineId\":7366,\"PromotionEngineType\":null,\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"ECom\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

**範例 4：簡化訂單資料**
```json
{"Data":"{\"ShopId\":2,\"CrmSalesOrderId\":329596,\"OrderDateTime\":\"2025-06-29T18:00:00.000\",\"PromotionEngineId\":7371,\"OrderTypeDefEnum\":\"Others\"}"}
```

<br>

**範例 5：點數回饋訂單**
```json
{"Data":"{\"ShopId\":2,\"CrmSalesOrderId\":329596,\"OrderDateTime\":\"2025-06-29T18:00:00.000\",\"PromotionEngineId\":7371,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

**範例 6：點數回饋訂單（訂單 329599）**
```json
{"Data":"{\"ShopId\":2,\"CrmSalesOrderId\":329599,\"OrderDateTime\":\"2025-06-30T17:00:00.000\",\"PromotionEngineId\":7371,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

**範例 7：點數回饋訂單（訂單 329601）**
```json
{"Data":"{\"ShopId\":2,\"CrmSalesOrderId\":329601,\"OrderDateTime\":\"2025-06-30T20:00:00.000\",\"PromotionEngineId\":7398,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\",\"RelatedOrderSlaveIdList\":null}"}
```

<br>

**範例 8：包含關聯訂單**
```json
{"Data":"{\"ShopId\":10230,\"CrmSalesOrderId\":405587,\"OrderDateTime\":\"2025-07-10T22:30:00\",\"PromotionEngineId\":31207,\"PromotionEngineType\":\"RewardReachPriceWithPoint2\",\"RewardFlowEnum\":\"Reward\",\"OrderTypeDefEnum\":\"Others\",\"RelatedOrderSlaveIdList\":[993236]}"}
```

<br>

---

## ValidTypeMemos

### 有效類型備註清單

以下是系統支援的有效類型備註：

<br>

- **Normal** - 一般訂單
- **Others** - 其他類型
- **Other** - 其他
- **Ignored** - 忽略的訂單
- **Empty** - 空訂單
- **Gift** - 贈品訂單
- **Coupon** - 優惠券訂單
- **Point** - 點數訂單

<br>

---

## 4. 線下訂單資料流程

### 4.1 取得 RuleRecord

**API：** `api/promotion-rules/rule-record`

<br>

**時間：** CrmSalesOrderTradesOrderFinishDateTime

<br>

**邏輯：** 找到活動在訂單結束前最接近的一次 record

<br>

### 4.2 排除非支援線下通路活動

promotion.Rule.MatchedSalesChannels 確認有包含 LocationWizard or InStore

<br>

### 4.3 準備訂單資料

#### 取得資料
CrmSalesOrders / CrmSalesOrderSlaves Table

<br>

#### TypeMemo
Normal / Others / Other / Ignored 目前都包

<br>

#### 篩選
order.PurchaseType = Normal

<br>

#### 資料的組織

| 欄位名稱 | 對應值 |
|---------|-------|
| **OrderSlaveId** | CrmSalesOrderSlaveId |
| **OrderSlaveCode** | OuterOrderSlaveCode1 ~ 6 用底線串起來 |
| **OrderGroupCode** | CrmSalesOrder:CrmSalesOrderId |
| **CrmSalesOrderGroupJoinCode** | OuterOrderSlaveCode1 ~ 5 用底線串起來 |
| **CrmSalesOrderSlaveCode1** | OuterOrderSlaveCode1 |
| **OrderStatus** | Finish |
| **OrderSlaveStatusDef** | Finish |
| **OrderSlaveStatusUpdatedDateTime** | CrmSalesOrderTradesOrderCreateDateTime |
| **MemberId** | CrmMemberId |
| **CrmShopMemberCardId** | CrmShopMemberCardId |
| **TrackSourceTypeDef** | TrackSourceTypeDef |
| **SkuId** | 0 |
| **SalepageId** | 0 |
| **OuterId** | OuterProductSkuCode |
| **Qty** | 篩選 return 資料 |
| **Price** | Price |
| **SuggestPrice** | 0 |
| **TotalPayment** | 篩選 return 資料 |
| **IsGift** | false |
| **IsSalePageGift** | false |
| **IsMajor** | true |
| **OrderGroupDateTime** | CrmSalesOrderTradesOrderCreateDateTime |
| **ShopId** | ShopId |
| **OrderId** | CrmSalesOrderId |
| **SalesOrderSlaveItemType** | '' |
| **SalesOrderSlaveItemGroup** | '' |
| **IsCalculatePoint** | IsCalculatePoint |
| **LocationId** | LocationId |

<br>

### 4.4 準備會員資料

| 欄位名稱 | 對應值 |
|---------|-------|
| **VipMemberId** | memberData.NineYiVipMemberId |
| **MemberId** | memberData.NineYiMemberId |
| **OuterId** | memberData.NineYiMemberCode |

<br>

### 4.5 組成菜籃計算參數

#### User 設定
- **Id** = memberInfo.MemberId
- **Tags** = ["AllUserScope","CrmShopMemberCard:{crmShopMemberCardId}"]
- **OuterId** = memberInfo.OuterId

<br>

#### Promotion 設定
- **PromotionSourceType** = Promotion
- **PromotionRules** (但只有一個)
  - Id = PromotionEngineId
  - Rule = RuleJson
  - Type = PromotionEngineType
- **PromotionRuleIds** (但只有一格) = PromotionEngineId

<br>

#### 其他參數
- **Channel** = InStore
- **CurrencyDecimalDigits** = _N1CONFIG:CurrencyDecimalDigits

<br>

#### SalepageSkuList 處理
1. 檢查 ShopStaticSetting 是否有設定 LoyaltyPoints / IsCalculateRewardPoint
2. 攤 orderSlave 到 qty 上，一個就是一個 sku
3. 每一筆都有自己的：
   - SalepageId
   - SkuId
   - Price
   - Qty = 1
   - Flags
   - OuterId = OuterId
   - SuggestPrice = SuggestPrice
   - OptionalTypeDef = ""
   - OptionalTypeId = 0
   - PointsPayPair = null
   - CartExtendInfoItemType = 'TradesOrderSlaveId'
   - CartExtendInfoItemGroup = orderSlave.OrderSlaveId
4. 但 basePrice == 0 的話 Qty = 0
5. 攤完後用 crmSalesOrderGroupJoinCode + crmSalesOrderSlaveCode1 當 key 值幫 slaves 大到小排序

<br>

#### 其他選項
- **Options** = IncludeRecordDetail = true
- **CalculateDateTime** = OrderDateTime
- **Shipping** = LocationId = firstSlave.LocationId

<br>

### 4.6 菜籃計算

**API：** `api/basket-calculate`

<br>

### 4.7 結果整理

#### 總給點
totalLoyaltyPoint = calculateResult.PromotionRecordList.Sum(x => x.Point);

<br>

#### 各TS攤提結果
amortizationInfos = this.Amortization(orderSlaves, calculateResult);

<br>

**條件：** PromotionRecordDetail.SerialNumber = OrderSlaveId

**排序：** order asc : PromotionRecordDetail.PurchasedItemId

**資料回傳：** TradesOrderSlaveCode / Seq / LoyaltyPoint / IsRewardPoint

<br>

#### 點數效期
periodType = promotion.Rule.PeriodType

<br>

#### 給點天數
```csharp
var (salesOrderSlaveStatusDef, bookingTime, rewardDays) = this.GetRewardConditionSetting(promotion.RewardPointConditions, orderTime);
salesOrderSlaveStatusDef = Finish
bookingTime = DateTime.Now.AddMinutes(-1)
rewardDays = 0
```

<br>

#### 回傳資料結構
- TradesOrderGroupCode
- PromotionEngineId
- PromotionName
- ShopId
- VipMemberId
- MemberId
- TotalLoyaltyPoint
- OrderTime
- PeriodType
- ShopMemberCardEndDateTime
- AmortizationInfos
- RewardDays = 0
- RewardSalesOrderSlaveStatusDef = Finish
- BookingTime = DateTime.Now.AddMinutes(-1)
- IsMatch
- TotalPayment

<br>

---

## 5. 開關

### 計算給點大開關

<br>

**Group:** LoyaltyPoints

<br>

**Key:** IsCalculateRewardPoint

<br>

### 計算給券大開關

<br>

**Group:** PromotionReward

<br>

**Key:** IsCalculateRewardCoupon

<br>