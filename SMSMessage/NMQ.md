C:\91APP\NMQ\nineyi.erp.nmqv2\ERP\Backend\NMQV2\SMS\SMSMessageProcess.cs

ERPNMQ

SMSMessagePriorityHigh => group9001 只有他特殊通道
SMSMessage =>  group9003 跟一堆job 擠

TaskData


MessageHigh
```json
{
  "ShopId": 76,
  "SMSType": "會員註冊",
  "IsSMSEntity": true,
  "PhoneNumber": "64764730",
  "MessageContent": "PUMA HK：您的驗證碼是1871，歡迎使用！",
  "BookingDateTime": "2025-12-10T16:42:15.1384565+08:00",
  "CountryProfileId": 85
}
```

一般
```json
{
  "ShopId": 22,
  "LocationId": 1681,
  "SMSType": "門市小幫手註冊成功",
  "IsSMSEntity": true,
  "PhoneNumber": "92690131",
  "MessageContent": "尚品網店 Welcome! Download the official App https://bmai.app/61731cf9. Your default password is 690131",
  "BookingDateTime": "2025-12-10T16:44:18.3109183+08:00",
  "CountryProfileId": 85
}
```


string pattern = @"(?<phone>\d+),(?<message>[^,]*)(,(?<countryProfileId>\d+))?";

🧾 實際匹配範例
1️⃣ "0912345678,Hello"



Task data 參數有傳   "CountryProfileId": 85


currentProfileId : "SystemDefault.Local.CountryProfileId"



```csharp
//// 非台灣簡訊,固定Nexmo 
if (country != CountryProfileEnum.Taiwan)
{
    return SmsMessageProfileVendorEnum.Nexmo;
}
```


sms 要加上 CountryProfileEnum

- Profileid = 35 (MYPROD)
- CountryProfileEnum = Singapore


myqa

Job_Id
6
239
Job_Name
SMSMessage
Job_Name
SmsMessagePriorityHigh

hkprod

Job_Id
6
Job_Id
264
Job_Name
SMSMessage
Job_Name
SmsMessagePriorityHigh



- ERPDB CountryProfile要壓資料
- enum要加
- 待確認他是否真的可以發 SG 簡訊 會掛掉嗎

應該就這樣


實測可以直接發送

