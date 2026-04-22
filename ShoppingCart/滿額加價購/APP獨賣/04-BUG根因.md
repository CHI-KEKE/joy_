# 04 — BUG 根因（APP 獨賣）

> 本文件只討論**商品頁維度**：`SalePage_IsAPPOnly`。

---

## 調查結論：現狀不存在 Bug

**`SalePage_IsAPPOnly = true` 的商品作為加購品，在新版流程中完全不受影響。**  
不需要修改任何程式碼。

---

## 為什麼不受影響

### `SalePage_IsAPPOnly` 在新版加購品流程中的讀取狀況

| 系統 / 層級 | 是否讀取此欄位 | 根據 |
|------------|--------------|------|
| SPL `GetSalepageListByIdsResponse` | ❌ **無此欄位** | 加購品清單 response 中不存在 |
| SPL `CartExtraPurchaseService` | ❌ **完全不讀** | 只過濾 `IsSoldOut`（L640） |
| SPL `SalepageModesEnum` | ❌ **無 APP 獨賣 mode** | 只有 PP/A(18禁)/B/S/C/L/M |
| Cart Domain 所有 Processor | ❌ **完全不讀** | 新版流程無任何相關判斷 |
| MWeb 前端 CartClientApp | ❌ **不讀** | `ISalepage` 介面無 isAPPOnly；硬碼 false |

---

## `SalePage_IsAPPOnly` 實際有效的地方

此欄位只在以下三個地方被使用，**都與加購品流程無關**：

| 使用位置 | 用途 |
|---------|------|
| SPL `SalePageInfoEntity.IsAPPOnly` | 商品頁展示 API（單一商品頁） |
| SPL `SkuQueryEntity.IsAPPOnly` | SKU 選擇 Popup |
| SPL `CouponSalepageEntity.IsAPPOnly` | 優惠券適用商品清單 |

---

## 不需要改 Code 的理由

1. 商品 A 正常出現在 Web 端加購清單 ✅
2. 商品 A 可以被加入購物車 ✅
3. 商品 A 不顯示「APP 獨享」限制標籤 ✅
4. 商品 A 可以以加購價格結帳 ✅

以上四項 AC 在現行程式碼下全部成立，無需任何修改。

---

## 歷史脈絡

舊版商品加價購流程（`PurchaseExtraRepository.cs`）確實有讀取 `SalePage_IsAPPOnly`，  
但此流程已被新版 CartExtraPurchase 流程（SPL + Cart Domain API）取代，  
**新流程設計上不繼承此限制**，加購品的促銷身份優先於其原有的銷售通路屬性。
