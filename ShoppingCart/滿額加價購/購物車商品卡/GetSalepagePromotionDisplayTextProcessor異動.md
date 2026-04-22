# GetSalepagePromotionDisplayTextProcessor 異動說明

**檔案路徑**：`Shopping\src\BusinessLogic\Nine1.Shopping.BL.Services\CartProcessor\GetSalepagePromotionDisplayTextProcessor.cs`

---

## 異動一：`_promotionSupportMemberCollectionTypes` list（line 211-212）

### 變更內容

```csharp
// 修改前
//// 購物車滿額加價購
PromotionEngineTypeDefEnum.CartReachPriceExtraPurchase,

// 修改後
//// 購物車滿額加價購
PromotionEngineTypeDefEnum.CartReachPriceExtraPurchase,
//// 購物車滿件加價購
PromotionEngineTypeDefEnum.CartReachPieceExtraPurchase,   ← 新增
```

### 用途

`_promotionSupportMemberCollectionTypes` 是「**針對 MemberCollection 檢核資格的活動類型**」白名單。  
當活動類型在這個 list 裡，才會進入 MemberCollection 的資格驗證流程（例如限定特定會員身分才能參與的活動）。

### 影響

| | 修改前 | 修改後 |
|---|---|---|
| 滿件加價購有 MemberCollection 限制時 | 資格檢核被略過 ❌ | 正確執行資格驗證 ✅ |

---

## 異動二：`cartExtraPurchasePromotionList` filter（line 362-366）

### 變更內容

```csharp
// 修改前
//// 購物車滿額滿件活動
var cartExtraPurchasePromotionList = context.Data.PromotionInfoList
    .Where(x => x is { SourceType: SourceTypeEnum.Promotion,
                        Type: PromotionEngineTypeDefEnum.CartReachPriceExtraPurchase })
    .ToList();

// 修改後
//// 購物車滿額滿件活動
var cartExtraPurchasePromotionList = context.Data.PromotionInfoList
    .Where(x => x is { SourceType: SourceTypeEnum.Promotion } &&
               (x.Type == PromotionEngineTypeDefEnum.CartReachPriceExtraPurchase ||
                x.Type == PromotionEngineTypeDefEnum.CartReachPieceExtraPurchase))
    .ToList();
```

> 原本使用 C# pattern matching 語法（`x is { A: val1, B: val2 }`），但此語法無法對同一欄位比對多個值，
> 改為標準 `||` 條件以同時納入兩種類型。

### 用途

這個 list 傳入 `ArrangeCartExtraPurchasePromotionDisplayTextAsync()`，作為**加購品商品卡（正常清單）文案產生**的活動資料來源。

```
cartExtraPurchasePromotionList
  └─ ArrangeCartExtraPurchasePromotionDisplayTextAsync()
       └─ matchExtraPurchasePromotions = list.Where(x.IsMatch)
       └─ GetSalepageDiscountDisplayAsync(CartExtraPurchase, skuId, optionalTypeId)
            └─ CartReachPieceExtraPurchaseDisplayService.GetPromotionHintText()
                 → DisplayText: "{活動名稱}累積滿件 N件"
```

### 影響

| | 修改前 | 修改後 |
|---|---|---|
| 滿件加價購 promotionInfo 是否進入 list | ❌ 被過濾掉，list 中無滿件資料 | ✅ 正確納入 |
| 正常清單加購品商品卡文案（情境一） | 無文案（DisplayText 為空） ❌ | `{活動名稱}累積滿件 N件` ✅ |

---

## 兩處異動關係總覽

| 異動 | 影響層面 | 若未修改的後果 |
|---|---|---|
| `_promotionSupportMemberCollectionTypes` 補上 `CartReachPieceExtraPurchase` | MemberCollection 資格驗證 | 有限定會員條件的滿件活動，資格檢核不執行 |
| `cartExtraPurchasePromotionList` filter 補上 `CartReachPieceExtraPurchase` | 情境一：正常清單加購品文案來源 | promotionInfo 撈不到 → 文案為空 |
