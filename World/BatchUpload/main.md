# BatchUpload 主要文件

## 目錄
1. [SuccessCount 變動](#1-successcount-變動)
2. [依次執行子 Process 直到結束的流程](#2-依次執行子-process-直到結束的流程)
3. [新增 Batchupload NMQ 語法](#3-新增-batchupload-nmq-語法)
4. [批次 Quota by 商店](#4-批次-quota-by-商店)
5. [整體資料邏輯](#5-整體資料邏輯)
6. [批次作業進度查詢 - 作業類型 - 下拉選單](#6-批次作業進度查詢---作業類型---下拉選單)
7. [Enum / TypeDef](#7-enum--typedef)
8. [批次匯入商品語系資料的銷售重點 (Selling Point) 欄位支援換行](#8-批次匯入商品語系資料的銷售重點-selling-point-欄位支援換行)
9. [下載錯誤明細](#9-下載錯誤明細)
10. [上限筆數驗證](#10-上限筆數驗證)

<br>

---

## 1. SuccessCount 變動

實際上每個子 Process 會更新一次 TotalCount / SuccessCount，因此要看各自實作

<br>

---

## 2. 依次執行子 Process 直到結束的流程

### Log 分析

<br>

```
匯出給點活動適用商品(料號) xlsx 檔案成功
刪除檔案：\\SG-HK-QA1-SCM2\Storage\Tmp\ExportRewardPromotionOuterId\BA2505201400001\BA2505201400001_6780__20250520140940.xlsx
匯出給點活動適用商品(料號)總筆數：4
匯出給點活動適用商品(料號)檔案名稱：BA2505201400001_6780__20250520140940.zip
匯出給點活動適用商品(料號)下載路徑：\\SG-HK-QA1-SCM2\Storage\Tmp\ExportRewardPromotionOuterId\BA2505201400001\BA2505201400001_6780__20250520140940.zip
更新作業執行狀態, status:Finish
新增錯誤清單明細, 筆數:0
尚有資料未處理, 發動 Task 繼續執行批次處理
taskData :{"BatchUploadId":11878,"ProcessType":"ByProcessCount","BatchUploadDataId":0,"ProcessCount":10,"UploadUser":"allenlin@nine-yi.com"}
Create Task, Data: {"BatchUploadId":11878,"ProcessType":"ByProcessCount","BatchUploadDataId":0,"ProcessCount":10,"UploadUser":"allenlin@nine-yi.com"}, Job: ExportRewardPromotionOuterIdTask
End BatchUploadTaskProcess
::Finished
```

<br>

最後一輪因為發現沒有 Ready 的資料可以撈

<br>

```
上傳失敗筆數:0
上傳成功筆數:1
```

<br>

---

## 3. 新增 Batchupload NMQ 語法

https://bitbucket.org/nineyi/nineyi.database.operation/pull-requests/21347/overview

<br>

---

## 4. 批次 Quota by 商店

### 4.1 ERP 有小工具釋放 ERP上限

https://erp.qa.91dev.tw/v2/DynamicReport/List?report=BatchUploadRepository-QueryOrUpdateBatchUpload

<br>

### 4.2 Nancy 10230 匯入其他交易資料後一直卡在待執行

批次作業編號：BA2503191400002

<br>

**BatchUpload 取得相關資訊**

<br>

```
BatchUpload_Id = 45518
BatchUpload_SupplierId = 1111
BatchUpload_TypeDef = BatchCrmMemberImport
BatchUpload_FileName = 有品編的會員.xlsx
```

<br>

**查 nmq**

<br>

```sql
use NMQV2DB

select *
from Task(nolock)
where Task_ValidFlag = 1
and Task_Data like '%45518%'
order by Task_CreatedDatetime desc
```

<br>

BatchCrmMemberImportTask ==> JobId = 290

<br>

如果怕查太久
1. 用jobId
2. 用 datetime
3. 不行只能用 taskData惹吧

<br>

**看log**

<br>

```
【2025-03-19T11:18:09.4189370Z】【TYO-QA-NMQ21】【e3a22aa5-f555-4c96-8248-9db8e92d54aa】【29947386】 目前 job 數 10, 最多執行 job 限制 10
【2025-03-19T11:18:09.4190836Z】【TYO-QA-NMQ21】【e3a22aa5-f555-4c96-8248-9db8e92d54aa】【29947386】 目前 job 數 10, 超過最多執行 job 限制 10, 發動 Task 重新排隊
```

<br>

看 supplier 確實有10筆卡住

<br>

```sql
use WebStoreDB
select *
from BatchUpload(nolock)
where BatchUpload_ValidFlag = 1
and BatchUpload_SupplierId = 1111
and BatchUpload_StatusDef = 'InProcess'
--and BatchUpload_Id = 44856
```

<br>

**壓掉**

<br>

```sql
use WebStoreDB
select *
from BatchUpload(nolock)
where BatchUpload_ValidFlag = 1
and BatchUpload_SupplierId = 1111
and BatchUpload_StatusDef = 'InProcess'
--and BatchUpload_Id = 44856

update BatchUpload
set BatchUpload_ValidFlag = 0
where BatchUpload_ValidFlag = 1
and BatchUpload_SupplierId = 1111
and BatchUpload_StatusDef = 'InProcess'
and BatchUpload_Id = 44856

select *
from BatchUpload(nolock)
where BatchUpload_ValidFlag = 1
and BatchUpload_SupplierId = 1111
and BatchUpload_StatusDef = 'InProcess'
--and BatchUpload_Id = 44856
```

<br>

**其他資訊**

<br>

```sql
select *
from BatchUploadMessage(nolock)
where BatchUploadMessage_ValidFlag = 1
and BatchUploadMessage_BatchUploadId = 45518

select *
from BatchUploadData(nolock)
where BatchUploadData_ValidFlag = 1
and BatchUploadData_BatchUploadId = 45518
```

<br>

### 4.3 檢查上限的程式碼

```csharp
public List<long> GetBatchUploadProcesseIdList(int supplierId)
{
    var transactionOptions = new TransactionOptions();
    transactionOptions.IsolationLevel = System.Transactions.IsolationLevel.ReadUncommitted;

    using (var transactionScope = new TransactionScope(TransactionScopeOption.Required, transactionOptions))
    {
        using (WebStoreDBEntitiesV2 context = this.LifetimeScope.Resolve<WebStoreDBEntitiesV2>())
        {
            var list = (from batchUpload in context.BatchUpload.Valids()
                        where batchUpload.BatchUpload_SupplierId == supplierId && batchUpload.BatchUpload_StatusDef == "InProcess"
                        select batchUpload).ToList();

            var result = new List<long>();
            foreach (var item in list)
            {
                result.Add(item.BatchUpload_Id);
            }

            return result;
        }
    }
}
```

<br>

---

## 5. 整體資料邏輯

- 一個 Batchupload，讀取 excel，每個 excel row 建立一個 batchuploaddata
- 建立一個 batchuploadtask job
- by processcount 去建立每次撈10個 batchupload data
- 做完壓狀態
- 會再繼續建立 batchupload task job
- 直到撈不到 batchuploaddata table 資料為止

<br>

---

## 6. 批次作業進度查詢 - 作業類型 - 下拉選單

### 6.1 Enum 定義

- BatchUploadTypeDefEnum
- BatchUploadExecuteTaskTypeEnum

<br>

### 6.2 新增權限

BatchUploadService.GetBatchUploadPermissionList()

http://bitbucket.org/nineyi/nineyi.sms/pull-requests/35898/overview

<br>

### 6.3 WebStoreDB.dbo.Definition 新增定義 (上限30字)

("BatchUpload", "BatchUpload_TypeDef", [Definition_Code])

<br>

### 6.4 LanguageTool 新增語系資料

- project: NineYi.Sms
- module: backend.definition.BatchUpload

<br>

### 6.5 BatchUploadService

- 新增批次作業類型
- BatchUploadExecuteTaskTypeEnum.DeliveryBatchPrintTranBill[Forwarder][DeliveryType]Task
- 調整 switch
- PrintBookingNoteBySearchCondition()
- GetBatchUploadExecuteTaskType()

<br>

### 6.6 UI

**NineYi.Sms\WebSite\WebSite\TypeScripts\Modules\BatchUpload\batchUploadList.controller.ts**

<br>

- 下載按鈕
- IsShowDownload()

<br>

**NineYi.Sms\WebSite\WebSite\TypeScripts\Modules\BatchUpload\batchUploadList.controller.ts**

<br>

- 下載檔案副檔名
- SendDownloadRequest()

<br>

---

## 7. Enum / TypeDef

BatchUploadTypeDefEnum

<br>

BatchUploadExecuteTaskTypeEnum

<br>

- WebStoreDB.dbo.Definition.Definition_Code, varchar(30)
- WebStoreDB.dbo.BatchUpload.BatchUpload_TypeDef, varchar(50)
- WebStoreDB.dbo.BatchUploadData.BatchUploadData_TypeDef, varchar(30)
- WebStoreDB.dbo.BatchUploadMessage.BatchUploadMessage_TypeDef, varchar(50)
- WebStoreDB.dbo.BatchUploadAverageTime.BatchUploadAverageTime_TypeDef, varchar(50)
- WebStoreDB.dbo.BatchUpload.BatchUpload_ExecuteTaskType, varchar(50)
- NMQV2DB.dbo.Job.Job_Name, varchar(50)

<br>

---

## 8. 批次匯入商品語系資料的銷售重點 (Selling Point) 欄位支援換行

https://bitbucket.org/nineyi/nineyi.scm.nmqv2/pull-requests/14557/diff

<br>

BatchUpdateSalePageMLService.CreateBatchUploadDataList

<br>

---

## 9. 下載錯誤明細

**頁面 URL**

<br>

https://sms.qa1.hk.91dev.tw/BatchUpload/Detail/11239

<br>

**API 端點**

<br>

https://sms.qa1.hk.91dev.tw/Api/BatchUpload/GetMessageList

<br>

**Request**

<br>

```json
{
    "SearchItem": 11239,
    "Take": 25,
    "Skip": 0
}
```

<br>

**Response**

<br>

```json
{
    "List": [
        {
            "Title": "Promotion ID: 242423",
            "StatusDef": "ValidateFailed",
            "StatusDefDesc": "資料檢查有誤",
            "Note": "• Promotion ID Not Found\n• Product Page ID Not Found: 651234,651235,651236,651237"
        },
        {
            "Title": "Promotion ID: 242424",
            "StatusDef": "ValidateFailed",
            "StatusDefDesc": "資料檢查有誤",
            "Note": "• Promotion ID Not Found\n• Product Page ID Not Found: 651235"
        },
        {
            "Title": "Promotion ID: 123499",
            "StatusDef": "ValidateFailed",
            "StatusDefDesc": "資料檢查有誤",
            "Note": "• Promotion ID Not Found\n• Product Page ID Not Found: 651233"
        }
    ],
    "PageCount": 1,
    "TotalCount": 3
}
```

<br>

---

## 10. 上限筆數驗證

**ModifyRewardPromotionSalePage**

<br>

固定 500 筆

<br>

```csharp
$"BatchUpload.{this.GetBatchUploadType()}.MaxCount", "500")
```

<br>

---

## 11. 🐏 🐏🐏 🐏🐏  狀態說明 🐏 🐏🐏 🐏🐏  

WaitingToLoadData : 已完成第一道驗證並建立 BatchUpload NMQ 等待處理

InLoadData : NMQ 接到任務開始初始化 準備 LoadExcel

InValidation : LoadExcel 完成 Wrapper 包好準備二次驗證


ReadyToProcess : 二次驗證成功 準備 CreateNMQ
ValidateFailed : 二次驗證失敗 
Finish : 主 NMQ 發現 dataList.Count == 0
WaitingToProces : 主 NMQ 發現 dataList.Count > 0
ScheduleAbortedWithErrors : 檢查 dataList 有 BatchUploadData_StatusDef == ValidateFailed 更新狀態

InProcess: 子 Job 已在處理中


Finish : 完成子 Job
ProcessFailed : 子 Job 跑完失敗
<br>