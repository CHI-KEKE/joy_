
## 4. Payment Middleware Role

消費者於 MWeb 結帳時，發送結帳相關資訊至 Payment Middleware ，再根據付款方式呼叫至對應的第三方付款服務

<br>

### PayProcesses (結帳)

正向表列金流，訂單相關邏輯建立，根據 PSP 以及 PayChannel 走 tw / hk..., Paymentmiddleware 會根據 paymethod 做處理

<br>

轉導

<br>

PayChannelReturn

<br>

### 退款流程

取消、退貨流程發動後，由 NMQ 發送退款相關資訊至 Payment Middleware ，再根據付款方式呼叫至對應的第三方付款服務

<br>

建立退款單後會被每小時定期排程撈到

<br>

NMQV2_GeneratePaymentMiddleWareRefundRequestGrouping (HH:02)

<br>

退款表就會開始轉狀態

<br>

Grouping -> 建立退款 Task -> PaymentMiddle

<br>

### 補單機制

消費者於 MWeb 結帳時的頁面轉導流程一旦中斷(3D 驗證 or QR Code)，將由 Push Worker 上的 Console Application 定時啟動，查詢付款狀態仍然停留在 WaitingToPay 的訂單付款狀態，透過 Payment Middleware 呼叫對應的第三方付款服務相關訂單狀態。

<br>

---