

OSM > 直收直付帳務核對 > 匯出帳務明細



## 查詢

https://sms.qa1.my.91dev.tw/Api/ExpenseOrder/GetExpenseOrderReportList

```json
{
    "DateTimeType": "CollectionPeriod",
    "StartDateTime": "2025-12-31T16:00:00.000Z",
    "EndDateTime": "2026-02-27T16:00:00.000Z",
    "StartPeriod": "",
    "EndPeriod": "",
    "CollectionPeriod": "",
    "Offset": 0,
    "Fetch": 50
}
```


Data.List[0].TotalGrabPayRazerFee
Data.List[1].TotalPayment



csp_GetExpenseOrderReportV2



## 匯出帳務明細


https://sms.qa1.my.91dev.tw/ExpenseOrder/ExportExpenseOrderReportDetailExcel

```json
{
    "DateTimeType": "CollectionPeriod",
    "StartDateTime": "2026-01-31T16:00:00.000Z",
    "EndDateTime": "2026-02-27T16:00:00.000Z",
    "StartPeriod": "",
    "EndPeriod": "",
    "CollectionPeriod": "2026/02",
    "Offset": 0,
    "Fetch": 50
}
```


csp_GetExpenseOrderReportDetailV2


文案取自 : backend.definition.PayProfile => PayProfile_TypeDef_GlobalPay_{payType}


