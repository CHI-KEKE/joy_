
## 8. WebView

WebView 是 App 內嵌的一個「網頁容器」，可以在 App 裡打開並顯示網頁內容（HTML / JS / CSS），但不跳出 App。你可以把 WebView 想成 App 中的一個「迷你瀏覽器」。它不是 Safari、Chrome，而是 App 裡內建的一個畫面元件，用來載入網站內容。

<br>

| 用途           | 說明                         |
| ------------ | -------------------------- |
| 顯示金流頁面       | 像付款完成畫面、訂單明細頁              |
| 顯示第三方協力平台    | 像 LINE Pay、Apple Pay 回傳頁   |
| 嵌入後台、問卷、客服頁面 | 直接使用既有網站畫面                 |
| 跨平台開發        | 一套網頁讓 Android/iOS 都能顯示相同介面 |




## 9. WebView 和 App 要怎樣有良好溝通

這是整個串接 Apple Pay/TapPay 成功與否的關鍵。

<br>

因為兩者是不同層級：

<br>

App 是原生層（Native）

<br>

WebView 是網頁層（HTML+JS）

<br>

所以溝通要靠「橋接技術（Bridge）」來串起兩邊。

<br>

| 方向            | 橋接技術                                  | 用途                      |
| ------------- | ------------------------------------- | ----------------------- |
| WebView → App | JS → Native Bridge（JavaScript 調用原生函式） | 網頁告訴 App：我要啟動 Apple Pay |
| App → WebView | WebView.evaluateJavascript()          | App 執行 JS：告訴網頁交易完成      |