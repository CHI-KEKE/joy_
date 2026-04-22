# 2C2P — 付款規格

## 支援的支付方式

- 信用卡付款（Visa, Mastercard）
- Alipay Hong Kong

<br>

## 支付方式識別

ERP 第三方金流付款查詢紀錄欄位（根據「信用卡一次付清」、「信用卡分期付款」、「AlipayHK」）

### 支付方案代碼對照表

| 支付方式 | agentCode | channelCode | paymentScheme |
|----------|-----------|-------------|---------------|
| 信用卡一次付清 | UOBT | VI | VI |
| 信用卡分期付款 | UOBT | VI | VI |
| Alipay HK | ALIPAY | AH | AH |

### 回應格式範例

```json
{
    "AgentCode": "UOBT",
    "ChannelCode": "VI",
    "CardType": "CREDIT",
    "IssuerCountry": "US",
    "IssuerBank": "FIRST DATA CORPORATIONS",
    "InstallmentPeriod": null,
    "PaymentScheme": "VI"
}
```

> 參考文件：[2C2P Payment Scheme Codes](https://developer.2c2p.com/docs/reference-codes-payment-scheme)

<br>

## Error Codes — 付款與查詢

| 代碼 | 描述 | 說明 |
|------|------|------|
| 0000 | Successful | 付款成功 |
| 2001 | Transaction in progress | 取得付款連結 |
| 0003 | Transaction is cancelled | 用戶取消付款 |
| 4081 | Unable to Authenticate Card Holder | 3D 驗證失敗或取消 |

> 完整 Error Codes 請參考：[Response Codes](https://developer.2c2p.com/docs/response-code-payment)
