# CancelOrderProcess 錯誤分析報告

- **發生時間**：2026-03-30 15:42:04
- **Task ID**：`3adbe963-128a-4a1e-b05b-a7d9fb56027b`
- **Job Name**：`CancelOrderProcess`
- **輸入資料**：`TS260330R000026`

---

## 錯誤摘要

| 項目 | 內容 |
|------|------|
| 例外類型 | `System.NullReferenceException` |
| 錯誤訊息 | Object reference not set to an instance of an object. |
| 錯誤檔案 | `CancelOrderService.cs` |
| 錯誤行號 | **Line 297** |
| 重試次數 | 10 次（全部失敗） |

---

## StackTrace

```
at NineYi.ERP.Backend.BLV2.CancelOrder.CancelOrderService.DoCancelOrder(String tradesOrderSlaveCode, Int64 taskId)
   in CancelOrderService.cs:line 297

at NineYi.ERP.Backend.NMQV2.CancelOrder.CancelOrderProcess.DoJob(TaskInfoEntity taskInfo)
   in CancelOrderProcess.cs:line 88

at NMQ.Core.Worker.Service.WorkerProcess.DoJob(StdIOTask task)
   in WorkerProcess.cs:line 105
```

---

## 根本原因分析

錯誤發生在 `CancelOrderService.cs` 第 296～297 行：

```csharp
// Line 296
var salesOrderSlaveItem = this._salesOrderService
    .GetSalesOrderSlaveByTradesSlaveCode(tradesOrderSlaveCode);

// Line 297 ← 💥 NullReferenceException 在此
var salesOrderFeeItem = this._salesOrderService
    .GetSalesOrderFeeBySalesOrderId(salesOrderSlaveItem.SalesOrderSlave_SalesOrderId);
```

**`salesOrderSlaveItem` 為 `null`**，因為 `GetSalesOrderSlaveByTradesSlaveCode("TS260330R000026")` 查無對應的 SalesOrderSlave 資料，導致第 297 行存取 `.SalesOrderSlave_SalesOrderId` 時拋出例外。

---

## 錯誤觸發流程

```
CancelOrderProcess (tradesOrderSlaveCode: TS260330R000026)
  └─> DoCancelOrder()
        └─> GetSalesOrderSlaveByTradesSlaveCode("TS260330R000026") → 回傳 null
        └─> salesOrderSlaveItem.SalesOrderSlave_SalesOrderId
              └─> 💥 NullReferenceException (line 297)
```

---

## 可能原因

1. **訂單資料不存在**：`TS260330R000026` 這筆 `TradesOrderSlaveCode` 在資料庫中查不到對應的 SalesOrderSlave 記錄
2. **資料尚未同步**：取消申請發出時，對應的銷售子單尚未建立完成
3. **資料已被刪除**：SalesOrderSlave 資料在處理前已從資料庫中移除

---

## 次要問題：SSL 憑證錯誤

任務全部重試失敗後，嘗試更新任務狀態至資料庫時，額外發生 SSL 連線錯誤：

```
A connection was successfully established with the server, but then an error
occurred during the login process.
(provider: SSL Provider, error: 0 - The certificate chain was issued by an
authority that is not trusted.)
```

- 重試 5 次後放棄，最終顯示 `Failed to update task status`
- 後續出現 `Invalid region endpoint provided`
- 此問題為環境/憑證設定問題，與 NullReferenceException 為獨立的兩個問題

---

## 建議修復方式

在 `CancelOrderService.cs` 第 296 行後，加入 `null` 防禦性檢查：

```csharp
var salesOrderSlaveItem = this._salesOrderService
    .GetSalesOrderSlaveByTradesSlaveCode(tradesOrderSlaveCode);

//// 防禦性檢查：銷售子單不存在時提早拋出明確例外，避免 NullReferenceException
if (salesOrderSlaveItem == null)
{
    throw new ApplicationException(
        string.Format("查不到銷售子單!! TS:{0}", tradesOrderSlaveCode));
}

var salesOrderFeeItem = this._salesOrderService
    .GetSalesOrderFeeBySalesOrderId(salesOrderSlaveItem.SalesOrderSlave_SalesOrderId);
```

---

## SSL 問題修復方向

於 SQL Server 連線字串加入 `TrustServerCertificate=True`（僅限非正式環境），或在正式環境中安裝受信任的 SSL 憑證：

```
Server=xxx;Database=xxx;TrustServerCertificate=True;...
```

---

*報告產生日期：2026-03-31*
