# Categories 批次作業類別文件

## 目錄
1. [其他交易資料匯入](#1-其他交易資料匯入)
2. [匯出活動商品](#2-匯出活動商品)
3. [匯出活動料號](#3-匯出活動料號)
4. [批次更新序號 / 料號](#4-批次更新序號--料號)

<br>

---

## 1. 其他交易資料匯入

### 1.1 Log 流程分析

BatchCrmOrderImportTask 交易資料匯入

<br>

會先將資料放在一個暫時的位置
CreateBatchUploadNMQTask
<br>

SFTP: /workspace/OMO/BatchCrmOrderImport/Tmp (.csv)

<br>

```
::Received Job : {"Id":"6ca6572a-2f57-4125-a57b-f76cde5d2d8d","SourceId":"3328812","JobName":"BatchCrmOrderImportTask","Data":"{\"BatchUploadId\":11447,\"ProcessType\":\"ByProcessCount\",\"BatchUploadDataId\":0,\"ProcessCount\":10,\"UploadUser\":\"nancyyeh@nine-yi.com\"}"}
::Create job : BatchCrmOrderImportTask
::Set task CurrentUICulture to => en-US
::Doing
Start BatchUploadTaskProcess
ProcessType:ByProcessCount
BatchUploadDataId:0
ProcessCount:10
批次上傳 User : BatchUpload
目前 job 數 7, 最多執行 job 限制 10
大量上傳批次 Code:BA2503170900001, Type:BatchCrmOrderImport
批次執行記錄目錄路徑\\SG-HK-QA1-SCM2\Storage\Tmp\BatchUpload\ExecutedTasks\20250317
產生執行記錄檔案:\\SG-HK-QA1-SCM2\Storage\Tmp\BatchUpload\ExecutedTasks\20250317\3328812_2_BA2503170900001
更新批次上傳狀態:InProcess
取得待處理批次資料, 筆數:21
執行作業, nmqTaskId:3328812, BatchUploadDataId:0
大量上傳批次處理(多筆)
儲存檔案
儲存路徑:\\SG-HK-QA1-SCM2\Storage\Tmp\BatchCrmOrderImport\2\91App_BatchCrmOrderImportTmp_2_11447.csv
取FTP
取 AWS FTP
cmd 檔案檢查傳入參數：/C D:\Softwares\curl-7.33.0-win64-ssl-sspi\curl.exe --insecure --tlsv1.2 --ftp-ssl --list-only -u qahk91user:@By2FQiwACf@zg sftp://tfsftp-01.qa.hk.91dev.tw/workspace/OMO/BatchCrmOrderImport/Tmp/ | findstr /i _2_
File Name:91App_BatchCrmOrderImportTmp_2_11446.csv
Process ExitCode:0
在FTP上處理中的檔案數量，shopId:2，fileProcessingCount:1
上傳檔案
cmd 上傳傳入參數：/C D:\Softwares\curl-7.33.0-win64-ssl-sspi\curl.exe --insecure --tlsv1.2 --ftp-ssl -T \\SG-HK-QA1-SCM2\Storage\Tmp\BatchCrmOrderImport\2\91App_BatchCrmOrderImportTmp_2_11447.csv -u qahk91user:@By2FQiwACf@zg sftp://tfsftp-01.qa.hk.91dev.tw/workspace/OMO/BatchCrmOrderImport/Tmp/91App_BatchCrmOrderImportTmp_2_11447.csv --ftp-create-dirs
filePath:cmd.exe, ExitCode:0
檔案上傳結束
更新作業執行狀態, status:ReadyToProcess
更新BatchUpload筆數 成功:21, 失敗:0
End BatchUploadTaskProcess
::Finished
^200 OK
["e7f73825-302b-4777-9e4c-99213ae0d457"] Sending HTTP request. Path: "/api/v1/tasks/6ca6572a-2f57-4125-a57b-f76cde5d2d8d:finish"
["e7f73825-302b-4777-9e4c-99213ae0d457"] Received HTTP response. StatusCode: OK, ElapsedMilliseconds: 19.2599
End processing request. RequestId: "e7f73825-302b-4777-9e4c-99213ae0d457",CallerMemberName: "FinishedAsync", ElapsedMilliseconds: 19.5818, RequestMetadata: ""e7f73825-302b-4777-9e4c-99213ae0d457""

- BA2503162100001
- BA2503170900001
```

<br>

接著等排程過來處理檔案

<br>

### 1.2 查 BatchUpload Table

- BatchuploadId : 11447
- TypeDef : BatchCrmOrderImport
- BatchUpload_StatusDef = FinishWithError
- ShopEtlFlowId_48
- BatchCrmOrderImportTask

<br>

### 1.3 查 BatchUploadData Table

有 21 筆資料

<br>

- BatchUploadData_StatusDef = ValidateFailed
- BatchUploadData_CreatedUser = BulkInsertBatchUploadData
- BatchUploadData_UpdatedUser = ShopEtlFlowId_48
- BatchUploadData_TypeDef = BatchCrmOrderImport

<br>

### 1.4 吃檔的 ETL 排程

ImportCrmOrderData FTP產品化串接-每日線下訂單匯入

<br>

- ETL (cron 4-59/10 * * * * )(\Order\OMO_Order_ETL_BatchCrmOrderImport, OMO_Order_CRM_PS_ImportCrmOrderData)
    - \Order\OMO_Order_ETL_BatchCrmOrderImport
    - SFTP: /workspace/OMO/BatchCrmOrderImport/2/ (.zip)
    - D:\Batch\EtlConsole\OSMPlusBatchRunFlowById\NineYi.OsmPlus.Batch.RunFlowById.exe
    - --flowname BatchCrmOrderImport
    - CrmTempDB Job / Task
    - 基本資訊 (ShopEtlSlow_Id = 48 , BatchCrmOrderImport, 交易資料匯入)

<br>

ImportCrmOrderData FTP產品化串接-每日線下訂單匯入

<br>

- JobName : OMO_Order_CRM_PS_ImportCrmOrderData
- 確認 : shopId / to_91APP  ==> shopId / to_91APP/CrmBackup 表示成功
- DB : CrmTempDB Job / Task

<br>

![alt text](./image-2.png)

<br>

機器與確認位置

<br>

TWQA -  TYO-QA-CRM02

<br>

![alt text](./image-1.png)

<br>

HKQA

<br>

E:/Files/OsmConsole/0/20250317/SalesOrder/100_BatchCrmOrderImport

<br>

異常原因

<br>

檔案未被搬移至 /workspace/OMO/ BatchCrmOrderImport /2

<br>

![alt text](./image.png)

<br>

相關語法

<br>

```sql
use EtlDB

select *
from EtlFlowTask(nolock)
where EtlFlowTask_ShopEtlFlowId = 48
and EtlFlowTask_Id = 7888908
and EtlFlowTask_UpdatedDateTime between '2025-03-17' and '2025-03-18'
order by EtlFlowTask_CreatedDateTime desc

select *
from ShopEtlFlowStep(nolock)
where ShopEtlFlowStep_ValidFlag = 1
and ShopEtlFlowStep_ShopEtlFlowId = 48
order by ShopEtlFlowStep_Id 

select *
from EtlFlowTaskSlave(nolock)
where EtlFlowTaskSlave_ValidFlag = 1
and EtlFlowTaskSlave_EtlFlowTaskId = 7888908
--and EtlFlowTaskSlave_UpdatedDateTime  between '2025-03-17' and '2025-03-18'
order by EtlFlowTaskSlave_UpdatedDateTime desc
```

<br>

### 1.5 QA FTPClient.CurlPathV2 登進去資訊

MachineConfig / Backend / QA300

<br>

```xml
<add key="QA.AWS.CrmSFTP.FTPServer" value="sftp://tfsftp-01.qa.hk.91dev.tw/"/>
<add key="QA.AWS.CrmSFTP.FTPUserName" value="qahk91user"/>
<add key="QA.AWS.CrmSFTP.FTPPassword" value="@By2FQiwACf@zg"/>
```

<br>

```csharp
var ftpFolder = string.Format("workspace/OMO/{0}/Tmp", this._ftpFolderName);
_ftpFolderName = "BatchCrmOrderImport";

string sourceFilePath = this.Save(batchUploadDataList, out shopId);
```

<br>

### 1.6 怎麼從 UI 看是否匯入

![alt text](./image-3.png)

<br>

---

## 2. 匯出活動商品

![alt text](./image-4.png)

<br>

**API**

<br>

https://sms.qa1.hk.91dev.tw/api/PromotionEngine/CreateBatchExportDataTask

<br>

```json
{
    "BatchUploadType": 166,
    "BatchUploadExecuteTaskType": 149,
    "ExportCondition": {
        "PromotionEngineId": 6045,
        "ShopId": 2
    },
    "Password": "",
    "ShopId": 2
}
```

<br>

- BatchUploadType: 166 => ExportRewardPromotionSalePage
- BatchUploadExecuteTaskType: 149 => ExportRewardPromotionSalePageTask

<br>

**流程**

<br>

塞 BatchUpload Table -> CreateBatchUploadNMQTask (BatchUpload)

<br>

```json
{
   "BatchUploadId":11269,
   "SupplierId":2,
   "UploadType":"ExportRewardPromotionSalePage",
   "UploadUser":"jackyhu@nine-yi.com",
   "FilePassword":"",
   "ExportDataCondition":{
      "ShopId":2,
      "PromotionEngineId":5801
   },
   "Country":null
}
```

<br>

ExportRewardPromotionSalePageService.cs

<br>

-> SCMAPIV2

<br>

/v2/Promotion/GetPromotionSalePages

<br>

```csharp
public class GetPromotionSalePagesRequestEntity
{
    /// <summary>
    /// 商店代碼
    /// </summary>
    public long ShopId { get; set; }

    /// <summary>
    /// 活動序號
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 分區標籤
    /// </summary>
    public string Tag { get; set; }
}
```

<br>

-> PromotionWeb

<br>

/api/promotion-rules/salepage-list

<br>

- ShopId
- Id
- Tag

<br>

by 類型取得商品頁並回復類型

<br>

---

## 3. 匯出活動料號

ExportRewardPromotionOuterId = 174

<br>

**DB 設定**

<br>

- JobId : 487
- name : ExportRewardPromotionOuterIdTask

<br>

**PromotionAPI**

<br>

productsku-outerid-list

<br>

ExportRewardPromotionOuterIdService.cs

<br>

---

## 4. 批次更新序號 / 料號

### 4.1 OSM

#### 4.1.1 檔案

**手動上傳檔案**

<br>

https://sms.qa1.hk.91dev.tw/Api/UploadFile

<br>

(FormData)

<br>

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

<br>

(this._tmpFolder, type, uniqueKey

<br>

c file temp, 2號店 , f9d48752-e06c-48e7-a021-f78e42d983fa guid!!

<br>

製作測試 Excel

<br>

- promotionid : 5978
- GUID

<br>

**正式環境 Filer by type**

<br>

![alt text](./image-5.png)

<br>

**範例檔**

<br>

https://sms.qa1.hk.91dev.tw/Docs/ModifyRewardPromotionSalePage/Batch%20update%20products%20in%20promotions%20template.xlsx

<br>

#### 4.1.2 關鍵字

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

**對應到批次類型**

<br>

```csharp
case BatchUploadTypeDefEnum.
```

<br>

**對應 Service**

<br>

BatchModifyPromotionSalePageService

<br>

**行為 (繼承自 AbstractCreateBatchTaskBaseService)**

<br>

```csharp
batchService.ProcessCreateBatchTask(entity);
```

<br>

**批次類型**

<br>

BatchModifyPromotionOuterId

<br>

**Service**

<br>

BatchModifyPromotionOuterIdService

<br>

**行為 (繼承自 AbstractCreateBatchTaskBaseService)**

<br>

```csharp
batchService.ProcessCreateBatchTask(entity);
```

<br>

#### 4.1.3 API

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

#### 4.1.4 主流程

**會分類跑對應的 Process**

<br>

```csharp
case BatchUploadTypeDefEnum.BatchModifyPromotionOuterId
```

<br>

**第一道驗證**

<br>

```csharp
this.GetFilePath();
this.VerifyFile();
```

<br>

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

**Load Excel 檔**

<br>

使用 LinqToExcel 套件的 ExcelQueryFactory 來執行 Excel 讀取操作

<br>

**Step 1. 建立 ExcelQueryFactory**

<br>

```csharp
// 使用 LinqToExcel 套件建立 excelQueryFactory，並指定要讀取的 fileName
ExcelQueryFactory excelQueryFactory = new ExcelQueryFactory(fileName);

// 設定 TrimSpaces 為 Both，以自動修剪字串前後的空白
Type typeFromHandle = typeof(ExcelQueryFactory);

// typeFromHandle 用於後續透過反射取得方法
excelQueryFactory.TrimSpaces = TrimSpacesType.Both;
```

<br>

**Step 2. 取得對應介面與對應定義 (Mapping Definition)**

<br>

```csharp
Type typeFromHandle2 = typeof(IColumnMapping<>);
Type type = typeFromHandle2.MakeGenericType(dataType);
object obj = LifetimeScope.Resolve(type);
```

<br>

IColumnMapping<> 是一個泛型介面，程式透過 MakeGenericType 產生對應 dataType 的型別。使用 LifetimeScope.Resolve(type) 來解析出對應的物件，這通常是 Dependency Injection (DI) 容器的操作。

<br>

**Step 3. 建立 MappingDefinition**

<br>

```csharp
Type typeFromHandle3 = typeof(MappingDefinition<>);
Type type2 = typeFromHandle3.MakeGenericType(dataType);
object obj2 = Activator.CreateInstance(type2);

// 建立出針對 dataType 的 MappingDefinition，用來描述 Excel 欄位與類別屬性的對應關係
```

<br>

**Step 4. 執行映射邏輯**

<br>

```csharp
MethodInfo method = type.GetMethod("MapExcelToEntity");
method.Invoke(obj, new object[1] { obj2 });
```

<br>

**Step 5. 取得對應資訊**

<br>

```csharp
PropertyInfo property = type2.GetProperty("Mappings");
object value = property.GetValue(obj2);
// 取得 Mappings 屬性：
// Mappings 是 obj2 中的欄位對應資訊，通常是 Dictionary<string, string> 格式，Key 是 Excel 欄位，Value 是 .NET 屬性
```

<br>

**Step 6. 建立映射至 ExcelQueryFactory**

<br>

```csharp
foreach (object item in value as IEnumerable)
{
    Type type3 = item.GetType();
    object value2 = type3.GetProperty("Key").GetValue(item);
    object value3 = type3.GetProperty("Value").GetValue(item);
    
    MethodInfo methodInfo = (from i in typeFromHandle.GetMethods()
                             where i.Name == "AddMapping" && i.GetGenericArguments().Count() == 1 && i.GetParameters().Count() == 2
                             select i).First();
    methodInfo = methodInfo.MakeGenericMethod(dataType);
    methodInfo.Invoke(excelQueryFactory, new object[2] { value2, value3 });
}

// 逐一從 Mappings 中取出每個項目 (Excel 欄位與屬性對應)
// 使用反射找到 AddMapping 方法，並指定 dataType
// 呼叫 AddMapping(value2, value3)：
// value2：Excel 欄位名稱
// value3：.NET 類別的屬性名稱
```

<br>

**Step 7. 呼叫 Worksheet 取得資料**

<br>

```csharp
MethodInfo methodInfo2 = (from i in typeFromHandle.GetMethods()
                          where i.Name == "Worksheet" && i.GetGenericArguments().Count() == 1 && i.GetParameters().Count() == 1 && i.GetParameters().First().ParameterType == typeof(string)
                          select i).First();
MethodInfo methodInfo3 = methodInfo2.MakeGenericMethod(dataType);
object obj3 = methodInfo3.Invoke(excelQueryFactory, new object[1] { worksheetName });

// 透過反射取得泛型的 Worksheet<T> 方法
// 呼叫 Worksheet<T>(worksheetName)，讀取指定工作表的資料
```

<br>

**Step 8. 回傳**

<br>

```csharp
return obj3 as IEnumerable<object>;
// 將讀取到的資料轉型為 IEnumerable<object>，以便後續使用
```

<br>

#### 4.1.5 CreateBatchTask (產生 Job 入口)

**方法層次**

<br>

- CreateBatchUploadNMQTask (墊一層)
    - CreateBatchUploadNMQTaskInner (又墊一層)
        - CreateBatchUploadEntity **(狀態 : WaitingToLoadData)**
        - 產一個 BatchuploadCode = **this._dbContext.Csp_GetSequencesCode("BatchUpload_Code")**
        - **塞 batchUpload Table**
        - **CreateBatchUploadNMQTask** : BatchUpload

<br>

**Batch Upload 資料結構**

<br>

| 欄位名稱 | 資料 |
|---------|------|
| BatchUpload_Id | 10457 |
| BatchUpload_SupplierId | 2 |
| BatchUpload_TypeDef | ModifyRewardPromotionSalePage |
| BatchUpload_FileName | 未命名的試算表 (52).xlsx |
| BatchUpload_StatusDef | Finish |
| BatchUpload_Code | BA2405281000001 |
| BatchUpload_TotalCount | 2 |
| BatchUpload_SuccessCount | 2 |
| BatchUpload_FailCount | 0 |
| BatchUpload_UploadDateTime | 2024-05-28 10:06:22.483 |
| BatchUpload_StartDateTime | 2024-05-28 10:06:43.530 |
| BatchUpload_FinishDateTime | 2024-05-28 10:21:03.003 |
| BatchUpload_Note | NULL |
| BatchUpload_UploadGuid | ad7df7b4-3da5-44c1-8c2a-7445e9c24e90 |
| BatchUpload_HasDetail | 0 |
| BatchUpload_FailDownloadPath | NULL |
| BatchUpload_EstimateFinishMinutes | 5 |
| BatchUpload_AverageSecondsPerRecord | 440 |
| BatchUpload_CreatedDateTime | 2024-05-28 10:06:22.497 |
| BatchUpload_CreatedUser | nancyyeh@nine-yi.com |
| BatchUpload_UpdatedTimes | 5 |
| BatchUpload_UpdatedDateTime | 2024-05-28 10:21:03.257 |
| BatchUpload_UpdatedUser | BatchUpload |
| BatchUpload_ValidFlag | 1 |
| BatchUpload_Rowversion | 0x00000003C315068C |
| BatchUpload_ExecuteTaskType | BatchModifyPromotionSalePageTask |
| BatchUpload_ExportFileDownloadPath | NULL |
| BatchUpload_ShopId | 2 |
| BatchUpload_ScheduledDateTime | 2024-05-28 10:20:00.000 |

<br>