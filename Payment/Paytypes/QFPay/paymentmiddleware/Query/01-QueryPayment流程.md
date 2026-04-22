# QFPay — QueryPayment 付款查詢流程分析

## 流程概覽

```
PMW → POST /trade/v1/query
       ↓
   QFPay 回傳所有符合條件的交易紀錄（可能多筆）
       ↓
   過濾出 order_type=payment AND respcd=0000 AND out_trade_no=OrderCode
       ↓
   取最早一筆成功紀錄 → Success
   若無成功紀錄 → 取任意一筆 → 判斷 responseCode
```

---

## API 呼叫

```
POST /trade/v1/query
Content-Type: application/x-www-form-urlencoded
X-QF-APPCODE: {apiCode}
X-QF-SIGN: {sha256_signature}
X-QF-SIGNTYPE: SHA256
```

Request Body（form-urlencoded，排序後簽名）：

| 欄位（QFPay） | 說明 | 來源 |
|-------------|------|------|
| `syssn` | QFPay 交易編號 | `request.TransactionId` |
| `out_trade_no` | 訂單號（TG Code） | `ExtendInfo.OrderCode` |
| `pay_type` | 支付類型篩選 | `ExtendInfo.PayType` |
| `respcd` | 篩選特定回應代碼 | `ExtendInfo.ResponseCode` |
| `start_time` | 查詢開始時間 | `ExtendInfo.StartTime` |
| `end_time` | 查詢結束時間 | `ExtendInfo.EndTime` |
| `txzone` | 時區 | `ExtendInfo.TimeZone` |
| `page` | 頁碼 | `ExtendInfo.Page` |
| `page_size` | 每頁筆數 | `ExtendInfo.PageSize` |
| `mchid` | 商戶編號 | ⚠️ **固定為 null（Dead field）** |

> `start_time`/`end_time` 提供 `syssn` 或 `out_trade_no` 時會被 QFPay 忽略。  
> 跨月查詢必須提供 `start_time` 和 `end_time`。

---

## 成功判定邏輯（多步驟）

```
Step 1：API level check
  └→ apiResponse == null OR responseCode != "0000"
     → throw ApplicationException("QFPay Api 回應錯誤")

Step 2：找出所有成功的付款紀錄
  └→ order_type == "payment"
     AND responseCode == "0000"
     AND out_trade_no == ExtendInfo.OrderCode

Step 3：取最早一筆成功紀錄（按 TransactionId 升冪排序）
  └→ successfulOrderFirstData

Step 4：統計成功筆數（successfulOrderCount）
  └→ ⚠️ count > 1 時，Payment Console 會告警（重複付款）

Step 5：若無成功紀錄，取任意一筆 out_trade_no == OrderCode 的紀錄
  └→ 按 TransactionId 升冪排序取第一筆

Step 6：ReturnCode 判斷
  └→ orderData != null AND orderData.responseCode == "0000" → Success (1000)
  └→ 其他 → WaitingToPay (2003)
```

> **⚠️ 重要設計**：QFPay 對同一個 `out_trade_no` 可能存在多筆記錄（成功+失敗）。  
> 程式碼**優先正向表列成功**，再才考慮其他記錄，避免因存在失敗記錄就誤判為失敗。

---

## ReturnCode 對照

| 情境 | ReturnCode | 說明 |
|------|-----------|------|
| 找到 payment + 0000 | `1000` Success | 付款成功 |
| 找到紀錄但非 0000 | `2003` WaitingToPay | 付款尚未成功，等待重查 |
| API 回 null 或非 0000 | throw `ApplicationException` | QFPay API 異常 |

---

## QueryPaymentResponseExtendInfo 欄位

| 欄位 | 來源 | 說明 |
|------|------|------|
| `Page` | `apiResponse.Page` | 目前頁碼 |
| `PageSize` | `apiResponse.PageSize` | 每頁筆數 |
| `ResponseCode` | `apiResponse.ResponseCode` | API level 回應碼（`0000`） |
| `ResponseDescription` | `apiResponse.ResponseDescription` | API 描述訊息 |
| `Data` | `apiResponse.Data[]` | 所有交易紀錄（含付款與退款） |
| `SuccessCount` | 計算所得 | 同一 TG 的成功付款筆數（>1 需告警） |
| `RawData` | `apiResponse` | 原始回應物件 |

---

## 情境分析

### 情境 1：正常查詢成功
```
request.TransactionId = "sys_xxx"
ExtendInfo.OrderCode = "TG123456"
→ POST /trade/v1/query
→ apiResponse.Data 中找到 order_type=payment, respcd=0000, out_trade_no=TG123456
→ ReturnCode = 1000 (Success)
→ TransactionId = orderData.TransactionId
```

### 情境 2：付款尚未完成
```
→ POST /trade/v1/query（apiResponse.ResponseCode = "0000"）
→ Data 中有紀錄但 respcd != "0000"
→ ReturnCode = 2003 (WaitingToPay)
```

### 情境 3：同一訂單存在多筆成功記錄（告警情境）
```
→ POST /trade/v1/query
→ 找到 2 筆 order_type=payment, respcd=0000, out_trade_no=TG123456
→ SuccessCount = 2
→ 取 TransactionId 最小（最早）那筆作為 TransactionId
→ ReturnCode = 1000，但 SuccessCount > 1 → Payment Console 告警
```

### 情境 4：QFPay API 異常
```
→ POST /trade/v1/query
→ apiResponse == null 或 responseCode != "0000"
→ throw ApplicationException("QFPay Api 回應錯誤")
```
