# Razer

Razer（舊稱 MOLPay，新品牌為 Fiuu）為東南亞地區的金流服務商，支援信用卡、分期付款、電子錢包及 OnlineBanking 等付款方式。

<br>

## 文件索引

| 主題 | 檔案 |
|------|------|
| 環境設定與商戶帳號 | [Environment.md](./Environment.md) |
| 付款規格與 txn_channel 對應 | [Payment.md](./Payment.md) |
| 付款錯誤情境 | [PaymentErrors.md](./PaymentErrors.md) |
| 退款機制與 ErrorCode | [Refund.md](./Refund.md) |
| PayNow（新加坡 QR Code 轉帳） | [PayNow.md](./PayNow.md) |
| Fiuu 新品牌整合與 Feature Toggle | [Fiuu.md](./Fiuu.md) |
| 分期付款（系統設計） | [Installment/](./Installment/) |
| 異常紀錄 | [異常紀錄/](./異常紀錄/) |

<br>

## 支援付款方式

| 付款類型 | PayProfileType | 地區 |
|----------|---------------|------|
| 信用卡一次付清 | `CreditCardOnce_Razer` | MY / SG |
| 信用卡分期付款 | `CreditCardInstallment_Razer` | MY |
| OnlineBanking | `OnlineBanking_Razer` | MY |
| GrabPay | `GrabPay_Razer` | MY / SG |
| Boost | `Boost_Razer` | MY |
| Touch 'n Go | `TNG_Razer` | MY |
| PayNow | `PayNow_Razer` | SG |
