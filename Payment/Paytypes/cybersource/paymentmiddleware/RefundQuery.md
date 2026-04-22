# Cybersource — RefundQuery 退款查詢流程分析

## 流程概覽

```
PMW → GET tss/v2/transactions/{RefundRequestTransactionId}
       ↓
    Cybersource 回傳 transaction 詳細資訊
       ↓
    解析 applicationInformation.status / reasonCode → 回傳 ReturnCode
```

RefundQuery 的目的是確認 Refund 階段回傳 `PENDING` 的退款是否最終完成。

---

## API 呼叫

```
GET tss/v2/transactions/{RefundRequestTransactionId}
Authorization: HTTP_SIGNATURE
```

| 欄位 | 來源 |
|------|------|
| `RefundRequestTransactionId`（URL） | `request.ExtendInfo.RefundRequestTransactionId`（退款時 Cybersource 回傳的 transaction ID） |

> 注意：此處用的是**退款的** transaction ID，不是原始付款的 transaction ID。

---

## 認證方式：HTTP Signature

```csharp
var authHeaders = HttpSignatureHelper.GenerateHttpSignatureHeaders(url, HttpMethod.Get, null, config);
```

GET 請求 body 為 `null`，其餘同 Refund 的 HTTP Signature 認證方式。

---

## 回應 Status 對應邏輯

判斷依據為 `applicationInformation.status`：

```csharp
switch (this.ApplicationInformation.Status)
{
    case "PENDING":      → ReturnCodes.RefundPending
    case "TRANSMITTED":  → ReturnCodes.Success
    default:
        → reasonCode == "100" ? RefundPending : RefundRejected
}
```

| `applicationInformation.status` | reasonCode | ReturnCode | 說明 |
|--------------------------------|-----------|-----------|------|
| `PENDING` | 任意 | RefundPending | 退款仍在處理中 |
| `TRANSMITTED` | 任意 | Success | 退款已傳送至發卡行，確認完成 |
| 其他 | `"100"` | RefundPending | 異常狀態但 reasonCode 成功，仍視為處理中 |
| 其他 | 非 `"100"` | RefundRejected | 退款被拒絕 |

---

## 特殊設計說明

### 為什麼 TRANSMITTED 才算成功，而不是 PENDING？

Cybersource 退款流程：
```
退款請求 → PENDING（受理）→ TRANSMITTED（已傳送至發卡行）→ 最終到帳
```

`PENDING` 僅代表 Cybersource 受理，尚未傳出去；`TRANSMITTED` 才代表實際退款動作已執行。

---

### reasonCode 判斷的 fallback 邏輯

當 status 不是 PENDING 或 TRANSMITTED 時，用 `reasonCode == "100"` 作為補充判斷：

> 參考：[Cybersource Reason Code 說明](https://ebc2test.cybersource.com/content/ebc/help/cybs/zh-tw/business-center/user/all/cybs/ebc2_olh/trns_mngmt_intro/view_details/details/reason_code_descriptions.html)
>
> `reasonCode = 100` 代表交易成功

若 reasonCode 為 100 但 status 為非預期值，視為仍在處理中（RefundPending），  
若 reasonCode 也非 100，則視為退款被拒絕（RefundRejected）。

---

## QueryRefundRequestExtendInfo

```csharp
public class QueryRefundRequestExtendInfo
{
    [JsonPropertyName("RefundRequestTransactionId")]
    public string RefundRequestTransactionId { get; set; }  // 退款 transaction ID
}
```

> `QueryRefundResponseExtendInfo` 為空類別，退款查詢回應不攜帶額外擴充資訊。

---

## 情境分析

### 情境 1：退款仍在處理中
```
Cybersource 回傳：{ applicationInformation: { status: "PENDING" } }
→ ReturnCode = RefundPending
→ 系統下次 RefundQuery 循環繼續查詢
```

### 情境 2：退款最終成功
```
Cybersource 回傳：{ applicationInformation: { status: "TRANSMITTED" } }
→ ReturnCode = Success
→ 退款流程完結
```

### 情境 3：退款查詢循環過長（已知異常）
```
系統持續 RefundQuery → 每次都回 PENDING
→ 直到系統 timeout 中斷
→ 最後一次查詢結果為 RefundPending (4003)
→ 系統 Redo，最終 TRANSMITTED 退款成功
```
> 原因：Cybersource 退款查詢回應時間可能需要數小時。  
> 參考：`異常紀錄/01-RefundQuery查詢循環異常.md`

### 情境 4：退款被拒絕
```
Cybersource 回傳：{ applicationInformation: { status: "REFUSED", reasonCode: "201" } }
→ 非 PENDING / TRANSMITTED，reasonCode 非 100
→ ReturnCode = RefundRejected
```

---

## ReturnCode 對照

| ReturnCode | 說明 |
|-----------|------|
| RefundPending | 退款處理中，下次 RefundQuery 繼續確認 |
| Success | 退款已確認傳送至發卡行 |
| RefundRejected | 退款最終被拒絕 |
