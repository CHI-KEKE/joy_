# 0616 思考紀錄

## RegularCheckPendingPaymentJob — WaitingToPay 的處理問題

### 背景

定期購建單流程：
1. `RegularCreateOrderJob`（每日 10:15）呼叫 `SubscriptionOrderAutoGen` API
2. 若 API 回傳 `OrderSlaveFlowStatus = "WaitingToPay"` → 標記 `rod_status = 3`（Pending）
3. `RegularCheckPendingPaymentJob`（待定時間）呼叫 `V2/SalesOrder/Get` 確認付款狀態
4. 若 `OrderStatus IN [WaitingToShipping, ConfirmedToShipping, Finish]` → 付款成功，推進週期
5. 其他狀態 → 標記 `CreditCardFail`

### 問題

`WaitingToPay` 代表**扣款指令已送出，支付閘道尚未回傳確認**，是一個非同步付款的中間狀態。

若 `RegularCheckPendingPaymentJob` 跑的當下訂單還是 `WaitingToPay`（支付閘道尚未回應），PHP 的策略是**直接標為 `CreditCardFail`**（建單失敗）。

這代表：若支付閘道在 Pending Job 執行後才回傳成功，系統會錯誤地將一筆已付款成功的訂單標為失敗。

### PHP 現況行為（已知風險）

```
CreateOrder 10:15 → WaitingToPay → rod_status=3
PendingJob  10:45 → 查到 OrderStatus=WaitingToPay → 標 CreditCardFail
（若 10:46 支付閘道才確認成功 → 訂單已被誤判為失敗）
```

失敗後 `rod_create_order_status=2`，讓下一天 `RegularCreateOrderJob` Step④ 重試建單。

### .NET 版本決策（待定）

**選項 A：對齊 PHP，WaitingToPay = CreditCardFail**
- 行為一致，不改變現有邏輯
- 誤判風險與 PHP 相同

**選項 B：跳過（continue），不處理 WaitingToPay，等下一次 Pending Job 再查**
- 需搭配「查所有 rod_status=3 不限日期」的查詢條件
- 支付閘道只要在某次 Pending Job 前回應，就能正確推進
- 風險：若付款真的失敗，rod_status=3 會卡住直到手動處理或另外設逾期機制

**選項 C：加逾期機制，超過 N 天仍 WaitingToPay → 才標 CreditCardFail**
- 最完整，但需要定義逾期天數

> **目前尚未決定，待與 PM / 後端同仁確認支付閘道的實際回應時間後定案。**
