# App 設定值處理 — GetMobileAppSettings API

## API 基本資訊

| 項目 | 內容 |
|------|------|
| **API 端點** | `/webapi/AppNotification/GetMobileAppSettings/{shopId}` |
| **範例 URL** | `https://shop2.shop.qa1.hk.91dev.tw/webapi/AppNotification/GetMobileAppSettings/2?r=t` |
| **設定位置** | `ExtendInfo.StripeConfiguration` 節點 |

![alt text](../../Img/image-2.png)

---

## 查詢參數說明

| 參數 | 說明 |
|------|------|
| `r=t` | 重新載入設定，清除伺服器端快取 |
| `lang` | 語言設定（如 `zh-TW`、`en-US`） |
| `shopId` | 指定要取得設定的商店 |
