# create-order-housekeeper — 邏輯說明

> PHP 檔案：`admin/daemon/create_order_housekeeper.php`
> 對應排程：台灣時間 08:00~00:59 每 1 分鐘（`*/1 00-16 * * *`）
> pg_id：3

---

## 一句話摘要

每分鐘用 UUID token 原子性佔用一批 `order_create_queue` 任務，對每筆呼叫 91APP API 取得首單資料，在本地建立 `regular_order`（計畫主表）+ `regular_order_detail`（第一期明細），完成後標記佇列已處理。

---

## 執行流程

```
main()
├── 產生 UUID token
├── queue.take_queue_job(token)   → 原子性佔用一批未處理佇列（UPDATE ocq_occupy_id = token）
├── queue.get_queue_job(token)    → 取出已佔用的佇列
└── foreach (ocq item)
    ├── V2_SalesOrder_Get()       → 取得首單訂單資料（TG/TS codes）
    ├── V2_RegularOrder_Get()     → 取得定期購計畫資料（頻率、商品、收件人...）
    ├── 驗證：OrderFrequency != 0（排除一次性訂單）
    ├── Regular_Product.load()    → 確認商品設定存在
    ├── Member_Info upsert        → 建立/更新會員資料
    ├── beginTransaction()
    │   ├── Regular_Order.insert() → 建立計畫主表（第1期 ship_count=1）
    │   └── Regular_Order_Detail.insert() → 建立第1期明細
    ├── commit()
    ├── Regular_Handler.regular_order_mapping() → 建立 mapping
    └── queue.ocq_status = 1（成功）/ 8（失敗）
```

---

## 關鍵設計細節

### 原子性佔用（防重複執行）
```php
 = Gb_Tools::get_uuid();
->take_queue_job();   // UPDATE ocq_occupy_id = token WHERE ocq_occupy_id IS NULL
 = ->get_queue_job();  // SELECT WHERE ocq_occupy_id = token
```
> 即使多個 Pod 同時執行，UUID token 確保每筆佇列只被一個 Pod 處理。

### 首期出貨日計算
```
rgo_ship_date      = FirstOrderDate + 2 天
rgo_next_ship_date = FirstOrderDate + 2天 + OrderFrequency 天
```
> `ship_date_from_create_count = 2` 寫死，需在新版確認是否需可設定。

### RegularOrderProvider 寫死為 "GameBall"
```php
"RegularOrderProvider" => "GameBall",
```
> **⚠️ 翻寫重點**：新版需改為 `"Nine1"`。

---

## 涉及資料表

| 資料表 | 操作 | 說明 |
|---|---|---|
| `order_create_queue` | UPDATE + SELECT + UPDATE | 佔用、取出、標記完成 |
| `regular_order` | INSERT | 建立計畫主表 |
| `regular_order_detail` | SELECT（防重複）+ INSERT | 第一期明細 |
| `regular_product` | SELECT | 確認商品設定 |
| `member_info` | SELECT + INSERT/UPDATE | 會員資料 upsert |

---

## 外部 API 呼叫

| API | 用途 |
|---|---|
| `V2_SalesOrder_Get` | 取得首單訂單資料 |
| `V2_RegularOrder_Get` | 取得定期購計畫設定（頻率、商品...） |

---

## 翻寫 .NET Core 注意事項

1. **RegularOrderProvider 改為 "Nine1"**：這是最重要的改動之一。
2. **原子性佔用機制**：`take_queue_job` 的 UUID token 機制需在新版保留，或改用 NMQ 的 exclusive consumer 達到同等效果。
3. **DB Transaction**：INSERT regular_order + regular_order_detail 在同一個 transaction，新版必須保留。
4. **首期出貨日 +2 天**：`ship_date_from_create_count = 2` 是否需改為可設定，需確認。
5. **ocq_status = 8**：失敗狀態碼，新版需定義等效的失敗狀態。
