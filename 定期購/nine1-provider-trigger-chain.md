# Nine1 Provider 完整觸發路徑分析

> 分析日期：2025-05-21  
> 涉及專案：`nineyi.scm.nmqv2`（SCM NMQ）、`nineyi.webstore.mobilewebmall`（mweb）

---

## 一、Big Picture：兩套定期購並存

| 面向 | Nine1（91APP 自家舊版）| GameBall（玩子 PHP 服務）|
|------|----------------------|------------------------|
| Provider 值 | `Nine1 = 0` | `GameBall = 1` |
| 訂單資料儲存 | ERP `RegularOrder` 表 | PHP 服務自己的 `regular_order` / `regular_order_detail` |
| 後續建單 API | mweb `RegularOrderAutoGen` | mweb `SubscriptionOrderAutoGen` |
| 建單觸發方式 | NMQ Job `RegularOrderCreateOrderProcess` | PHP cron `regular_create_housekeeper` |
| CheckoutInfo | 不儲存（null/empty）| 儲存信用卡 token（供後續扣款）|
| 管理頁面 | mweb 內建定期購管理頁 | 導向玩子 PHP 服務頁面 |

---

## 二、Nine1 完整觸發路徑

```
用戶在 mweb 前台結帳（含 Nine1 定期購商品）
        ↓
[mweb] CreateRegularOrderProcessor
  → 寫入 ERP DB RegularOrder 表
  → RegularOrder_Provider = "Nine1"
  → RegularOrder_CheckoutInfo = null（不需要信用卡 token）
  → 設定 ExpectShippingDate / NextExpectShippingDate
        ↓
[NMQ] RegularOrderCreatedProcess (OrderCreated 事件)
  → Provider == "Nine1"
  → ⚡ 直接跳過，Log: 「舊版定期購商品，不須通知」
  → 不呼叫 PHP 服務
        ↓
  ====（每日排程觸發）====
        ↓
[???] 建立 NMQ Task: "RegularOrderCreateOrder"
  → Data: { StartDate, EndDate }
  ⚠️ 觸發來源：在本地 repo 中未找到，推測為：
     - Kubernetes CronJob 直接呼叫 DB / API，或
     - 某個未 clone 的內部排程服務
        ↓
[NMQ] RegularOrderCreateOrderProcess
  → RegularOrderService.GenerateOrder(startDate, endDate)
        ↓
[scmnmq] RegularOrderService.GenerateOrder()
  ├─ Step 1: 查詢 ERP DB
  │    GetExpectShippingList(startDate, endDate)
  │    → 找出 RegularOrder_ExpectShippingDate 在範圍內的所有 Nine1 訂單
  │
  ├─ Step 2: 對每筆訂單：
  │    a. InsertRegularOrderTracing (Status: New)
  │    b. ValidateSalePage (shopId, salePageId, saleProductSkuId)
  │       ├─ 失敗 → CreateOrderFail → 若昨日也失敗 → 遞延 + 建 RegularOrderCancelNotice task
  │       └─ 成功 → 呼叫 mweb API
  │
  └─ Step 3: 呼叫 mweb RegularOrderAutoGen API
       POST {RegularOrder.AutoGen.Uri}
       Body: { ShopId, MemberId, RegularOrderId }
       ↓
[mweb] TradesOrderV2Controller.RegularOrderAutoGen()
  → IsFromCompany() 驗證（內部 IP 限制）
  → TradesOrderService.GetPayProcessContextForRegularOrder(shopId, memberId, regularOrderId)
       → ShoppingCartV2Service.GetShoppingCartContextForRegularOrder(...)
       → PayProcessService.GetProcessContextForRegularOrderPay(...)
  → PayProcessService.CreateTradesOrder(payProcessContext)
  → 建立新的 TG/TS 訂單（TrackSourceTypeDef = "InternalSchedule"）
        ↓
回傳 RegularOrderAutoGenResponseEntity { Status, TradesOrderGroupCode, ... }
        ↓
[scmnmq] RegularOrderService.CreateOrderSuccess / CreateOrderFail
  → 更新 RegularOrderTracing 狀態
  → 更新 RegularOrder.ExpectShippingDate → NextExpectShippingDate
  → 計算並更新下次 NextExpectShippingDate（依頻率 Day/Week/Month/Year）
```

---

## 三、九個觸發鏈的 API 差異（Nine1 vs GameBall）

### `RegularOrderAutoGen`（Nine1 專用）

| 項目 | 說明 |
|------|------|
| 路徑 | `POST /webapi/TradesOrderV2/RegularOrderAutoGen` |
| 呼叫方 | scmnmq `RegularOrderService.GenerateOrder()` |
| 參數 | `shopId`（long）、`memberId`（int）、`regularOrderId`（long）|
| 收件人 / 配送資訊 | **不需要傳**（從 ERP RegularOrder 取）|
| 安全機制 | `IsFromCompany()` 內部 IP 驗證 |
| 防重複 | **無** `TraceId` 機制 |

### `SubscriptionOrderAutoGen`（GameBall 專用）

| 項目 | 說明 |
|------|------|
| 路徑 | `POST /webapi/TradesOrderV2/SubscriptionOrderAutoGen` |
| 呼叫方 | PHP `regular_create_housekeeper`（hardcoded `RegularOrderProvider = "GameBall"`）|
| 參數 | 完整收件人/配送資訊（ReceiverName/Address/City…）+ TraceId |
| 防重複 | `TraceId` + `RegularOrderTracing` 表防重（State: New → Success/Fail）|

---

## 四、建單成功後的狀態流轉

```
RegularOrderTracing
  New
   │
   ├─ ValidateSalePage 失敗 → Fail → (連續2天) → 遞延 + CancelShipping 通知
   └─ 呼叫 API 成功 → Success → 更新 RegularOrder 出貨日期

RegularOrder
  ExpectShippingDate     = 本次出貨日
  NextExpectShippingDate = 下次出貨日（依頻率計算）
  
  建單成功後：
  ExpectShippingDate     ← NextExpectShippingDate（本次）
  NextExpectShippingDate ← 計算下次（AddDays/Week/Month/Year）
```

---

## 五、建單失敗邏輯

```
CreateOrderFail(regularOrder, tracing, causeDef)
  ├─ 更新 RegularOrderTracing.Status = Fail
  ├─ 查詢昨日 RegularOrderTracing 是否也是 Fail
  │
  └─ 昨日也是 Fail（連續 2 天）？
       ├─ Yes → 遞延：RegularOrder.NextExpectShippingDate += 頻率週期
       │         建立 NMQ Task: "RegularOrderCancelNotice"
       │         → RegularOrderCancelNoticeProcess → 推播通知 + 寄信
       └─ No  → 靜默失敗，等明天重試
```

---

## 六、驗證項目（ValidateSalePage）

| 檢查項目 | 失敗原因碼 |
|---------|-----------|
| SalePage 不存在 | `SalePageIsDelete` |
| SalePage 已關閉 | `SalePageIsClose` |
| 販售時間未到 | `UnstartSelling` |
| 販售時間已過 | `OverSellingEndDateTime` |
| SKU 未啟用 | `SkuIsClose` |

---

## 七、頻率計算邏輯（`GetNextTimeShippingDate`）

| `OrderFrequencyTypeDef` | 計算方式 |
|------------------------|---------|
| `Day` | `NextExpectShippingDate.AddDays(OrderFrequency)` |
| `Week` | `NextExpectShippingDate.AddDays(OrderFrequency * 7)` |
| `Month` | `NextExpectShippingDate.AddMonths(OrderFrequency)` |
| `Year` | `NextExpectShippingDate.AddYears(OrderFrequency)` |

---

## 八、重要說明：`InternalSchedule` 防循環機制

mweb `RegularOrderAutoGen` 建立的訂單會設：

```
SalesOrderGroup_TrackSourceTypeDef = "InternalSchedule"
```

後續若這張訂單觸發 `OrderCreated` NMQ 事件，  
`RegularOrderCreatedProcess` 檢查到 `InternalSchedule` 後會直接跳過，  
**避免無限循環觸發**。

---

## 九、關鍵檔案索引

| 角色 | 路徑 |
|------|------|
| **NMQ Job（入口）** | `C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\NMQV2\RegularOrders\RegularOrderCreateOrderProcess.cs` |
| **NMQ Job DI** | `C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\NMQV2\RegularOrders\RegularOrderCreateOrderProcessSetting.cs` |
| **Task Data Entity** | `C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\BE\RegularOrders\RegularOrderCreateOrderEntity.cs` |
| **核心 Service** | `C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\BLV2\RegularOrders\RegularOrderService.cs` |
| **Service 介面** | `C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\BLV2\RegularOrders\IRegularOrderService.cs` |
| **API 呼叫 Service** | `C:\91APP\NMQ\nineyi.scm.nmqv2\ERP\Backend\BLV2\RegularOrders\RegularOrderAutoGenApiService.cs` |
| **mweb API（被呼叫）** | `C:\91APP\nineyi.webstore.mobilewebmall\WebStore\WebAPI\Controllers\TradesOrderV2Controller.cs`（L1108 `RegularOrderAutoGen`）|
| **mweb ShoppingCart** | `C:\91APP\nineyi.webstore.mobilewebmall\WebStore\Frontend\BLV2\ShoppingCartsV2\ShoppingCartV2Service.cs`（L634）|
| **Provider 定義** | `C:\91APP\nineyi.webstore.mobilewebmall\WebStore\Frontend\BE\RegularOrder\RegularOrderProviderTypeDefEnum.cs` |
| **ERP Repository 介面** | `C:\91APP\NMQ\nineyi.scm.nmqv2\ERP\DA\ERPDBV2\Repositories\Interface\IRegularOrderRepository.cs` |

---

## 十、未解之謎

| 項目 | 狀況 |
|------|------|
| `RegularOrderCreateOrder` NMQ task 的觸發來源 | 在本地 repo 找不到 `CreateTask("RegularOrderCreateOrder", ...)`。推測由 k8s CronJob、DB Stored Procedure 或未 clone 的內部排程服務建立 |
| `RegularOrder.AutoGen.Uri` config 的實際值 | 指向 mweb 的 `/webapi/TradesOrderV2/RegularOrderAutoGen`（需確認環境設定）|
