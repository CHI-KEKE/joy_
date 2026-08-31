
## 3. Paymentmiddleware 訂單流程狀態

https://app.diagrams.net/#G1eUZ-EHMtYZnOWgzXQh1jIlaIazfvuM6G#%7B%22pageId%22%3A%220783ab3e-0a74-02c8-0abd-f7b4e66b4bec%22%7D

<br>

### 三個階段

建立訂單（下單）

<br>

發動付款（導向金流）

<br>

回傳付款結果（成功 / 失敗）

<br>

### 角色

NMQV2：內部 API Gateway 或後端模組，處理訂單資料

<br>

MMWeb：商店網站前端／商城平台

<br>

External Websites：外部金流網頁（例如信用卡輸入頁、3D 驗證頁）

<br>

Payment Middleware：金流中介層，封裝各家金流商 API（像 TapPay、NCCC、PayNow）

<br>

### 金流

Pay：發動付款

<br>

3D Auth / PayCode：進行安全驗證

<br>

Return / Callback：金流商通知結果

<br>

Query / Cancel：查詢或取消交易

<br>

### 🟢 Step 1：使用者建立購物車並下單

位置：MMWeb

<br>

使用者在網站結帳 → 觸發 API /WebApi/TransOrderLite/Send

<br>

系統呼叫 ThirdPartyProcess（第三方金流初始化流程）

<br>

這個步驟會把：

<br>

訂單明細

<br>

金額

<br>

付款方式

<br>

打包好準備送給金流服務

<br>

這是「交易起始點」，建立一筆「待付款訂單」

<br>

### 🟡 Step 2：呼叫金流中介層發動付款

位置：Payment Middleware → External Websites

<br>

MMWeb 呼叫 Payment Middleware 的 Pay()

<br>

middleware會：

<br>

產生交易序號（Trade ID）

<br>

呼叫外部金流商的付款 API

<br>

取得金流導向網址（例如 TapPay 的 PayUrl 或 3D 驗證頁面）

<br>

接著，使用者會被導向金流商頁面，進行：

<br>

信用卡輸入

<br>

3D 驗證（如果有開啟）

<br>

### 3D 驗證或 PayCode 流程（安全驗證）

位置：External Websites

<br>

若銀行要求 3D 驗證（例如 Visa Secure / MasterCard ID Check）：

<br>

使用者會跳轉至銀行頁面，輸入 OTP 或驗證碼

<br>

驗證成功後 → 回傳結果給平台的回呼網址（Callback URL）

<br>

若是非 3D 模式（一般授權）：由金流商直接回傳結果給平台（免跳頁）

<br>

### 金流回傳付款結果（Callback → PayChannelReturn）

位置：/V2/PayChannel/PayChannelReturn

<br>

這是金流最重要的回呼點：

<br>

外部金流網站會呼叫此 API，把交易結果回報給平台：

<br>

成功 → 回傳授權碼、交易序號

<br>

失敗 → 回傳錯誤代碼（例如授權拒絕、逾時）

<br>

平台接到回傳後會呼叫 ThirdPartyFinishProcess 處理後續：

<br>

更新訂單狀態（成功 / 失敗）

<br>

記錄金流交易紀錄（PaymentLog）

<br>

若成功 → 發票與庫存流程啟動

<br>

### 🔘 Step 5：判斷付款結果

系統根據回傳結果分兩種：

<br>

成功 → 結束流程，回傳成功頁面

<br>

失敗 / 逾時 / 錯誤 → 顯示付款失敗頁，允許重新付款

<br>

若平台沒收到金流回傳（例如使用者中途關掉頁面），系統可呼叫 QueryPayment 主動向金流商查詢付款狀態。

<br>

### 🔴 Step 6：例外處理：Query / Cancel

QueryPayment（查詢交易）

<br>

用交易編號問金流商：「這筆交易到底成功沒？」

<br>

避免 Callback 遺漏的狀況。

<br>

Cancel（取消交易）

<br>

若金流授權成功但後來需要撤銷（例如使用者取消訂單），

<br>

就會呼叫取消 API 通知金流商撤回授權。

<br>