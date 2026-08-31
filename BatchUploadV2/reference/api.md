# BatchUpload API 參考

## 下載錯誤明細

### 頁面 URL

```bash
https://sms.qa1.hk.91dev.tw/BatchUpload/Detail/{BatchUploadId}
```

### API Endpoint

```bash
POST https://sms.qa1.hk.91dev.tw/Api/BatchUpload/GetMessageList
```

### Request

```json
{
    "SearchItem": 11239,
    "Take": 25,
    "Skip": 0
}
```

### Response

```json
{
    "List": [
        {
            "Title": "Promotion ID: 242423",
            "StatusDef": "ValidateFailed",
            "StatusDefDesc": "資料檢查有誤",
            "Note": "• Promotion ID Not Found\n• Product Page ID Not Found: 651234,651235,651236,651237"
        },
        {
            "Title": "Promotion ID: 242424",
            "StatusDef": "ValidateFailed",
            "StatusDefDesc": "資料檢查有誤",
            "Note": "• Promotion ID Not Found\n• Product Page ID Not Found: 651235"
        },
        {
            "Title": "Promotion ID: 123499",
            "StatusDef": "ValidateFailed",
            "StatusDefDesc": "資料檢查有誤",
            "Note": "• Promotion ID Not Found\n• Product Page ID Not Found: 651233"
        }
    ],
    "PageCount": 1,
    "TotalCount": 3
}
```
