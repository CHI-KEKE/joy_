# QFPay — 付款規格

## 技術架構特性

| 類別 | 項目 | 說明 |
|------|------|------|
| **URL 組裝** | Redirect URL 自組 | QFPay 需自行組裝跳轉網址，組裝錯誤可能導致使用者停留在 `QFSign_Error` 頁面，透過付款成功率與逾時比例進行監控 |
| **環境配置** | QA 測試環境 | Live/Testing (第三方支付) / SandBox (信用卡) |
| **逾期機制** | UI 頁面逾期 | `checkoutExpiredTime` 參數設定付款頁面逾期時間戳記 |
| **逾期機制** | QR Code 逾期 | `expiredTime` 參數控制 QR Code 有效期限 |
| **交易紀錄** | 系統行為 | 未達 3DS 驗證或通道未拒絕時，QFPay 系統不產生訂單紀錄 |
| **資料對應** | 交易識別 | `syssn` 對應 TransactionId |

<br>

## 平台整合規格

| 支付方式 | 整合方式 | 技術規格 |
|----------|----------|----------|
| **信用卡支付** | WebView | 標準網頁內嵌顯示 |
| **第三方支付** | App 跳轉 | 使用 `{appInitPath}-s{shopId:D6}://thirdpartypayconfirm?url={encodedUri}` 協議 |

<br>

## 使用者介面行為

| 平台 | 倒數計時顯示 | 實際行為 |
|------|-------------|----------|
| **桌面端** | 正常顯示 | 倒數結束後跳轉 |
| **行動端** | 不顯示 | 後台倒數，時間到自動跳轉 |

<br>

## 訂單處理邏輯

| 情境 | 系統行為 |
|------|----------|
| **重複付款** | 偵測到訂單已付款，直接執行跳轉 |
| **第三方失敗** | 目前未發現訂單失敗後無法繼續付款的情境 |

<br>

## 金額限制規範

| 限制類型 | 規則 | 備註 |
|----------|------|------|
| **最低金額** | 無官方限制 | 測試確認 $0.5 可正常交易 |
| **最高金額** | 系統限制 | 超限錯誤：銀行系統繁忙，請耐心等待，建議稍後重試或使用其他支付方式 (1297) |

<br>

## Pay Page URL 範例

```
https://openapi-int.qfapi.com/checkstand/#/?appcode=51E1B3648E92428A8507BFE0918ED042&out_trade_no=TG240618K00001&paysource=91App_checkout&return_url=https://shop2.shop.qa1.hk.91dev.tw/V2/PayChannel/Default/QFPay/TG240618K00001?shopId=2&k=c44bc1b7-45e5-4d13-9a0f-c7658b1ee6fd&lang=zh-HK&sign_type=sha256&txamt=4400&txcurrcd=HKD&txdtm=0001-01-01 12:00:00&sign=888e5e6d66f0afb4b7b467ebb17673bef9fe6b299d60ed932672be18eeb5e90f
```
