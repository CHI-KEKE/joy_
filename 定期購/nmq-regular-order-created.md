# RegularOrderCreatedProcess — NMQ Job 鏈路分析

> 分析日期：2026-05-21  
> 涉及專案：`nineyi.scm.nmqv2`（SCM NMQ）、`n1-regularpurchase-nkp`（PHP 定期購服務）

---

## 一、在整個流程中的位置（Big Picture）

```
用戶在 mweb 前台結帳（含定期購商品）
        ↓
[mweb] 付款成功
        ↓
[mweb] CreateRegularOrderProcessor
  → 寫入 ERP DB 的 RegularOrder 表
  → RegularOrder_Provider = "GameBall" / "Nine1"
        ↓
[ERP/支付服務] 訂單成立後
  → PublishEventService.FireOrderCreatedEvent(tgId)
  → 建立 NMQ Task: "CreateEvent" (EventName: "OrderCreated")
        ↓
[nineyi.scm.nmqv2] ← ★ 本文件分析範圍
  RegularOrderCreatedProcess.DoJob()
        ↓（僅 GameBall provider + 非系統排程建單）
[n1-regularpurchase-nkp] POST /api/NewOrder
  → 防重複 → 寫入 order_create_queue
        ↓
create-order-housekeeper（每 1 分鐘）
  → 建立定期購計畫（regular_order / regular_order_detail）
```

---

## 二、觸發條件

| 項目 | 說明 |
|------|------|
| 觸發方式 | NMQ `OrderCreated` 事件被 dispatch |
| 事件來源 | `PublishEventService.FireOrderCreatedEvent(tgId)` |
| EventName | `OrderCreated` |
| SourceType | `Orders` |
| IdempotencyKey | `TG-{TGCode}`（防重複）|
| 前置條件 | 訂單完成付款、TG 訂單已建立在 WebStoreDB |

> **⚠️ 注意：** `PublishEventService` 目前在 `nineyi.scm.nmqv2` 中已定義但在 `JKOPaymentService`、`ICashPayPaymentService` 僅注入未呼叫 `FireOrderCreatedEvent`。實際呼叫方可能在未本地 clone 的其他 repo（如 ERP 支付處理服務）。

---

## 三、Request / Response 格式

### NMQ Task Data（傳入）

```json
{
  "SourceType": "Orders",
  "EventName": "OrderCreated",
  "IdempotencyKey": "TG-TG202501010001",
  "Version": "1.0.0",
  "EventArgs": {
    "ShopId": 1204
  },
  "Source": {
    "ShopId": 1204,
    "OrderCode": "TG202501010001",
    "TotalAmount": 1200.00,
    "CurrencyCode": "TWD",
    "OrderDateTime": "2025-01-01T10:00:00",
    "PayType": "CreditCard",
    "MemberId": 12345,
    "CountryCode": "TW",
    "CellPhone": "0912345678"
  }
}
```

### POST /api/NewOrder（呼叫 PHP 服務）

**Request Header：**
```
x-api-key: {RegularOrder.Api.Key}
Content-Type: application/json
```

**Request Body（`RegularOrderCreatedEntity`）：**

| 欄位 | 型別 | 說明 |
|------|------|------|
| `ShopId` | long | 商店序號 |
| `TGCode` | string | 主單編號（如 TG202501010001）|
| `TSCodeList` | List\<string\> | 定期購子單編號清單（ShippingTypeDef=7 的 TS）|
| `MemberMobile` | string | 會員手機號碼（從 VipMember 取得）|

**Response（`RegularOrderApiResultEntity<string>`）：**

| 欄位 | 說明 |
|------|------|
| `Status` | `"Success"` or 其他 |

---

## 四、核心流程（逐步）

```
DoJob(taskInfo)
  │
  ├─ Step 1：解析 NMQ Task Data
  │   taskInfo.Data → FireEventExtRequest
  │   fireEventRequest.Source → OrderCreatedSourceEntity
  │   取得：ShopId、OrderCode（TGCode）
  │
  ├─ Step 2：查詢訂單資料（WebStoreDB）
  │   salesOrderService.GetGroup(OrderCode)        → SalesOrderGroup（含 TrackSourceTypeDef）
  │   salesOrderService.GetMasterList(groupId)     → SalesOrder 清單
  │   salesOrderService.GetSalesOrderSlaveList(ids)→ SalesOrderSlave 清單
  │   .Where(ShippingTypeDef == 7)                 → 篩選定期購子單
  │
  ├─ Step 3：判斷是否需要通知（分支決策 → 見下節）
  │
  └─ Step 4：（若需通知）呼叫 PHP /api/NewOrder
      取得 RegularOrder.Url + RegularOrder.Api.Key from AppSettings
      取得 MemberMobile（vipMemberService.GetVipMemberInfoItem）
      POST {url}/api/NewOrder with x-api-key header
      檢查 response.Status == "Success"
```

---

## 五、分支決策表

| 條件 | Log 訊息 | 行為 |
|------|----------|------|
| 沒有 ShippingTypeDef=7 的 TS | 「無定期購商品，不須通知」 | 直接結束，不呼叫 API |
| 有定期購 TS，且 `RegularOrderProvider == "Nine1"` | 「舊版定期購商品，不須通知」 | 跳過，不呼叫 API |
| 有定期購 TS，且 `TrackSourceTypeDef == "InternalSchedule"` | 「系統排程建單版定期購商品，不須通知」 | 跳過，不呼叫 API（避免循環觸發）|
| 有定期購 TS，且以上條件都不符合（GameBall 首單）| — | **呼叫 `/api/NewOrder`** |

> **關鍵說明：**
> - `Nine1` provider：使用新版 .NET Core 定期購系統，不走 PHP 服務
> - `InternalSchedule`：`regular-create-housekeeper` 建的續購單，若再通知會無限循環
> - `GameBall` provider（預設）：現行 PHP 定期購服務，需要通知

---

## 六、例外處理一覽

| 例外類型 | 發生時機 | 行為 |
|----------|----------|------|
| `ApplicationException`（解析失敗）| taskInfo.Data 無法轉型 | throw，NMQ 進 retry |
| `ApplicationException`（Source 解析失敗）| fireEventRequest.Source 格式錯誤 | throw，NMQ 進 retry |
| `ApplicationException`（通知失敗）| response.Status != "Success" | throw，NMQ 進 retry |
| `ApplicationException`（HTTP 錯誤）| response.StatusCode != 200 | throw，NMQ 進 retry |
| `Exception`（其他）| 任何未預期例外 | Logger.Error → rethrow |

> 所有例外都走 `throw`，由 NMQ 框架決定是否 retry。`OnError()` 目前為空方法。

---

## 七、關鍵資料說明

### `SaleProductSlave_ShippingTypeDef == 7` — 定期購識別碼

這是判斷「哪些 TS 是定期購訂單」的核心欄位：

```
SalesOrderGroup（TG）
  └─ SalesOrder（TS 主單，複數）
       └─ SalesOrderSlave（TS 子單）
            └─ SalesOrderSlave_ShippingTypeDef == 7 → 定期購商品
```

### `SalesOrderGroup_TrackSourceTypeDef` — 來源類型

| 值 | 意義 |
|----|------|
| `InternalSchedule` | 由 `regular-create-housekeeper` 建的定期續購單 |
| （其他）| 由前台用戶結帳建立的首單 |

---

## 八、特殊說明 / 與翻寫的關係

### 翻寫 .NET Core 定期購服務的影響

| 面向 | 現況 | 翻寫後（.NET Core）|
|------|------|-------------------|
| `RegularOrderProvider` | `"GameBall"` → 走此 NMQ 呼叫 PHP | `"Nine1"` → **此 NMQ Job 直接跳過** |
| 訂單建立觸發方式 | NMQ → HTTP `/api/NewOrder` → DB Queue | .NET Core 服務自行處理（不走此 NMQ）|
| 此 NMQ Job 後續命運 | 繼續服務 GameBall 商店 | 等所有商店遷移完成後可退場 |

> **重要結論：** 翻寫後，新的 .NET Core 定期購服務**不需要**透過這支 NMQ Job 觸發。  
> 只要商店的 `RegularOrderProvider` 設為 `"Nine1"`，此 NMQ Job 就會自動跳過。  
> 這意味著新服務需要自己的觸發機制（可能是從 mweb 結帳時直接呼叫 API，或另一個 NMQ job）。

---

## 九、關鍵檔案索引

| 角色 | 路徑 |
|------|------|
| **NMQ Job 主程式** | `C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\NMQV2\RegularOrders\RegularOrderCreatedProcess.cs` |
| **DI 設定** | `C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\NMQV2\RegularOrders\RegularOrderCreatedProcessSetting.cs` |
| **Request Entity** | `C:\91APP\NMQ\nineyi.scm.nmqv2\ERP\Backend\BE\RegularOrder\RegularOrderCreatedEntity.cs` |
| **OrderCreated Source Entity** | `C:\91APP\NMQ\nineyi.scm.nmqv2\WebStore\Backend\BE\Events\OrderCreatedSourceEntity.cs` |
| **事件發送服務** | `C:\91APP\NMQ\nineyi.scm.nmqv2\WebStore\Backend\BLV2\Events\PublishEventService.cs` |
| **事件服務介面** | `C:\91APP\NMQ\nineyi.scm.nmqv2\WebStore\Backend\BLV2\Events\IPublishEventService.cs` |
| **PHP 接收端（被呼叫）** | `C:\91APP\regular\n1-regularpurchase-nkp\src\...\api\webapi.php`（`new_order()` 函式）|

---

## 缺少的元素（待補充）

- [ ] **`FireOrderCreatedEvent` 的實際呼叫方**：在 `nineyi.scm.nmqv2` 中已定義、在 JKO/ICashPay 中有注入但未呼叫。實際觸發點可能在本地未 clone 的 repo（如 ERP 支付服務）
- [ ] **NMQ Job 定義（DB 設定）**：`nmq_job_setting` 表中此 Job 的 `job_code`、concurrency 設定等
- [ ] **新版 .NET Core 服務的觸發機制設計**：Nine1 provider 跳過此 NMQ，新服務用什麼觸發？
