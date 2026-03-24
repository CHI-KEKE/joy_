## 線下處理邏輯

線下會 by Shop 一次撈昨日等級計算訂單，匹配活動觸發回饋

**時間範圍：** 前一天 0:00(含) ~ 當天 0:00 (CrmSalesOrderSlaveCalculateMemberTierDateTime)
**訂單時間：** CrmSalesOrderTradesOrderFinishDateTime
**活動起始時間：** StartDateTime
**活動結束時間：** EndDateTime

<br>

## NormalOrders 定義

**PurchaseType = Normal**
- TypeMemos = Normal, Others, Other, Ignored, Gift
- CrmSalesOrderSlaveOriginalCrmSalesOrderSlaveId = 0

**PurchaseType = Return**
- TypeMemos = Coupon, Point
- CrmSalesOrderSlaveOriginalCrmSalesOrderSlaveId != 0
**TypeDef = Others**

<br>

## returnOrderSlaves 定義

**PurchaseType = Normal**
TypeMemos = Normal, Others, Other, Ignored, Gift, 空

**PurchaseType = Return**
TypeMemos = Coupon, Point

**TypeDef = Others**

且有帶 CrmSalesOrderSlaveOriginalCrmSalesOrderSlaveId 者

<br>

## filteredReturnOrderSlaves 定義

returnOrderSlaves 排除存在在今日正向單的負向子單

## RelatedOrderSlaveIdList 定義

不同單但同天勾稽

<br>

## 勾稽派發邏輯

- 同一主單有正負向 => 在正流程一起處理掉
- 正負不同主單同一天等級計算 => 帶在 RelatedOrders 走正流程一起計算
- 純負向單 => 走逆流程
- 單獨純在的未勾稽的負向子單 => 不會被納入任何計算

<br>

## 測試資料

```json
{"Data":"{\"Id\":\"834ab1a6-2342-43f4-8155-d64ccf2edbcf\",\"IdempotencyKey\":\"09547d1b-13b5-444d-9928-b10d753fd221\",\"EventName\":\"Internal_MemberTierCalculateFinished\",\"EventArgs\":{\"Market\":\"HK\",\"ShopId\":125},\"Source\":{\"ShopId\":125,\"CalculateDate\":\"2025-06-27T16:00:00+00:00\",\"CreatedBy\":\"NineYi.OSMPlus\"},\"SourceType\":\"All\",\"SourceKey\":\"125\",\"CreatedAt\":\"2025-06-27T22:33:07.8911785+00:00\"}"}
```

```json
{"Data":"{\"Id\":\"1238c820-dceb-4fd4-9cc5-1977ca55c9f5\",\"IdempotencyKey\":\"2fe24eb8-8099-45fe-b60d-e5b7a5c68325\",\"EventName\":\"Internal_MemberTierCalculateFinished\",\"EventArgs\":{\"Market\":\"HK\",\"ShopId\":2},\"Source\":{\"ShopId\":2,\"CalculateDate\":\"2025-07-01T16:00:00+00:00\",\"CreatedBy\":\"NineYi.OSMPlus\"},\"SourceType\":\"All\",\"SourceKey\":\"2\",\"CreatedAt\":\"2025-07-02T09:33:09.1194845+00:00\"}"}
```