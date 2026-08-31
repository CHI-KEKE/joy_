# BatchAuditCoupon — 批次優惠券稽核 Job

## 概覽

| 項目 | 說明 |
|------|------|
| Job 類別 | `BatchAuditCouponJob` |
| 觸發方式 | 定時排程（每小時觸發一次） |
| 時間視窗 | `StartDateTime = 當下 - 2hr`、`EndDateTime = 當下 - 1hr` |
| 目的 | 稽核在該時間視窗內應該被發放的優惠券，確認實際發放結果是否正確 |
| 通知管道 | Slack（`_N1CONFIG:AuditAlertSlack` 設定） |

---

## 觸發參數（NMQ Task Data）

```json
{
  "ShopId": 0,
  "StartDateTime": "2026-04-28T13:00:00",
  "EndDateTime": "2026-04-28T14:00:00",
  "SourceType": "PromotionRewardCoupon"
}
```

| 欄位 | 說明 |
|------|------|
| `ShopId` | `0` = 全商店稽核；`> 0` = 單一商店稽核 |
| `StartDateTime` | 稽核起始時間（當下 - 2hr） |
| `EndDateTime` | 稽核結束時間（當下 - 1hr） |
| `SourceType` | 目前僅支援 `PromotionRewardCoupon`（線上/線下活動給券） |

---

## 整體流程

```
排程觸發
  │
  ▼
BatchAuditCouponJob.DoJobAsync
  │
  ├─ ShopId > 0 ──→ GetAuditDataAsync(entity)         [單店模式，目前 NotImplemented]
  │
  └─ ShopId = 0 ──→ GetAllShopAuditDataAsync(entity)  [全店模式，實際使用]
                          │
                          ▼
                   依 ShopId 分組
                          │
                          ▼ (per shop)
                    DoAuditAsync
                      │
                      ├─── [Base] 給券數量 & 序號比對
                      │
                      └─── [Override] DDB 狀態稽核
```

---

## 資料來源：DynamoDB 查詢

**Service**：`AuditPromotionRewardCouponService.GetAllShopAuditDataAsync`

**Table**：`PromotionRewardTable`（`PromotionRewardCouponEntity`）

**GSI**：`BookingDateUTC-BookingTimeUTC-index`

**查詢條件**：
```
BookingDateUTC  =  startTime.Date
BookingTimeUTC  BETWEEN  startTime AND endTime
RewardType      =  ECoupon
```

**過濾掉**（不需稽核的終態/排除邏輯）：
| 排除狀態 | 說明 |
|----------|------|
| `Unmatch` | 訂單不符合任何活動，無需給券 |
| `MatchWithoutQuota` | 中活動但無額度，無需給券 |
| `Cancel` | 已取消，無需給券 |

---

## 稽核邏輯一覽

BatchAuditCoupon 執行兩層稽核，由父類別與子類別各自負責不同面向。

### 第一層：給券數量 & 序號比對（BaseAuditCouponService）

針對狀態為 `Reward`（已給券）的紀錄，從 ECouponSlave DB 查詢會員實際持有的券，與 DDB 紀錄交叉比對。

**排除 `Occupy` 狀態**（訂單成立但尚未付款，不列入給券比對）。

**DB 查詢**（ECouponSlave，WebStore ReadOnly）：
```sql
WHERE ECouponSlave_ECouponId = @eCouponId
  AND ECouponSlave_ShopId = @shopId
  AND ECouponSlave_MemberId = @memberId
  AND ECouponSlave_ValidFlag = true
  AND ECouponSlave_IsTake = true
```

**比對規則**：

| 錯誤條件 | 錯誤訊息 |
|----------|----------|
| ECouponSlave DB 查無紀錄 | `DB查無紀錄` |
| DDB 已發券序號列表為空 | `DDB已發券序號為空` |
| `GivenCouponQty > ECouponSlave 筆數` | `DDB發券數量少於實際` |
| DDB 記錄的券序號不在 ECouponSlave 清單中 | `稽核資料券序號不在券DB紀錄中` |

有任何錯誤 → 發 Slack 告警（系統異常，需人工介入）。

---

### 第二層：DDB 狀態稽核（AuditPromotionRewardCouponService）

針對 DDB 中狀態 **不是** `Reward` 的紀錄，判斷是系統異常還是預期中的券不足重試。

#### 判斷流程

```
RewardStatus != Reward
        │
        ├─ isLastReasonCouponShortage?  ← UpdateReason.Contains("券不足")
        │   ← 最後一次寫入 DDB 確實因券庫存不足
        │
        ├─ isUpdateTimeReasonable?      ← UpdateTime >= StartDateTime - 13h
        │   ← DDB 有被正常更新（非 job crash 造成的過期資料）
        │   ← 正常 12h 重試間隔，容錯 1h
        │
        ├─ 兩個條件皆成立 ───→ ✅ [ECoupon庫存不足] 通知（正常重試，無需介入）
        │
        └─ 任一條件不成立 ───→ ⚠️ [系統異常] 告警（需人工調查）
```

#### 時間維度說明

| 情境 | UpdateTime 相對 StartDateTime | 結果 |
|------|-------------------------------|------|
| 正常 12h 重試週期 | 約 `-12h`（≥ -13h） | ✅ 分類為券不足 |
| 單次 Job Crash（DDB 未更新） | 約 `-12h`（≥ -13h） | ✅ 仍分類為券不足（可接受） |
| 連續多次 Crash | 約 `-24h` 以上（< -13h） | ⚠️ 分類為系統異常 |

---

## 通知訊息格式

### ⚠️ 系統異常（需人工介入）

```
@owner [Market][Env]
線上優惠券紀錄稽核異常
ServiceName: AuditPromotionRewardCouponService
ShopId: 360
異常項目:
522995_TG251217NA00XC 已達給券時間但狀態未更新，狀態為：WaitToReward
522995_TG251215QA00FU 已達給券時間但狀態未更新，狀態為：WaitToReward
522995_TG251217QA00NM 已達給券時間但狀態未更新，狀態為：Occupy，如訂單尚未付款，可不用處理。
```

> `Occupy` 狀態會附加提示說明，訂單尚未付款時可忽略。

---

### ✅ ECoupon 庫存不足（正常重試，確認補券後可忽略）

```
@owner [Market][Env]
[ECoupon庫存不足] 以下訂單因券不足持續重試中，系統將自動於下次排程重試，確認券庫存補充後可忽略此訊息
ServiceName: AuditPromotionRewardCouponService
ShopId: 360
訂單狀態：
  ✅ 522995_TG251217NA00XC  下次重試：04/27 22:00  已重試 3 次
  ✅ 522995_TG251215QA00FU  下次重試：04/27 22:00  已重試 5 次
  ✅ 522995_TG251216YA01PB  下次重試：04/27 22:00  已重試 7 次
```

> `下次重試` 取自 DDB `BookingTime`（即 `CalculateNextRetryBookingTime` 計算的 10:00/22:00）。  
> `已重試次數` 從 `RewardHistoryList` 中統計 UpdateReason 含「券不足」的筆數。

---

## 關鍵 RewardStatus 說明

| 狀態 | 說明 | 稽核行為 |
|------|------|----------|
| `WaitToReward` | 待給券（尚未給） | 稽核是否異常或正常重試 |
| `Reward` | 已給券 | 比對 ECouponSlave 數量與序號 |
| `Occupy` | 訂單成立但未付款 | 跳過給券數量比對；若出現在時間視窗則提示維運 |
| `Unmatch` | 不符合活動 | 排除，不稽核 |
| `MatchWithoutQuota` | 中活動但無額度 | 排除，不稽核 |
| `Cancel` | 取消 | 排除，不稽核 |

---

## 券不足重試機制（背景知識）

`CalculateNextRetryBookingTime` 計算規則：每天 **10:00** 和 **22:00** 兩個固定時間點輪流重試，最長重試至 `活動結束日 + 90天`。

每次重試後 DDB 更新：
```
RewardStatus  → WaitToReward（不變）
UpdateReason  → "券不足，給券異常，本次已給予0張優惠券"
BookingTime   → 下一個 10:00 / 22:00
UpdateTime    → 本次執行時間
```

---

## 相關程式檔案

| 類別 | 路徑 |
|------|------|
| NMQ Job | `NMQv3Worker/Jobs/PromotionRewardAudit/BatchAuditCouponJob.cs` |
| DI 設定 | `NMQv3Worker/DI/PromotionRewardAudit/BatchAuditCouponJobDIAttribute.cs` |
| 稽核 Service（override） | `BL/Services/CouponAudits/AuditPromotionRewardCouponService.cs` |
| 稽核 Service（base） | `BL/Services/CouponAudits/BaseAuditCouponService.cs` |
| 給券 Service | `BL/Services/PromotionReward/CouponService.cs` |
| DDB Entity | `BE/PromotionReward/PromotionRewardCouponEntity.cs` |
| DDB Base Entity | `BE/PromotionReward/PromotionRewardBaseEntity.cs` |
| History Entity | `BE/PromotionReward/PromotionRewardCouponHistoryEntity.cs` |
| 稽核請求 Entity | `BE/PromotionReward/Requests/AuditCouponRequestEntity.cs` |
| 稽核資料 Entity | `BE/PromotionReward/AuditDatas/PromotionRewardCouponAuditDataEntity.cs` |
| ECoupon Repository | `DA.Repositories/Repository/ECoupon/ECouponRepository.cs` |
