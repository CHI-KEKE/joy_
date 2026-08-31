# `regular_product` 新增欄位規劃 — 定期購活動內容（贈品／折扣／點數）

> 專案：`nine1.regularpurchase.v2`
> 異動資料表：既有表 `regular_product`（**ALTER，加欄位，不新建表**）
> 撰寫日期：2026-07-31

---

## 1. 異動總覽

| 異動類型 | 資料表 | 說明 |
|---|---|---|
| ALTER（加欄位） | `regular_product` | 新增 `rpd_plan_date_begin`／`rpd_plan_date_end`／`rpd_promotion_json` 三個欄位 |

---

## 2. 新增欄位定義

```sql
ALTER TABLE regular_product
  ADD COLUMN rpd_plan_date_begin      timestamp   NULL,          -- 定期購方案生效開始時間
  ADD COLUMN rpd_plan_date_end        timestamp   NULL,          -- 定期購方案生效結束時間（NULL = 永久有效）
  ADD COLUMN rpd_promotion_json  jsonb       NOT NULL DEFAULT '[]';  -- 定期購活動內容（JSON 陣列）

CREATE INDEX idx_regular_product_rpd_date_range     ON regular_product (rpd_plan_date_begin, rpd_plan_date_end);
CREATE INDEX idx_regular_product_rpd_promotion_json ON regular_product USING gin (rpd_promotion_json);
```

| 欄位 | 型別 | Null | 說明 |
|---|---|---|---|
| `rpd_plan_date_begin` | `timestamp` | 可為 NULL | 控制「定期購訂閱」本身是否開放讓會員訂閱的起始時間，屬於**方案層級**，比商品層級的 `rpd_sale_date_begin`／`rpd_sale_date_end` 更上層。即使商品仍在一般販售期間內，定期購訂閱視窗仍可獨立提前關閉或延後開放；`rpd_sale_date_*` 維持原本「商品本身是否可見／可購買」的用途不變 |
| `rpd_plan_date_end` | `timestamp` | 可為 NULL | 定期購訂閱視窗結束時間；`NULL` 表示永久開放訂閱，無到期日 |
| `rpd_promotion_json` | `jsonb` | `NOT NULL DEFAULT '[]'` | 該定期購商品設定的**活動規則陣列**，可同時包含贈品／折扣／點數多筆規則，見第 3 節 |

> `rpd_promotion_json` 使用 `jsonb` + GIN index，便於未來以 `rpd_promotion_json @> '[{"type":"GIFT"}]'` 查詢「哪些設定含贈品規則」等後台統計需求。編輯時採**整段陣列覆寫**（非局部 patch），驗證與 `id` 唯一性交由 Application Layer 處理，DB 端不做 CHECK constraint（jsonb 欄位無法有效表達分支驗證規則）。

---

## 3. `rpd_promotion_json` — 活動內容 JSON Schema

以**陣列**保存該定期購設定的所有活動規則。單一 JSON 物件即可描述「多期數觸發」與「多贈品品項」，不需要為每個期數或每個贈品品項各存一列。

```jsonc
[
  {
    "id": "promo-1",                 // 本設定列內唯一 id，訂單觸發時快照用來回溯來源規則
    "type": "GIFT",                  // GIFT | DISCOUNT | POINT（未來可擴充 COUPON / FREE_SHIP）
    "triggerMode": "INTERVAL",       // INTERVAL | SEQUENCE | THRESHOLD
    "interval": 3,                   // INTERVAL 模式：每 N 期觸發一次（第 3, 6, 9... 期）
    "outOfStockAction": "PROCEED",   // GIFT 專用：贈品缺貨處理，PROCEED=照常建單／SKIP_PERIOD=不建立訂單
    "items": [                       // GIFT 專用：同一次觸發可送多個不同贈品，各自數量獨立
      { "pageId": 50001, "sku": "SKU-GIFT-123", "name": "保溫杯", "picUrl": "https://cdn.../a.jpg", "qty": 2 },
      { "pageId": 50002, "sku": "SKU-GIFT-456", "name": "環保袋", "picUrl": "https://cdn.../b.jpg", "qty": 1 }
    ]
  },
  {
    "id": "promo-2",
    "type": "DISCOUNT",
    "triggerMode": "SEQUENCE",
    "shipSequences": [4, 7, 9],       // SEQUENCE 模式：指定多個期數，同一物件即可描述「第4、7、9期」，不需拆多列
    "discountType": "AMOUNT",         // AMOUNT（固定折抵）/ PERCENT（折數）
    "discountValue": 10.000
  },
  {
    "id": "promo-3",
    "type": "POINT",
    "triggerMode": "THRESHOLD",
    "fromSequence": 3,                // THRESHOLD 模式：從第 N 期起，之後「每一期」都觸發（無需窮舉期數，亦適用無限期方案）
    "pointValue": 20                  // 第 3 期起，每期加贈 20 點
  }
]
```

`outOfStockAction` 可選值：

| 值 | 說明 |
|---|---|
| `PROCEED` | 照常建單：訂單照常成立，會員收到商品及有庫存的贈品（缺貨贈品不出貨，不影響商品訂單成立） |
| `SKIP_PERIOD` | 不建立訂單：本期視為異常不建立，會員收不到該期商品，下期恢復正常建單 |

### 3.1 對照使用者需求範例

| 商業需求 | JSON 表達 |
|---|---|
| 每 3 期送贈品（可多品項不同數量） | `type=GIFT, triggerMode=INTERVAL, interval=3, items=[{sku:123,qty:2},{sku:456,qty:1}]` |
| 指定第 4、7、9 期折 10 元 | `type=DISCOUNT, triggerMode=SEQUENCE, shipSequences=[4,7,9], discountValue=10` |
| 每滿 3 期送 20 點 | `type=POINT, triggerMode=INTERVAL, interval=3, pointValue=20` |
| 第 3 期以後，之後每一期都送 20 點（非每 3 期一次，是從第 3 期起連續每期） | `type=POINT, triggerMode=THRESHOLD, fromSequence=3, pointValue=20` |

### 3.2 型別欄位規則（Application Layer 驗證，非 DB Constraint）

| 條件 | 規則 |
|---|---|
| `triggerMode = INTERVAL` | `interval` 必填（>0），`shipSequences`、`fromSequence` 不可出現 |
| `triggerMode = SEQUENCE` | `shipSequences` 必填且非空陣列，`interval`、`fromSequence` 不可出現 |
| `triggerMode = THRESHOLD` | `fromSequence` 必填（>0），`interval`、`shipSequences` 不可出現；觸發條件為 `期數 >= fromSequence`，無終止期數（除非本設定列本身有 `rpd_ship_limit_count` 限制） |
| `type = GIFT` | `items` 必填且非空陣列，每筆需有 `sku`／`qty`（`qty` > 0）；`outOfStockAction` 必填，僅允許 `PROCEED`／`SKIP_PERIOD` |
| `type = DISCOUNT` | `discountType`、`discountValue` 必填 |
| `type = POINT` | `pointValue` 必填（>0） |
| 上限驗證（GIFT 專用，對應畫面規則） | `items.length <= 10`；每筆 `1 <= qty <= 10`；`sum(items[].qty) <= 50` |
| 期數上限驗證 | 若 `rpd_ship_limit_status = 2`（有限制）：`interval <= rpd_ship_limit_count`；`shipSequences` 內所有值 `<= rpd_ship_limit_count`；`fromSequence <= rpd_ship_limit_count` |

---

## 4. `RegularProductEntity` 對應異動（程式碼落地）

`src\BusinessLogic\Nine1.Regularpurchase.V2.BL.BE\RegularProducts\RegularProductEntity.cs` 需新增三個屬性：

| 新增屬性 | 對應欄位 | 型別 |
|---|---|---|
| `RpdPlanDateBegin` | `rpd_plan_date_begin` | `DateTime?` |
| `RpdPlanDateEnd` | `rpd_plan_date_end` | `DateTime?` |
| `RpdPromotionJson` | `rpd_promotion_json` | `string`（儲存原始 JSON 字串，或反序列化為強型別 `List<PromotionRule>`，視 Repository 層設計而定） |

`ProductAddRequest.cs`／`ProductEditRequest.cs` 需對應新增：

| 新增欄位 | 說明 |
|---|---|
| `PlanDateBegin` / `PlanDateEnd` | 對應畫面「方案生效期間」 |
| `Promotions` | `List<PromotionRuleRequest>`，對應畫面「贈品設定」等活動規則區塊；目前畫面僅提供 GIFT／INTERVAL 操作介面，DISCOUNT／POINT／SEQUENCE／THRESHOLD 為 Schema 預留擴充，尚無對應 UI |

> 本次僅規劃「畫面目前有的 GIFT + INTERVAL」路徑；DISCOUNT/POINT/SEQUENCE/THRESHOLD 為預留擴充能力，實作優先序與 UI 開放時程另行確認。

---

## 5. 未來設計討論（非本次實作範圍，先記錄設計方向）

### 5.1 每期實際觸發的贈品/折扣/點數快照紀錄

`rpd_promotion_json` 只是「規則定義」；當某一期實際觸發規則（例如送出贈品）時，需要有地方記錄「這期實際觸發了什麼、狀態如何」，供後台/客服查詢。

**方向：延伸既有 `regular_order_detail`（`rod_*`），不新建表**，與 `regular_product` 這次的做法一致：

```sql
ALTER TABLE regular_order_detail
  ADD COLUMN rod_promotion_snapshot_json jsonb NULL;
  -- 該期實際觸發的活動快照（贈品/折扣/點數），建單當下寫入；未觸發任何規則時為 NULL
```

顆粒度延續既有 `rod_gift_ts_code`（該期贈品獨立建單後的訂單編號）的精神，`rod_promotion_snapshot_json` 記錄「這期觸發了哪些規則、內容是什麼、各品項狀態如何」：

```jsonc
[
  {
    "promoId": "promo-1",           // 回溯 rpd_promotion_json 裡的 id
    "type": "GIFT",
    "items": [
      { "sku": "SKU-GIFT-123", "name": "保溫杯", "qty": 2, "status": "SUCCESS" },
      { "sku": "SKU-GIFT-456", "name": "環保袋", "qty": 1, "status": "OUT_OF_STOCK" }
    ]
  }
]
```

- ~~贈品實際訂單號仍用既有 `rod_gift_ts_code` 追蹤~~：**此假設已推翻**，依最新討論結論（見 5.2），贈品未來會合併進商品本身那張訂單一起下單，`rod_gift_ts_code`（獨立贈品訂單編號）這個既有欄位的用途需要重新檢視，可能需要廢止或改為其他意義，待建單流程設計定案後處理。
- `rod_promotion_snapshot_json` 只負責記錄「規則內容與各品項狀態」，供客服/後台查詢用；不再依賴獨立訂單號欄位。
- 折扣／點數觸發時，同一 JSON 陣列可放 `type=DISCOUNT`／`POINT` 的快照物件，欄位比照 `rpd_promotion_json` 的規則物件精簡對應。

#### 優點

1. 與 `regular_product` 加 `rpd_promotion_json` 的做法一致，架構風格統一、學習成本低。
2. 查詢單一期別的完整資訊（含贈品內容）不需 JOIN，一次 SELECT 拿到。
3. 不新增表，維運（DDL、遷移、權限）成本最低。

#### 缺點

1. `rod_gift_ts_code`（訂單號）與 `rod_promotion_snapshot_json`（內容快照）兩個欄位分別描述同一件事，未來若沒同步更新容易兜不起來。
2. 若要做「這個 SKU 這個月總共送出幾件」這類跨期統計，得對 `regular_order_detail`（本身是高頻成長表）整表掃 JSON，效能較差、無法建 index 到單一品項層級。
3. 更新單一品項狀態（例如缺貨→事後補貨成功）要整段 JSON 讀出再寫回，不能像關聯表一樣單列 UPDATE。

#### 結論與保留退路

現階段沒有明確的「跨 SKU / 跨期贈品統計報表」需求，先採用上述擴充欄位方案即可（避免過度設計）。
`rod_promotion_snapshot_json` 的 JSON 內容刻意讓每個 `items[]` 都帶獨立 `status`，未來若真的出現統計需求，可以直接寫 migration script 把既有 JSON 資料展開匯入成獨立明細表，不會是死路。

### 5.2 建單流程如何判斷 `outOfStockAction`（Worker 設計方向）

現況確認：`nine1.regularpurchase.v2.worker` 的 `RegularCreateOrderJob.cs` 目前完全沒有贈品相關程式碼，只有「呼叫一次下游 mweb 建單 API、整單 Success/Fail」的商品訂單流程，贈品判斷需全新設計。

討論結論：
1. **贈品合併進商品本身那張訂單一起下單**（同一次 API 呼叫、同一張訂單），不另外建立獨立的贈品訂單。
2. **判斷時機在呼叫下游建單 API 之前**，Worker 先讀取本方案 `rpd_promotion_json` 中 GIFT 規則的 `outOfStockAction`：
   - `PROCEED`：不做贈品庫存 pre-check，直接建單（各贈品品項是否出貨，交由下游 mweb 訂單建立結果決定；細節待與 mweb API 對接規格確認）。
   - `SKIP_PERIOD`：建單前**先做贈品庫存 pre-check**，只要任一贈品品項庫存不足，本期就**不建立訂單**（視為異常），下一期恢復正常排程。
3. 待確認：`SKIP_PERIOD` 情境下，本期「不建立訂單」是否計入 `rgo_ship_count`（會影響下一期配送日期的排程計算），此細節待建單流程子項目實作時再與現有排程邏輯對齊。

---

## 6. 商品頁（pageId）與定期購方案的關聯設計

### 6.1 現況：全有全無擋法，非時間區間判斷

現有 `RegularProductRepository.ExistsByProductAsync(shopId, pageId)` 的檢查邏輯：

```sql
SELECT COUNT(1) FROM regular_product
WHERE rpd_shop_id = @ShopId AND rpd_page_id = @PageId AND rpd_status = 1
LIMIT 1
```

只要該 `pageId` 底下存在**任一筆** `rpd_status = 1`（啟用中）的設定，就視為「已被設定」而擋下新增。此方法被兩處呼叫：

| 呼叫者 | 對應端點 | 時機 |
|---|---|---|
| `ValidateProductAsync` | `POST /api/regular/products/validate` | 新增畫面輸入商品序號當下的即時驗證 |
| `AddProductAsync` | `POST /api/regular/products` | 表單送出、正式 `INSERT` 前的伺服器端最終防呆 |

且新增當下 `RpdStatus` 是寫死 `= 1`（`AddProductAsync` 內），`EditProductAsync` 不會改動此欄位，目前程式碼中**沒有任何路徑會把 `rpd_status` 改回非啟用**。也就是說現況設計是「一個 pageId 一旦被設定過，就永久佔用、沒有結束機制」，與時間區間完全無關。

### 6.2 未來目標：改為時間區間 overlap 判斷

隨著本次新增 `rpd_plan_date_begin`／`rpd_plan_date_end`，方案與商品的關聯改為：**一個 pageId 可以被多個 `regular_product` 設定列關聯，但同一 pageId 底下，不同設定列的生效區間不可重疊（overlap）**。

判斷邏輯調整為：新增/編輯時，掃描同一 `shopId + pageId` 底下所有既有列，只要新填的 `[newBegin, newEnd]` 與既有任一列的 `[rpd_plan_date_begin, rpd_plan_date_end]` 重疊，就擋下。

```sql
SELECT COUNT(1) FROM regular_product
WHERE rpd_shop_id = @ShopId
  AND rpd_page_id  = @PageId
  AND rpd_status   = 1
  AND rpd_plan_date_begin < COALESCE(@NewEnd, TIMESTAMP '9999-12-31')
  AND COALESCE(rpd_plan_date_end, TIMESTAMP '9999-12-31') > @NewBegin
```

`NULL` 視為「無限遠的未來」參與比較，讓永久有效方案（`rpd_plan_date_end = NULL`）也能正確納入 overlap 判斷。

### 6.3 本次範圍定案：生效時間為必填，不支援「永久有效」

依 VSTS #643392（EPIC：定期購支援贈品_202607）需求文字明確載明「生效時間：**必填，無預設值**」「生效結束時間須晚於生效開始時間」，**未提及可留空／永久有效**。因此本次範圍定案如下：

- **API／畫面層強制 `PlanDateBegin`／`PlanDateEnd` 皆為必填**，不接受 null；新增/編輯方案時，若任一欄位為空，前端與後端皆須擋下並提示「此為必填欄位」。
- `regular_product.rpd_plan_date_end` 於 **DB 層仍保留可為 `NULL` 的彈性**（欄位定義不變，見第 2 節），僅是本次透過 API／UI 驗證層強制不得傳入 null，避免因應未來其他建立管道（如舊資料轉置、後續其他 EPIC）需要用到「永久有效」語意時，還要再異動 DB schema。
- 因此，前一版文件中「6.3 永久有效方案會卡死該 pageId」的 side effect 討論，**本次不會發生**（因為不允許建立 `rpd_plan_date_end = NULL` 的方案），該分析予以撤回，改列為下方「已知限制／未來若開放」的備註，供日後若真的要開放永久方案時參考。
- 6.2 節的 overlap 判斷式維持不變（仍相容 `NULL` 比較邏輯），作為未來若開放永久方案時可直接沿用的判斷基礎，不需重新設計。

> **備註（非本次實作範圍，僅記錄以利未來查閱）**：若未來確實開放「效期永久」的方案（`rpd_plan_date_end = NULL`），則會出現「一個 pageId 一旦被設定為永久有效方案，且沒有下架/結束方案的操作入口，該 pageId 就無法再被任何其他方案關聯」的 side effect，屬於業務上可接受、且與現況（6.1）行為一致的設計，但仍需額外設計「方案下架」功能才能解除卡死狀態。

### 6.4 商品頁如何辨別「是否為定期購商品」——不需新增標記欄位

延伸第 10.5 節「贈品項目需標記為定期購使用」的討論，特別釐清**商品頁本身**是否也需要類似的標記機制。查證 `NineYi.Sms` 既有程式碼後，結論是：**不需要，既有欄位＋即時查詢即可滿足，這與贈品品項的標記需求（10.5）是兩件不同的事**。

商品「是否為定期購使用」實際上分兩層意思：

| 層級 | 意思 | 判斷依據 | 資料位置 |
|---|---|---|---|
| 靜態屬性（商品資格） | 這個商品的交期類型是否為「定期購」，決定它**能不能**被設定進定期購方案 | 商品頁本身的交期欄位，既有 `SaleProductShippingTypeDefEnum.RegularOrder = 7`（`BusinessLogic/BE/SaleProducts/SaleProductShippingTypeDefEnum.cs`） | `NineYi.Sms` 商品資料（既有欄位，非本次新增） |
| 動態狀態（目前是否被關聯） | 這個商品**目前實際**有幾組生效中的定期購方案 | 需即時查詢 `nine1.regularpurchase.v2` 的 `regular_product` 表（`rpd_page_id`） | `nine1.regularpurchase.v2` 端資料庫，不在 `NineYi.Sms` 本地 |

- 「商品交期＝定期購」既有欄位已可作為新增方案時「商品資格檢核」的依據（對照 VSTS #643392 需求「檢核商品條件：商品交期為『定期購』」），本次規劃無需新增任何商品層級的標記欄位。
- 「商品目前是否有生效中的方案」屬於動態關聯資訊，**不應在 `NineYi.Sms` 本地另存一個標記欄位去追蹤**，正確作法是透過 9.4／9.5 規劃的查詢 API 即時向 `nine1.regularpurchase.v2` 查詢，避免雙邊資料不同步（例如方案於 v2 端被刪除，但 SMS 端標記忘了同步清除）的風險。
- 此結論與 10.5 節「贈品項目需可辨別為定期購商品使用」**不是同一件事**：10.5 討論的是**贈品品項**（活動小舖庫存商品）需要被標記為「被定期購佔用」，用途是給庫存/活動小舖系統識別，目前確實**沒有現成機制**，10.5 的缺口分析維持不變，仍需另外向需求方確認範圍；本節（6.4）僅釐清「商品頁本身」不需要比照辦理。

### 6.5 釐清順序：新增定期購方案「不會」新增商品頁，是反過來引用既有商品頁

延伸 6.4 節，特別澄清一個常見的順序誤解：**「新增定期購方案」這個動作，完全不涉及新增/建立商品頁，商品頁必須是早就已經存在、且交期類型已設定為「定期購」的既有資料，定期購方案只是去引用它。**

```
【第一步：商品在 SMS 早就已經存在（既有商品管理流程，非本次新增）】
  營運人員在「商品管理」建立一般商品頁時
    → 選擇「交期」欄位 = 「定期購」（ShippingTypeDef = 7，既有選項，非本次新增）
    → 這個商品頁就此被標記為「可設定定期購方案的商品」
        ↓
【第二步：之後，才去「新增定期購方案」畫面，設定既有商品要怎麼跑定期購】
  操作人員輸入「商品序號」（ProductNo，就是這個既有商品的 SalePageId）
    → 呼叫 9.2 驗證 API → 查詢這個 pageId 的 ShippingTypeDef 是否 = 7
    → 是 → 才允許繼續設定配送週期/次數/贈品等
    → 不是 → 擋下並提示「此商品交期非定期購，無法設定」
```

- **確認「是否為定期購商品」的時機，不是商品頁建立當下，而是「新增定期購方案、輸入商品序號當下」**——商品頁本身的「交期＝定期購」屬性，是在商品建立/編輯階段（既有商品管理功能）就已經設定好的既有欄位，定期購方案這邊只負責讀取、驗證，不負責設定。
- 這個「先建立好既有資源、再由定期購方案引用、不會順便新建」的模式，與第 11 章「贈品」的處理方式完全一致（贈品也必須先在既有贈品管理頁面建立好，定期購方案只負責引用，不會呼叫 `CreateSaleProductGift` 去新建贈品），兩者可互相對照理解。
- 對應既有 API：`Api/SalePage/Get/{id}`（`SalePageController.Get` → `SalePageService.Get`）已可一次查到 `ShippingTypeDef`（交期）、`SellingStartDateTime`／`SellingEndDateTime`（商品銷售起訖時間，用於比對 `PlanDateBegin`／`PlanDateEnd`）、`ValidFlag`（商品頁是否有效）等驗證所需欄位，**9.2 節規劃的驗證 API 可直接複用此既有查詢，不需另外設計商品頁資料結構**。

---

## 8. NineYi.Sms（OSM 後台）現況分析

`NineYi.Sms` 是營運人員實際操作的後台系統，未來將由此站台呼叫 `nine1.regularpurchase.v2` 的 API，讓營運人員可在 OSM 畫面上設定定期購方案（含贈品活動）。本章先整理 **NineYi.Sms 目前已有的現況**（已串接的 API、前端頁面骨架），第 9 章再另外規劃本次需新增的內容。

### 8.1 現況：已串接的 API（BFF 代理層模式）

`NineYi.Sms` 目前已有兩組「定期購」功能走 **BFF（Backend For Frontend）代理層**設計：OSM 前端 → `NineYi.Sms` 後端 Controller（做登入驗證與資料轉換）→ 呼叫下游 v2 站台真正的 API。兩組功能皆與本次「定期購方案 + 贈品設定」無關，但其架構是本次新增 API 的**參考範本**。

#### (1) 定期購功能設定（商店層級開關）

| 項目 | 內容 |
|---|---|
| OSM 前端 Controller | `Api/RegularShopSetting`（`RegularShopSettingController.cs`） |
| 對應下游 v2 端點 | `GET /api/regular/shop-settings`、`PUT /api/regular/shop-settings`、`GET /api/regular/shop-settings/history` |
| 用途 | 商店層級「消費者可否自行終止定期購」「消費者可否自行暫停定期購」兩個開關的查詢/更新/異動歷程 |
| Request（`UpdateSettings`） | `ShopId`（long）、`IsCancelRegularEnabled`（bool?）、`IsSuspendRegularEnabled`（bool?）—— 合併更新，至少需提供一個欄位 |
| Response（`GetSetting`） | `ShopId`、`ShopName`、`IsCancelRegularEnabled`（bool）、`IsSuspendRegularEnabled`（bool） |
| 與本次範疇的關係 | **無關**，僅為既有架構參考 |

#### (2) 消費者定期購訂單管理（訂單查詢/建單歷程）

| 項目 | 內容 |
|---|---|
| OSM 前端 Controller | `api/RegularOrderManagement`（`RegularOrderManagementController.cs`） |
| 對應下游 v2 端點 | `GET /api/regular/orders`（分頁查詢）、`GET /api/regular/orders/{id}/history`（建單歷程） |
| 用途 | 客服/營運查詢會員的定期購訂單狀態、建單成功/失敗、暫停/終止操作 |
| Request（`GetList`，摘要） | `ShopId`、`DateOption`（NextShipDate/NewCreateDate）、`StartDate`／`EndDate`、`OrderStatus`（Ordering/Pause/Cancel）、`OrderResult`（Success/Fail）、`MemberMobile`、`MemberCode`、`SalepageTitle`、`SalepageId`、`PlanTitle`、**`PlanId`（定期購方案序號，精確比對）**、`ShipCount`、分頁 `PageNo`／`PageSize` |
| Response（`RegularOrderManagementDataEntity`，摘要） | 訂單主鍵 `Id`、`MemberCode`、**`PlanTitle`／`PlanId`（方案名稱快照與序號）**、收件資訊、`SalepageId`／`SKUId`／`SalepageTitle`、`ShipPeriodDays`、`ShipCount`、`OrderStatus`／`OrderResultText`、各操作按鈕開關（`EnablePauseRegular` 等） |
| HttpClient | `RegularShopSettingHttpClient`（沿用 `Nine1V2.RegularOrder.Url`／`Api.Key` 設定，以 `Flurl.Http` 封裝） |
| 與本次範疇的關係 | **無關**，是查詢「已建立的訂單」，不是「建立/編輯方案」；但其中 `PlanId`／`PlanTitle` 欄位證實 v2 站台已有「方案」概念可被引用查詢 |

> 上述兩組 API 皆採用相同三層架構：`Controller`（登入驗證＋路由）→ `Service`（商業邏輯、下游回應轉換）→ `HttpClient`（`Flurl.Http` 封裝，共用 `Nine1V2.RegularOrder.Url`／`Nine1V2.RegularOrder.Api.Key` 設定）。**本次新增「定期購方案（含贈品設定）」相關 API 時，將直接比照此架構複製一份**（見第 9 章）。

### 8.2 現況：前端頁面骨架已建立但尚無內容

`NineYi.Sms` 前端（React，`ClientApp/package_cloud/commerce/src/pages/RegularPurchase/`）已註冊以下路由，但「新增/編輯方案」頁面目前是空殼：

| Route | 對應頁面 | 現況 |
|---|---|---|
| `RegularPurchaseManagement` | 定期購方案管理（列表頁） | 空殼，僅 `<div>定期購方案管理</div>` |
| `RegularPurchaseTemplate/:mode`（`create`/`edit`/`clone`/`view`） | 新增/編輯/複製/檢視方案 | 空殼，僅依 mode 顯示對應標題文字，**無任何表單欄位、無 Service、無 Type** |
| `RegularPurchaseCustomerOrder` | 消費者定期購訂單管理 | 已完整實作（對應 8.1-(2)） |
| `RegularPurchaseSettings` | 定期購功能設定 | 已完整實作（對應 8.1-(1)） |

也就是說，你的截圖「新增定期購方案」畫面（基本設定＋贈品設定）**應該落地在 `RegularPurchaseTemplate.tsx` 的 `create` 分支，但目前完全沒有實作**。

---

## 9. NineYi.Sms 本次新增規劃

依截圖畫面欄位與本文件第 2、3 節的 `regular_product` 新欄位設計，NineYi.Sms 端需新增以下 API（比照 8.1 的三層 BFF 架構：`RegularProductController` → `RegularProductService` → `RegularProductHttpClient`，命名比照下游 v2 站台同名 `RegularProductController.cs`，直接對應 `regular_product` 資源）：

### 9.1 新增定期購方案

| 項目 | 內容 |
|---|---|
| OSM 前端 Controller（新增） | `Api/RegularProduct/Create`（`RegularProductController`，比照下游 v2 站台同名 `RegularProductController.cs`，直接對應 `regular_product` 資源） |
| 對應下游 v2 端點 | `POST /api/regular/products` |
| Response | 新增結果（成功/失敗訊息），失敗時比照既有 `ValidateProductAsync`／`AddProductAsync` 錯誤訊息文案風格 |
| 對應下游驗證 | 商品序號需為定期購交期、非點數商品、尚未結束銷售；生效開始時間須晚於現在時間 10 分鐘以上，生效結束時間須晚於生效開始時間，且開始/結束時間皆不得超出商品銷售時間範圍；同 pageId 需通過本文件 6.2 節 overlap 檢查 |

**Request 欄位（對應截圖「基本設定＋贈品設定」，依 VSTS #643392 需求核對）**

| 欄位 | 型別 | 說明 |
|---|---|---|
| `ShopId` | long | 商店代碼 |
| `Title` | string | 方案名稱，**非必填**，此名稱不顯示於前台；全形/半形混合上限 20 字，若皆為半形則上限 40 字；輸入或貼上超過上限時前端需自動截斷 |
| `ProductNo` | string | 定期購商品序號／pageId，**必填**；每個方案只能設定一個商品，同一商品可設定於多個方案（受 6.2 overlap 規則限制） |
| `PlanDateBegin` | datetime | 方案生效開始時間，**必填**（依 VSTS 需求本次不支援留空，見第 6.3 節定案） |
| `PlanDateEnd` | datetime | 方案生效結束時間，**必填**（本次不支援「永久有效」，見第 6.3 節定案） |
| `ShipPeriodList` | int[] | 配送週期複選，**必填**，選項限定為 `3/7/10/14/15/30/45/60/90/180/365/730`（天） |
| `ShippingCountMode` | enum | 配送次數設定，**必填**，單選：`Unlimited`（無上限）／`CustomerChoice`（消費者自選次數）／`Fixed`（固定次數）；三個 enum 值定義見下方說明 |
| `ShipLimitCount` | int? | 次數上限值；當 `ShippingCountMode` 為 `CustomerChoice` 或 `Fixed` 時必填，範圍為 **2~99 的正整數**；`Unlimited` 時免填 |
| `AllowOneTimePurchase` | bool | 是否允許消費者在此商品頁「單次購買（不訂閱定期購）」，**必填**，預設 `false`（僅提供『定期購』購買選項）；`true` 時同時提供『定期購』與『僅購買一次』兩種購買方式 |
| `IsGiftEnabled` | bool | 贈送贈品開關（**僅為 OSM 端畫面控制欄位**，控制是否展開贈品設定區塊、`Promotions` 是否必填），**必填**，預設「關閉」；**此欄位不會透傳給 v2 API、也不會存成獨立 DB 欄位**，v2 端由 `Promotions` 陣列是否為空來判斷是否有開啟贈品，詳見下方說明 |
| `Promotions` | `List<PromotionRuleRequest>` | 僅當 `IsGiftEnabled=true` 時必填，結構對應本文件第 3 節 JSON Schema；**本次 VSTS 範圍僅使用 `type=GIFT` 且 `triggerMode=INTERVAL`**（對應需求「配送每滿 X 次，送贈品 Y 件」），`DISCOUNT`／`POINT` 與 `SEQUENCE`／`THRESHOLD` 模式為既有 schema 的預留擴充，本次不實作 |

#### `PromotionRuleRequest` 欄位定義（`Promotions` 陣列內單一元素，本次 VSTS 範圍僅會有 0 或 1 筆）

| 欄位 | 型別 | 說明 |
|---|---|---|
| `Id` | string（GUID） | 本設定列內唯一 id，用於訂單觸發時快照回溯來源規則；**由 OSM 後端在收到 Request 後自動產生（GUID），前端 Request 不需要、也不應該帶入此欄位** |
| `Type` | enum（string） | 活動類型，值域 `GIFT`／`DISCOUNT`／`POINT`；**本次 VSTS #643392 僅使用 `GIFT`**，`DISCOUNT`／`POINT` 為既有 schema 預留擴充，本次不實作、後端可先擋掉非 `GIFT` 的輸入，**必填** |
| `TriggerMode` | enum（string） | 觸發模式，值域 `INTERVAL`／`SEQUENCE`／`THRESHOLD`；**本次僅使用 `INTERVAL`**，其餘為既有 schema 預留擴充，本次不實作，**必填** |
| `Interval` | int | `TriggerMode=INTERVAL` 時必填：每 N 期觸發一次（配送每滿 X 次送贈品的 X），範圍為 `2` 至「最高配送次數」之間的正整數（依 `ShippingCountMode`／`ShipLimitCount` 動態決定上限，見第 9.1 節） |
| `OutOfStockAction` | enum（string） | `Type=GIFT` 專用，值域 `PROCEED`（照常建單）／`SKIP_PERIOD`（不建立訂單），**必填** |
| `Items` | `List<PromotionGiftItemRequest>` | `Type=GIFT` 專用，本次觸發要送的贈品清單，可一次送多個不同贈品；限制 `Items.Count <= 10`、`sum(Items[].Qty) <= 50`，**必填，至少 1 筆** |

#### `PromotionGiftItemRequest` 欄位定義（`Items` 陣列內單一元素，對應畫面每一筆已驗證的贈品項目）

| 欄位 | 型別 | 說明 |
|---|---|---|
| `PageId` | long | 贈品序號，即該贈品自己的 `SalePageId`（見第 11 章查證），**必填**；新增時需先透過既有「批次驗證贈品序號」機制（`CheckIsPromotionFreeGiftValid`）驗證為合法 `PromotionFreeGift` 類型 |
| `Sku` | string | 贈品的 SKU 編號，驗證通過後由後端查詢帶出，前端唯讀顯示 |
| `Name` | string | 贈品名稱，驗證通過後由後端查詢帶出，前端唯讀顯示 |
| `PicUrl` | string | 贈品圖片網址，驗證通過後由後端查詢帶出，前端唯讀顯示 |
| `Qty` | int | 本次送出的數量，操作人員手動輸入，範圍 `1 <= Qty <= 10`，**必填** |

> 對應前端會送入的實際內容範例（**Request 中不含 `id`，由後端產生**）：
> ```json
> "promotions": [
>   {
>     "type": "GIFT",
>     "triggerMode": "INTERVAL",
>     "interval": 3,
>     "outOfStockAction": "PROCEED",
>     "items": [
>       { "pageId": 50001, "sku": "SKU-GIFT-123", "name": "保溫杯", "picUrl": "https://cdn.../a.jpg", "qty": 2 },
>       { "pageId": 50002, "sku": "SKU-GIFT-456", "name": "環保袋", "picUrl": "https://cdn.../b.jpg", "qty": 1 }
>     ]
>   }
> ]
> ```
> 這份範例逐欄位對應到上面 `PromotionRuleRequest`／`PromotionGiftItemRequest` 兩張表，細節說明見第 13.1 節。

> **`ShippingCountMode` enum 值定義**（原命名 `ShipLimitStatus` 經討論後正式改名，理由：舊名稱看不出是在講「配送次數的限制方式」，`ShippingCountMode` 更貼近語意）：
> - `Unlimited`＝1：無上限，持續配送到消費者自行取消訂閱為止
> - `CustomerChoice`＝2：有上限，但配送次數由消費者自己選擇（例如 3~5 次自選）
> - `Fixed`＝3：固定次數，配滿系統設定的次數即自動終止，消費者不能自行選擇（畫面提示「僅支援 APP 25.12 以上版本」，見 12.4 節）
>
> **⚠️ 待討論**：`ShippingCountMode` 這個命名雖然已比原本 `ShipLimitStatus` 更清楚，但仍有人覺得不夠直覺（容易與「配送週期」搞混），**尚未定案**，留待後續確認是否有更好的命名。


> **`Promotions` 內 GIFT 規則細節（對應需求 2、3）**：`interval` 即需求中的 `X`（配送每滿 X 次觸發），範圍為 `2` 至「最高配送次數」之間的正整數（例如選「消費者自選次數 3-5次」則 `interval` 僅能為 2~5；選「固定次數 6 次」則僅能為 2~6）；需求中的 `Y`（送贈品 Y 件）並非單一欄位，而是透過 `items[].qty` 加總表達（`items.length <= 10`、單品 `1 <= qty <= 10`、`sum(items[].qty) <= 50`）；`outOfStockAction` 對應「贈品缺貨處理」（`PROCEED`＝照常建單／`SKIP_PERIOD`＝不建立訂單），為必填欄位。

### 9.2 驗證商品序號（新增畫面即時驗證）

| 項目 | 內容 |
|---|---|
| OSM 前端 Controller（新增） | `Api/RegularProduct/Validate` |
| 對應下游 v2 端點 | `POST /api/regular/products/validate` |
| Request 欄位 | `ShopId`、`ProductNo`、`PlanDateBegin`／`PlanDateEnd`（用於 overlap 檢查） |
| Response | 是否可設定（`IsValid`）、若不可設定之錯誤訊息（例如「此商品序號在此期間已被其他定期購方案設定」，需因應 overlap 邏輯調整既有全有全無錯誤文案，見本文件 6.3） |
| 呼叫時機 | 使用者於新增畫面點擊「新增商品」/輸入商品序號當下 |

### 9.3 編輯定期購方案

| 項目 | 內容 |
|---|---|
| OSM 前端 Controller（新增） | `Api/RegularProduct/Edit` |
| 對應下游 v2 端點 | `PUT /api/regular/products/{productNo}` |
| Request 欄位 | 同「新增」，但不可變更 `ProductNo`（商品序號綁定後不可更換，比照既有 `EditProductAsync` 行為） |
| Response | 更新結果 |

### 9.4 查詢單一方案（供編輯頁載入既有資料 / 列表頁使用）

| 項目 | 內容 |
|---|---|
| OSM 前端 Controller（新增） | `Api/RegularProduct/Get` |
| 對應下游 v2 端點 | `GET /api/regular/products/{productNo}` |
| Response 欄位 | 對應新增/編輯的所有欄位，含 `PlanDateBegin`／`PlanDateEnd`／`Promotions`（`rpd_promotion_json` 反序列化後的結構） |

### 9.5 方案列表查詢（`RegularPurchaseManagement.tsx` 列表頁所需）

| 項目 | 內容 |
|---|---|
| OSM 前端 Controller（新增） | `Api/RegularProduct/GetList` |
| 對應下游 v2 端點 | `GET /api/regular/products`（分頁查詢，比照既有 `GetListAsync`） |
| Request 欄位 | `ShopId`、分頁 `PageNo`／`PageSize`，及既有列表頁篩選條件（方案名稱、商品序號等，待與 UI 設計確認） |
| Response 欄位 | 方案清單摘要（方案名稱、商品序號、生效期間、贈品開關狀態、啟用狀態等） |

> 以上 9.1-9.5 為依現有畫面與資料設計推導出的**規劃草案**，實際 Request/Response 欄位命名與是否需要額外欄位，需待與 `nine1.regularpurchase.v2` 端 API 規格文件（或該專案 Swagger）逐一核對後定案，本節僅先行記錄設計方向與對應關係，作為後續開發依據。

---

## 10. VSTS #643392 需求缺口盤點

比對 VSTS #643392（EPIC：定期購支援贈品_202607）完整需求文字與本文件第 8、9 章現有規劃，整理出以下**尚未涵蓋或涵蓋不完整**的項目，作為後續補件依據。

### 10.1 缺漏：少一支「贈品序號驗證／查詢」API

需求「3. 新增贈品選項流程」描述的是新增畫面上一個**獨立的、即時互動的批次查詢動作**：textarea 貼上多筆贈品序號（單次上限 10 筆）→ 後端逐筆檢查 → 回傳有誤序號清單（彈窗顯示＋提供複製功能）→ 重複序號**直接靜默略過、不列入有誤、不提示**→ 若加總後超過總上限 10 筆，另外顯示「超過贈品選項數量上限」彈窗。

這不是單純把 `Promotions.items` 塞進 9.1 `Add` API 就能滿足的需求，目前 9.1-9.5 規劃**完全沒有這一支 API**，需要新增，例如：

| 項目 | 內容 |
|---|---|
| OSM 前端 Controller（新增） | `Api/RegularProduct/GiftItems/Validate` |
| 對應下游 v2 端點（建議新增） | `POST /api/regular/products/gift-items/validate` |
| Request 欄位 | `ShopId`、`GiftNoList`（string[]，單次最多 10 筆） |
| Response 欄位 | 驗證成功清單（含贈品圖片、名稱、序號、庫存數量）、有誤序號清單（序號不存在或非「活動贈品」類型） |
| 特別規則 | 重複序號（同批次內重複，或與既有已新增贈品重複）**不落入有誤清單，直接靜默略過**，此規則容易被忽略，需特別提醒實作與測試 |

### 10.2 缺漏：9.2「驗證商品序號」的 Response 欄位不完整

需求「商品頁檢核正確：顯示商品名稱、商品主圖、商品銷售時間」，但目前 9.2 的 Response 只規劃了 `IsValid` + 錯誤訊息，**未包含商品名稱／主圖／銷售時間**這幾個畫面顯示用欄位，需要補進 9.2 Response（或另外新增一支查商品資訊的 API）。

### 10.3 缺漏：9.4「查詢單一方案」未涵蓋配送方式／付款方式

需求「5. 配送方式及付款方式」要求顯示該商品的配送方式、付款方式資料（唯讀，來源為商品頁本身既有設定，非本次新增欄位）。目前 9.4 Response 僅列出 `PlanDateBegin`／`PlanDateEnd`／`Promotions`，未納入這兩項商品既有資訊，需確認是否由 `Get` API 一併回傳，或前端另行呼叫既有商品頁 API 取得。

### 10.4 缺漏：「示意圖」彈窗與多語系需求未被記錄

需求「商品頁購買設定」旁有「示意圖」彈窗，且**需支援多語系**。此屬於前端靜態圖片／文案，但因涉及多語系，需走既有 Language Tool 抽多語系流程。目前文件完全未提及此項，容易於開發規劃時遺漏，需另外排入前端任務並確認多語系 Key 命名。

### 10.5 缺漏：贈品項目「辨別為定期購商品使用」的標記機制歸屬未定

需求「2. 贈品設定」與「3. 新增贈品選項流程」皆提到「需可辨別此贈品為定期購商品所使用」。此暗示贈品品項需要某種標記／分類機制，讓其他系統（如活動小舖／庫存管理）能識別「此贈品被定期購佔用」。目前文件僅將贈品資料視為 `rpd_promotion_json` 的內容快照，**未討論此標記要寫在哪裡、由哪個系統負責維護**，可能牽涉跨系統的庫存鎖定邏輯，需另外向需求方確認範圍（純顯示用 tag，或涉及庫存預留機制）。

### 10.6 關聯需求：商品頁狀態依定期購方案數量鎖定（依賴 #649890，本文件未涵蓋）

另有一張關聯需求：商品「目前 0 組定期購方案」時（無論從未建立過，或既有方案已全數刪除降回 0 組），商品頁狀態應鎖定為「關閉」且不可切換；商品新建立第一組方案後解鎖。此需求依賴 #649890（定期購方案刪除規則）尚未定案，且改動範圍屬於「商品頁管理」模組而非本文件的 `RegularProductController`，**目前本文件完全未涵蓋**，僅在此記錄提醒：待 #649890 定案後，需另開文件或章節追蹤此聯動邏輯的開發歸屬。

---

## 11. 贈品定義、新增機制與定期購方案的關聯（查證 NineYi.Sms 既有機制）

延伸第 10.1、10.5 節的缺口，本章查證 `NineYi.Sms` 既有「贈品」相關程式碼，釐清三件事：**贈品目前怎麼被定義／怎麼被新增／怎麼分類活動贈品**，以及**定期購方案要如何跟既有贈品機制關聯**。

### 11.0 先搞懂資料階層：`SalePage` 是商品頁，`SaleProduct` 是頁面底下的銷售項目

在談贈品之前，必須先釐清 `NineYi.Sms` 商品資料的階層關係，否則容易誤解「贈品」跟「一般商品」的資料是怎麼疊在一起的。

```
SalePage（商品頁，SalePage_Id）
   —— 一個「頁面」：決定消費者看到的標題、圖片、售價、上下架時間
   │
   └─ SaleProduct（頁面底下掛的銷售項目，SaleProduct_SalePageId → 指回 SalePage_Id）
        —— 關鍵：一個 SalePage 底下可以掛「多筆」SaleProduct！
        —— 用旗標欄位區分這筆 SaleProduct 在頁面中扮演的角色：
             SaleProduct_IsMajor         = true  → 本頁的「主商品」
             SaleProduct_IsGift          = true  → 本頁綁定的「滿額贈贈品」項目
             SaleProduct_IsExtra         = true  → 本頁的「加價購」項目
             SaleProduct_IsOptionalGift  = true  → 本頁的「任選贈品」項目
```

依 `SaleProductConfiguration.cs` 的 FK 設定 `HasRequired(a => a.SalePage).WithMany(b => b.SaleProduct)`，證實這是「一對多」：**一個 SalePage 可以對應多筆 SaleProduct**。

延伸出兩種完全不同的「送贈品」情境，容易搞混，先在這裡分清楚：

| 情境 | 白話說法 | 資料上怎麼實現 | 是否需要「另外建立」一個贈品商品頁 |
|---|---|---|---|
| **A. 買這個商品就附贈**（商品贈品／買就送） | 買 A 商品，加送隨附的小贈品，贈品本身不是拿來單獨賣的 | 同一個 `SalePage`（就是 A 商品自己的頁面）底下，多掛一筆 `SaleProduct`（`IsMajor=false, IsGift=true` 或 `IsOptionalGift=true`） | **不需要**，贈品資訊直接掛在原商品頁下面，沒有獨立序號 |
| **B. 活動贈品**（滿額贈／滿件贈／贈品券兌換／本次定期購要用的） | 贈品是「共用池」概念，可以被很多個不同活動／方案引用，本身要有獨立庫存、獨立管理 | 贈品是**它自己獨立的一個 `SalePage`**（有自己的 `SalePage_Id`），其下掛一筆 `SaleProduct`（`IsMajor=true`，因為對「贈品」這個頁面而言它自己就是主件），並透過 `SaleProductGiftType = PromotionFreeGift` 標記這個頁面是拿來當「活動贈品」用 | **需要**，必須先在「贈品管理」頁面建立好，取得屬於它自己的 `SalePageId` |

**定期購贈品屬於情境 B**：因為同一個贈品要能被 X 期觸發、被多組定期購方案引用，甚至未來可能同時被滿額贈活動共用，所以走的是「獨立商品頁」的活動贈品機制，不是掛在原商品底下的買就送。

### 11.1 贈品目前如何被定義與新增（既有機制，非本次新增）

`NineYi.Sms` 早已有一套完整的「贈品」管理機制，位於 `SaleProductController`（`Api/SaleProduct/*`）＋ `SaleProductService`：

| 操作 | Controller Route | 說明 |
|---|---|---|
| 查詢贈品庫存清單 | `GetSaleProductGiftList` | 依商店、上下架時間、活動序號、庫存量等條件查詢，回傳 `SaleProductGiftDataEntity` 清單 |
| 查詢單筆贈品資料 | `GetSaleProductGift/{shopId}/{id}` | `id` 即贈品的商品頁序號（`SalePageId`），回傳完整贈品資料（含庫存、圖片、物流/付款方式等） |
| 新增贈品 | `CreateSaleProductGift` | Request 為 `SaleProductGiftUpdateEntity`，本質上是**建立一個新的商品頁（SalePage）**，只是被歸類為「贈品」用途 |
| 更新贈品 | `UpdateSaleProductGift` | 同上，更新既有贈品的庫存、上下架時間等資料 |

也就是說：**贈品在資料層級上就是一個「商品頁」（SalePage）**，跟一般銷售商品共用同一套商品頁基礎設施（`SaleProductId`／`SkuId`／`SaleProductDescId` 等欄位皆存在），差異只在於多了 `SaleProductGiftType` 這個分類欄位（見 11.2）與贈品專屬欄位（`ReqQty` 已發送數量、`SafetyStockQty` 安全庫存等）。

### 11.2 「活動贈品」如何被定義（分類依據）

`SaleProductGiftUpdateEntity.SaleProductGiftType`（string 列舉值）是判斷贈品類型的欄位，前端下拉選單（`saleProductGiftEdit.controller.ts`）目前提供兩種類型：

| `SaleProductGiftType` 值 | 中文名稱 | 說明 |
|---|---|---|
| `SalePageFreeGift` | 商品贈品（買就送） | 綁定特定商品頁，消費者購買該商品頁時直接附贈，不透過活動引擎判斷 |
| `PromotionFreeGift` | **活動贈品**（滿額贈／滿件贈／贈品券兌換） | 透過既有「活動引擎」（`PromotionEngine`）的滿額/滿件規則觸發贈送，`SaleProductService.CheckIsPromotionFreeGiftValid()` 即用來驗證一批商品頁序號是否皆為此類型 |

VSTS #643392 需求中所有提到的「使用『活動贈品』」，對應的就是 `SaleProductGiftType = PromotionFreeGift` 這個既有分類，**本次不需要新增贈品類型，只需要沿用既有的 `PromotionFreeGift` 分類**。

### 11.3 既有「批次驗證贈品序號」機制可直接參考（呼應 10.1 缺口）

`SaleProductService` 已有現成的**批次驗證商品頁序號是否合法**的 pattern，例如：

```csharp
// 驗證是否為合法的活動贈品
public SalePageIdCheckValidResultEntity CheckIsPromotionFreeGiftValid(long shopId, List<long> salePageIds)
{
    var data = this._saleProductRepository.GetPromotionFreeGiftSalePageId(salePageIds, shopId);
    var exceptIds = salePageIds.Where(a => data.Contains(a) == false);

    return new SalePageIdCheckValidResultEntity
    {
        FailCount = exceptIds.Count(),
        FailIds = exceptIds.ToList(),
        SuccessCount = salePageIds.Count - exceptIds.Count()
    };
}
```

`SalePageIdCheckValidResultEntity`（`FailCount`／`FailIds`／`SuccessCount`）這個既有回傳結構，**與 10.1 節規劃的「贈品序號驗證／查詢」API 需求高度吻合**（回傳成功清單與有誤序號清單）。因此 10.1 節建議新增的 `Api/RegularProduct/GiftItems/Validate` 可直接比照 `CheckIsPromotionFreeGiftValid` 的架構實作：

- 驗證邏輯：判斷傳入序號是否存在、且 `SaleProductGiftType = PromotionFreeGift`（活動贈品），不合格者列入 `FailIds`。
- 重複序號的靜默略過規則（VSTS 需求特別要求）**需要額外補在 `RegularProductService` 這層**，因為既有 `CheckIsPromotionFreeGiftValid` 本身不處理輸入去重，此為本次新增邏輯需自行實作的部分。
- 驗證成功後，畫面需顯示的圖片/名稱/庫存數量，可透過既有 `GetSaleProductGift` 或 `GetSaleProductGiftList` 取得，不需重新設計欄位。

### 11.4 贈品與定期購方案的關聯方式（資料流向釐清）

依前述機制，定期購方案與贈品的關聯關係如下：

1. **贈品本身的建立與維護，完全沿用既有 `SaleProduct` 贈品機制**（`SaleProductGiftType = PromotionFreeGift`），本次規劃**不需要**在 `NineYi.Sms` 新增另一套贈品建立流程。
2. **定期購方案（`regular_product`）只儲存贈品的「引用關係」**：`rpd_promotion_json` 內 `items[]` 陣列僅記錄贈品的 `pageId`／`sku`／`name`／`picUrl`／`qty`（見本文件第 3 節），這些內容是**新增方案當下向既有贈品資料查詢後寫入的快照**，並非另建一份贈品主檔。
3. **因此「新增贈品選項流程」（10.1）的批次驗證，驗證的對象就是既有的活動贈品商品頁**，通過驗證後，只是把該贈品的 `pageId`／基本資訊組進 `Promotions` JSON，不會呼叫 `CreateSaleProductGift` 去新建贈品——若營運人員要用的贈品序號不存在，必須**先到既有「贈品管理」頁面建立好該贈品**，定期購新增畫面只負責「引用」而非「建立」。
4. 呼應 10.5 節缺口：既有 `SaleProductGiftType` 分類欄位**只能區分「商品贈品」vs「活動贈品」兩種既有用途，無法標記「此贈品目前被定期購方案佔用」**，這是本次真正的缺口所在——若庫存/活動小舖系統需要辨識「此贈品被定期購鎖定」，需另外設計標記機制（例如新增 `SaleProductGiftType = RegularOrderGift` 分類，或另建一張引用關聯表記錄「哪些贈品被哪些 `regular_product` 引用」），10.5 節的待確認事項維持不變，本節補充的是「可行方案方向」而非最終定案。

### 11.5 完整情境還原：操作人員從 0 到 1 設定「定期購送贈品」的實際步驟

把 11.0-11.4 串成一個完整、好懂的操作情境：

```
【第一步：先在「贈品管理」頁面建立好活動贈品（既有功能，非本次新增）】
  操作人員開啟 EditGift.cshtml（新增贈品視窗）
    → 贈品類型選「活動贈品(滿額贈/滿件贈/贈品券兌換)」= PromotionFreeGift
    → 填寫 Title／OuterId／庫存／圖片／物流付款方式等
    → 送出 → 呼叫 CreateSaleProductGift
    → 後端建立一個全新、獨立的 SalePage（連帶掛一筆 IsMajor=true 的 SaleProduct）
    → 取得這個贈品自己的 SalePageId（例如 50001）
        ↓
【第二步：到「新增定期購方案」畫面，引用剛剛建立好的贈品】
  操作人員開啟贈品設定：
    IsGiftEnabled = 開啟
        ↓
  在「新增贈品選項」流程貼上贈品序號 50001（可一次貼多筆，最多 10 筆／次）
        ↓
  前端送出查詢 → 呼叫 10.1 節規劃的驗證 API（比照既有 CheckIsPromotionFreeGiftValid 架構）
    → 後端逐筆檢查：
       a. 序號是否存在
       b. 該序號的 SaleProductGiftType 是否為 PromotionFreeGift（活動贈品）
       c. 是否與本批次或既有清單重複（重複則靜默略過，不提示）
    → 序號有誤（不存在 / 類型不對）→ 彈窗列出，供「繼續新增」或「重新確認」
        ↓
  驗證通過的序號，連同查到的名稱／圖片／庫存，寫入畫面上的「贈品內容」清單
    → 操作人員設定每個贈品的「數量」（qty）
        ↓
【第三步：設定觸發條件並儲存方案】
  設定「配送每滿 X 次，送贈品」→ 對應 rpd_promotion_json 的 interval
  設定「缺貨處理」→ 對應 outOfStockAction
        ↓
  儲存方案 → 通過 6.2 overlap 檢查等驗證後
    → 將上述贈品清單（pageId/sku/name/picUrl/qty）連同 X／缺貨處理，
      整包寫入 regular_product.rpd_promotion_json（快照，不建立新贈品主檔）
```

**這個流程明確回答三個常見疑問：**

- **Q：定期購的贈品是不是要先額外建立活動贈品？**
  A：**是的，必須先建立。** 定期購方案的新增畫面只負責「輸入序號→驗證→引用」，完全不會幫你新建贈品；如果序號對應的贈品還沒建立，或建立時類型選錯（選了「商品贈品」而非「活動贈品」），驗證都會失敗。
- **Q：既有的一般商品頁能不能直接變成贈品？**
  A：**不能直接變身。** 贈品是它自己獨立的一個 `SalePage`（獨立序號、獨立庫存），跟一般販售商品是兩筆分開的商品頁資料；「一般商品」與「贈品」唯一的橋樑是操作人員手動重新建立一筆贈品，資料上沒有「一鍵轉換」的機制。
- **Q：贈品序號、SalePageId、SaleProductId 是什麼關係？**
  A：**贈品序號＝該贈品自己的 `SalePageId`**。這個 SalePage 底下掛一筆 `SaleProduct`（因為對贈品頁面而言，它自己就是主件，`IsMajor=true`），`SaleProductId` 只是這筆內部銷售項目的識別碼，操作人員平常看到、填寫、引用的都是 `SalePageId`。

---

## 12. 對照截圖畫面 + 實查 `nine1.regularpurchase.v2` 現況：Request 欄位總表與重大落差發現

依 2026-08-03 截圖（新增定期購方案畫面：基本設定／贈品設定／配送付款方式三段），重新逐欄位比對 VSTS #643392 需求文字，**並實際查看 `nine1.regularpurchase.v2` 專案現有程式碼**，發現 SMS 端規劃與 v2 端現況存在重大落差，記錄如下。

### 12.1 截圖畫面逐欄位比對結果

| 畫面欄位 | 對應 Request 欄位 | 比對結果 |
|---|---|---|
| 方案名稱 | `Title` | ✅ 與文件 9.1 節一致（6/20 字數提示、全半形規則） |
| 定期購商品頁序號 | `ProductNo` | ✅ 一致；卡片顯示「商品名稱＋商品銷售時間」證實 9.2 驗證 API 需回傳這兩項 |
| 定期購方案生效時間 | `PlanDateBegin`／`PlanDateEnd` | ✅ 一致，畫面提示「方案生效時間需介於商品銷售時間內」對應 6.3／6.5 節規則 |
| 配送週期設定 | `ShipPeriodList` | ✅ 一致，12 個選項天數（3/7/10/14/15/30/45/60/90/180/365/730）完全吻合 |
| 配送次數設定 | `ShippingCountMode`／`ShipLimitCount` | ⚠️ **新發現缺口**：「固定次數」選項下方有提示文字「僅支援 APP 25.12 以上版本」——VSTS 原始需求文字與文件 9.1 節皆未記錄這條 App 版本相依限制，見 12.4 節 |
| 商品頁購買設定 | `AllowOneTimePurchase` | ✅ 一致，含「示意圖」連結 |
| 贈送贈品 | `IsGiftEnabled` | ✅ 一致 |
| 配送每滿 N 次，送贈品 | `Promotions[].interval` | ✅ 一致，畫面提示「指定次數需為 2 至最高配送次數之間的整數」與文件吻合 |
| 贈品缺貨處理 | `Promotions[].outOfStockAction` | ✅ 一致 |
| 贈品項目（圖片/名稱/序號/庫存/數量/移除） | `Promotions[].items[]` | ✅ 一致，畫面「共 3 種，合計 13/50 件」印證 `items.length<=10`、`sum(qty)<=50` 限制 |
| 配送方式及付款方式 | （唯讀顯示，非 Request 欄位） | ✅ 確認此區塊是唯讀顯示既有商品頁設定（連結導去「商品頁管理」編輯），**不是新增方案的輸入欄位**，對應 9.4 節查詢單一方案的規劃即可 |

### 12.2 重大發現：`nine1.regularpurchase.v2` 目前已有 `RegularProductController`，但完全不支援本次新欄位

實際查看 `C:\91APP\regular\api\nine1.regularpurchase.v2` 專案程式碼，發現該站台**已經有一支 `RegularProductController`（路由 `api/regular/products`）在運作**，但這是**沿用 V1 PHP 舊功能搬遷過來的版本**（註解明寫「對應 PHP ajaxAddRegularProduct」等），**完全沒有實作本次「生效時間」「贈品」相關的任何欄位**：

```csharp
// src/BusinessLogic/.../RegularProduct/ProductAddRequest.cs（v2 端現有，非本次新增）
public class ProductAddRequest
{
    public long ShopId { get; set; }
    public long ProductNo { get; set; }
    public List<object> ProductShippingPeriod { get; set; } = [];   // 配送週期，尚未型別化（object list）
    public int ProductShippingSet { get; set; }                      // 配送次數限制：1=無限制,2=有限制,3=固定次數
    public int? ProductShippingMax { get; set; }                     // 有限制時的上限
    public int? ProductShippingFixed { get; set; }                   // 固定次數
    public int ProductPurchaseOption { get; set; }                   // 購買選項：1=僅定期購,2=定期購+一次性
    public string OperatorName { get; set; } = string.Empty;
}
```

同時查看 `RegularProductEntity`（對應 `regular_product` 資料表現況）：目前只有 `RpdSaleDateBegin`／`RpdSaleDateEnd`（**商品販售時間**，是既有既有欄位，跟本次要新增的「方案生效時間」`rpd_plan_date_begin`／`rpd_plan_date_end` 是完全不同的兩組欄位，不要混淆！），**沒有任何 `rpd_plan_date_begin`／`rpd_plan_date_end`／`rpd_promotion_json` 的痕跡**——證實第 2 節規劃的 DB 異動（ALTER）與 Entity 異動，在 v2 端**目前都還沒有真的落地**，僅停留在本文件的規劃階段。

### 12.3 命名原則確認：既有欄位沿用 v2 命名 mapping，新欄位維持文件原規劃的清楚命名

v2 端現有的 `ProductAddRequest`／`ProductEditRequest`／`RegularProductEntity` 沿用「V1 PHP 搬遷」的命名慣例（帶 `Product` 前綴、int enum），與文件 9.1 節原規劃的欄位命名不一致。討論後確認命名原則：

- **v2 既有欄位（型別/命名已定案、正在運作）**：OSM 端**不改名遷就，直接沿用 v2 既有命名與型別**，呼叫 v2 API 時做轉換／mapping 即可。
- **v2 目前沒有、本次才新增的欄位**：因為雙方都是從零開始，**沒有「既有命名」要遷就**，直接採用文件原規劃、較好懂的命名（`Title`／`PlanDateBegin`／`PlanDateEnd`／`IsGiftEnabled`／`Promotions`），OSM 與 v2 兩端統一用同一套名稱，欄位對欄位透傳，不需要額外 mapping。

| 概念 | OSM 端欄位（文件 9.1 節） | v2 端欄位 | 屬性 | 串接方式 |
|---|---|---|---|---|
| 配送週期 | `ShipPeriodList: int[]` | `ProductShippingPeriod: List<int>`（本次順便型別化，原為未型別化的 `List<object>`） | v2 既有 | OSM 呼叫 v2 時直接把值放入 `ProductShippingPeriod` |
| 配送次數設定 | `ShippingCountMode: enum`（無上限/自選/固定） | `ProductShippingSet: int`（1=無限制,2=有限制,3=固定次數） | v2 既有 | OSM 呼叫 v2 時將 enum 轉換為對應 int 值放入 `ProductShippingSet` |
| 次數上限值 | `ShipLimitCount: int?`（單一欄位） | 拆成兩個：`ProductShippingMax`（有限制時）／`ProductShippingFixed`（固定次數時） | v2 既有 | OSM 依 `ShippingCountMode` 決定把 `ShipLimitCount` 的值放進 `ProductShippingMax` 或 `ProductShippingFixed` 其中一欄，另一欄傳 `null` |
| 購買設定 | `AllowOneTimePurchase: bool` | `ProductPurchaseOption: int`（1=僅定期購,2=定期購+一次性） | v2 既有 | OSM 呼叫 v2 時轉換：`false→1`／`true→2` |
| 方案名稱 | `Title: string` | `Title: string`（**本次新增，雙方統一命名**） | 雙方新增 | 直接透傳，無需轉換 |
| 生效時間 | `PlanDateBegin`／`PlanDateEnd: datetime` | `PlanDateBegin`／`PlanDateEnd: datetime`（**本次新增，雙方統一命名**；注意與 v2 既有 `RpdSaleDateBegin`／`RpdSaleDateEnd`「商品販售時間」是不同欄位，不要混淆） | 雙方新增 | 直接透傳，無需轉換 |
| 贈品開關 | `IsGiftEnabled: bool`（**僅 OSM 端畫面控制欄位**） | **不存在，v2 端無此欄位** | 僅 OSM 端 | `IsGiftEnabled` 只用來控制 OSM 畫面是否展開設定區塊、`Promotions` 是否必填；**不會透傳給 v2 API**，v2 端改以「`Promotions` 陣列是否為空」判斷是否有開啟贈品：空陣列＝未開啟贈品，非空＝已開啟且內含贈品規則 |
| 贈品規則 | `Promotions: List<PromotionRuleRequest>` | `Promotions: List<PromotionRuleRequest>`（**本次新增，雙方統一命名**，內部結構對應第 3 節 JSON Schema） | 雙方新增 | 直接透傳，無需轉換 |

### 12.4 App 版本相依限制（新缺口，補充進 10 章需求缺口盤點）

截圖畫面「固定次數」選項下方文字：「達固定次數會自動終止，消費者無法自行選擇。**僅支援 APP 25.12 以上版本**」。這條規則：

- VSTS #643392 原始需求文字**沒有提及**
- 文件 9.1 節 Request 欄位表**未記錄**
- 隱含意義：若消費者當前使用的 APP 版本低於 25.12，「固定次數」這個 `ShippingCountMode`（即 v2 端 `ProductShippingSet=3`）選項對該會員可能**無法正常運作或需要相容性處理**（例如舊版 APP 呼叫定期購相關 API 時，是否會忽略此設定、或後端要額外判斷 App 版本號）——此為**待與需求方／App 團隊確認的新缺口**，本文件目前僅記錄現象，不做結論。

### 12.5 本次待確認事項彙總

1. **v2 端 `RegularProductController` 需要同步改造**：新增 `Title`／`PlanDateBegin`／`PlanDateEnd`／`Promotions` 欄位（**沿用文件原命名，不加 `Product` 前綴**；`IsGiftEnabled` 僅 OSM 端使用，不需 v2 新增），且 `ProductShippingPeriod` 需從 `List<object>` 改為型別化的 `List<int>`；**這不是只改 SMS 端就能完成的串接，v2 端 Request/Entity/DB 三層都需要同步異動**。
2. **命名原則**：v2 既有欄位（`ProductShippingPeriod` 等）由 OSM 端轉換／mapping 過去沿用；本次雙方新增的欄位（`Title`／`PlanDateBegin`／`PlanDateEnd`／`Promotions`）統一用文件原規劃的清楚命名，OSM 與 v2 直接透傳，無需二次轉換。
3. **「方案生效時間」與「商品販售時間」欄位語意不可混淆**：v2 端 `RegularProductEntity` 已有 `RpdSaleDateBegin`／`RpdSaleDateEnd`（商品販售時間），本次新增的是「方案生效時間」（`PlanDateBegin`／`PlanDateEnd`，DB 對應 `rpd_plan_date_begin`／`rpd_plan_date_end`，見第 2 節），兩者語意不同、欄位名稱也刻意區隔避免混淆。
4. **APP 版本相依的「固定次數」限制**（12.4）需另外向需求方／App 團隊確認範圍與處理方式。

---

## 13. 逐欄位端到端資料流：前端輸入 → OSM Request → 轉換／驗證 → v2 API → 落地 DB 欄位

本章逐一欄位說明從「使用者在畫面輸入」到「最終寫入 v2 端 `regular_product` 資料表」的完整路徑，包含每個欄位在 OSM 後端會做什麼驗證／轉換、呼叫 v2 API 時傳的是什麼、以及 v2 端具體存進哪個 DB 欄位。

| # | 欄位 | 前端行為 | OSM 後端驗證／轉換 | 呼叫 v2 API 欄位 | 最終落地 DB 欄位 |
|---|---|---|---|---|---|
| 1 | `ShopId` | 不需輸入，登入 session 自動帶入 | 原樣放入 | `ShopId` | `regular_product.rpd_shop_id`（既有） |
| 2 | `Title` | 輸入框，非必填，全半形混合上限 20 字／純半形 40 字，超過自動截斷 | 後端二次驗證長度 | `Title`（雙方統一命名，直接透傳） | `regular_product.rpd_title`（**本次新增**） |
| 3 | `ProductNo` | 輸入 pageId，觸發即時驗證，成功後顯示商品名稱＋銷售時間 | 呼叫既有 `Api/SalePage/Get/{id}`，確認商品頁存在、`ShippingTypeDef=7`，取得 `SellingStartDateTime`／`SellingEndDateTime` 供下一步比對 | `ProductNo` | `regular_product.rpd_product_no`（既有） |
| 4 | `PlanDateBegin`／`PlanDateEnd` | 選日期時間，提示「需介於商品銷售時間內」 | ① 比對第 3 步取得的商品銷售時間區間；② 6.2 節 overlap 規則檢查同商品是否有時間重疊的其他方案 | `PlanDateBegin`／`PlanDateEnd`（雙方統一命名，直接透傳） | `regular_product.rpd_plan_date_begin`／`rpd_plan_date_end`（**本次新增**；注意與既有 `rpd_sale_date_begin`／`rpd_sale_date_end`「商品販售時間」不同） |
| 5 | `ShipPeriodList` | 複選框勾選天數 | 驗證值域為合法選項（3/7/10/14/15/30/45/60/90/180/365/730） | 放入 v2 既有 `ProductShippingPeriod`（型別化為 `List<int>`） | v2 既有配送週期欄位（沿用其既有序列化實作） |
| 6 | `ShippingCountMode` | 單選（無上限／自選次數／固定次數），選固定次數顯示 App 版本提示 | 轉換 enum→v2 既有 `int`：`Unlimited→1`／`CustomerChoice→2`／`Fixed→3` | 放入 v2 既有 `ProductShippingSet` | v2 既有 `ProductShippingSet` 對應欄位 |
| 7 | `ShipLimitCount` | 選「自選次數」或「固定次數」才顯示輸入框，範圍 2~99 | 依 `ShippingCountMode` 決定放哪一欄：`CustomerChoice`→v2 `ProductShippingMax`（另一欄傳 `null`）；`Fixed`→v2 `ProductShippingFixed`（另一欄傳 `null`）；`Unlimited`→兩欄皆 `null` | 放入 v2 既有 `ProductShippingMax`／`ProductShippingFixed` 其中一欄 | v2 既有 `ProductShippingMax`／`ProductShippingFixed` 對應欄位 |
| 8 | `AllowOneTimePurchase` | 單選（僅提供定期購／定期購+一次性），附示意圖連結 | 轉換：`false→1`（僅定期購）／`true→2`（定期購+一次性） | 放入 v2 既有 `ProductPurchaseOption` | v2 既有 `ProductPurchaseOption` 對應欄位 |
| 9 | `IsGiftEnabled` | 開關切換，預設關閉，開啟後展開贈品設定區塊 | **僅 OSM 端畫面控制欄位**：`true` 時要求 `Promotions` 必填且至少一筆；`false` 時 `Promotions` 送空陣列 `[]` | **不傳給 v2 API**，此欄位只影響 OSM 端如何組出 `Promotions` 陣列 | **無對應 DB 欄位**（不會有 `rpd_is_gift_enabled`）；v2 端由 `rpd_promotion_json` 是否為空陣列判斷是否有開啟贈品 |
| 10 | `Promotions[].id` | **前端不需輸入、Request 中不包含此欄位** | 後端收到 Request 後，為每筆 `PromotionRuleRequest` 自動產生 GUID 作為 `id` | **不在前端傳入的 Request 內**，由 OSM 後端補上後才組成要存入 v2 的完整 `Promotions` 結構 | 同上，連同後端產生的 `id` 一併整包存入 `rpd_promotion_json` |
| 11 | `Promotions[].type` | 不需選擇（本次畫面僅提供贈品設定 UI），但**前端仍需在 Request 中明確帶入 `"type": "GIFT"`**，不可省略或由後端推斷 | 驗證值域為 `GIFT`／`DISCOUNT`／`POINT`；本次僅開放 `GIFT` 通過，其餘值直接拒絕（**保留欄位供未來 UI 開放 `DISCOUNT`／`POINT` 時直接沿用同一資料流，無需改變傳輸方式**） | 同上，`Promotions` 結構的一部分 | 同上，整包存入 `rpd_promotion_json` |
| 12 | `Promotions[].triggerMode` | 不需選擇（本次畫面僅提供「配送每滿 X 次」UI），但**前端仍需在 Request 中明確帶入 `"triggerMode": "INTERVAL"`**，不可省略或由後端推斷 | 驗證值域為 `INTERVAL`／`SEQUENCE`／`THRESHOLD`；本次僅開放 `INTERVAL` 通過，其餘值直接拒絕（**保留欄位供未來 UI 開放 `SEQUENCE`／`THRESHOLD` 時直接沿用同一資料流**） | 同上，`Promotions` 結構的一部分 | 同上，整包存入 `rpd_promotion_json` |
| 13 | `Promotions[].items[]` | 輸入贈品序號（`SalePageId`），觸發既有「批次驗證贈品序號」機制（`CheckIsPromotionFreeGiftValid`），驗證成功回傳名稱/圖片/庫存供設定數量 | 二次驗證 `items.length<=10`、單品 `1<=qty<=10`、`sum(qty)<=50` | `Promotions`（雙方統一命名，直接透傳整個結構） | `regular_product.rpd_promotion_json`（**本次新增**，整包序列化存入，僅存快照 pageId/sku/name/picUrl/qty，不建立新贈品主檔） |
| 14 | `Promotions[].interval` | 輸入 X，提示「需為 2 至最高配送次數之間的整數」（隨 `ShippingCountMode`／`ShipLimitCount` 動態變化上限） | 依當下 `ShippingCountMode`／`ShipLimitCount` 計算合法上限並驗證 | 同上，`Promotions` 結構的一部分 | 同上，整包存入 `rpd_promotion_json` |
| 15 | `Promotions[].outOfStockAction` | 單選（照常建單／不建立訂單） | 必填驗證 | 同上，`Promotions` 結構的一部分 | 同上，整包存入 `rpd_promotion_json` |

**共同的最後一步**：OSM 後端把上述所有欄位組成單一 Request，呼叫 `Api/RegularProduct/Create`（對應 v2 端 `RegularProductController`），v2 端 Service 層再把整個 Request 拆解，分別寫入 `regular_product` 資料表的既有欄位（`rpd_shop_id`／`rpd_product_no`／`ProductShippingPeriod` 對應欄位等）與**本次新增欄位**（`rpd_title`／`rpd_plan_date_begin`／`rpd_plan_date_end`／`rpd_promotion_json`）。**注意 `IsGiftEnabled` 不在此列，它不會出現在 v2 API 或 DB 欄位中。**

### 13.1 完整 Request Sample（對照上表 15 個欄位，前端實際會送出的內容）

**情境一：開啟贈品（`IsGiftEnabled=true`），配送次數設為「消費者自選次數」**

```json
{
  "shopId": 1001,
  "title": "夏季保養品訂閱方案",
  "productNo": "20240001",
  "planDateBegin": "2026-08-01T00:00:00",
  "planDateEnd": "2026-12-31T23:59:59",
  "shipPeriodList": [30, 60, 90],
  "shippingCountMode": "CustomerChoice",
  "shipLimitCount": 5,
  "allowOneTimePurchase": true,
  "isGiftEnabled": true,
  "promotions": [
    {
      "type": "GIFT",
      "triggerMode": "INTERVAL",
      "interval": 3,
      "outOfStockAction": "PROCEED",
      "items": [
        { "pageId": 50001, "sku": "SKU-GIFT-123", "name": "保溫杯", "picUrl": "https://cdn.../a.jpg", "qty": 2 },
        { "pageId": 50002, "sku": "SKU-GIFT-456", "name": "環保袋", "picUrl": "https://cdn.../b.jpg", "qty": 1 }
      ]
    }
  ]
}
```

**情境二：關閉贈品（`IsGiftEnabled=false`），配送次數設為「固定次數」**

```json
{
  "shopId": 1001,
  "title": "",
  "productNo": "20240002",
  "planDateBegin": "2026-09-01T00:00:00",
  "planDateEnd": "2027-08-31T23:59:59",
  "shipPeriodList": [7, 14, 30],
  "shippingCountMode": "Fixed",
  "shipLimitCount": 6,
  "allowOneTimePurchase": false,
  "isGiftEnabled": false,
  "promotions": []
}
```

- 情境一對照第 13 章表格第 7 列：`shippingCountMode="CustomerChoice"` → OSM 呼叫 v2 時放入 `ProductShippingMax=5`、`ProductShippingFixed=null`
- 情境二對照第 7 列：`shippingCountMode="Fixed"` → OSM 呼叫 v2 時放入 `ProductShippingFixed=6`、`ProductShippingMax=null`
- 情境二 `title` 為空字串示範「非必填」情境；`promotions` 為空陣列時，`isGiftEnabled` 這欄**不會**傳給 v2 API（見第 9 列），v2 端存入 `rpd_promotion_json` 的內容即為空陣列 `[]`
- **`promotions[].id` 刻意未出現在以上 Request Sample 中**：此欄位由 OSM 後端收到 Request 後自動產生 GUID 補上，最終存入 `rpd_promotion_json` 的內容才會包含 `id`（見 13.2 節範例）


### 13.2 `Promotions` 陣列實際內容範例（本次 VSTS #643392 範圍：僅贈品 `type=GIFT`）

當 `IsGiftEnabled=true` 時，OSM 後端會依畫面填寫的贈品設定，組出一筆 `PromotionRuleRequest` 放進 `Promotions` 陣列（本次範圍僅會有 0 或 1 筆，因為 VSTS #643392 只支援單一組「配送每滿 X 次送贈品」規則；陣列結構保留供未來擴充多筆／多類型規則）。**以下是「最終落地 `rpd_promotion_json` 的內容」，`id` 是後端補上後才有的值，前端 Request 本身不含 `id`（見 13.1 節說明）**：

```json
{
  "promotions": [
    {
      "id": "3f7b8e4a-1c2d-4e5f-9a6b-7c8d9e0f1a2b",  // 由 OSM 後端產生的 GUID，前端 Request 不含此欄位；訂單觸發時快照用來回溯來源規則
      "type": "GIFT",                  // GIFT | DISCOUNT | POINT（未來可擴充 COUPON / FREE_SHIP，本次僅用 GIFT）
      "triggerMode": "INTERVAL",       // INTERVAL | SEQUENCE | THRESHOLD（本次僅用 INTERVAL）
      "interval": 3,                   // INTERVAL 模式：每 N 期觸發一次（第 3, 6, 9... 期），對應 Promotions[].interval
      "outOfStockAction": "PROCEED",   // GIFT 專用：贈品缺貨處理，PROCEED=照常建單／SKIP_PERIOD=不建立訂單
      "items": [                       // GIFT 專用：同一次觸發可送多個不同贈品，各自數量獨立
        { "pageId": 50001, "sku": "SKU-GIFT-123", "name": "保溫杯", "picUrl": "https://cdn.../a.jpg", "qty": 2 },
        { "pageId": 50002, "sku": "SKU-GIFT-456", "name": "環保袋", "picUrl": "https://cdn.../b.jpg", "qty": 1 }
      ]
    }
  ]
}
```

- `IsGiftEnabled=false` 時，`Promotions` 傳空陣列 `[]`，這筆 JSON 完全不會產生
- `items[].pageId` 即畫面輸入的「贈品序號」，也就是該贈品自己的 `SalePageId`（見第 11 章查證）
- 這整包 JSON（`{"promotions":[...]}` 或依 Repository 層設計僅存陣列本身）序列化後就是 v2 端 `regular_product.rpd_promotion_json` 欄位的內容



