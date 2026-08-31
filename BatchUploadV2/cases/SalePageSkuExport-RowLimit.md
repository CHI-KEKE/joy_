# 案例：SalePageSkuExport 匯出筆數超過 Excel 行數上限，作業失敗

## 案例資訊

| 項目 | 值 |
|------|-----|
| 批次類型（TypeDef） | `SalePageSkuExport` |
| 異常類型 | `System.ArgumentOutOfRangeException` |
| Parameter name | `cellAddressInRange` |
| 錯誤訊息 | `Row number must be between 1 and 1048576` |
| 拋出位置 | `ClosedXML.Excel.XLRangeBase.Cell()` |
| 影響 | NMQ Task 直接失敗，作業狀態停在錯誤，使用者無法取得匯出檔 |

<br>

---

## 前台操作情境

使用者在 **SMS 後台 → 商品管理 → 商品列表**（`/SalePage/List`）點擊「匯出商品選項資料」按鈕。

匯出條件取自當前搜尋篩選（`ReturnSearchItem()`）。本次案例使用者**未設定任何篩選條件**即直接匯出，產生的查詢條件 JSON 如下：

```json
{
  "FilePassword": "",
  "ExportDataCondition": {
    "SupplierId": 16,
    "ShopId": 20,
    "Ids": [],
    "ExportFileType": 0,
    "SourceType": "SalePage",
    "ValidFlag": true,
    "Title": null,
    "IsClosed": null,
    "StartDateTime": null,
    "EndDateTime": null,
    "ReferrerPromotionId": null
    ...（其餘篩選欄位全為 null）
  }
}
```

`Ids=[]`（未指定商品頁）＋ 其餘條件全 null → 匯出 ShopId=20 **所有有效商品的所有 SKU**。

> 使用者本可在匯出前輸入商品頁序號（最多 50 個）或其他篩選條件縮小範圍，但系統目前**沒有事前筆數估算或警告機制**。

<br>

---

## 症狀

使用者從 SMS 後台觸發商品選項庫存量匯出（SalePageSkuExport），作業進入非同步處理後失敗，log 出現以下異常：

```
System.ArgumentOutOfRangeException: Row number must be between 1 and 1048576
Parameter name: cellAddressInRange
   at ClosedXML.Excel.XLRangeBase.Cell(XLAddress& cellAddressInRange)
   at ClosedXML.Excel.XLRangeBase.Cell(Int32 row, Int32 column)
   at NineYi.Utility.Excels.ExcelService.ConvertToExcelWorkbook(...)
      ExcelService.cs:line 506
   at NineYi.Utility.Excels.ExcelService.SaveToXlsx(...)
      ExcelService.cs:line 221
   at NineYi.SCM.Frontend.BLV2.BatchUpload.SalePageSkuExportService.DoProcess(...)
      SalePageSkuExportService.cs:line 270
   at NineYi.SCM.Frontend.NMQV2.BatchUpload.BatchUploadTaskProcess.DoBatchProcess(...)
      BatchUploadTaskProcess.cs:line 221
   ...
```

<br>

---

## 根本原因

### Excel 行數限制

xlsx 格式（ClosedXML 實作）單一工作表最多 **1,048,576 列**（rows）。
`ExcelService.ConvertToExcelWorkbook` 在寫入 header 後從第 2 列開始，依序遞增 `rowPointer`：

```csharp
// ExcelService.cs ~line 469
int rowPointer = 2;   // header 佔第 1 列

// ~line 495
foreach (DataRow row in exportData.Rows)
{
    rowPointer++;     // 每寫一筆 SKU 就 +1
    // ~line 506
    ws.Cell(rowPointer, colPointer)...  // rowPointer > 1,048,576 → 拋出例外
}
```

當匯出的 SKU 數量（DataTable 列數）超過 **1,048,575 筆**，`rowPointer` 超過 1,048,576，ClosedXML 拋出 `ArgumentOutOfRangeException`。

<br>

### 現有 catch 的缺陷

`SalePageSkuExportService.DoProcess` 有捕捉 `ArgumentOutOfRangeException`，但條件寫死只攔截「單格字元超限」的案例：

```csharp
// SalePageSkuExportService.cs line 262-271
catch (ArgumentOutOfRangeException ex)
{
    if (ex.HResult == -2146233086 && ex.Message.Contains("Cells can hold a maximum"))
    {
        // ✅ 攔截：單格字元 > 32,767 → 記 log，繼續
        this.Logger.Info("商品頁詳細說明超過Excel單一儲存格上限32767。");
    }
    else
    {
        throw;  // ❌ Row 超限走這裡 → rethrow → Task 失敗
    }
}
```

兩種 `ArgumentOutOfRangeException` 的 Message 比較：

| 情境 | Message |
|------|---------|
| 單格字元 > 32,767 | `"Cells can hold a maximum of 32767 characters"` |
| **Row 超限（本次）** | **`"Row number must be between 1 and 1048576"`** |

Row 超限的 message 不包含 `"Cells can hold a maximum"`，所以直接 `throw` → task 失敗。

<br>

---

## 觸發條件

| 前提 | 說明 |
|------|------|
| 匯出格式 | XLSX（CSV 不經過 ClosedXML，不會觸發） |
| SKU 數量 | > 1,048,575 筆（一商店 SKU 非常多時） |
| NPOI 設定 | `ShopStaticSetting: BatchUploadExcelExport/IsEnableNPOI` = false（未啟用 NPOI） |

> ⚠️ 改用 NPOI 也只是切換實作，**NPOI 同樣有 1,048,576 行上限**，不能根本解決問題。

<br>

---

## 完整失敗路徑

```
SMS 使用者觸發匯出
  ↓
BatchUploadProcess.DoJob()
  → 建立 1 筆 BatchUploadData（狀態 ReadyToProcess）
  → 送 Phase 2 NMQ task
  ↓
SalePageSkuExportTaskProcess.DoJob()
  → GetReadyToProcessDataList() → 取那 1 筆
  → DoProcess(batchUploadData)
      → ExportSalePageSkuDataAll()  ← 一次撈出所有 SKU（例：120 萬筆）
      → ConverToDataTable()         ← DataTable 有 120 萬 rows
      → ExcelService.SaveToXlsx()
          → ConvertToExcelWorkbook()
              → rowPointer 跑到第 1,048,577 → 💥 throw
  ← DoProcess 未攔截 → rethrow
  ← DoBatchProcess 未攔截 → task 失敗
```

<br>

---

## 排查步驟

### Step 1. 確認 BatchUpload 狀態

```sql
USE WebStoreDB

SELECT BatchUpload_Id, BatchUpload_Code, BatchUpload_TypeDef,
       BatchUpload_StatusDef, BatchUpload_SupplierId,
       BatchUpload_CreatedDatetime
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_TypeDef = 'SalePageSkuExport'
ORDER BY BatchUpload_CreatedDatetime DESC
```

### Step 2. 確認 BatchUploadData 狀態

```sql
USE WebStoreDB

SELECT BatchUploadData_Id, BatchUploadData_StatusDef,
       BatchUploadData_Data
FROM BatchUploadData (NOLOCK)
WHERE BatchUploadData_ValidFlag = 1
  AND BatchUploadData_BatchUploadId = @BatchUploadId
```

> 失敗時 `BatchUploadData_StatusDef` 通常是 `ProcessFailed` 或停在 `ReadyToProcess`。

### Step 3. 確認 NMQ Log

在 scmnmq log 中搜尋：

```
Row number must be between 1 and 1048576
```

或：

```
SalePageSkuExportService.DoProcess
```

### Step 4. 確認該店 SKU 總數

```sql
USE WebStoreDB

SELECT COUNT(*)
FROM SalePageSku (NOLOCK)
WHERE SalePageSku_ValidFlag = 1
  AND SalePageSku_SupplierId = @SupplierId
  -- 視查詢條件加上其他篩選
```

<br>

---

## 建議修正方向

| 方案 | 說明 |
|------|------|
| **方案 A：事前檢查行數** | 在 `DoProcess` 撈完資料後，若 `result.Rows.Count > 1,048,575` 則提前終止並回寫友善錯誤訊息 |
| **方案 B：分批多個 xlsx** | 自動切分成多個 Excel 檔，壓縮成 zip 提供下載 |
| **方案 C：強制改輸出 CSV** | 當 SKU 數量超過 xlsx 上限時自動 fallback 到 CSV 格式 |
| **方案 D：修補 catch 邏輯** | 在現有 catch 中補上 Row 超限的分支，給出清楚的使用者錯誤提示（最小成本） |

**方案 D 的最小 patch 示意**：

```csharp
catch (ArgumentOutOfRangeException ex)
{
    if (ex.HResult == -2146233086 && ex.Message.Contains("Cells can hold a maximum"))
    {
        this.Logger.Info("商品頁詳細說明超過Excel單一儲存格上限32767。");
    }
    else if (ex.Message.Contains("Row number must be between"))
    {
        // 補上：行數超限，給友善錯誤
        var msg = $"匯出資料筆數({totalCount})超過 xlsx 行數上限(1,048,575)，請縮小查詢範圍或改用 CSV 格式匯出。";
        this.Logger.Info(msg);
        // 可選：寫入 BatchUploadMessage 讓前台顯示
        dataItem.BatchUploadData.BatchUploadData_StatusDef = BatchUploadDataStatusEnum.ProcessFailed.ToString();
        return;
    }
    else
    {
        throw;
    }
}
```

<br>

---

## 相關文件

- [SalePageSkuExport 批次類型說明](../types/SalePageSkuExport.md)
