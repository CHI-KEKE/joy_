# BatchProcess 文件

## 目錄
1. [案例說明](#1-案例說明)

<br>

---

## 1. 案例說明

### 1.1 活動無法批次更新商品頁

<br>

系統監控發現異常，部分促購活動在進行「批次商品頁更新」時被阻擋，導致操作失敗。

<br>

https://monitoring-dashboard.91app.io/d/3dSbCsL4k/shoppingcart-loki-log?orgId=2&refresh=30s[…]var-tid=&var-ExceptionType=&var-Source=&var-ErrorCode=

<br>

**問題關聯 PR**

<br>

❗ 異常 PR : 阻擋純門市活動更新商品頁
https://gitlab.91app.com/commerce-cloud/nine1.promotion/nine1.promotion.web.api/-/merge_requests/1316/diffs

<br>

✅ 修復 PR : 防範性初始化節點
https://gitlab.91app.com/commerce-cloud/nine1.promotion/nine1.promotion.web.api/-/merge_requests/1328/diffs

<br>

**影響範圍與重現方式**

<br>

影響條件：活動類型在觸發商品頁更新時，會進入 促購後台 Collection UI（需要白名單開啟）

<br>

可重現條件（QA 檢查用）：

<br>

1. 建立任一受影響活動（見下方清單）
2. 執行「商店 API」或「批次作業」觸發商品頁更新
3. 預期會被阻擋，無法更新成功

<br>

**受影響活動類型（白名單內）**

<br>

以下活動類型目前會觸發轉入 Collection 模式，若未支援將導致異常：

<br>

- DiscountReachPieceWithFreeGift
- DiscountReachPriceWithFreeGift
- DiscountByMemberWithPrice
- RewardReachPriceWithPoint2
- RewardReachPriceWithRatePoint2
- AddOnsSalepageExtraPurchase
- MultiBuyLowestPriceFree
- RewardReachPriceWithCoupon
- DiscountReachPieceWithRate
- DiscountReachPieceWithAmount
- DiscountReachPriceWithRate
- DiscountReachPriceWithAmount
- PayTypeReachPriceWithAmount

<br>

白名單判斷依據：以活動類型判斷是否進入 Collection UI，並透過商店設定開關：PromotionEngine.IsSwitchToCollectionUI = true

<br>

**注意事項**

<br>

- 測試時請確認是否正確套用白名單設定，否則可能誤以為不影響
- 測試需要測試其他活動類型

<br>

### 1.2 批次異動料號檢核邏輯調整

<br>

從僅支援半形 → 改為支援全型英數字

<br>

**原本檢核規則**

<br>

初始版本僅允許「半形英數字」作為合法料號格式（如：ABC123）

<br>

**調整背景**

<br>

後續由 OMO 團隊反映，實際使用中遇到需支援「全形英數」的情境

<br>

推測來源：

<br>

- 客戶端系統產出的資料可能預設使用全形字元（例：ＡＢＣ１２３）
- 操作人員貼上料號時未留意格式

<br>

此需求經通報並驗證後，進行調整

<br>

**處理方式**

<br>

- 將檢核條件放寬為：允許全型及半形的英數字
- 已依專案需求進行更新與測試

<br>

### 1.3 料號於編輯頁更新後大儲存會被歸0

<br>

**問題描述**

<br>

料號於編輯頁更新後，大儲存會被歸 0

<br>

**背景：GetDetail 取得活動資料**

<br>

當進入編輯頁時，會呼叫 GetDetail API 取得原有活動資料。若料號數量 > 10000（資料爆炸風險），後端不會直接回傳所有料號 Id，而是改為以下方式處理：

<br>

- TargetTypeProductSkuOuterIdTag：回傳已存在的 OuterIdTag（供前端辨識、顯示料號）
- IsOverPromotionProductSkuOuterIdTagLimit：表示是否超過系統設定的料號上限（超過則僅顯示管理 tag，不顯示詳細料號）
- RewardProductSkuOuterIdCount：顯示目前實際的料號數量（提供提示用）

<br>

**問題情境：編輯導致異常操作**

<br>

異常步驟模擬：

<br>

1. 編輯頁重新圈選料號時，系統會更新圈選範圍
2. 若接著將資料儲存（送出 Update API），且這筆資料新增了新的料號，將出現下列狀況

<br>

例如：原資料量為：10002，系統限制上限為：10000，新增料號數為：2

<br>

結果：編輯時移除了 10000 筆原本的料號圈選，僅留下新增的 2 筆 → 實際大儲存僅剩下 2 筆

<br>

**修正**

<br>

在 Update API 中加入防呆處理：若接收到 TargetTypeProductSkuOuterIdTag，則一律不更新料號 Tag 保留原資料，避免資料被不當覆蓋為 0 筆

<br>

### 1.4 商品頁序號未被移除

<br>

**問題描述**

<br>

商品頁序號未被正確移除

<br>

**問題原因**

<br>

在 ReCreateCollectionAndUpdateRelationAsync 方法中，會根據促購活動的類型決定是否要執行 Collection 的重建邏輯。目前僅「給點」類型的活動會觸發這段邏輯，導致其他活動類型未處理到只允許加入。開發時指測試可新增，沒考慮到移除會失敗。

<br>

**詳細背景**

<br>

- 該邏輯僅針對白名單中的活動類型執行
- 當前白名單僅包含：給點 類型活動

<br>

**調整方案**

<br>

- 擴充白名單條件，讓邏輯更加明確
- 將 RewardReachPriceWithCoupon 活動類型加入白名單

<br>

**備註建議**

<br>

後續如仍需根據活動類型決定是否重建 Collection，可考慮：

<br>

- 改為設定驅動（config driven）
- 白名單改存於可設定的資料表或參數

<br>

### 1.5 匯入商品頁序號後，料號不見了

<br>

**問題描述**

<br>

匯入商品頁序號後，原本的料號不見了

<br>

**背景說明**

<br>

批次更新商品頁序號時，系統會依序執行以下流程：

<br>

1. 呼叫 GetPromotionDetail 取得原始促銷資料
2. 將序號資訊帶入 UpdatePromotion，更新整體促銷活動

<br>

**問題原因**

<br>

初次修正不完整：當時未補上既有的料號資訊，導致資料在流程中缺乏 Mapping 對應 → 料號遺失

<br>

大量料號限制導致隱性問題：特定情況下，若料號數量 > 10000，系統僅回傳料號 Count 而非具體料號清單。原因來自 GetPromotionDetail 本身的設計限制（避免資料過載）

<br>

**修正方案**

<br>

修正 Mapping 節點：將既有料號資訊補帶進更新流程

<br>

更新邏輯調整如下：

<br>

- 在匯入商品頁序號時，會一併帶入既有的料號資訊至更新 API
- 若 DB 層中該筆資料的料號與新資料無差異，將不進行實體更新，避免重複寫入與誤刪

<br>

**額外建議（可選）**

<br>

為避免類似問題再發生，建議：

<br>

- 為 GetPromotionDetail 設計一種強制取得完整資料的模式（overrideLimit: true）
- 增加後端告警或日誌：當「實際料號數 > 傳回筆數」時提示有遺漏風險

<br>

### 1.6 範例檔翻譯錯誤

<br>

**問題描述**

<br>

指定商品分類，指定料號匯出，Applicable Products欄位未顯示指定

<br>

**背景說明**

<br>

匯出的欄位資訊有抽 Key, 會動態讀取活動 TargetType，取得 Key

<br>

**問題原因**

<br>

漏加上分類的 TargetType

<br>

**修正方案**

<br>

通知 PO，抽 Key

<br>

### 1.7 HK商店匯出料號時，活動序號欄位應為英文

<br>

**問題描述**

<br>

HK商店匯出料號時，活動序號欄位應為英文

<br>

**背景說明**

<br>

匯出欄位會抽 key

<br>

**問題原因**

<br>

當初的開發者 key 值沒有 mapping 正確，因此只能吐出預設值

<br>

**修正方案**

<br>

程式修正取得正確的 key 值

<br>