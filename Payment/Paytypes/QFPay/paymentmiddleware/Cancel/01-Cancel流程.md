# QFPay — Cancel 取消付款流程分析

## 流程概覽

```
PMW → POST /trade/v1/reversal
       ↓
   QFPay 回傳撤銷結果（ResponseCode）
       ↓
   0000 → Success，其他 → Failed
```

> Cancel（Reversal）與 Refund 的差異：  
> - **Cancel/Reversal**：用於取消**尚未結算**的交易（通常當日），資金直接撤銷不經退款流程  
> - **Refund**：用於退還**已結算**的交易

---

## API 呼叫

```
POST /trade/v1/reversal
Content-Type: application/x-www-form-urlencoded
X-QF-APPCODE: {apiCode}
X-QF-SIGN: {sha256_signature}
X-QF-SIGNTYPE: SHA256
```

Request Body（form-urlencoded，排序後簽名）：

| 欄位（QFPay） | 說明 | 來源 | 必填 |
|-------------|------|------|------|
| `syssn` | QFPay 原始交易編號 | `request.TransactionId` | ✅ |
| `out_trade_no` | 訂單代碼 | `ExtendInfo.OrderCode` | ✅ |
| `txamt` | 取消金額（× 100 最小單位） | `ExtendInfo.Amount * 100` | ✅ |
| `txdtm` | 原始交易時間（`yyyy-MM-dd hh:mm:ss`） | `ExtendInfo.TransactionDatetime` | ✅ |
| `udid` | 裝置 ID | `ExtendInfo.Udid` | ❌ |

---

## ReturnCode 判斷

```csharp
ReturnCode = apiResponse.ResponseCode == "0000"
    ? ReturnCodes.Success
    : ReturnCodes.Failed;
```

| QFPay ResponseCode | ReturnCode |
|-------------------|-----------|
| `0000` | Success |
| 其他（含 `1143`/`1145`） | Failed |

> ⚠️ Cancel 沒有 Pending 狀態，與 Refund 不同。  
> QFPay 文件說明 `1143`/`1145` 在 Cancel 時代表「處理中」，但程式碼直接視為 Failed，不做輪詢。

---

## Cancel 回傳結構

| PMW 欄位 | 來源 |
|---------|------|
| `TransactionId` | `apiResponse.CancelTransactionId`（新建的撤銷 syssn） |
| `ReturnCode` | `0000` → Success，其他 → Failed |
| `ReturnMessage` | `apiResponse.ResponseMessage` |
| `ExtendInfo` | 完整 `QFPayCancelResponseEntity` |

---

## CancelRequestExtendInfo 欄位

```csharp
public class QFPayCancelRequestExtendInfoEntity
{
    public string OrderCode { get; set; }         // 訂單代碼
    public decimal Amount { get; set; }           // 取消金額（原始金額，非最小單位）
    public DateTime TransactionDatetime { get; set; } // 原始交易時間
    public string? Udid { get; set; }             // 裝置 ID（非必填）
    public bool IsUsingLiveTest { get; set; }     // 環境切換
}
```

---

## 情境分析

### 情境 1：取消成功
```
→ POST /trade/v1/reversal
→ responseCode = "0000"
→ ReturnCode = Success
→ TransactionId = apiResponse.CancelTransactionId（撤銷交易的 syssn）
```

### 情境 2：取消失敗（如已結算）
```
→ POST /trade/v1/reversal
→ responseCode != "0000"
→ ReturnCode = Failed
→ ReturnMessage = apiResponse.ResponseMessage（失敗原因）
```
