# OnlineBanking Razer 退款異常處理

## 問題描述

`RefundRequest_BankName` 為空 + `RefundRequest_TypeDef = Remittance` 的退款記錄，每輪 NMQ 排程都會被撈出但被 skip 跳過，形成無限循環，退款永遠無法完成。

---

## 核心邏輯（scmnmq）

**檔案**：`PaymentMiddleWareRefundRequestService.cs`（L252-258）

```csharp
if (IsNullOrWhiteSpace(BankName) && TypeDef == "Remittance")
{
    status = RefundRequestProcessing; // hold 住，等補填
    continue; // 跳過不打 API
}
```

| BankName | TypeDef | 結果 |
|---|---|---|
| **空** | Remittance | 設為 `RefundRequestProcessing`，排程跳過（無限循環 ⚠️） |
| **有值** | Remittance | 條件 false → 繼續打 Razer API ✅ |
| **有值** | 任意 | 正常打 API ✅ |

---

## Remittance TypeDef 的兩種來源

1. **消費者主動選擇「匯款退款」**（mweb 前台取消申請時選擇）
2. **Razer 回傳 `RefundPeriodExceeded`**（退款期限已過）→ `GetRefundRequestType()` 強制回傳 `Remittance`

> ⚠️ Razer OnlineBanking 不走傳統線下匯款，Remittance TypeDef 仍需打 Razer API，並由 Razer 將款項匯入消費者銀行帳戶。

---

## BankName 資料流

```
消費者在 mweb 填寫取消申請（refundInfoForm.tsx）
    ↓
CancelRequestService.cs
    CancelRequest_BankName = confirmEntity.RefundInfo.BankName

    ↓
ThirdPartyRefundRequestService.CreateRefundRequestByCancelFlow()
    RefundRequest_BankName = cancelOrderSlave.CancelOrderSlave_BankName

    ↓
RazerPayChannelService.GetRefundExtendInfo()
    BeneficiaryName = refundRequest.RefundRequest_BankName
    BeneficiaryAccNo = refundRequest.RefundRequest_BankAccountNo
    → 呼叫 Razer Refund API
```

**BankName 來源**：mweb 前台（消費者填寫）；SMS 後台**沒有**設定 BankName 的地方。

---

## 前台是否必填確認

**檔案**：`CancelRequestService.IsRefundInfoCollectingEnabled()`

- OnlineBanking_Razer → `return true`（必填）
- 若未填則回傳 `API0004` 錯誤，**無法送出取消申請**

**例外**：商店設定 `AlternativeRefundTypeDef = StoreCredit`（退購物金）→ 消費者選退購物金時，**可跳過**銀行資訊驗證

---

## 消費者補填入口

**API**：`MemberTradesOrderController.UpdateRefundInfo()`（mweb）
- 需登入（`[RequireLogin]`）
- 路徑：訂單頁面 → 我的訂單 → 填寫退款資料
- 驗證後呼叫 `UpdateMemberTradesOrderRefundDetail()` 更新 `RefundRequest_BankName`

**Email 通知**：`RefundRequestAccountNoMailService`
- 系統自動寄出「退款資料帳號通知」給消費者
- 內容：「請至 My Orders → Refund Information 補填銀行資訊」
- 含連結 `$(OrderRefURL)` 直接帶到訂單頁

---

## 處理步驟

```
[系統偵測到 BankName 空 + Remittance]
        ↓
系統寄 email 通知消費者補填
        ↓（若消費者未收到或未填）
商店 CS 主動聯繫消費者：
  「請前往訂單頁面 → 我的訂單 → 填寫退款資料，輸入銀行帳號」
        ↓
消費者在 mweb 補填 BankName + BankAccountNo
        ↓
RefundRequest_BankName 有值
        ↓
下次 NMQ 排程：skip 條件不成立 → 呼叫 Razer API → 退款完成 ✅
```

---

## 相關檔案

| 檔案 | 說明 |
|---|---|
| `scmnmq/PaymentMiddleWareRefundRequestService.cs` | 核心退款排程，BankName 空值跳過邏輯（L252-258） |
| `scmnmq/RazerPayChannelService.cs` | 組建 Razer API 銀行資訊（`GetRefundExtendInfo()`） |
| `scmnmq/ThirdPartyRefundRequestService.cs` | 從 CancelOrderSlave 建立 RefundRequest（`CreateRefundRequestByCancelFlow()`） |
| `mweb/CancelRequestV2Controller.cs` | 取消申請，含銀行資訊必填驗證 |
| `mweb/CancelRequestService.cs` | `IsRefundInfoCollectingEnabled()` 判斷是否必填 |
| `mweb/MemberTradesOrderController.cs` | `UpdateRefundInfo()` 消費者補填退款銀行資訊 API |
