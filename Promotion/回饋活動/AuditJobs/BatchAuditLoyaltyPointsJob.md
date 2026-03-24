


**主要處理器** `LoyaltyPointWorker`  每小時 01 分執行

- AuditRecycleLoyaltyPointsV2 每小時 05 分 稽核前一小時
- PromotionRewardLoyaltyPointsV2 每小時 01 分 稽核前一小時


| 服務類型 | 服務名稱 | 適用環境 | 說明 |
|----------|----------|----------|------|
| **🛍️ 線上服務** | `AuditRecycleLoyaltyPointsV2Service` | 線上訂單 | 處理電商平台訂單的點數稽核 |
| **🏪 線下服務** | `AuditOfflineRecycleLoyaltyPointsV2Service` | 線下訂單 | 處理實體店面訂單的點數稽核 |
| **🎁 道具服務** | `BaseAuditLoyaltyPointsService.cs` | 道具相關 | 處理虛擬道具相關的點數稽核 |


## AuditRecycleLoyaltyPointsV2Service

**實際回收點數**
```csharp
// 依活動 ID 取得實際回收點數
var actualRecyclePoints = detailEntities.Sum(detail => detail.LoyaltyPoint) - insufficientPoints;
```

<br>

#### 資料狀態不一致

**錯誤訊息:** `點數回收稽核錯誤：DDB 已還點而狀態未更新 IDs`

**檢查條件:**
```csharp
detail.Status != nameof(RewardDetailStatusEnum.NoReward) &&
detail.IsRecycle == false &&
detail.Status != nameof(RewardDetailStatusEnum.Recycle) &&
detail.Status != nameof(RewardDetailStatusEnum.Cancel)
```

<br>

##### 退點數量異常

**錯誤訊息:** `點數回收稽核錯誤：DDB 發現退點超過給點數量 IDs`

**檢查邏輯:**
```csharp
record.GivingPoints < record.RecyclePoints
```

確保回收點數不超過原始發放點數

<br>

#### 交易記錄完整性

| 欄位名稱 | 說明 | 必要性 |
|----------|------|--------|
| `LoyaltyPointTransactionOccurTypeId` | 交易發生類型 ID | ✅ 必須存在 |
| `LoyaltyPointTransactionEventTypeDef` | 交易事件類型定義 | ✅ 必須存在 |
| `VipmemberId` | VIP 會員 ID | ✅ 必須存在 |即觸發稽核異常
