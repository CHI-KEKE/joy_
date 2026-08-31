# QA 釐清紀錄 #3

> 日期：2026-06-16

---

## 一、轉單後 SalesOrderSlave_StatusDef 的賦值邏輯

**檔案：`csp_TradesOrderTransToSalesOrderWithFlow_Mall.sql`**

- `CreditCardOnce_Razer` 的 `@orderPayProfileDef = 'CreditCardOnce_Razer'`，不等於 `'CreditCardOnce'`
- 所以 L1015 的 `CreditCardOnce` 判斷**不命中**
- 落到 L1030：`@sourcePayTypes` 清單包含 `CreditCardOnce_Razer` → `WaitingThirdPartyPaymentCheck`
- 這是**轉單當下的初始狀態**，非永久狀態
- 後續 Razer webhook 回來 → SalesOrder 變 `WaitingToShipping`

```
CASE 優先順序（節錄）：
  L1013: TransToCancel → Fail
  L1015: @orderPayProfileDef IN ('CreditCardOnce',...) → 依授權結果
  L1029: ATM/CustomOfflinePayment → WaitingToPay
  L1030: @sourcePayTypes（含 CreditCardOnce_Razer）→ WaitingThirdPartyPaymentCheck
  L1031: ELSE → WaitingToShipping
```

---

## 二、Razer 付款兩種路徑

| 情境 | 付款方式 | 流程 |
|---|---|---|
| 首期消費者結帳 | 跳付款頁（帶卡號） | Return URL → ThirdPartyFinishProcess → TransferOrderProcessor |
| 後期 AutoGen 自動扣款 | 帶 card_token（server-to-server） | SubscriptionOrderAutoGen → Pending/Success → TransferOrderProcessor |

- 首期：消費者主動完成，不走 AutoGen
- 後期：`RegularCreateOrderJob` 呼叫 `SubscriptionOrderAutoGen`，直接用 token 扣款

---

## 三、card_token 儲存鏈路

1. 首期消費者結帳 → 跳 Razer 付款頁
2. Return URL → `PaymentMiddlewareService.QueryPayment` → `AppendCardInfo`
   → `context.ThirdPartyPaymentInfo.ExtendInfo["creditcard_token"]`
3. `ThirdPartyFinishProcess` flow → `CreateRegularOrderProcessor` L249-255
   → 存 `credit_card_token` 進 Regular Order DB（MY + CreditCardOnce_Razer 條件）
4. 後期 AutoGen 時直接讀取此 token 送 Razer

---

## 四、Pending Job 的設計邊界

### rod_status = 3 只會出現在第 2 期以後

- **第 1 期**：`RegularOrderInitJob` 建立，`rod_status = 1`
  - 消費者在前台自行結帳，不走 AutoGen，不會產生 `rod_status=3`
- **第 2 期以後**：`RegularCreateOrderJob` Step④ 呼叫 `SubscriptionOrderAutoGen`
  - 回傳 `WaitingToPay` → `UpdateAfterApiPendingAsync` → `rod_status = 3`

### Pending 機制只有當天有效

```
rod_daemon_active_time = 今天 → PendingJob 能撈到
rod_daemon_active_time = 昨天 → 永遠撈不到（設計限制）
```

### WaitingToPay 對齊 PHP 行為

- 定期購只有 MY 使用 `CreditCardOnce_Razer`，後期 AutoGen 一定用 card_token
- `WaitingToPay` 代表 Razer 扣款還在 Pending 中
- 對齊 PHP：不在 `availableStatus` 內 → 直接標 `CreditCardFail`
- 這是 **PHP 已知設計限制**，非預期行為（詳見 0616.md）

### Region 付款方式對照

| Region | QA 付款方式 |
|---|---|
| TW | CreditCardOnce |
| HK | CreditCardOnce_Stripe |
| MY | CreditCardOnce_Razer |

---

## 五、EF Core Identity Cache 問題與修正

### 問題根源

EF Core DbContext 有 **Identity Map（一級快取）**：

> 同一 DbContext 實例，對同一 PK 查兩次，第二次直接回傳 cache 物件，不打 DB。

### 問題情境

```
GetPendingDetailsAsync() → load rod_pk=1 進 Identity Cache
ResolvePendingSuccessAsync(1) → FirstOrDefaultAsync → 回傳 cache 舊物件
改屬性 → SaveChangesAsync → change tracking 比對不穩定 → SQL 不一定發出
```

### 修正方式

改用 `ExecuteUpdateAsync` 直接產生 SQL，完全繞過 tracking：

```csharp
await _dbContext.RegularOrderDetails
    .Where(r => r.RodPk == rodPk)
    .ExecuteUpdateAsync(s => s
        .SetProperty(r => r.RodStatus, (sbyte)1)
        .SetProperty(r => r.RodDaemonActiveTime, now),
    cancellationToken);
```

### 受影響並已修正的方法

| 方法 | Repository |
|---|---|
| `ResolvePendingSuccessAsync` | RegularOrderDetailRepository |
| `ResolvePendingFailAsync` | RegularOrderDetailRepository |
| `UpdateAfterApiSuccessAsync` | RegularOrderRepository |
| `UpdateAfterApiFailAsync` | RegularOrderRepository |
| `UpdateStatsAsync` | RegularDailyCreateRepository |

---

## 六、Program.cs Job 路由表

新 Job 必須在 `Program.cs` 的 `map.Add` 登記，否則啟動時拋 `NotImplementedException`：

```csharp
map.Add("RegularCheckPendingPayment", typeof(RegularCheckPendingPaymentJob));
```
