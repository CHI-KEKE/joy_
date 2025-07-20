# 📁 Filer 維護文件

> 📚 這是 Filer 系統的完整維護指南，包含不同環境下文件存放位置的詳細資訊

<br>

## 📖 目錄

- [📁 Filer 維護文件](#-filer-維護文件)
  - [📖 目錄](#-目錄)
  - [🌍 環境文件存放位置](#-環境文件存放位置)
  - [⚙️ 設定檔路徑配置](#️-設定檔路徑配置)
  - [🖥️ 機台實體位置](#️-機台實體位置)
  - [🔗 後台檔案下載機制](#-後台檔案下載機制)
  - [🛣️ OSM 路由設定](#️-osm-路由設定)
  - [📄 範例檔案案例](#-範例檔案案例)

<br>

---

## 🌍 環境文件存放位置

**HK/MY QA 環境**：

<br>

文件存放路徑：

<br>

```
\\SG-HK-QA1-SCM2\Storage\Docs
```

<br>

**環境說明**：

<br>

- 適用於香港 (HK) 和馬來西亞 (MY) 的 QA 測試環境
- 使用網路共享路徑進行文件存取
- 確保網路連線正常才能存取該路徑

<br>

**TW QA 環境**：

<br>

文件存放路徑：

<br>

```
D:\Files\Docs\BatchModifyPromotionOuterId
```

<br>

---

## ⚙️ 設定檔路徑配置

### 📋 HK QA 檔案路徑設定

<br>

```xml
<!-- Filer 檔案路徑 -->
<add key="QA.File.Path" value="\\SG-HK-QA1-SCM2\Storage" xdt:Transform="Insert"/>
<add key="QA.File.Path.Docs" value="\\SG-HK-QA1-SCM2\Storage\Docs" xdt:Transform="Insert"/>
<add key="QA.File.Path.Image" value="\\SG-HK-QA1-SCM2\Storage\Images" xdt:Transform="Insert"/>
<add key="QA.File.Path.Tmp" value="\\SG-HK-QA1-SCM2\Storage\Tmp" xdt:Transform="Insert"/>
<add key="QA.File.Path.Tmp.BatchUpload" value="\\SG-HK-QA1-SCM2\Storage\Tmp\BatchUpload" xdt:Transform="Insert"/>
<add key="QA.File.Path.Tmp.Image" value="\\SG-HK-QA1-SCM2\Storage\Tmp\Images" xdt:Transform="Insert"/>
```

<br>

### 🏭 HK PROD 檔案路徑設定

<br>

```xml
<add key="Prod.File.Path" value="\\SG-HK-Filer1\Storage" xdt:Transform="Insert" />
<add key="Prod.File.Path.Docs" value="\\SG-HK-Filer1\Storage\Docs" xdt:Transform="Insert" />
<add key="Prod.File.Path.Image" value="\\SG-HK-Filer1\Storage\Images" xdt:Transform="Insert" />
<add key="Prod.File.Path.Tmp" value="\\SG-HK-Filer1\Storage\Tmp" xdt:Transform="Insert" />
<add key="Prod.File.Path.Tmp.BatchUpload" value="\\SG-HK-Filer1\Storage\Tmp\BatchUpload" xdt:Transform="Insert" />
<add key="Prod.File.Path.Tmp.Image" value="\\SG-HK-Filer1\Storage\Tmp\Images" xdt:Transform="Insert" />
```

<br>

---

## 🖥️ 機台實體位置

**進入機台後的實際存放位置**：

<br>

```
E:\Storage\Docs\ModifyRewardPromotionSalePage
E:\Storage\Docs\BatchModifyPromotionOuterId
```

<br>

---

## 🔗 後台檔案下載機制

**範例檔下載機制**：

<br>

當使用者在後台點擊下載按鈕時，系統會呼叫相對應的 API 來提供檔案下載服務。

<br>

**API 呼叫範例**：

<br>

```
https://sms.qa1.hk.91dev.tw/CommerceCloud/Docs/BatchModifyPromotionOuterId/Batch update products in promotions template (Product part number).xlsx
```

<br>

---

## 🛣️ OSM 路由設定

**文件存取路由配置**：

<br>

```csharp
routes.MapRoute(
    "VirtualDirectoryDocuments",
    "Docs/{*pathInfo}",
    new { controller = "Document", action = "Get" });
```

<br>

**路由說明**：

<br>

- **路由名稱**：VirtualDirectoryDocuments
- **URL 模式**：Docs/{*pathInfo}
- **控制器**：Document
- **動作**：Get
- **功能**：處理所有以 "Docs/" 開頭的請求，將其導向 Document 控制器進行檔案存取處理

<br>

---

## 📄 範例檔案案例

### 1. 交易黑名單

<br>

```
https://bitbucket.org/nineyi/nineyi.files/pull-requests/464/diff
```
