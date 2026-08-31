# scmnmq 運費（SalesOrderFee）處理邏輯

> 來源專案：`C:\91APP\NMQ\nineyi.scm.nmqv2`

---

## 1. SalesOrderFee 欄位說明

```csharp
public long   SalesOrderFee_Id { get; set; }
public long   SalesOrderFee_SalesOrderId { get; set; }
public decimal SalesOrderFee_Fee { get; set; }               // 運費金額
public long?  SalesOrderFee_TradesOrderSlaveId { get; set; } // ⭐ null = 未使用（可退運費）
public bool   SalesOrderFee_IsClosed { get; set; }           // 是否結案
public DateTime? SalesOrderFee_ClosedDateTime { get; set; }
public DateTime? SalesOrderFee_ShippingFinishDateTime { get; set; }
public DateTime? SalesOrderFee_RefundRequestCreateDateTime { get; set; }
```

`SalesOrderFee` 以 `salesOrderId` 為維度。

---

## 2. SalesOrderFee_TradesOrderSlaveId 何時被寫入（不為 null）

> 寫入 = 運費被「使用」或已結案

| # | 觸發時機 | 檔案 | 行號 | TriggerPoint | 退運費？ |
|---|---------|------|------|-------------|---------|
| 1 | **出貨完成**（宅配到店） | `ShippingOrderService.cs` | 1384 | `ShippingPickedUp` | ❌ 不退 |
| 2 | **出貨失敗**（宅配無法投遞） | `ShippingOrderService.cs` | 1552 | `ShippingDeliveryFail` | ❌ 不退 |
| 3 | **超商驗收失敗** | `ShippingOrderShippingFailService.cs` | 727 | — | ✅ 可退（hasVerifyFailShipment 繞過） |
| 4 | **取消流程結案**（全退） | `SalesOrderRepository.cs` | 1009 | `CancelOrderProcess` | ✅ 退款後才結案 |

### 各情境程式碼

**情境 1 & 2 — 出貨完成 / 出貨失敗（ShippingOrderService.cs）**

```csharp
// L1384（出貨完成 ShippingPickedUp）
// L1552（出貨失敗 ShippingDeliveryFail）
salesOrderFee.SalesOrderFee_IsClosed = true;
salesOrderFee.SalesOrderFee_ClosedDateTime = DateTime.Now;
salesOrderFee.SalesOrderFee_ShippingFinishDateTime = DateTime.Now;
salesOrderFee.SalesOrderFee_TradesOrderSlaveId =
    shippingOrderSlaveEntity.ShippingOrderSlave_TradesOrderSlaveId;
this._salesOrderRepository.UpdateSalesOrderFee(salesOrderFee);
```

> 貨物一出門（或確認無法投遞），運費就結案，之後即使全退也不退運費。

**情境 3 — 超商驗收失敗（ShippingOrderShippingFailService.cs L723-729）**

```csharp
// 只在 IsClosed == false 才寫
if (salesOrderFee != null && salesOrderFee.SalesOrderFee_IsClosed == false)
{
    salesOrderFee.SalesOrderFee_IsClosed = true;
    salesOrderFee.SalesOrderFee_ClosedDateTime = DateTime.Now;
    salesOrderFee.SalesOrderFee_TradesOrderSlaveId =
        shippingSlaveItem.ShippingOrderSlave_TradesOrderSlaveId; // L727
    salesOrderFee.SalesOrderFee_ShippingFinishDateTime = DateTime.Now;
    this._salesOrderRepository.UpdateSalesOrderFee(salesOrderFee);
}
```

> ⚠️ 雖然寫入了 TradesOrderSlaveId，但退款邏輯裡 `hasVerifyFailShipment == true` 會繞過 `isSalesOrderFeeUnused` 的判斷，**仍然可以退運費**。

**情境 4 — 取消流程結案（SalesOrderRepository.cs L1009）**

```csharp
updateItem.SalesOrderFee_IsClosed = true;
updateItem.SalesOrderFee_ClosedDateTime = DateTime.Now;
updateItem.SalesOrderFee_TradesOrderSlaveId = tradesOrderSlaveId; // L1009
updateItem.SalesOrderFee_UpdatedDateTime = DateTime.Now;
updateItem.SalesOrderFee_UpdatedUser = "NMQV2 CancelOrderProcess";
context.SaveChanges();
```

> 這是退款流程**自己執行結案**，不是禁止退款，而是退款後標記已處理。

---

## 3. 退運費條件判斷

### ThirdPartyRefundRequestService.cs Line 273

```csharp
bool isSalesOrderFeeUnused = isAllCanceled
    && salesOrderFee != null
    && salesOrderFee.SalesOrderFee_TradesOrderSlaveId == null;
```

| 條件 | 說明 |
|------|------|
| `isAllCanceled == true` | 同一訂單所有子單都已取消 |
| `salesOrderFee != null` | 訂單有運費單（非免運） |
| `TradesOrderSlaveId == null` | 運費未被出貨綁定（從未出貨） |

### 再進一步（Line 283-298）

```csharp
if (isSalesOrderFeeUnused || hasVerifyFailShipment)
{
    int shipmentCount = GetShipmentCount(...);

    if (hasWaitingToShipment == false ||
       (hasVerifyFailShipment && shipmentCount == verifyFailShipmentCount))
    {
        salesOrderFeeFee = salesOrderFee.SalesOrderFee_Fee; // ← 才真正退運費
    }
}
```

---

## 4. 各情境結論

| 情境 | 退運費？ | 原因 |
|------|---------|------|
| 全部商品尚未出貨 → 全取消 | ✅ 自動退 | `TradesOrderSlaveId == null` |
| 部分出貨，其餘取消 | ❌ 不退 | `TradesOrderSlaveId` 已寫入 |
| 全部出貨後退貨 | ❌ 不退 | 運費已使用 |
| 超商驗收失敗（全部驗收失敗子單） | ✅ 退 | `hasVerifyFailShipment == true` |

---

## 5. UpdateSalesOrderFeeClose 結案內容

> `SalesOrderService.cs` Line 400-407

```csharp
salesOrderFee.SalesOrderFee_IsClosed = true;
salesOrderFee.SalesOrderFee_ClosedDateTime = DateTime.Now;
salesOrderFee.SalesOrderFee_TradesOrderSlaveId = tradesOrderSlaveId; // 綁定 TS
salesOrderFee.SalesOrderFee_UpdatedDateTime = DateTime.Now;
salesOrderFee.SalesOrderFee_UpdatedUser = "NMQV2 CancelOrderProcess";
this._salesOrderRepository.UpdateSalesOrderFee(salesOrderFee);
```

---

## 6. 多付款方式運費攤提

當訂單使用多種付款方式（IsMultiPayment）時，運費需依比例攤提給各付款方式各自建立退款單。

**ThirdPartyRefundRequestService.cs Line 301-326：**

```csharp
foreach (var otherPayTypeRefundRequest in otherPayTypeRefundRequests)
{
    var otherPayTypeTotalFee = _salesOrderFeeRepository.GetOtherPayTypeTotalFee(
        otherPayTypeRefundRequest.RefundRequest_TradesOrderSlaveCode,
        otherPayTypeRefundRequest.RefundRequest_PayProfileTypeDef);

    if (otherPayTypeTotalFee?.TotalFee > 0)
    {
        salesOrderFeeFee -= otherPayTypeTotalFee.TotalFee; // 從主運費扣除攤提部分

        // 為其他付款方式建立獨立的運費退款單
        this.CreateRefundRequest(
            otherPayTypeRefundRequest,
            RefundRequestSourceDefEnum.SalesOrderFee, ...);
    }
}
```

---

## 7. 超商驗收失敗的特殊流程

**GetVerifyFailShipmentsAndSalesOrderFee（SalesOrderService.cs Line 459-496）**

觸發條件：出貨子單狀態為以下 5 種之一，且非超商取貨付款（Family / SevenEleven）：

```csharp
// 超商驗收失敗 5 種狀態
"VerifyFailAbnormalPackage"  // 異常外觀
"VerifyFailErrorCode"        // 錯誤代碼
"VerifyFailInvalidCode"      // 無效代碼
"VerifyFailLost"             // 遺失
"VerifyFailRenovation"       // 整修
```

驗收失敗時：只要所有貨運子單都是驗收失敗狀態，就可以退運費（不需等其他子單完成）。

---

## 8. 完整決策樹

```bash
訂單成立
  ↓
SalesOrderFee 建立（Fee=XX, TradesOrderSlaveId=null, IsClosed=false）
  ↓
┌─────────────────────────────────────────────────┐
│              出貨（撿貨完成）                     │
│  ShippingOrderService 寫入 TradesOrderSlaveId    │
│  → 運費被使用，之後不退運費                       │
└─────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────┐
│              取消流程                            │
│  isAllCanceled?                                 │
│  ├─ NO  → 部分取消，運費不動                     │
│  └─ YES → 全取消                                │
│      TradesOrderSlaveId == null?                │
│      ├─ NO  → 運費已使用，不退                  │
│      └─ YES → shipmentCount == 0?              │
│          ├─ NO  → 有待出貨子單，不退            │
│          └─ YES → ✅ 建立 SalesOrderFee 退款單  │
└─────────────────────────────────────────────────┘
  ↓
┌─────────────────────────────────────────────────┐
│              超商驗收失敗                        │
│  hasVerifyFailShipment == true                  │
│  + 所有子單都是驗收失敗                          │
│  → ✅ 建立 SalesOrderFee 退款單                 │
└─────────────────────────────────────────────────┘
```

---

## 9. 關鍵檔案索引

| 功能 | 檔案路徑 | 行號 |
|------|---------|------|
| 退款主邏輯（含運費判斷） | `scmnmq — ERP\Backend\BLV2\RefundRequests\ThirdPartyRefundRequestService.cs` | 273-329 |
| 結案更新（UpdateSalesOrderFeeClose） | `scmnmq — ERP\Backend\BLV2\SalesOrder\SalesOrderService.cs` | 400-407 |
| 驗收失敗判斷 | `scmnmq — ERP\Backend\BLV2\SalesOrder\SalesOrderService.cs` | 459-496 |
| 出貨完成綁定運費 | `scmnmq — ERP\Backend\BLV2\ShippingOrder\ShippingOrderService.cs` | 1384（ShippingPickedUp） |
| 出貨失敗綁定運費 | `scmnmq — ERP\Backend\BLV2\ShippingOrder\ShippingOrderService.cs` | 1552（ShippingDeliveryFail） |
| 超商驗收失敗結案 | `scmnmq — ERP\Backend\BLV2\ShippingOrder\StoreShippings\ShippingOrderShippingFailService.cs` | 727 |
| 取消流程結案（Repository 層） | `scmnmq — ERP\DA\ERPDBV2\Repositories\SalesOrderRepository.cs` | 1009 |
| ATM 取消 | `scmnmq — ERP\Backend\BLV2\CancelOrders\AtmCancelOrderService.cs` | 220-236 |
| 多付款方式攤提查詢 | `scmnmq — ERP\DA\ERPDBV2\Repositories\SalesOrderFeeRepository.cs` | — |
| SMS 手動退運費 API | `sms — WebSite\Controllers\Apis\RefundRequestController.cs` | 136-141 |
| SMS 手動退運費 Service | `sms — BusinessLogic\Services\RefundRequests\RefundRequestService.cs` | 360-471 |
