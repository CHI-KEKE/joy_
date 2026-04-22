# AuditPromotionRecycleLoyaltyPointsJob

> **所屬專案**：nine1.promotion.worker  
> **Job 名稱常數**：`AuditPromotionRecycleLoyaltyPoints`  
> **Job 類別路徑**：`Nine1.Promotion.Console.NMQv3Worker.Jobs.PromotionRewardAudit`  
> **用途**：線上訂單（ECom）點數回收行為稽核，確認逆流程完成後 DynamoDB 中的點數回收紀錄是否與促銷引擎重算結果一致，並驗證活動佔額是否已正確返還。

---

## 完整觸發鏈總覽

```bash
[外部事件] OrderCancelled / OrderReturned
 維度：單一訂單子單（TSCode）× 商店（ShopId）
         ↓
[Job 1] RecycleLoyaltyPointsDispatcherV2Job
 維度：單一訂單子單（TSCode）→ 展開為多個活動，輸出 N 個 RecycleLoyaltyPointsV2 Task
         ↓ （正流程完成後，另由 AuditPromotionRecycleDispatchJob 觸發稽核）
[Job 2] AuditPromotionRecycleDispatchJob（線上分支）
 維度：全域（ExecuteTime）→ 查詢待稽核訂單子單，輸出 N 個稽核 Task
         ↓ 每個訂單子單一個 Task
[Job 3] AuditPromotionRecycleLoyaltyPointsJob  ← 本 Job
 維度：單一訂單子單（TSCode）→ 稽核其對應的多筆活動 DDB 紀錄
```

---

## 各階段詳細說明（含維度）

---

### Stage 1：`RecycleLoyaltyPointsDispatcherV2Job`（上游觸發）

> **🎯 維度：單一訂單子單（TSCode）× 商店（ShopId）維度**

**觸發源**：EDA 事件 `OrderCancelled` 或 `OrderReturned`。

**執行邏輯**：

1. 解析事件取得 `ShopId + TSCode + 觸發時間`
2. 查詢訂單成立時在期的給點活動（`GetOngoingPromotionByInputDatetimeAsync`）
3. 過濾出點數類型活動（`RewardReachPriceWithPoint2` / `RewardReachPriceWithRatePoint2`）
4. 對每個活動建立一個 `RecycleLoyaltyPointsV2` Task（正式回收點數）

> 稽核流程由 `AuditPromotionRecycleDispatchJob` 排程觸發，非此 Job 直接觸發。

---

### Stage 2：`AuditPromotionRecycleDispatchJob`（線上分支）

> **🎯 維度：全域查詢（ExecuteTime）→ 展開為單一訂單子單（TSCode）維度**

**執行邏輯**（`ProcessSalesOrderRecycleAsync`）：

1. 呼叫 `SalesOrderService.GetRecycleAuditDataAsync(ExecuteTime)` 查詢當日需稽核的線上退單/取消訂單
2. 對每筆訂單子單建立：

```json
{
  "JobName": "AuditPromotionRecycleLoyaltyPoints",
  "Payload": {
    "ShopId": 2,
    "TSCode": "TS250903T000003",
    "TriggerDatetime": "2026-03-24T10:00:00",
    "OrderCreateDate": "2026-01-15T09:00:00",
    "OrderTypeDefEnum": "ECom"
  }
}
```

> **📌 重點總結**：此 Stage 以「訂單子單（TSCode）」為輸出粒度，一個退單/取消單對應一個稽核 Task，且此時 `PromotionId` 未設定，後續由 DDB 查出所有活動記錄。

---

### Stage 3：`AuditPromotionRecycleLoyaltyPointsJob`（本 Job）

> **🎯 維度：單一訂單子單（TSCode）→ 展開為多筆活動（PromotionEngineId）維度逐一稽核**  
> 一個 Task = 一筆訂單子單（TSCode），內部對其原始訂單群組所關聯的所有活動 DDB 紀錄逐一比對。

**觸發源**：Stage 2 建立的 `AuditPromotionRecycleLoyaltyPoints` Task。

---

#### Step 1：組裝稽核資料（`GetAuditDataAsync`）

以 `TSCode`（訂單子單代碼）為起點：

| 查詢來源 | 查詢鍵值 | 說明 |
|---------|---------|------|
| `SalesOrderRepository` | `ShopId + TSCode` | 取得點數回收訂單資料（含 `OrderGroupCode`、`MemberId`） |
| **DynamoDB** | `ShopId + OrderGroupCode` | 取得**所有活動的點數回饋主紀錄**（可能 N 筆，每筆對應一個活動） |
| `PromotionService` | `ShopId + OrderCreateDate` | 取得訂單成立時在期的點數活動清單 |
| `VipMemberRepository` | `ShopId + MemberId` | 取得會員資訊 |

> ⚠️ **DDB Key 說明**：使用 `OrderGroupCode`（即 TG Code）為查詢鍵，一次取回該訂單群組下所有活動的 DDB 記錄。

---

#### Step 2：並行執行兩個稽核器（`Task.WhenAll`）

每個稽核器迭代 `PromotionRewardLoyaltyPointsRecordList`，每筆 = 一個活動（PromotionEngineId）維度。

---

##### 稽核器 A：`PromotionRecycleReCalculatePointsRecordAuditor`

> **🎯 稽核維度：單一活動（PromotionEngineId）× 單一訂單子單**

**過濾條件（跳過不稽核）**：

| 條件 | 說明 |
|------|------|
| `Version != RecalculateModeVersion` | 非重算版本 → 跳過 |
| `RewardStatus == Unmatch` | 未命中活動 → 跳過 |
| `RewardStatus == MatchWithoutQuota` | 有命中但無佔額 → 跳過 |

**通過後**：取正向訂單（`IsProcessRecycle == false`）→ 呼叫促銷引擎 `BasketCalculateAsync` 重算 → 比對：

| RewardStatus | 驗證邏輯 |
|-------------|---------|
| `WaitToReward` | 預期點數 ≠ `TotalLoyaltyPoint` → 異常 |
| `Reward` | 預期點數 ≠ `TotalLoyaltyPoint - RecyclePoints` → 異常 |
| `Cancel` | `TotalLoyaltyPoint` ≠ 0 → 異常 |

---

##### 稽核器 B：`PromotionRecycleQuotaPointsRecordAuditor`

> **🎯 稽核維度：單一活動（PromotionEngineId）× 單一訂單子單**

- 過濾非佔額活動（`GroupCode == null`）或 `Unmatch/MatchWithoutQuota` 狀態
- DDB 狀態為 `Cancel` 或完全回收 → 查 **PCPS** 確認佔額已清零
- 若 PCPS 仍有佔額記錄 → 異常

---

#### Step 3：處理稽核結果

```
所有稽核器均 Success == true
    → Log 正常結束（不觸發告警）

任何稽核器有 Success == false
    → SendMessage（Slack 告警，async void）
       包含：市場環境、TSCode、失敗訊息明細
```

> **⚠️ 注意**：此 Job 的 `SendMessage` 為 `async void`（非 `async Task`），異常不會向上傳播。

> **📌 重點總結**：此階段以「**訂單子單（TSCode）**」進入，稽核其對應原始訂單群組下所有活動的 DDB 回收紀錄。與線下版（`AuditCrmOthersOrderPromotionRecycleLoyaltyPointsJob`）最大差異：查詢來源為 WebStore 訂單 DB，使用 TSCode 而非 CrmSalesOrderSlaveId，且不需要勾稽原始訂單的複雜邏輯。

---

## 各階段維度對照表

| 階段 | Job 名稱 | 輸入維度 | 輸出維度 | 每次處理數量 |
|------|---------|---------|---------|-------------|
| Stage 1 | `RecycleLoyaltyPointsDispatcherV2Job` | 訂單子單（TSCode）× 事件 | 活動（PromotionId）× N 個回收 Task | 1 個事件 → N 個活動 Task |
| Stage 2 | `AuditPromotionRecycleDispatchJob`（線上） | ExecuteTime（全域） | 訂單子單（TSCode）× N 個稽核 Task | 全域 → N 個 Task |
| Stage 3 | `AuditPromotionRecycleLoyaltyPointsJob` | 訂單子單（TSCode）× 1 個 Task | 活動（PromotionEngineId）× 逐一稽核 | 1 個子單 → M 個活動稽核 |

---

## 與線下版（AuditCrmOthersOrderPromotionRecycleLoyaltyPointsJob）的差異

| 比較項目 | 線上（本 Job） | 線下（CrmOthers） |
|---------|------------|----------------|
| 訂單識別鍵 | `TSCode`（訂單子單代碼） | `CrmSalesOrderSlaveId`（退貨子單 ID） |
| DDB Key | `OrderGroupCode`（TG Code） | `CrmSalesOrderCodePrefix + OriginalCrmSalesOrderId` |
| 訂單查詢來源 | `SalesOrderRepository`（WebStore） | `CrmSalesOrderRepository`（CRM DB） |
| 原始訂單勾稽 | 不需要額外查詢 | 需查 `GetRecycleAndOriginalOrderDataAsync` |
| 退貨子單清單 | 包含在 `RecycleOrderData` 中 | 需額外查 `CrmSalesOrderSlaveRepository` |
| 發送告警方式 | `async void`（非同步，不阻塞） | `async Task`（等待完成） |
