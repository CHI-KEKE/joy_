# AuditPromotionRewardLoyaltyPointsV2Job

> **所屬專案**：nine1.promotion.worker  
> **Job 名稱常數**：`AuditPromotionRewardLoyaltyPointsV2`  
> **Job 類別路徑**：`Nine1.Promotion.Console.NMQv3Worker.Jobs.PromotionRewardAudit`  
> **用途**：線上訂單（ECom）給點紀錄稽核，確認正流程完成後 DynamoDB 中的給點紀錄是否與促銷引擎重算結果一致。

---

## 完整觸發鏈總覽

```
[外部事件] OrderCreated（線上訂單成立）
 維度：單一訂單群組（TradesOrderGroupCode）× 商店（ShopId）
         ↓
[Job 1] PromotionRewardLoyaltyPointsDispatcherV2Job
 維度：訂單群組 → 展開為活動維度，對每個給點活動建立一個 PromotionRewardLoyaltyPointsV2 Task
         ↓ （稽核由另一流程觸發）
[Job 2] AuditPromotionRewardLoyaltyPointsDispatchV2Job（線上分支）
 維度：全域（ExecuteTime）→ 查詢待稽核訂單群組，輸出 N 個稽核 Task
         ↓ 每個訂單群組一個 Task
[Job 3] AuditPromotionRewardLoyaltyPointsV2Job  ← 本 Job
 維度：單一訂單群組（TradesOrderGroupCode）→ 稽核其對應的多筆活動 DDB 紀錄
```

---

## 各階段詳細說明（含維度）

---

### Stage 1：`PromotionRewardLoyaltyPointsDispatcherV2Job`（上游觸發）

> **🎯 維度：單一訂單群組（TradesOrderGroupCode）× 商店（ShopId）維度**

**觸發源**：EDA 事件 `OrderCreated`。

**執行邏輯**：

1. 解析事件取得 `ShopId + TradesOrderGroupCode`
2. 查詢訂單成立時在期的給點活動
3. 對每個活動建立一個 `PromotionRewardLoyaltyPointsV2` Task（實際執行給點）

> 稽核流程由 `AuditPromotionRewardLoyaltyPointsDispatchV2Job` 排程觸發，非此 Job 直接觸發。

---

### Stage 2：`AuditPromotionRewardLoyaltyPointsDispatchV2Job`（線上分支）

> **🎯 維度：全域查詢（ExecuteTime）→ 展開為單一訂單群組（TradesOrderGroupCode）維度**

**執行邏輯**（`ProcessSalesOrderGroupAsync`）：

1. 呼叫 `SalesOrderService.GetWaitToAuditSalesOrderGroupAsync(ExecuteTime)` 查詢當日待稽核線上訂單群組
2. 確認期間有給點活動後，對每個訂單群組建立稽核 Task：

```json
{
  "JobName": "AuditPromotionRewardLoyaltyPointsV2",
  "Payload": {
    "ShopId": 2,
    "TradesOrderGroupCode": "TG250411L00004",
    "OrderDateTime": "2025-04-11T10:20:41",
    "OrderTypeDefEnum": "ECom"
  }
}
```

> **📌 重點總結**：此 Stage 以「訂單群組（TradesOrderGroupCode）」為輸出粒度，且只在有給點活動存在的情況下才建立稽核 Task。

---

### Stage 3：`AuditPromotionRewardLoyaltyPointsV2Job`（本 Job）

> **🎯 維度：單一訂單群組（TradesOrderGroupCode）→ 展開為多筆活動（PromotionEngineId）維度逐一稽核**  
> 一個 Task = 一個訂單群組，內部對其所有活動的 DDB 給點紀錄逐一驗證。

**觸發源**：Stage 2 建立的 `AuditPromotionRewardLoyaltyPointsV2` Task。

---

#### Step 1：組裝稽核資料（`GetAuditDataAsync`）

以 `TradesOrderGroupCode` 為核心：

| 查詢來源 | 查詢鍵值 | 說明 |
|---------|---------|------|
| `PromotionService` | `ShopId + OrderDateTime` | 取得訂單成立時在期的點數活動清單 |
| `SalesOrderService` | `ShopId + TradesOrderGroupCode` | 取得訂單群組的所有子單資料（`OrderSlaves`） |
| `VipMemberRepository` | `ShopId + MemberId` | 取得會員資訊（MemberId 來自第一筆 OrderSlave） |
| **DynamoDB** | `ShopId + TradesOrderGroupCode` | 取得**所有活動的給點主紀錄**（`GetLoyaltyPointRewardRecordAsync`） |
| `PromotionService` | `SetValidSupportedPromotionRuleRecords` | 過濾並設定訂單對應的有效活動規則 |

> ⚠️ **DDB 篩選**：只取 `RewardType == LoyaltyPoints`（或 RewardType 為空的舊資料）的主紀錄與明細。

---

#### Step 2：並行執行稽核器（`Task.WhenAll`）

使用 `IPromotionRewardRecordAuditor` 介面集合（多個稽核器），並行呼叫 `Audit(auditData)`。

> ⚠️ **例外處理**：每個稽核器都包裝在 `Task.Run` 中，若拋出 `AuditPromotionRewardPassException` 則視為通過（不中斷其他稽核器），其他例外會讓整個稽核失敗。

稽核器對 `LoyaltyPromotionRewards`（DDB 給點主紀錄）逐一驗證，每筆 = 一個活動（PromotionEngineId）維度。

---

#### Step 3：處理稽核結果

```
所有稽核器均 Success == true
    → Log 正常結束

任何稽核器有 Success == false
    → SendMessageAsync（Slack 告警）
       包含：市場環境、TradesOrderGroupCode（TG Code）、失敗訊息明細
```

> **📌 重點總結**：此階段以「**訂單群組（TradesOrderGroupCode）**」進入，稽核其 DDB 中所有給點活動紀錄。與線下版（`AuditCrmOthersOrderPromotionRewardLoyaltyPointsV2Job`）最大差異：資料來源是 WebStore 訂單 DB，使用 TG Code（TradesOrderGroupCode）直接作為 DDB 查詢鍵，不需要 CrmSalesOrderCodePrefix 轉換，會員資訊也直接從 OrderSlave 取得。

---

## 各階段維度對照表

| 階段 | Job 名稱 | 輸入維度 | 輸出維度 | 每次處理數量 |
|------|---------|---------|---------|-------------|
| Stage 1 | `PromotionRewardLoyaltyPointsDispatcherV2Job` | 訂單群組（TGCode）× 事件 | 活動（PromotionId）× N 個給點 Task | 1 個事件 → N 個活動 Task |
| Stage 2 | `AuditPromotionRewardLoyaltyPointsDispatchV2Job` | ExecuteTime（全域） | 訂單群組（TGCode）× N 個稽核 Task | 全域 → N 個 Task |
| Stage 3 | `AuditPromotionRewardLoyaltyPointsV2Job` | 訂單群組（TGCode）× 1 個 Task | 活動（PromotionEngineId）× 逐一稽核 | 1 個群組 → M 個活動稽核 |

---

## 核心資料實體關係

| 實體 | 說明 |
|------|------|
| `PromotionRewardRequestEntity` | NMQ Task Payload，含 ShopId / TradesOrderGroupCode / OrderDateTime / OrderTypeDefEnum |
| `PromotionRewardRecordAuditDataEntity` | 稽核用主資料，含 OrderSlaves / LoyaltyPromotionRewards / LoyaltyPromotionRewardDetails / MemberInfo |
| `PromotionRewardLoyaltyPointsEntity` | DDB 給點主紀錄，含 RewardStatus / TotalLoyaltyPoint / Version |
| `OrderSlaveEntity` | 線上訂單子單資料，含 MemberId / TrackSourceTypeDef 等 |

---

## 與線下版（AuditCrmOthersOrderPromotionRewardLoyaltyPointsV2Job）的差異

| 比較項目 | 線上（本 Job） | 線下（CrmOthers） |
|---------|------------|----------------|
| 訂單識別鍵 | `TradesOrderGroupCode`（TG Code） | `CrmSalesOrderId` |
| DDB Key | `TradesOrderGroupCode`（直接使用） | `CrmSalesOrderCodePrefix + CrmSalesOrderId` |
| 訂單查詢來源 | `SalesOrderService`（WebStore DB） | `CrmSalesOrderRepository`（CRM DB） |
| 會員資訊取得 | 直接由 `OrderSlave.MemberId` → `VipMember` | 需先 `CrmMemberId → NineYiMemberId → VipMember` |
| 跨單退貨處理 | 不需要 | 需查 `RelatedOrderSlaveIdList` |
| 稽核器例外處理 | 包裝在 `Task.Run`，`PassException` 視為通過 | 直接 `Task.WhenAll`，不包裝 |
