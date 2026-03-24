
## API Response

退款成功時，API Status=success
退款失敗時，API Status=rejected，以及特定 ErrorCode 會觸發 Retry 機制運作（RD 需盤點現況供 PO 確認）


## ERROR Code

- PR015
- PR020

其餘均跳 Slack 通知 RD 介入處理

- PR001~PR014
- PR016~PR010
- INQ001~INQ006
- INQ011

此為既有功能，應與其他付款方式相同處理，包括大馬既有 Razer 金流：信用卡一次付清、信用卡分期付款、Onlinebanking、3 種電子錢包（Grabpay、ouch 'n Go、Boost）

新加坡既有Razer 金流：信用卡一次付清、 Grabpay
退款狀態未定時，API Status=pending or processing，91 系統需透過退款查詢 API，再次確認退款狀態