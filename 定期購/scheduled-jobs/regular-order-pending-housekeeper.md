# regular-order-pending-housekeeper — 邏輯說明

> PHP 檔案：`admin/daemon/regular_order_pending_housekeeper.php`
> 對應排程：未在 Rancher 找到（可能尚未部署或已整合到其他排程）
> pg_id：9

---

## 一句話摘要

查詢今日 Pending 狀態（`rod_status = 3`）的期明細，逐一呼叫 91APP API 查詢訂單狀態，若訂單已進入有效狀態（WaitingToShipping/ConfirmedToShipping/Finish）則解除 Pending、推進出貨週期；否則標記為刷卡失敗。最後統計今日建單總數更新 `regular_daily_create`。

---

## 執行流程

```
main()
├── current_date = today
├── order_detail.find_pending_order_by_date(today) → 取今日 Pending 訂單
└── foreach pending_order
    ├── regular_order.load_by_pk()
    ├── regular_order_detail.load_by_pk()
    ├── api.V2_SalesOrder_Get(shop_id, tg_code, '', ts_code) → 查訂單狀態
    ├── 訂單狀態在 [WaitingToShipping, ConfirmedToShipping, Finish]？
    │   ├── YES（check_result = true）→ 解除 Pending，推進週期
    │   │   ├── rod_status = 1
    │   │   ├── rgo_in_schedule = 0
    │   │   ├── rgo_ship_date = rgo_next_ship_date
    │   │   ├── rgo_next_ship_date = rgo_ship_date + rgo_ship_period 天
    │   │   ├── rgo_ship_count = rod_ship_sequence
    │   │   ├── rgo_create_order_status = 1
    │   │   └── 若達到 ship_limit_count → rgo_is_complete=1, rgo_status=3（完成）
    │   └── NO（check_result = false）→ 標記刷卡失敗
    │       ├── error_code = "CreditCardFail"
    │       ├── rgo_create_order_status = 2
    │       └── rod_create_order_status = 2
    ├── regular_order.update()
    ├── regular_order_detail.update()
    └── regular_handler.regular_order_mapping()
└── check_order_create()  → 統計今日成單數，UPDATE regular_daily_create
```

---

## Pending 狀態說明

Pending（`rod_status = 3`）由 `regular-create-housekeeper` 的 `take_order_api()` 設定：
```
API 回傳 OrderSlaveFlowStatus = 'WaitingToPay' && Status = 'Success'
→ rod_status = 3（Pending）
```
表示訂單已建立但尚在等待付款確認，需此 job 後續追蹤。

---

## 有效訂單狀態

```php
 = ["WaitingToShipping", "ConfirmedToShipping", "Finish"];
```
> 付款通過後訂單會進入這些狀態，代表刷卡成功。

---

## 涉及資料表

| 資料表 | 操作 | 說明 |
|---|---|---|
| `regular_order_detail` | SELECT + UPDATE | 查 Pending 訂單、更新狀態 |
| `regular_order` | SELECT + UPDATE | 推進出貨週期、更新成單狀態 |
| `regular_daily_create` | SELECT + UPDATE | 統計今日成單數 |

---

## 外部 API 呼叫

| API | 用途 |
|---|---|
| `V2_SalesOrder_Get` | 查詢 Pending 訂單的最新狀態 |

---

## Pending 機制的設計限制

```
同一天：
  Step ④ 跑 → rod_status = 3，rod_daemon_active_time = 今天
  pending_housekeeper 跑（同天稍晚）→ 能撈到，處理一次

隔天以後：
  rod_daemon_active_time = 昨天 → SQL 條件不符，永遠撈不到
```

**結論：Pending 機制只有當天有效。**

這個設計只適用於「API 非同步但快速確認」的場景（如信用卡授權延遲數秒）。  
對於需要等待多天的付款方式（如 ATM 轉帳），當天若未確認就會被視為失敗，  
消費者會收到錯誤的「建單失敗」通知，實際上訂單已存在只是還未付款。

**不論成功或失敗，`rod_status` 都設回 `1`（解除 Pending）。**

---

## 翻寫 .NET Core 注意事項

1. **Pending 機制**：`rod_status = 3` 是由 regular-create 設的，此 job 負責解除，兩支 job 有狀態依賴，翻寫時需一起考慮。
2. **失敗固定寫 "CreditCardFail"**：目前 check_result = false 時 error_code 硬寫為 `"CreditCardFail"`，實際上可能有其他失敗原因，新版可考慮改進。
3. **出貨週期推進邏輯**：`rgo_next_ship_date = rgo_ship_date + rgo_ship_period` 這段邏輯與 regular-create 的邏輯需保持一致。
4. **計畫完成判斷**：`rgo_ship_count >= rgo_ship_limit_count` 時標記完成（rgo_status=3），新版需保留此邊界判斷。
5. **排程未在 Rancher 找到**：此 job 在 QA 環境可能尚未部署，需確認是否應與 regular-create 合並執行或另外部署。
