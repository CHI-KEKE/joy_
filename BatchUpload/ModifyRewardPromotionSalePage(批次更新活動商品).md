
## OSM 上傳檔案


#### API

https://sms.qa1.hk.91dev.tw/Api/UploadFile

(FormData)

```
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


(this._tmpFolder, type, uniqueKey

c file temp, 2號店 , f9d48752-e06c-48e7-a021-f78e42d983fa guid!!


製作測試 Excel

- promotionid : 5978
- GUID


#### 範例檔

https://sms.qa1.hk.91dev.tw/Docs/ModifyRewardPromotionSalePage/Batch%20update%20products%20in%20promotions%20template.xlsx

<br>


## OSM API


#### 關鍵字

- BatchUploadType
- BatchModifyPromotionOuterId
- ModifyRewardPromotionSalePage
- ModifyPromotionSalePagesRequestEntity
- BatchModifyPromotionOuterIdService
- BatchModifyPromotionOuterIdDataEntityValidator
- BatchModifyPromotionOuterIdTaskProcess
- BatchUploadTypeDefEnum
- BatchUploadExecuteTaskTypeEnum

<br>

#### 對應到批次類型

```csharp
case BatchUploadTypeDefEnum.
```

<br>

#### 對應 Service

BatchModifyPromotionSalePageService

繼承自 AbstractCreateBatchTaskBaseService

```csharp
batchService.ProcessCreateBatchTask(entity);
```

<br>

#### CreateBatchTask

https://sms.qa1.hk.91dev.tw/Api/BatchUpload/CreateBatchTask

<br>

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

<br>

#### OSM 第一道驗證

<br>

```csharp
this.GetFilePath();
this.VerifyFile();
```

- .xlsx
- tw : sheetName = 批次更新活動商品範例檔
- others : Update
- BatchUpload.{this.GetBatchUploadType()}.MaxCount 預設 500
- BatchUpload.{this.GetBatchUploadType()}.SoonestScheduleOffsetInMinutes 預設 30
- HasMappingError
- PromotionId / ScheduleTime / SalePageIds / ModifyStatus 有空
- PromotionId 格式
- IsSalePageIdsValid
- ValidModifyStatuses

<br>

#### Load Excel 檔 建立 mappingProfile

```csharp
using NineYi.Common.Utility.Upload.Mappers;
using NineYi.Sms.BL.BE.BatchUploads;
using NineYi.Sms.Utilities.Helpers;

namespace NineYi.Sms.Mappers.BatchUploads
{
    /// <summary>
    /// 批次新增商品標籤
    /// </summary>
    public class BatchModifyPromotionSalePageMappingProfile : IColumnMapping<BatchModifyPromotionOuterIdEntity>, IColumnMapping<ModifyRewardPromotionSalePageEntity>
    {
        #region Implementation of IColumnMapping<BatchModifyPromotionSalePageExcelEntity>
        /// <summary>
        /// MapExcelToEntity
        /// </summary>
        /// <param name="excelFile">MappingDefinition</param>
        public void MapExcelToEntity(MappingDefinition<BatchModifyPromotionOuterIdEntity> excelFile)
        {
            //// 語系 Hard Code
            //// 除台灣以外, 其他國家一律預設英文
            switch (SettingHelper.DefaultCountry)
            {
                case "TW":
                    excelFile.Add(a => a.ScheduleTime, "期望生效時間");
                    excelFile.Add(a => a.PromotionId, "活動序號");
                    excelFile.Add(a => a.SalePageIds, "商品頁序號");
                    excelFile.Add(a => a.ModifyStatus, "異動狀態");
                    break;

                default:
                    excelFile.Add(a => a.ScheduleTime, "Execution Time");
                    excelFile.Add(a => a.PromotionId, "Promotion ID");
                    excelFile.Add(a => a.SalePageIds, "Product Page ID");
                    excelFile.Add(a => a.ModifyStatus, "Update");
                    break;
            }
        }
        #endregion
        
        /// <summary>
        /// MapExcelToEntity
        /// </summary>
        /// <param name="excelFile">MappingDefinition</param>
        public void MapExcelToEntity(MappingDefinition<ModifyRewardPromotionSalePageEntity> excelFile)
        {
            //// 語系 Hard Code
            //// 除台灣以外, 其他國家一律預設英文
            switch (SettingHelper.DefaultCountry)
            {
                case "TW":
                    excelFile.Add(a => a.ScheduleTime, "期望生效時間");
                    excelFile.Add(a => a.PromotionId, "活動序號");
                    excelFile.Add(a => a.SalePageIds, "商品頁序號");
                    excelFile.Add(a => a.ModifyStatus, "異動狀態");
                    break;

                default:
                    excelFile.Add(a => a.ScheduleTime, "Effective Time");
                    excelFile.Add(a => a.PromotionId, "Promotion ID");
                    excelFile.Add(a => a.SalePageIds, "Product Page ID");
                    excelFile.Add(a => a.ModifyStatus, "Action");
                    break;
            }
        }
    }
}
```

<br>

#### CreateBatchTask (產生 Job 入口)

## 主 BatchUploadProcess NMQ

```json
{
   "BatchUploadId":11229,
   "SupplierId":2,
   "UploadType":"ModifyRewardPromotionSalePage",
   "UploadUser":"allenlin@nine-yi.com",
   "FilePassword":"",
   "ExportDataCondition":null,
   "Country":"HK"
}

{"BatchUploadId":11519,"SupplierId":2,"UploadType":"BatchModifyPromotionOuterId","UploadUser":"ellygong@nine-yi.com","FilePassword":"","ExportDataCondition":null,"Country":"HK"}

```

<br>

#### LoadData

(`BatchModifyPromotionSalePageService`)
(`BatchModifyPromotionOuterId`)

初始化行為 BatchUploadService
位置在 BatchUploadProcess.InitContainerBuilder
```csharp
.RegisterBatchUploadService<BatchModifyPromotionSalePageService>(BatchUploadTypeEnum.ModifyRewardPromotionSalePage) //// 批次更新點數折扣活動可使用商品
```

<br>

#### LoadExcel

使用 LinqToExcel 套件的 ExcelQueryFactory 來執行 Excel 讀取操作


- Step 1. 建立 ExcelQueryFactory
- Step 2. 取得對應介面與對應定義 (Mapping Definition)
- Step 3. 建立 MappingDefinition
- Step 4. 執行映射邏輯
- Step 5. 取得對應資訊
- Step 6. 建立映射至 ExcelQueryFactory
- Step 7. 呼叫 Worksheet 取得資料
- Step 8. 回傳

appSetting : BatchUpload.Storage.Floder
預設 : C:\Temp\UploadData
```
C:\Temp\UploadData / supplierId / BatchUpload_UploadGuid
```

驗證檔案是否存在 (System.IO.File.Exists)

取 excel 欄位資料

```csharp
ScheduleTime = x.ScheduleTime,
PromotionId = int.Parse(x.PromotionId),
ModifyStatus = x.ModifyStatus,
SalePageIds = x.GetSalePageIds()
```

by excelData 包成 `List<BatchUploadDataWrapperEntities>`

- BatchUploadData(大量上傳資料, BatchUploadDataEntity)

```csharp
BatchUploadDataEntity
{
    BatchUploadData_Id
    BatchUploadData_BatchUploadId
    BatchUploadData_SupplierId
    BatchUploadData_TypeDef
    BatchUploadData_TypeDefDesc
    ...
}
```

- DataEntity(資料內容，用於與 BatchUploadData 一對一的情境)
- DataEntityList(資料內容 Entity List)

每一列 excel 長一個 wrapper, 每一個 wrapper 有一組 BatchUploadData, DataEntityList, DataEntity

<br>

#### DoValidate (二次驗證)

1. by PromotionId 分群
2. 集合驗證


BatchModifyPromotionOuterIdDataEntityValidator
UploadFailedMessageList
DoValidate

- FlentValidation : using ValidationEntity = NineYi.WebStore.Backend.BE.BatchUpload.GroupedBatchUploadDataValidationEntity<int, NineYi.WebStore.Backend.BE.BatchUpload.BatchModifyPromotionOuterIdDataEntity>;
- BatchModifyPromotionOuterIdDataEntityValidator
- 驗重複料號
- 驗證活動序號
- 驗證料號字元
- 驗證異動狀態

<br>

#### 為什麼要包 wrapper

一個 wrapper 是一個 promotion 維度
this.BatchUploadDataList = 一堆 wrapper
wrapper 主要分兩個節點

- BatchUploadData = BatchUploadDataEntity, 紀載 title, data, status…等等
- DataEntityList = 一組 promotion 的資料

看起來一個 batchuploadData 就是一筆 promotion 維度的資料, 也就是 excel 某一列

<br>

#### Insert BatchUploadData Table 並建立子 NMQ

取得 this.BatchUploadDataList 來自 List<BatchUploadDataWrapperEntity<BatchModifyPromotionSalePageDataEntity>>

<br>

#### 依 Supplier 決定是否 Bulk Insert BatchUploadData

- shopId = 0
- groupname = BatchUpload
- key = BulkInsert.Enable.SupplierIds

**開關有開 (var bulkLimit = 10000)**

```csharp
this.BatchUploadRepository.BulkInsertBatchUploadData(batchUploadDataList);
```

**開關沒開 (WebStoreDB csp_InsertBatchUploadData)**

<br>

```csharp
this.BatchUploadRepository.CreateBatchUploadData(batchUploadDataList)
```

**塞入 BatchUploadData**

BatchUpload_TotalCount = 共幾列

- dataList.Count == 0, 更新批次上傳狀態 : Finish
- dataList.Count > 0, 更新批次上傳狀態 : WaitingToProces
- 檢查 dataList 有 BatchUploadData_StatusDef == ValidateFailed 更新狀態

<br>

#### 組織 taskNMQDataEntity

<br>

```csharp
taskNMQDataEntity.BatchUploadId = this.BatchUploadEntity.BatchUpload_Id;
taskNMQDataEntity.ProcessType = BatchUploadTaskProcessTypeEnum.ByProcessCount.ToString();  //// 多筆, 依定義筆數執行 Task
string typeProcessCountSetting = string.Format("BatchUpload.{0}.TaskProcessCount", this.BatchUploadEntity.BatchUpload_TypeDef);
taskNMQDataEntity.ProcessCount = Convert.ToInt32(AppSetting.GetAppSetting(typeProcessCountSetting, "10"));
taskNMQDataEntity.UploadUser = nmqDataEntity.UploadUser;
```

- 本案為 : BatchModifyPromotionSalePageTask (仍有繼承 BatchUploadTask)
- 一般為 : BatchUploadTask

<br>

## BatchUploadTaskProcess(子 Jobs)

每 10 為一組 產生一個 BatchModifyPromotionOuterIdTask / BatchModifyPromotionSalePageTask

```json
{
   "BatchUploadId":11221,
   "ProcessType":"ByProcessCount",
   "BatchUploadDataId":0,
   "ProcessCount":10,
   "UploadUser":"yujiechen@nine-yi.com"
}

{"BatchUploadId":11519,"ProcessType":"ByProcessCount","BatchUploadDataId":0,"ProcessCount":10,"UploadUser":"ellygong@nine-yi.com"}
{"BatchUploadId":11519,"ProcessType":"ByProcessCount","BatchUploadDataId":0,"ProcessCount":10,"UploadUser":"ellygong@nine-yi.com"}
{"BatchUploadId":11519,"ProcessType":"ByProcessCount","BatchUploadDataId":0,"ProcessCount":10,"UploadUser":"ellygong@nine-yi.com"}
{"BatchUploadId":11519,"ProcessType":"ByProcessCount","BatchUploadDataId":0,"ProcessCount":10,"UploadUser":"ellygong@nine-yi.com"}
{"BatchUploadId":11519,"ProcessType":"ByProcessCount","BatchUploadDataId":0,"ProcessCount":10,"UploadUser":"ellygong@nine-yi.com"}
```

<br>

**DB 設定**

商品頁
JobId : 334

<br>

料號
486

位置 : NineYi.SCM.Frontend.NMQV2.BatchUpload.BatchModifyPromotionSalePageTaskProcess

<br>

#### 批次異動商品頁子檔 TASK 狀態

<br>

```sql
USE NMQV2DB
SELECT *
FROM Task(NOLOCK)
WHERE Task_ValidFlag = 1
AND Task_JobId = 334
ORDER BY Task_CreatedDatetime DESC
```

<br>

#### 確認 BatchUpload Data

- 尋找 BatchUpload ( `BatchUpload` DB)
- 狀態是否 BatchUploadStatusEnum.WaitingToProcess,BatchUploadStatusEnum.InProcess

<br>

#### 判斷廠商是否超過最多執行 job 限制

key : BatchUpload.Supplier.JobLimit

預設 : 10
撈取 BatchUpload 的 BatchUpload_StatusDef = "InProcess"
==> 發動 Task 重新排隊 再產一次 BatchUploadTask

delay 多久
key : BatchUpload.JobLimit.DelaySeconds
預設 : 120

<br>

#### IsBatchTaskExecuted (短時間內重複執行 直接中斷)**

檢查是否 IsBatchTaskExecuted

批次執行記錄目錄 : config : BatchUpload.Storage.Floder 預設 C:\Temp\UploadData

```
C:\Temp\UploadData \ ExecutedTasks \ yyyyMMdd \ taskId \ BatchUpload_SupplierId \ BatchUpload_Code
```

若找到這個檔案 表示可能執行過

taskRedoExpireSeconds 由 Config : BatchUpload.{BatchUpload_TypeDef}.TaskExecuted.ExpireSeconds 預設 50秒

若最新執行時間加上redo時間 > 現在
視為執行過 且 尚在逾時期間內
重複執行註記
執行批次大量上傳重複處理檢查及中斷通知
config : BatchUpload.ProcessDuplicate.EmailReceivers
預設 : erichsu@91app.com;reonachao@91app.com
csp : csp_NoticeBatchProcessInterrupted
已逾時可重做, 將舊執行紀錄檔壓改名
if (isExecuted == false) 產生執行記錄檔案
短時間內重複執行 直接中斷

<br>

## SCMAPIV2

/v2/Promotion/ModifyPromotionSalePages
/v2/Promotion/ModifyPromotionProductSkuOuterIds

```csharp
var request = new ModifyPromotionProductSkuOuterIdsRequestEntity
{
    ShopId = this.BatchUploadEntity.BatchUpload_ShopId.Value,
    Id = entity.PromotionId,
    ModifyType = ModifyStatusMapping[entity.ModifyStatus].ToEnum<PromotionScopeModifyTypeDefEnum>(),
    ProductSkuOuterIds = entity.OuterIds.ToList()
};
```

若發生錯誤會 InsertUploadFailedMessageList BatchUploadMessage Table

#### API 錯誤時 可以查看的資訊

<br>

- BatchUploadMessage_StatusDef = ValidateFailed
- BatchUploadMessage_Note = erroMessage
- BatchUploadData_StatusDef = ProcessFailed

<br>

#### API 錯誤釐清案例

批次更新料號，要上傳 10 萬個，只上傳了6萬多就終止問題

- code: BA2503211400003
- Id: 11519
- 時間: 2025/03/21 14:40

**NMQ 資料**

**BatchUploadData 詳細資訊**

| 欄位名稱 | 資料值 |
|---------|--------|
| BatchUploadData_Id | 1835340 |
| BatchUploadData_BatchUploadId | 11519 |
| BatchUploadData_SupplierId | 2 |
| BatchUploadData_TypeDef | BatchModifyPromotionOuterId |
| BatchUploadData_StatusDef | ProcessFailed |
| BatchUploadData_Title | 2 |
| BatchUploadData_CreatedDateTime | 2025-03-21 14:09:14.867 |
| BatchUploadData_CreatedUser | BulkInsertBatchUploadData |
| BatchUploadData_UpdatedTimes | 1 |
| BatchUploadData_UpdatedUser | BatchUpload |
| BatchUploadData_UpdatedDateTime | 2025-03-21 14:46:46.687 |
| BatchUploadData_ValidFlag | 1 |

**BatchUploadData_Data 內容**

<br>

```json
{
    "ScheduleTime": "3/21/2025 2:40:00 PM",
    "PromotionId": 6298,
    "OuterIds": [
        "Z7BkR", "tfu8R", "pQqQC", "UeXIq", "t4HZD", "nPa96", "pqhOn", "YArbF", 
        "ZbEzd", "xt6dz", "ecBD4", "8MiN6", "fpvED", "MLolr", "pVA1y", "QIjft", 
        "q0Bkr", "M5apB", "7bQDB", "B27px", "oogCE", "rnNHp", "3XSkn", "LfVl3", 
        "nykRp", "mm5OS", "GxjdC", "Za1CZ", "q9E7e", "5QDt5", "ivyW4", "VhgrJ", 
        "OMyO9", "adSdJ", "3qpQv", "uR70C", "CqVkk", "dLw1W", "nuYHO", "9CLLO"
        // ... 更多料號
    ],
    "ModifyStatus": "Add"
}
```

<br>

**錯誤回應 (促購後台)**

**Response - traceId:**
```json
{
    "errorCode": "InternalServerError",
    "message": "SqlException",
    "data": "Transaction (Process ID 196) was deadlocked on lock resources with another process and has been chosen as the deadlock victim. Rerun the transaction."
}
```

<br>

**問題分析**

- 發生 SQL 死鎖 (Deadlock)
- 交易過程中與其他處理序產生鎖定資源衝突
- 系統選擇該交易作為死鎖受害者並中止執行
- 需要重新執行交易

<br>

#### 4.5.2 API 回 null!? 導致錯誤明細顯示異常

https://bitbucket.org/nineyi/nineyi.scm.nmqv2/pull-requests/15870/overview

- API 回傳 null 導致錯誤明細顯示異常
- 影響錯誤訊息的正確呈現
- 需要透過程式碼修正處理 null 回應的情況