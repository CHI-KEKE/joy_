## APP 初次啟動資料寫入

當使用者下載併開啟 APP 後，系統會在資料表中寫入以下資訊：

- **Token**：用於推播功能
- **GUID**：用於訊息中心識別
- **APPVer**：APP 版本，用於推播相容性

<br>
<br>

## 會員登入註冊流程

當 APP 進行登入或註冊操作時，系統會將對應的 MemberId 寫入 `DeviceAPPMapping` 資料表中

<br>
<br>

## 訊息中心

- `NotificationDB.Notification` - 主要通知資料
- `NotificationDB.NotificationSlave` - 通知從屬資料

<br>
<br>

## 推播

- `NotificationDB.PushNotification` - 推播主要資料

- TypeDef - 類型定義（例如：'LoyaltyPoint'）
- PushNotification_DateTime - 推播時間
- Content - 推播內容
- StatusDef - 狀態定義
- CreatedUser - 建立使用者


- `NotificationDB.PushNotificationSlave` - 推播從屬資料（包含 token 記錄）

- ShopId - 商店識別碼
- Platform - 平台類型
- pushstatus - 推播狀態
- createdDatetime - 建立時間
- createUser - 建立使用者
- GUID - 全域唯一識別碼
- App_Version - APP 版本
- token - 推播權杖

<br>
<br>

## 統計相關資料表

- `NotificationDB.PushNotificationStatistics` - 推播統計資料