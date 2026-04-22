# QFPay — 退款

`out_trade_no` : TS_RefundRequestId

<br>

## 一般規則

- 信用卡付款無法 `Cancel`
- 退款金費是扣除當天交易金額，所以可能會發生退款餘額不足的問題
- Visa / Mastercard 卡交易，系统规定，若是当日交易当日退款，需要全额申请，隔日退款才可以用部分退款 `For void request, it must be a full refund, please correct refund amount and re-initiate it`

<br>

## Refund Mapping

| 狀態名稱 | Return Code | 描述 | 91APP 狀態 |
|----------|-------------|------|------------|
| TransactionClosed | 1264 | 第三方顯示訂單已結束，通常發生在退款單重複申請 | 退款失敗 |
| Success | 0000 | 退款成功 | 退款成功 |
| SameDayPartialRefundRejected | 1124 | 信用卡不可當天部分退 | 退款處理中 |
| RequestProcessing | 1145 | 處理中請稍後 | 退款處理中 |
| RefundPeriodExceeded | 1265 | 該筆訂單被禁止退款，已經超出可退款時限 | 退款失敗 |
| RefundNotAllowedDuringBillingPeriod | 1265 | 11:30 PM～12:30 AM 帳期無法退款 | 退款處理中 |
| InsufficientRefundableBalance | 1269 | 帳戶餘額不足 | 退款處理中 |

<br>

## RefundQuery Mapping（Refund 後確認類型）

有 RefundRequest TransactionId，執行 RefundQuery 確認退款狀態

| 狀態名稱 | Return Code | 描述 | 91APP 狀態 |
|----------|-------------|------|------------|
| Success | 0000 | 退款成功 | 退款成功 |
| SameDayPartialRefundRejected | 1124 | 信用卡不可當天部分退 | 退款處理中 |
| InsufficientRefundableBalance | 1269 | 帳戶餘額不足 | 退款處理中 |
| RefundNotAllowedDuringBillingPeriod | 1265 | 11:30 PM～12:30 AM 帳期無法退款 | 退款處理中 |
| RequestProcessing | 1145 | 處理中請稍後 | 退款處理中 |
| TransactionClosed | 1264 | 第三方顯示訂單已結束，通常發生在退款單重複申請 | 退款失敗 |

<br>

## 關帳期限

| Code | Description | 關帳期間 (天) |
|------|-------------|--------------|
| 800201 | WeChat Merchant Presented QR Code Payment (MPM) (Overseas & HK Merchants) | 365 |
| 800212 | WeChat H5 Payment (In mobile browser) | 365 |
| 801512 | Alipay Online WAP Payment (HK Merchants) | 365 |
| 801514 | Alipay Online WEB Payment (HK Merchants) | 365 |
| 802001 | FPS Merchant Presented QR Code Payment (MPM) (HK Merchants) | 29 |
| 802801 | Visa / Mastercard Online Payments | 365 |
| 805812 | PayMe Online WAP (in mobile browser Chrome etc.) Payment (HK Merchants) | 90 |
| 805814 | PayMe Online WEB (in browser Chrome etc.) Payment (HK Merchants) | 90 |
| 未定義 | 未定義的例外處理 | 365（取最大值）|
