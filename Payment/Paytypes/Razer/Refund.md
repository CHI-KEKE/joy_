# Razer — 退款

## 注意事項

- Razer **不走匯款**，採連路退（直接原路退回）

<br>

## Refund API 回應

| 狀態 | API Status | 說明 |
|------|------------|------|
| 退款成功 | `success` | - |
| 退款失敗 | `rejected` | 特定 ErrorCode 會觸發 Retry |
| 退款未定 | `pending` / `processing` | 需透過退款查詢 API 再次確認 |

<br>

## ErrorCode — Retry 機制

**自動 Retry（系統處理）：**
- `PR015`
- `PR020`

**需人工介入（跳 Slack 通知 RD）：**
- `PR001` ~ `PR014`
- `PR016` ~ `PR010`（原文如此，疑似筆誤，待確認正確範圍）
- `INQ001` ~ `INQ006`
- `INQ011`

<br>

## 支援範圍

適用於以下所有 Razer 付款方式：

**馬來西亞：**
- 信用卡一次付清（CreditCardOnce_Razer）
- 信用卡分期付款（CreditCardInstallment_Razer）
- OnlineBanking
- GrabPay、Touch 'n Go、Boost

**新加坡：**
- 信用卡一次付清
- GrabPay
