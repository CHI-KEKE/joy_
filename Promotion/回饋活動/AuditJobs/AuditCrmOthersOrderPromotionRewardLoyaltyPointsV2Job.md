# AuditCrmOthersOrderPromotionRewardLoyaltyPointsV2Job

> **所屬專案**：nine1.promotion.worker  
> **Job 名稱常數**：`AuditCrmOthersOrderPromotionRewardLoyaltyPointsV2`  
> **Job 類別路徑**：`Nine1.Promotion.Console.NMQv3Worker.Jobs.PromotionRewardAudit`  
> **用途**：線下訂單（CRM Others Order）給點紀錄稽核，確認正流程完成後 DynamoDB 中的給點紀錄是否與促銷引擎重算結果一致。

---

## 完整觸發鏈總覽

```
[外部事件] InternalMemberTierCalculateFinished
 維度：單一商店（ShopId）× 計算日期
         ↓
[Job 1] PromotionRewardBatchDispatcherV2Job
 維度：單一商店（ShopId）→ 若有正流程活動，輸出 1 個 Dispatch Task
         ↓ 預約 +6.5hr（等待正流程跑完）
[Job 2] AuditPromotionRewardLoyaltyPointsDispatchV2Job
 維度：單一商店（ShopId）→ 展開為多個線下訂單（CrmSalesOrderId），輸出 N 個 NMQ Task
         ↓ 每個線下訂單一個 Task
[Job 3] AuditCrmOthersOrderPromotionRewardLoyaltyPointsV2Job  ← 本 Job
 維度：單一線下訂單（CrmSalesOrderId）→ 稽核其對應的多筆活動 DDB 紀錄
```

---

## 各階段詳細說明（含維度）

---

### Stage 1：`PromotionRewardBatchDispatcherV2Job`

> **🎯 維度：單一商店（ShopId）維度**  
> 每個商店在每個計算日期只會觸發一次，若正流程有活動則輸出一個預約稽核 Task。

**觸發源**：EDA 事件 `InternalMemberTierCalculateFinished`。

**執行邏輯**：

1. 以 `ShopId + CalculateDate` 拿取該商店前一天所有線下訂單子單
2. 進行活動比對，找出有參與給點活動的正向訂單（`promotionRewardRequestEntitys`）
3. 若 `promotionRewardRequestEntitys.Any() == true`，建立一筆預約 Task：

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

> **📌 重點總結**：此階段以「商店」為最小單位，目的是在正流程預計完成後（+6.5 小時），啟動針對該商店所有線下給點訂單的批次稽核分派。

---

### Stage 2：`AuditPromotionRewardLoyaltyPointsDispatchV2Job`（分派器）

> **🎯 維度：商店（ShopId）→ 展開為單一線下訂單（CrmSalesOrderId）維度**  
> 輸入是一個商店，輸出是 **N 個以線下訂單（CrmSalesOrderId）為單位的 NMQ Task**。

**觸發源**：Stage 1 預約的 `AuditPromotionRewardLoyaltyPointsDispatchV2` Task。

**執行邏輯**（進入 `ProcessCrmOthersOrderAsync` 分支）：

1. 重新查詢當日所有線下訂單子單
2. 分類正向訂單，呼叫 `ProcessAuditPromotionRewardByOrders` 建立 `List<PromotionRewardRequestEntity>`
3. 依 ShopId 分組，查詢該期間活動，篩選出有給點活動的訂單
4. **若有給點活動，針對每筆訂單建立稽核 Task**：

```json
{
  "JobName": "AuditCrmOthersOrderPromotionRewardLoyaltyPointsV2",
  "Payload": {
    "ShopId": 123,
    "CrmSalesOrderId": 456,
    "OrderDateTime": "2026-03-23T10:00:00",
    "OrderTypeDefEnum": "Others"
  }
}
```

> **📌 重點總結**：此階段扮演「扇出（Fan-Out）分派器」角色，將商店維度展開成以**線下訂單（CrmSalesOrderId）為粒度**的獨立稽核 Task。

---

### Stage 3：`AuditCrmOthersOrderPromotionRewardLoyaltyPointsV2Job`（本 Job）

> **🎯 維度：單一線下訂單（CrmSalesOrderId）→ 展開為多筆活動（PromotionEngineId）維度逐一稽核**  
> 一個 Task = 一筆線下訂單，內部對其 DDB 給點紀錄逐一驗證。

**觸發源**：Stage 2 建立的 `AuditCrmOthersOrderPromotionRewardLoyaltyPointsV2` Task。

---

#### Step 1：組裝稽核資料（`GetAuditDataAsync`）

以 `CrmSalesOrderId` 為核心，查詢所有必要資料：

| 查詢來源 | 查詢鍵值 | 說明 |
|---------|---------|------|
| `CrmSalesOrderRepository` | `ShopId + CrmSalesOrderId` | 取得線下訂單所有子單（含同天勾稽的退貨子單） |
| `CrmSalesOrderRepository` | `ShopId + RelatedOrderSlaveIdList` | 取得跨單勾稽的退貨子單資料 |
| **DynamoDB** | `ShopId + ddbOrderGroupCode` | 取得**所有活動的給點主紀錄**（`CrmSalesOrderCodePrefix + CrmSalesOrderId`） |
| `PromotionService` | `ShopId + OrderDateTime` | 取得訂單成立時在期的點數活動清單 |
| `CrmMemberRepository` | `ShopId + CrmMemberId` | 透過 CRM 會員 ID 取得 NineYi 會員 ID |
| `VipMemberRepository` | `ShopId + NineYiMemberId` | 取得會員資訊（含 VipMemberId，用於 AES 加密） |

> ⚠️ **特殊邏輯**：
> - 排除「原始訂單不是當前 CrmSalesOrderId 的退貨子單」（`ExcludeInvalidNormalOrders`）
> - `RelatedOrderSlaveIdList` 處理跨單勾稽退貨，確保同天不同主單的退貨也納入計算

---

#### Step 2：並行執行稽核器（`Task.WhenAll`）

使用 `IPromotionRewardRecordAuditor` 介面集合，對每個稽核器並行呼叫 `Audit(auditData)`。

稽核過程：
- 每個稽核器迭代 `LoyaltyPromotionRewards`（DDB 給點主紀錄），每筆 = 一個活動維度
- 個別稽核器的例外透過 `AuditPromotionRewardPassException` 可標記為「可跳過」（視為通過）

---

#### Step 3：處理稽核結果

```bash
所有稽核器均 Success == true
    → Log 正常結束

任何稽核器有 Success == false
    → SendMessageAsync（Slack 告警）
       包含：市場環境、CrmSalesOrderId、所有失敗訊息明細
```

> **📌 重點總結**：此階段以「**線下訂單（CrmSalesOrderId）**」進入，稽核其 DDB 中所有給點活動紀錄。與線上版（`AuditPromotionRewardLoyaltyPointsV2Job`）最大差異在於：資料來源是 CRM DB 而非 WebStore 訂單，DDB Key 使用 `CrmSalesOrderCodePrefix + CrmSalesOrderId` 格式，且需要額外查詢 CRM 會員映射。

---

## 各階段維度對照表

| 階段 | Job 名稱 | 輸入維度 | 輸出維度 | 每次處理數量 |
|------|---------|---------|---------|-------------|
| Stage 1 | `PromotionRewardBatchDispatcherV2Job` | 商店（ShopId）× 日期 | 商店 × 1 個 Dispatch Task | 1 個商店 → 1 個 Task |
| Stage 2 | `AuditPromotionRewardLoyaltyPointsDispatchV2Job` | 商店（ShopId）× 1 個 Task | 線下訂單（CrmSalesOrderId）× N 個 Task | 1 個商店 → N 個訂單 Task |
| Stage 3 | `AuditCrmOthersOrderPromotionRewardLoyaltyPointsV2Job` | 線下訂單（CrmSalesOrderId）× 1 個 Task | 活動（PromotionEngineId）× 逐一稽核 | 1 個訂單 → M 個活動稽核 |

---

## 核心資料實體關係

| 實體 | 說明 |
|------|------|
| `PromotionRewardRequestEntity` | NMQ Task Payload，含 ShopId / CrmSalesOrderId / OrderDateTime / RelatedOrderSlaveIdList |
| `PromotionRewardRecordAuditOthersOrderEntity` | 稽核用主資料，繼承自 `PromotionRewardRecordAuditDataEntity`，含 `OrderSlaves`（CRM 訂單子單） |
| `PromotionRewardLoyaltyPointsEntity` | DDB 給點主紀錄，含 RewardStatus / TotalLoyaltyPoint / Version |
| `CrmOrderSlaveEntity` | CRM DB 線下訂單子單資料 |
| `OrderSlaveEntity` | 轉換後的稽核用訂單資料（IsGift=false、IsMajor=true 等線下訂單特性） |

---

## 與線上版（AuditPromotionRewardLoyaltyPointsV2Job）的差異

| 比較項目 | 線下（本 Job） | 線上（V2Job） |
|---------|-------------|-------------|
| 訂單識別鍵 | `CrmSalesOrderId` | `TradesOrderGroupCode` |
| DDB Key | `CrmSalesOrderCodePrefix + CrmSalesOrderId` | `TradesOrderGroupCode`（直接使用） |
| 訂單查詢來源 | `CrmSalesOrderRepository`（CRM DB） | `SalesOrderService`（WebStore） |
| 會員查詢流程 | CRM MemberId → NineYi MemberId → VipMember | 直接由 OrderSlave.MemberId → VipMember |
| 跨單退貨處理 | 需額外查詢 `RelatedOrderSlaveIdList` | 不需要 |
| 訂單 IsGift | 線下一律 false（即使是 Gift 品項） | 依實際訂單類型 |
