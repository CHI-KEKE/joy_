# QFPay — 測試卡資訊

[測試卡文件](https://sdk.qfapi.com/docs/online-shop/visa-master-online-payment#test-cards)

<br>

## 測試卡列表

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

## 測試卡特殊行為說明

| 測試情境 | 卡別 | 卡號 | Return Code | 回應訊息 | 行為特徵 |
|----------|------|------|-------------|----------|----------|
| 付款失敗 (at verification) | MasterCard | 5200000000001120 | 1145 | 處理中，請稍等 | 1. 重新到相同付款頁仍然可以用正確卡號結帳轉導<br>2. QFPay 會有 2 筆紀錄<br>3. 屬於交易進行中情境<br>4. 跳轉 FailedUrl |
| 付款失敗 (at verification) | Visa | 4000000000001125 | 1145 | 處理中，請稍等 | 1. 重新到相同付款頁仍然可以用正確卡號結帳轉導<br>2. QFPay 會有 2 筆紀錄<br>3. 屬於交易進行中情境<br>4. 跳轉 FailedUrl |
| 付款失敗 (at 3DS frictionless) | MasterCard | 5200000000001013 | 1205 | 交易失敗，請稍後重试 | 1. 填卡頁會直接阻擋<br>2. 重新填正確卡號可以完成交易跳轉<br>3. 會有 2 筆資料 |
| 付款失敗 (at 3DS frictionless) | Visa | 4000000000001018 | 1205 | 交易失敗，請稍後重试 | 1. 填卡頁會直接阻擋<br>2. 重新填正確卡號可以完成交易跳轉<br>3. 會有 2 筆資料 |
