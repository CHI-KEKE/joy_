# QFPay 退款流程

## 基本資訊

| 項目 | 值 |
|------|-----|
| PayChannel | `QFPay` |
| 退款路線 | `RefundByRequestId`（逐筆退款） |
| CanGroupingRefund | `false` |
| NMQ JobName | `QFPayRefundRequestFinish` |

---

## 退款判斷邏輯

### IsQueryRefund

```csharp
// TransactionId 有值 → 查詢；無值 → 直接退款
public override bool IsQueryRefund(RefundRequest refundRequest)
{
    return !string.IsNullOrWhiteSpace(refundRequest.RefundRequest_TransactionId);
}
```

- `TransactionId == null` → 走 **Refund**（新退款）
- `TransactionId 有值` → 走 **RefundQuery**（查詢之前的退款結果）

### IsRefund

未 override → 預設 `true`（只要不是 QueryRefund 就直接退款）

---

## 打 API 時傳入的額外資訊

### Refund（新退款）ExtendInfo

```csharp
{
    "orderCode": "{TradesOrderSlaveCode}-{RefundRequest_Id}",  // 不可重複，用 SlaveCode+RequestId 組合
    "requestDatetime": DateTime.Now,
    "isUsingLiveTest": bool  // 是否使用 LivingTest 環境（QA 商店分兩種環境）
}
```

### RefundQuery ExtendInfo

```csharp
{
    "orderCode": "{TradesOrderSlaveCode}-{RefundRequest_Id}",
    "isUsingLiveTest": bool
}
```

### GetTransactionId（RefundQuery 查詢用）

```csharp
// QFPay 用退款交易單號查詢（不是原始付款 TransactionId）
return refundRequest.RefundRequest_TransactionId;
```

> ⚠️ 其他金流通常用 `SalesOrderThirdPartyPayment_TransactionId`（原始付款 TxnId），
> QFPay 特殊：用的是 `RefundRequest_TransactionId`（退款時 Razer/QFPay 回傳的退款單號）

---

## HTTP Headers

從 ShopSecret `QFPay` group 取得：

```
X-Api-Key: {QFPay_APIKey}
X-Api-Code: {QFPay_APICode}
```

共用 AbstractPayChannelService 的 base headers（所有金流都有）：
```
ny-request-id
ny-sender
ny-pay-type
ny-trades-order-group-code
```

---

## 退款延時

```csharp
public const int RefundDelayTime = 30000;  // 30 秒

public override void RefundDelay()
{
    // 原本建議 30 sec，避免重複退款錯誤
    // 同一 TGCode 多筆退款時，每筆之間 sleep 30 秒
    Thread.Sleep(RefundDelayTime);
}
```

> ⚠️ **已知問題**：QFPay 曾至少 3 次發生「仍有處理中的退款請求，請勿重複提交」錯誤。
> 程式碼 comment 提到「先優化成 3 sec」但 const 仍為 30000（30s），請確認實際執行值。

---

## LivingTest 環境

QA 商店分為：
- **LivingTest 環境**：僅支援第三方支付（AlipayHK、WechatPay 等）
- **SandBox 環境**：一般測試

設定來源：
```
AppSetting: "PayChannel.QFPay.LiveTestingEnvironment"
格式: "false|none|none"（或 "true|shopId1,shopId2|none"）
```

---

## 重試機制

`GetRedoRefundRequestList()` → `null`（無 redo 重試機制）

---

## 關鍵檔案

| 檔案 | 路徑 |
|------|------|
| `QFPayPayChannelService.cs` | `ERP/Backend/BLV2/PayChannel/` |
| `PaymentMiddleWareRefundRequestService.cs` | `ERP/Backend/BLV2/ThirdPartyPayments/` |
