# AuditPromotionRecycleDispatchJob

> **所屬專案**：nine1.promotion.worker  
> **Job 名稱常數**：`AuditPromotionRecycleDispatch`  
> **Job 類別路徑**：`Nine1.Promotion.Console.NMQv3Worker.Jobs.PromotionRewardAudit`  
> **用途**：回收稽核分派器，負責將「回收稽核」作業依訂單類型（線上/線下）展開，並分別為點數和優惠券各建立獨立的稽核 NMQ Task。

---

## 完整觸發鏈總覽

```bash
[外部事件] InternalMemberTierCalculateFinished（線下）
 或
[外部事件] OrderCancelled / OrderReturned（線上，由 RecycleLoyaltyPointsDispatcherV2Job 後續觸發）
         ↓
[Job 1] PromotionRewardBatchDispatcherV2Job（線下觸發）
 維度：單一商店（ShopId）→ 若有逆流程活動，輸出 1 個預約 Dispatch Task
         ↓ 預約 +10hr（等待逆流程跑完）
[Job 2] AuditPromotionRecycleDispatchJob  ← 本 Job（分派器）
 維度：單一商店（ShopId）→ 展開為多個訂單/子單，輸出 N × 2 個 NMQ Task（點數 + 券各一）
         ↓ 每個訂單各建立兩個 Task
[Job 3a] AuditPromotionRecycleLoyaltyPointsJob（線上點數稽核）
[Job 3b] AuditCrmOthersOrderPromotionRecycleLoyaltyPointsJob（線下點數稽核）
[Job 3c] AuditPromotionRecycleCouponJob（線上券稽核）
[Job 3d] AuditCrmOthersOrderPromotionRecycleCouponJob（線下券稽核）
```

---

## 各階段詳細說明（含維度）

---

### Stage 1：`PromotionRewardBatchDispatcherV2Job`（線下觸發）

> **🎯 維度：單一商店（ShopId）維度**  
> 線下訂單流程中，每個商店在每個計算日期若有逆流程活動，輸出一個預約稽核 Task。

**執行邏輯**（線下路徑）：

1. 活動比對找出有逆流程活動的退貨訂單（`recycleRequests`）
2. 若 `recycleRequests.Any() == true`，建立預約 Task：

```json
{
  "JobName": "AuditPromotionRecycleDispatch",
  "BookingTime": "現在 + 10小時",
  "Payload": {
    "ShopId": 123,
    "IsCrmOthersOrder": true,
    "ExecuteTime": "2026-03-24"
  }
}
```

**線上觸發路徑（獨立觸發）**：  
線上訂單的回收稽核由另外的流程觸發，Payload 中 `IsCrmOthersOrder = false`，`ShopId` 可能為空。

> **📌 重點總結**：此 Job 為分派器，在預約時間到達後統一處理該商店的回收稽核分派工作，依 `IsCrmOthersOrder` 旗標分流至不同處理邏輯。

---

### Stage 2：`AuditPromotionRecycleDispatchJob`（本 Job）

> **🎯 維度：依 IsCrmOthersOrder 分流**
> - **線下（true）**：商店（ShopId）→ 展開為退貨子單（CrmSalesOrderSlaveId）維度
> - **線上（false）**：全域查詢 → 展開為訂單子單（TSCode）維度

---

#### 分支 A：線上訂單（`ProcessSalesOrderRecycleAsync`）

> **🎯 維度：全域 → 展開為單一線上訂單子單（TSCode）維度**

1. 呼叫 `SalesOrderService.GetRecycleAuditDataAsync(ExecuteTime)` 查詢當日需要稽核的線上訂單子單
2. 對每筆訂單子單各建立兩個 Task：

```json
{
  "JobName": "AuditPromotionRecycleLoyaltyPoints",
  "Payload": { "ShopId": 2, "TSCode": "TS250903T000003", "OrderCreateDate": "...", "OrderTypeDefEnum": "ECom" }
}
{
  "JobName": "AuditPromotionRecycleCoupon",
  "Payload": { ... }
}
```

> **📌 重點總結（線上分支）**：以「訂單子單（TSCode）」為輸出粒度，點數與優惠券稽核各自獨立 Task，互不影響。

---

#### 分支 B：線下訂單（`ProcessCrmOthersRecycleAsync`）

> **🎯 維度：商店（ShopId）→ 展開為退貨子單（CrmSalesOrderSlaveId）維度**

1. 重新查詢當日線下訂單子單（`GetCrmSalesOrderSlavesByDateAsync`）
2. 過濾出「有勾稽到原始訂單」且「不在同天正向單中」的退貨子單（`filteredReturnOrderSlaves`）
3. 呼叫 `ProcessAuditRecycleByOrderSlaves`，對每筆退貨子單建立 `RecycleRequestEntity`：

```csharp
// 每一筆 filteredReturnOrderSlave → 一筆 RecycleRequestEntity
new RecycleRequestEntity()
{
    ShopId = ...,
    CrmSalesOrderSlaveId = filteredReturnOrderSlave.CrmSalesOrderSlaveId,
    OrderCreateDate = originalOrderSlave.CrmSalesOrderSlaveTradesOrderFinishDateTime,
    OrderTypeDefEnum = CrmOrderSourceTypeDefEnum.Others
    // PromotionId 未設定，後續由 DDB 查出
}
```

4. 對每筆 `RecycleRequestEntity` 各建立兩個 Task：

```json
{
  "JobName": "AuditCrmOthersOrderPromotionRecycleLoyaltyPoints",
  "Payload": { "ShopId": 123, "CrmSalesOrderSlaveId": 456, "OrderCreateDate": "..." }
}
{
  "JobName": "AuditCrmOthersOrderPromotionRecycleCoupon",
  "Payload": { ... }
}
```

> **📌 重點總結（線下分支）**：以「退貨子單（CrmSalesOrderSlaveId）」為輸出粒度，`PromotionId` 尚未確定，留待後續稽核 Job 從 DDB 查出所有活動記錄。點數與優惠券稽核各自獨立 Task。

---

## 各階段維度對照表

| 階段 | 訂單類型 | 輸入維度 | 輸出維度 | 輸出 Task 數量 |
|------|---------|---------|---------|--------------|
| Stage 1 | 線下 | 商店（ShopId）× 日期 | 1 個 Dispatch Task | 1 |
| Stage 2（線上分支） | 線上 | ExecuteTime（全域查詢） | 訂單子單（TSCode）× 2 Task（點/券各一） | N × 2 |
| Stage 2（線下分支） | 線下 | 商店（ShopId）× 1 個 Task | 退貨子單（CrmSalesOrderSlaveId）× 2 Task（點/券各一） | N × 2 |

---

## 核心資料實體關係

| 實體 | 說明 |
|------|------|
| `AuditPromotionRecycleDispatchRequestEntity` | 本 Job 的 NMQ Task Payload，含 ShopId / IsCrmOthersOrder / ExecuteTime |
| `RecycleRequestEntity` | 輸出給下游 Job 的 Payload，含 ShopId / TSCode（線上）或 CrmSalesOrderSlaveId（線下）/ OrderCreateDate |
| `CrmSalesOrderSlaveLiteEntity` | 線下訂單子單輕量實體，用於篩選和分類 |

---

## 下游 Job 對照

| 訂單類型 | 稽核類型 | 下游 Job 名稱 |
|---------|---------|-------------|
| 線上（ECom） | 點數 | `AuditPromotionRecycleLoyaltyPoints` |
| 線上（ECom） | 優惠券 | `AuditPromotionRecycleCoupon` |
| 線下（CrmOthers） | 點數 | `AuditCrmOthersOrderPromotionRecycleLoyaltyPoints` |
| 線下（CrmOthers） | 優惠券 | `AuditCrmOthersOrderPromotionRecycleCoupon` |
