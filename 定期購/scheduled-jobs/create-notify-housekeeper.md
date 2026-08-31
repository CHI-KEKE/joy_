# create_notify_housekeeper

> PHP 檔案：`admin/daemon/create_notify_housekeeper.php`
> pg_id：5

## 職責

依據「距配送日天數」與「是否有失敗錯誤碼」，將應發的通知 INSERT 進 `notify_queue` 表。  
**本排程不直接發送通知**，實際發送由下一支排程 `notify_queue_housekeeper` 負責。

---

## 整體流程

```
main()
  │
  ├─ notify_cash_on_delivery()       貨到付款預告 → 距配送日 3 天前
  ├─ notify_swipe_credit_card()      信用卡扣款預告 → 距配送日 3 天前
  ├─ notify_swipe_credit_card_fail() 信用卡失敗通知 → 距配送日 1 天前
  └─ notify_create_order_fail()      建單失敗通知 → 距配送日 1 天前
```

---

## 4 種通知類型

| 通知類型 | 觸發天數 | 篩選條件 | 說明 |
|---|:---:|---|---|
| `CashOnDeliveryOrderNotify` | D-3 | 付款方式**不含** `CreditCardOnce` | 提醒消費者即將到貨，請準備現金 |
| `CreditCardOrderNotify` | D-3 | 付款方式**包含** `CreditCardOnce` | 提醒消費者即將自動扣款 |
| `CreditCardFailedNotify` | D-1 | 有錯誤碼 + 錯誤碼為 `CreditCardBlackList` 或 `CreditCardFail` | 通知信用卡刷卡失敗 |
| `CreateFailedNotify` | D-1 | 有錯誤碼 + 錯誤碼**非**信用卡類 | 通知建單失敗（其他原因） |

---

## 觸發 SQL（共用）

```sql
SELECT mbi_vip_mbid, rgo_pk, rgo_shop_id, rgo_next_ship_date,
       rgo_ship_type, rgo_pay_type, rgo_product_name, rgo_qty,
       rgo_ship_period, rgo_create_order_error_code
FROM regular_order
LEFT JOIN member_info ON rgo_member_id = mbi_mbid AND rgo_shop_id = mbi_shopid
WHERE rgo_status = 1
  AND rgo_next_ship_date < rgo_pause_date
  AND DATEDIFF(rgo_next_ship_date, today) = :diff_day
  -- 失敗通知額外加上：
  AND rgo_create_order_error_code <> ''
```

### 天數常數

| 常數 | 值 | 用途 |
|---|:---:|---|
| `NOTIFY_CASH_ON_DELIVERY_IN_DAYS` | 3 | 貨到付款預告 |
| `NOTIFY_CREDIT_CARD_IN_DAYS` | 3 | 信用卡扣款預告 |
| `NOTIFY_CREDIT_CARD_FAILED_IN_DAYS` | 1 | 信用卡失敗通知 |
| `NOTIFY_CREATE_ORDER_FAILED_IN_DAYS` | 1 | 建單失敗通知 |

---

## 時間軸

```
配送日 = D

D-3  ┌─ 信用卡付款 → CreditCardOrderNotify 入 queue（提醒即將扣款）
     └─ 非信用卡   → CashOnDeliveryOrderNotify 入 queue（提醒準備現金）

D-2  Step ③ 建立 detail（rgo_in_schedule = 1）

D-1  Step ④ 開始打 API（第一次嘗試）
     ┌─ 失敗 + 信用卡錯誤碼 → CreditCardFailedNotify 入 queue
     └─ 失敗 + 其他錯誤碼   → CreateFailedNotify 入 queue

D    Step ④ 最後一次重試機會

D+1  Step ② 偵測過期 → 廢棄這期，推進週期
```

> **D-3 通知**：事前預防，讓消費者有時間確認信用卡額度、收件地址等  
> **D-1 通知**：此時已打過 API 仍失敗，趁最後機會告知消費者

---

## set_queue() 防重複機制

```
load_by_order_and_type(rgo_pk, type)
    │
    ├── 已存在 → 跳過（同一張訂單同類型只塞一次）
    └── 不存在 → INSERT notify_queue
                  nq_type       = 通知類型
                  nq_ref_shop   = rgo_shop_id
                  nq_ref_order  = rgo_pk
                  nq_ref_member = mbi_vip_mbid
                  nq_content    = JSON payload
                  nq_priority   = 5
                  nq_send_limit = 3（最多嘗試發送 3 次）
                  nq_create_user = 'DAEMON'
```

---

## Payload 欄位

| 欄位 | 說明 |
|---|---|
| `VipMemberId` | 會員 ID |
| `Type` | 通知類型 |
| `ShippingDateTime` | 配送日（`rgo_next_ship_date`） |
| `PayDateTime` | 付款時間（信用卡為配送日 -2 天，貨到付款同配送日） |
| `ShippingProfileTypeDef` | 配送方式 |
| `PayProfileTypeDef` | 付款方式 |
| `SalePageTitle` | 商品名稱 |
| `Qty` | 數量 |
| `DeliveryPeriod` | 配送週期（天） |

---

## 涉及資料表

| 資料表 | 操作 | 說明 |
|---|---|---|
| `regular_order` | SELECT | 依 `rgo_next_ship_date` 計算 diff 天數 |
| `member_info` | SELECT | 取 `mbi_vip_mbid` |
| `notify_queue` | SELECT + INSERT | 防重複後寫入通知佇列 |

---

## 注意事項

1. **本排程只負責「建 queue」**，通知實際由 `notify_queue_housekeeper` 發出
2. **防重複依 `(rgo_pk, type)` 判斷**，同一訂單同類型只會塞一次 queue
3. **`CreditCardFailedNotify` 與 `CreateFailedNotify` 互斥**，依錯誤碼分流，不會同時發兩種
4. **D-3 通知不看是否建單成功**，只看 `rgo_status = 1` 且距配送日 3 天，是純預告性質
5. **翻寫注意**：`NOTIFY_*_IN_DAYS` 常數需對應新版設定檔；付款類型判斷 `strpos(pay_type, 'CreditCardOnce')` 需對應新版 enum
