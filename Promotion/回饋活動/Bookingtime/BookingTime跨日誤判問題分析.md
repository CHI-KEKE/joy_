# BookingTime 跨日誤判問題分析

## 一、背景說明

### BookingTime 的用途

`BookingTime`（DDB 欄位：`BookingTimeUTC`）是 DynamoDB 回饋活動主檔中的核心排程時間欄位，代表「預定執行回饋作業的時間點」。

系統透過 `BookingDateUTC-BookingTimeUTC-index` 這個 GSI（全域次要索引）來查詢「今天應該被執行的回饋記錄」。

```bash
PromotionRewardService / CouponService
  └─ Query DDB GSI: BookingDateUTC-BookingTimeUTC-index
       └─ 撈出 BookingDate = Today, BookingTime <= Now 的記錄
            └─ 送進正流程或稽核流程處理
```

---

## 二、正流程（給點）如何設定 BookingTime

**入口位置：** `EComOrderPromotionRewardCalculateService.GetRewardConditionSetting()`

**來源檔案：**
```bash
src/Nine1.Promotion.Console.BL/Services/Promotions/EComOrderPromotionRewardCalculateService.cs
```

### 計算邏輯（線上訂單 ECom）

```csharp
//// 預設為訂單成立時間
var bookingTime = orderTime;

if (condition.RewardTimingType == nameof(RewardTimingTypeEnum.ByDateTime) && condition.RewardDateTime.HasValue)
{
    //// 若為指定日期，則使用指定日期
    bookingTime = condition.RewardDateTime.Value;
}
else
{
    if (status == SalesOrderSlaveStatusDefEnum.WaitingToShipping)
    {
        //// 即時給點：直接使用 OrderTime
        bookingTime = orderTime;
    }
    else
    {
        //// 當回饋天數大於 0 時，將時分秒歸零確保日期計算的一致性
        bookingTime = days > 0 ? orderTime.Date.AddDays(days) : orderTime.AddDays(days);
    }
}
```

### 關鍵結論

| 活動類型 | BookingTime 計算基準 | 時分秒處理 |
|---------|-------------------|-----------|
| 即時給點（WaitingToShipping） | `OrderTime` | 保留原始時分秒 |
| `+N 天`（RewardDays > 0） | `OrderTime.Date.AddDays(N)` | **時分秒歸零** |
| `+0 天` | `OrderTime.AddDays(0)` | 保留原始時分秒 |
| 指定日期 (ByDateTime) | `RewardDateTime` | 使用設定值 |

> **重點：`+N 天` 模式以 `OrderTime` 為基準，且時分秒歸零（取日期部分）。**

---

## 三、稽核器如何驗證 BookingTime

**入口位置：** `PromotionRewardBookingTimeAuditor.VerifyBookingTimeForUnshippedOrder()`

**來源檔案：**
```bash
src/Nine1.Promotion.Console.BL/Services/PromotionReward/Auditors/PromotionRewardBookingTimeAuditor.cs
```

### 稽核路由決策樹（未出貨訂單）

```bash
AuditLoyaltyPointsRewards()
  └─ foreach reward in LoyaltyPromotionRewards
       ├─ ShouldSkipRewardAudit()    ← 若狀態為 MatchWithoutQuota / Unmatch / Cancel / Reward → Skip
       ├─ hasShipped && !fixedDateTime  → VerifyBookingTimeForShippedOrder()
       ├─ fixedDateTime                 → VerifyBookingTimeForRewardDateTime()
       └─ 其餘（未出貨）               → VerifyBookingTimeForUnshippedOrder()   ← 問題所在
```

### VerifyBookingTimeForUnshippedOrder 決策流程

```csharp
bool isOfflineOrder = tgCode.Contains("CrmSalesOrder:");
bool hasDelayBookingTime = HasDelayBookingTime(dynamicReward);

if (isOfflineOrder)
{
    expectedBookingtime = OrderTime.AddDays(RewardDays);          // 線下單：始終用 OrderTime
}
else if (hasDelayBookingTime == false && RewardDays > 0)
{
    expectedBookingtime = OrderTime.AddDays(RewardDays);          // 暫解：從未延遲過，用 OrderTime
}
else
{
    expectedBookingtime = UpdateTime.AddDays(RewardDays);         // 一般情況：用 UpdateTime
}

if (RewardDays > 0)
{
    expectedBookingtime = expectedBookingtime.Date;               // 時分秒歸零
}

bool isCorrect = Math.Abs((expectedBookingtime - actual).TotalHours) < 1;  // 容許 1 小時誤差
```

---

## 四、問題根因：跨日誤判

### 根本原因

在 `+N 天` 活動模式下：

- **正流程**（`EComOrderPromotionRewardCalculateService`）以 **`OrderTime`** 為基準計算 BookingTime
- **舊稽核邏輯**（修改前）以 **`UpdateTime`** 為基準計算期望的 BookingTime

當訂單成立時間（`OrderTime`）與正流程執行時間（`UpdateTime`）剛好跨越台灣時間的午夜，兩者在加上 `+N 天` 後會落在**不同的日期**，導致稽核誤判，出現 24 小時差異的假警報。

### 時間差異示意圖

```bash
台灣時間（UTC+8）的午夜 = UTC 16:00
                          ↓
             ┌────────────┼────────────┐
             │    Nov 5   │   Nov 6    │  (台灣時間)
             │  UTC+8     │  UTC+8     │
─────────────┼────────────┼────────────────── UTC 時間
  OrderTimeUTC            │              UpdateTimeUTC
  2025-11-05 15:54        │              2025-11-05 16:02
  (台灣: 11/5 23:54)      ↑              (台灣: 11/6 00:02)
                     UTC 16:00
                  = 台灣時間午夜 00:00
```

### 具體例子（程式碼註解中的真實案例）

```bash
BookingTimeUTC : 2025-11-12T16:00:00.0000000Z  (= 台灣時間 2025-11-13 00:00)
OrderTimeUTC   : 2025-11-05T15:54:52.2055854Z  (= 台灣時間 2025-11-05 23:54)
UpdateTimeUTC  : 2025-11-05T16:02:14.9945738Z  (= 台灣時間 2025-11-06 00:02)
活動設定        : +7 天
```

**正流程計算（使用 OrderTime）：**
```bash
OrderTime.Date = 2025-11-05（UTC 儲存，以 UTC 時區計算日期）
BookingTime = 2025-11-05.Date + 7 days = 2025-11-12T00:00:00Z  ✅ 符合 DDB 中的值
```

**舊稽核計算（使用 UpdateTime）：**
```bash
UpdateTime.Date = 2025-11-05（但時間已超過 15:54，為 16:02，UTC 日期相同）
expectedBookingtime = 2025-11-05.Date + 7 days = 2025-11-12T00:00:00Z
```

> ⚠️ 等等，上面 UTC 日期相同，怎麼會出問題？  
> **關鍵在於時區轉換**：系統可能在部分運算中使用本地時間（UTC+8），此時：
> - `OrderTime` 本地 = 2025-11-05 23:54 → 本地日期 = **11/05**
> - `UpdateTime` 本地 = 2025-11-06 00:02 → 本地日期 = **11/06** ← 已跨日！
>
> 若稽核器在取 `.Date` 之前已轉換為本地時間，則：
> ```
> 以 OrderTime 本地日期計算：11/05 + 7 = 11/12 → BookingDate = 11/12
> 以 UpdateTime 本地日期計算：11/06 + 7 = 11/13 → expectedDate = 11/13
> 差異 = 24 小時 → 超過 1 小時容許值 → ❌ 誤判為異常
> ```

---

## 五、HasDelayBookingTime 判斷邏輯

**位置：** `PromotionRewardBookingTimeAuditor.HasDelayBookingTime()`

```csharp
public const string SetBookingTimeRecordKeyWord = "設定BookingDataTime";

private bool HasDelayBookingTime(dynamic dynamicReward)
{
    IEnumerable<PromotionRewardHistoryBaseEntity>? rewardHistories =
        dynamicReward?.RewardHistoryList as IEnumerable<PromotionRewardHistoryBaseEntity>;

    if (rewardHistories != null && rewardHistories.Any())
    {
        hasDelayBookingTime = rewardHistories.Any(history =>
            string.IsNullOrEmpty(history.UpdateReason) == false &&
            history.UpdateReason.Contains("設定BookingDataTime"));
    }

    return hasDelayBookingTime;
}
```

### 說明

| 狀況 | `HasDelayBookingTime` | 代表意義 |
|-----|----------------------|---------|
| `RewardHistoryList` 中無「設定BookingDataTime」記錄 | `false` | BookingTime 從未被手動延遲，仍是正流程初始設定值（以 OrderTime 為基準） |
| `RewardHistoryList` 中有「設定BookingDataTime」記錄 | `true` | BookingTime 曾被延遲過（例如：未出貨、退貨中重新排程），此時應以 UpdateTime 為基準驗證 |

---

## 六、各情境演練

### 情境一：`+7 天`，OrderTime 與 UpdateTime 同日，無延遲歷史

```
OrderTime  = 2025-11-05 10:00 UTC
UpdateTime = 2025-11-05 10:05 UTC
RewardDays = 7
hasDelayBookingTime = false
isOfflineOrder = false

條件命中：hasDelayBookingTime == false && RewardDays > 0
expectedBookingtime = OrderTime + 7 days = 2025-11-12 10:00 UTC
.Date → 2025-11-12 00:00 UTC

BookingTime in DDB = 2025-11-12 00:00 UTC（正流程：OrderTime.Date + 7）
|差異| = 0 小時 < 1 小時 → ✅ 稽核通過
```

---

### 情境二：`+7 天`，跨日（問題情境），無延遲歷史（暫解保護）

```
OrderTime  = 2025-11-05 15:54 UTC（台灣: 11/05 23:54）
UpdateTime = 2025-11-05 16:02 UTC（台灣: 11/06 00:02）
RewardDays = 7
hasDelayBookingTime = false
isOfflineOrder = false

條件命中：hasDelayBookingTime == false && RewardDays > 0  ← 暫解生效
expectedBookingtime = OrderTime + 7 days = 2025-11-12 15:54 UTC
.Date → 2025-11-12 00:00 UTC

正流程 BookingTime = 2025-11-12 00:00 UTC（OrderTime.Date + 7）
|差異| = 0 小時 < 1 小時 → ✅ 稽核通過（暫解防止誤判）
```

> **無暫解的舊行為：**
> ```
> 命中 else 分支：expectedBookingtime = UpdateTime + 7 days = 2025-11-12 16:02 UTC
> .Date → 2025-11-12 00:00 UTC（UTC 時區下日期相同，故不誤判）
> ```
> ⚠️ 若系統在本地時間（UTC+8）計算 .Date：
> ```
> UpdateTime 本地 = 2025-11-06 00:02 → .Date 本地 = 2025-11-06
> expectedBookingtime = 2025-11-06 + 7 = 2025-11-13 00:00
> BookingTime in DDB = 2025-11-12 00:00
> |差異| = 24 小時 > 1 小時 → ❌ 誤判為異常（已被暫解修正）
> ```

---

### 情境三：`+7 天`，曾手動延遲過 BookingTime

```
OrderTime  = 2025-11-05 15:54 UTC
UpdateTime = 2025-11-10 08:00 UTC（延遲後更新的時間）
RewardDays = 7
hasDelayBookingTime = true（RewardHistoryList 含「設定BookingDataTime」）
isOfflineOrder = false

條件命中：else 分支
expectedBookingtime = UpdateTime + 7 days = 2025-11-17 08:00 UTC
.Date → 2025-11-17 00:00 UTC

BookingTime in DDB = 2025-11-17 00:00 UTC（延遲後重新計算：UpdateTime.Date + 7）
|差異| = 0 小時 < 1 小時 → ✅ 稽核通過
```

---

### 情境四：線下訂單（CrmSalesOrder）

```
TGCode = "CrmSalesOrder:12345"
OrderTime  = 2025-11-05 15:54 UTC
UpdateTime = 2025-11-05 16:02 UTC
RewardDays = 7
isOfflineOrder = true

條件命中：isOfflineOrder == true → 始終使用 OrderTime
expectedBookingtime = OrderTime + 7 days = 2025-11-12 15:54 UTC
.Date → 2025-11-12 00:00 UTC

BookingTime in DDB = 2025-11-12 00:00 UTC
|差異| = 0 小時 < 1 小時 → ✅ 稽核通過
```

---

### 情境五：即時給點（`+0 天`）

```
OrderTime  = 2025-11-05 15:54 UTC
UpdateTime = 2025-11-05 16:02 UTC
RewardDays = 0
hasDelayBookingTime = false

條件命中：else 分支（因 RewardDays == 0，不符合 hasDelayBookingTime==false && RewardDays>0）
expectedBookingtime = UpdateTime + 0 days = 2025-11-05 16:02 UTC
（RewardDays == 0，不歸零時分秒）

正流程 BookingTime = OrderTime = 2025-11-05 15:54 UTC
|差異| = 0.13 小時 ≈ 8 分鐘 < 1 小時 → ✅ 稽核通過（8 分鐘差異在允許範圍內）
```

---

### 情境六：指定日期（ByDateTime）

```
RewardTimingType = "ByDateTime"
RewardDateTime = 2025-12-01 00:00 UTC
稽核路由 → VerifyBookingTimeForRewardDateTime()（非 VerifyBookingTimeForUnshippedOrder）

稽核直接比對 BookingTime 與 RewardDateTime 的設定值
→ ✅ 或 ❌（依 DDB 中的值是否符合 ExtendInfo 設定）
```

---

### 情境七：已出貨訂單

```
訂單狀態全部為 Finish/Cancel → hasShipped = true
→ VerifyBookingTimeForShippedOrder()

expectedDate = max(lastShippingTime + RewardDays, ValidDateTime).Date
與 BookingDate（DDB 欄位）比對精確相等
→ ✅ 或 ❌
```

---

### 情境八：優惠券券不足（InsufficientECoupon）

```
最近 2 小時內有「券不足」歷史記錄
→ VerifyBookingTimeForInsufficientECoupon()

UpdateTime 10:00–22:00 → expectedDate = UpdateTime.Date 22:00
UpdateTime 22:00–隔天  → expectedDate = 隔天 10:00
UpdateTime 00:00–10:00 → expectedDate = 當天 10:00

精確比對（無容許誤差）
→ ✅ 或 ❌
```

---

## 七、稽核流程完整決策圖

```
VerifyBookingTime (未出貨路徑)
│
├─ isOfflineOrder?（TGCode 含 "CrmSalesOrder:"）
│    └─ YES → expectedTime = OrderTime + N days → .Date → 比對
│
├─ hasDelayBookingTime == false && RewardDays > 0?
│    └─ YES（暫解）→ expectedTime = OrderTime + N days → .Date → 比對
│
└─ else（曾延遲過，或 RewardDays == 0）
     └─ expectedTime = UpdateTime + N days（RewardDays > 0 時 .Date）→ 比對

比對：|expectedTime - actualBookingTime| < 1 小時 → PASS
      |expectedTime - actualBookingTime| ≥ 1 小時 → FAIL
```

---

## 八、暫解的限制與潛在風險

| 風險項目 | 說明 | 是否已處理 |
|---------|------|-----------|
| 跨日誤判（OrderTime vs UpdateTime） | 主要問題，已由暫解修正 | ✅ 已修正（暫解） |
| `HasDelayBookingTime` 關鍵字拼寫 | 程式碼中為 `"設定BookingDataTime"`（非 `"設定BookingDateTime"`），需確保與設定端保持一致 | ⚠️ 需持續確認 |
| `RewardDays = 0` 時仍以 UpdateTime 為基準 | 當 `RewardDays = 0` 且有跨日時，UpdateTime 與 OrderTime 差異在 8 分鐘以內（< 1 小時），仍在容許範圍 | ✅ 容許誤差覆蓋 |
| 曾延遲 + 再次跨日 | 若延遲後再次在跨日時間點執行，仍可能以 UpdateTime 計算而產生誤差 | ⚠️ 邊緣案例，尚未處理 |
| 線下訂單均強制用 OrderTime | 無論是否曾延遲，線下單始終用 OrderTime，若線下單曾手動延遲則可能錯誤通過 | ⚠️ 待評估 |

---

## 九、關鍵程式碼位置速查

| 功能 | 檔案路徑 | 方法 |
|-----|---------|------|
| 正流程 BookingTime 設定 | `BL/Services/Promotions/EComOrderPromotionRewardCalculateService.cs` | `GetRewardConditionSetting()` |
| 稽核 BookingTime 主入口 | `BL/Services/PromotionReward/Auditors/PromotionRewardBookingTimeAuditor.cs` | `Audit()` |
| 未出貨稽核（含暫解） | 同上 | `VerifyBookingTimeForUnshippedOrder()` |
| 已出貨稽核 | 同上 | `VerifyBookingTimeForShippedOrder()` |
| 延遲判斷 | 同上 | `HasDelayBookingTime()` |
| 指定日期稽核 | 同上 | `VerifyBookingTimeForRewardDateTime()` |
| 券不足稽核 | 同上 | `VerifyBookingTimeForInsufficientECoupon()` |
| DDB 欄位定義 | `BE/PromotionReward/PromotionRewardBaseEntity.cs` | `BookingTime`, `OrderTime`, `UpdateTime`, `RewardDays` |

---

## 十、總結

| 面向 | 說明 |
|-----|------|
| **問題** | `+N 天` 活動，正流程以 `OrderTime` 為基準，稽核舊邏輯以 `UpdateTime` 為基準，當兩者跨越台灣時間午夜時，導致期望 BookingDate 差 1 天（24 小時）的誤判 |
| **觸發條件** | `RewardDays > 0`，且 `OrderTime` 與 `UpdateTime` 在 UTC+8 時區下跨日（差距約 8 分鐘，但橫跨午夜） |
| **暫解** | 若 `RewardHistoryList` 中無「設定BookingDataTime」歷史，表示 BookingTime 從未手動延遲，改以 `OrderTime` 為基準驗證，與正流程對齊 |
| **容許誤差** | `1 小時`（`Math.Abs(difference.TotalHours) < 1`） |
| **完整解** | 建議正流程寫入 DDB 時，同步記錄計算 BookingTime 時所使用的基準時間（OrderTime 或 UpdateTime），稽核端直接讀取此值比對，避免依賴推算 |
