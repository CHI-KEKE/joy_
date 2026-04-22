# QFPay — RefundQuery 退款查詢流程分析

## 重要說明：與 QueryPayment 使用相同 API

RefundQuery 呼叫的端點與 QueryPayment **完全相同**：`POST /trade/v1/query`  
但傳入的參數更精簡，且結果解析邏輯也有所不同。

---

## 流程概覽

```
PMW → POST /trade/v1/query（帶退款 TransactionId 或 OrderCode）
       ↓
   QFPay 回傳交易紀錄
       ↓
   找出 out_trade_no == ExtendInfo.OrderCode 的第一筆
       ↓
   解析該筆的 responseCode → 對應 ReturnCode
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

Request Body（只傳最小必要欄位）：

| 欄位（QFPay） | 說明 | 來源 |
|-------------|------|------|
| `syssn` | 退款 QFPay 交易編號 | `body.TransactionId`（退款時的 syssn） |
| `out_trade_no` | 退款訂單號（TS Code） | `ExtendInfo.OrderCode` |

> QueryPayment 傳了 9 個欄位，RefundQuery 只傳 2 個，其餘全部省略。

---

## 結果解析邏輯

```
Step 1：API level check
  └→ apiResponse == null OR responseCode != "0000"
     → throw ApplicationException("QFPay Api 回應錯誤")

Step 2：找出第一筆 out_trade_no == ExtendInfo.OrderCode 的紀錄
  └→ apiResponse.Data.FirstOrDefault(i => i.TradeOrderCode == OrderCode)

Step 3：解析 orderData.ResponseCode → ReturnCode（見 Refund/02-ReturnCode對照.md）
```

### 與 QueryPayment 的差異

| 項目 | QueryPayment | RefundQuery |
|------|-------------|------------|
| 過濾條件 | `order_type=payment` + `respcd=0000` | 無 order_type 過濾 |
| 取紀錄方式 | 先找成功再 fallback | 直接取第一筆 |
| SuccessCount 統計 | ✅ 有 | ❌ 無 |
| ReturnCode 依據 | orderData.responseCode + 正向過濾 | orderData.responseCode 直接對應 |

---

## ReturnCode 對照

完整對照請見 [Refund/02-ReturnCode對照.md](../Refund/02-ReturnCode對照.md)

| QFPay ResponseCode | ReturnCode |
|-------------------|-----------|
| `0000` | Success（退款完成） |
| `1124` / `1145` / `1143` / `1265` / `1269` | RefundPending |
| `1265` + 特殊描述 | Failed（FPS 超過 29 天限制） |
| `1250` / `1297` | RefundPeriodExceeded |
| 其他（含 `1181`） | Failed |

---

## RefundQueryResponseExtendInfo 欄位

| 欄位 | 來源 | 說明 |
|------|------|------|
| `Page` | `apiResponse.Page` | 頁碼 |
| `PageSize` | `apiResponse.PageSize` | 每頁筆數 |
| `ResponseCode` | `apiResponse.ResponseCode` | API level 回應碼 |
| `ResponseDescription` | `apiResponse.ResponseDescription` | API 描述 |
| `Data` | `apiResponse.Data[]` | 所有交易紀錄 |
| `RawData` | `apiResponse` | 原始回應物件 |

> 與 QueryPayment 回傳的 `QFPayQueryResponseEntity` 相同結構，但 `SuccessCount` 不會被設定（預設 0）。

---

## 情境分析

### 情境 1：退款確認成功
```
body.TransactionId = "refund_syssn_xxx"
ExtendInfo.OrderCode = "TS123456"
→ POST /trade/v1/query
→ 找到 out_trade_no=TS123456 的紀錄，respcd=0000
→ ReturnCode = 1000 (Success)
```

### 情境 2：退款仍處理中
```
→ 找到記錄，respcd=1145（RequestProcessing）
→ ReturnCode = RefundPending
→ 系統排程繼續輪詢
```

### 情境 3：FPS 超過退款期限
```
→ 找到記錄，respcd=1265，responseDescription 含「超出可退款時限」
→ ReturnCode = Failed（不再重試）
```
