
## 4. 根據 App 條件來選擇正確的 PSP（像 TapPay）

可以判斷 app 版號 & 商店設定決定該筆交易的 Apple Pay 要走哪一個 PSP，並且

<br>

載入正確的 PSP 的 SDK，如為 PSP=TapPay，再查詢收單行設定，並至信用卡子代碼查詢取 proxyMID 對正確的 PSP 送出交易請求

<br>

功能漸進部署（Feature Flag by version），可能在新版 App 中才支援某個 PSP（例如加入 TapPay 支援），所以舊版 App 要走舊的 PSP、新版才能走新的 SDK