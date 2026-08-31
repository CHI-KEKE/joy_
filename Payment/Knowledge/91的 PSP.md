
## 1. 91的 PSP

設計原因：全市場共用PayType，商品頁/購物車/Search顯示一致性，減少調整範圍

<br>

透過簡單的設定，可切換各金流串接的服務，ex：Atome走直串或PaymentMiddleware

<br>

資料設定：ShopStaticSetting

<br>

| Market | ShopId | GroupName             | Key   | Value             |
| ------ | ------ | --------------------- | ----- | ----------------- |
| TW     | 0      | PaymentServiceProvider| Atome | Direct            |
| TW     | 12202  | PaymentServiceProvider| Atome | Nine1Payment      |
| HK     | 0      | PaymentServiceProvider| Atome | PaymentMiddleware |

<br>

---