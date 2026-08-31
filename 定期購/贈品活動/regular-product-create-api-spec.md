# 定期購方案 新增 API Spec — `Api/RegularProduct/Create`

> 涵蓋 Request／Response 欄位、型別、驗證規則、錯誤案例與完整範例。

---

## 1. API 基本資訊

| 項目 | 內容 |
|---|---|
| 用途 | 新增一筆「定期購方案」，包含基本設定（生效期間、配送週期、配送次數、購買方式）與贈品活動設定 |
| HTTP Method | `POST` |
| OSM 前端路徑 | `/Api/RegularProduct/Create` |
| Controller | `RegularProductController`（NineYi.Sms） |
| Content-Type | `application/json` |
| 呼叫時機 | 使用者在「新增定期購方案」畫面填完「基本設定＋贈品設定」後，點擊畫面最下方「新增」按鈕 |

---

## 2. Request Body

### 2.1 頂層欄位

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `Title` | string | 否 | 方案名稱，供後台管理識別；全形/半形混合上限 20 字，純半形上限 40 字；超過上限前端需自動截斷 |
| `SalepageId` | long | 是 | 定期購商品序號；每個方案僅能設定一個商品，同一商品可設定於多個方案（受 2.5 節 overlap 規則限制） |
| `PlanDateBegin` | datetime（ISO 8601） | 是 | 方案生效開始時間；需晚於「現在時間」10 分鐘以上，且不得早於商品銷售開始時間 |
| `PlanDateEnd` | datetime（ISO 8601） | 是 | 方案生效結束時間；需晚於 `PlanDateBegin`，且不得晚於商品銷售結束時間；|
| `ShipPeriodList` | int[] | 是 | 配送週期（天），複選；合法值為 `3/7/10/14/15/30/45/60/90/180/365/730` |
| `ShippingCountMode` | enum（string） | 是 | 配送次數設定模式，見 2.2 節 |
| `ShipLimitCount` | int | 條件必填 | 配送次數上限；`ShippingCountMode` 為 `CustomerChoice` 或 `Fixed` 時必填，範圍 `2~99`；`Unlimited` 時免填、應傳 `-1` |
| `AllowOneTimePurchase` | bool | 是 | 是否允許消費者在此商品頁選擇「單次購買（不訂閱定期購）」；預設 `false` |
| `IsGiftEnabled` | bool | 是 | 贈品設定開關；**僅為前端/OSM 端畫面控制欄位**，用來決定 `RegularProductPromotionRules` 是否必填|
| `RegularProductPromotionRules` | `List<RegularProductPromotionRule>` | 條件必填 | 當 `IsGiftEnabled=true` 時必填、至少 1 筆；`IsGiftEnabled=false` 時傳空陣列 `[]`；本次範圍僅會有 0 或 1 筆，結構見 2.3 節 |


### 2.2 `ShippingCountMode` enum 值定義

| 值 | 意義 | 搭配 `ShipLimitCount` |
|---|---|---|
| `Unlimited` | 無上限，持續配送到消費者自行取消訂閱為止 | 免填，應傳 `-1` |
| `CustomerChoice` | 有上限，但配送次數由消費者自己選擇 | 必填，範圍 `2~99` |
| `Fixed` | 固定次數，配滿即自動終止，消費者不能自選 | 必填，範圍 `2~99`；|


### 2.3 `RegularProductPromotionRule`（`RegularProductPromotionRules` 陣列元素）

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `Type` | enum（string） | 是 | 活動類型，值域 `Gift`／`Discount`／`Point`；**本次僅開放 `Gift`**，其餘值後端會直接拒絕；前端仍須明確帶入 `"Gift"`，不可省略或期待後端推斷（為未來擴充預留同一資料流） |
| `TriggerMode` | enum（string） | 是 | 觸發模式，值域 `Interval`／`Sequence`／`Threshold`；**本次僅開放 `Interval`**；前端仍須明確帶入 `"Interval"` |
| `Interval` | int | 是（`TriggerMode=Interval` 時） | 每 N 期觸發一次（配送每滿 X 次送贈品的 X）；範圍為 `2` 至「當下 `ShippingCountMode`／`ShipLimitCount` 決定的最高配送次數」之間的整數 |
| `OutOfStockAction` | enum（string） | 是 | 贈品缺貨處理：`Proceed`（照常建單）／`SkipPeriod`（該期不建立訂單） |
| `RegularProductPromotionGiftItemList` | `List<RegularProductPromotionGiftItem>` | 是，至少 1 筆 | 本次觸發要送出的贈品清單；限制 `.length <= 10`、`sum(Qty) <= 50` |


### 2.4 `RegularProductPromotionGiftItem`（`RegularProductPromotionGiftItemList` 陣列元素）

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `GiftTypeId` | long | 是 | 贈品序號，即該贈品自己的 `SalePageId`；新增前需先呼叫既有「批次驗證贈品序號」機制驗證通過 |
| `Qty` | int | 是 | 本次送出數量，操作人員手動輸入，範圍 `1 <= Qty <= 10` |

### 2.5 `SalepageId` overlap（時間重疊）規則提醒

同一 `SalepageId` 若已存在其他方案，其 `PlanDateBegin`／`PlanDateEnd` 區間**不可與既有方案重疊**。此項檢查是在使用者填完整個表單、**按下最下方「新增」按鈕、實際呼叫 `Api/RegularProduct/Create` 時**由後端一併驗證

---

## 3. Request Sample

### 3.1 情境一：開啟贈品、配送次數為「消費者自選次數」

```json
{
  "title": "夏季保養品訂閱方案",
  "salepageId": "20240001",
  "planDateBegin": "2026-08-01T00:00:00",
  "planDateEnd": "2026-12-31T23:59:59",
  "shipPeriodList": [30, 60, 90],
  "shippingCountMode": "CustomerChoice",
  "shipLimitCount": 5,
  "allowOneTimePurchase": true,
  "isGiftEnabled": true,
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
}
```

### 3.2 情境二：關閉贈品、配送次數為「固定次數」

```json
{
  "title": "",
  "salepageId": "20240002",
  "planDateBegin": "2026-09-01T00:00:00",
  "planDateEnd": "2027-08-31T23:59:59",
  "shipPeriodList": [7, 14, 30],
  "shippingCountMode": "Fixed",
  "shipLimitCount": 6,
  "allowOneTimePurchase": false,
  "isGiftEnabled": false,
  "regularProductPromotionRules": []
}
```

### 3.3 情境三：配送次數為「無上限」（`ShipLimitCount` 傳 `-1`）

```json
{
  "title": "冬季保暖商品訂閱",
  "salepageId": "20240003",
  "planDateBegin": "2026-10-01T00:00:00",
  "planDateEnd": "2027-03-31T23:59:59",
  "shipPeriodList": [30],
  "shippingCountMode": "Unlimited",
  "shipLimitCount": -1,
  "allowOneTimePurchase": false,
  "isGiftEnabled": false,
  "regularProductPromotionRules": []
}
```


---

## 4. Response Body

> ⚠️ **重要：本節已依 NineYi.Sms 既有 API 包裝機制修正。** 所有 `/Api/*` 端點的 HTTP Response 都會被 `UnifiedResultHandler`（`WebSite/WebSite/Extensions/DelegatingHandlers/UnifiedResultHandler.cs`）統一包裝成 `ApiResultEntity` 外層結構，Controller Action 本身回傳的內容只會被放進 `Data` 欄位；**不會有** `IsSuccess`／`ErrorCode` 這種自訂欄位直接出現在頂層。

### 4.0 SMS 統一外層包裝結構 `ApiResultEntity`

| 欄位 | 型別 | 說明 |
|---|---|---|
| `Status` | string | `"Success"`／`"Failure"`，由 `UnifiedResultHandler` 依 HTTP 狀態碼自動判斷（`IsSuccessStatusCode` 為 `true` 則 `Success`） |
| `Data` | object | 成功時為 Controller Action 實際回傳的物件（見 4.1 節）；失敗時通常為 `null` 或驗證錯誤內容（`{ Errors: ... }`），視例外型別而定 |
| `ErrorMessage` | string | 失敗時的錯誤訊息文案；成功時為 `null` |
| `TimeStamp` | datetime | 伺服器回應時間，由 `UnifiedResultHandler` 自動填入 |

> 此結構為 SMS 全站共用（`NineYi.Sms.BL.BE.Apis.ApiResultEntity`），**Create API 本身不需要、也不應該自訂 `IsSuccess`／`ErrorCode` 欄位**，前端應統一以外層 `Status`／`ErrorMessage` 判斷成功或失敗；業務邏輯失敗案例改以 HTTP 4xx（`ApplicationException`）搭配 `ErrorMessage` 文案表達，見 4.3 節錯誤案例。

### 4.1 成功

Controller Action 實際回傳、會被放進外層 `Data` 欄位的內容：

| 欄位 | 型別 | 說明 |
|---|---|---|
| `salepageId` | long | 回傳新增成功的商品序號，供前端導頁或列表刷新使用 |

```json
{
  "status": "Success",
  "data": {
    "salepageId": "20240001"
  },
  "errorMessage": null,
  "timeStamp": "2026-08-03T17:30:00"
}
```

### 4.2 失敗

失敗時 HTTP 狀態碼會回傳 `400 Bad Request`（一般業務錯誤，Service 層拋出 `ApplicationException`）或 `401 Unauthorized`（Session 逾時等系統錯誤），並由 `GlobalExceptionHandler` 統一組出以下結構：

| 欄位 | 型別 | 說明 |
|---|---|---|
| `Status` | string | 固定為 `"Failure"` |
| `Data` | object | 一般業務錯誤時為 `null`；若為 Model Validation 錯誤（如必填欄位缺漏、格式錯誤），則為 `{ Errors: { 欄位名稱: [錯誤訊息, ...] } }` |
| `ErrorMessage` | string | 錯誤訊息文案，比照既有 `ValidateProductAsync`／`AddProductAsync` 錯誤訊息風格，**可直接顯示給使用者**；Model Validation 錯誤時為多筆錯誤訊息以 `, ` 串接 |
| `TimeStamp` | datetime | 伺服器回應時間 |

```json
{
  "status": "Failure",
  "data": null,
  "errorMessage": "此商品序號在此期間已被其他定期購方案設定，請調整生效期間",
  "timeStamp": "2026-08-03T17:30:00"
}
```

> **本次規劃並無獨立的 `ErrorCode` 欄位**（SMS 既有錯誤處理機制以 `ErrorMessage` 文案為主，非結構化錯誤代碼）；4.3 節列出的「錯誤情境」為**開發／前端溝通用的錯誤情境對照**，前端應以 `ErrorMessage` 文案內容（或約定的關鍵字比對）判斷情境，而非解析獨立的代碼欄位。若後續 v2 端有提供結構化 `ErrorCode`，需另行與 OSM 後端確認是否於 Service 層轉譯後併入 `ErrorMessage`。

### 4.3 常見錯誤情境對照（`ErrorMessage` 內容參考，非結構化代碼）

| 觸發情境 | `ErrorMessage`（參考文案，實際文案以後端為準） | 前端建議處理 |
|---|---|---|
| `SalepageId` 對應商品頁不存在 | 「查無此商品序號」 | 提示「查無此商品序號」，聚焦於商品序號輸入框 |
| 商品非定期購交期或為點數商品 | 「此商品不支援設定定期購方案」 | 提示「此商品不支援設定定期購方案」 |
| 商品銷售已結束 | 「此商品銷售已結束，無法設定」 | 提示「此商品銷售已結束，無法設定」 |
| `PlanDateBegin`／`PlanDateEnd` 超出商品銷售時間範圍 | 「生效期間需在商品銷售期間內」 | 提示「生效期間需在商品銷售期間內」，聚焦於日期欄位 |
| `PlanDateBegin` 未晚於現在時間 10 分鐘以上 | 「生效開始時間需晚於目前時間 10 分鐘以上」 | 提示「生效開始時間需晚於目前時間 10 分鐘以上」 |
| 同商品序號的生效期間與既有方案重疊 | 「此商品序號在此期間已被其他方案設定」 | 提示「此商品序號在此期間已被其他方案設定」 |
| `ShipLimitCount` 未在 `2~99` 範圍或必填未填 | 「配送次數需介於 2~99 之間」 | 聚焦於配送次數輸入框並顯示範圍提示 |
| `RegularProductPromotionRules[].type` 非 `Gift` | 「不支援的活動類型」 | 理論上前端本次僅會送 `Gift`，此錯誤多半代表前端資料組錯 |
| `RegularProductPromotionRules[].interval` 超出 `2~最高配送次數` 範圍 | 「配送次數設定超出範圍」 | 聚焦於「每滿 X 次」輸入框 |
| 贈品項目數量或加總超出限制（`items<=10`、`sum(qty)<=50`） | 「贈品項目數量超出限制」 | 提示對應限制文案 |
| 贈品序號未通過批次驗證機制 | 「贈品序號驗證失敗，請重新確認」 | 提示「贈品序號驗證失敗，請重新確認」 |

> 以上文案為**依現有規劃推導的參考值**，實際 `ErrorMessage` 文案需與 OSM 後端 Service 層實作核對後定案；前端不應寫死比對整段文案字串，建議與後端約定關鍵字或改為結構化 `ErrorMessage` key（例如以英文代碼作為 `ErrorMessage` 前綴）。