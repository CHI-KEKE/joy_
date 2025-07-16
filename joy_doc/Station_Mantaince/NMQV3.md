# 🔄 NMQV3 維護文件

> 📚 這是 NMQV3 系統的完整維護指南，包含開發環境設定、語系工具和測試配置等重要資訊

<br>

## 📖 目錄

- [🔄 NMQV3 維護文件](#-nmqv3-維護文件)
  - [📖 目錄](#-目錄)
  - [🌐 語系工具設定](#-語系工具設定)
  - [🛠️ Debug 屬性設定](#️-debug-屬性設定)

<br>

---

## 🌐 語系工具設定

**Translations 語系工具執行**：

<br>

NMQV3 開發時需要執行 Translations 語系工具

<br>

**參考文件**：

<br>

詳細的語系工具安裝和使用方法，請參考三中心開發維護文件中的相關章節

<br>

**相關章節包含**：
- Cake Tool 安裝與使用
- 語系工具安裝
- 系統多語系設定

<br>

**重要提醒**：

<br>

確保按照三中心文件中的步驟完成語系工具的安裝和初始化，這是 NMQV3 正常運行的必要條件

<br>

---

## 🛠️ Debug 屬性設定

**測試 Job 設定**：

<br>

在 Visual Studio 中進行以下設定：

<br>

```
Nine1.Commerce.Nmqv3.Console.NMQv3Worker Debug Properties => 設定測試 Job
```

<br>

**操作步驟**：

<br>

1. 在 Visual Studio 中找到 `Nine1.Commerce.Nmqv3.Console.NMQv3Worker` 專案
2. 右鍵點選專案，選擇「Properties」或「屬性」
3. 導航至「Debug」或「偵錯」頁籤
4. 在 Debug Properties 中設定測試 Job