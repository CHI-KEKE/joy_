# Cybersource — Refund 退款流程分析

## 流程概覽

```
PMW → POST pts/v2/payments/{transactionId}/refunds/
       ↓
    Cybersource 處理
       ↓
    解析回應 status → 回傳 ReturnCode
```

Pay 階段是表單導向（不打 API），但 **Refund 是標準的 server-to-server REST API 呼叫**。

---

## API 呼叫

```
POST pts/v2/payments/{transactionId}/refunds/
Authorization: HTTP_SIGNATURE
Content-Type: application/json

{
  "clientReferenceInformation": {
    "code": "{TradesOrderGroupCode}"
  },
  "orderInformation": {
    "amountDetails": {
      "totalAmount": "{Amount}",
      "currency": "{Currency}"
    }
  }
}
```

| 欄位 | 來源 |
|------|------|
| `transactionId`（URL） | `request.TransactionId`（付款時的 Cybersource transaction ID） |
| `clientReferenceInformation.code` | `request.ExtendInfo.TradesOrderGroupCode`（TG Code） |
| `orderInformation.amountDetails.totalAmount` | `request.Amount` |
| `orderInformation.amountDetails.currency` | `request.Currency` |

---

## 認證方式：HTTP Signature

與 QueryPayment 路徑 A 相同，由 `HttpSignatureHelper.GenerateHttpSignatureHeaders()` 產生：

| Header 欄位 | 用途 |
|------------|------|
| `x-merchant-id` | Merchant ID |
| `x-merchant-key-id` | Key ID |
| `x-secret-key` | HMAC 簽名密鑰 |

---

## 回應 Status 對應邏輯

```csharp
switch (this.Status)
{
    case "PENDING":   → ReturnCodes.RefundPending
    case "500":       → ReturnCodes.RefundFailed（含 errorInformation）
    case "DECLINED":  → ReturnCodes.RefundFailed（含 errorInformation）
    default:          → throw NotImplementedException
}
```

| Status | ReturnCode | ReturnMessage |
|--------|-----------|--------------|
| `PENDING` | RefundPending | `this.Message` |
| `500` | RefundFailed | `ErrorInformation.Reason + Details[0].Reason + ErrorInformation.Message` |
| `DECLINED` | RefundFailed | `ErrorInformation.Reason + Details[0].Reason + ErrorInformation.Message` |
| 其他 | **拋例外** `NotImplementedException` | — |

> ⚠️ 未知 status 會 throw `NotImplementedException`，需注意若 Cybersource 新增狀態會造成系統異常。

---

## 特殊設計：HTTP 錯誤不拋出

```csharp
try
{
    return await base.PostJsonAsync<CybersourceRefundResponseEntity>(url, body, authHeaders);
}
catch (FlurlHttpException ex)
{
    return await ex.GetResponseJsonAsync<CybersourceRefundResponseEntity>();
}
```

**即使 HTTP Status 非 2xx（如 400、500），也不拋出例外，改為直接解析 response body。**

原因：Cybersource 退款被拒時（如 `DECLINED`）會回傳 HTTP 201（Created）但 body 中的 status 為 `DECLINED`；  
部分錯誤情境也可能回傳非 2xx，仍需解析 errorInformation。

---

## RefundRequestExtendInfo

```csharp
public class RefundRequestExtendInfo
{
    public string TradesOrderGroupCode { get; set; }  // TG Code，帶入 clientReferenceInformation.code
}
```

> `RefundResponseExtendInfo` 為空類別，退款回應不攜帶額外擴充資訊。

---

## 情境分析

### 情境 1：退款受理中
```
Cybersource 回傳：{ "status": "PENDING", "message": "Pending" }
→ ReturnCode = RefundPending
→ 系統定時執行 RefundQuery 確認最終結果
```

### 情境 2：退款被發卡行拒絕（DECLINED）
```
Cybersource 回傳：{
  "status": "DECLINED",
  "errorInformation": {
    "reason": "PROCESSOR_DECLINED",
    "message": "Decline - General decline of the card."
  }
}
→ ReturnCode = RefundFailed
→ ReturnMessage = "PROCESSOR_DECLINED  Decline - General decline of the card."
```

> 實際異常案例：`UNAUTHORIZED_CARD`（非活躍卡或不支援無卡交易）、`PROCESSOR_DECLINED`（發卡行一般性拒絕）  
> 參考：`異常紀錄/02-退款失敗異常處理.md`

### 情境 3：HTTP 500 系統錯誤
```
Cybersource 回傳 HTTP 500，body 包含：{ "status": "500", "errorInformation": {...} }
→ FlurlHttpException 被 catch，解析 body
→ ReturnCode = RefundFailed
```

---

## ReturnCode 對照

| ReturnCode | 說明 |
|-----------|------|
| RefundPending | 退款處理中，需 RefundQuery 後續追蹤 |
| RefundFailed | 退款失敗（發卡行拒絕或系統錯誤） |
