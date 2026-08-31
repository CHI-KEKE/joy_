# 定期購狀態欄位說明

> 本文件說明 `regular_order`（主計畫）與 `regular_order_detail`（每期紀錄）中  
> 四個關鍵狀態欄位的職責、差異與實際情境對照。

---

## 四個欄位一眼看懂

```
┌─────────────────────────────────────────────────────┐
│              regular_order（主計畫）                  │
│                                                     │
│  rgo_status        → 這個訂閱還活著嗎？               │
│  rgo_in_schedule   → 這期的排程作業走到哪了？          │
└─────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────┐
│          regular_order_detail（每期紀錄）             │
│                                                     │
│  rod_status              → 這筆 detail 還算數嗎？    │
│  rod_create_order_status → 這期 API 有沒有成功？      │
└─────────────────────────────────────────────────────┘
```

---

## 欄位詳細說明

### 1. `rgo_status`｜訂閱生命週期

| 值 | 意義 | 由誰設定 |
|---|---|---|
| `1` | 訂閱中（正常進行） | 建立訂閱時 |
| `2` | 暫停中 | 消費者手動暫停 |
| `3` | 已完成（達到期數上限） | Step ④ API 成功且 ship_count 達上限 |

---

### 2. `rgo_in_schedule`｜當期排程進度（防重複機制）

| 值 | 意義 | 由誰設定 |
|---|---|---|
| `0` | 閒置，可以開始建下一期 | 初始 / Step ④ 成功 / Step ② 延展 |
| `1` | 已建 detail，等待打 API | Step ③ |

---

### 3. `rod_status`｜這筆 detail 是否有效

| 值 | 意義 | 由誰設定 |
|---|---|---|
| `1` | 有效，正在使用 | Step ③ 建 detail 時 |
| `2` | 已廢棄（這期放棄不算） | Step ② 失敗延展時 |
| `3` | Pending（已建單但等付款） | Step ④ API 回傳 WaitingToPay |

---

### 4. `rod_create_order_status`｜這期建單結果

| 值 | 意義 | 由誰設定 |
|---|---|---|
| `0` | 待建單（初始值） | Step ③ |
| `1` | 建單成功 | Step ④ API 成功 |
| `2` | 建單失敗 | Step ④ API 失敗 |

---

## 狀態流轉圖

### 主計畫（regular_order）

```
rgo_status 流轉：

    [建立訂閱]
         │
         ▼
   rgo_status = 1（訂閱中）
         │
         ├──── 消費者暫停 ──────────────▶ rgo_status = 2（暫停）
         │                                      │
         │                               消費者恢復 │
         │                                      ▼
         │                              rgo_status = 1
         │
         └──── Step ④ 成功且達期數上限 ──▶ rgo_status = 3（完成）
```

```
rgo_in_schedule 流轉：

   rgo_in_schedule = 0
         │
         ▼ Step ③（距配送日剛好 2 天）
         │  INSERT detail
         ▼
   rgo_in_schedule = 1
         │
         ├── Step ④ API 成功 ──────▶ rgo_in_schedule = 0（解鎖，等下一期）
         │
         ├── Step ④ API 失敗 ──────▶ rgo_in_schedule 維持 1（隔天重試）
         │
         └── Step ② 過期延展 ───────▶ rgo_in_schedule = 0（廢棄這期，推進週期）
```

---

### 每期紀錄（regular_order_detail）

```
rod_create_order_status 流轉：

   Step ③ INSERT detail
         │
         ▼
   rod_create_order_status = 0（待建單）
         │
         ├── Step ④ 成功 ──▶ rod_create_order_status = 1（成功）
         │
         └── Step ④ 失敗 ──▶ rod_create_order_status = 2（失敗，隔天重試）
```

```
rod_status 流轉：

   Step ③ INSERT detail
         │
         ▼
   rod_status = 1（有效）
         │
         ├── Step ④ 成功（正常）    ──▶ rod_status 維持 1
         │
         ├── Step ④ 成功（Pending） ──▶ rod_status = 3（等付款）
         │                                    │
         │                        付款完成後其他排程處理 │
         │                                    ▼
         │                              rod_status = 1
         │
         └── Step ② 失敗延展時廢棄    ──▶ rod_status = 2（廢棄）
```

---

## 實際場景對照表

| 場景 | rgo_status | rgo_in_schedule | rod_status | rod_create_order_status |
|---|:---:|:---:|:---:|:---:|
| 剛建立訂閱，尚未到第 2 期 | 1 | 0 | （無 detail） | — |
| Step ③ 建了 detail，等打 API | 1 | 1 | 1 | 0 |
| Step ④ 打 API 失敗，隔天重試 | 1 | 1 | 1 | 2 |
| Step ④ 打 API 成功 | 1 | 0 | 1 | 1 |
| Step ④ 成功但訂單等待付款（Pending）| 1 | 1 | 3 | 1 |
| Step ② 延展：廢棄這期、推進週期 | 1 | 0 | 2 | 2 |
| 達到期數上限，訂閱完成 | 3 | 0 | 1 | 1 |
| 消費者暫停訂閱 | 2 | 0 | — | — |

---

## 具體範例

### 範例 A：正常三期訂閱（每月一次，共 3 期）

> 消費者 5/1 下單，配送週期 30 天，上限 3 期

```
時間軸：

5/1   消費者下單（首單）
      → 第 1 期 detail 由 create_order_housekeeper 建立
      → rod_create_order_status = 1（已成功）
      → rgo_ship_date = 5/3，rgo_next_ship_date = 6/2

6/1   （距 6/2 剩 1 天）Step ③ 尚未觸發
5/31  （距 6/2 剩 2 天）Step ③ 觸發
      → INSERT 第 2 期 detail
      → rgo_in_schedule = 1，rod_create_order_status = 0

6/1   Step ④ 觸發（距配送日 <= 2 天）
      → 打 API 成功
      → rod_tg_code、rod_ts_code 填入
      → rod_create_order_status = 1
      → rgo_in_schedule = 0
      → rgo_ship_date = 6/2，rgo_next_ship_date = 7/2
      → rgo_ship_count = 2

7/1   Step ③ 觸發（距 7/2 剩 1 天... 不對）
      距 7/2 剩 2 天是 6/30，Step ③ 在 6/30 觸發
      → INSERT 第 3 期 detail

7/1   Step ④ 觸發 → 打 API 成功
      → rgo_ship_count = 3 = rgo_ship_limit_count
      → rgo_is_complete = 1，rgo_status = 3（訂閱完成）
```

**最終狀態：**
- `rgo_status = 3`（完成）
- 共 3 筆 detail，`rod_create_order_status` 全為 1

---

### 範例 B：第 2 期 API 失敗後延展

> 消費者 5/1 下單，週期 30 天，無期數限制

```
5/31  Step ③ 建第 2 期 detail
      → rgo_in_schedule = 1，rod_create_order_status = 0

6/1   Step ④ 打 API → 失敗
      → rod_create_order_status = 2
      → rgo_create_order_status = 2
      → rgo_in_schedule 維持 1

6/2   Step ④ 重試 → 仍失敗
      → 狀態不變，繼續等

6/3   配送日（6/2）已過，Step ② 偵測到
      → rod_status = 2（廢棄這筆失敗 detail）
      → rgo_next_ship_date = 6/2 + 30 = 7/2（跳過這期，推進一個週期）
      → rgo_in_schedule = 0（解鎖，等下一期）

7/1   Step ③ 觸發 → INSERT 第 3 期 detail（期數從 rgo_ship_count + 1 算）
```

**注意：** 第 2 期被廢棄，`rgo_ship_count` 沒有 +1，消費者實際少收一次貨。

---

### 範例 C：第 2 期 API 回傳 Pending（等付款）

> 消費者選擇 ATM 轉帳付款

```
6/1   Step ④ 打 API → 成功，但 OrderSlaveFlowStatus = WaitingToPay
      → rod_tg_code、rod_ts_code 填入
      → rod_create_order_status = 1
      → rod_status = 3（Pending）
      → rgo_in_schedule 維持 1（主計畫不推進）

      後續由 regular_order_pending_housekeeper 處理：
      → 消費者付款後：rod_status = 1，主計畫推進（同成功流程）
      → 逾時未付：視情況處理
```

---

## 常見混淆釐清

| 問題 | 答案 |
|---|---|
| `rod_create_order_status = 1` 代表訂閱完成嗎？ | 不是，只代表「這期」建單成功，訂閱完成看 `rgo_status = 3` |
| `rgo_in_schedule = 1` 時 Step ③ 還會執行嗎？ | 不會，SQL 條件限制 `rgo_in_schedule = 0` 才撈 |
| `rod_status = 2` 和 `rod_create_order_status = 2` 一定同時出現嗎？ | 不一定。`rod_status = 2` 是 Step ② 廢棄時設的，`rod_create_order_status = 2` 是 Step ④ 失敗時設的，通常一起出現但邏輯不同 |
| Pending 的 detail `rod_create_order_status = 1` 但 `rod_status = 3`，矛盾嗎？ | 不矛盾。`create_order_status = 1` 說明 API 有成功打出去，`rod_status = 3` 說明訂單在等付款，兩個維度不同 |
