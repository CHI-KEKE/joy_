# BatchAuditLoyaltyPointsJob（實作類別：AuditV2PromotionRewardJob）

> **所屬專案**：nine1.promotion.worker  
> **Job 名稱常數**：`BatchAuditLoyaltyPoints`（`PromotionRewardJobConst.BatchAuditLoyaltyPointsJobName`）  
> **實作類別**：`AuditV2PromotionRewardJob`（登記常數：`AuditV2PromotionRewardJob`）  
> **Job 類別路徑**：`Nine1.Promotion.Console.NMQv3Worker.Jobs.PromotionRewardAudit`  
> **用途**：批次稽核線下訂單（CRM Others Order）的點數交易記錄，從 LoyaltyDB 查詢一段時間內的點數交易（Add/Recycle），與促銷引擎重算結果比對，驗證正流程（給點）與逆流程（扣點/回收）的正確性。
>
> ⚠️ **注意**：`BatchAuditLoyaltyPointsJobName = "BatchAuditLoyaltyPoints"` 常數用於觸發 Task，但 Program.cs 中實際登記的 Job 名稱為 `"AuditV2PromotionRewardJob"`（對應 `AuditV2PromotionRewardJob` 類別）。此為稽核道具的批次稽核機制，與其他稽核 Job 不同。

---

## 完整觸發鏈總覽

```
[外部事件] InternalMemberTierCalculateFinished（線下）
 維度：單一商店（ShopId）× 計算日期
         ↓
[Job 1] PromotionRewardBatchDispatcherV2Job
 維度：單一商店（ShopId）→ 若有逆流程活動，輸出 1 個預約稽核 Task
         ↓ 預約 +10.5hr（等待逆流程及回收後完成）
[Job 2] BatchAuditLoyaltyPointsJob / AuditV2PromotionRewardJob  ← 本 Job
 維度：單一商店（ShopId）× 時間區間 → 從 LoyaltyDB 取得交易記錄，逐筆稽核
```

---

## 各階段詳細說明（含維度）

---

### Stage 1：`PromotionRewardBatchDispatcherV2Job`

> **🎯 維度：單一商店（ShopId）維度**  
> 線下訂單流程中，每個商店若有逆流程活動輸出一個預約稽核 Task。

**觸發源**：EDA 事件 `InternalMemberTierCalculateFinished`。

**執行邏輯**：

1. 進行活動比對，若 `recycleRequests.Any() == true`，建立預約 Task：

```json
{
  "JobName": "BatchAuditLoyaltyPoints",
  "BookingTime": "現在 + 10.5小時",
  "Payload": {
    "ShopId": 123,
    "SourceType": "AuditOfflineRecycleLoyaltyPointsV2",
    "StartDateTime": "前一天日期",
    "EndDateTime": "今天日期"
  }
}
```

> **📌 重點總結**：此階段以「商店」為單位，在逆流程及回收完成後（+10.5 小時）啟動批次點數交易稽核。Payload 為 `AuditLoyaltyPointsRequestEntity`（含時間區間），與其他稽核 Job 的 Payload 格式不同。

---

### Stage 2：`AuditV2PromotionRewardJob`（本 Job）

> **🎯 維度：單一商店（ShopId）× 時間區間 → 展開為 LoyaltyDB 點數交易記錄（逐筆）維度**  
> 一個 Task = 一個商店的一段時間，內部查詢 LoyaltyDB 取得所有相關點數交易記錄後逐筆稽核。

**觸發源**：Stage 1 建立的 `BatchAuditLoyaltyPoints`（或手動觸發 `AuditV2PromotionRewardJob`）Task。

---

#### Step 1：組裝稽核流程（`DoJobAsync`）

```csharp
// 接收 AuditV2PromotionRewardJobEntity { ShopId, QueryMinutes }
// 預設 QueryMinutes = 1440（一天）
```

依序執行：
1. **稽核正流程（給點）**：呼叫 `ProcessAuditFlowAsync(taskData, "Add")`
2. **稽核逆流程（扣點）**：呼叫 `ProcessAuditFlowAsync(taskData, "Recycle")`
3. 合併兩個流程的失敗清單，發送 Slack 告警

---

#### Step 2：正流程稽核（`eventTypeDef = "Add"`）

> **🎯 稽核維度：單一 LoyaltyPointTransaction（OccurTypeId = 訂單代碼 | 活動序號）**

```
從 LoyaltyDB 查詢：
  SystemType = "PromotionReward"
  EventTypeDef = "Add"
  ShopId = taskData.ShopId
  CreateUser = "0"
  時間區間 = [Now - QueryMinutes, Now]
```

對每筆交易記錄：
1. 解析 `OccurTypeId`，格式：`[OrderCode]|[PromotionEngineId]`（ex: `TestOrder123|6177`）
2. 透過 `OrderCode` 查詢 CRM 銷售訂單（`GetSalesOrderAsync`）
3. **只處理線下訂單**（`CrmSalesOrderEcomTrackSourceTypeDef` 為空）
4. 建立 `PromotionRewardRequestEntity`（`RewardFlowEnum = Reward`）加入待計算清單

---

#### Step 3：逆流程稽核（`eventTypeDef = "Recycle"`）

> **🎯 稽核維度：單一 TransactionCode（多個退貨子單分組合併）**

```
從 LoyaltyDB 查詢：
  SystemType = "SCM.API.V2"
  EventTypeDef = "Recycle"
  CreateUser = taskData.ShopId.ToString()
```

對每個 TransactionCode 分組（因退貨以子單方式進行退點）：
1. 解析 `OccurTypeId` 取得退貨子單 `OrderCode` 和 `TransactionCode`
2. 透過 `TransactionCode` 查詢對應的主單 `OccurTypeId`（`GetOccurTypeIdByTransactionCodeAsync`）
3. 解析主單取得 `actualOrderCode`（主單訂單代碼）
4. 查詢 CRM 銷售訂單，只處理線下訂單
5. 查詢關聯訂單子單清單（`GetRelatedOrderSlaveIdListAsync`）
6. 計算 `totalPoints = 主單點數 + 所有子單點數的加總`
7. 建立 `PromotionRewardRequestEntity`（`RewardFlowEnum = Recycle`）加入待計算清單

---

#### Step 4：執行計算並比對（`Semaphore` 控制並發）

> **🎯 稽核維度：單一訂單 × 單一活動（PromotionEngineId）**

使用 `SemaphoreSlim(1)`（避免 DbContext 並發問題），對每筆待計算項目：

1. 呼叫 `OthersCrmSalesOrderPromotionRewardCalculateService` 執行活動計算
2. 將 LoyaltyDB 記錄的實際點數 vs 計算結果比對
3. 不一致則加入 `auditFailList`

---

#### Step 5：發送稽核結果（`SendAlertMessageAsync`）

```
有失敗記錄
    → 發送 Slack 告警（含正流程、逆流程失敗明細）

無失敗記錄
    → 發送 Slack 正常通知（可設定是否通知）
```

> **📌 重點總結**：此 Job 的稽核邏輯與其他稽核 Job 完全不同。其他 Job 以「DDB 紀錄」為稽核基礎，本 Job 以「**LoyaltyDB 的點數交易記錄（LoyaltyPointTransaction）**」為稽核基礎，從底層交易層驗證點數的正確性。正流程按 OccurTypeId 逐筆處理；逆流程按 TransactionCode 分組合併（主單 + 所有子單），再重新計算比對。

---

## 各階段維度對照表

| 階段 | Job 名稱 | 輸入維度 | 輸出維度 | 每次處理數量 |
|------|---------|---------|---------|-------------|
| Stage 1 | `PromotionRewardBatchDispatcherV2Job` | 商店（ShopId）× 日期 | 商店 × 1 個 Task | 1 個商店 → 1 個 Task |
| Stage 2 | `AuditV2PromotionRewardJob` | 商店（ShopId）× 時間區間 | LoyaltyPointTransaction × 逐筆稽核 | 1 個商店 → N 筆交易記錄 |

---

## 核心資料實體關係

| 實體 | 說明 |
|------|------|
| `AuditLoyaltyPointsRequestEntity` | NMQ Task Payload，含 ShopId / SourceType / StartDateTime / EndDateTime |
| `AuditV2PromotionRewardJobEntity` | Job 內部使用，含 ShopId / QueryMinutes |
| `LoyaltyPointTransactionEntity` | LoyaltyDB 點數交易記錄，含 OccurTypeId / EventTypeDef / TotalPoints |
| `PromotionRewardRequestEntity` | 計算請求，含 CrmSalesOrderId / PromotionEngineId / RewardFlowEnum / RelatedOrderSlaveIdList |

---

## 與其他稽核 Job 的核心差異

| 比較項目 | BatchAuditLoyaltyPointsJob | 其他稽核 Job（如 AuditPromotionRewardLoyaltyPointsV2Job） |
|---------|--------------------------|-------------------------------------------------------|
| 稽核資料來源 | **LoyaltyDB 交易記錄** | **DynamoDB 給點主紀錄** |
| 查詢方式 | 時間區間批次查詢 | 以訂單 / 子單 ID 精確查詢 |
| 觸發維度 | 商店 × 時間區間 | 單一訂單 / 子單 |
| 逆流程處理 | 需 TransactionCode 分組合併 | 直接讀取 DDB 的 RecyclePoints |
| 適用場景 | 道具層（LoyaltyDB）核對 | 促銷紀錄層（DDB）核對 |
