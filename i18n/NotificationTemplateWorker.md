# 📧 NotificationTemplateWorker

<br>

## 📖 目錄

1. [🔧 本機開發問題排除](#-本機開發問題排除)
2. [☁️ AWS 設定問題](#-aws-設定問題)
3. [📱 推播資料設計](#-推播資料設計)
4. [🗄️ 通知多語系資料表結構](#️-通知多語系資料表結構)
5. [⚙️ AWS 設定資訊](#️-aws-設定資訊)
6. [🔗 通知模板 API](#-通知模板-api)
7. [💾 DynamoDB 資料儲存](#-dynamodb-資料儲存)
8. [📋 通知多語系邏輯要點](#-通知多語系邏輯要點)

<br>

---

## 🔧 本機開發問題排除

**問題 1：Test 專案找不到 GeneratedMSBuildEditorConfigFile**

<br>

**解決步驟**：

<br>

1. 開啟登錄編輯程式
   ```
   WIN + R => regedit
   ```

<br>

2. 導航至指定路徑
   ```
   HKEY_LOCAL_MACHINE => SYSTEM => CurrentControlSet => Control => FileSystem => LongPathsEnabled
   ```

<br>

3. 修改數值
   ```
   右鍵 => Value 改成 1
   ```

<br>

4. 重新啟動電腦

<br>

**說明**：這個問題通常是因為 Windows 系統對長路徑的限制導致，啟用長路徑支援可以解決此問題

<br>

---

## ☁️ AWS 設定問題

**問題 2：沒吃到 AWS 設定**

<br>

**解決方法**：

<br>

把正確的 AWS Key / Secret 複製過去

<br>

**操作步驟**：

<br>

1. 確認 AWS 認證資訊設定檔位置
2. 複製正確的 AWS Access Key ID
3. 複製正確的 AWS Secret Access Key
4. 更新本機設定檔或環境變數

<br>

**注意事項**：

<br>

確保 AWS 認證資訊具有適當的權限，並且格式正確無誤

<br>

---

## 📱 推播資料設計

**APP 初次啟動資料寫入**：

<br>

當使用者下載併開啟 APP 後，系統會在資料表中寫入以下資訊：

<br>

- **Token**：用於推播功能
- **GUID**：用於訊息中心識別
- **APPVer**：APP 版本，用於推播相容性

<br>

**會員登入註冊流程**：

<br>

當 APP 進行登入或註冊操作時，系統會將對應的 MemberId 寫入 `DeviceAPPMapping` 資料表中

<br>

---

## 🗄️ 通知多語系資料表結構

**訊息中心相關資料表**：

<br>

- `NotificationDB.Notification` - 主要通知資料
- `NotificationDB.NotificationSlave` - 通知從屬資料

<br>

**推播相關資料表**：

<br>

- `NotificationDB.PushNotification` - 推播主要資料
- `NotificationDB.PushNotificationSlave` - 推播從屬資料（包含 token 記錄）

<br>

**統計相關資料表**：

<br>

- `NotificationDB.PushNotificationStatistics` - 推播統計資料

<br>

---

## ⚙️ AWS 設定資訊

**認證設定**：

<br>

```json
{
    "AccessKey": "",
    "AccessSecret": "",
    "S3Bucket": "91dev-ap-southeast-1-qa-hk-data",
    "Region": "ap-southeast-1"
}
```

<br>

**認證檔案確認**：

<br>

檢查 `Users/.aws/credentials` 檔案確認是否有相關記錄

<br>

---

## 🔗 通知模板 API

**建立模板 API**：

<br>

```
POST http://notification-template-api-internal.qa1.hk.91dev.tw/api/Templates/Create
```

<br>

**請求範例**：

<br>

```json
{
    "type": "PushNotification",
    "name": "LoyaltyPointUseStartNotification_Template",
    "shopId": -1,
    "language": "zh-TW",
    "parameters": [],
    "template": {
        "Title": "紅利點數生效通知",
        "Content": "紅利點數發送通知，提醒您儘快使用唷~",
        "NotificationType": "LoyaltyPoint",
        "NotificationTargetType": "LoyaltyPoint",
        "NotificationSource": "LoyaltyPoint",
        "NotificationCountType": "ECoupon",
        "NeedPush": "true",
        "NeedStatistics": "true"
    },
    "messageType": "ECoupon",
    "editor": "AllenLin"
}
```

<br>

**取得模板 API**：

<br>

```
GET http://notification-template-api-internal.qa1.hk.91dev.tw/api/Templates/LoyaltyPointUseStartNotification_Template/-1
```

<br>

---

## 💾 DynamoDB 資料儲存

**模板資料儲存位置**：

<br>

```
DynamoDB Key: PushNotification/LoyaltyPointUseStartNotification_Template/-1/zh-TW
```

<br>

---

## 📋 通知多語系邏輯要點

**處理流程重點**：

<br>

1. 每一個 receiver 都有 `NeedPush` 參數控制是否推播
2. 每一個 request 代表一組 Receivers 的處理批次
3. 每一個 message 會產生一個 notification 主檔與對應的 slave 從檔
4. 當 receiver 的 `needpush = true` 時，系統會寫入推播佇列 `waitToPushNotifications`
5. 一個 memberId + device 組合會產生一個主檔搭配一個子檔

<br>

**PushNotificationSlave 資料欄位**：

<br>

- ShopId - 商店識別碼
- Platform - 平台類型
- pushstatus - 推播狀態
- createdDatetime - 建立時間
- createUser - 建立使用者
- GUID - 全域唯一識別碼
- App_Version - APP 版本
- token - 推播權杖

<br>

**PushNotification 資料欄位**：

<br>

- TypeDef - 類型定義（例如：'LoyaltyPoint'）
- PushNotification_DateTime - 推播時間
- Content - 推播內容
- StatusDef - 狀態定義
- CreatedUser - 建立使用者

<br>

---