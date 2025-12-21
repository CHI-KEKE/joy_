
## 📎 環境

#### Microsoft .NET Framework 參考組件安裝

安裝網址：https://www.nuget.org/packages/microsoft.netframework.referenceassemblies.net45

<br>

#### NET Framework 4.5.2 Developer Pack 離線安裝器

參考文件：https://www.cnblogs.com/rqcim/p/15882239.html
這個安裝包提供了完整的開發環境支援，適用於離線環境的安裝需求

<br>

#### Visual Studio 2022 相容性問題*

如果在使用 Visual Studio 2022 時遇到 IDE 報錯問題，需要安裝 Visual Studio 2019 才能解決

參考討論：https://www.reddit.com/r/VisualStudio/comments/x9w4ep/vs_2022_keeps_asking_me_to_download_net_framework/?rdt=58431

這個問題通常出現在 VS 2022 持續要求下載 .NET Framework 的情況下，透過安裝 VS 2019 可以解決相容性問題

<br>

#### NMQ Dispatcher 框架需求

需要安裝 .NET Core 2.1

<br>

#### 子 NMQ 組件框架需求

需要安裝 .NET Framework 4.5

<br>
<br>

## 🧪 測試

- step 1 : clone SCM.NMQV2 / NMQV2 到本機
- step 2 : NMQV2 專案 → Tool folder → DebugConsole proj → tasksetting.json 填值, 這裡的資料表示 task data 的 payload

<br>

**ThirdPartyPaymentReCheck**

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

**RefundFinish**

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

- step 3 : 設定 NMQV2 config 到對應的市場環境
- step 4 : 在 NMQV2 專案的 Program.cs 中設定正確路徑

範例：`File.ReadAllText(@"C:\91APP\NMQ\nineyi.nmqv2\Tools\DebugConsole\tasksettings.json")`

- step 5 : 設定 NMQV2.NineYi.SCM.Fronted.NMQV2 為起始專案

路徑：`C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\NMQV2\NineYi.SCM.Frontend.NMQV2.csproj`

<br>

- step 6 : 在 (SCMNMQ) properties → 偵錯 → Debug.exe 中設定：

`C:\91APP\nineyi.nmqv2\Tools\DebugConsole\bin\Debug\NineYi.NMQV2.DebugConsole.exe`





## QA 切環境測試

#### BRANCH (QAn)

以切換到不同的機器 BRANCH (QAn) 上做測試
會推送到 branch：`jenkins-91app-nineyi.scm.nmqv2-feature%2FVSTS410499-QA8-SoNice-LoyaltyPointProject-1`
並部署到 QA8 上

#### 確認步驟

以在 QA8 開發為例：TYO-QA8-SCM

1. 確認 JobGroup & Server 關係
2. 解除舊 Job & JobGroup 關係
3. 新增 Job & JobGroup 關係
4. 壓掉 Switch 的資料

<br>

#### 切換語法

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

#### 還原語法

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

#### 通報頻道模板

Channel :  upd-team

```PLAINTEXT
因 SONICE自訂給點天數專案測試需求，TW-QA 環境以下 NMQ Job
RewardLoyaltyPoint

預計將會做以下調整：
將 Job 從 V3 切回至 V2 (移除 JobSwitch)
將 JobGroupMapping 從 QA 切至 QA8
如有問題再麻煩於此 thread 底下留言，謝謝
```



## NMQV3 本機開發

Nine1.Commerce.Nmqv3.Console.NMQv3Worker Debug Properties => 設定測試 Job

1. 在 Visual Studio 中找到 `Nine1.Commerce.Nmqv3.Console.NMQv3Worker` 專案
2. 右鍵點選專案，選擇「Properties」或「屬性」
3. 導航至「Debug」或「偵錯」頁籤
4. 在 Debug Properties 中設定測試 Job

**NMQV3 餵測試資料的模板**

```json
{ "Data":"{\"StartDatetime\":\"2025-03-07 02:45:34.592212\",\"EndDatetime\":\"2025-03-09 02:45:34.592212\"}"}
```