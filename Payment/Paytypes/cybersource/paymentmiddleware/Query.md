# Cybersource — QueryPayment 付款查詢流程分析

## 核心設計：雙路徑查詢

Cybersource QueryPayment 有兩條完全不同的執行路徑，由 request 的 `ExtendInfo` 是否包含 `TradesOrderGroupCode` 決定走哪條。

```
QueryPayment 入口
│
├── ExtendInfo 有 TradesOrderGroupCode？
│   ├── Yes → 【路徑 A】Create Search API 查詢
│   └── No  → 【路徑 B】Form-Data 簽名驗證與解析
```

> **背景說明**：Cybersource 的 Create Search API 有同步資料延遲（付款剛完成時查不到），  
> 因此設計了路徑 B 讓前台直接將 Cybersource 導回的 form-data 帶入 PMW 解析，  
> 跳過查詢延遲問題，確保付款後能即時進入完成頁。

---

## 路徑 A：Create Search API 查詢

### 觸發條件
`request.ExtendInfo` 包含 key `"TradesOrderGroupCode"`（即 TG Code）

### API 呼叫
```
POST /tss/v2/searches
Authorization: HTTP_SIGNATURE（見下方說明）
Content-Type: application/json

{
  "query": "clientReferenceInformation.code:{TG Code}"
}
```

### 認證方式：HTTP Signature
由 `HttpSignatureHelper.GenerateHttpSignatureHeaders()` 產生，使用：
- `x-merchant-id` — Merchant ID
- `x-merchant-key-id` — Key ID
- `x-secret-key` — 簽名密鑰

### 回傳結果判斷邏輯

```
搜尋結果（TransactionSummaries）

Step 1：有無資料？
  └── 無資料（_embedded 為 null 或空）→ WaitingToPay (2003)
       ⚠️ 代表查詢時付款資料尚未同步

Step 2：是否付款成功？
  └── 找到任一筆 TransactionSummary 同時滿足：
        - applicationInformation.applications[] 中有 name == "ics_bill"
        - applicationInformation.reasonCode == "100"
      → Success (1000)
      → TransactionId = 該筆 transaction.id

Step 3：是否確定失敗？
  └── 找到任一筆 TransactionSummary 同時滿足：
        - reasonCode != "100"
        - applicationInformation.rFlag == "ESYSTEM"
      → Failed (3000)
      → ReturnMessage = 各 application 的 name + reasonCode + rMessage 串接

Step 4：其他情況 → WaitingToPay (2003)
  ⚠️ 包含 3D 驗證失敗等情境，Cybersource 允許消費者在其頁面重試
```

### 成功判定特殊說明

> Cybersource 官方說明：  
> 「需在 `_embedded.transactionSummaries[].applicationInformation.applications[]` 中找到 `name == "ics_bill"` 且 `reasonCode == 100`，才代表 capture 成功。」

**為什麼需要這麼嚴格的判斷？**  
Secure Acceptance 付款失敗時不一定會導回商戶頁面，消費者可在 Cybersource 頁面重試，  
導致一筆訂單的搜尋結果中同時存在成功與失敗的 transaction records。  
因此**必須先檢查是否有成功，才能判斷失敗**，避免誤判。

### 失敗判定：只認 ESYSTEM

只有 `rFlag == "ESYSTEM"` 才視為確定失敗，其他失敗狀態（如 3D 驗證失敗）一律維持 WaitingToPay，  
允許消費者在 Cybersource 頁面重試。

---

## 路徑 B：Form-Data 簽名驗證與解析

### 觸發條件
`request.ExtendInfo` **不**包含 `"TradesOrderGroupCode"`（由 Cybersource 導回的 form-data 帶入）

### 執行步驟

**Step 1：HMAC-SHA256 簽名驗證**
```csharp
var signature = SecureAcceptanceHelper.sign(config.ProfileSecret, request.ExtendInfo);
bool isValid = signature == request.ExtendInfo.Get("signature");
if (isValid == false)
{
    throw new SecurityException("'signature' is not match");
}
```
> 驗簽失敗直接拋 `SecurityException`，防止偽造 callback。

**Step 2：解析 `decision` 欄位**

| `decision` 值 | ReturnCode | 說明 |
|--------------|-----------|------|
| `ACCEPT` | `1000` Success | 付款成功 |
| `DECLINE` | `3000` Failed | 發卡行拒絕 |
| `ERROR` | `3000` Failed | 系統錯誤 |
| `CANCEL` | `3000` Failed | 消費者取消 |
| 其他 | `2003` WaitingToPay | 未知狀態 |

**Step 3：組建回應**

| 欄位 | 來源 |
|------|------|
| `TransactionId` | `request.ExtendInfo["transaction_id"]` |
| `ReturnMessage` | `request.ExtendInfo["message"]` |
| `ExtendInfo.Source` | `"FormData"` |
| `ExtendInfo.card_type_name` | `request.ExtendInfo["card_type_name"]` |
| `ExtendInfo.req_payment_method` | `request.ExtendInfo["req_payment_method"]` |

---

## ReturnCode 對照表

| ReturnCode | 說明 | 觸發情境 |
|-----------|------|---------|
| `1000` | Success | 付款成功（A路徑: ics_bill + reasonCode 100；B路徑: decision=ACCEPT） |
| `2003` | WaitingToPay | 無資料（同步延遲）、非確定性失敗（如 3D 重試）、未知 decision |
| `3000` | Failed | 確定失敗（A路徑: ESYSTEM；B路徑: DECLINE/ERROR/CANCEL） |

---

## 情境分析

### 情境 1：付款剛完成，前台立刻用 Form-Data 查詢（路徑 B）
```
Cybersource redirect → 前台收到 form-data（含 decision=ACCEPT, signature）
前台 → POST /QueryPayment（ExtendInfo = form-data）
PMW 驗簽 → decision=ACCEPT → ReturnCode=1000（Success）
```

### 情境 2：後台 recheck 用 TG Code 查詢（路徑 A）
```
後台 → POST /QueryPayment（ExtendInfo = {TradesOrderGroupCode: "TG123456"}）
PMW → POST /tss/v2/searches（query="clientReferenceInformation.code:TG123456"）
找到 ics_bill + reasonCode=100 → ReturnCode=1000（Success）
```

### 情境 3：查詢時付款資料尚未同步（路徑 A）
```
後台 → POST /QueryPayment（TG Code）
PMW → POST /tss/v2/searches
回傳 _embedded 為空 → ReturnCode=2003（WaitingToPay）
⚠️ 系統會排程重試，直到超時
```

### 情境 4：3D 驗證失敗，消費者在 Cybersource 頁面重試（路徑 A）
```
搜尋結果有失敗 transaction（3D 失敗）但沒有 ESYSTEM
→ 沒有 ics_bill + reasonCode=100
→ rFlag 非 ESYSTEM
→ ReturnCode=2003（WaitingToPay）
✅ 消費者可重新輸入卡號嘗試
```

### 情境 5：簽名驗證失敗（路徑 B）
```
前台帶入的 form-data 被篡改或 signature 不符
→ throw SecurityException("'signature' is not match")
→ PMW 回傳錯誤，不處理付款結果
```

---

## QueryPaymentResponseExtendInfo 欄位說明

| 欄位 | 路徑 A | 路徑 B |
|------|-------|-------|
| `Source` | `"Query"` | `"FormData"` |
| `card_type_name` | （空字串） | Cybersource form-data 中的 `card_type_name` |
| `req_payment_method` | （空字串） | Cybersource form-data 中的 `req_payment_method` |
| `paymentInformation_paymentType_type` | `transaction.PaymentInformation.PaymentType.Type` | （空字串） |
| `paymentInformation_paymentType_method` | `transaction.PaymentInformation.PaymentType.Method` | （空字串） |
