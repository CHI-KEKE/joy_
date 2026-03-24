# AuditCrmOthersOrderPromotionRecycleLoyaltyPointsJob

> **所屬專案**：nine1.promotion.worker  
> **Job 名稱常數**：`AuditCrmOthersOrderPromotionRecycleLoyaltyPoints`  
> **Job 類別路徑**：`Nine1.Promotion.Console.NMQv3Worker.Jobs.PromotionRewardAudit`  
> **用途**：線下訂單（CRM Others Order）點數回收行為稽核，確認逆流程完成後 DynamoDB 中的點數回收紀錄是否與促銷引擎重算結果一致，並驗證活動佔額是否已正確返還。

---

## 完整觸發鏈總覽

```BASH
[外部事件] InternalMemberTierCalculateFinished
 維度：單一商店（ShopId）× 計算日期
         ↓
[Job 1] PromotionRewardBatchDispatcherV2Job
 維度：單一商店（ShopId）→ 輸出 1 個 Dispatch Task
         ↓ 預約 +10hr（等待逆流程跑完）
[Job 2] AuditPromotionRecycleDispatchJob
 維度：單一商店（ShopId）→ 展開為多個退貨子單，輸出 N 個 NMQ Task
         ↓ 每個退貨子單一個 Task
[Job 3] AuditCrmOthersOrderPromotionRecycleLoyaltyPointsJob  ← 本 Job
 維度：單一退貨子單（CrmSalesOrderSlaveId）→ 稽核其對應的多筆活動 DDB 紀錄
```

---

## 各階段詳細說明（含維度）

---

### Stage 1：`PromotionRewardBatchDispatcherV2Job`

> **🎯 維度：單一商店（ShopId）維度**  
> 每個商店在每個計算日期只會觸發一次，輸出一個預約稽核 Task。

**觸發源**：EDA 事件 `InternalMemberTierCalculateFinished`，每個商店的會員等級計算完成後發出一則訊息。

**執行邏輯**：

1. 以 `ShopId + CalculateDate` 拿取該商店前一天所有線下訂單子單
2. 分類正向 / 逆向訂單並進行活動比對
3. 若有符合逆流程活動的退貨訂單（`recycleRequests.Any() == true`），建立一筆預約 Task：

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

> **📌 重點總結**：此階段以「商店」為最小單位，不區分個別訂單。目的是在逆流程預計完成後（+10 小時），啟動針對該商店所有退單的批次稽核分派。

---

### Stage 2：`AuditPromotionRecycleDispatchJob`（分派器）

> **🎯 維度：商店（ShopId）→ 展開為單一退貨子單（CrmSalesOrderSlaveId）維度**  
> 輸入是一個商店，輸出是 **N 個以退貨子單為單位的 NMQ Task**。

**觸發源**：Stage 1 預約的 `AuditPromotionRecycleDispatch` Task。

**執行邏輯**（進入 `ProcessCrmOthersRecycleAsync` 分支）：

1. 重新查詢當日所有線下訂單子單
2. 過濾出「有勾稽到原始訂單」且「原始訂單不在同天正向單內」的退貨子單（`filteredReturnOrderSlaves`）
3. 呼叫 `ProcessAuditRecycleByOrderSlaves`，對每筆退貨子單建立一個 `RecycleRequestEntity`：

```csharp
// 每一筆 filteredReturnOrderSlave → 一筆 RecycleRequestEntity
requests.Add(new RecycleRequestEntity()
{
    ShopId = filteredReturnOrderSlave.CrmSalesOrderSlaveShopId,
    CrmSalesOrderSlaveId = filteredReturnOrderSlave.CrmSalesOrderSlaveId,  // ← 退貨子單 ID
    OrderCreateDate = originalOrderSlave.CrmSalesOrderSlaveTradesOrderFinishDateTime,
    OrderTypeDefEnum = CrmOrderSourceTypeDefEnum.Others
    // 注意：此處 PromotionId 未設定，後續由 DDB 查詢展開
});
```

4. 針對每筆 `RecycleRequestEntity` 各建立兩個 NMQ Task：
   - `AuditCrmOthersOrderPromotionRecycleLoyaltyPoints`（點數稽核）← 本 Job
   - `AuditCrmOthersOrderPromotionRecycleCoupon`（優惠券稽核）

> **📌 重點總結**：此階段扮演「扇出（Fan-Out）分派器」角色，將商店維度的批次處理展開成以**退貨子單為粒度**的獨立 Task，每筆退貨子單獨立進入後續稽核流程，不會互相干擾。此時 `PromotionId` 尚未確定，留待 Stage 3 從 DDB 查出。

---

### Stage 3：`AuditCrmOthersOrderPromotionRecycleLoyaltyPointsJob`（本 Job）

> **🎯 維度：單一退貨子單（CrmSalesOrderSlaveId）→ 展開為多筆活動（PromotionEngineId）維度逐一稽核**  
> 一個 Task = 一筆退貨子單，內部對其原始訂單所關聯的所有活動 DDB 紀錄逐一比對。

**觸發源**：Stage 2 建立的每個 `AuditCrmOthersOrderPromotionRecycleLoyaltyPoints` Task。

---

#### Step 1：組裝稽核資料（`GetAuditDataAsync`）

以 `CrmSalesOrderSlaveId`（退貨子單）為起點，向外擴展查詢：

| 查詢來源 | 查詢鍵值 | 說明 |
|---------|---------|------|
| `CrmSalesOrderRepository` | `ShopId + CrmSalesOrderSlaveId` | 取得退款單 + 找到勾稽的原始訂單 ID |
| `CrmSalesOrderRepository` | `ShopId + OriginalCrmSalesOrderId` | 取得原始訂單所有子單明細 |
| `CrmSalesOrderSlaveRepository` | `ShopId + CrmMemberId + OriginalOrderId` | 取得同會員所有退貨子單清單 |
| **DynamoDB** | `ShopId + ddbOrderGroupCode` | 取得**所有活動的點數回饋主紀錄**（可能 N 筆，每筆對應一個活動） |
| `PromotionService` | `ShopId + OrderCreateDate` | 取得訂單成立時在期的點數活動清單 |
| `VipMemberRepository` | `ShopId + NineYiMemberId` | 取得會員資訊（用於計算點數時的 AES ShopMemberCode） |

> ⚠️ **DDB Key 說明**：  
> `ddbOrderGroupCode = CrmSalesOrderCodePrefix + OriginalCrmSalesOrderId`  
> 以「原始訂單 ID」為群組鍵查詢，因此一次可撈回**該原始訂單下所有活動**的 DDB 紀錄。

---

#### Step 2：並行執行兩個稽核器（`Task.WhenAll`）

每個稽核器的執行維度都是：**逐一迭代 `PromotionRewardLoyaltyPointsRecordList`（DDB 活動維度）**

```csharp
// 每一筆 recycleRecord = 一個 PromotionEngineId（活動）的 DDB 主紀錄
foreach (var recycleRecord in auditData.PromotionRewardLoyaltyPointsRecordList)
{
    // 稽核邏輯...
}
```

---

##### 稽核器 A：`PromotionRecycleReCalculatePointsRecordAuditor`

> **🎯 稽核維度：單一活動（PromotionEngineId）× 單一退貨子單**

**過濾條件（跳過不稽核）**：

| 條件 | 說明 |
|------|------|
| `Version != RecalculateModeVersion` | 非重算版本 → 跳過 |
| `RewardStatus == Unmatch` | 未命中活動 → 跳過 |
| `RewardStatus == MatchWithoutQuota` | 有命中但無佔額 → 跳過 |

**通過過濾後執行**：

1. 以 `PromotionEngineId + OrderCreateDate` 取得活動規則（`GetPromotionRuleRecordAsync`）
2. 將 CRM 訂單轉換為 `PromotionRecycleAuditEntity` 格式（含 `IsProcessRecycle` 標記）
3. **呼叫促銷引擎重新計算預期點數**（`BasketCalculateAsync`）
4. 依 `RewardStatus` 驗證結果：

| RewardStatus | 驗證邏輯 | 異常說明 |
|-------------|---------|---------|
| `WaitToReward` | 預期點數 ≠ `TotalLoyaltyPoint` | 給點前計算與記錄不符 |
| `Reward` | 預期點數 ≠ `TotalLoyaltyPoint - RecyclePoints` | 給點後回收數量與計算不符 |
| `Cancel` | `TotalLoyaltyPoint` ≠ 0 | 取消後點數未歸零 |

---

##### 稽核器 B：`PromotionRecycleQuotaPointsRecordAuditor`

> **🎯 稽核維度：單一活動（PromotionEngineId）× 單一退貨子單**

**過濾條件（跳過不稽核）**：

| 條件 | 說明 |
|------|------|
| `RewardStatus == Unmatch / MatchWithoutQuota` | 跳過 |
| `promotion.GroupCode == null` | 非佔額活動 → 跳過 |

**通過過濾後執行**：

- DDB 狀態為 `Cancel` 或「`TotalLoyaltyPoint == RecyclePoints`（完全回收）」→ 查詢 **PCPS** 確認活動佔額是否已清零
- 若 PCPS 仍有佔額紀錄 → 標記為稽核異常

---

#### Step 3：處理稽核結果

```
所有稽核器均 Success == true
    → Log 正常結束

任何稽核器有 Success == false
    → SendMessageAsync（Slack 告警）
       包含：市場環境、CrmSalesOrderSlaveId、所有失敗訊息明細
```

> **📌 重點總結**：此階段以「**退貨子單**」進入，但實際稽核粒度細化到「**每一個活動的 DDB 紀錄**」。一筆退貨子單可能對應多個活動，每個活動獨立驗證點數計算正確性與佔額返還完整性。任一活動異常即觸發 Slack 告警，兩個稽核器並行執行以提升效率。

---

## 各階段維度對照表

| 階段 | Job 名稱 | 輸入維度 | 輸出維度 | 每次處理數量 |
|------|---------|---------|---------|-------------|
| Stage 1 | `PromotionRewardBatchDispatcherV2Job` | 商店（ShopId）× 日期 | 商店 × 1 個 Dispatch Task | 1 個商店 → 1 個 Task |
| Stage 2 | `AuditPromotionRecycleDispatchJob` | 商店（ShopId）× 1 個 Task | 退貨子單（CrmSalesOrderSlaveId）× N 個 Task | 1 個商店 → N 個退單 Task |
| Stage 3 | `AuditCrmOthersOrderPromotionRecycleLoyaltyPointsJob` | 退貨子單（CrmSalesOrderSlaveId）× 1 個 Task | 活動（PromotionEngineId）× 逐一稽核 | 1 個退單 → M 個活動稽核 |

---

## 核心資料實體關係

| 實體 | 說明 |
|------|------|
| `RecycleRequestEntity` | NMQ Task Payload，含 ShopId / CrmSalesOrderSlaveId / PromotionId / OrderCreateDate |
| `PromotionRecycleLoyaltyPointsAuditCrmOthersOrderEntity` | 稽核用主資料，繼承自 `PromotionRecycleLoyaltyPointsAuditDataEntity`，額外含 `OriginalCrmOrderData` 與 `ReturnCrmOrderList` |
| `PromotionRewardLoyaltyPointsEntity` | DDB 點數回饋主紀錄，含 RewardStatus / TotalLoyaltyPoint / RecyclePoints / Version / UserTagList |
| `CrmOrderSlaveEntity` | 線下訂單原始子單資料（來自 CRM DB） |
| `PromotionRecycleAuditEntity` | 轉換後的訂單稽核用資料，含 `IsProcessRecycle` 標記區分正向 / 退貨子單 |

---

## DI 注入服務清單

> 定義於 `AuditCrmOthersOrderPromotionRecycleLoyaltyPointsJobDIAttribute`

| 類型 | 服務 |
|------|------|
| DB Context | ERP RO / CRM RO / WebStore RW / NineYiDW RO / Loyalty RO |
| Repository | `CrmSalesOrderRepository`、`CrmSalesOrderSlaveRepository`、`VipMemberRepository` 等 |
| 稽核器 | `PromotionRecycleReCalculatePointsRecordAuditor`、`PromotionRecycleQuotaPointsRecordAuditor` |
| Service | `LoyaltyPointService`、`PromotionService`、`OthersCrmSalesOrderPromotionRecycleService` |
| 通知 | `SlackMessageSender` |
| 基礎設施 | DynamoDB、AWS S3、CacheManager |

---

## 稽核失敗情境彙整

| 稽核器 | 失敗情境 | 告警訊息說明 |
|--------|---------|-------------|
| `ReCalculate` | `WaitToReward`：重算預期點數 ≠ DDB 記錄點數 | 給點前計算有誤 |
| `ReCalculate` | `Reward`：重算預期點數 ≠ TotalLoyaltyPoint - RecyclePoints | 給點後回收數量有誤 |
| `ReCalculate` | `Cancel`：TotalLoyaltyPoint ≠ 0 | 取消後點數未正確歸零 |
| `Quota` | DDB 狀態為 Cancel 或完全回收，但 PCPS 仍有佔額紀錄 | 逆流程佔額未成功返還 |

稽核失敗時發送 **Slack 告警**，頻道與接收者由以下設定決定：

```
_N1CONFIG:AuditAlertSlack:Channel
_N1CONFIG:AuditAlertSlack:Owner
_N1CONFIG:AuditAlertSlack:WebhookUrl:{Channel}
```
