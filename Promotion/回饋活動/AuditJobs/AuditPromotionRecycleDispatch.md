
```json
{
  "ExecuteTime": "2025-12-27T06:05:16.6890263+08:00"
}
```

- IsCrmOthersOrder 判定線下線上

## 線上訂單

- 每小時 01 分
- ExecuteTime 撈前兩2hr + 往後 1hr
- CancelOrderSlave : CancelOrderSlave_UpdatedDateTime
- ReturnGoodsOrderSlave : ReturnGoodsOrderSlaveStatusUpdatedDateTime
- by order 建立 `AuditPromotionRecycleLoyaltyPointsJob` / `AuditPromotionRecycleCouponJob`

## 線下訂單

- 撈前一天等級計算
- 有勾到子單的 returnOrderSlave 一個個派出
- by order 建立 `AuditCrmOthersOrderPromotionRecycleLoyaltyPointsJob` / `AuditCrmOthersOrderPromotionRecycleCouponJob`