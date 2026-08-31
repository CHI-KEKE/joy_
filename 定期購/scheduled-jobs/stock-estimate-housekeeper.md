# stock-estimate-housekeeper — 邏輯說明

> PHP 檔案：`admin/daemon/stock_estimate_housekeeper.php`
> 對應排程：每週一台灣時間 10:00（`0 02 * * 1`）
> pg_id：7

---

## 一句話摘要

每週一掃描未來 3 週的定期購預計出貨訂單，依商店/商品彙整預估出貨量，為每家商店寫入一筆 `StockNotify` 到 `notify_queue`，後續由 notify-queue-housekeeper 呼叫庫存通知 API。

---

## 觸發 SQL（每週各跑一次）

```sql
SELECT rgo_shop_id, rgo_page_id, rpd_title, SUM(rgo_qty) AS rgo_qty
FROM regular_order
LEFT JOIN regular_product ON rgo_page_id = rpd_page_id AND rgo_shop_id = rpd_shop_id
WHERE rgo_status = 1
  AND rgo_next_ship_date >= :start_date
  AND rgo_next_ship_date <= :end_date
GROUP BY rgo_page_id
```

> 以 `rgo_page_id` 分組，算出每個商品在該週的預計出貨總量。

---

## 執行流程

```
main()
├── week = 1, 2, 3（未來 3 週）
│   ├── start_date = today + week*7 天
│   ├── end_date   = start_date + 6 天
│   └── regular_order.get_product_next_delivery_by_date(start_date, end_date)
│       → 依 shop_id + page_id 彙整 → shop_queue[shop_id][page_id][]
└── foreach shop_queue
    ├── 整理 payload（Cycle, SalePageTitle, SalePageId, Qty）
    └── notify_queue.load_by_shop_and_type(shop, 'StockNotify')
        → 不存在才 INSERT（防重複）
```

---

## 資料結構

### shop_queue 結構
```
shop_queue = {
    "shop_id_1": {
        "page_id_1": [
            { "Cycle": 1, "SalePageTitle": "...", "Qty": 3 },
            { "Cycle": 2, "SalePageTitle": "...", "Qty": 5 },
        ],
        "page_id_2": [...],
    },
    "shop_id_2": {...}
}
```

### notify_queue payload（nq_content）
```json
[
    { "Cycle": 1, "SalePageTitle": "商品A", "SalePageId": "123", "Qty": 3 },
    { "Cycle": 2, "SalePageTitle": "商品A", "SalePageId": "123", "Qty": 5 },
    { "Cycle": 1, "SalePageTitle": "商品B", "SalePageId": "456", "Qty": 2 }
]
```

> 每家商店一筆 StockNotify，payload 包含該店所有商品未來 3 週的預估量。

---

## 防重複機制

```php
if(!->load_by_shop_and_type(, 'StockNotify')){
    // 不存在才 INSERT
}
```
> 以 (nq_ref_shop, nq_type) 為唯一鍵防止重複，本週已建就不再建。

---

## 涉及資料表

| 資料表 | 操作 | 說明 |
|---|---|---|
| `regular_order` | SELECT | 查未來 3 週出貨計畫（依日期範圍） |
| `regular_product_define` | SELECT（JOIN） | 取商品標題 `rpd_title` |
| `notify_queue` | SELECT + INSERT | 防重複後寫入庫存通知佇列 |

---

## 外部依賴

| 依賴 | 說明 |
|---|---|
| MySQL | 全部操作 |
| **無** 91APP API | 只寫佇列，實際 API 由 notify-queue-housekeeper 呼叫 |

---

## 翻寫 .NET Core 注意事項

1. **nq_ref_order = 0, nq_ref_member = 0**：StockNotify 是店家層級的通知，不綁定特定訂單或會員，新版需注意 nullable 處理。
2. **未來 3 週範圍**：`week <= 3` 硬寫，新版可考慮改為設定檔可調整。
3. **防重複鍵**：以 `(nq_ref_shop, nq_type='StockNotify')` 唯一，翻寫時需確認 DB constraint 或程式層防重複。
4. **StockNotify 用不同 API**：此 job 建的通知由 notify-queue-housekeeper 用 `V2_RegularOrder_EstimatedQtyNotify` 發送，與其他通知類型不同。
5. **週排程風險**：失敗後要等下週一，建議加上 Slack 告警或手動重跑機制。
