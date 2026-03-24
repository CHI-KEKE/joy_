
```json
{"Data":"{\"ShopId\":2,\"TSCode\":\"TS250903T000003\",\"EventName\":\"\",\"TriggerDatetime\":\"2025-09-03T19:01:30.7072732+08:00\",\"OrderCreateDate\":\"2025-09-03T17:41:45.45\",\"PromotionId\":0,\"PromotionEngineType\":\"\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":0,\"OrderTypeDefEnum\":\"ECom\",\"MemberId\":0,\"S3Key\":\"\"}"}

{"Data":"{\"ShopId\":2,\"TSCode\":\"TS250917K000001\",\"EventName\":\"\",\"TriggerDatetime\":\"2025-09-17T11:01:27.0909998+08:00\",\"OrderCreateDate\":\"2025-09-17T09:06:37.407\",\"PromotionId\":0,\"PromotionEngineType\":\"\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":0,\"OrderTypeDefEnum\":\"ECom\",\"MemberId\":0,\"S3Key\":\"\"}"}

```json
{"Data":"{\"ShopId\":2,\"TSCode\":\"TS250918Q000001\",\"EventName\":\"\",\"TriggerDatetime\":\"2025-09-18T16:01:16.3509765+08:00\",\"OrderCreateDate\":\"2025-09-18T14:08:35.017\",\"PromotionId\":0,\"PromotionEngineType\":\"\",\"PendingRetryCount\":0,\"CrmSalesOrderSlaveId\":0,\"OrderTypeDefEnum\":\"ECom\",\"MemberId\":0,\"S3Key\":\"\"}"}
```


## AuditPromotionRecycleLoyaltyPoints / AuditPromotionRecycleCouponJob

- ts code 為單位
- 由 AuditPromotionRecycleDispatchJob 派發
  - CancelOrderSlave
  - returnGoodsSlave


**📋 訂單類型與條件**

| 訂單類型 | 資料表 | 時間欄位 | 狀態條件 |
|----------|--------|----------|----------|
| **🚫 取消訂單** | `CancelOrderSlave` | `CancelOrderSlave_UpdatedDateTime` | 狀態為 `Finish` |
| **📦 退貨訂單** | `ReturnGoodsOrderSlave` | `ReturnGoodsOrderSlaveStatusUpdatedDateTime` | 狀態為 `Finish` |

#### 🔍 查詢邏輯說明

- **取消訂單 (`cancelTsCodes`)**:
  - 查詢 `CancelOrderSlave` 表
  - 以 `CancelOrderSlave_UpdatedDateTime` 為時間基準
  - 篩選 -2 hr ~ +1hr 內狀態為 `Finish` 的記錄

- **退貨訂單**:
  - 查詢 `ReturnGoodsOrderSlave` 表  
  - 以 `ReturnGoodsOrderSlaveStatusUpdatedDateTime` 為時間基準
  - 篩選 -2 hr ~ +1hr 內狀態為 `Finish` 的記錄


## 計算前

orderSlaveFlow.OrderSlaveFlowReturnGoodsOrderSlaveIsClosed
orderSlaveFlow.OrderSlaveFlowReturnGoodsOrderSlaveStatusDef == "Finish"
orderSlaveFlow.OrderSlaveFlowSalesOrderSlaveStatusDef == "Cancel"
orderSlaveFlow.OrderSlaveFlowSalesOrderSlaveStatusDef == "Fail"


- PromotionRecycleReCalculatePointsRecordAuditor
- PromotionRecycleQuotaPointsRecordAuditor
- PromotionRecycleTGLimitationAuditor
