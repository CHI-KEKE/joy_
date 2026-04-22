# CreatePromotion API — 支援滿件加價購 實作規劃

> Story：[#599213](https://91appinc.visualstudio.com/MoneyLogistic/_workitems/edit/599213)
> Sprint：193
> 對應 API：`POST /Promotion/CreatePromotion`、`POST /Promotion/UpdatePromotion`
> 專案：SCMAPIV2 (`C:\91APP\SCMAPIV2\nineyi.scm.apiv2`)

---

## 一、背景說明

既有系統已支援：
- `CartReachPriceExtraPurchase`（滿額加價購，= 1048576）
- `CartReachPieceExtraPurchase`（滿件加價購，= 2097152）

TypeDef enum、`PromotionConst.Nine1PromotionSwitchType`、`ExtraPurchaseRuleList` 欄位、`IsCartExtraPurchaseThresholdEligible` 欄位皆已完成。

本次 Story 剩餘工作：
1. 為滿件加價購新建 Validator 並完成 DI 註冊
2. 在 `ExtraPurchaseRuleList` 的規則條件 Entity 新增新欄位（`IsCyclable` 累計循環、`ThresholdLimitQty` 門檻限購數）
3. 更新既有滿額加價購 Validator 同步支援新欄位

---

## 二、Request 路徑

```
POST /Promotion/CreatePromotion
  │
  ├─ [ValidateModel] ActionFilter
  │     └─ CreatePromotionRequestEntityValidator.Validate()
  │           └─ PromotionBaseValidator<T>.Validate()  ← 覆寫版本
  │                 ├─ ① ResolveNamed<IValidator<PromotionBaseEntity>>(TypeDef)
  │                 │       → CartReachPieceExtraPurchaseValidator  ← [新建]
  │                 └─ ② PromotionBaseValidator 通用規則 + Create 自訂規則
  │
  └─ PromotionService.Create()
        ├─ Nine1PromotionSwitchType.Contains("CartReachPieceExtraPurchase") → true ✅
        └─ GetNine1PromotionCreateBody()
              └─ ExtraPurchaseRuleList = entity.ExtraPurchaseRuleList  (直接傳遞)
                    └─ Nine1PromotionWebApiHttpClient.Create()
                          → POST /api/promotion-rules/create (促購中心)
```

---

## 三、修改清單（按路徑順序）

### 【改動 1】`PromotionEngineExtraPurchaseRuleEntity.cs`

**路徑**：`BusinessLogic/BE/Promotions/PromotionEngineExtraPurchaseRuleEntity.cs`

**原因**：
`ExtraPurchaseRuleList` 每筆條件的 Entity，同時被 SCMAPIV2 `PromotionBaseEntity` 與 `Nine1Promotions.CreatePromotionRequestEntity` 引用，加在這裡一次到位，不需額外改 PromotionService 的 mapping。

**新增欄位**：

| 欄位名稱 | 型別 | 預設值 | 說明 |
|----------|------|--------|------|
| `IsCyclable` | `bool` | `false` | 活動單階門檻是否累計循環；僅適用單階，多階不支援 |
| `ThresholdLimitQty` | `int` | `-1` | 門檻限購數；`-1` = 無限購，`1–999` = 限購數量 |

```csharp
// 新增於現有三個欄位之後：

/// <summary>
/// 活動單階門檻是否累計循環（false = 不累計，true = 累計循環）
/// </summary>
/// <remarks>僅適用單階活動，IsMultiLevel = true 時不可設 true</remarks>
public bool IsCyclable { get; set; } = false;

/// <summary>
/// 門檻限購數（-1 = 無限購；1-999 = 限購數量）
/// </summary>
public int ThresholdLimitQty { get; set; } = -1;
```

---

### 【改動 2】`CartReachPieceExtraPurchaseValidator.cs`（新建）

**路徑**：`CrossLayer/Validators/Promotions/Rules/CartReachPieceExtraPurchaseValidator.cs`

**原因**：
`PromotionBaseValidator.Validate()` 會用 TypeDef 字串做 Named Resolve。`CartReachPieceExtraPurchase` 目前沒有任何 Named Validator，呼叫 API 時直接 DI Resolve 失敗。需新建此 Validator。

**驗證規則**：

| 驗證項目 | 條件 | 錯誤訊息 key |
|----------|------|-------------|
| ExtraPurchaseRuleList 必填 | `== null \|\| !Any()` | `CartExtraPurchseMustHave` |
| ReachPiece 必須 > 0 | `Any(rule => rule.ReachPiece <= 0)` | 新增 `ReachPieceMustHaveAndOverZero` |
| 不得與折價券合併使用 | `CanUseECoupon == true` | `CartExtraPurchaseCanNotUseEcoupon` |
| 累計循環不支援多階 | `ExtraPurchaseRuleList.Any(r => r.IsCyclable) && IsMultiLevel` | 新增 `CyclableNotSupportMultiLevel` |
| ThresholdLimitQty 合法值 | 非 `-1` 且非 `1–999`（即值為 0 或 < -1 或 > 999）| 新增 `InvalidThresholdLimitQty` |
| 同時生效活動數上限 | 讀 ShopStaticSetting `CartReachPieceExtraPurchaseConcurrentMaxCount` | `OverConcurrentMaxCount` |

**類別結構（參考 CartReachPriceExtraPurchaseValidator）**：

```csharp
public class CartReachPieceExtraPurchaseValidator : AbstractValidator<PromotionBaseEntity>
{
    // 注入 IShopStaticSettingService、IPromotionEngineRepository（同滿額）
    
    public CartReachPieceExtraPurchaseValidator(
        IShopStaticSettingService shopStaticSettingService,
        IPromotionEngineRepository promotionEngineRepository)
    {
        this.Custom(this.ValidateConcurrentMaxCount);
        this.Custom(this.CheckExtraPurchaseRuleList);
    }

    // ValidateConcurrentMaxCount：
    //   讀 ShopStaticSetting key = "CartReachPieceExtraPurchaseConcurrentMaxCount"
    //   邏輯與 CartReachPriceExtraPurchaseValidator 完全相同

    // CheckExtraPurchaseRuleList：
    //   1. ExtraPurchaseRuleList 必填
    //   2. 任一 rule.ReachPiece <= 0 → 錯誤
    //   3. CanUseECoupon == true → 錯誤
    //   4. 任一 rule.IsCyclable == true 且 IsMultiLevel == true → 錯誤
    //   5. 任一 rule.ThresholdLimitQty 不為 -1 且不在 1–999 → 錯誤
}
```

---

### 【改動 3】`ValidatorModule.cs`

**路徑**：`CrossLayer/Modules/BL/ValidatorModule.cs`

**原因**：Named Validator 必須手動在 Autofac 中以 TypeDef 字串 key 註冊，否則 Resolve 時拋出例外。

**改動（在第 58 行 CartReachPriceExtraPurchaseValidator 之後新增一行）**：

```csharp
// 現有（第 58 行）：
builder.RegisterType<CartReachPriceExtraPurchaseValidator>()
       .Named<IValidator<PromotionBaseEntity>>(nameof(PromotionEngineTypeDefEnum.CartReachPriceExtraPurchase));

// 新增（第 59 行）：
builder.RegisterType<CartReachPieceExtraPurchaseValidator>()
       .Named<IValidator<PromotionBaseEntity>>(nameof(PromotionEngineTypeDefEnum.CartReachPieceExtraPurchase));
```

---

### 【改動 4】`CartReachPriceExtraPurchaseValidator.cs`（補充）

**路徑**：`CrossLayer/Validators/Promotions/Rules/CartReachPriceExtraPurchaseValidator.cs`

**原因**：Story 要求滿額加價購也支援新欄位（累計循環、門檻限購），現有 `CheckExtraPurchaseRuleList` 僅驗 `ReachPrice > 0` 與 `CanUseECoupon == false`，需補充。

**補充至 `CheckExtraPurchaseRuleList` 現有邏輯之後**：

```csharp
// 新增 ① 累計循環不支援多階
if (entity.ExtraPurchaseRuleList.Any(r => r.IsCyclable) && entity.IsMultiLevel)
{
    return new ValidationFailure(..., PromotionBase.CyclableNotSupportMultiLevel);
}

// 新增 ② ThresholdLimitQty 合法值檢查
if (entity.ExtraPurchaseRuleList.Any(r => r.ThresholdLimitQty != -1 
    && (r.ThresholdLimitQty < 1 || r.ThresholdLimitQty > 999)))
{
    return new ValidationFailure(..., PromotionBase.InvalidThresholdLimitQty);
}
```

---

### 【改動 5】`PromotionService.cs`（確認不需改動）

**路徑**：`BusinessLogic/Services/Promotions/PromotionService.cs`

`GetNine1PromotionCreateBody()` 第 1958 行：

```csharp
ExtraPurchaseRuleList = entity.ExtraPurchaseRuleList
```

整個 List 直接傳入，`PromotionEngineExtraPurchaseRuleEntity` 新增的欄位會自動序列化帶到 Nine1Promotions，**不需修改**。

---

## 四、新增 i18n 錯誤訊息 key

以下 key 需在 Translations 資源檔新增（`PromotionBase` resource class）：

| Key | 建議中文訊息 |
|-----|-------------|
| `ReachPieceMustHaveAndOverZero` | 折扣門檻數量必填且需大於 0 |
| `CyclableNotSupportMultiLevel` | 累計循環僅適用單階活動，多階活動不支援 |
| `InvalidThresholdLimitQty` | 門檻限購數需為 -1（無限購）或 1–999 的整數 |

---

## 五、檔案異動總覽

| # | 檔案 | 動作 |
|---|------|------|
| 1 | `BusinessLogic/BE/Promotions/PromotionEngineExtraPurchaseRuleEntity.cs` | 新增 `IsCyclable`、`ThresholdLimitQty` |
| 2 | `CrossLayer/Validators/Promotions/Rules/CartReachPieceExtraPurchaseValidator.cs` | **新建** |
| 3 | `CrossLayer/Modules/BL/ValidatorModule.cs` | 新增 1 行 Named 註冊 |
| 4 | `CrossLayer/Validators/Promotions/Rules/CartReachPriceExtraPurchaseValidator.cs` | 補充新欄位驗證 |
| 5 | Translations resource（`PromotionBase`） | 新增 3 個 i18n key |
| 6 | `BusinessLogic/Services/Promotions/PromotionService.cs` | ✅ 確認不需改動 |

---

## 六、驗收對照（Story AC）

| AC 項目 | 對應改動 |
|---------|---------|
| API 支援傳入 `CartReachPieceExtraPurchase` TypeDef 並寫入 DB | 改動 2、3（Validator 建立後 DI 正常解析） |
| 傳入不支援 TypeDef 回傳錯誤 | PromotionBaseValidator 既有邏輯已處理 |
| ExtraPurchaseRuleList 必填 | 改動 2 |
| ReachPiece <= 0 回傳錯誤 | 改動 2 |
| ReachPrice <= 0 回傳錯誤（滿額） | 改動 4 |
| 累計循環未傳預設 false | 改動 1（`IsCyclable` default = false） |
| 累計循環 + IsMultiLevel = true → 錯誤 | 改動 2、4 |
| 門檻限購數非法值（非 -1 且非 1–999）→ 錯誤 | 改動 2、4 |
| IsCartExtraPurchaseThresholdEligible 非加價購類型時回傳錯誤 | `CheckIsCartExtraPurchaseThresholdEligible` 既有邏輯已覆蓋 |
