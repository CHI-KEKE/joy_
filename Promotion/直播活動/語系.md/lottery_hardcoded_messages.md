# 抽獎相關硬編碼文案清單

> 掃描日期：2026-03-09  
> 目的：評估哪些文案可以抽出來接入多語系（Translation 平台）

---

## 圖例說明

| 符號 | 說明 |
|------|------|
| ✅ | 建議納入多語系（使用者可見的訊息） |
| ⚠️ | 謹慎評估（含動態 Enum 狀態名稱，需同時對狀態值做多語系） |
| ❌ | 不建議納入（純技術診斷訊息，僅開發者可見） |

---

## 一、Validators（`WithMessage`）— 輸入驗證訊息

### `CreateLotteryActivityRequestEntityValidator`

| 訊息 | 多語系 |
|------|--------|
| `"請求物件不可為 null"` | ✅ |
| `"商店 ID 必須大於 0"` | ✅ |
| `"直播場次 ID 不可為空白"` | ✅ |
| `"直播場次 ID 長度不可超過 100 個字元"` | ✅ |
| `"抽獎活動名稱不可為空白"` | ✅ |
| `"抽獎活動名稱長度不可超過 50 個字元"` | ✅ |
| `"抽獎關鍵字不可為空白"` | ✅ |
| `"抽獎關鍵字長度不可超過 20 個字元"` | ✅ |
| `"關鍵字不可輸入全形字元"` | ✅ |
| `"關鍵字僅能為中文、英文或數字"` | ✅ |
| `"中獎名額必須介於 1 到 200 之間"` | ✅ |
| `"當開啟自動開獎時，必須指定自動開獎時間"` | ✅ |
| `"自動開獎時間必須晚於目前時間 10 分鐘以上"` | ✅ |
| `"進階抽獎條件不可為 null"` | ✅ |
| `"訊息模板不可為 null"` | ✅ |

### `UpdateLotteryActivityRequestEntityValidator`

| 訊息 | 多語系 |
|------|--------|
| `"請求物件不可為 null"` | ✅ |
| `"ShopId 必須大於 0"` | ✅ |
| `"ActivityId 為必填參數"` | ✅ |
| `"ActivityId 長度不可超過 100 個字元"` | ✅ |
| `"LiveSessionId 為必填參數"` | ✅ |
| `"LiveSessionId 長度不可超過 100 個字元"` | ✅ |
| `"CurrentStatus 為必填參數"` | ✅ |
| `"CurrentStatus 必須為有效的狀態值 (NotStarted, InProgress, Drawing, Drawn, Terminated, AutoDrawFailed)"` | ⚠️ 含技術 Enum 值 |
| `"抽獎活動名稱長度不可超過 50 個字元"` | ✅ |
| `"抽獎關鍵字長度不可超過 20 個字元"` | ✅ |
| `"關鍵字不可輸入全形字元"` | ✅ |
| `"關鍵字僅能為中文、英文或數字"` | ✅ |
| `"中獎名額必須介於 1 到 200 之間"` | ✅ |
| `"自動開獎時間必須晚於目前時間 10 分鐘以上"` | ✅ |
| `"當啟用自動開獎時，必須設定預定開獎時間"` | ✅ |
| `"ECouponId 必須大於 0"` | ✅ |
| `"標記好友數量必須介於 0 到 10 之間"` | ✅ |
| `"當需要標記好友時，標記好友數量必須大於 0"` | ✅ |
| `"當不需要標記好友時，標記好友數量應為空或 0"` | ✅ |
| `"活動開始訊息長度不可超過 1000 個字元"` | ✅ |
| `"開獎訊息長度不可超過 300 個字元"` | ✅ |
| `"中獎私訊訊息長度不可超過 1000 個字元"` | ✅ |

### `DrawLotteryActivityRequestEntityValidator`

| 訊息 | 多語系 |
|------|--------|
| `"請求物件不可為 null"` | ✅ |
| `"商店 ID 必須大於 0"` | ✅ |
| `"直播場次 ID 不可為空白"` | ✅ |
| `"開獎類型不可為空白"` | ✅ |
| `"開獎類型必須為 Manual 或 Additional"` | ⚠️ 含技術 Enum 值 |
| `"中獎人數必須大於等於 1"` | ✅ |
| `"加抽人數上限為 100 位"` | ✅ |
| `"手動開獎中獎人數必須介於 1 到 200 之間"` | ✅ |

### `GetLotteryActivityDetailRequestEntityValidator`

| 訊息 | 多語系 |
|------|--------|
| `"請求物件不可為 null"` | ✅ |
| `"ShopId 必須大於 0"` | ✅ |
| `"LiveSessionId 為必填參數"` | ✅ |
| `"LiveSessionId 長度不可超過 {N} 字元"` | ✅ |
| `"LiveSessionId 格式不正確，僅允許英數字、連字號及底線"` | ✅ |
| `"ActivityId 為必填參數"` | ✅ |
| `"ActivityId 長度不可超過 {N} 字元"` | ✅ |
| `"ActivityId 格式不正確，僅允許英數字、連字號及底線"` | ✅ |

### `GetLotteryWinnersRequestEntityValidator`

| 訊息 | 多語系 |
|------|--------|
| `"請求物件不可為 null"` | ✅ |
| `"商店 ID 不可為空白"` | ✅ |
| `"直播場次 ID 不可為空白"` | ✅ |
| `"抽獎活動 ID 不可為空白"` | ✅ |
| `"開獎 ID 不可為空白或僅包含空白字元"` | ✅ |

### `StartLotteryActivityRequestEntityValidator`

| 訊息 | 多語系 |
|------|--------|
| `"請求物件不可為 null"` | ✅ |
| `"商店 ID 必須大於 0"` | ✅ |
| `"直播場次 ID 不可為空白"` | ✅ |
| `"直播場次 ID 長度不可超過 100 個字元"` | ✅ |

### `TerminateLotteryActivityRequestEntityValidator`

| 訊息 | 多語系 |
|------|--------|
| `"請求物件不可為 null"` | ✅ |
| `"ShopId 必須大於 0"` | ✅ |
| `"ActivityId 為必填參數"` | ✅ |
| `"ActivityId 長度不可超過 100 個字元"` | ✅ |
| `"LiveSessionId 為必填參數"` | ✅ |
| `"LiveSessionId 長度不可超過 100 個字元"` | ✅ |

### `PromoteLotteryActivityRequestEntityValidator`

| 訊息 | 多語系 |
|------|--------|
| `"請求物件不可為 null"` | ✅ |
| `"商店 ID 必須大於 0"` | ✅ |
| `"直播場次 ID 不可為空白"` | ✅ |
| `"直播場次 ID 長度不可超過 100 個字元"` | ✅ |

### `SendLotteryWinnerMessageRequestEntityValidator`

| 訊息 | 多語系 |
|------|--------|
| `"請求物件不可為 null"` | ✅ |
| `"商店 ID 必須大於 0"` | ✅ |
| `"LiveSessionId 為必填欄位"` | ✅ |
| `"ActivityId 為必填欄位"` | ✅ |
| `"DrawId 為必填欄位"` | ✅ |
| `"Message 為必填欄位"` | ✅ |
| `"Message 長度不可超過 500 字"` | ✅ |

---

## 二、LotteryActivityService — 業務邏輯錯誤（`ApplicationApiException`）

> 檔案路徑：`src/BusinessLogic/Nine1.Livebuy.BL.Services/LotteryActivities/LotteryActivityService.cs`

| 行號 | Exception Enum | 訊息 | 多語系 |
|------|----------------|------|--------|
| 132 | `KEYWORD_ALREADY_IN_USE` | `"關鍵字 '{keyword}' 已被商品使用，請使用其他關鍵字"` | ✅ |
| 157 | `KEYWORD_ALREADY_IN_USE` | `"關鍵字 '{keyword}' 已被其他抽獎活動使用，請使用其他關鍵字"` | ✅ |
| 175 | `InvalidOperation` | `"抽獎活動數量已達上限 {N} 檔，無法再建立新活動"` | ✅ |
| 344 | `InvalidOperation` | `"ActivityId 不可為空"` | ✅ |
| 359 | `InvalidOperation` | `"LiveSessionId 不可為空"` | ✅ |
| 388 | `NoData` | `"找不到抽獎活動 {id}"` | ✅ |
| 406 | `InvalidOperation` | `"抽獎活動狀態無效: {status}"` | ⚠️ 含動態狀態值 |
| 429 | `InvalidOperation` | `"抽獎活動已開始，不可重複開始"` | ✅ |
| 444 | `InvalidOperation` | `"抽獎活動開獎中，無法再次開始"` | ✅ |
| 460/475 | `InvalidOperation` | `"抽獎活動狀態為 {status}，無法開始"` | ⚠️ 含動態狀態值 |
| 501 | `InvalidOperation` | `"自動開獎時間已過，無法手動開始活動"` | ✅ |
| 551 | `InvalidOperation` | `"開始抽獎活動時發生並行衝突，請重試"` | ✅ |
| 637 | `InvalidOperation` | `"ActivityId 不可為空"` | ✅ |
| 652 | `InvalidOperation` | `"LiveSessionId 不可為空"` | ✅ |
| 681 | `ACTIVITY_NOT_FOUND` | `"找不到抽獎活動 {id}"` | ✅ |
| 698 | `InvalidOperation` | `"抽獎活動狀態無效: {status}"` | ⚠️ 含動態狀態值 |
| 716 | `ACTIVITY_STATUS_MISMATCH` | `"僅允許進行中（InProgress）的活動執行推廣，目前狀態為 {status}"` | ✅ |
| 730 | `MESSAGE_TEMPLATE_NOT_SET` | `"未設定開始訊息範本，無法推廣活動"` | ✅ |
| 763 | `MESSAGE_PUBLISH_FAILED` | `"推廣訊息發布失敗，請稍後再試"` | ✅ |
| 1003 | `InvalidOperation` | `"ActivityId 為必填參數"` | ✅ |
| 1128 | `NoData` | `"找不到抽獎活動 {id}"` | ✅ |
| 1142 | `ACTIVITY_STATUS_MISMATCH` | `Common.Translations.Backend.Service.LotteryActivityService.StatusHasChanged` | ✅ 已多語系 |
| 1151 | `InvalidOperation` | `"活動狀態無效: {status}"` | ⚠️ 含動態狀態值 |
| 1159 | `InvalidOperation` | `"活動狀態為 {status}，不允許編輯"` | ✅ |
| 1199 | `InvalidOperation` | `"活動狀態 {status} 不允許編輯"` | ✅ |
| 1403 | `InvalidOperation` | `"活動進行中時，無法新增自動開獎時間"` | ✅ |
| 1538 | `KEYWORD_ALREADY_IN_USE` | `"關鍵字 '{keyword}' 已被商品使用，請使用其他關鍵字"` | ✅ |
| 1557 | `KEYWORD_ALREADY_IN_USE` | `"關鍵字 '{keyword}' 已被其他抽獎活動使用，請使用其他關鍵字"` | ✅ |
| 1591 | `NoData` | `"找不到指定的抽獎活動"` | ✅ |
| 1630 | `InvalidOperation` | `"LiveSessionId 為必填參數"` | ✅ |
| 1637 | `InvalidOperation` | `"ActivityId 為必填參數"` | ✅ |
| 1663 | `NoData` | `"活動不存在或不屬於該商店"` | ✅ |
| 1769 | `NoData` | `"活動不存在或不屬於該商店"` | ✅ |
| 1833 | `NoData` | `"找不到抽獎活動 {id}"` | ✅ |
| 1850 | `InvalidOperation` | `"抽獎活動狀態無效: {status}"` | ⚠️ 含動態狀態值 |
| 1874 | `InvalidOperation` | `"抽獎活動尚未開始，無法終止"` | ✅ |
| 1889 | `InvalidOperation` | `"抽獎活動開獎中，無法終止"` | ✅ |
| 1904 | `InvalidOperation` | `"抽獎活動已終止，無法重複終止"` | ✅ |
| 1919 | `InvalidOperation` | `"抽獎活動狀態為 {status}，無法終止"` | ✅ |
| 2053 | `InvalidParameter` | `"ActivityId 為必填參數"` | ✅ |
| 2061 | `InvalidParameter` | `"請求內容不可為空"` | ✅ |
| 2069 | `InvalidParameter` | `"ShopId 必須大於 0"` | ✅ |
| 2077 | `InvalidParameter` | `"LiveSessionId 為必填參數"` | ✅ |
| 2098 | `NoData` | `"活動不存在"` | ✅ |
| 2114 | `InvalidParameter` | `"ShopId 不符"` | ✅ |
| 2131 | `ACTIVITY_STATUS_MISMATCH` | `"活動狀態不允許刪除，目前狀態: {status}"` | ✅ |
| 2150 | `InvalidOperation` | `"資料異常：此活動在 NotStarted 狀態下不應有參與者（目前有 {N} 位），請聯繫技術人員"` | ❌ 技術診斷訊息 |

---

## 三、LotteryDrawService — 開獎邏輯（`ApplicationApiException`）

> 檔案路徑：`src/BusinessLogic/Nine1.Livebuy.BL.Services/LotteryActivities/Draw/LotteryDrawService.cs`

| 行號 | Exception Enum | 訊息 | 多語系 |
|------|----------------|------|--------|
| 113 | `InvalidOperation` | `"活動不存在"` | ✅ |
| 128 | `LIVE_SESSION_DRAW_PROCESSING_CONFLICT` | `"目前有其他抽獎活動正在開獎中，請稍後再試"` | ✅ |
| 162 | `ACTIVITY_STATUS_MISMATCH` | `"狀態已變更，無法加抽"` | ✅ |
| 179 | `InvalidOperation` | `"存在尚未完成的手動或自動開獎，無法進行加抽"` | ✅ |
| 191 | `InvalidOperation` | `"活動正在處理加抽作業，請稍後再試"` | ✅ |
| 230 | `ACTIVITY_NO_ELIGIBLE_PARTICIPANT` | `"所有參加者皆已中獎，無法加抽"` | ✅ |
| 245 | `ACTIVITY_STATUS_MISMATCH` | `"活動狀態不允許手動開獎"` | ✅ |
| 254 | `InvalidOperation` | `"活動尚未開始，無法進行手動開獎"` | ✅ |
| 262 | `InvalidOperation` | `"中獎人數 ({N}) 必須與活動預計中獎人數 ({M}) 相同"` | ✅ |
| 274 | `InvalidOperation` | `"已存在手動開獎記錄，無法再次手動開獎"` | ✅ |
| 284 | `InvalidOperation` | `"已完成自動開獎，無法再進行手動開獎"` | ✅ |
| 304 | `ACTIVITY_NO_ELIGIBLE_PARTICIPANT` | `"目前沒有符合資格的參加者，無法進行開獎"` | ✅ |
| 472 | `InvalidOperation` | `"活動不存在"` | ✅ |

---

## 四、LotteryNotificationService（`ApplicationApiException`）

> 檔案路徑：`src/BusinessLogic/Nine1.Livebuy.BL.Services/LotteryActivities/LotteryNotificationService.cs`

| 行號 | Exception Enum | 訊息 | 多語系 |
|------|----------------|------|--------|
| 99 | `NoData` | `"活動不存在"` | ✅ |
| 119 | `NoData` | `"開獎記錄不存在"` | ✅ |
| 134 | `NoData` | `"此留言不在中獎名單中"` | ✅ |
| 159 | `NoData` | `"直播場次不存在"` | ✅ |

---

## 統計摘要

| 來源 | 總筆數 | ✅ 適合多語系 | ⚠️ 謹慎評估 | ❌ 不建議 |
|------|--------|------------|------------|----------|
| Validators (`WithMessage`) | ~45 | ~43 | 2 | 0 |
| LotteryActivityService | ~45 | ~38 | 6 | 1 |
| LotteryDrawService | 13 | 13 | 0 | 0 |
| LotteryNotificationService | 4 | 4 | 0 | 0 |
| **總計** | **~107** | **~98** | **~8** | **1** |

---

## 實作建議

### 建議的 Translation Module
新增 `backend.lottery` module 於 Translation 平台，集中管理抽獎相關文案：

```
backend.lottery
├── validation.*        ← Validator 的 WithMessage 訊息
├── activity.*          ← LotteryActivityService 業務邏輯錯誤
├── draw.*              ← LotteryDrawService 開獎錯誤
└── notification.*      ← LotteryNotificationService 錯誤
```

### 已完成多語系的項目
目前 `LotteryActivityService.cs:1142` 已使用：
```csharp
Common.Translations.Backend.Service.LotteryActivityService.StatusHasChanged
```
可作為其他文案多語系化的參考範例。

### ⚠️ 含動態狀態值的訊息處理
約 6 筆訊息格式為 `"活動狀態為 {activity.Status}，無法..."` 需要額外處理：
- 方法一：將狀態值也做多語系對應，在文案中用 Placeholder 帶入
- 方法二：維持 hardcode，僅把固定文案部分多語系化
