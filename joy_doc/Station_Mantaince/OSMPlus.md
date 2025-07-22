# 🚀 OSMPlus 維護文件

## 📖 目錄

  - [📁 基本路徑資訊](#-基本路徑資訊)
  - [⚠️ Exception 處理](#️-exception-處理)
  - [📦 套件相關路徑](#-套件相關路徑)
  - [🛠️ 部署前準備](#️-部署前準備)
  - [🔧 Nuget 更新版本問題處理](#-nuget-更新版本問題處理)
  - [🌍 市場確認路徑](#-市場確認路徑)
  - [🚀 部署流程](#-部署流程)
  - [🔍 問題處理](#-問題處理)
  - [⏪ 版本還原](#-版本還原)
  - [💼 CRM Console 部署](#-crm-console-部署)

---


<br>
<br>

## 📁 基本路徑資訊

**主要程式碼路徑**:
```bash
D:/Files/OsmConsole
```

<br>
<br>

## ⚠️ Exception 處理

當 PushWK 發生 Exception 時，請檢查以下路徑：

> 💡 **提示**: 這個路徑通常包含錯誤日誌和除錯資訊

```bash
D:\Files\OsmConsole\ProgramException\20240919
```

<br>
<br>

<br>
<br>

## 📦 套件相關路徑

**Newtonsoft 套件相關路徑**:
```bash
D:\Prod\NineYi\EtlConsole
```

<br>
<br>

## 🛠️ 部署前準備

> ⚠️ **重要注意事項**

- 📸 停止排程位置的畫面參考圖


<br>
<br>

## 🔧 Nuget 更新版本問題處理

**因 Nuget 更新版本導致部署失敗**:
- 🔑 需要 SG-Jenkins-1 權限進機器 restore

<br>
<br>

**Miller 舊版 SOP**:  
🔗 **參考連結**: [Wiki SOP 文件](https://wiki.91app.com/pages/viewpage.action?pageId=54716023)

> 📋 **操作步驟**  
> 在執行 Build a Visual Studio projects or solution using MSBuild 步驟前，  
> 新增執行 Windows 批次指令：

```batch
.nuget\NuGet.exe restore [要建置的專案完整檔名.附檔名]
```

📝 **範例**:
```batch
.nuget\NuGet.exe restore NineYi.WebStore.MallAndApi.sln
```

<br>
<br>

**Bill 提供的新版本**:  
特定專案拉 NuGet 清單做更新

📷 **參考圖片**: [Google Photos 連結](https://photos.google.com/share/AF1QipNHwUtUCJoAIgibPa4JsEcfa-0Pb8IsKJEFAmf_3eMQrwDCoQc02dOBdOtC4ASU7w/photo/AF1QipMkHJLR0D3gACG50UODzp1lLqLsfKaeF3eKbfF6?key=bjdPbkZtYncyNHByc3cwS010MmVnQ2UxMVNQTkZR)


<br>
<br>


## 🌍 市場確認路徑

**MY 市場**:
- Exception 路徑:
  ```bash
  PushWK Exception : D:\Files\OsmConsole\ProgramException\20240919
  ```
- 程式碼路徑:
  ```bash
  Code Newton : D:\Prod\NineYi\EtlConsole
  D:\Prod\NineYi\EtlConsole\OSMPlusBatchRunFlowByMessageQueueV2
  ```

**HK 市場**:
- **位置路徑**:
  ```bash
  C:\temp\raw\EtlConsole\OSMPlusBatchRunFlowById
  C:\temp\raw
  ```

<br>
<br>

## 🚀 部署流程

**Build 步驟**:
1. Order ==> ToCSV關兩個JOB
2. 複製打包路徑

**下載路徑**:

- **FirstFlowTask**:
  ```
  http://master.build.91app.io/repository/nineyi-release-raw-hosted/tmp/MKT/HK/OsmPlus/NineYi.OsmPlus.Batch.RunFirstFlowTask_b20240222-1404_h2042271995.zip
  ```

- **FlowById**:
  ```
  http://master.build.91app.io/repository/nineyi-release-raw-hosted/tmp/MKT/HK/OsmPlus/NineYi.OsmPlus.Batch.RunFlowById_b20240222-1404_h2042271995.zip
  ```

- **FlowByMessageQueueV2**:
  ```
  http://master.build.91app.io/repository/nineyi-release-raw-hosted/tmp/MKT/HK/OsmPlus/NineYi.OsmPlus.Batch.RunFlowByMessageQueueV2_b20240222-1404_h2042271995.zip
  ```

**InstanceName 清單**:

| 編號 | Instance Name |
|------|---------------|
| 4 | SG-HK-PushWK11 |
| 5 | SG-HK-PushWK12 |
| 6 | SG-HK-PushWK13 |

**機器上確認 Task**:
```
TASK Schedule --> order --> task manager --> end
```

**部署完成後檢查清單**:

> 📋 **檢查清單**

1. 進機台
2. 到 task schedule 去確認那些 job ps 會寫在哪個位置 可以確認 buildtag
3. pushworder 2,3...只需要確認是否部署到
4. TASK SCHEDULER 重新把根據 CHECKLIST 啟動曾經關的 JOB

<br>
<br>

## 🔍 問題處理

**被咬檔處理問題**:

TASK MANAGER 把 JOB 停掉

**確認檔案路徑**:
```bash
D:\Prod\NineYi\EtlConsole\OSMPlusBatchRunFlowByMessageQueueV2
```

<br>
<br>

## ⏪ 版本還原

**備份路徑**:
```bash
D:\Backup\Prod\SG-MY-PushWK1
```

**還原機器**:
```bash
SG-Jenkins1 << 這台機器上
```

**還原方式**:
- 📂 **直接用檔案覆蓋的方式來還原**

<br>
<br>

## 💼 CRM Console 部署

**版本確認路徑**：

<br>

```bash
D:\Batch\ExportModifiedCrmMemberData
```

<br>

**部署確認**：

<br>

可以到上述路徑確認 CRM Console 是否有版本更新，透過檢查檔案時間戳記或版本資訊來驗證部署是否成功。

<br>

**操作說明**：

<br>

- 進入指定路徑檢查檔案更新狀態
- 確認部署後的程式版本是否正確
- 驗證 CRM Console 功能是否正常運作

<br>

---