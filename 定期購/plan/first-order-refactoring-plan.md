## 1. 改造目標

將現有 GameBall PHP 首單建立機制，以 .NET Core（Nine1 Provider）重新實作。

**主要訴求：**
- 保留 PHP `/api/NewOrder` 作為入口（Nine1 仍呼叫此 API）
- PHP NewOrder API 改為 Publish NMQ，取代寫入 `order_create_queue` DB Table
- 移除 `create_order_housekeeper` CronJob 對 Nine1 流量的消費
- 以新 NMQ Job `RegularPlanCreateJob` 取代 CronJob 輪詢架構
- 維持現有防重複、錯誤重試等保護機制

---

## 2. 現行架構（GameBall / PHP）

```
mweb 結帳
    │
    ▼ OrderCreated Event (NMQ)
RegularOrderCreatedProcess (scmnmq)
    │  provider == "GameBall"
    ▼ HTTP POST
PHP /api/NewOrder
    │  防重複檢查（查 order_create_queue + regular_order_detail）
    ▼ INSERT
order_create_queue（DB Table）
    │  ocq_status = 0，ocq_occupy_id = ''
    ▼ CronJob 每分鐘輪詢
create_order_housekeeper
    │  1. UPDATE ocq_occupy_id = UUID（原子搶佔，最多 100 筆）
    │  2. 呼叫 V2/SalesOrder/Get
    │  3. 呼叫 V2/RegularOrder/Get
    │  4. INSERT regular_order（主計畫）
    │  5. INSERT regular_order_detail（首期配送）
    └► ocq_status = 1（成功）/ 8（失敗）
```

### 現行架構問題

| 問題 | 說明 |
|------|------|
| 延遲最長 1 分鐘 | CronJob 每分鐘輪詢才能觸發 |
| 多餘的 DB Table | `order_create_queue` 僅為中繼暫存，非業務資料 |
| 複雜的並發鎖機制 | UUID `ocq_occupy_id` 原子搶佔，維護成本高 |
| 跨語言依賴 | .NET Core NMQ Job → PHP HTTP API → DB |

---

## 3. 新版架構（Nine1 / .NET Core）

```
mweb 結帳
    │
    ▼ OrderCreated Event (NMQ)
RegularOrderCreatedProcess (scmnmq) ← 改動：Nine1 分支改為也呼叫 PHP
    │  provider == "Nine1" OR "GameBall" → 都呼叫 PHP /api/NewOrder
    ▼ HTTP POST
PHP /api/NewOrder
    │  防重複檢查（查 regular_order_detail by TG+TS）
    │  ┌─ provider == "Nine1"    → Publish NMQ RegularPlanCreateJob ─┐
    │  └─ provider == "GameBall" → INSERT order_create_queue（不變） ─┘
    ▼ NMQ Message
RegularPlanCreateJob（新 NMQ Job，Nine1 .NET Core 服務）
    │  1. 防重複檢查（查 regular_order_detail by TG+TS）
    │  2. 呼叫 V2/SalesOrder/Get
    │  3. 呼叫 V2/RegularOrder/Get
    │  4. INSERT regular_order（主計畫）
    │  5. INSERT regular_order_detail（首期配送）
    └► 6. INSERT regular_order_mapping
```

> ⚠️ PHP `/api/NewOrder` 需依 `ShopDefault.RegularOrderProvider`（by ShopId）判斷走哪條路徑。

### 新版架構優點

| 面向 | PHP 舊版 | .NET Core 新版 |
|------|----------|----------------|
| 延遲 | 最長 1 分鐘 | NMQ 近即時（秒級） |
| 持久化 | DB Table | NMQ Broker |
| 重試 | CronJob 重跑 | NMQ 自動 Retry |
| 冪等 | UUID 原子鎖 | NMQ IdempotencyKey |
| 並發安全 | `ocq_occupy_id` 搶佔 | NMQ 多 Consumer 天然隔離 |
| 跨語言依賴 | NMQ → PHP → DB Queue → CronJob → DB | NMQ → PHP → NMQ → .NET Core → DB |

---

## 4. 改動項目設計

### 4.1 RegularOrderCreatedProcess（修改）

**檔案：** `nineyi.scm.nmqv2/.../RegularOrderCreatedProcess.cs`

**現行 Nine1 分支（僅 Log，不做事）：**
```csharp
if (regularOrderProvider == "Nine1")
{
    this.Logger.Info($"舊版定期購商品，不須通知");
}
```

**改動後（Nine1 也呼叫 PHP /api/NewOrder，同 GameBall 路徑）：**
```csharp
// 移除 Nine1 的獨立分支，讓 Nine1 走原本 else 的邏輯（呼叫 PHP）
// 只保留 InternalSchedule 的排除判斷
if (salesOrderGroup.SalesOrderGroup_TrackSourceTypeDef == "InternalSchedule")
{
    this.Logger.Info($"系統排程建單版定期購商品，不須通知");
}
else
{
    // Nine1 & GameBall 都走這裡，呼叫 PHP /api/NewOrder
    // PHP 端根據 RegularOrderProvider 自行決定寫 DB queue 或 Publish NMQ
    var url = this._configService.GetAppSetting("RegularOrder.Url");
    // ... 原有 HTTP POST 邏輯不變 ...
}
```

---

### 4.2 PHP `/api/NewOrder`（修改）

**現行邏輯：** 防重複 → `INSERT order_create_queue`

**改動後邏輯：**

```
Step 1｜防重複檢查（不變）
  ├── 查 order_create_queue by TGCode（Nine1 可省略此查詢）
  └── 查 regular_order_detail by TG+TS

Step 2｜判斷 Provider（新增）
  └── 查 ShopDefault.RegularOrderProvider by ShopId
      ├── "Nine1"    → Publish NMQ RegularPlanCreateJob
      └── "GameBall" → INSERT order_create_queue（原邏輯不變）
```

**NMQ Publish 的訊息 Body（同現有 ocq_data）：**
```json
{
  "ShopId": 12345,
  "TGCode": "TG2024052100001",
  "TSCodeList": ["TS2024052100001", "TS2024052100002"],
  "MemberMobile": "0912345678"
}
```

**IdempotencyKey：** `{TGCode}`

---

### 4.3 RegularPlanCreateJob（新增）

**訂閱：** PHP Publish 的 NMQ `RegularPlanCreateJob` 訊息  
**所在服務：** Nine1 .NET Core 定期購服務  

**處理流程：**

```
Step 1｜防重複檢查
  └── 查 regular_order_detail WHERE rod_tg_code = TGCode AND rod_ts_code IN TSCodeList
      已存在 → 記錄 Log，直接結束（不拋例外，避免觸發重試）

Step 2｜取完整訂單資料
  └── 呼叫 V2/SalesOrder/Get（ShopId + TGCode）
      回傳空 → 拋例外（觸發 NMQ Retry）

Step 3｜逐筆 TS 處理
  └── foreach TSCode in TSCodeList:
      a. 確認 TSCode 在回傳清單中
      b. 呼叫 V2/RegularOrder/Get（ShopId + TSCode）
      c. 檢核 OrderFrequency != 0（非一次性訂單）
      d. 取得或建立 member_info 紀錄

Step 4｜建立主計畫（regular_order）
  └── 寫入欄位見第 5 節

Step 5｜建立首期配送（regular_order_detail）
  └── rod_ship_sequence = 1
      rod_create_order_status = 1（首單已建，不需再呼叫 API）
      rod_ship_date = FirstOrderDate + 2天

Step 6｜建立 Mapping
  └── regular_order_mapping（rgo_pk ↔ rod_pk）
```

## 5. 資料寫入規格

### 5.1 regular_order（主計畫）寫入欄位

| 欄位 | 來源 | 說明 |
|------|------|------|
| `rgo_source_id` | V2/RegularOrder/Get → `Id` | 原始定期購設定 ID |
| `rgo_source_tg` | V2/SalesOrder/Get → `TGCode` | 首單 TG Code |
| `rgo_source_ts` | V2/SalesOrder/Get → `TSCode` | 首單 TS Code |
| `rgo_shop_id` | Event `ShopId` | |
| `rgo_member_id` | V2/RegularOrder/Get → `MemberId` | |
| `rgo_page_id` | V2/RegularOrder/Get → `SalePageId` | |
| `rgo_product_name` | V2/RegularOrder/Get → `SaleProductTitle` | |
| `rgo_qty` | V2/RegularOrder/Get → `Qty` | |
| `rgo_original_price` | V2/RegularOrder/Get → `Price` | |
| `rgo_sale_price` | V2/RegularOrder/Get → `Price` | |
| `rgo_pay_type` | V2/RegularOrder/Get → `PayProfileTypeDef` | |
| `rgo_receiver_*` | V2/RegularOrder/Get → `Receiver*` | 收件人資訊（8 欄） |
| `rgo_invoice_*` | V2/RegularOrder/Get → `Invoice*` | 發票資訊（3 欄） |
| `rgo_ship_type` | V2/RegularOrder/Get → `ShippingProfileTypeDef` | |
| `rgo_ship_period` | V2/RegularOrder/Get → `OrderFrequency` | 配送週期（天） |
| `rgo_ship_date` | `FirstOrderDate + 2天` | 首次配送日 |
| `rgo_next_ship_date` | `FirstOrderDate + OrderFrequency + 2天` | 下次配送日 |
| `rgo_ship_count` | 固定 `1` | 目前已配送 1 次（首單） |
| `rgo_create_order_status` | 固定 `1` | 首單已建立 |
| `rgo_pause_date` | 固定 `2099-12-31` | 預設不暫停 |
| `rgo_status` | 固定 `1` | 啟用 |
| `rgo_create_user` | 固定 `'DAEMON'` | |

> 日期計算常數：`GENERATE_NEW_ORDER_DIFF_DAYS = 2`（PHP `Gb_Config::$GENERATE_NEW_ORDER_DIFF_DAYS`）

### 5.2 regular_order_detail（首期配送）寫入欄位

| 欄位 | 值 | 說明 |
|------|---|------|
| `rod_rgo_pk` | `regular_order.rgo_pk` | FK |
| `rod_tg_code` | V2/SalesOrder/Get → `TGCode` | |
| `rod_ts_code` | V2/SalesOrder/Get → `TSCode` | |
| `rod_ship_sequence` | `1` | 第一期 |
| `rod_ship_date` | `FirstOrderDate + 2天` | |
| `rod_create_order_status` | `1` | 首單已建，不需 API 建單 |
| `rod_status` | `1` | 啟用 |
| `rod_create_user` | `'DAEMON'` | |

---

## 6. 防重複機制對照

| 層級 | PHP 舊版（GameBall） | .NET Core 新版（Nine1） |
|------|---------------------|------------------------|
| **NMQ 層** | IdempotencyKey（框架） | IdempotencyKey = `TGCode`（框架） |
| **PHP API 層** | 查 `order_create_queue` by TGCode<br>查 `regular_order_detail` by TG+TS | 查 `regular_order_detail` by TG+TS |
| **Job 層（消費時）** | — | 查 `regular_order_detail` by TG+TS |
| **並發鎖** | `ocq_occupy_id` UUID 原子鎖 | NMQ Consumer 天然隔離 |

---

## 7. 不需要改動的項目

| 項目 | 原因 |
|------|------|
| `order_create_queue` DB Table | PHP 舊商戶（GameBall）仍在使用，不可刪除 |
| `create_order_housekeeper` CronJob | GameBall 舊路徑仍需運作 |
| `RegularOrderCreatedProcess` GameBall 路徑 | 舊商戶繼續走 PHP → DB Queue 路徑 |
| `TrackSourceTypeDef == "InternalSchedule"` 判斷 | 防止每期建單循環觸發，邏輯不變 |

---

## 8. 切換方式

新舊系統可同時運行，透過 `ShopDefault.RegularOrderProvider` 控制流量：

```
"GameBall" → 走 PHP 舊路徑（現行）
"Nine1"    → 走 .NET Core 新路徑（新版）
```

可逐商戶切換，風險可控。

---

## 9. 實作 Checklist

- [ ] **scmnmq** 修改 `RegularOrderCreatedProcess` Nine1 分支：移除 Log-only，改為與 GameBall 同路徑（呼叫 PHP `/api/NewOrder`）
- [ ] **PHP** `/api/NewOrder` 新增 Provider 判斷，查 `ShopDefault.RegularOrderProvider` by ShopId
- [ ] **PHP** Nine1 路徑：Publish NMQ `RegularPlanCreateJob`（IdempotencyKey = TGCode）
- [ ] **PHP** GameBall 路徑：原邏輯（INSERT `order_create_queue`）不動
- [ ] **Nine1 .NET Core** 建立 `RegularPlanCreateJob` NMQ Job Handler
- [ ] 實作防重複邏輯（查 `regular_order_detail` by TG+TS）
- [ ] 實作 `V2/SalesOrder/Get` 呼叫與欄位 mapping
- [ ] 實作 `V2/RegularOrder/Get` 呼叫與欄位 mapping
- [ ] 實作 `regular_order` 寫入（含日期計算：FirstOrderDate + 2天）
- [ ] 實作 `regular_order_detail` 寫入（首期，`rod_create_order_status = 1`）
- [ ] 實作 `regular_order_mapping` 寫入
- [ ] 單元測試：防重複、日期計算、正常流程
- [ ] 整合測試：端對端驗證（mweb 結帳 → DB 計畫建立）
- [ ] 切換 ShopDefault 設定，驗證線上行為
