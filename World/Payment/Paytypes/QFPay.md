# QFPay

![alt text](./Img/image.png)

QFPay 為支援多種付款方式的金流服務商，目前用 hosted payment pages 方式串接，需要自己使用 API Key 做 加密簽名組 url

## 目錄
1. [文件、環境](#文件環境)
2. [測試卡資訊](#測試卡資訊)
3. [付款](#付款)
4. [付款錯誤情境](#付款錯誤情境)
5. [退款](#退款)
6. [退款異常紀錄](#退款異常紀錄)

<br>

## 文件、環境

聯絡方式 : WhatsApp

[Paytypes](https://sdk.qfapi.com/docs/preparation/paycode/#paytype)
[Currencies](https://sdk.qfapi.com/docs/preparation/paycode#currencies)
[Transaction Status Codes](https://sdk.qfapi.com/docs/preparation/paycode#transaction-status-codes)
[Refund](https://test-openapi-th.qfapi.com/trade/v1/refund)
[問題清單](https://docs.google.com/spreadsheets/d/1F5wXCP7-w-_u2C7vpVbIP_GgMe75Y9kH2HM-rJH7vAQ/edit?gid=1016597365#gid=1016597365)
[開發文件](https://sdk.qfapi.com/)

<br>

#### 商戶後台登入

| 環境 | 用途 | 登入網址 |
|------|------|----------|
| Sandbox | 測試環境後台 | https://sh-int-hk.qfapi.com/ |
| Testing | 正式測試環境後台 | https://sh-hk.qfapi.com/#/login |

<br>

#### API 環境網址

| Environment Name | 用途說明 | API URL |
|------------------|----------|---------|
| Sandbox | 僅限信用卡模擬測試 | https://openapi-int.qfapi.com |
| Live Testing Environment | 正式測試環境 | https://test-openapi-hk.qfapi.com |
| Production | 正式生產環境 | https://openapi-hk.qfapi.com |

<br>
<br>

## 測試卡資訊

[測試卡文件](https://sdk.qfapi.com/docs/online-shop/visa-master-online-payment#test-cards)

<br>

| 卡別 | 卡號 | 預期結果 |
|------|------|----------|
| MasterCard | 5200000000001096 | 付款成功 |
| Visa | 4000000000001091 | 付款成功 |
| MasterCard | 5200000000001005 | 付款成功 (免 3D 驗證) |
| Visa | 4000000000001000 | 付款成功 (免 3D 驗證) |
| MasterCard | 5200000000001120 | 付款失敗 (at verification) |
| Visa | 4000000000001125 | 付款失敗 (at verification) |
| MasterCard | 5200000000001013 | 付款失敗 (at 3DS frictionless) |
| Visa | 4000000000001018 | 付款失敗 (at 3DS frictionless) |

<br>

#### 測試卡特殊行為說明

| 測試情境 | 卡別 | 卡號 | Return Code | 回應訊息 | 行為特徵 |
|----------|------|------|-------------|----------|----------|
| 付款失敗 (at verification) | MasterCard | 5200000000001120 | 1145 | 處理中，請稍等 | 1. 重新到相同付款頁仍然可以用正確卡號結帳轉導<br>2. QFPay 會有 2 筆紀錄<br>3. 屬於交易進行中情境<br>4. 跳轉 FailedUrl |
| 付款失敗 (at verification) | Visa | 4000000000001125 | 1145 | 處理中，請稍等 | 1. 重新到相同付款頁仍然可以用正確卡號結帳轉導<br>2. QFPay 會有 2 筆紀錄<br>3. 屬於交易進行中情境<br>4. 跳轉 FailedUrl |
| 付款失敗 (at 3DS frictionless) | MasterCard | 5200000000001013 | 1205 | 交易失敗，請稍後重试 | 1. 填卡頁會直接阻擋<br>2. 重新填正確卡號可以完成交易跳轉<br>3. 會有 2 筆資料 |
| 付款失敗 (at 3DS frictionless) | Visa | 4000000000001018 | 1205 | 交易失敗，請稍後重试 | 1. 填卡頁會直接阻擋<br>2. 重新填正確卡號可以完成交易跳轉<br>3. 會有 2 筆資料 |

<br>

## 付款


#### API 端點
`/api/v1.0/pay/QFPay/TG240618K00002`

<br>

#### 技術架構特性

| 類別 | 項目 | 說明 |
|------|------|------|
| **URL 組裝** | Redirect URL 自組 | QFPay 需自行組裝跳轉網址，組裝錯誤可能導致使用者停留在 `QFSign_Error` 頁面，透過付款成功率與逾時比例進行監控 |
| **環境配置** | QA 測試環境 | Live/Testing (第三方支付) / SandBox (信用卡) |
| **逾期機制** | UI 頁面逾期 | `checkoutExpiredTime` 參數設定付款頁面逾期時間戳記 |
| **逾期機制** | QR Code 逾期 | `expiredTime` 參數控制 QR Code 有效期限 |
| **交易紀錄** | 系統行為 | 未達 3DS 驗證或通道未拒絕時，QFPay 系統不產生訂單紀錄 |
| **資料對應** | 交易識別 | `syssn` 對應 TransactionId |

<br>

#### 平台整合規格

| 支付方式 | 整合方式 | 技術規格 |
|----------|----------|----------|
| **信用卡支付** | WebView | 標準網頁內嵌顯示 |
| **第三方支付** | App 跳轉 | 使用 `{appInitPath}-s{shopId:D6}://thirdpartypayconfirm?url={encodedUri}` 協議 |

<br>

#### 使用者介面行為

| 平台 | 倒數計時顯示 | 實際行為 |
|------|-------------|----------|
| **桌面端** | 正常顯示 | 倒數結束後跳轉 |
| **行動端** | 不顯示 | 後台倒數，時間到自動跳轉 |

<br>

#### 訂單處理邏輯

| 情境 | 系統行為 |
|------|----------|
| **重複付款** | 偵測到訂單已付款，直接執行跳轉 |
| **第三方失敗** | 目前未發現訂單失敗後無法繼續付款的情境 |

<br>

#### 金額限制規範

| 限制類型 | 規則 | 備註 |
|----------|------|------|
| **最低金額** | 無官方限制 | 測試確認 $0.5 可正常交易 |
| **最高金額** | 系統限制 | 超限錯誤：銀行系統繁忙，請耐心等待，建議稍後重試或使用其他支付方式 (1297) |
  
<br>

#### Pay Page Url

<br>

```
https://openapi-int.qfapi.com/checkstand/#/?appcode=51E1B3648E92428A8507BFE0918ED042&out_trade_no=TG240618K00001&paysource=91App_checkout&return_url=https://shop2.shop.qa1.hk.91dev.tw/V2/PayChannel/Default/QFPay/TG240618K00001?shopId=2&k=c44bc1b7-45e5-4d13-9a0f-c7658b1ee6fd&lang=zh-HK&sign_type=sha256&txamt=4400&txcurrcd=HKD&txdtm=0001-01-01 12:00:00&sign=888e5e6d66f0afb4b7b467ebb17673bef9fe6b299d60ed932672be18eeb5e90f
```

<br>

## 付款錯誤情境

#### 常見付款異常

| 錯誤類型 | 錯誤內容 | 系統行為 | 處理方式 |
|----------|----------|----------|----------|
| **3D 驗證失敗** | `Verification of PaRes failed` | 跳轉至交易進行中狀態 | 系統自動處理 |
| **信用卡驗證錯誤** | 卡號或資訊錯誤 | 直接阻擋交易 | 使用者重新輸入 |
| **付款逾期** | 超過 12 分鐘時限 | QFPay UI 倒數顯示，逾期後跳轉回 91APP | 重新發起付款 |

<br>

#### WeChat Pay 整合問題

| 問題情境 | 現象 | 根本原因 | 解決方案 |
|----------|------|----------|----------|
| **行動端 QR Code 限制** | 手機版僅顯示 QR Code | 未配置 H5 支付 | 請 QFPay 人員配置 H5 功能 |
| **Domain 配置錯誤** | H5 配置後出現商家參數異常 | QFPay 人員遺漏配置測試環境 Domain | 確認 `https://test-openapi-hk.qfapi.com` 已正確配置 |

<br>

#### 頁面組裝錯誤：QFPay Sign_Error

**常見原因與解決方案**

| 問題 | 解決方案 |
|------|----------|
| **ReturnUrl 編碼問題** | 確保 ReturnUrl 經過正確的 URL Encode 處理 |
| **API Key 異常** | 檢查 Merchant 設定，確認 Key 值正確性 |

<br>

#### 並行交易處理

**問題描述**
同一筆 TG 可能產生多筆交易紀錄，原因為使用者可開啟多個瀏覽器視窗同時進行付款

**處理邏輯**
1. **優先原則**：只要有成功紀錄，即視為該 TG 交易成功
2. **監控機制**：定期執行 Recheck 檢查重複成功交易
3. **後續處理**：發現多筆成功交易時，自動通知商店進行退款作業

<br>
<br>

## 退款


`out_trade_no` :　TS_RefundRequestId

<br>

#### 一般資訊

- 信用卡付款無法 `Cancel`
- 退款金費是扣除當天交易金額，所以可能會發生退款餘額不足的問題
- Visa / Mastercard 卡交易，系统规定，若是当日交易当日退款，需要全额申请，隔日退款才可以用部分退款 `For void request, it must be a full refund, please correct refund amount and re-initiate it`

<br>

####　Refund Mapping

<br>

| 狀態名稱 | Return Code | 描述 | 91APP 狀態 |
|----------|-------------|------|------------|
| TransactionClosed | 1264 | 第三方顯示訂單已結束，通常發生在退款單重複申請 | 退款失敗 |
| Success | 0000 | 退款成功 | 退款成功 |
| SameDayPartialRefundRejected | 1124 | 信用卡不可當天部分退 | 退款處理中 |
| RequestProcessing | 1145 | 處理中請稍後 | 退款處理中 |
| RefundPeriodExceeded | 1265 | 改筆訂單已經關帳 該筆訂單被禁止退款，已經超出可退款時限| 退款失敗 |
| RefundNotAllowedDuringBillingPeriod | 1265 11:30 PM~ 12:30 AM 帳期無法退款 | 退款處理中 |
| InsufficientRefundableBalance | 1269 | 帳戶餘額不足 | 退款處理中 |

<br>

#### RefundQuery Mapping ( 屬於 Refund 後確認類型 )

<br>

有 RefundRequest TransactionId , 執行 RefundQuery 確認 退款狀態

<br>

| 狀態名稱 | Return Code | 描述 | 91APP 狀態 |
|----------|-------------|------|----------------|
| Success | 0000 | 退款成功 | 退款成功 |
| SameDayPartialRefundRejected | 1124 | 信用卡不可當天部分退 | 退款處理中 |
| InsufficientRefundableBalance | 1269 | 帳戶餘額不足 | x | 退款處理中 |
| RefundNotAllowedDuringBillingPeriod | 1265 | 11:30 PM~ 12:30 AM 帳期無法退款 | 退款處理中 |
| RequestProcessing | 1145 | 處理中請稍後 | 退款處理中 |
| TransactionClosed | 1264 | 第三方顯示訂單已結束，通常發生在退款單重複申請 | 退款失敗 |

<br>

#### 關帳

<br>

| Code | Description | 關帳期間 (天) |
|------|-------------|--------------|
| 800201 | WeChat Merchant Presented QR Code Payment (MPM) (Overseas & HK Merchants) | 365 |
| 800212 | WeChat H5 Payment (In mobile browser) | 365 |
| 801512 | Alipay Online WAP Payment (HK Merchants) | 365 |
| 801514 | Alipay Online WEB Payment (HK Merchants) | 365 |
| 802001 | FPS Merchant Presented QR Code Payment (MPM) (HK Merchants)*** | 29 |
| 802801 | Visa / Mastercard Online Payments | 365 |
| 805812 | PayMe Online WAP (in mobile browser Chrome etc.) Payment (HK Merchants) | 90 |
| 805814 | PayMe Online WEB (in browser Chrome etc.) Payment (HK Merchants) | 90 |
| 未定義 | 未定義的例外處理 | 365(取最大值) |

<br>
<br>

## 退款異常紀錄

<br>

#### 退款間隔過短導致的重複申請錯誤


```
錯誤訊息：該筆訂單仍有正在處理中的退款請求，請勿重複提交申請
參考連結：https://91app.slack.com/archives/C7T5CTALV/p1753837502573089
```

<br>


**事件時間軸與驗證資料**

| 時間 | 事件 | 驗證結果 |
|------|------|----------|
| 2025-07-29 10:32:52 | 原始付款成功 | 805812 (PayMe) payment 交易完成 |
| 2025-07-29 12:05 | 退款申請 | TS250729L00150C-160027 單次執行確認 |

<br>

**TG 訂單退款狀況分析**

| 子單順序 | 處理結果 | 備註 |
|----------|----------|------|
| 第 1 張子單 | ✅ 退款成功 | 正常處理 |
| 第 2 張子單 | ✅ 退款成功 | 正常處理 |
| 第 3 張子單 | ❌ 退款失敗 | 間隔過短觸發保護機制 |

<br>

**API 查詢驗證**

透過 Query API 確認交易狀態：
- **交易紀錄**：僅存在單一筆付款成功紀錄
- **重複性檢查**：Log 分析確認無重複發送 Refund 請求
- **錯誤訊息**：`"You still have processing requests, please do not repeat the initiation"`

<br>

**解決方案實施**

已完成系統優化調整，增加 TS 退款間隔控制

<br>

#### 退款時 Return Data 為空造成錯誤

**問題時間軸**

| 時間點 | 事件 | 結果 | 備註 |
|--------|------|------|------|
| 10/09 或 10/26 | 訂單交易成功 | - | 原始交易完成 |
| 11/28 | 申請退款 | 失敗 (respcd: 1265) | 退款接口只能申請 29 天內的訂單 (0010000293181285348321738) |
| 11/28 17:00 | 退款被拒 | Pending 等下一輪 | 該筆訂單被禁止退款，已經超出可退款時限，FPS 只能退 29 天以內的交易 |
| 11/28 18:00 | RefundQuery | 持續 1265 | 該筆訂單被禁止退款，23:30-00:30 的交易以及部分特殊交易會被禁止 |
| 持續循環 | 重複查詢 | 持續 Pending | 一直收到相同狀態，持續 Pending |
| 12/01 0:03 | 最終結果 | RefundFail | 訂單資料為空 |

<br>

**根本原因**

距離退款時間已經關帳，但 91APP 在 RefundQuery 時收到該訊息未正確處理，導致循環 RefundQuery 直到資料被清空

<br>

**處理**


已修正 PaymentMiddleware 處理 "該筆訂單被禁止退款，已經超出可退款時限" msg 區分

- **"時間區間的禁止退款"** : 可 Retry
- **"關帳退款的禁止"** : 需壓成 Fail