# WaitingThirdPartyPaymentCheck 訂單狀態

## 狀態意義

消費者完成結帳後，金流回呼尚未確認，訂單暫時停留於「等待第三方支付確認」的中間狀態。

---

## 何時觸發

**`csp_TradesOrderTransToSalesOrderWithFlow_Mall.sql` Line 1012-1032：**

```sql
CASE
    WHEN @payTypeDef IN (
        'LinePay', 'JKOPay', 'Atome',
        'PayNow_Razer', 'CreditCard_Razer', ...  -- 共 25 種第三方支付
    )
    THEN 'WaitingThirdPartyPaymentCheck'
    
    WHEN @market = 'HK' AND @payTypeDef IN ('ApplePay', 'GooglePay')
    THEN 'WaitingThirdPartyPaymentCheck'
    
    ELSE '其他狀態'
END
```

→ 轉單（`csp_TradesOrderTransToSalesOrderWithFlow_Mall`）時由 DB 設定。

---

## 前台顯示

**`ThirdpartyPaymentWaitingToPayMessage.tsx`：**

- 顯示文字：「**交易處理中**」
- Polling 機制：每 **30 秒**查詢一次支付狀態
- 最長等待：**10 分鐘**

---

## NMQ 處理

由 `ThirdPartyPaymentTransactionCheckProcess` 消費 NMQ 訊息，向第三方支付查詢結果：

```
WaitingThirdPartyPaymentCheck
    ↓
NMQ 訊息
    ↓
ThirdPartyPaymentTransactionCheckProcess
    ↓
查詢金流結果
    ↓ (成功)           ↓ (失敗/逾時)
OrderFlowFinish     CancelOrder / ErrorStatus
```

---

## 完整狀態流程

```
消費者結帳
    ↓
csp_TradesOrderTransToSalesOrderWithFlow_Mall
    ↓ (第三方支付)
WaitingThirdPartyPaymentCheck   ← 本狀態
    ↓
PayChannelController.PayChannelReturn()  ← 金流回呼
    ↓ (或 NMQ 主動查詢)
    ↓ 成功                ↓ 失敗
OrderFlowFinish        取消訂單 / 錯誤狀態
```

---

## 相關檔案

| 檔案 | 說明 |
|------|------|
| `csp_TradesOrderTransToSalesOrderWithFlow_Mall.sql` | 設定初始狀態 |
| `ThirdPartyPaymentTransactionCheckProcess.cs` | NMQ 消費者 |
| `ThirdpartyPaymentWaitingToPayMessage.tsx` | 前台「交易處理中」畫面 |
| `usePaymentStatus.tsx` | 前台 30 秒 polling 邏輯 |
| `PayChannelController.cs` | 金流回呼入口 |
