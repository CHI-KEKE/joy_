# SalePageSkuExport — 商品選項資料匯出

## 基本資訊

| 項目 | 值 |
|------|-----|
| 批次類型（TypeDef） | `SalePageSkuExport` |
| NMQ Phase 2 Job Class | `SalePageSkuExportTaskProcess` |
| Service | `SalePageSkuExportService.cs` |
| Entity | `SalePageSkuExportEntity.cs` |
| 匯出單位 | 一列 = 一個 SKU（商品選項） |
| BatchUploadData 筆數 | **1 筆**（存查詢條件 JSON，與 SKU 數量無關） |

<br>

---

## 前台入口

**SMS 後台 → 商品管理 → 商品列表**
URL：`/SalePage/List`（`Views/SalePage/List.cshtml`）

「匯出商品選項資料」按鈕出現在頁面兩處（上方篩選區按鈕列、下方列表按鈕列），點擊後呼叫 `ExportSku()`。

### 可用的篩選條件

匯出條件直接取自當前搜尋篩選（`ReturnSearchItem()`），使用者在點匯出前可透過以下條件縮小範圍：

| 篩選條件 | JSON 欄位 | 備註 |
|---------|-----------|------|
| 商品頁序號（輸入框） | `Ids` | 最多 50 個，有值才帶入，否則為 `[]` |
| 商品名稱 | `Title` | 模糊搜尋 |
| 上下架 | `IsClosed` | |
| 料號 | `SaleProductSKUOuterId` | |
| 日期範圍 | `StartDateTime / EndDateTime / DateTimeType` | |
| 庫存可售量 | `SellingQtyOperator / SellingQty` | 進階設定 |
| 配送方式 | `DeliverTypeSet` | 進階設定 |

> ⚠️ 若使用者**未設定任何篩選條件**直接按匯出，`Ids=[]`、其餘欄位全 `null`，代表匯出該店**所有**有效商品的 SKU，數量可能超過 xlsx 上限。

<br>

---

## 完整流程

### Phase 0 — SMS 前台觸發

```
SMS TypeScript:
  list.controller.ts → ExportSku()
    → ReturnSearchItem()               // 收集當前篩選條件
    → BatchExport("SalePageSkuExport")
    → SalePageService.CreateBatchExportDataTask(searchModel)
    → POST /api/SalePage/CreateBatchExportDataTask
```

```
SMS API Controller:
  SalePageController.CreateBatchExportDataTask()
    → BatchUploadService.CreateBatchExportTask()
      → CreateBatchUploadEntity()          // 建立 BatchUpload 主檔
      → CreateBatchUploadNMQTask()         // 送 NMQ task 至 "BatchUpload" queue
```

Phase 0 送出的 NMQ data（`BatchUploadNMQDataEntity`）：

```json
{
  "BatchUploadId": 12345,
  "ExportDataCondition": { /* SalePageExportSearchEntity */ },
  "FilePassword": "...",
  "UploadUser": "allenlin@nine-yi.com"
}
```

<br>

---

### Phase 1 — scmnmq 建立 BatchUploadData

由 `BatchUploadProcess.DoJob()` 處理（`BatchUpload` queue）。

```
BatchUploadProcess.DoJob()
  → 依 TypeDef 解析出 SalePageSkuExportService（Autofac DI）
  → SalePageSkuExportService.LoadData()
      → ExportDataCondition → SalePageExportSearchEntity
      → CreateBatchUploadDataList(searchEntity)
          → 建立 1 筆 BatchUploadDataEntity
             BatchUploadData_Title    = ShopId
             BatchUploadData_StatusDef = ReadyToProcess
             BatchUploadData_Data     = { FilePassword, ExportDataCondition }.ToJson()
  → CreateBatchUploadData([那 1 筆])  // 寫入 DB
  → 發送 Phase 2 NMQ task
```

Phase 1 送出的 NMQ data（`BatchUploadTaskNMQDataEntity`）：

```json
{
  "BatchUploadId": 12345,
  "ProcessType": "ByProcessCount",
  "ProcessCount": 10,
  "BatchUploadDataId": 0,
  "UploadUser": "allenlin@nine-yi.com"
}
```

> ⚠️ `ProcessType` 為 `ByProcessCount`，但實際上 BatchUploadData 只有 **1 筆**，故一輪就處理完畢，不會有多次 loop。

<br>

---

### Phase 2 — scmnmq 執行匯出

由 `SalePageSkuExportTaskProcess.DoJob()` 處理（`SalePageSkuExportTaskProcess` job）。

```
SalePageSkuExportTaskProcess.DoJob()  （繼承自 BatchUploadTaskProcess）
  → 判斷 Job 數量是否超過上限
  → 依 TypeDef 解析出 SalePageSkuExportService
  → ExecuteByProcessType()
      → GetReadyToProcessDataList()    // 從 DB 撈 ReadyToProcess 的 BatchUploadData
      → DoBatchProcess(dataList)
          → DoProcess(batchUploadData) // 逐筆處理
```

<br>

`DoProcess()` 核心邏輯：

```
DoProcess(batchUploadData)
  1. 反序列化 BatchUploadData_Data → SalePageExportSearchEntity
  2. 讀 ShopStaticSetting:
       - SalePage/IsEnableSearch  → 決定走 ByIds 還是全撈
  3. 撈 SKU 資料：
       - isEnableSearch / isReferrerPromotion → ExportSalePageSkuDataAllByIds()
       - else                                 → ExportSalePageSkuDataAll()
  4. 若 SourceType == "ShopCategory"，再依分類過濾 exportData
  5. 轉換為 DataTable → FilterDataTable（過濾欄位）
  6. 依 ExportFileType：
       - CSV  → CvService.Save()
       - XLSX → 讀 ShopStaticSetting: BatchUploadExcelExport/IsEnableNPOI
                  true  → ExcelService.SaveToNPOIXlsx()
                  false → ExcelService.SaveToXlsx()   ← 預設走 ClosedXML
  7. 依 AppSetting BatchUpload.ExportDataCompression.IsEnable 決定是否壓縮 zip
  8. 更新 BatchUpload_FileName、BatchUpload_ExportFileDownloadPath
```

<br>

---

## 匯出資料結構（每列 = 一個 SKU）

`SalePageSkuExportEntity` 主要欄位：

| 欄位 | 說明 |
|------|------|
| `Id` | 商品頁序號 |
| `Title` | 商品名稱 |
| `SkuId` | SKU 序號 |
| `SkuName` | SKU 選項名稱 |
| `OuterId` | 料號 |
| `SellingQty` | 庫存數量 |
| `Price` | 售價 |
| `CostPrice` | 成本 |
| ... | （依 FilterDataTable 欄位設定動態過濾） |

<br>

---

## BatchUploadData 結構說明

| 角色 | 數量 | 說明 |
|------|------|------|
| `BatchUpload` | 1 筆 | 匯出作業主檔 |
| `BatchUploadData` | **1 筆** | 存放查詢條件 JSON（不是 SKU 資料） |
| 實際 SKU 資料 | N 筆 | 在 `DoProcess` 時從 DB 一次撈出 |

**BatchUploadData_Data 內容範例**：

```json
{
  "FilePassword": "",
  "ExportDataCondition": {
    "ShopId": 6780,
    "SourceType": "All",
    "ExportFileType": "Xlsx",
    "Ids": null,
    "ReferrerPromotionId": 0,
    ...
  }
}
```

<br>

---

## 相關設定

| 設定 | 說明 |
|------|------|
| `ShopStaticSetting: SalePage/IsEnableSearch` | 是否用搜尋引擎（ElasticSearch）撈資料 |
| `ShopStaticSetting: BatchUploadExcelExport/IsEnableNPOI` | 是否改用 NPOI 產生 xlsx（預設 ClosedXML） |
| `AppSetting: BatchUpload.ExportDataCompression.IsEnable` | 是否壓縮成 zip 後提供下載 |
| `AppSetting: BatchUpload.SalePageSkuExport.TaskProcessCount` | Phase 2 每輪處理筆數（預設 10，實際只有 1 筆） |

<br>

---

## 相關查詢 SQL

```sql
USE WebStoreDB

-- 查作業主檔
SELECT BatchUpload_Id, BatchUpload_Code, BatchUpload_TypeDef,
       BatchUpload_StatusDef, BatchUpload_FileName, BatchUpload_ExportFileDownloadPath,
       BatchUpload_SupplierId, BatchUpload_CreatedDatetime
FROM BatchUpload (NOLOCK)
WHERE BatchUpload_ValidFlag = 1
  AND BatchUpload_TypeDef = 'SalePageSkuExport'
ORDER BY BatchUpload_CreatedDatetime DESC

-- 查子資料（BatchUploadData，通常只有 1 筆）
SELECT BatchUploadData_Id, BatchUploadData_StatusDef,
       BatchUploadData_Data, BatchUploadData_CreatedDatetime
FROM BatchUploadData (NOLOCK)
WHERE BatchUploadData_ValidFlag = 1
  AND BatchUploadData_BatchUploadId = @BatchUploadId

-- 查錯誤訊息
SELECT *
FROM BatchUploadMessage (NOLOCK)
WHERE BatchUploadMessage_ValidFlag = 1
  AND BatchUploadMessage_BatchUploadId = @BatchUploadId
```

<br>

---

## 相關檔案

| 檔案 | 路徑 |
|------|------|
| SMS 前端 | `TypeScripts/Modules/SalePage/list.controller.ts` |
| SMS API | `Controllers/Apis/SalePageController.cs` |
| SMS Service | `BusinessLogic/Services/BatchUploads/BatchUploadService.cs` |
| scmnmq Service | `SCM/Frontend/BLV2/BatchUpload/SalePageSkuExportService.cs` |
| scmnmq TaskProcess | `SCM/Frontend/NMQV2/BatchUpload/SalePageSkuExportTaskProcess.cs` |
| scmnmq Phase 1 | `SCM/Frontend/NMQV2/BatchUpload/BatchUploadProcess.cs` |
| scmnmq Phase 2 base | `SCM/Frontend/NMQV2/BatchUpload/BatchUploadTaskProcess.cs` |
| Excel 工具 | `Core/Utility/Excels/ExcelService.cs` |
| Export Entity | `WebStore/Backend/BE/BatchUpload/SalePageSkuExportEntity.cs` |


## Doc (文件)


<add key="QA.File.Path.Tmp" value="\\TYO-QA-Filer1\Files\Tmp" xdt:Transform="Insert" />

C:\Files\Tmp\BatchExportSalesOrderData\BA2604281100003_54361439_SalesOrderData_20260429101607_1.csv


System.IO.DirectoryNotFoundException: '找不到路徑 'C:\Files\Tmp\BatchExportSalesOrderData\BA2604281100003_54361439_SalesOrderData_20260428192111_1.csv' 的一部分。'



## BatchUpload 參數


```json
{
  "BatchUploadId": 53939,
  "SupplierId": 2914,
  "UploadType": "SalesOrderDataExport",
  "UploadUser": "allenlin@nine-yi.com",
  "FilePassword": "YWxsZW4=",
  "ExportDataCondition": {
    "ShopID": 12765,
    "SupplierID": 2914,
    "DateTimeType": "SalesOrderSlaveDate",
    "StartDateTime": "2026-04-01T00:00:00+08:00",
    "EndDateTime": "2026-05-05T23:59:00+08:00",
    "SalesOrderStatus": null,
    "ShippingOrderStatus": null,
    "ExportVersion": "v2",
    "PayProfileStatisticsTypeDef": null,
    "ShippingProfileStatisticsTypeDef": null,
    "ShippingProfileDistributorDef": null,
    "TemperatureTypeDef": null,
    "CancelOrderSlaveCauseDef": null,
    "ConfirmTypeDef": null,
    "ShippingTypeDef": null
  },
  "Country": null
}
```