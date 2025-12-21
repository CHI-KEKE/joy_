## 建立模板 API

POST http://notification-template-api-internal.qa1.hk.91dev.tw/api/Templates/Create


#### 請求範例

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
<br>

## 取得模板 API

GET http://notification-template-api-internal.qa1.hk.91dev.tw/api/Templates/LoyaltyPointUseStartNotification_Template/-1