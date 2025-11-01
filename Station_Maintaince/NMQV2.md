# 🔄 NMQV2 文件

<br>

## 📖 目錄
  - [🔧 .NET Framework 安裝](#-net-framework-安裝)
  - [💻 IDE 問題排除](#-ide-問題排除)
  - [⚙️ NMQ 組件框架需求](#️-nmq-組件框架需求)
  - [🧪 本機開發測試步驟](#-本機開發測試步驟)
  - [🔄 更新線上 NMQ 狀態](#-更新線上-nmq-狀態)
  - [🚀 NMQ V2 的 QA 部署步驟](#-nmq-v2-的-qa-部署步驟)
  - [� NMQV2 部署後版號確認位置](#-nmqv2-部署後版號確認位置)
  - [�🔀 切換環境測試方法](#-切換環境測試方法)
  - [📊 Athena Log](#-athena-log)

<br>
---

## 🔧 .NET Framework 安裝

**Microsoft .NET Framework 參考組件安裝**：

<br>

安裝網址：https://www.nuget.org/packages/microsoft.netframework.referenceassemblies.net45

<br>

**.NET Framework 4.5.2 Developer Pack 離線安裝器**：

<br>

參考文件：https://www.cnblogs.com/rqcim/p/15882239.html

<br>

這個安裝包提供了完整的開發環境支援，適用於離線環境的安裝需求

<br>

---

## 💻 IDE 問題排除

**Visual Studio 2022 相容性問題**：

<br>

如果在使用 Visual Studio 2022 時遇到 IDE 報錯問題，需要安裝 Visual Studio 2019 才能解決

<br>

**問題詳細說明與解決方案**：

<br>

參考討論：https://www.reddit.com/r/VisualStudio/comments/x9w4ep/vs_2022_keeps_asking_me_to_download_net_framework/?rdt=58431

<br>

這個問題通常出現在 VS 2022 持續要求下載 .NET Framework 的情況下，透過安裝 VS 2019 可以解決相容性問題

<br>

---

## ⚙️ NMQ 組件框架需求

**NMQ Dispatcher 框架需求**：

<br>

需要安裝 .NET Core 2.1

<br>

**其他子 NMQ 組件框架需求**：

<br>

需要安裝 .NET Framework 4.5

<br>

**重要提醒**：

<br>

不同的 NMQ 組件有不同的框架需求，請確保按照組件類型安裝正確的框架版本，避免相容性問題

<br>

---

## 🧪 本機開發測試步驟

**步驟 1：複製專案**

<br>

複製 SCM.NMQV2 / NMQV2 儲存庫到本機

<br>

**步驟 2：設定任務設定檔**

<br>

前往 NMQV2 專案 → Tool folder → DebugConsole proj → tasksetting.json 填值

這裡的資料表示 task data 的 payload

<br>

**案例 1：ThirdPartyPaymentReCheck**

```json
{
  "Job": {
    "Name": "ThirdPartyPaymentReCheck",
    "AssemblyRoot": "C:\\91APP\\nineyi.scm.nmqv2\\SCM\\Frontend\\NMQV2",
    "AssemblyPath": "bin\\Debug",
    "AssemblyName": "NineYi.SCM.Frontend.NMQV2",
    "ClassName": "NineYi.SCM.Frontend.NMQV2.ThirdPartyPayment.ThirdPartyPaymentReCheckProcess"
  },
  "Id": 2,
  "Data": "{\"RefundRequestIds\":[8957],\"TradesOrderGroupId\":34437,\"PayType\":\"QFPay\"}"
}
```

<br>

**案例 2：RefundFinish**

```json
{
  "Job": {
    "Name": "TwoCTwoPRefundRequestFinish",
    "AssemblyRoot": "C:\\91APP\\nineyi.scm.nmqv2\\SCM\\Frontend\\NMQV2",
    "AssemblyPath": "bin\\Debug",
    "AssemblyName": "NineYi.SCM.Frontend.NMQV2",
    "ClassName": "NineYi.SCM.Frontend.NMQV2.ThirdPartyPayments.PaymentMiddleWareRefundRequestFinishProcess"
  },
  "Id": 2,
  "Data": "{\"RefundRequestIds\":[5610],\"TradesOrderGroupId\":22409,\"PayType\":\"TwoCTwoP\"}"
}
```

<br>

**步驟 3：設定環境設定檔**

<br>

設定 NMQV2 config 到對應的市場環境

<br>

**步驟 4：程式碼路徑設定**

<br>

在 NMQV2 專案的 Program.cs 中設定正確路徑

範例：`File.ReadAllText(@"C:\91APP\NMQ\nineyi.nmqv2\Tools\DebugConsole\tasksettings.json")`

<br>

**步驟 5：設定起始專案**

<br>

設定 NMQV2.NineYi.SCM.Fronted.NMQV2 為起始專案

路徑：`C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\NMQV2\NineYi.SCM.Frontend.NMQV2.csproj`

<br>

**步驟 6：偵錯設定**

<br>

在 (SCMNMQ) properties → 偵錯 → Debug.exe 中設定：

`C:\91APP\nineyi.nmqv2\Tools\DebugConsole\bin\Debug\NineYi.NMQV2.DebugConsole.exe`

<br>

---

## 🔄 更新線上 NMQ 狀態

**1. 更新 NMQ 狀態為 Ready 語法**

<br>

```sql
USE NMQV2DB

--Backup Begin
SELECT
	*
INTO MATempDB.dbo.tmpNMQV2DB_Task_VSTS109027
FROM dbo.Task WITH (NOLOCK)
WHERE Task_Id = 1167609
AND Task_JobId = 213
--Backup End

--Update Begin
UPDATE dbo.Task
SET Task_Status = 'Ready'
,Task_UpdatedTimes = Task_UpdatedTimes % 255 + 1
,Task_UpdatedDatetime = GETDATE()
,Task_UpdatedUser = 'VSTS109027'
WHERE Task_Id = 1167609
AND Task_JobId = 213
--Update End

--Verify Begin
SELECT
	*
FROM dbo.Task WITH (NOLOCK)
WHERE Task_Id = 1167609
AND Task_JobId = 213
--Verify End
```

<br>

**2. 塞入 NMQ Task 語法**

<br>

```sql
--Step03 新增排程任務 ShippingOrderFinish (全家取貨完成)
USE NMQV2DB
GO
 
DECLARE @CreateDateTime DATETIME = GETDATE()
,@NmqJobId INT = -1
,@NmqJobName varchar(100) ='ShippingOrderFinish'
,@DataType varchar(30)  = 'R96'
,@CreateUser varchar(30) = 'VSTS109011';


SELECT @NmqJobId = [Job_Id]
FROM [NMQV2LS].[NMQV2DB].[dbo].[Job] with(nolock)
WHERE [Job_Name] = @NmqJobName

-- 新增 NMQV2 批次處理
INSERT INTO [NMQV2DB].[dbo].[Task]
([Task_JobId]
,[Task_Status]
,[Task_DispatchTime]
,[Task_StartTime]
,[Task_ErrorCount]
,[Task_Data]
,[Task_ServerId]
,[Task_CreatedDatetime]
,[Task_CreatedUser]
,[Task_UpdatedTimes]
,[Task_UpdatedDatetime]
,[Task_UpdatedUser]
,[Task_ValidFlag])
VALUES
(@NmqJobId
,'Ready'
,@CreateDateTime
,@CreateDateTime
,0
, @DataType
,1
,@CreateDateTime
,@CreateUser
,0
,@CreateDateTime
,@CreateUser
,1)
```
---

## 🚀 NMQ V2 的 QA 部署步驟

**步驟 1：確認 CI Master 建構狀態**

<br>

確認該 NMQ CI Master 已經 Build 完成，並成功上傳至 Artifact

<br>

**CI Master 網址**：http://ci-master.91dev.tw:8080/

<br>

**步驟 2：關閉 NMQV3 Dashboard Router**

<br>

在部署前先關閉 Router 以確保系統穩定性

<br>

**NMQV3 Dashboard 網址**：https://nmqv3-dashboard.qa1.hk.91dev.tw/router-management/

<br>

**步驟 3：執行部署程序**

<br>

執行 CI MY 部署程式至 NMQV3

<br>

**部署網址**：https://ci.my.91app.io/view/Deploy%20Global%20QA/job/NineYi.Global.NMQV3%20-%20QA/

<br>

**步驟 4：系統重啟**

<br>

關機後重啟系統以確保新版本正常載入

<br>

**重要提醒**：

<br>

- 確保在部署前進行必要的備份
- 按順序執行各個步驟，不可跳過
- 部署完成後驗證系統功能是否正常

<br>

---

## � NMQV2 部署後版號確認位置

**部署後版號更新檢查路徑**：

<br>

**位置 1：Worker Configs**

<br>

路徑：`D:\nmq\artifacts\worker-configs`

<br>

此位置包含 NMQ Worker 的設定檔資訊

<br>

**位置 2：Worker Modules Jobs**

<br>

路徑：`D:\nmq\artifacts\worker-modules\jobs\Prod\NineYi\SCM.NMQV2`

<br>

此位置包含生產環境 SCM NMQV2 的作業模組

<br>

**位置 3：NMQV2 Library**

<br>

路徑：`D:\Prod\NineYi\NMQV2\Library\NineYi.Scm.Frontend`

<br>

此位置包含 NMQV2 前端函式庫檔案

<br>

**版號確認重要提醒**：

<br>

- 部署完成後應逐一檢查以上三個位置的版號
- 確保所有位置的版號都已正確更新
- 如發現版號不一致，請重新執行部署程序
- 建議在部署完成後建立檢查清單以確保版號一致性

<br>

---

## �🔀 切換環境測試方法

**切換 V2 / V3 Group Mapping**

<br>

以切換到不同的機器 BRANCH (QAn) 上做測試

<br>

會推送到 branch：`jenkins-91app-nineyi.scm.nmqv2-feature%2FVSTS410499-QA8-SoNice-LoyaltyPointProject-1`

並部署到 QA8 上

<br>

**操作步驟**：

<br>

1. 確認 JobGroup & Server 關係
2. 解除舊 Job & JobGroup 關係
3. 新增 Job & JobGroup 關係
4. 壓掉 Switch 的資料

<br>

**切換語法**：

<br>

```sql
-- V3切回V2:
Update dbo.JobSwitch(NOLOCK)
SET JobSwitch_ValidFlag =0
Where JobSwitch_JobId = 195

-- 復活JobGroupMapping  jobid = 195，並且把JobGroupId改成10 (QA8):
Update dbo.JobGroupMapping(NOLOCK)
SET JobGroupMapping_ValidFlag = 1,JobGroupMapping_JobGroupId = 10
Where JobGroupMapping_JobId = 195
AND JobGroupMapping_Id = 610

-- 驗證
SELECT *
FROM JobGroupMapping  
Where JobGroupMapping_JobId = 195
AND JobGroupMapping_Id = 610

SELECT *
FROM JobSwitch
Where JobSwitch_JobId = 195
```

<br>

**還原語法**：

<br>

```sql
--還原switch
Update dbo.JobSwitch
SET JobSwitch_ValidFlag = 1
Where JobSwitch_JobId = 195

--還原JobGroupMapping
Update dbo.JobGroupMapping
SET JobGroupMapping_ValidFlag = 0,JobGroupMapping_JobGroupId = 1
Where JobGroupMapping_JobId = 195
AND JobGroupMapping_Id = 610

SELECT *
FROM JobGroupMapping  
Where JobGroupMapping_JobId = 195
AND JobGroupMapping_Id = 610
```

<br>

**測試環境範例**：

<br>

以在 QA8 開發為例：TYO-QA8-SCM

<br>

**通報頻道模板**：

<br>

發送至 upd-team 頻道：

<br>

```
因 SONICE自訂給點天數專案測試需求，TW-QA 環境以下 NMQ Job
RewardLoyaltyPoint

預計將會做以下調整：
將 Job 從 V3 切回至 V2 (移除 JobSwitch)
將 JobGroupMapping 從 QA 切至 QA8
如有問題再麻煩於此 thread 底下留言，謝謝
```

<br>

---

## 📊 Athena Log

**Workgroup 設定**：

<br>

**nmqv3_hk：** 要使用 workgroup UPD-UPD2-B2E6

**nmqv3_my：** 要使用 workgroup UPD-UPD2-B2E8

<br>

**查詢語法範例**：

<br>

```sql
SELECT * FROM "nmqv3_my"."archive_task"
--where 1 =1
--and controller = 'tradesOrderLite'
--and action = 'CompleteForNewCart'
--and date = '2025/06/24'
--and message like '%MG250624J00008%'
where job_name = 'OnlineBankingRazerRefundRequestFinish'
--and requestid = '{"message":"202506160110276106'
and date = '2025/07/14'
and id = 'd0b8de76-683b-4ed7-8a00-232ea1f90623'
limit 100;
```

<br>

![alt text](./image-13.png)

<br>

---