# CartExtraPurchase API 呼叫流程

## API 總覽

| # | 名稱 | URL | 觸發時機 |
|---|------|-----|----------|
| ① | `getCartExtraPurchase` | `{FtsApiDomain}/salepage-listing/api/salepage/cart-extra-purchase/{shopId}/{promotionId}` | 頁面載入，`init()` |
| ② | `getSellingQtyListNewModel` | 庫存查詢服務 | API ① 完成後，依回傳 SKU IDs 查庫存 |
| ③ | `getCartExtraPurchaseItems` | `{ShoppingDomain}/shopping/api/carts/salepage-cart-extra-purchase?promotionId=...&cartUniqueKey=...` | 僅 cart mode（有 cartUniqueKey） |
| ④ | `insertItem` | 購物車寫入 | 消費者按「確認加入購物車」 |

## 詳細說明

### API ① — 取得加購品清單（FTS）

**完整 URL 組裝：**
```
getFtsApiUrl("salepage-listing/api/salepage/cart-extra-purchase/{shopId}/{promotionId}")
  └─ 自動附加 ?shopId={shopId}&lang={lang}&r={random}
```

**Request 參數：**
- Path: `shopId`、`promotionId`
- Query: `shopId`（重複附加）、`lang`、`r`（cache busting）

**特性：**
- 只帶 `shopId` + `promotionId`，**不帶任何消費者 context**
- 不知道消費者目前購物車有哪些商品
- 不知道消費者是從 Web 還是 APP 來

**回傳型別 `ICartExtraPurchaseResponse`：**
```typescript
interface ICartExtraPurchaseResponse {
    promotionId: number;
    name: string;
    typeDef: string;
    startDateTime: string;
    endDateTime: string;
    memberCollectionId: string;
    cartExtraPurchases: ICartExtraPurchaseCondition[];  // 各門檻條件
}

interface ICartExtraPurchaseCondition {
    conditionId: number;
    reachPrice: number;   // 滿額門檻
    reachPiece: number;   // 滿件門檻
    salePageList: ICartExtraPurchaseSalePage[];
}

interface ICartExtraPurchaseSalePage {
    salepageId: number;
    title: string;
    picUrl: string;
    suggestPrice: number;
    price: number;
    minExtraPurchasePrice: number;
    isSoldOut: boolean;
    soldOutActionType: string;
    order: number;
    skuList: ICartExtraPurchaseSku[];
    extraPurchaseSalepageCode: string;
    // ⚠️ 無 isAppOnly、無通路相關欄位
}
```

### API ② — 庫存查詢

- 依 API ① 的 SKU IDs 查詢即時庫存
- 用於 `filterSoldOutProducts`（前端唯一的過濾邏輯）
- 庫存為 0 → 不顯示該加購品

### API ③ — 取得購物車已選加購品（僅 cart mode）

```
URL: {ShoppingDomain}/shopping/api/carts/salepage-cart-extra-purchase
     ?promotionId={promotionId}&cartUniqueKey={cartUniqueKey}
```

- 只在 `isFromCart = true` 且有 `cartUniqueKey` 時呼叫
- 用於預填消費者已在購物車中選過的加購品數量

### API ④ — 加入購物車

- 消費者點「確認加入購物車」後觸發
- 由 `handleUpdateCart()` 呼叫

## `init()` 執行流程（`CartExtraPurchaseProvider.tsx`，L198-312）

```
init()
  ├─ 呼叫 API ①：getCartExtraPurchase(shopId, promotionId)
  │    └─ 得到加購品清單 + 各條件門檻
  ├─ 呼叫 API ②：getSellingQtyListNewModel(skuIds)
  │    └─ 得到庫存資料
  ├─ filterSoldOutProducts()
  │    └─ 過濾庫存 = 0 的商品（唯一過濾邏輯）
  └─ if (isFromCart && cartUniqueKey)
       └─ 呼叫 API ③：getCartExtraPurchaseItems(promotionId, cartUniqueKey)
            └─ 預填已選數量
```

## FtsApiDomain 來源追蹤

```
Web.config
  └─ appSettings["FtsApiDomain"] = "https://fts.xxx.com"
       └─ InitialReactSetting.cshtml
            └─ window.nineyi.MWeb.FtsApiDomain = "@ConfigurationManager.AppSettings["FtsApiDomain"]"
                 └─ globalVariableProvider.ts
                      └─ getGlobalVariable("MWeb.FtsApiDomain")
                           └─ getFtsApiUrl() in path.utility.ts
```
