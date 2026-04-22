# AuditPromotionRewardLoyaltyPointsDispatchV2Job

> **所屬專案**：nine1.promotion.worker  
> **Job 名稱常數**：`AuditPromotionRewardLoyaltyPointsDispatchV2`  
> **Job 類別路徑**：`Nine1.Promotion.Console.NMQv3Worker.Jobs.PromotionRewardAudit`  
> **用途**：給點稽核分派器，負責將「給點稽核」作業依訂單類型（線上/線下）展開，並依活動類型（給點/給券）分別建立對應的稽核 NMQ Task。

---

## 完整觸發鏈總覽

```bash
[外部事件] InternalMemberTierCalculateFinished（線下）
 或
[外部事件] OrderCreated（線上，由 PromotionRewardLoyaltyPointsDispatcherV2Job 觸發給點後排程稽核）
         ↓
[Job 1] PromotionRewardBatchDispatcherV2Job（線下觸發）
 維度：單一商店（ShopId）→ 若有正流程活動，輸出 1 個預約 Dispatch Task
         ↓ 預約 +6.5hr（等待正流程跑完）
[Job 2] AuditPromotionRewardLoyaltyPointsDispatchV2Job  ← 本 Job（分派器）
 維度：單一商店（ShopId）→ 展開為多個訂單，依活動類型輸出 N 個稽核 Task
         ↓ 每個訂單各建立對應類型 Task
[Job 3a] AuditPromotionRewardLoyaltyPointsV2Job（線上給點稽核）
[Job 3b] AuditCrmOthersOrderPromotionRewardLoyaltyPointsV2Job（線下給點稽核）
[Job 3c] AuditPromotionRewardCouponJob（線上給券稽核）
[Job 3d] AuditCrmOthersOrderPromotionRewardCouponJob（線下給券稽核）
```

---

## 各階段詳細說明（含維度）

---

### Stage 1：`PromotionRewardBatchDispatcherV2Job`

> **🎯 維度：單一商店（ShopId）維度**  
> 線下觸發路徑，每個商店若有正流程活動輸出一個預約稽核 Task。

```json
{
  "JobName": "AuditPromotionRewardLoyaltyPointsDispatchV2",
  "BookingTime": "現在 + 6.5小時",
  "Payload": {
    "ShopId": 123,
    "IsCrmOthersOrder": true,
    "ExecuteTime": "2026-03-24"
  }
}
```

> **📌 重點總結**：此階段以「商店」為單位，在正流程預計完成後（+6.5 小時）啟動稽核分派。

---

### Stage 2：`AuditPromotionRewardLoyaltyPointsDispatchV2Job`（本 Job）

> **🎯 維度：依 IsCrmOthersOrder 分流**
> - **線下（true）**：商店（ShopId）→ 展開為線下訂單（CrmSalesOrderId）維度
> - **線上（false）**：全域查詢 → 展開為訂單群組（TradesOrderGroupCode）維度

---

#### 分支 A：線上訂單（`ProcessSalesOrderGroupAsync`）

> **🎯 維度：全域查詢 → 展開為單一訂單群組（TradesOrderGroupCode）維度**

1. 呼叫 `SalesOrderService.GetWaitToAuditSalesOrderGroupAsync(ExecuteTime)` 查詢當日待稽核線上訂單群組
2. 建立 `PromotionRewardRequestEntity` 清單，進入共用分派邏輯

---

#### 分支 B：線下訂單（`ProcessCrmOthersOrderAsync`）

> **🎯 維度：商店（ShopId）→ 展開為線下訂單（CrmSalesOrderId）維度**

1. 重新查詢當日線下訂單子單（`GetCrmSalesOrderSlavesByDateAsync`）
2. 分類正向訂單，呼叫 `ProcessAuditPromotionRewardByOrders` 建立 `List<PromotionRewardRequestEntity>`

---

#### 共用分派邏輯（`ProcessAuditPromotionRewardsDispatch`）

> **🎯 維度：商店（ShopId）→ 依活動類型建立給點或給券稽核 Task**

1. 依 `ShopId` 分組所有 `PromotionRewardRequestEntity`
2. 對每個 ShopId 群組查詢活動（`GetPromotionsByDatetimeRangeAsync`），依類型分類：
   - 給點活動：`RewardReachPriceWithRatePoint2` / `RewardReachPriceWithPoint2`
   - 給券活動：`RewardReachPriceWithCoupon`
3. **若有給點活動，建立給點稽核 Task**（`ProcessLoyaltyPointAudit`）：

```
線上訂單 → JobName = "AuditPromotionRewardLoyaltyPointsV2"
線下訂單 → JobName = "AuditCrmOthersOrderPromotionRewardLoyaltyPointsV2"
```

4. **若有給券活動，建立給券稽核 Task**（`ProcessCouponAudit`）：

```
線上訂單 → JobName = "AuditPromotionRewardCoupon"
線下訂單 → JobName = "AuditCrmOthersOrderPromotionRewardCoupon"
```

> **📌 重點總結**：此 Job 的核心決策邏輯是「依活動類型（點/券）× 訂單類型（線上/線下）決定下游 Job 名稱」。不是所有訂單都會建立稽核 Task，只有 **當期間確實有對應類型的活動存在時** 才會建立。每個訂單為一個獨立 Task，不同活動類型的稽核任務也是獨立的。

---

## 各階段維度對照表

| 階段 | 訂單類型 | 輸入維度 | 輸出維度 | 輸出 Task 數量 |
|------|---------|---------|---------|--------------|
| Stage 1 | 線下 | 商店（ShopId）× 日期 | 1 個 Dispatch Task | 1 |
| Stage 2（線上分支） | 線上 | ExecuteTime（全域查詢） | 訂單群組（TradesOrderGroupCode）× Task（依活動類型） | N（點）+ M（券）|
| Stage 2（線下分支） | 線下 | 商店（ShopId）× 1 個 Task | 訂單（CrmSalesOrderId）× Task（依活動類型） | N（點）+ M（券）|

---

## 核心資料實體關係

| 實體 | 說明 |
|------|------|
| `AuditPromotionRewardLoyaltyPointsDispatchV2RequestEntity` | 本 Job 的 NMQ Task Payload，含 ShopId / IsCrmOthersOrder / ExecuteTime |
| `PromotionRewardRequestEntity` | 輸出給下游 Job 的 Payload，含 ShopId / TradesOrderGroupCode（線上）或 CrmSalesOrderId（線下）/ OrderDateTime |
| `PromotionEngineOngoingEntity` | 活動資訊，TypeDef 用於判斷給點或給券 |

---

## 下游 Job 決策矩陣

| 訂單類型 | 活動類型 | 下游 Job 名稱 |
|---------|---------|-------------|
| 線上（ECom） | 給點 | `AuditPromotionRewardLoyaltyPointsV2` |
| 線上（ECom） | 給券 | `AuditPromotionRewardCoupon` |
| 線下（CrmOthers） | 給點 | `AuditCrmOthersOrderPromotionRewardLoyaltyPointsV2` |
| 線下（CrmOthers） | 給券 | `AuditCrmOthersOrderPromotionRewardCoupon` |
