# 滿額加價購 — FB 分享後購買（IsShareToBuy）邏輯現狀

## 概述

說明滿額加價購（CartExtraPurchaseService）在顯示加購品清單時，
對於「FB 分享後購買（IsShareToBuy）商品」的處理現狀。

---

## IsShareToBuy 是什麼

- SPS index 欄位：`isShareToBuy`（bool）
- **意義**：商品被設定為「需要在 FB 分享後才能購買」的特殊商品
- SPS 預設行為：若不傳 `IsIncludingShareToBuy = true`，此類商品**不會出現**在搜尋結果中

---

## 完整呼叫路徑

```
前端 React
  GET ${FtsApiDomain}/salepage-listing/api/salepage/cart-extra-purchase/{shopId}/{promotionId}
  ↓
CartExtraPurchaseController
  ↓
CartExtraPurchaseService.BuildSalePageDictionaryAsync()
  ↓
this._salepageService.SalepagesSearchByIdsAsync(
    shopId, salePageIds, headerParameters, cleanCache,
    includeSalePageGroup: false,
    includeInvisibleSalepage: true)
  ↓
SalepageService.SalepagesSearchByIdsAsync() 內部 L235：
  requestEntity.Filters.IsIncludingInvisible = includeInvisibleSalepage (= true)
  requestEntity.Filters.IsIncludingShareToBuy = true   ← 硬寫（固定包含 ShareToBuy 商品）
  ↓
SearchForPresentationAsync（SPS 搜尋）→ MapResponse()
  ↓ 回傳 GetSalepageListByIdsResponse（無 IsShareToBuy 欄位）
  ↓
BuildPromotionEntitiesAsync()
  if (salepage.IsSoldOut) continue;   ← 只排除售完
  // IsShareToBuy 判斷？ ← ❌ 沒有，且 GetSalepageListByIdsResponse 根本沒有此欄位
  ↓
回傳加購品清單（ShareToBuy 商品顯示）
```

---

## GetSalepageListByIdsResponse 的 IsShareToBuy 欄位現狀

| 欄位 | 狀態 | 說明 |
|------|------|------|
| `IsShareToBuy` | ❌ **不存在** | `GetSalepageListByIdsResponse` 無此欄位 |
| `SalePageModes` | ✅ 存在 | 但 `SalepageModesEnum` 沒有 ShareToBuy 對應的 Mode 字母 |

> `SalepageModesEnum` 的 Mode 清單：
> - `A` = 18禁 / `B` = 搶先曝光 / `C` = 組合商品 / `L` = 門市 / `M` = OMO / `PP` = 點加金 / `S` = 規格表
> - **沒有 ShareToBuy 的 Mode 代碼**，無法透過 `SalePageModes` 識別

---

## 為何 IsIncludingShareToBuy = true 是「硬寫」

`SalepageService.SalepagesSearchByIdsAsync()` 的設計初衷是 **by-ID 查詢**：  
呼叫方已知道要查哪些 ID（由 promotionEngine 設定決定），所以不分商品類型全部拿回來，  
由呼叫方（CartExtraPurchaseService）自行決定後置過濾邏輯。

```csharp
// SalepageService.cs L235
requestEntity.Filters.IsIncludingShareToBuy = true;  // 硬寫，by-ID 查詢一律全拿
```

---

## 與商品加價購的對比

| | 商品加價購 | 滿額加價購 |
|--|-----------|----------|
| SPS 是否含入 ShareToBuy | ✅ `IsIncludingShareToBuy = true`（Service 主動設定）| ✅ `IsIncludingShareToBuy = true`（`SalepageService` 硬寫）|
| Response 是否有 `IsShareToBuy` 欄位 | ✅ `SalepageListingResponseDataSalepageModel.IsShareToBuy` | ❌ `GetSalepageListByIdsResponse` 無此欄位 |
| C# 是否排除 ShareToBuy | ❌ 無排除 | ❌ 無排除（且無欄位可用）|
| 最終行為 | **ShareToBuy 商品顯示** | **ShareToBuy 商品顯示** |

---

## 對 Story #582440 的意涵

### 現狀已對齊
商品加價購與滿額加價購**行為一致**：兩者都顯示 ShareToBuy 商品。

- Story AC「對照商品加價購」→ 商品加價購顯示 ShareToBuy → 滿額加價購也顯示 → **✅ 不需要修改**

### 若 AC 明確要求「排除 ShareToBuy 商品」

這屬於**超出「對照」基準的新需求**，修改步驟如下：

**Step 1：`GetSalepageListByIdsResponse` 新增屬性**
```csharp
/// <summary>
/// 是否為 FB 分享後購買商品
/// </summary>
public bool IsShareToBuy { get; set; }
```

**Step 2：`SalepageService.MapResponse()` 補映射**
```csharp
IsShareToBuy = salepage.IsShareToBuy,
```

**Step 3：`CartExtraPurchaseService.BuildPromotionEntitiesAsync()` 加排除**
```csharp
if (salepage.IsShareToBuy) continue;   // 排除 FB 分享後購買商品
```

> ⚠️ 此修改需先確認需求（Task #597235），確認 PM/PO 是否明確要求排除，
> 或僅需「對照商品加價購」（即保持現狀顯示）。

---

## 相關檔案路徑

| 檔案 | 路徑 |
|------|------|
| 滿額加價購 Service | `C:\91APP\Salepage\salepage-listing\...\Cart\CartExtraPurchaseService.cs` |
| SalepageService（硬寫處）| `C:\91APP\Salepage\salepage-listing\...\Salepage\SalepageService.cs`（L235）|
| Response Model | `C:\91APP\Salepage\salepage-listing\...\GetSalepageListByIdsResponse.cs` |
| 來源 Response Model | `SalepageListingResponseDataSalepageModel.cs`（L117-118 IsShareToBuy 欄位）|
