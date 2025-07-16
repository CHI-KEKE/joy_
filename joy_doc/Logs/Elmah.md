# 🔍 Elmah 錯誤記錄查詢文件

![alt text](image.png)

<br>

## 📖 目錄

- [🔍 Elmah 錯誤記錄查詢文件](#-elmah-錯誤記錄查詢文件)
  - [📖 目錄](#-目錄)
  - [🔗 錯誤詳情查詢](#-錯誤詳情查詢)
  - [🌍 環境對應 URL](#-環境對應-url)
  - [👤 使用者識別說明](#-使用者識別說明)

<br>

---

## 🔗 錯誤詳情查詢

**Error ID 快速查詢**：

<br>

可以透過 Error ID 快速查看錯誤的詳細資訊

<br>

**查詢格式**：

<br>

```
http://elmahdashboard.91app.hk/Log/Details/{ErrorId}
```

<br>

**範例**：

<br>

```
http://elmahdashboard.91app.hk/Log/Details/146df0f5-2a6a-4fc6-bfc4-10c50bc318d2
```

<br>

---

## 🌍 環境對應 URL

**TW QA 環境**：

<br>

查詢 TW 環境的 Elmah 錯誤記錄：

<br>

```
http://elmahdashboard.qa.91dev.tw/?Page=1&Pagesize=300
```

<br>

**URL 參數說明**：

<br>

- `Page=1`：指定查看第一頁
- `Pagesize=300`：每頁顯示 300 筆記錄

<br>



---

## 👤 使用者識別說明

**使用者資訊特徵**：

<br>

Elmah 記錄中的 user 欄位通常顯示為一堆數字

<br>

**常見情況**：

<br>

這些數字型態的使用者 ID 通常是測試店家在進行各種操作測試

<br>

**識別方式**：

<br>

1. **數字格式使用者**：通常為測試環境的測試店家
2. **正常使用者 ID**：為正式環境的真實用戶
3. **空白或特殊值**：可能為系統自動操作或匿名存取

<br>

---