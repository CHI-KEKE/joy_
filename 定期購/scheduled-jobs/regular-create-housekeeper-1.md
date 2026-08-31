# regular_create_housekeeper — Part 1：核心概念與資料模型

> 文件版本：v1.0  
> 更新日期：2026-05-22  

---

## 1. 這支 CronJob 在做什麼

**負責每天自動幫所有定期購計畫建立下一期的真實訂單。**

消費者訂購後，第 1 期的訂單是結帳當下建立的；從第 2 期起，每一期訂單都由這支排程每天定時觸發建立。

---

## 2. 核心資料模型

### 兩張表的關係

```
regular_order（主計畫）                regular_order_detail（每一期配送紀錄）
─────────────────────                 ──────────────────────────────────────
一個訂閱 = 一筆 rgo                    每一期 = 一筆 rod

rgo_pk              = 1               rod_pk=1, rod_rgo_pk=1, rod_ship_sequence=1 ← 第1期（首單）
rgo_source_ts       = TS001           rod_pk=2, rod_rgo_pk=1, rod_ship_sequence=2 ← 第2期（續購）
rgo_ship_period     = 30（天）         rod_pk=3, rod_rgo_pk=1, rod_ship_sequence=3 ← 第3期
rgo_ship_limit_count= 6（共幾期）      ...
rgo_ship_date       = 5/3（本期）
rgo_next_ship_date  = 6/2（下期）
rgo_ship_count      = 1（已完成幾期）
```

- `regular_order` = 「這個人訂了一個每 30 天配送一次、共 6 期的計畫」
- `regular_order_detail` = 「這個計畫的第 2 期，預計 6/2 出貨，建單狀態是待處理」

---

## 3. 兩個關鍵常數

| 常數 | 值 | 用途 |
|------|----|------|
| `GENERATE_NEW_ORDER_DIFF_DAYS` | 2 | 提前幾天建立 detail 佔位紀錄 |
| `CALL_API_AND_GET_NEW_ORDER_IN_DAYS` | 2 | 配送日前幾天內去打 API 建真實訂單 |

---

## 4. 「建立 detail」vs「打 API 建訂單」是兩件事

| 動作 | 說明 | 結果 |
|------|------|------|
| **Step ③ 建立 detail** | 在 DB 先建一筆佔位紀錄 | `rod_create_order_status = 0`（尚無真實訂單） |
| **Step ④ 打 API** | 呼叫 `SubscriptionOrderAutoGen`，在 mweb 建立真實訂單 | 成功後 rod 拿到真實 `TGCode/TSCode`，`status = 1` |

**為什麼要分兩步？**

提前建立 detail 讓系統知道「這筆要處理」，並提供 2 天的 API 重試窗口（配送日前 1 天和前 2 天各觸發一次）。

---

## 5. 配送日計算公式

### 第 1 期（首單建立時計算）
```
rgo_ship_date      = FirstOrderDate + 2天
rgo_next_ship_date = FirstOrderDate + ship_period + 2天
```

> **+2 天**只在首單建立時出現一次，代表「給倉庫 2 天準備時間」。

### 第 2 期以後（每次 Step ④ 成功後更新）
```
rgo_ship_date      = rgo_next_ship_date           ← 直接搬過來
rgo_next_ship_date = rgo_ship_date + ship_period  ← 只加週期，不再 +2
```

**因此週期從第 2 期起就是精準的 ship_period 天，不會每期都多 2 天。**

---

## 6. 完整時間軸範例

條件：`FirstOrderDate = 5/1`，`ship_period = 30天`，`ship_limit_count = 6`

### 第 1 期（結帳當下，不經過此排程）
```
5/1   消費者結帳
       ├─ regular_order 建立
       │    rgo_ship_date      = 5/3（5/1 + 2）
       │    rgo_next_ship_date = 6/2（5/1 + 30 + 2）
       │    rgo_ship_count     = 1
       └─ regular_order_detail seq=1 建立
            rod_ship_date            = 5/3
            rod_create_order_status  = 1（首單本身就是真實訂單，不需打 API）
```

### 第 2 期（regular_create_housekeeper 負責）
```
5/31  CronJob 執行
       Step ③：DATEDIFF(rgo_next_ship_date=6/2, today=5/31) = 2
               → 觸發！INSERT rod seq=2
                  rod_ship_date           = 6/2
                  rod_create_order_status = 0（待建單）

       Step ④：DATEDIFF(rod_ship_date=6/2, today=5/31) = 2 <= 2
               → 觸發！呼叫 SubscriptionOrderAutoGen
               → 成功：rod_tg_code = "TG...", rod_status = 1
               → 更新主計畫：
                  rgo_ship_date      = 6/2
                  rgo_next_ship_date = 7/2（6/2 + 30）
                  rgo_ship_count     = 2

6/1   CronJob 執行（若 5/31 失敗，今天重試）
       Step ③：DATEDIFF(6/2, 6/1) = 1 ≠ 2 → 不觸發（已在 5/31 建好）
       Step ④：DATEDIFF(6/2, 6/1) = 1 <= 2 → 重試打 API
```

### 第 3 期
```
6/30  CronJob 執行
       Step ③：DATEDIFF(7/2, 6/30) = 2 → 建立 rod seq=3
       Step ④：打 API → 成功
               rgo_next_ship_date = 8/1（7/2 + 30）
               rgo_ship_count     = 3
```

### 完結
```
當 rgo_ship_count >= rgo_ship_limit_count（6）時
  → rgo_status = 3（計畫結束）
```

---

## 7. regular_order_detail 的 status 流轉

```
建立時 status = 0（待建單）
    │
    ▼ Step ④ 打 API
    ├─ 成功                → status = 1（已建單）
    ├─ 成功但待付款         → status = 3（pending，由另一排程處理）
    └─ 失敗                → status = 2（失敗，等下次重試）
                                  │
                                  ▼ 若配送日已過
                              Step ② 將 status = 2 → 廢棄
                              並將主計畫 rgo_next_ship_date 推進一個週期
```

---

## 8. 第 1 期與第 2 期以後的本質差異

| | 第 1 期 | 第 2 期以後 |
|-|---------|------------|
| **誰建立** | `create_order_housekeeper`（首單流程） | `regular_create_housekeeper`（本排程） |
| **rod 初始 status** | `1`（已建單） | `0`（待建單） |
| **是否需要打 API** | ❌（首單本人就是真實訂單） | ✅（需呼叫 SubscriptionOrderAutoGen） |
| **TGCode / TSCode** | 來自首單結帳 | 來自 API 回傳 |
