# ModifyRewardPromotionSalePage — 批次更新活動商品

## 基本資訊

| 項目 | 值 |
|------|-----|
| 批次類型（TypeDef） | `ModifyRewardPromotionSalePage` |
| 執行 Task 類型 | `BatchModifyPromotionSalePageTask` |
| NMQ JobId（子 Task） | 334 |
| Service | `BatchModifyPromotionSalePageService` |
| 範例檔位置 | `E:\Storage\Docs\ModifyRewardPromotionSalePage` |

<br>

---

## 完整流程

```bash
1. 使用者上傳 Excel（OSM UploadFile API）
2. 呼叫 CreateBatchTask API → 第一道驗證
3. 建立 BatchUpload NMQ Task（主流程開始）
4. LoadExcel：讀取 Excel → 建立 BatchUploadData
5. DoValidate：二次驗證
6. Insert BatchUploadData + 建立子 NMQ Task
7. 子 NMQ Task：每 10 筆一組，呼叫 SCMAPIV2 執行異動
8. 持續觸發直到所有資料處理完成
```

<br>

---

## Step 1. 上傳 Excel 檔案

**API**

```bash
POST https://sms.qa1.hk.91dev.tw/Api/UploadFile
Content-Type: multipart/form-data
```

**FormData 範例**

```bash
file: (binary)
uniqueKey: f9d48752-e06c-48e7-a021-f78e42d983fa
type: 2
singleFile: false
flowFilename: Batch update products in promotions template (1).xlsx
flowIdentifier: 11956-Batchupdateproductsinpromotionstemplate(1)xlsx
flowTotalChunks: 1
flowChunkNumber: 1
flowChunkSize: 52428800
flowTotalSize: 11956
```

> 暫存路徑規則：`{tmpFolder} / {type} / {uniqueKey（GUID）}`

**範例檔下載**

```bash
https://sms.qa1.hk.91dev.tw/Docs/ModifyRewardPromotionSalePage/Batch%20update%20products%20in%20promotions%20template.xlsx
```

<br>

---

## Step 2. CreateBatchTask（第一道驗證）

**API**

```bash
POST https://sms.qa1.hk.91dev.tw/Api/BatchUpload/CreateBatchTask
```

**Request**

```json
{
    "FileName": "Batch update products in promotions template.xlsx",
    "FileGuid": "9a684a4d-8bbb-475b-845c-9c53ecacae4a",
    "BatchUploadType": 164,
    "NeedAdvancedVerify": false,
    "ShopId": 2,
    "ScheduleDateTime": null
}
```

**第一道驗證項目**

```csharp
this.GetFilePath();
this.VerifyFile();
```

- 檔案格式：`.xlsx`
- Sheet 名稱：TW = `批次更新活動商品範例檔`；其他 = `Update`
- 上限筆數：Config `BatchUpload.{type}.MaxCount`，預設 500
- 最早排程時間：Config `BatchUpload.{type}.SoonestScheduleOffsetInMinutes`，預設 30 分鐘
- `HasMappingError`
- 必填欄位：`PromotionId` / `ScheduleTime` / `SalePageIds` / `ModifyStatus`
- `PromotionId` 格式驗證
- `IsSalePageIdsValid`
- `ValidModifyStatuses`

<br>

---

## Step 3. 主 NMQ Task Data

```json
{
    "BatchUploadId": 11229,
    "SupplierId": 2,
    "UploadType": "ModifyRewardPromotionSalePage",
    "UploadUser": "allenlin@nine-yi.com",
    "FilePassword": "",
    "ExportDataCondition": null,
    "Country": "HK"
}
```

<br>

---

## Step 4. LoadExcel

使用 LinqToExcel 套件（詳見 [reference/load-excel.md](../reference/load-excel.md)）

**Excel 欄位 Mapping（`BatchModifyPromotionSalePageMappingProfile`）**

| 語系 | ScheduleTime | PromotionId | SalePageIds | ModifyStatus |
|------|-------------|-------------|-------------|--------------|
| TW | 期望生效時間 | 活動序號 | 商品頁序號 | 異動狀態 |
| Others | Execution Time / Effective Time | Promotion ID | Product Page ID | Update / Action |

**實作位置**

```csharp
.RegisterBatchUploadService<BatchModifyPromotionSalePageService>(BatchUploadTypeEnum.ModifyRewardPromotionSalePage)
// 位置：BatchUploadProcess.InitContainerBuilder
```

**appSetting**

```bash
BatchUpload.Storage.Floder（預設：C:\Temp\UploadData）
路徑：C:\Temp\UploadData / supplierId / BatchUpload_UploadGuid
```

**Excel 欄位取值**

```csharp
ScheduleTime = x.ScheduleTime,
PromotionId = int.Parse(x.PromotionId),
ModifyStatus = x.ModifyStatus,
SalePageIds = x.GetSalePageIds()
```

**包成 Wrapper**

每一列 Excel 長一個 `BatchUploadDataWrapperEntity`，包含：

- `BatchUploadData`（BatchUploadDataEntity — 紀錄 title, data, status 等）
- `DataEntityList`（一組 promotion 維度的資料）
- `DataEntity`（一對一情境用）

> 一個 wrapper = 一個 promotion 維度 = Excel 某一列

<br>

---

## Step 5. DoValidate（二次驗證）

1. 依 `PromotionId` 分群
2. 集合驗證

**使用 `BatchModifyPromotionOuterIdDataEntityValidator`（FluentValidation）**

- 驗重複料號
- 驗證活動序號
- 驗證料號字元
- 驗證異動狀態

<br>

---

## Step 6. Insert BatchUploadData & 建立子 NMQ

**依 Supplier 決定是否 Bulk Insert**

- Config Key: `BulkInsert.Enable.SupplierIds`（groupname = `BatchUpload`，shopId = 0）
- 有開啟（limit = 10000）：`BatchUploadRepository.BulkInsertBatchUploadData()`
- 沒開啟（用 `WebStoreDB csp_InsertBatchUploadData`）：`BatchUploadRepository.CreateBatchUploadData()`

**BatchUpload 狀態更新規則**

| 條件 | 狀態 |
|------|------|
| `dataList.Count == 0` | `Finish` |
| `dataList.Count > 0` | `WaitingToProcess` |
| 有 `ValidateFailed` 資料 | `ScheduleAbortedWithErrors` |

**子 NMQ Task Data 結構**

```csharp
taskNMQDataEntity.BatchUploadId = ...;
taskNMQDataEntity.ProcessType = "ByProcessCount";
taskNMQDataEntity.ProcessCount = AppSetting("BatchUpload.{TypeDef}.TaskProcessCount", "10");
taskNMQDataEntity.UploadUser = ...;
```

<br>

---

## Step 7. 子 NMQ Task（BatchModifyPromotionSalePageTask）

**子 Task Data 範例**

```json
{
    "BatchUploadId": 11221,
    "ProcessType": "ByProcessCount",
    "BatchUploadDataId": 0,
    "ProcessCount": 10,
    "UploadUser": "yujiechen@nine-yi.com"
}
```

**短時間重複執行防護（IsBatchTaskExecuted）**

- 執行記錄路徑：`BatchUpload.Storage.Floder / ExecutedTasks / yyyyMMdd / taskId / supplierId / BatchUpload_Code`
- 重複執行 expire：Config `BatchUpload.{TypeDef}.TaskExecuted.ExpireSeconds`，預設 **50 秒**
- 重複時發送通知 Email：Config `BatchUpload.ProcessDuplicate.EmailReceivers`（預設：erichsu@91app.com;reonachao@91app.com）

**廠商 Job 上限判斷**

- 超過上限（`BatchUpload.Supplier.JobLimit`，預設 10）→ 重新排隊
- delay：`BatchUpload.JobLimit.DelaySeconds`（預設 120 秒）

**SCMAPIV2 呼叫**

```bash
POST /v2/Promotion/ModifyPromotionSalePages
POST /v2/Promotion/ModifyPromotionProductSkuOuterIds
```

```csharp
var request = new ModifyPromotionProductSkuOuterIdsRequestEntity
{
    ShopId = this.BatchUploadEntity.BatchUpload_ShopId.Value,
    Id = entity.PromotionId,
    ModifyType = ModifyStatusMapping[entity.ModifyStatus].ToEnum<PromotionScopeModifyTypeDefEnum>(),
    ProductSkuOuterIds = entity.OuterIds.ToList()
};
```

**API 發生錯誤時**

- `BatchUploadMessage_StatusDef = ValidateFailed`
- `BatchUploadMessage_Note = errorMessage`
- `BatchUploadData_StatusDef = ProcessFailed`

<br>

---

## 附：Selling Point 欄位支援換行

批次匯入商品語系資料時，銷售重點（Selling Point）欄位需支援換行。

- PR：https://bitbucket.org/nineyi/nineyi.scm.nmqv2/pull-requests/14557/diff
- 相關 Service：`BatchUpdateSalePageMLService.CreateBatchUploadDataList`

```bash
匯出給點活動適用商品(料號)總筆數：4
匯出給點活動適用商品(料號)檔案名稱：BA2505201400001_6780__20250520140940.zip
更新作業執行狀態, status:Finish
新增錯誤清單明細, 筆數:0
尚有資料未處理, 發動 Task 繼續執行批次處理
taskData :{"BatchUploadId":11878,"ProcessType":"ByProcessCount","BatchUploadDataId":0,"ProcessCount":10,"UploadUser":"allenlin@nine-yi.com"}
End BatchUploadTaskProcess
::Finished
```

最後一輪因為撈不到 Ready 的資料，不再觸發子 Task。
