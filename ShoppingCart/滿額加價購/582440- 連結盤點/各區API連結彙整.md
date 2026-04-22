# Story #582440 — 各區 API 連結彙整

> 盤點商品加價購與滿額加價購各入口的 API 呼叫路徑與篩選機制現狀

---

## 一、路徑總覽

| # | 頁面入口 | MWeb URL 格式 | 最終資料來源 API |
|---|---------|--------------|----------------|
| A | 滿額加價購 — 加購列表頁（ViewOnly） | `/V2/CartExtraPurchase/ExtraPurchaseList/{promotionId}?shopId={shopId}` | FTS: `salepage/cart-extra-purchase/{shopId}/{promotionId}` |
| B | 滿額加價購 — 加購列表頁（購物車進入） | `/V2/CartExtraPurchase/ExtraPurchaseList/{promotionId}?shopId={shopId}&from=cart&cartUniqueKey={key}` | FTS: `salepage/cart-extra-purchase/{shopId}/{promotionId}` |
| C | 滿額加價購 — 購物車 P1 加購區 | `/ShoppingCart/Index?shopId={shopId}`（頁面內嵌，非獨立頁） | Shopping API: `carts/cart-extra-purchase?cartUniqueKey={key}&shopId={shopId}` → 內部呼叫 FTS: `salepage/cart-extra-purchase/{shopId}/{promotionId}` |
| D | 商品加價購 — 加購列表頁 | `/V2/AddOnsSalePage/ExtraPurchaseList/{salePageId}?shopId={shopId}` | FTS: `salepage/add-ons/{shopId}/{salePageId}` |

---

## 二、各路徑詳細說明

### A & B — 滿額加價購加購列表頁（ViewOnly / from=cart）

兩條路徑共用同一個 FTS endpoint，差異僅在前端行為。

```bash
[ClientApp]
  → CartExtraPurchaseController.ExtraPurchaseList(id)
      ↓ id = promotionId
  → ClientApp React 啟動
      ↓
  → FTS: {FtsApiDomain}/salepage-listing/api/salepage/cart-extra-purchase/{shopId}/{promotionId}
      ↓ SPL [Route("/{shopId}/{promotionId}", Order = 1)]
      ↓ BuildPromotionEntitiesAsync()
  ← 回傳商品清單

差異（ViewOnly vs from=cart）：
  - ViewOnly：無 from / cartUniqueKey，唯讀呈現
  - from=cart：額外呼叫 Shopping API 取得已加購數量，可選購加入購物車
```

**MWeb 端篩選**：無（直接呈現 FTS 結果）

---

### C — 滿額加價購購物車 P1 加購區

```bash
[CartClientApp]
  → cart.machine.ts: getCartReachPriceExtraPurchase 狀態
  → cart.service.ts: getCartExtraPurchaseList(cartUniqueKey)
      ↓
  → Shopping API: /shopping/api/carts/cart-extra-purchase?cartUniqueKey={key}&shopId={shopId}
      ↓ Cart Domain 判斷符合的 CartReachPriceExtraPurchase 活動
      ↓ 對每個活動內部呼叫（強推斷）：
      → FTS: salepage/cart-extra-purchase/{shopId}/{promotionId}
  ← 回傳 ICartExtraPurchase[]（含 isAppOnly）

前端篩選（CartClientApp）：
  - filter(!item.isAppOnly)  ← 排除 App 限定活動
  - slice(0, 5)              ← 最多顯示 5 檔活動
  - cartExtraPurchases[0]    ← 每檔活動只取第一個條件的商品清單
  - slice(0, 100)            ← 最多 100 件商品
```

**注意**：P1 加購區顯示的商品是 `cartExtraPurchases[0].salePageList`，**只取第一個條件**，多階梯活動只顯示第一階的商品。

---

### D — 商品加價購加購列表頁

```bash
[ClientApp]
  → AddOnsSalePageController.ExtraPurchaseList(id)
      ↓ id = 主商品 salePageId 或 salePageCode
  → ClientApp React 啟動
      ↓
  Step 1: SalePage V2 API → 取得主商品真實 salePageId
      ↓
  Step 2: FTS: {FtsApiDomain}/salepage-listing/api/salepage/add-ons/{shopId}/{salePageId}
  ← 回傳 SalepageExtraPurchaseEntity（含加購品清單 + 活動設定）

  sortAddOnsSalepageResponse()  ← 依 itemInfo.order 排序（僅排序，非篩選）
  AddOnsProductList.tsx         ← 全部直接渲染
```

**MWeb 端篩選**：無（直接呈現 FTS 結果，僅排序）

---

## 三、各路徑篩選機制現狀（PM 版）

> ✅ 有篩選 = 此條件目前有被排除，使用者看不到  
> ❌ 沒有篩選 = 此條件目前**沒有**被排除，使用者可能會看到

| 篩選項目 | 說明 | A 滿額加價購<br>加購列表（ViewOnly） | B 滿額加價購<br>加購列表（購物車進入） | C 滿額加價購<br>購物車 P1 加購區 | D 商品加價購<br>加購列表 |
|---------|------|:---:|:---:|:---:|:---:|
| **售完商品** | 商品已售完 | ✅ | ✅ | ✅ | ✅（顯示遮罩但不隱藏）|
| **商品頁關閉** | 後台已下架/關閉的商品頁 | ❌ | ❌ | ❌ | ✅ |
| **非販賣時間** | 商品尚未開賣或已結束販賣 | ❌ | ❌ | ❌ | ✅ |
| **隱賣商品** | 後台設為隱藏販售的商品 | ❌ | ❌ | ❌ | ✅（依商店開關）|
| **組合商品** | 需搭配主商品一起購買的組合型商品 | ❌ | ❌ | ❌ | ❌ |
| **分享活動商品** | 需透過 FB 分享後才能購買的商品 | ❌ | ❌ | ❌ | ❌ |
| **SKU 已下架** | 商品下所有規格選項皆已關閉 | ❌ | ❌ | ❌ | ✅ |

> **技術說明（給 RD 參考）**：A/B/C 的篩選缺口根源在 SPL `CartExtraPurchaseService`，D 的篩選邏輯在 SPL `SalepageExtraPurchaseService.GetSalepageSearchDataAsync()`。

---

## 四、篩選缺口彙整

| 篩選項目 | A（ViewOnly） | B（from=cart） | C（P1 加購區） | D（商品加價購） |
|---------|:---:|:---:|:---:|:---:|
| **售完排除** | ✅ SPL 排除 | ✅ SPL 排除 | ✅ SPL 排除 | ⚠️ SPL 回傳，前端顯示遮罩 |
| **商品頁關閉排除**（`IsClosed`） | ❌ 欄位不存在 | ❌ 欄位不存在 | ❌ 欄位不存在 | ✅ SPL 服務層排除 |
| **販賣時間檢查** | ❌ 欄位存在但未使用 | ❌ 欄位存在但未使用 | ❌ 欄位存在但未使用 | ✅ SPL 服務層排除 |
| **隱賣商品控制** | ❌ 一律帶入，無後置 | ❌ 一律帶入，無後置 | ❌ 一律帶入，無後置 | ✅ 依商店開關 |
| **SKU IsShow 檢查** | ❌ 未實作 | ❌ 未實作 | ❌ 未實作 | ✅ SPL 服務層排除 |
| **組合商品排除** | ⚠️ 欄位可取得，未篩選 | ⚠️ 欄位可取得，未篩選 | ⚠️ 欄位可取得，未篩選 | ⚠️ 欄位可取得，未篩選 |
| **分享活動排除** | ❌ SPS 故意帶入，無後置 | ❌ SPS 故意帶入，無後置 | ❌ SPS 故意帶入，無後置 | ❌ SPS 故意帶入，無後置 |
| **App 限定活動排除** | — | — | ✅ MWeb 前端篩除 | — |
| **活動數量上限** | — | — | ✅ 5 檔 | — |
| **商品數量上限** | — | — | ✅ 100 件 | — |
| **加購數量限制檢核** | — | — | — | ✅ `qtyLimit × mainSalePageQty` |
---

## 五、關鍵結論

1. **A/B/C（滿額加價購）與 D（商品加價購）使用不同的 SPL 資料型別**：
   - A/B/C：`GetSalepageListByIdsResponse`（by-ID 批次查詢），**無 `IsClosed` 欄位**
   - D：`SalepageListingResponseDataSalepageModel`（SPS 直接搜尋），**有 `IsClosed` 欄位且有過濾**

2. **`IsClosed`、販賣時間、SKU IsShow、隱賣商品**：商品加價購（D）在 SPL 服務層全部有排除；滿額加價購（A/B/C）全部缺失，是最主要的篩選缺口。

3. **組合商品（`IsSalePageBundle`）和分享活動（`IsShareToBuy`）**：兩個 endpoint 均未排除，是兩者共同的缺口。

4. **P1 加購區（C）**：Shopping API 內部呼叫同一個 `cart-extra-purchase` endpoint，繼承相同的篩選缺口；前端額外有 `isAppOnly` 排除和只取第一個條件的限制。

5. **修改方向**：若要補齊 A/B/C 的篩選，需在 SPL `CartExtraPurchaseService.BuildPromotionEntitiesAsync()` 補上對應條件，並視需要在 `GetSalepageListByIdsResponse` 補充欄位（`IsClosed`、`IsShareToBuy`）。

---

## 六、相關文件

| 文件 | 說明 |
|------|------|
| `加購品列表頁-ViewOnly流程與篩選機制.md` | 路徑 A 完整分析 |
| `加購品列表頁-Cart模式流程與篩選機制.md` | 路徑 B 完整分析 |
| `購物車P1加購區-流程與篩選機制.md` | 路徑 C 完整分析 |
| `商品加價購加購頁-流程與篩選機制.md` | 路徑 D 完整分析 |
