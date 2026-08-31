# PHP 既有專案說明

# 定期購服務（Regular Purchase Service）— 功能範圍與商業邏輯

逆向工程基準日：2026-04-17  
文件產生日：2026-05-15

---

## 一、這個服務做什麼

**定期購服務**是 91APP 電商平台的**訂閱制購物計畫管理引擎**。

消費者在 91APP 電商平台購買「定期購商品」後，本系統負責：

- 接收首次訂單通知  
- 自動按週期（如每 30/60/90 天）向 91APP 下重複訂單  
- 管理整個訂閱計畫的生命週期（進行中 → 暫停 → 重啟 → 終止）

服務本身**不直接面對消費者下單**，而是作為**後端自動化引擎**，隱藏在 91APP 商城後面運作。

---

## 二、服務邊界（四層職責）

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. 外部 API（/api/webapi.php）                                    │
│    ← 91APP 電商平台呼叫（EDA 事件通知 + 查詢）                       │
│    認證：X-API-Key Header                                          │
├─────────────────────────────────────────────────────────────────┤
│ 2. 消費者前台（/subscriptionorder）                                 │
│    ← 消費者查看自己的訂閱清單、自助取消                                │
│    認證：91APP SSO Cookie + DB Session                             │
├─────────────────────────────────────────────────────────────────┤
│ 3. 管理員後台（/admin）                                             │
│    ← 管理員查詢訂單、暫停/重啟/取消、商品設定、Excel 匯出              │
│    認證：OpenID Connect（91APP OSM）                               │
├─────────────────────────────────────────────────────────────────┤
│ 4. 背景排程（Kubernetes CronJob）                                   │
│    ← 系統自動觸發：建立下期訂單、發送通知、確認付款                    │
└─────────────────────────────────────────────────────────────────┘
```

---

## 三、核心資料模型

```
regular_product（定期購商品設定）
  └─ 由管理員設定：配送週期清單、購買選項、次數上限

order_create_queue（新訂單佇列）
  └─ 91APP EDA 事件打進來的首單，等待排程處理

regular_order（定期購計畫主表）
  ├─ rgo_status：1=進行中 / 2=暫停 / 3=終止
  ├─ rgo_ship_period：配送週期（天數）
  ├─ rgo_next_ship_date：下次配送日
  ├─ rgo_ship_count：已配送期數
  └─ rgo_ship_limit_count：上限次數（0=無限制）

regular_order_detail（每期配送明細）
  ├─ rod_ship_sequence：第幾期
  ├─ rod_ship_date：本期配送日
  ├─ rod_create_order_status：1=建單成功 / 2=失敗
  └─ rod_status：1=正常 / 3=Pending 付款

notify_queue（通知發送佇列）
  └─ 類型：刷卡預告 / 貨到付款 / 刷卡失敗 / 建單失敗
```

### ER Diagram

```
erDiagram
    shop_info {
        int spi_pk PK
        int spi_shopid
        varchar spi_shop_name
        varchar spi_x_api_key
        varchar spi_regular_provider
        tinyint spi_enable_regular
        tinyint spi_cancel_regular
        tinyint spi_suspend_regular
        tinyint spi_status
    }

    regular_product {
        int rpd_pk PK
        int rpd_shop_id FK
        int rpd_page_id
        varchar rpd_title
        json rpd_ship_period
        tinyint rpd_ship_limit_status
        int rpd_ship_limit_count
        tinyint rpd_buy_setting
        tinyint rpd_status
    }

    regular_order {
        int rgo_pk PK
        int rgo_shop_id FK
        int rgo_rpd_pk FK
        varchar rgo_source_tg
        varchar rgo_source_ts
        int rgo_member_id FK
        tinyint rgo_status
        int rgo_ship_count
        int rgo_ship_period
        date rgo_next_ship_date
        int rgo_ship_limit_count
    }

    regular_order_detail {
        int rod_pk PK
        int rod_rgo_pk FK
        int rod_shop_id FK
        varchar rod_tg_code
        varchar rod_ts_code
        int rod_ship_sequence
        date rod_ship_date
        tinyint rod_create_order_status
        tinyint rod_status
    }

    order_create_queue {
        int ocq_pk PK
        varchar ocq_tgid
        varchar ocq_txid
        text ocq_data
        varchar ocq_occupy_id
        tinyint ocq_status
    }

    notify_queue {
        int nq_pk PK
        varchar nq_type
        int nq_ref_shop FK
        varchar nq_ref_member
        int nq_ref_order
        text nq_content
        tinyint nq_send_status
    }

    shop_info ||--o{ regular_product : "spi_shopid = rpd_shop_id"
    shop_info ||--o{ regular_order : "spi_shopid = rgo_shop_id"
    regular_product ||--o{ regular_order : "rpd_pk = rgo_rpd_pk"
    regular_order ||--o{ regular_order_detail : "rgo_pk = rod_rgo_pk"
    shop_info ||--o{ notify_queue : "spi_shopid = nq_ref_shop"
```

---

## 四、核心商業邏輯

### 4-1. 配送日期計算

| 時機 | 計算公式 |
| :---- | :---- |
| **首期**配送日 | `首次下單日 + 2 天` |
| **首期**下次配送日 | `首次下單日 + 配送週期 + 2 天` |
| **續期**下次配送日 | `本期配送日 + 配送週期天數` |
| **暫停**後下次配送日 | `today + 4 天`（`PAUSE_REGULAR_PLUS_DAYS`，固定值） |
| **刷卡時間**（通知用） | `rgo_next_ship_date - 2 天`（提前建單天數） |

### 4-2. 定期購計畫生命週期

```
91APP 首單下單
      │
      ▼
order_create_queue（佇列）
      │ create_order_housekeeper（每分鐘）
      ▼
regular_order 建立（進行中 status=1）
      │
      ├──── 管理員/會員 暫停 ──→ status=2（暫停）
      │                               │
      │                        管理員 重啟 ──→ status=1（進行中）
      │
      ├──── 達到次數上限 ──→ status=3（終止，自動）
      │
      └──── 管理員/會員 取消 ──→ status=3（終止，手動）
```

### 4-3. 建單前置機制（Housekeeper 每日執行）

```
提前 2 天：掃描 rgo_next_ship_date <= today+2 的計畫
           → 生成 regular_order_detail（本期 rod 明細）

提前 2 天：掃描 rod_ship_date <= today+2 的明細
           → 呼叫 91APP TradesOrderV2/SubscriptionOrderAutoGen 建立下一期訂單

建單成功：
  → rod_create_order_status = 1
  → rgo_ship_count += 1
  → rgo_next_ship_date = rgo_ship_date + rgo_ship_period

建單失敗：
  → rod_create_order_status = 2
  → 繼續重試（直到過期）

失敗過期（rod_ship_date < today）：
  → 廢棄該期明細
  → rgo_next_ship_date 順延一個週期
```

### 4-4. 次數上限模式（三種）

| 模式 | rpd\_ship\_limit\_status | 行為 |
| :---- | :---- | :---- |
| 無限制 | 1 | 持續建單，永不自動終止 |
| 最多 N 次 | 2 | `rgo_ship_count >= rgo_ship_limit_count` 時自動終止 |
| 固定 N 次 | 3 | 建立時固定次數，達到即終止 |

**注意：** `rgo_ship_limit_count = 0` 代表無限制，不觸發自動終止邏輯。

### 4-5. Pending 付款狀態分流

```
91APP 建單回傳 OrderSlaveFlowStatus = "WaitingToPay"
  → rod_status = 3（Pending）
  → 不推進 ship_count、不更新 next_ship_date
  → 交由 regular_order_pending_housekeeper 定期查詢

後續查詢 91APP 訂單：
  WaitingToShipping / ConfirmedToShipping / Finish
    → 付款成功 → 正常推進期數，rod_status = 1

  其他狀態
    → 視為刷卡失敗 → error_code = "CreditCardFail"
```

### 4-6. 通知分流規則

```
付款方式（rgo_pay_type）含 "CreditCardOnce"？
  是 → 發「即將刷卡通知」（CreditCardOrderNotify）
  否 → 發「貨到付款通知」（CashOnDeliveryOrderNotify）

建單失敗的 error_code 是 CreditCardBlackList / CreditCardFail？
  是 → 發「刷卡失敗通知」（CreditCardFailedNotify）
  否 → 發「建單失敗通知」（CreateFailedNotify）
```

### 4-7. 合法配送週期白名單

由 `settings.json` 的 `SHIPPING_PERIOD_LIST` 定義，僅允許以下天數：

```
7 / 10 / 14 / 15 / 30 / 45 / 60 / 90 / 180 / 365 / 730
```

---

## 五、User Stories 統計

| 領域 | Story 數 | 說明 |
| :---- | :---- | :---- |
| 前台（STR-F） | 3 | SSO 登入、查看訂閱清單、自助取消 |
| 後台（STR-A） | 19 | 訂單管理、商品設定、店家管理、通知設定 |
| 外部 API（STR-EXT） | 7 | 接受 91APP 電商平台呼叫 |
| 背景排程（STR-JOB） | 9 | 建單、通知、付款確認、庫存預估等 |
| **合計** | **38** | — |

---

## 六、外部 API 合約（對 91APP 電商平台）

所有 API 認證：`X-API-Key` Header  
HTTP Status Code 統一回傳 200，業務錯誤透過 Response Body `Status` 欄位表達

### 標準 Response 格式（5 個 API）

```json
{
  "Status": "Success" | "Failure",
  "TxId": "<全小寫 UUID>",
  "Data": { ... } | null,
  "ErrorMessage": null | "<錯誤說明>"
}
```

### 非標準 Response 格式（GetOriginalOrderSource 系列，⚠️ 格式不同）

```json
{
  "Status": "Success" | "Failure",
  "Data": { ... } | null,
  "Message": "success" | "<錯誤說明>"
}
```

**無 `TxId`，錯誤欄位是 `Message` 而非 `ErrorMessage`**

### API 清單

| URL | 用途 | 類型 |
| :---- | :---- | :---- |
| `POST /api/NewOrder` | 接收首單通知，寫入建單佇列 | **寫入** ⚠️ |
| `POST /api/GetRegularSetting` | 查商品定期購設定（有 Redis Cache，TTL 24h） | 查詢 |
| `POST /api/GetRegularOrderSequence` | 查訂單配送序列（第幾期/主子單對應） | 查詢 |
| `POST /api/GetFirstOrderTotal` | 查當日首期建單總數 | 查詢 |
| `POST /api/GetOrderInfo` | 查定期購計畫狀態（整數：1進行/2暫停/3終止） | 查詢 |
| `POST /api/GetOriginalOrderSource` | 反查原始訂單的購買渠道（APP/WEB 等） | 查詢＋外部呼叫 |
| `POST /api/GetOriginalOrderSourceList` | 批次查詢指定配送日所有訂單的購買渠道 | 查詢＋外部呼叫 |
| `GET /_hc` | 健康檢查，回傳 `"OK"` | — |

---

## 七、背景排程（CronJob）清單

| Housekeeper 檔案 | 功能 | 觸發開關（daemon\_list.pk） |
| :---- | :---- | :---- |
| `create_order_housekeeper.php` | 處理新訂單佇列 → 建立 regular\_order | pk=3 |
| `regular_create_housekeeper.php` | 每日建單主流程（生成明細 → 建單 → 確認） | pk=4 |
| `create_notify_housekeeper.php` | 掃描需通知訂單 → 寫入 notify\_queue | pk=5 |
| `notify_queue_housekeeper.php` | 發送通知（呼叫 91APP Notify API） | pk=6 |
| `regular_check_housekeeper.php` | 確認建單結果 | — |
| `regular_order_pending_housekeeper.php` | 確認 Pending 付款狀態 | — |
| `stock_estimate_housekeeper.php` | 庫存預估通知 | — |
| `sync_translate_housekeeper.php` | 同步多語系翻譯資料 | — |
| `delete_log_file_housekeeper.php` | 清理舊 Log 檔 | — |

---

## 八、多市場支援

服務同時支援三個市場，部署獨立：

| 市場 | 地址驗證方式 | 超商取貨支援 |
| :---- | :---- | :---- |
| TW（台灣） | 縣市鄉鎮三級聯動 | 全家 / 7-11 / 萊爾富 |
| HK（香港） | 縣市鄉鎮三級聯動 | — |
| MY（馬來西亞） | 州 / 城市 / 郵遞區號三級聯動 | 順豐取貨點 |

設定檔：`charts/regular-purchase-service-web-api/values-{TW|HK|MY}-{QA|Prod}.yaml`（6 組）

---

## 九、系統依賴關係

```
定期購服務
  ├── 91APP API Gateway（下行呼叫）
  │     ├── 建單：TradesOrderV2/SubscriptionOrderAutoGen
  │     ├── 查訂單：V2/SalesOrder/Get
  │     ├── 查商品：V2/SalePage/Get + V1/SalePage/GetStock
  │     ├── 查店家：V2/Shop/GetShopInfoList（取得 ShopDefault 開關）
  │     ├── 發通知：V2/RegularOrder/Notify
  │     ├── 庫存預估通知：V2/RegularOrder/EstimatedQtyNotify
  │     ├── 地址聯動：V2/Location/*
  │     └── 更新商品頁狀態：V2/SalePage/UpdateSalePageIsClosedOfRegularOrder
  ├── MySQL（主要資料儲存，schema: n1regular2204）
  ├── Redis（商品設定快取，讀寫分離）
  │     └── Key 格式：{REDIS_PREFIX}_{PRODUCT_CACHE_PREFIX}_{shop_id}_{page_id}
  ├── AWS S3（設定檔版本化儲存）
  └── AWS Secrets Manager（DB 密碼、API Key 等敏感設定）
```

### 91APP ShopDefault 關鍵開關

服務透過 `V2/Shop/GetShopInfoList` 取得 ShopDefault 設定，以下兩個欄位直接影響定期購功能是否啟用：

| ShopDefault Key | 說明 | 合法值 | 對應本地欄位 |
| :---- | :---- | :---- | :---- |
| `RegularOrder-RegularOrderProvider` | 定期購提供商識別碼。**必須為 `Nine1`**，其他值代表該店家由不同廠商提供定期購服務，本系統不處理。 | `Nine1` / 其他廠商代碼 | `shop_info.spi_regular_provider` |
| `SalePage-IsRegularOrderEnable` | 商品頁是否允許定期購。`true` 時該 SalePage 才可被設定為定期購商品；`false` 時定期購功能對該商品頁關閉。 | `true` / `false` | —（由 `V2/SalePage/UpdateSalePageIsClosedOfRegularOrder` 回寫 91APP 端） |

**注意：** 舊版 PHP 程式碼中 `RegularOrderProvider` 寫死為 `"GameBall"`（前身廠商名），翻寫 .NET 時應改為 `"Nine1"`。

---

## 十、技術堆疊

| 面向 | 技術選型 |
| :---- | :---- |
| 語言 / 版本 | PHP 7.4 |
| 伺服器 | Apache 2 \+ mod\_rewrite（無 PHP Framework） |
| 資料庫 | MySQL（PDO，utf8mb4） |
| 快取 | Redis（讀寫分離） |
| Session | MySQL DB-based（自訂 SessionHandlerInterface） |
| 認證（前台） | 91APP SSO Cookie \+ AES-256-CBC Token |
| 認證（後台） | OpenID Connect（91APP OSM） |
| 認證（API） | X-API-Key Header |
| 排程 | Kubernetes CronJob（PHP CLI） |
| 部署 | Docker \+ Kubernetes（Helm），3 市場 × 2 環境 \= 6 部署 |
| 設定管理 | JSON 本地 \+ AWS S3 版本化 \+ AWS Secrets Manager |
| 語系支援 | zh-TW / zh-HK / ms-MY / en-US / ja-JP |

# 資料表

# Data Model

逆向工程基準日：2026-04-17 欄位來源：`models/base/*.php`（INSERT/UPDATE/SELECT SQL 語句） 型別標注「推估」表示從程式碼行為推斷，非資料庫 DDL 確認值

---

## ER Diagram

```
erDiagram
    shop_info {
        int spi_pk PK
        int spi_shopid
        varchar spi_shop_name
        varchar spi_x_api_key
        varchar spi_regular_provider
        tinyint spi_enable_regular
        tinyint spi_cancel_regular
        tinyint spi_suspend_regular
        tinyint spi_status
    }

    member_info {
        int mbi_pk PK
        int mbi_shopid FK
        varchar mbi_mbid
        varchar mbi_vip_mbid
        varchar mbi_acc_token
        datetime mbi_acc_token_expire
        tinyint mbi_status
    }

    regular_product {
        int rpd_pk PK
        int rpd_shop_id FK
        int rpd_page_id
        varchar rpd_title
        json rpd_ship_period
        tinyint rpd_ship_limit_status
        int rpd_ship_limit_count
        tinyint rpd_buy_setting
        tinyint rpd_status
    }

    regular_order {
        int rgo_pk PK
        int rgo_shop_id FK
        int rgo_rpd_pk FK
        varchar rgo_source_tg
        varchar rgo_source_ts
        int rgo_member_id FK
        int rgo_page_id
        tinyint rgo_status
        int rgo_ship_count
        int rgo_ship_period
        date rgo_next_ship_date
    }

    regular_order_detail {
        int rod_pk PK
        int rod_rgo_pk FK
        int rod_shop_id FK
        varchar rod_tg_code
        varchar rod_ts_code
        int rod_ship_sequence
        date rod_ship_date
        tinyint rod_create_order_status
        tinyint rod_status
    }

    order_create_queue {
        int ocq_pk PK
        varchar ocq_tgid
        varchar ocq_txid
        text ocq_data
        varchar ocq_occupy_id
        tinyint ocq_status
    }

    notify_queue {
        int nq_pk PK
        varchar nq_type
        int nq_ref_shop FK
        varchar nq_ref_member
        int nq_ref_order
        text nq_content
        tinyint nq_send_status
    }

    member_login_log {
        int mll_pk PK
        int mll_shopid FK
        int mll_mbi_pk FK
        datetime mll_login_time
    }

    daemon_list {
        int dl_pk PK
        varchar dl_name
        tinyint dl_status
        datetime dl_last_time
    }

    regular_daily_create {
        int rdc_pk PK
        date rdc_date
        int rdc_estimate
        int rdc_execute
        int rdc_success
        int rdc_failed
    }

    update_history_main {
        int uhm_pk PK
        varchar uhm_tb_name
        int uhm_tb_pk
        datetime uhm_update_time
        varchar uhm_update_user
    }

    update_history_detail {
        int uhd_pk PK
        int uhd_uhm_pk FK
        varchar uhd_lang
        varchar uhd_update_item
        text uhd_update_before
        text uhd_update_after
    }

    create_regular_log {
        int crl_pk PK
        int crl_rod_pk FK
        text crl_api_data
        varchar crl_error_code
        tinyint crl_status
        datetime crl_create_time
    }

    regular_order_map {
        int rom_pk PK
        int rom_rgo_pk FK
        int rom_rod_pk FK
    }

    session {
        varchar session_id PK
        datetime session_expires
        text session_data
    }

    shop_info ||--o{ member_info : "shop has members"
    shop_info ||--o{ regular_product : "shop has products"
    shop_info ||--o{ regular_order : "shop has orders"
    member_info ||--o{ regular_order : "member has orders"
    regular_product ||--o{ regular_order : "product in orders"
    regular_order ||--o{ regular_order_detail : "order has details"
    regular_order_detail ||--o{ create_regular_log : "detail has logs"
    update_history_main ||--o{ update_history_detail : "history has details"
    notify_queue }o--|| shop_info : "notify for shop"
    member_info ||--o{ member_login_log : "member login logs"
    regular_order ||--o{ regular_order_map : "order map"
    regular_order_detail ||--o{ regular_order_map : "detail map"
```

---

## 資料表詳細說明

---

## `regular_order`

**說明：** 定期購計畫主表。每筆代表一個會員的一個商品定期購訂閱計畫（一次購買產生一筆）。

| 欄位名稱 | 型別（推估） | 可空 | 預設值 | 說明 |
| :---- | :---- | :---- | :---- | :---- |
| `rgo_pk` | int | NO | AUTO | 主鍵 |
| `rgo_txid` | varchar(36) | YES | NULL | 交易 UUID |
| `rgo_source_id` | int | NO | 0 | 來源 ID（91APP 系統用） |
| `rgo_source_tg` | varchar | YES | NULL | 首期主單號（TGCode） |
| `rgo_source_ts` | varchar | YES | NULL | 首期子單號（TSCode） |
| `rgo_rpd_pk` | int | NO | 0 | 關聯 `regular_product.rpd_pk` |
| `rgo_shop_id` | int | NO | 0 | 店家 ID（關聯 `shop_info.spi_shopid`） |
| `rgo_member_id` | int | NO | 0 | 會員 ID（91APP MemberId） |
| `rgo_page_id` | int | NO | 0 | 商品頁 ID |
| `rgo_product_name` | varchar | YES | NULL | 商品名稱（快照） |
| `rgo_product_sku` | varchar | YES | NULL | 商品 SKU（快照） |
| `rgo_product_style` | varchar | YES | NULL | 商品規格名稱（快照） |
| `rgo_product_style_id` | int/varchar | YES | NULL | 商品規格 ID（快照） |
| `rgo_qty` | int | NO | 0 | 購買數量 |
| `rgo_original_price` | decimal | NO | 0.000 | 原價 |
| `rgo_sale_price` | decimal | NO | 0.000 | 售價 |
| `rgo_pay_type` | varchar | YES | NULL | 付款方式（`CreditCardOnce`/`CreditCardInstallment`/`StorePay`/`ATM`/`CashOnDelivery`/`LinePay`/`JKOPay`/`Aftee`/`ApplePay`/`GooglePay`/`EasyWallet`） |
| `rgo_receiver_name` | blob | YES | NULL | **AES-256 加密**：收件人姓名 |
| `rgo_receiver_mobile` | blob | YES | NULL | **AES-256 加密**：收件人電話 |
| `rgo_receiver_country` | varchar | YES | NULL | 國家名稱 |
| `rgo_receiver_country_code` | varchar | YES | NULL | 國家代碼 |
| `rgo_receiver_state` | varchar | YES | NULL | 州/省（MY 市場） |
| `rgo_receiver_zip_code` | varchar | YES | NULL | 郵遞區號 |
| `rgo_receiver_city` | varchar | YES | NULL | 城市 |
| `rgo_receiver_district` | varchar | YES | NULL | 區 |
| `rgo_receiver_address` | blob | YES | NULL | **AES-256 加密**：詳細地址 |
| `rgo_receiver_shop` | varchar | YES | NULL | 超商門市名稱 |
| `rgo_receiver_shop_code` | varchar | YES | NULL | 超商門市代碼 |
| `rgo_receiver_shop_address` | varchar | YES | NULL | 超商門市地址 |
| `rgo_invoice_title` | varchar | YES | '' | 發票抬頭 |
| `rgo_invoice_no` | varchar | YES | '' | 統一編號 |
| `rgo_invoice_email` | blob | YES | '' | **AES-256 加密**：發票 email |
| `rgo_ship_type` | varchar | YES | NULL | 配送方式（`Home`/`CashOnDelivery`/`FamilyPickup`/`SevenElevenPickup`/`HiLifePickup`/`LocationPickup`） |
| `rgo_ship_limit_status` | tinyint | NO | 1 | 配送次數限制類型（1=無限制 / 2=最多N次 / 3=固定N次） |
| `rgo_ship_limit_count` | int | NO | 0 | 配送次數限制值（0=無限制） |
| `rgo_ship_count` | int | NO | 0 | 已配送次數 |
| `rgo_ship_period` | int | NO | 0 | 配送週期（天數） |
| `rgo_ship_date` | date | YES | NULL | 最近一次配送日 |
| `rgo_next_ship_date` | date | YES | NULL | 下次配送日 |
| `rgo_create_order_date` | date | YES | NULL | 建單日期 |
| `rgo_create_order_status` | tinyint | NO | 0 | 建單結果（0=待建單 / 1=成功 / 2=失敗） |
| `rgo_create_order_error_code` | varchar | YES | NULL | 建單錯誤碼（參見 CREATE\_REGULAR\_ERROR\_MAP） |
| `rgo_first_create_date` | datetime | YES | NULL | 首次建立定期購計畫時間（來自 OrderDateTime，格式 Y-m-d H:i:s） |
| `rgo_is_cancel` | tinyint | NO | 0 | 是否已取消（0=否 / 1=是） |
| `rgo_cancel_reason` | text | YES | NULL | 取消原因 |
| `rgo_cancel_time` | datetime | YES | NULL | 取消時間 |
| `rgo_is_complete` | tinyint | NO | 0 | 是否已完成所有期數（0=否 / 1=是） |
| `rgo_complete_time` | datetime | YES | NULL | 完成時間 |
| `rgo_pause_date` | date | YES | NULL | 暫停後的下次配送日 |
| `rgo_in_schedule` | tinyint | NO | 0 | 是否正在排程中（0=否 / 1=是，防重複建單） |
| `rgo_status` | tinyint | NO | 1 | 計畫狀態（1=進行中 / 2=暫停 / 3=終止） |
| `rgo_create_user` | varchar | YES | NULL | 建立者 |
| `rgo_create_time` | datetime | YES | NULL | 建立時間 |
| `rgo_update_user` | varchar | YES | NULL | 最後更新者 |
| `rgo_update_time` | datetime | YES | NULL | 最後更新時間 |

**索引（推估）：**

- PRIMARY KEY (`rgo_pk`)  
- INDEX (`rgo_shop_id`)  
- INDEX (`rgo_member_id`)  
- UNIQUE / INDEX (`rgo_source_tg`, `rgo_source_ts`)

**關聯：**

- `rgo_shop_id` → `shop_info.spi_shopid`  
- `rgo_rpd_pk` → `regular_product.rpd_pk`

---

## `regular_order_detail`

**說明：** 定期購配送明細表。每筆代表定期購計畫的一個配送期次（第 N 期）。

| 欄位名稱 | 型別（推估） | 可空 | 預設值 | 說明 |
| :---- | :---- | :---- | :---- | :---- |
| `rod_pk` | int | NO | AUTO | 主鍵 |
| `rod_txid` | varchar(36) | YES | NULL | 期次交易 UUID |
| `rod_rgo_pk` | int | NO | 0 | 關聯 `regular_order.rgo_pk` |
| `rod_shop_id` | int | NO | 0 | 店家 ID |
| `rod_member_id` | int | NO | 0 | 會員 ID |
| `rod_tg_id` | int | NO | 0 | TG 數字 ID |
| `rod_tg_code` | varchar | YES | NULL | 主單號（建單後填入） |
| `rod_ts_code` | varchar | YES | NULL | 子單號（建單後填入） |
| `rod_page_id` | int | NO | 0 | 商品頁 ID |
| `rod_qty` | int | NO | 0 | 數量 |
| `rod_original_price` | decimal | NO | 0.000 | 原價 |
| `rod_sale_price` | decimal | NO | 0.000 | 售價 |
| `rod_pay_type` | varchar | YES | NULL | 付款方式 |
| `rod_receiver_name` | blob | YES | NULL | **AES-256 加密**：收件人姓名 |
| `rod_receiver_mobile` | blob | YES | NULL | **AES-256 加密**：收件人電話 |
| `rod_receiver_country` | varchar | YES | NULL | 國家名稱 |
| `rod_receiver_country_code` | varchar | YES | NULL | 國家代碼 |
| `rod_receiver_state` | varchar | YES | NULL | 州（MY 市場） |
| `rod_receiver_zip_code` | varchar | YES | NULL | 郵遞區號 |
| `rod_receiver_city` | varchar | YES | NULL | 城市 |
| `rod_receiver_district` | varchar | YES | NULL | 區 |
| `rod_receiver_address` | blob | YES | NULL | **AES-256 加密**：詳細地址 |
| `rod_receiver_shop` | varchar | YES | NULL | 超商門市名稱 |
| `rod_receiver_shop_code` | varchar | YES | NULL | 超商門市代碼 |
| `rod_receiver_shop_address` | varchar | YES | NULL | 超商門市地址 |
| `rod_invoice_title` | varchar | YES | '' | 發票抬頭 |
| `rod_invoice_no` | varchar | YES | '' | 統一編號 |
| `rod_invoice_email` | blob | YES | '' | **AES-256 加密**：發票 email |
| `rod_ship_type` | varchar | YES | NULL | 配送方式 |
| `rod_ship_sequence` | int | NO | 0 | 配送序號（第幾期） |
| `rod_ship_date` | date | YES | NULL | 本期配送日 |
| `rod_create_order_date` | date | YES | NULL | 建單日期 |
| `rod_create_order_status` | tinyint | NO | 0 | 建單狀態（0=待建單 / 1=成功 / 2=失敗） |
| `rod_create_order_error_code` | varchar | YES | NULL | 建單錯誤碼 |
| `rod_daemon_active_time` | datetime | YES | NULL | Daemon 最後處理時間 |
| `rod_status` | tinyint | NO | 1 | 記錄狀態（1=有效） |
| `rod_create_user` | varchar | YES | NULL | 建立者 |
| `rod_create_time` | datetime | YES | NULL | 建立時間 |
| `rod_update_user` | varchar | YES | NULL | 最後更新者 |
| `rod_update_time` | datetime | YES | NULL | 最後更新時間 |

**關聯：**

- `rod_rgo_pk` → `regular_order.rgo_pk`

---

## `regular_product`

**說明：** 定期購商品設定表。紀錄每個店家為哪些商品頁開啟定期購及其配送規則。

| 欄位名稱 | 型別（推估） | 可空 | 預設值 | 說明 |
| :---- | :---- | :---- | :---- | :---- |
| `rpd_pk` | int | NO | AUTO | 主鍵 |
| `rpd_uuid` | varchar(36) | YES | NULL | UUID |
| `rpd_page_id` | int | NO | — | 商品頁 ID（91APP SalePage ID） |
| `rpd_shop_id` | int | NO | — | 店家 ID |
| `rpd_category_id` | int | YES | NULL | 商品分類 ID |
| `rpd_shop_category_id` | int | YES | NULL | 店家分類 ID |
| `rpd_title` | varchar | YES | NULL | 商品名稱（快照） |
| `rpd_sku` | varchar | YES | NULL | SKU |
| `rpd_pic_url` | varchar | YES | NULL | 商品圖片 URL |
| `rpd_original_price` | decimal | YES | NULL | 原價 |
| `rpd_sale_price` | decimal | YES | NULL | 售價 |
| `rpd_cost_price` | decimal | YES | NULL | 成本價 |
| `rpd_sale_qty` | int | YES | NULL | 銷售量（`RegQty - CancelQty`） |
| `rpd_limit_qty` | int | YES | NULL | 銷售上限 |
| `rpd_ship_period` | text/json | YES | NULL | 配送週期陣列（JSON，如 `[7,14,30]`） |
| `rpd_ship_limit_status` | tinyint | NO | — | 配送次數限制類型（1=無限制 / 2=最多N次 / 3=固定N次） |
| `rpd_ship_limit_count` | int | YES | NULL | 配送次數限制值（2\~99） |
| `rpd_ship_method` | varchar | YES | NULL | 配送方式設定 |
| `rpd_pay_method` | varchar | YES | NULL | 付款方式設定 |
| `rpd_buy_setting` | tinyint | NO | — | 購買選項（1=僅定期購 / 2=定期購+單次） |
| `rpd_sale_date_begin` | datetime | YES | NULL | 商品開賣時間（快照） |
| `rpd_sale_date_end` | datetime | YES | NULL | 商品截止時間（快照） |
| `rpd_status` | tinyint | NO | — | 狀態（1=啟用 / 9=刪除） |
| `rpd_create_time` | datetime | YES | NULL | 建立時間 |
| `rpd_create_user` | varchar | YES | NULL | 建立者 |
| `rpd_update_time` | datetime | YES | NULL | 最後更新時間 |
| `rpd_update_user` | varchar | YES | NULL | 最後更新者 |

**索引（推估）：**

- PRIMARY KEY (`rpd_pk`)  
- UNIQUE (`rpd_shop_id`, `rpd_page_id`)

**Redis 快取：**

- Key：`n1regular_GetRegularSetting_{shop_id}_{page_id}`，TTL 86400 秒

---

## `shop_info`

**說明：** 店家資訊表。管理使用定期購服務的店家基本設定。

| 欄位名稱 | 型別（推估） | 可空 | 預設值 | 說明 |
| :---- | :---- | :---- | :---- | :---- |
| `spi_pk` | int | NO | AUTO | 主鍵 |
| `spi_shopid` | int | NO | — | 91APP 店家 ID |
| `spi_shop_name` | varchar | YES | NULL | 店家名稱 |
| `spi_x_api_key` | varchar | YES | NULL | 本系統 API Key |
| `spi_supplierid` | varchar | YES | NULL | 供應商 ID |
| `spi_merchant_code` | varchar | YES | NULL | 商家代碼 |
| `spi_vendors_key` | varchar | YES | NULL | 廠商 Key |
| `spi_options` | text/json | YES | NULL | 店家選項（key-value JSON，含 `cancel_regular` 等） |
| `spi_lang_def` | varchar | YES | NULL | 預設語系（`zh-TW`/`zh-HK`/`ms-MY`/`en-US`/`ja-JP`） |
| `spi_lang_availabe` | varchar | YES | NULL | 可用語系 |
| `spi_sales_currency` | varchar | YES | NULL | 銷售幣別 |
| `spi_symbol` | varchar | YES | NULL | 貨幣符號 |
| `spi_digits` | int | YES | NULL | 小數位數 |
| `spi_separator` | varchar | YES | NULL | 小數分隔符號 |
| `spi_group_separator` | varchar | YES | NULL | 千分位分隔符號 |
| `spi_enable_ecoupon` | tinyint | NO | 0 | 是否啟用電子票券 |
| `spi_enable_regular` | tinyint | NO | 0 | 是否啟用定期購（1=啟用） |
| `spi_regular_provider` | varchar | YES | NULL | 定期購供應商（需為 `GameBall`） |
| `spi_cancel_regular` | tinyint | NO | 1 | 允許會員自助取消（0=關閉 / 1=開啟） |
| `spi_suspend_regular` | tinyint | NO | — | 允許暫停定期購（0=關閉 / 1=開啟） |
| `spi_status` | tinyint | NO | — | 啟用狀態 |
| `spi_create_time` | datetime | YES | NULL | 建立時間 |
| `spi_update_time` | datetime | YES | NULL | 最後更新時間 |

---

## `member_info`

**說明：** 前台會員表。儲存透過 SSO 登入過定期購前台的會員資料。

| 欄位名稱 | 型別（推估） | 可空 | 預設值 | 說明 |
| :---- | :---- | :---- | :---- | :---- |
| `mbi_pk` | int | NO | AUTO | 主鍵 |
| `mbi_uid` | varchar(36) | YES | NULL | UUID |
| `mbi_shopid` | int | NO | — | 店家 ID |
| `mbi_mbid` | varchar | YES | NULL | 91APP MemberId |
| `mbi_vip_mbid` | varchar | YES | NULL | 91APP VipMemberId（主要識別鍵） |
| `mbi_mbcode` | varchar | YES | NULL | 會員代碼 |
| `mbi_outer_mbcode` | varchar | YES | NULL | 外部會員代碼 |
| `mbi_country_code` | varchar | YES | NULL | 國碼 |
| `mbi_mobile` | varchar | YES | NULL | 手機號碼 |
| `mbi_email` | varchar | YES | NULL | Email |
| `mbi_nickname` | varchar | YES | NULL | 暱稱 |
| `mbi_acc_token` | varchar(36) | YES | NULL | Access Token（UUID，Session 驗證用） |
| `mbi_acc_token_expire` | datetime | YES | NULL | Token 到期時間（+2 天） |
| `mbi_status` | tinyint | NO | — | 會員狀態（1=有效） |
| `mbi_notify` | tinyint | YES | NULL | 通知設定 |
| `mbi_login_count` | int | YES | NULL | 登入次數 |
| `mbi_active_time` | datetime | YES | NULL | 最後活動時間 |
| `mbi_create_time` | datetime | YES | NULL | 建立時間 |
| `mbi_create_ip` | varchar | YES | NULL | 建立 IP |
| `mbi_last_login_time` | datetime | YES | NULL | 最後登入時間 |
| `mbi_last_update_time` | datetime | YES | NULL | 最後更新時間 |
| `mbi_last_buy_time` | datetime | YES | NULL | 最後購買時間 |
| `mbi_last_verify_time` | datetime | YES | NULL | 最後驗證時間 |

**索引（推估）：**

- PRIMARY KEY (`mbi_pk`)  
- UNIQUE (`mbi_shopid`, `mbi_vip_mbid`)  
- INDEX (`mbi_acc_token`)

---

## `member_login_log`

**說明：** 會員登入記錄。每次 SSO 登入成功後寫入一筆。

| 欄位名稱 | 型別（推估） | 可空 | 說明 |
| :---- | :---- | :---- | :---- |
| `mll_pk` | int | NO | 主鍵（AUTO） |
| `mll_shopid` | int | NO | 店家 ID |
| `mll_mbi_pk` | int | NO | 關聯 `member_info.mbi_pk` |
| `mll_sender_os` | varchar | YES | 作業系統（User-Agent 解析） |
| `mll_sender_ua` | varchar(500) | YES | User-Agent（最多 500 字） |
| `mll_login_time` | datetime | NO | 登入時間 |
| `mll_login_ip` | varchar | YES | 登入 IP |

---

## `order_create_queue`

**說明：** 新訂單建立佇列。EDA 收到 NewOrder 事件後寫入，由 Create\_Order\_Housekeeper 消費。

| 欄位名稱 | 型別（推估） | 可空 | 預設值 | 說明 |
| :---- | :---- | :---- | :---- | :---- |
| `ocq_pk` | int | NO | AUTO | 主鍵 |
| `ocq_tgid` | varchar | NO | — | TGCode（主單號，唯一） |
| `ocq_txid` | varchar(36) | YES | NULL | 系統交易 UUID |
| `ocq_data` | text | YES | NULL | 完整的 NewOrder 請求 JSON |
| `ocq_occupy_id` | varchar(36) | YES | '' | 佔用的 Daemon processId（'' \= 未佔用） |
| `ocq_status` | tinyint | NO | 0 | 狀態（0=待處理 / 1=已完成） |
| `ocq_create_time` | datetime | YES | NULL | 建立時間 |

---

## `notify_queue`

**說明：** 通知發送佇列。Create\_Notify\_Housekeeper 寫入，Notify\_Queue\_Housekeeper 消費。

| 欄位名稱 | 型別（推估） | 可空 | 預設值 | 說明 |
| :---- | :---- | :---- | :---- | :---- |
| `nq_pk` | int | NO | AUTO | 主鍵 |
| `nq_type` | varchar | NO | — | 通知類型（`CashOnDeliveryOrderNotify`/`CreditCardOrderNotify`/`CreditCardFailedNotify`/`CreateFailedNotify`/`StockNotify`） |
| `nq_ref_shop` | int | NO | 0 | 關聯店家 ID |
| `nq_ref_member` | varchar | NO | 0 | 91APP VIP 會員 ID（`mbi_vip_mbid`，字串識別碼；庫存通知固定為 `0`） |
| `nq_ref_order` | int | NO | 0 | 關聯訂單 PK |
| `nq_content` | text | YES | NULL | 通知內容（JSON payload） |
| `nq_priority` | int | NO | 5 | 優先順序 |
| `nq_send_status` | tinyint | NO | 0 | 發送狀態（0=待發送 / 1=成功 / 2=失敗） |
| `nq_send_limit` | int | NO | 3 | 最大發送次數 |
| `nq_sended_count` | int | NO | 0 | 已發送次數 |
| `nq_create_user` | varchar | YES | NULL | 建立者 |
| `nq_create_time` | datetime | YES | NULL | 建立時間 |
| `nq_send_time` | datetime | YES | NULL | 最後發送時間 |

#### notify\_queue\_bak（封存表）

結構與 `notify_queue` 相同，由 `notify_queue_housekeeper` 在發送成功後將處理完成的記錄從 `notify_queue` 搬移至此表封存。（`models/notify_queue.php:120`）

---

## `daemon_list`

**說明：** 排程開關控制表。控制各 Housekeeper 是否允許執行。

| 欄位名稱 | 型別（推估） | 可空 | 預設值 | 說明 |
| :---- | :---- | :---- | :---- | :---- |
| `dl_pk` | int | NO | AUTO | 主鍵（對應各 Housekeeper 的 pg\_id） |
| `dl_name` | varchar | YES | NULL | 排程名稱 |
| `dl_status` | tinyint | NO | 0 | 啟用狀態（0=停用 / 1=啟用） |
| `dl_last_time` | datetime | YES | NULL | 最後執行時間 |
| `dl_last_user` | varchar | YES | NULL | 最後執行的主機 `$_SERVER['SERVER_NAME']` |

**特殊規則：** pk=1 為全域開關，必須與個別排程的開關同時為 1 才可執行。

---

## `regular_daily_create`

**說明：** 每日建單統計表。記錄每日定期購建單的預估與實際執行數量，用於監控告警。

| 欄位名稱 | 型別（推估） | 可空 | 預設值 | 說明 |
| :---- | :---- | :---- | :---- | :---- |
| `rdc_pk` | int | NO | AUTO | 主鍵 |
| `rdc_date` | date | NO | — | 日期（一天一筆） |
| `rdc_estimate` | int | NO | 0 | 預估建單數 |
| `rdc_execute` | int | NO | 0 | 已執行建單數 |
| `rdc_success` | int | NO | 0 | 成功建單數 |
| `rdc_failed` | int | NO | 0 | 失敗建單數 |

---

## `update_history_main`

**說明：** 修改歷程主表。記錄每次對特定資料表記錄的修改事件。

| 欄位名稱 | 型別（推估） | 可空 | 預設值 | 說明 |
| :---- | :---- | :---- | :---- | :---- |
| `uhm_pk` | int | NO | AUTO | 主鍵 |
| `uhm_tb_name` | varchar | NO | '' | 被修改的資料表名稱（如 `regular_order`/`regular_product`/`shop_info`） |
| `uhm_tb_pk` | int | NO | 0 | 被修改記錄的 PK |
| `uhm_update_time` | datetime | YES | NULL | 修改時間 |
| `uhm_update_user` | varchar | YES | NULL | 修改者（管理員帳號或 `'User'`） |
| `uhm_status` | tinyint | NO | 0 | 狀態 |

---

## `update_history_detail`

**說明：** 修改歷程明細表。每個修改事件可有多筆欄位變更紀錄。

| 欄位名稱 | 型別（推估） | 可空 | 預設值 | 說明 |
| :---- | :---- | :---- | :---- | :---- |
| `uhd_pk` | int | NO | AUTO | 主鍵 |
| `uhd_uhm_pk` | int | NO | 0 | 關聯 `update_history_main.uhm_pk` |
| `uhd_lang` | varchar | YES | '' | 語系 |
| `uhd_update_item` | varchar | NO | '' | 修改的欄位名稱 |
| `uhd_update_before` | text | YES | '' | 修改前的值 |
| `uhd_update_after` | text | YES | '' | 修改後的值 |

---

## `create_regular_log`

**說明：** 建單 API 呼叫日誌。記錄每次呼叫 91APP 建單 API 的請求與結果。

| 欄位名稱 | 型別（推估） | 可空 | 預設值 | 說明 |
| :---- | :---- | :---- | :---- | :---- |
| `crl_pk` | int | NO | AUTO | 主鍵 |
| `crl_rod_pk` | int | NO | 0 | 關聯 `regular_order_detail.rod_pk` |
| `crl_api_data` | text | YES | NULL | API 請求/回應資料（JSON） |
| `crl_error_code` | varchar | YES | NULL | 錯誤碼 |
| `crl_status` | tinyint | NO | 0 | 結果狀態（0=失敗 / 1=成功） |
| `crl_create_time` | datetime | YES | NULL | 建立時間 |

---

## `regular_order_map`

**說明：** 定期購訂單對應表。紀錄主計畫（`regular_order`）與期次明細（`regular_order_detail`）的對應關係。

| 欄位名稱 | 型別（推估） | 可空 | 預設值 | 說明 |
| :---- | :---- | :---- | :---- | :---- |
| `rom_pk` | int | NO | AUTO | 主鍵 |
| `rom_rgo_pk` | int | NO | 0 | 關聯 `regular_order.rgo_pk` |
| `rom_rod_pk` | int | NO | 0 | 關聯 `regular_order_detail.rod_pk` |

---

## `session`

**說明：** DB-based Session 儲存表。同時用於前台（`REGULAR_FRSESSID`）與後台（`REGULAR_BKSESSID`）的 Session 資料。

| 欄位名稱 | 型別（推估） | 可空 | 說明 |
| :---- | :---- | :---- | :---- |
| `session_id` | varchar | NO | Session ID（主鍵） |
| `session_expires` | datetime | NO | 過期時間（依 TTL 計算） |
| `session_data` | text | YES | PHP serialize 的 Session 資料 |

**寫入方式：** `REPLACE INTO session`（存在時更新，不存在時插入）

---

## 加密說明

以下欄位使用 MySQL `AES_ENCRYPT()` / `AES_DECRYPT()` 函數，Key 為 `Gb_Config::$DB_SALT`：

| 資料表 | 加密欄位 |
| :---- | :---- |
| `regular_order` | `rgo_receiver_name`、`rgo_receiver_mobile`、`rgo_receiver_address`、`rgo_invoice_email` |
| `regular_order_detail` | `rod_receiver_name`、`rod_receiver_mobile`、`rod_receiver_address`、`rod_invoice_email` |

加密後儲存為 `blob` 型別，讀取時自動解密。  
