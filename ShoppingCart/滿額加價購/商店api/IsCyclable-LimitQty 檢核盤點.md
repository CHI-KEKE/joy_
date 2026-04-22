# IsCyclable / LimitQty 檢核盤點

> 對應 API：`POST /Promotion/CreatePromotion`、`POST /Promotion/UpdatePromotion`
> 活動類型：`CartReachPriceExtraPurchase`（滿額加價購）、`CartReachPieceExtraPurchase`（滿件加價購）、`AddOnsSalepageExtraPurchase`（商品加價購）
> 盤點日期：2026-04-22

---

## 零、Spec 對照總覽（實作缺口一目了然）

### CreatePromotion

| 欄位 | Spec 規格 | SCMAPIV2 | PromotionWebAPI | 備註 |
|------|-----------|----------|----------------|------|
| IsCyclable（選填） | 多階不可為 true | ✅ | ✅ | — |
| IsCyclable（選填） | 單階且無限購（LimitQty≤0）不可為 true | ✅ | ✅ | — |
| IsCyclable（選填） | 未填預設為 false | ✅ | ✅ | `bool?` nullable，未填為 null → BL 視為 false |
| IsCyclable（選填） | 活動開始後不可變更 | N/A | N/A | CreatePromotion 無「活動已開始」情境，不適用 |
| LimitQty（必填） | 範圍 1–999 或 -1（不限購）；不可為 0 / <-1 | ✅ | ✅ | — |
| LimitQty（必填） | 必填 | ⚠️ 無 NotNull | ⚠️ 無 NotNull | 型態 `int`，預設 -1，未傳入視為不限購；永遠有值，NotNull 非必要 |
| LimitQty（必填） | 活動開始後不可變更 | N/A | N/A | 同上，CreatePromotion 不適用 |

### UpdatePromotion

| 欄位 | Spec 規格 | SCMAPIV2 | PromotionWebAPI | 備註 |
|------|-----------|----------|----------------|------|
| IsCyclable（選填） | 多階不可為 true | ✅ | ✅ | — |
| IsCyclable（選填） | 單階且無限購（LimitQty≤0）不可為 true | ✅ | ✅ | — |
| IsCyclable（選填） | 未填預設為 false | ✅ | ✅ | — |
| IsCyclable（選填） | **活動開始後不可變更** | ❌ 未實作 | ✅ | SCMAPIV2 缺口，PromotionWebAPI 仍會擋 |
| LimitQty（必填） | 範圍 1–999 或 -1；不可為 0 / <-1 | ✅（已補實作） | ✅ | — |
| LimitQty（必填） | **活動開始後不可變更** | ❌ 未實作 | ✅ | 透過 `IsExtraPurchaseRuleListEqual` 整體比對 |

> **共 2 條 SCMAPIV2 缺口**（已修正 LimitQty range check），後端 PromotionWebAPI 皆有保護，不會造成資料錯誤。
> LimitQty range check 已於 `UpdatePromotionRequestEntityValidator.CheckExtraPurchaseLimitQty()` 補上實作，文案統一回傳 SCMAPIV2 多語系（`invalid_threshold_limit_qty`）。

---

## 零、全活動類型通吃的 IsCyclable 檢核

> `IsCyclable` **非必填**，兩層皆無 `NotNull` / `NotEmpty` 檢核，未傳入預設為 `false`。
>
> 唯一一條「活動類型無關」的 IsCyclable 限制如下（`LimitQty` 無跨類型通吃邏輯）：

| 情境 | 條件（任何活動類型） | PromotionWebAPI 文案 | SCMAPIV2 |
|------|-------------------|--------------------|---------|
| 有設定 `PayTypes` 或 `ShippingTypes` 時，`IsCyclable` 不得為 true | 指定付款 / 配送方式贈品不支援循環累計 | 指定付款/配送方式贈品不可循環累計（**hardcoded，非 i18n key**） | ❌ 未實作 |

**PromotionWebAPI 程式碼位置**
```
C:\91APP\Promotion\webapi\nine1.promotion.web.api\src\
  Common\Nine1.Promotion.Common.Validators\Promotions\CreatePromotionRequestEntityValidator.cs
    CheckPayShippingTypes()  line 115
      條件 → line 123–125  .When(PayTypes 或 ShippingTypes 有值)
      攔截 → line 126  .WithMessage("指定付款/配送方式贈品不可循環累計")   ← 硬編碼

  Common\Nine1.Promotion.Common.Validators\Promotions\UpdatePromotionRequestEntityValidator.cs
    CheckPayTypesShippingTypes()  line 821
      條件 → line 839–841  .When(PayTypes 或 ShippingTypes 有值)
      攔截 → line 842  .WithMessage("指定付款/配送方式贈品不可循環累計")   ← 硬編碼
```

---

## CreatePromotion

每筆 request 先過 **SCMAPIV2** 再打 **PromotionWebAPI**，兩層皆有各自 validator。

| # | 情境 | SCMAPIV2 文案（zh-TW） | PromotionWebAPI 文案（zh-TW） |
|---|------|----------------------|---------------------------|
| 1 | `IsCyclable=true` 且多階活動（`IsMultiLevel=true`） | 累計循環僅適用單階活動，多階活動不支援 | 此活動不支援設定多階循環 |
| 2 | `IsCyclable=true` 且單階但全部 `LimitQty ≤ 0` | 累積循環時，限購數量必須設定 | 需設定單階限購才可啟用循環 |
| 3 | `LimitQty` 不是 `-1` 且不在 `1–999`（0 / < -1 / > 999） | 門檻限購數需為 -1（無限購）或 1–999 的整數 | 限購支援輸入 1-999 數字範圍 |

### SCMAPIV2 程式碼位置

**#1 多階不可循環**
```
C:\91APP\SCMAPIV2\nineyi.scm.apiv2\
  CrossLayer\Validators\Promotions\Rules\CartReachPriceExtraPurchaseValidator.cs  line 120–122
  CrossLayer\Validators\Promotions\Rules\CartReachPieceExtraPurchaseValidator.cs  line 119–121
```

**#2 單階需設限購才可循環**
```
C:\91APP\SCMAPIV2\nineyi.scm.apiv2\
  CrossLayer\Validators\Promotions\Rules\CartReachPriceExtraPurchaseValidator.cs  line 126–128
  CrossLayer\Validators\Promotions\Rules\CartReachPieceExtraPurchaseValidator.cs  line 125–127
```

**#3 LimitQty 範圍**
```
C:\91APP\SCMAPIV2\nineyi.scm.apiv2\
  CrossLayer\Validators\Promotions\Rules\CartReachPriceExtraPurchaseValidator.cs  line 132–134
  CrossLayer\Validators\Promotions\Rules\CartReachPieceExtraPurchaseValidator.cs  line 131–133
```

**i18n（SCMAPIV2）**
```
C:\91APP\SCMAPIV2\nineyi.scm.apiv2\
  CrossLayer\Translations\i18n\NineYi.Scm.ApiV2\backend.validator.promotion_base\zh-TW.json
    cyclable_not_support_multi_level      → 累計循環僅適用單階活動，多階活動不支援
    cyclable_requires_limit_qty           → 累積循環時，限購數量必須設定
    invalid_threshold_limit_qty           → 門檻限購數需為 -1（無限購）或 1–999 的整數
```

### PromotionWebAPI 程式碼位置

**#1 #2 多階不可循環 / 單階需設限購**
```
C:\91APP\Promotion\webapi\nine1.promotion.web.api\src\
  Common\Nine1.Promotion.Common.Validators\Promotions\CreatePromotionRequestEntityValidator.cs
    CheckIsCyclable()  line 516
      #1 → line 519–526  (.WithMessage MultiLevelWithCyclableNotSupport)
      #2 → line 529–536  (.WithMessage OnlyOneLevelAndHasLimitQtyCanSetCyclable)
```

**#3 LimitQty 範圍**
```
C:\91APP\Promotion\webapi\nine1.promotion.web.api\src\
  Common\Nine1.Promotion.Common.Validators\Promotions\CreatePromotionRequestEntityValidator.cs
    CheckCartExtraPurchaseRule()  line 472
      #3 → line 491–494  (.WithMessage LimitQtyMustBetween1999)
```

**i18n（PromotionWebAPI）**
```
C:\91APP\Promotion\webapi\nine1.promotion.web.api\src\
  Common\Nine1.Promotion.Common.Translations\i18n\Nine1.Promotion.Web.Api\
    common.validators.promotions.create_promotion_request\zh-TW.json
      multi_level_with_cyclable_not_support                   → 此活動不支援設定多階循環
      only_one_level_and_has_limit_qty_can_set_cyclable       → 需設定單階限購才可啟用循環
      limit_qty_must_between_1_999                            → 限購支援輸入 1-999 數字範圍
```

---

## UpdatePromotion

| # | 情境 | SCMAPIV2 文案（zh-TW） | PromotionWebAPI 文案（zh-TW） |
|---|------|----------------------|---------------------------|
| 1 | `IsCyclable=true` 且多階活動 | 累計循環僅適用單階活動，多階活動不支援 | 此活動不支援設定多階循環 |
| 2 | `IsCyclable=true` 且單階但全部 `LimitQty ≤ 0` | 累積循環時，限購數量必須設定 | 需設定單階限購才可啟用循環 |
| 3 | `LimitQty` 不是 `-1` 且不在 `1–999` | ❌ 未實作 | 限購支援輸入 1-999 數字範圍 |
| 4 | **活動已開始**後嘗試變更 `IsCyclable` | ❌ 未實作 | 活動已開始，不可變更此欄位 |
| 5 | **活動已開始**後嘗試變更 `LimitQty` | ❌ 未實作 | 活動已開始，不可變更此欄位（ExtraPurchaseRuleList 整體比對含 LimitQty） |

> **備註**：#3、#4、#5 SCMAPIV2 未實作，PromotionWebAPI 層仍會攔截，不會造成資料錯誤。
> 差異影響：這 3 條錯誤目前由 PromotionWebAPI 回傳，非 SCMAPIV2 多語系文案。

### SCMAPIV2 程式碼位置

**#1 #2 多階不可循環 / 單階需設限購**
```
C:\91APP\SCMAPIV2\nineyi.scm.apiv2\
  CrossLayer\Validators\Promotions\UpdatePromotionRequestEntityValidator.cs
    CheckExtraPurchaseIsCyclable()  line 601
      #1 → line 611–613  (.ValidationFailure CyclableNotSupportMultiLevel)
      #2 → line 616–618  (.ValidationFailure CyclableRequiresLimitQty)
```

**#3 #4 #5 — 未實作**
```
（無對應程式碼）
```

### PromotionWebAPI 程式碼位置

**#1 #2 多階不可循環 / 單階需設限購**
```
C:\91APP\Promotion\webapi\nine1.promotion.web.api\src\
  Common\Nine1.Promotion.Common.Validators\Promotions\UpdatePromotionRequestEntityValidator.cs
    CheckIsCyclable()  line 1150
      #1 → line 1153–1160  (.WithMessage MultiLevelWithCyclableNotSupport)
      #2 → line 1163–1170  (.WithMessage OnlyOneLevelAndHasLimitQtyCanSetCyclable)
```

**#3 LimitQty 範圍**
```
C:\91APP\Promotion\webapi\nine1.promotion.web.api\src\
  Common\Nine1.Promotion.Common.Validators\Promotions\UpdatePromotionRequestEntityValidator.cs
    CheckCartExtraPurchaseRule()  line 1082
      #3 → line 1101–1104  (.WithMessage LimitQtyMustBetween1999)
```

**#4 IsCyclable 活動已開始不可變更**
```
C:\91APP\Promotion\webapi\nine1.promotion.web.api\src\
  Common\Nine1.Promotion.Common.Validators\Promotions\UpdatePromotionRequestEntityValidator.cs
    CheckOngoingPromotionModification()
      #4 → line 516–518  (PromotionEngine_IsCyclable != entity.IsCyclable)
```

**#5 LimitQty 活動已開始不可變更**
```
C:\91APP\Promotion\webapi\nine1.promotion.web.api\src\
  Common\Nine1.Promotion.Common.Validators\Promotions\UpdatePromotionRequestEntityValidator.cs
    CheckOngoingPromotionModification()
      #5 → line 594–596  (IsExtraPurchaseRuleListEqual → line 1436–1439 含 LimitQty 比對)
```

**i18n（PromotionWebAPI）**
```
C:\91APP\Promotion\webapi\nine1.promotion.web.api\src\
  Common\Nine1.Promotion.Common.Translations\i18n\Nine1.Promotion.Web.Api\
    common.validators.promotions.update_promotion_request\zh-TW.json
      forbidden_modified_when_promotion_ongoing               → 活動已開始，不可變更此欄位
    common.validators.promotions.create_promotion_request\zh-TW.json  ← UpdatePromotion 共用 Create 的 key
      multi_level_with_cyclable_not_support                   → 此活動不支援設定多階循環
      only_one_level_and_has_limit_qty_can_set_cyclable       → 需設定單階限購才可啟用循環
      limit_qty_must_between_1_999                            → 限購支援輸入 1-999 數字範圍
```

---

## 附錄：商品加價購（AddOnsSalepageExtraPurchase）— PurchaseLimitQty

> API：`POST /Promotion/ModifyAddOnsSalePages`
> 此欄位為 `AddOnsRuleList[0].PurchaseLimitQty`（主商品件數門檻），**非** `ExtraPurchaseRuleList[].LimitQty`，但同屬限購類欄位，一併收錄。

| # | 情境 | SCMAPIV2 文案（zh-TW） | PromotionWebAPI |
|---|------|----------------------|----------------|
| 1 | `PurchaseLimitQty < 1` 或 `> maxPurchaseQty` | 加購品限購件數最少 1 件，最多 {0} 件 | 未確認 |

**SCMAPIV2 程式碼位置**
```
C:\91APP\SCMAPIV2\nineyi.scm.apiv2\
  CrossLayer\Validators\Promotions\Rules\AddOnsSalepageExtraPurchaseValidator.cs
    CheckLimitedAddOnsPurchaseRule()  line 44
      條件 → line 88  entity.AddOnsRuleList.First().PurchaseLimitQty < 1 || > _maxPurchaseQty
      攔截 → line 91  ValidationFailure("LimitedAddOnsPurchase", ModifyAddOnsSalePagesRequest.OverPurchaseQtyLimit)
```

**i18n（SCMAPIV2）**
```
C:\91APP\SCMAPIV2\nineyi.scm.apiv2\
  CrossLayer\Translations\i18n\NineYi.Scm.ApiV2\backend.validator.modify_add_ons_sale_pages_request\zh-TW.json
    over_purchase_qty_limit  → 加購品限購件數最少 1 件，最多 {0} 件
```
