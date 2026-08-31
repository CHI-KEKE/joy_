# 定期購方案 查詢明細 API Spec — `Api/RegularProduct/GetDetail`

> 涵蓋 Request／Response 欄位、型別、驗證規則、錯誤案例與完整範例。

---

## 1. API 基本資訊

| 項目 | 內容 |
|---|---|
| 用途 | 依「定期購方案序號」查詢單筆定期購方案明細（含基本設定與贈品活動設定），供編輯畫面回填使用 |
| HTTP Method | `GET` |
| OSM 前端路徑 | `/Api/RegularProduct/GetDetail?planId={planId}` |
| Controller | `RegularProductController`（NineYi.Sms） |
| Content-Type | 無 Request Body |
| 呼叫時機 | 使用者進入「定期購方案編輯」畫面時，依網址列或列表帶入的方案序號查詢明細以回填表單 |

---

## 2. Request

### 2.1 Query String 參數

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `planId` | long | 是 | 定期購方案序號（對應下游 `rpd_pk`）；需為大於 0 的正整數 |

> **ShopId 不需前端傳入**：由 OSM 後端從登入 Session 取得，比照既有 `Api/RegularProduct/*` 系列 API 的慣例。

### 2.2 Request Sample

```
GET /Api/RegularProduct/GetDetail?planId=12345
```

（無 Request Body）

---

## 3. Response Body


### 3.0 SMS 統一外層包裝結構 `ApiResultEntity`

| 欄位 | 型別 | 說明 |
|---|---|---|
| `Status` | string | `"Success"`／`"Failure"`，由 `UnifiedResultHandler` 依 HTTP 狀態碼自動判斷（`IsSuccessStatusCode` 為 `true` 則 `Success`） |
| `Data` | object | 成功時為 Controller Action 實際回傳的物件（見 3.1 節）；失敗時通常為 `null` |
| `ErrorMessage` | string | 失敗時的錯誤訊息文案；成功時為 `null` |
| `TimeStamp` | datetime | 伺服器回應時間，由 `UnifiedResultHandler` 自動填入 |

> 此結構為 SMS 全站共用（`NineYi.Sms.BL.BE.Apis.ApiResultEntity`）

### 3.1 成功

Controller Action 實際回傳、會被放進外層 `Data` 欄位的內容：

> **命名說明**：本節欄位命名與型別刻意比照 `Api/RegularProduct/Create` 的 Request 欄位（OSM 端慣用詞彙），**不會**直接透傳下游 v2 API 的原始欄位名稱／型別（例如下游 `Title` 其實是「商品標題」，與 OSM 慣用的 `Title`=「方案名稱」語意相反，故本 API 另外拆出 `salePage_Title` 欄位承接，避免前端誤用）。

| 欄位 | 型別 | 說明 |
|---|---|---|
| `SalepageId` | long | 商品頁序號；語意對應 `Api/RegularProduct/Create` Request 的 `SalepageId` |
| `Title` | string | 方案名稱，可能為 `null`；語意對應 `Api/RegularProduct/Create` Request 的 `Title` |
| `SalePage_Title` | string | 商品標題（即該商品頁本身的標題，非方案名稱） |
| `Price` | string | 商品售價（含幣值符號的格式化字串，如 `$1,000`） |
| `PicUrl` | string | 商品圖片 URL |
| `ShipPeriodList` | int[] | 配送週期（天），複選；語意與型別對應 `Create` Request 的 `ShipPeriodList` |
| `ShippingCountMode` | enum（string） | 配送次數設定模式；語意與型別對應 `Create` Request 的 `ShippingCountMode`，值域定義見 3.2 節 |
| `ShipLimitCount` | int | 配送次數上限；`ShippingCountMode=Unlimited` 時為 `-1`；語意對應 `Create` Request 的 `ShipLimitCount` |
| `AllowOneTimePurchase` | bool | 是否允許消費者選擇「單次購買（不訂閱定期購）」；語意對應 `Create` Request 的 `AllowOneTimePurchase` |
| `IsGiftEnabled` | bool | 贈品設定開關；由 `RegularProductPromotionRules` 是否有內容推導而來（有規則=`true`），語意對應 `Create` Request 的 `IsGiftEnabled`，供編輯畫面回填「贈品設定開關」UI 狀態使用 |
| `ShippingList` | string[] | 配送方式說明清單 |
| `PayList` | string[] | 付款方式說明清單 |
| `PlanDateBegin` | datetime? | 方案生效開始時間；`null`=無下限 |
| `PlanDateEnd` | datetime? | 方案生效結束時間；`null`=永久有效 |
| `RegularProductPromotionRules` | `List<RegularProductPromotionRule>` | 贈品活動規則清單；查無規則時為空陣列 `[]`，結構見 3.3 節 |

### 3.2 `ShippingCountMode` enum 值定義

| 值 | 意義 | 搭配 `ShipLimitCount` |
|---|---|---|
| `Unlimited` | 無上限，持續配送到消費者自行取消訂閱為止 | 固定為 `-1` |
| `CustomerChoice` | 有上限，但配送次數由消費者自己選擇 | 配送次數上限 |
| `Fixed` | 固定次數，配滿即自動終止，消費者不能自選 | 固定配送次數 |

### 3.3 `RegularProductPromotionRule`（`RegularProductPromotionRules` 陣列元素）

| 欄位 | 型別 | 說明 |
|---|---|---|
| `Type` | enum（string） | 活動類型；本次僅會回傳 `"Gift"` |
| `TriggerMode` | enum（string） | 觸發模式；本次僅會回傳 `"Interval"` |
| `Interval` | int | 每 N 期觸發一次（配送每滿 X 次送贈品的 X） |
| `OutOfStockAction` | enum（string） | 贈品缺貨處理：`"Proceed"`（照常建單）／`"SkipPeriod"`（該期不建立訂單） |
| `RegularProductPromotionGiftItemList` | `List<RegularProductPromotionGiftItem>` | 本次觸發要送出的贈品清單，結構見 3.2.1 節 |

#### 3.3.1 `RegularProductPromotionGiftItem`（`RegularProductPromotionGiftItemList` 陣列元素）

| 欄位 | 型別 | 說明 |
|---|---|---|
| `GiftTypeId` | long | 贈品序號，即該贈品自己的 `SalePageId` |
| `Qty` | int | 本次送出數量 |

> **範圍限定**：本 API 僅回傳 v2 原始欄位（`GiftTypeId`／`Qty`），**不會**額外查詢 SCM 補上贈品的 SKU 名稱／圖片；若畫面需要顯示贈品名稱或圖片，前端需自行以 `GiftTypeId` 再呼叫既有商品查詢機制取得。

### 3.4 成功範例

#### 情境一：有贈品規則

```json
{
  "status": "Success",
  "data": {
    "salepageId": 20001,
    "title": "夏季保養品訂閱方案",
    "salePage_Title": "夏季保養精華組",
    "price": "$1,000",
    "picUrl": "https://cdn.example.com/product.jpg",
    "shipPeriodList": [30, 60, 90],
    "shippingCountMode": "CustomerChoice",
    "shipLimitCount": 5,
    "allowOneTimePurchase": true,
    "isGiftEnabled": true,
    "shippingList": ["宅配", "超商取貨"],
    "payList": ["信用卡", "ATM"],
    "planDateBegin": "2026-08-01T00:00:00",
    "planDateEnd": "2026-12-31T23:59:59",
    "regularProductPromotionRules": [
      {
        "type": "Gift",
        "triggerMode": "Interval",
        "interval": 3,
        "outOfStockAction": "Proceed",
        "regularProductPromotionGiftItemList": [
          { "giftTypeId": 50001, "qty": 2 },
          { "giftTypeId": 50002, "qty": 1 }
        ]
      }
    ]
  },
  "errorMessage": null,
  "timeStamp": "2026-08-03T17:30:00"
}
```

#### 情境二：無贈品規則（`RegularProductPromotionRules` 為空陣列）、配送次數無上限

```json
{
  "status": "Success",
  "data": {
    "salepageId": 20002,
    "title": null,
    "salePage_Title": "冬季保暖商品",
    "price": "$800",
    "picUrl": "https://cdn.example.com/product2.jpg",
    "shipPeriodList": [30],
    "shippingCountMode": "Unlimited",
    "shipLimitCount": -1,
    "allowOneTimePurchase": false,
    "isGiftEnabled": false,
    "shippingList": ["宅配"],
    "payList": ["信用卡"],
    "planDateBegin": "2026-10-01T00:00:00",
    "planDateEnd": null,
    "regularProductPromotionRules": []
  },
  "errorMessage": null,
  "timeStamp": "2026-08-03T17:30:00"
}
```

---

## 4. 失敗

失敗時 HTTP 狀態碼會依情境不同而異，並由 `GlobalExceptionHandler` 統一組出以下結構：

| 欄位 | 型別 | 說明 |
|---|---|---|
| `Status` | string | 固定為 `"Failure"` |
| `Data` | object | 一般業務錯誤時為 `null` |
| `ErrorMessage` | string | 錯誤訊息文案，**可直接顯示給使用者** |
| `TimeStamp` | datetime | 伺服器回應時間 |

```json
{
  "status": "Failure",
  "data": null,
  "errorMessage": "查無此定期購方案",
  "timeStamp": "2026-08-03T17:30:00"
}
```

### 4.1 常見錯誤情境對照（`ErrorMessage` 內容參考，非結構化代碼）

| 觸發情境 | HTTP 狀態碼 | `ErrorMessage`（參考文案，實際文案以後端為準） | 前端建議處理 |
|---|---|---|---|
| `planId` 未帶或 `<= 0` | `400 Bad Request` | 「shopId 或 planId 無效」 | 提示參數錯誤，通常代表前端網址組錯，不應開放使用者手動輸入此值 |
| 登入 Session 逾時／未登入 | `401 Unauthorized`（`ApplicationException`，訊息為系統既有 `Message.InvalidAccount`） | 「帳號驗證失敗」（依系統既有文案） | 導回登入頁 |
| 查無此方案序號（下游回傳查無資料） | `400 Bad Request` | 「查無此定期購方案」 | 提示「查無此定期購方案」，可導回列表頁 |
| 下游服務逾時／連線失敗 | `400 Bad Request`（由 `RegularProductHttpClient` 轉譯） | 依下游實際錯誤訊息或預設文案 | 提示稍後再試 |