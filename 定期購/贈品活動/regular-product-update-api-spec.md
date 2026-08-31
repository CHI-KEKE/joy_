# 定期購方案 編輯（依方案序號）API Spec — `Api/RegularProduct/Update`

> 對應 EPIC：**定期購支援贈品_202607**。

---

## 1. API 基本資訊

| 項目 | 內容 |
|---|---|
| 用途 | 依「方案序號（`PlanId`）」編輯一筆既有的定期購方案，可修改方案名稱、生效結束時間、配送設定、贈品活動設定；依方案目前狀態（尚未生效／生效中／已結束）限制可修改欄位範圍 |
| HTTP Method | `POST`（比照 `Api/Promotion/UpdatePromotion` 慣例，SMS `/Api/*` 端點多數採 `POST`，非 RESTful `PUT`） |
| OSM 前端路徑 | `/Api/RegularProduct/Update` |
| Controller | `RegularProductController`（NineYi.Sms），**新增 Action**（現況僅有 `Create`／`GetDetail`，尚無 Update） |
| Content-Type | `application/json` |
| 呼叫時機 | 使用者在「編輯定期購方案」畫面（先呼叫 `GetDetail` 帶回既有資料回填表單）修改完欄位後，點擊畫面最下方「儲存」按鈕 |

---


## 3. 依方案狀態的欄位可編輯矩陣（前端畫面須據此控制欄位 disable / readonly）

方案狀態由後端依 `PlanDateBegin`／`PlanDateEnd` 與目前時間計算而得（沿用 `GetDetail` 回傳之 `Status` 欄位，見 `RegularProductPlanStatusEnum`：`Unstarted`／`Ongoing`／`Closed`），**前端呼叫 `Update` 前應先呼叫 `GetDetail` 取得最新狀態，用以控制表單欄位 disable/readonly；後端仍會以 DB 現值重新計算狀態做最終把關，不信任前端傳入值**。

| 欄位 | 尚未生效（`Unstarted`） | 生效中（`Ongoing`） | 已結束（`Closed`） |
|---|:---:|:---:|:---:|
| 商品頁序號（`SalepageId`） | ✅（可換頁） | ❌ | ❌ |
| 方案名稱（`Title`） | ✅ | ✅ | ❌ |
| 生效開始時間（`PlanDateBegin`） | ✅ | ❌ | ❌ |
| 生效結束時間（`PlanDateEnd`） | ✅ | ✅ | ❌ |
| 配送週期／配送次數設定（`ShipPeriodList`／`ShippingCountMode`／`ShipLimitCount`） | ✅ | ✅ | ❌ |
| 是否允許單次購買（`AllowOneTimePurchase`） | ✅ | ✅ | ❌ |
| 贈送條件 `Interval`、贈品內容 `RegularProductPromotionGiftItemList` | ✅ | ❌ | ❌ |
| 贈品缺貨處理 `OutOfStockAction` | ✅ | ✅ | ❌ |
| 贈品開關 `IsGiftEnabled`（是否有贈品規則） | ✅ | ❌ | ❌ |

**驗證失敗處理原則（all-or-nothing）**：Request 中任一「依目前狀態已被鎖定」的欄位，其值與 DB 現值不同 → **整筆請求驗證失敗，不做任何寫入**；不會忽略欄位或部分寫入。前端應盡量透過表單 disable/readonly 從源頭避免使用者誤改鎖定欄位，但**仍須以後端回傳的錯誤訊息為最終判斷依據**（例如使用者用瀏覽器工具繞過前端限制的情況）。

---

## 4. Request

> 比照 `Api/Promotion/UpdatePromotion`（`PromotionDetailEntity`）慣例，**`PlanId` 放在 Request Body 頂層欄位，不使用路由參數**。

### 4.1 Request Body 頂層欄位

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `PlanId` | long | 是 | 定期購方案序號（對應下游 `rpd_pk`），即 `Create` 成功回應或 `GetDetail` 查詢結果中的 `PlanId`；**用於定位要更新的方案，取代路由參數** |
| `Title` | string | 否 | 方案名稱；全形/半形混合上限 20 字，純半形上限 40 字；「已結束」狀態不可異動 |
| `SalepageId` | long | 是 | 定期購商品序號；**「尚未生效」狀態下可修改（換頁），「生效中」「已結束」須與 DB 現值一致，否則整筆驗證失敗**（換頁後台會驗證新商品頁交期為定期購、非點數商品、未結束銷售） |
| `PlanDateBegin` | datetime（ISO 8601） | 是 | 方案生效開始時間；僅「尚未生效」狀態可異動，其餘狀態須與 DB 現值一致 |
| `PlanDateEnd` | datetime（ISO 8601） | 是 | 方案生效結束時間；需晚於 `PlanDateBegin`，且不得晚於商品銷售結束時間；「尚未生效」「生效中」可異動，「已結束」不可異動 |
| `ShipPeriodList` | int[] | 是 | 配送週期（天），複選；合法值為 `3/7/10/14/15/30/45/60/90/180/365/730`；「尚未生效」「生效中」可異動 |
| `ShippingCountMode` | enum（string） | 是 | 配送次數設定模式，見 4.3 節；「尚未生效」「生效中」可異動 |
| `ShipLimitCount` | int | 條件必填 | 配送次數上限；`ShippingCountMode` 為 `CustomerChoice` 或 `Fixed` 時必填，範圍 `2~99`；`Unlimited` 時免填、應傳 `-1` |
| `AllowOneTimePurchase` | bool | 是 | 是否允許消費者在此商品頁選擇「單次購買（不訂閱定期購）」；「尚未生效」「生效中」可異動 |
| `IsGiftEnabled` | bool | 是 | 贈品設定開關；**僅為前端/OSM 端畫面控制欄位**，用來決定 `RegularProductPromotionRules` 是否必填；「生效中」狀態下若目前已有贈品規則，此開關實質鎖定為 `true`，不可關閉（見 4.4 節） |
| `RegularProductPromotionRules` | `List<RegularProductPromotionRule>` | 條件必填 | 當 `IsGiftEnabled=true` 時必填、至少 1 筆；本次範圍僅會有 0 或 1 筆，結構見 4.4 節 |

### 4.2 `ShippingCountMode` enum 值定義（與 Create API 一致）

| 值 | 意義 | 搭配 `ShipLimitCount` |
|---|---|---|
| `Unlimited` | 無上限，持續配送到消費者自行取消訂閱為止 | 免填，應傳 `-1` |
| `CustomerChoice` | 有上限，但配送次數由消費者自己選擇 | 必填，範圍 `2~99` |
| `Fixed` | 固定次數，配滿即自動終止，消費者不能自選 | 必填，範圍 `2~99` |

### 4.3 `RegularProductPromotionRule`（`RegularProductPromotionRules` 陣列元素）

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `Id` | string（Guid） | 條件必填 | 規則識別碼；**「生效中」狀態下必須原封不動回傳 `GetDetail` 取得的既有 `Id`**，後端會強制以 DB 現值覆寫、不採信前端傳入值；「尚未生效」狀態下若為既有規則沿用原 `Id`，新增規則可留空或不傳，由後端產生新 `Id` |
| `Type` | enum（string） | 是 | 活動類型，本次僅開放 `Gift`，須明確帶入 `"Gift"` |
| `TriggerMode` | enum（string） | 是 | 觸發模式，本次僅開放 `Interval`，須明確帶入 `"Interval"` |
| `Interval` | int | 是 | 每 N 期觸發一次；範圍 `2` 至目前 `ShippingCountMode`／`ShipLimitCount` 決定的最高配送次數之間；**「生效中」狀態不可異動**，須與 DB 現值一致 |
| `OutOfStockAction` | enum（string） | 是 | 贈品缺貨處理：`Proceed`（照常建單）／`SkipPeriod`（該期不建立訂單）；**「尚未生效」「生效中」皆可異動** |
| `RegularProductPromotionGiftItemList` | `List<RegularProductPromotionGiftItem>` | 是，至少 1 筆 | 贈品清單；限制 `.length <= 10`、`sum(Qty) <= 50`；**「生效中」狀態不可異動**，須與 DB 現值一致 |

> **重要（前端實作提醒）**：「生效中」狀態下畫面應將 `Interval`、贈品清單、贈品開關全部設為唯讀，僅開放 `OutOfStockAction` 下拉選單可編輯；提交時仍須把唯讀欄位的既有值原封不動放回 Request Body（包含 `Id`），而非省略不傳，避免後端誤判為「清空贈品規則」。

### 4.4 `RegularProductPromotionGiftItem`（陣列元素，與 Create API 一致）

| 欄位 | 型別 | 必填 | 說明 |
|---|---|---|---|
| `GiftTypeId` | long | 是 | 贈品序號，即該贈品自己的 `SalePageId` |
| `Qty` | int | 是 | 本次送出數量，範圍 `1 <= Qty <= 10` |

---

## 5. Request Sample

### 5.1 情境一：尚未生效方案，修改生效期間、配送設定與贈品內容

```json
// POST /Api/RegularProduct/Update
{
  "planId": "1001",
  "title": "夏季保養品訂閱方案（調整版）",
  "salepageId": "20240001",
  "planDateBegin": "2026-09-01T00:00:00",
  "planDateEnd": "2026-12-31T23:59:59",
  "shipPeriodList": [30, 60],
  "shippingCountMode": "CustomerChoice",
  "shipLimitCount": 6,
  "allowOneTimePurchase": true,
  "isGiftEnabled": true,
  "regularProductPromotionRules": [
    {
      "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "type": "Gift",
      "triggerMode": "Interval",
      "interval": 2,
      "outOfStockAction": "Proceed",
      "regularProductPromotionGiftItemList": [
        { "giftTypeId": 50001, "qty": 2 }
      ]
    }
  ]
}
```

### 5.2 情境二：生效中方案，僅修改方案名稱、生效結束時間與贈品缺貨處理（其餘鎖定欄位須原值回傳）

```json
// POST /Api/RegularProduct/Update
{
  "planId": "1002",
  "title": "秋季保養品訂閱方案",
  "salepageId": "20240002",
  "planDateBegin": "2026-07-01T00:00:00",
  "planDateEnd": "2027-01-31T23:59:59",
  "shipPeriodList": [30, 60, 90],
  "shippingCountMode": "CustomerChoice",
  "shipLimitCount": 5,
  "allowOneTimePurchase": true,
  "isGiftEnabled": true,
  "regularProductPromotionRules": [
    {
      "id": "8c1a2b3c-4d5e-6f70-8192-a3b4c5d6e7f8",
      "type": "Gift",
      "triggerMode": "Interval",
      "interval": 3,
      "outOfStockAction": "SkipPeriod",
      "regularProductPromotionGiftItemList": [
        { "giftTypeId": 50001, "qty": 2 },
        { "giftTypeId": 50002, "qty": 1 }
      ]
    }
  ]
}
```

> 情境二中 `planDateBegin`、`shipPeriodList`、`interval`、`regularProductPromotionGiftItemList` 皆與 `GetDetail` 回傳值完全相同（原值回傳），僅 `title`、`planDateEnd`、`outOfStockAction` 與原值不同——這是「生效中」狀態下合法的異動組合。

### 5.3 情境三：關閉贈品設定（僅限「尚未生效」狀態）

```json
// POST /Api/RegularProduct/Update
{
  "planId": "1003",
  "title": "",
  "salepageId": "20240003",
  "planDateBegin": "2026-10-01T00:00:00",
  "planDateEnd": "2027-03-31T23:59:59",
  "shipPeriodList": [7, 14, 30],
  "shippingCountMode": "Fixed",
  "shipLimitCount": 6,
  "allowOneTimePurchase": false,
  "isGiftEnabled": false,
  "regularProductPromotionRules": []
}
```

---

## 6. Response Body

> ⚠️ 與 Create／GetDetail 相同，所有 `/Api/*` 端點的 HTTP Response 皆由 `UnifiedResultHandler` 統一包裝為 `ApiResultEntity` 外層結構，Controller Action 本身回傳的內容只會被放進 `Data` 欄位，**不會有** `IsSuccess`／`ErrorCode` 這種自訂欄位直接出現在頂層。

### 6.0 SMS 統一外層包裝結構 `ApiResultEntity`

| 欄位 | 型別 | 說明 |
|---|---|---|
| `Status` | string | `"Success"`／`"Failure"`，由 `UnifiedResultHandler` 依 HTTP 狀態碼自動判斷 |
| `Data` | object | 成功時為 Controller Action 實際回傳的物件（見 6.1 節）；失敗時通常為 `null` |
| `ErrorMessage` | string | 失敗時的錯誤訊息文案；成功時為 `null` |
| `TimeStamp` | datetime | 伺服器回應時間 |

### 6.1 成功

Controller Action 實際回傳、會被放進外層 `Data` 欄位的內容：

| 欄位 | 型別 | 說明 |
|---|---|---|
| `planId` | long | 編輯成功的方案序號，回傳原值供前端確認 |
| `salepageId` | long | 對應的商品序號，供前端導頁或列表刷新使用 |

```json
{
  "status": "Success",
  "data": {
    "planId": "1002",
    "salepageId": "20240002"
  },
  "errorMessage": null,
  "timeStamp": "2026-08-07T17:40:00"
}
```

### 6.2 失敗

失敗時 HTTP 狀態碼會回傳 `400 Bad Request`（一般業務錯誤／驗證失敗）或 `401 Unauthorized`（Session 逾時等系統錯誤）：

```json
{
  "status": "Failure",
  "data": null,
  "errorMessage": "此方案目前狀態下不可修改所選欄位，請重新確認後再進行儲存",
  "timeStamp": "2026-08-07T17:40:00"
}
```

> 本次規劃同樣**無獨立的 `ErrorCode` 欄位**，前端以 `ErrorMessage` 文案內容判斷情境；下表為開發／前端溝通用的錯誤情境對照。

---

## 7. 常見錯誤情境對照（`ErrorMessage` 內容參考）

| # | 觸發情境 | `ErrorMessage`（參考文案） | 前端建議處理 |
|---|---|---|---|
| 1 | `PlanId` 查無此方案 | 「此定期購方案不存在，請重新輸入」 | 提示後導回列表頁，該方案可能已被刪除或序號有誤 |
| 2 | `SalepageId` 與 DB 現值不同，但方案狀態為「生效中」或「已結束」 | 「此方案目前狀態下不可修改所選欄位，請重新確認後再進行儲存」 | 前端應依 `GetDetail` 回傳狀態控制此欄位是否可編輯（僅「尚未生效」開放），此錯誤多半代表前端未依狀態鎖定或資料組錯 |
| 3 | **狀態鎖定：任一鎖定欄位與 DB 現值不同**（方案名稱／生效開始時間／配送設定／贈送條件／贈品內容／贈品開關，依 3 節矩陣） | 「此方案目前狀態下不可修改所選欄位，請重新確認後再進行儲存」 | 提示使用者重新整理頁面取得最新方案狀態與資料後再編輯；建議前端依 `GetDetail` 的 `Status` 落實表單 disable/readonly，從源頭避免此錯誤 |
| 4 | `PlanDateEnd` 早於 `PlanDateBegin` | 「生效結束時間須晚於生效開始時間」 | 聚焦於日期欄位並提示 |
| 5 | `PlanDateEnd` 晚於商品銷售結束時間 | 「生效結束時間不得晚於商品銷售結束時間」 | 提示對應文案 |
| 6 | 商品序號無效（SCM 查無商品頁） | 「商品序號無效，請重新輸入」 | 提示「商品序號無效，請重新輸入」（理論上編輯畫面商品序號唯讀，此錯誤代表資料異常） |
| 7 | 商品交期非定期購 | 「此商品的「交期」需為定期購，才可進行設定，請先至商品頁重新確認」 | 提示對應文案 |
| 8 | 商品為點數商品 | 「定期購設定僅支援一般商品，請重新輸入」 | 提示對應文案 |
| 9 | 生效期間與其他方案重疊（排除自己這筆） | 「此商品序號於指定生效期間已存在重疊方案，請重新輸入」 | 聚焦於生效期間欄位並提示 |
| 10 | `ShipPeriodList` 為空 | 「請填寫週期設定」 | 聚焦於配送週期選項 |
| 11 | `ShippingCountMode` 值域錯誤 | 「請填寫正確的配送次數選項（1=無限制(Unlimited), 2=有限制(CustomerChoice), 3=固定次數(Fixed)）」 | 理論上前端下拉選單已限制值域，此錯誤代表前端資料組錯 |
| 12 | `ShipLimitCount` 未在 `2~99` 範圍或必填未填 | 「請填寫正確的配送次數限制數量」／「配送次數限制數量請輸入{Min}~{Max}之數字」 | 聚焦於配送次數輸入框並顯示範圍提示 |
| 13 | `Title` 超過字數上限 | 「方案名稱字數超出上限（全半形混合{Max}字，純半形{Max}字）」 | 聚焦於方案名稱輸入框並顯示字數限制 |
| 14 | `RegularProductPromotionRules` 超過 1 筆 | 「本次僅支援單一贈品活動規則」 | 理論上前端本次僅會送 0 或 1 筆，此錯誤代表前端資料組錯 |
| 15 | 贈品項目數量或加總超出限制（`items<=10`、`sum(qty)<=50`） | 「贈品項目數量超出限制」 | 提示對應限制文案 |
| 16 | 贈品序號未通過批次驗證機制 | 「贈品序號驗證失敗，請重新確認」 | 提示「贈品序號驗證失敗，請重新確認」 |
| 17 | DB 寫入失敗 | 「儲存失敗」 | 提示使用者稍後再試 |
| 18 | **成功** | 「成功儲存定期購方案」（依 EPIC 需求原文用字，非舊路由的「儲存成功」） | 提示成功並導回列表頁或刷新明細 |

> 以上文案為依 `regularv2-api-edit-implement-plan.md` 第 12 節推導的參考值，實際 `ErrorMessage` 文案需與後端實作核對後定案；前端不應寫死比對整段文案字串，建議與後端約定關鍵字或改為結構化 key。