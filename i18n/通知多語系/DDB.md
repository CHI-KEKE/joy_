

## 模板資料儲存位置

DynamoDB Key: PushNotification/LoyaltyPointUseStartNotification_Template/-1/zh-TW

<br>
<br>

## 📋 通知多語系邏輯要點

**處理流程重點**：

<br>

1. 每一個 receiver 都有 `NeedPush` 參數控制是否推播
2. 每一個 request 代表一組 Receivers 的處理批次
3. 每一個 message 會產生一個 notification 主檔與對應的 slave 從檔
4. 當 receiver 的 `needpush = true` 時，系統會寫入推播佇列 `waitToPushNotifications`
5. 一個 memberId + device 組合會產生一個主檔搭配一個子檔

<br>


<br>

---