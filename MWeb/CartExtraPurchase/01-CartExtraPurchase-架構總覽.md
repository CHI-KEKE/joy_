# CartExtraPurchase 架構總覽

## 端點資訊

| 項目 | 內容 |
|------|------|
| URL | `V2/CartExtraPurchase/ExtraPurchaseList/{promotionId}?shopId={shopId}` |
| 活動類型 | 滿額/滿件加價購（`CartReachPriceExtraPurchase`） |
| Controller | `WebStore/Frontend/MobileWebMallV2/Controllers/CartExtraPurchaseController.cs` |
| 前端核心 | `CartExtraPurchaseProvider.tsx` |

## 架構說明

```
MVC Controller (CartExtraPurchaseController)
  └─ ExtraPurchaseList(long id)
       └─ ViewBag.PromotionId = id
       └─ return View()   ← 只回 View，無任何後端邏輯

View (ExtraPurchaseList.cshtml)
  └─ React App
       └─ CartExtraPurchaseProvider.tsx  ← 所有業務邏輯在這裡
```

## 兩種進入情境

### 情境 A：從活動詳細頁進入

```
PromotionEngineDetail（活動頁）
  └─ isShowCartExtraPurchaseBlock
       └─ 判斷條件：promotionType === CartReachPriceExtraPurchase
  └─ PromotionExtraPurchaseInfoComponent（加購品資訊區塊）
       └─ 連結到 ExtraPurchaseList 頁
            └─ URL: {domain}/V2/CartExtraPurchase/ExtraPurchaseList/{promotionId}?shopId={shopId}
```

**特性：**
- 沒有 `cartUniqueKey`（不是從購物車來）
- `isFromCart = false`
- 只打 API ①②（不打 API ③）

### 情境 B：從購物車（P1）觸發

```
購物車頁面（P1）
  └─ 促購活動 block（CartReachPriceExtraPurchase 達門檻）
       └─ 導向同一個 ExtraPurchaseList 頁
            └─ 攜帶 cartUniqueKey
```

**特性：**
- 有 `cartUniqueKey`
- `isFromCart = true`
- 打 API ①②③（多一支取得購物車已選加購品）

## 相關檔案位置

| 檔案 | 說明 |
|------|------|
| `CartExtraPurchaseController.cs` | MVC Controller |
| `CartExtraPurchaseProvider.tsx` | React 核心邏輯（含 init、handleUpdateCart） |
| `ExtraPurchaseProductList.tsx` | 加購品清單 UI |
| `ExtraPurchaseBasket.tsx` | 已選加購品菜籃 |
| `BasketProductCard.tsx` | 菜籃商品卡 |
| `CartExtraPurchaseBlock.tsx` | 整合區塊 |
| `models.d.ts` | FTS API 回傳型別定義 |
| `salepageListing.model.ts` | `getCartExtraPurchase` 函式 |
| `cartExtraPurchase.model.ts` | `getCartExtraPurchaseItems` 函式 |
