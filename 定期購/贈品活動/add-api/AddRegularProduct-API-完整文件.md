# 新增定期購方案 API（AddProductAsync）完整文件

> 對應端點：`POST /api/regular/products`
> 對應 PHP 舊版邏輯：`ajaxAddRegularProduct`
> 本文件僅涵蓋「新增」流程（Add），不包含「編輯」流程（Edit）。

---

## 目錄

1. [API 總覽](#1-api-總覽)
2. [Request 結構](#2-request-結構)
3. [Response 結構](#3-response-結構)
4. [整體處理流程（Pipeline）](#4-整體處理流程pipeline)
5. [Model Binding 階段（JSON 反序列化）](#5-model-binding-階段json-反序列化)
6. [ValidationActionFilter 與 FluentValidation 驗證層](#6-validationactionfilter-與-fluentvalidation-驗證層)
7. [ProductAddRequestValidator 詳細規則](#7-productaddrequestvalidator-詳細規則)
8. [RegularProductPromotionRuleValidator（贈品活動規則驗證）](#8-regularproductpromotionrulevalidator贈品活動規則驗證)
9. [Service 層：AddProductAsync 完整邏輯](#9-service-層addproductasync-完整邏輯)
10. [ValidateSalePageData / ValidateSalePageDataForAddAsync](#10-validatesalepagedata--validatesalepagedataforaddasync)
11. [ValidateShipLimitInput 與配送次數計算](#11-validateshiplimitinput-與配送次數計算)
12. [SerializePromotionRules（贈品規則序列化）](#12-serializepromotionrules贈品規則序列化)
13. [DB 寫入（InsertAsync / regular_product 資料表）](#13-db-寫入insertasync--regular_product-資料表)
14. [錯誤訊息一覽表](#14-錯誤訊息一覽表)
15. [已知限制與注意事項](#15-已知限制與注意事項)

---

## 1. API 總覽

| 項目 | 內容 |
|---|---|
| Method | `POST` |
| Route | `api/regular/products` |
| Controller | `RegularProductController.AddProductAsync` |
| Service 方法 | `RegularProductService.AddProductAsync(ProductAddRequest request)` |
| 對應資料表 | `regular_product`（欄位前綴 `rpd_*`） |
| 對應 PHP | `ajaxAddRegularProduct` |
| HTTP 狀態碼 | 恆為 `200 OK`（成功/失敗皆同），以 Body 內 `Status` 欄位區分 |

**Controller 原始碼位置**：
`src\Web\Nine1.Regularpurchase.V2.Web.Api\Controllers\RegularProductController.cs`（約行 187~220）

```csharp
[HttpPost]
public async Task<IActionResult> AddProductAsync([FromBody] ProductAddRequest request)
{
    this._logger.LogInformation(
        "[AddProductAsync] 收到請求，ShopId={ShopId}, ProductNo={ProductNo}",
        request.ShopId, request.ProductNo);

    var result = await this._productService.AddProductAsync(request);

    if (result.IsSuccess == false)
    {
        this._logger.LogWarning(
            "[AddProductAsync] 處理失敗，ShopId={ShopId}, ProductNo={ProductNo}, Message={Message}",
            request.ShopId, request.ProductNo, result.Message);
        return Ok(new MessageApiResponseEntity<object>
        {
            Status = "Failure",
            Data = null,
            Message = result.Message,
        });
    }

    this._logger.LogInformation(
        "[AddProductAsync] 處理成功，ShopId={ShopId}, ProductNo={ProductNo}",
        request.ShopId, request.ProductNo);

    return Ok(new MessageApiResponseEntity<string>
    {
        Status = "Success",
        Data = result.Message,
    });
}
```

Controller 本身很薄，不含業務邏輯，只負責：
- 接收 Request（此時已經過 Model Binding + ValidationActionFilter）
- 呼叫 Service
- 依 `RegularProductResult` 的 `IsSuccess` 包裝成統一的 API Response 格式

---

## 2. Request 結構

**檔案**：`src\BusinessLogic\Nine1.Regularpurchase.V2.BL.BE\RegularProduct\ProductAddRequest.cs`

```csharp
public class ProductAddRequest
{
    /// <summary>店家代號</summary>
    public long ShopId { get; set; }

    /// <summary>商品頁序號</summary>
    public long ProductNo { get; set; }

    /// <summary>配送週期清單（JSON 物件陣列，對應 PHP productShippingPeriod）</summary>
    public List<object> ProductShippingPeriod { get; set; } = [];

    /// <summary>配送次數限制選項（1=無限制, 2=有限制, 3=固定次數）</summary>
    public int ProductShippingSet { get; set; }

    /// <summary>最大配送次數（productShippingSet=2 時使用）</summary>
    public int? ProductShippingMax { get; set; }

    /// <summary>固定配送次數（productShippingSet=3 時使用）</summary>
    public int? ProductShippingFixed { get; set; }

    /// <summary>購買選項（1=僅定期購, 2=定期購+一次性）</summary>
    public int ProductPurchaseOption { get; set; }

    /// <summary>方案名稱（對應 rpd_plan_title，可空）</summary>
    public string? PlanTitle { get; set; }

    /// <summary>方案生效開始時間（對應 rpd_plan_date_begin）</summary>
    public DateTime? PlanDateBegin { get; set; }

    /// <summary>方案生效結束時間（對應 rpd_plan_date_end；NULL = 永久有效）</summary>
    public DateTime? PlanDateEnd { get; set; }

    /// <summary>贈品活動規則（對應 rpd_promotion_json）；本次範圍僅會有 0 或 1 筆</summary>
    public List<RegularProductPromotionRule> RegularProductPromotionRules { get; set; } = [];

    /// <summary>操作人員名稱</summary>
    public string OperatorName { get; set; } = string.Empty;
}
```

### 2.1 `RegularProductPromotionRule`（贈品活動規則單筆）

**檔案**：`RegularProductPromotionRule.cs`

```csharp
public class RegularProductPromotionRule
{
    /// <summary>方案內唯一 id；由後端產生，前端 Create 不需傳入</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>活動類型：本次僅開放 Gift</summary>
    public RegularProductPromotionTypeEnum Type { get; set; }

    /// <summary>觸發模式：本次僅開放 Interval</summary>
    public RegularProductPromotionTriggerModeEnum TriggerMode { get; set; }

    /// <summary>Interval 模式：每 N 期觸發一次</summary>
    public int? Interval { get; set; }

    /// <summary>贈品缺貨處理</summary>
    public RegularProductPromotionOutOfStockActionEnum OutOfStockAction { get; set; }

    /// <summary>本次觸發要送出的贈品清單；限制 items.length &lt;= 10、sum(Qty) &lt;= 50</summary>
    public List<RegularProductPromotionGiftItem> RegularProductPromotionGiftItemList { get; set; } = [];
}
```

### 2.2 相關 Enum

| Enum | 值域 | 檔案 |
|---|---|---|
| `RegularProductPromotionTypeEnum` | `Gift = 1`（本次僅開放此值） | `Enums\RegularProductPromotionTypeEnum.cs` |
| `RegularProductPromotionTriggerModeEnum` | `Interval = 1`（本次僅開放此值） | `Enums\RegularProductPromotionTriggerModeEnum.cs` |
| `RegularProductPromotionOutOfStockActionEnum` | `Proceed = 1`（照常建單）、`SkipPeriod = 2`（該期不建立訂單） | `Enums\RegularProductPromotionOutOfStockActionEnum.cs` |

> **重要**：以上皆為 C# `enum`，透過 `System.Text.Json` 反序列化。若 Request 傳入字串（如 `"type": "Gift"`），需搭配 `JsonStringEnumConverter` 才能被正確解析成字串對應值；若傳入不存在的字串（如 `"fefewf"`）或未加轉換器情境下傳入型別不合的值，會在 **Model Binding 階段**直接失敗（詳見第 5 節）。

### 2.3 `RegularProductPromotionGiftItem`（贈品品項）

```csharp
public class RegularProductPromotionGiftItem
{
    /// <summary>贈品序號（即該贈品自己的 SalePageId）</summary>
    public long GiftTypeId { get; set; }

    /// <summary>本次送出數量，範圍 1~10</summary>
    public int Qty { get; set; }
}
```

### 2.4 Request 範例（含贈品活動）

```json
{
  "shopId": 73,
  "productNo": 67226,
  "productShippingPeriod": ["3", "7"],
  "productShippingSet": 2,
  "productShippingMax": 12,
  "productShippingFixed": null,
  "productPurchaseOption": 1,
  "planTitle": "測試方案",
  "planDateBegin": "2026-08-06T00:00:00",
  "planDateEnd": "2026-09-01T23:59:59",
  "regularProductPromotionRules": [
    {
      "type": "Gift",
      "triggerMode": "Interval",
      "interval": 3,
      "outOfStockAction": "Proceed",
      "regularProductPromotionGiftItemList": [
        { "giftTypeId": 987654, "qty": 2 },
        { "giftTypeId": 987655, "qty": 1 }
      ]
    }
  ],
  "operatorName": "AllenLin"
}
```

---

## 3. Response 結構

成功／失敗皆回傳 HTTP `200 OK`，以 `MessageApiResponseEntity<T>` 包裝：

**成功**：
```json
{
  "Status": "Success",
  "Data": "儲存成功"
}
```

**失敗（Service 層驗證/業務規則失敗）**：
```json
{
  "Status": "Failure",
  "Data": null,
  "Message": "此商品序號於指定生效期間已存在重疊方案，請重新輸入"
}
```

**失敗（Model Binding 階段，欄位格式錯誤，如 enum 傳入非法字串）**：
```json
{
  "Status": "Failure",
  "TxId": "c8c6f690237c44b7adabd0eb69985992",
  "Data": null,
  "ErrorMessage": "Invalid Data: Type"
}
```
> 注意此情境的欄位是 `ErrorMessage`（非 `Message`），且多了 `TxId`，因為這是由 `Program.cs` 的 `InvalidModelStateResponseFactory` 產生，走的是 `ApiResponseEntity<T>` 而非 `MessageApiResponseEntity<T>`（詳見第 5 節）。

---

## 4. 整體處理流程（Pipeline）

新增定期購方案的一次完整請求，會依序經過以下 **4 層**檢查/處理，任何一層失敗即短路回傳，不會進入下一層：

```
① HTTP Request (JSON Body)
        │
        ▼
② Model Binding（ASP.NET Core 內建 JSON 反序列化）
   - enum 欄位字串轉型失敗 → 直接短路，回傳 InvalidModelStateResponseFactory 產生的訊息
        │ 通過
        ▼
③ ValidationActionFilter（全域 Action Filter）
   - 反射尋找 ProductAddRequest 對應的 IValidator<ProductAddRequest>
   - 執行 ProductAddRequestValidator（純欄位/跨欄位規則，無外部 I/O）
        │ 通過
        ▼
④ RegularProductController.AddProductAsync
        │
        ▼
⑤ RegularProductService.AddProductAsync（Service 層業務邏輯）
   - 計算 shipLimitCount
   - 呼叫 SCM API 取得商品頁資料（GetSalePageAsync）
   - ValidateSalePageDataForAddAsync（商品頁基本檢查 + 生效期間 vs 銷售期間 + 生效期間重疊）
   - 組裝 Entity → InsertAsync 寫入 DB
   - 呼叫 SCM API 開啟商品頁定期購狀態
        │
        ▼
⑥ 回傳結果
```

**分層設計精神**：
- **② Model Binding**：型別層級的格式驗證（JSON → C# 物件），這一層失敗時，Controller/Service/Validator 完全不會被觸發。
- **③ ValidationActionFilter + FluentValidation**：純欄位/跨欄位驗證，**不含任何外部 I/O**（不查 DB、不呼叫 SCM API）。
- **⑤ Service 層**：只保留「必須依賴外部資料才能判斷」的規則（如商品是否存在於 SCM、生效期間是否與商品銷售期間衝突、是否與其他方案重疊），以及實際的資料組裝與寫入。

---

## 5. Model Binding 階段（JSON 反序列化）

### 5.1 為什麼非法 enum 字串只會回傳籠統訊息？

當 Request 帶入 enum 欄位不存在的值（例如 `"type": "fefewf"` 或 `"giftTypeId": "yyy"`，後者是型別不符，`GiftTypeId` 為 `long`），ASP.NET Core 會在 **`[FromBody]` 反序列化階段**（早於 Controller Action 被呼叫、早於 `ValidationActionFilter`、早於 Service）就判定 `ModelState` 無效，直接觸發 `[ApiController]` 內建的自動 400 邏輯。專案透過 `Program.cs` 自訂 `InvalidModelStateResponseFactory` 覆寫這個自動回應，把 HTTP 狀態碼固定改為 `200`、格式對齊既有 V1 API 慣例，但**訊息內容原本只有籠統的 `"Invalid Data"`**（因為在這個階段，FluentValidation 的 Validator 完全還沒被執行到，Service/Controller 的 Log 也不會出現）。

### 5.2 強化後：帶出實際出錯欄位名稱

**檔案**：`src\Web\Nine1.Regularpurchase.V2.Web.Api\Program.cs`（約行 29~52）

```csharp
//// VERIFY: 確認 Nine1 BaseSDK 或其他中介軟體未覆寫 InvalidModelStateResponseFactory 設定
options.InvalidModelStateResponseFactory = context =>
{
    var txId = Guid.NewGuid().ToString("N").ToLowerInvariant();

    //// 附上 ModelState 裡實際出錯的欄位名稱（如 "Invalid Data: Type"）
    //// 但保留原本的 Status/ErrorMessage 前綴與 HTTP 200，不影響既有前端對錯誤格式的依賴
    var invalidFields = context.ModelState
        .Where(entry => entry.Value?.Errors.Count > 0)
        .Select(entry => entry.Key)
        .ToArray();

    var errorMessage = invalidFields.Length > 0
        ? $"Invalid Data: {string.Join(", ", invalidFields)}"
        : "Invalid Data";

    return new OkObjectResult(new ApiResponseEntity<object>
    {
        Status = "Failure",
        TxId = txId,
        Data = null,
        ErrorMessage = errorMessage
    });
};
```

修正後，帶入 `"type": "fefewf"` 會回傳 `"ErrorMessage": "Invalid Data: Type"`，可讓前端/測試者快速定位是哪個欄位格式錯誤，而不需要逐一排查。

### 5.3 Model Binding 失敗 vs Service 層驗證失敗 差異對照

| 項目 | Model Binding 失敗 | Service/Validator 層失敗 |
|---|---|---|
| 觸發時機 | JSON 反序列化階段（最早） | Action Filter 或 Service 內部 |
| 回應欄位 | `ErrorMessage` + `TxId` | `Message`（無 `TxId`） |
| 訊息內容 | `"Invalid Data: {欄位名}"` | FluentValidation/Service 的具體業務訊息 |
| 是否會呼叫 Validator | 否 | 是 |
| 常見觸發情境 | enum 欄位帶入不存在字串、型別不符（如字串塞進 `long`） | 欄位必填未填、數值範圍錯誤、業務規則衝突（如序號重複） |

---

## 6. ValidationActionFilter 與 FluentValidation 驗證層

### 6.1 設計動機

原本每個 Service（如 `RegularProductService`）都要各自注入自己的 `IValidator<TRequest>` 並在方法開頭手動呼叫 `ValidateAsync`。隨著 Request 種類增加，Service 建構子會被迫塞入越來越多驗證器依賴，且「驗證」與「業務邏輯」混在同一層，職責不夠單一。

改用全域 Action Filter 後：
1. 驗證發生在 Controller/Service 之前，屬於 MVC pipeline 的橫切關注點（cross-cutting concern）。
2. 新增一個 Request 的驗證，只需要註冊對應的 `AbstractValidator<TRequest>`（透過 `AddValidatorsFromAssembly` 自動掃描），不需要修改 Filter 本身，也不需要修改 Service 建構子。
3. Service 不再需要注入 `IValidator<TRequest>`，建構子回歸只放「真正的業務依賴」。

### 6.2 運作機制

**檔案**：`src\Web\Nine1.Regularpurchase.V2.Web.Api\Filters\ValidationActionFilter.cs`

```csharp
public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
{
    foreach (var argument in context.ActionArguments.Values)
    {
        if (argument == null)
            continue;

        var validatorType = typeof(IValidator<>).MakeGenericType(argument.GetType());
        var validator = context.HttpContext.RequestServices.GetService(validatorType) as IValidator;

        //// 該參數型別沒有對應的 Validator，代表這個 Request 不需要 FluentValidation 驗證（維持現況放行）
        if (validator == null)
            continue;

        var validationContext = new ValidationContext<object>(argument);
        var validationResult = await validator.ValidateAsync(validationContext);

        if (validationResult.IsValid)
        {
            continue;
        }

        context.Result = new OkObjectResult(new MessageApiResponseEntity<object>
        {
            Status = "Failure",
            Data = null,
            Message = validationResult.Errors[0].ErrorMessage,
        });
        return;
    }

    await next();
}
```

重點：
- 用反射動態找出 Action 參數型別對應的 `IValidator<T>`，找不到就放行（代表該型別不需要驗證）。
- 驗證失敗時，直接短路設定 `context.Result`，**Action（Controller/Service）完全不會被呼叫**。
- 只回傳**第一個**錯誤訊息（`validationResult.Errors[0]`），不是全部錯誤清單。

---

## 7. ProductAddRequestValidator 詳細規則

**檔案**：`src\Common\Nine1.Regularpurchase.V2.Common.Validators\RegularProduct\ProductAddRequestValidator.cs`

這是 `ProductAddRequest` 對應的 FluentValidation Validator，涵蓋所有**純欄位/跨欄位、不需外部 I/O**的規則：

| 欄位 | 規則 | 錯誤訊息 |
|---|---|---|
| `ShopId` | 必須 > 0 | `shopId 無效` |
| `ProductNo` | 必須 > 0 | `商品序號無效，請重新輸入` |
| `ProductShippingPeriod` | 不可為空 | `請填寫週期設定` |
| `ProductShippingSet` | 必須為 `1`、`2` 或 `3` | `請填寫正確的配送次數選項（1=無限制, 2=有限制, 3=固定次數）` |
| `ProductShippingMax` | 當 `ProductShippingSet == 2` 時：必填，範圍 `2~99` | `請填寫正確的配送次數限制數量` / `配送次數限制數量請輸入2~99之數字` |
| `ProductShippingFixed` | 當 `ProductShippingSet == 3` 時：必填，範圍 `2~99` | `請填寫正確的配送次數固定數量` / `配送次數固定數量請輸入2~99之數字` |
| `ProductPurchaseOption` | 必須為 `1` 或 `2` | `請填寫正確的購買選項（1=僅定期購, 2=定期購+一次性）` |
| `PlanTitle` | 未填不驗證；有填時：全半形混合上限 20 字，純半形上限 40 字 | `方案名稱字數超出上限（全半形混合20字，純半形40字）` |
| `PlanDateBegin` | 未填不驗證；有填時：須晚於目前時間 10 分鐘以上 | `生效開始時間須晚於現在時間 10 分鐘以上` |
| `PlanDateEnd` | 僅當 `PlanDateBegin` 與 `PlanDateEnd` 皆有填才驗證：須晚於 `PlanDateBegin` | `生效結束時間須晚於生效開始時間` |
| `RegularProductPromotionRules` | 陣列長度 `<= 1`（本次範圍僅支援單一規則） | `本次僅支援單一贈品活動規則` |
| `RegularProductPromotionRules[i]` | 逐筆呼叫 `RegularProductPromotionRuleValidator`（見第 8 節） | （見第 8 節） |

### 7.1 PlanTitle 全半形字數判斷邏輯

```csharp
//// 判斷 PlanTitle 字數是否符合上限：只要包含任一全形字元（含中文、全形標點、全形符號等），
//// 上限為 20 字；若整串皆為半形字元（ASCII 0x00~0xFF 範圍），上限為 40 字。
private static bool IsPlanTitleLengthValid(string title)
{
    var maxLength = title.Any(IsFullWidthChar)
        ? PlanTitleMaxLengthMixedWidth   // 20
        : PlanTitleMaxLengthHalfWidthOnly; // 40

    return title.Length <= maxLength;
}

//// 半形字元範圍：Unicode code point <= 0xFF（ASCII + Latin-1），超出此範圍視為全形字元
private static bool IsFullWidthChar(char c) => c > 0xFF;
```
> WHY：採用「字元的 UTF-16 code point 是否超出半形 ASCII/Latin-1 範圍」判斷全半形，而非計算顯示寬度（wcwidth），因為需求描述的是「文字類型」而非「畫面佔用寬度」，且此判斷法不需要額外套件、對中文/日文/全形符號皆能正確歸類為全形。

### 7.2 ComputeShipLimitCount（Validator 內部專用）

```csharp
//// 依 ProductShippingSet 計算配送次數上限；1=無限制(0)、2=有限制取 Max、3=固定次數取 Fixed
private static int ComputeShipLimitCount(ProductAddRequest request)
{
    return request.ProductShippingSet switch
    {
        2 => request.ProductShippingMax ?? 0,
        3 => request.ProductShippingFixed ?? 0,
        _ => 0,
    };
}
```
此方法**只服務於 Validator 內部**，用來把 `shipLimitCount` 算出來、傳給 `RegularProductPromotionRuleValidator` 做 `Interval` 範圍檢查（見第 8 節）。它與 Service 層 `AddProductAsync` 開頭那段等價的 `switch` 運算式是**各自獨立、重複但一致的兩份計算**（詳見第 11 節說明為何不共用）。

### 7.3 PreValidate（記錄追蹤用）

```csharp
protected override bool PreValidate(
    FluentValidation.ValidationContext<ProductAddRequest> context,
    FluentValidation.Results.ValidationResult result)
{
    this._logger.LogInformation(
        "開始驗證 ProductAddRequest，ShopId={ShopId}, ProductNo={ProductNo}, PromotionRuleCount={PromotionRuleCount}",
        context.InstanceToValidate.ShopId,
        context.InstanceToValidate.ProductNo,
        context.InstanceToValidate.RegularProductPromotionRules.Count);

    return base.PreValidate(context, result);
}
```
`PreValidate` 是 FluentValidation 提供、在所有 `RuleFor` 規則執行「之前」保證會被呼叫一次的擴充點，用來記錄「這次驗證進來的是誰、關鍵欄位是什麼」，方便對照 log 追蹤某一次呼叫是否真的有跑到這支 Validator。規則本身的通過/失敗結果，統一交給 Service 層在拿到結果後記錄，避免同一份資訊在 Validator 與 Service 兩處重複記錄。

---

## 8. RegularProductPromotionRuleValidator（贈品活動規則驗證）

**檔案**：`RegularProductPromotionRuleValidator.cs`

由 `ProductAddRequestValidator` 動態建立（因為需要 `shipLimitCount`／`shippingSet` 這兩個由 Request 其他欄位計算出的上下文）：

```csharp
this.RuleForEach(x => x.RegularProductPromotionRules)
    .SetValidator((request, _) => new RegularProductPromotionRuleValidator(
        ComputeShipLimitCount(request), request.ProductShippingSet));
```

| 欄位 | 規則 | 錯誤訊息 |
|---|---|---|
| `Type` | 必須等於 `Gift` | `不支援的活動類型` |
| `TriggerMode` | 必須等於 `Interval` | `不支援的觸發模式` |
| `Interval` | 必填；`>= 2`；當 `shippingSet != 1`（有配送次數上限）時，`Interval <= shipLimitCount` | `配送次數設定超出範圍` |
| `OutOfStockAction` | 必須為 `Proceed` 或 `SkipPeriod` | `不支援的缺貨處理方式` |
| `RegularProductPromotionGiftItemList` | 不可為空 | `贈品清單不可為空` |
| `RegularProductPromotionGiftItemList` | 品項數量 `<= 10` | `贈品品項數量超出上限` |
| `RegularProductPromotionGiftItemList` | 總數量（`sum(Qty)`）`<= 50` | `贈品總數量超出上限` |
| `RegularProductPromotionGiftItemList[i]` | 逐筆呼叫 `RegularProductPromotionGiftItemValidator` | （見下方） |

### 8.1 RegularProductPromotionGiftItemValidator

```csharp
this.RuleFor(x => x.GiftTypeId)
    .GreaterThan(0)
    .WithMessage("贈品序號無效，請重新輸入");

this.RuleFor(x => x.Qty)
    .InclusiveBetween(1, 10)
    .WithMessage("贈品數量需介於 1~10 之間");
```

---

## 9. Service 層：AddProductAsync 完整邏輯

**檔案**：`src\BusinessLogic\Nine1.Regularpurchase.V2.BL.Services\RegularProduct\RegularProductService.cs`

```csharp
public async Task<RegularProductResult<object?>> AddProductAsync(ProductAddRequest request)
{
    this._logger.LogInformation(
        "AddProductAsync 開始，ShopId={ShopId}, ProductNo={ProductNo}",
        request.ShopId, request.ProductNo);

    //// ① 計算配送次數限制（驗證已搬到 ProductAddRequestValidator，這裡只負責計算數值）
    var shipLimitCount = request.ProductShippingSet switch
    {
        2 => request.ProductShippingMax ?? 0,
        3 => request.ProductShippingFixed ?? 0,
        _ => 0,
    };

    if (request.ProductShippingPeriod.Count < 1)
        return RegularProductResult<object?>.Failure("error_period");

    //// ② 取得並驗證 SCM 商品資訊（含商品頁基本檢查、生效期間 vs 銷售期間、期間重疊）
    var apiProfile = await this._apiInformationService.GetApiInformationAsync(request.ShopId);
    var salePageData = await this.GetSalePageAsync(apiProfile, request.ProductNo);

    var salePageValidation = await this.ValidateSalePageDataForAddAsync(salePageData, request);
    if (salePageValidation.IsSuccess == false)
        return RegularProductResult<object?>.Failure(salePageValidation.Message);

    //// 通過 ValidateSalePageDataForAddAsync 後 salePageData 保證非 null
    var validatedSalePageData = salePageData!;

    //// ③ 組裝新增實體
    var now = DateTimeHelper.GetNow(this._timeProvider);
    var entity = new RegularProductEntity
    {
        RpdUuid = Guid.NewGuid().ToString("N"),
        RpdPageId = request.ProductNo,
        RpdShopId = request.ShopId,
        RpdCategoryId = validatedSalePageData.CategoryId,
        RpdSku = validatedSalePageData.CategoryId, // V1 PHP：與 rpd_category_id 相同值
        RpdShopCategoryId = validatedSalePageData.ShopCategoryId,
        RpdTitle = validatedSalePageData.SalePageTitle ?? string.Empty,
        RpdPicUrl = validatedSalePageData.ImageList?.FirstOrDefault()?.PicUrl ?? string.Empty,
        RpdOriginalPrice = validatedSalePageData.SuggestPrice,
        RpdSalePrice = validatedSalePageData.Price,
        RpdCostPrice = validatedSalePageData.Cost,
        RpdSaleQty = 0,
        RpdLimitQty = 0,
        RpdShipPeriod = JsonSerializer.Serialize(request.ProductShippingPeriod),
        RpdShipLimitStatus = request.ProductShippingSet,
        RpdShipLimitCount = shipLimitCount,
        RpdBuySetting = request.ProductPurchaseOption,
        RpdSaleDateBegin = validatedSalePageData.SellingStartDateTime,
        RpdSaleDateEnd = validatedSalePageData.SellingEndDateTime,
        RpdPlanTitle = request.PlanTitle,
        RpdPlanDateBegin = request.PlanDateBegin,
        RpdPlanDateEnd = request.PlanDateEnd,
        RpdPromotionJson = SerializePromotionRules(request.RegularProductPromotionRules),
        RpdStatus = 1,
        RpdCreateTime = now,
        RpdCreateUser = request.OperatorName,
    };

    //// ④ 寫入 DB
    var success = await this._productRepository.InsertAsync(entity);
    if (success == false)
    {
        this._logger.LogError(
            "AddProductAsync 儲存失敗，ShopId={ShopId}, ProductNo={ProductNo}",
            request.ShopId, request.ProductNo);
        return RegularProductResult<object?>.Failure("儲存失敗");
    }

    //// ⑤ 對應 PHP：成功插入後開啟商品頁定期購狀態（isClosed = false）
    await this.UpdateSalePageIsClosedOfRegularOrderAsync(apiProfile, request.ProductNo, isClosed: false);

    this._logger.LogInformation(
        "AddProductAsync 新增成功，ShopId={ShopId}, ProductNo={ProductNo}, RpdUuid={RpdUuid}",
        request.ShopId, request.ProductNo, entity.RpdUuid);

    return RegularProductResult<object?>.Success("儲存成功");
}
```

### 9.1 流程圖

```
AddProductAsync(request)
  │
  ├─ 1. 計算 shipLimitCount（switch，不再重複驗證）
  ├─ 2. ProductShippingPeriod.Count < 1 → "error_period"
  ├─ 3. GetApiInformationAsync（取得 SCM API 憑證）
  ├─ 4. GetSalePageAsync（呼叫 SCM 取得商品頁即時資料）
  ├─ 5. ValidateSalePageDataForAddAsync
  │      ├─ 5.1 salePageData == null？ → "商品序號無效，請重新輸入"
  │      ├─ 5.2 ShippingTypeDef != 7（定期購交期）？ → "此商品的「交期」需為定期購..."
  │      ├─ 5.3 HasPointsPayPairs（點數折抵組合）？ → "定期購設定僅支援一般商品，請重新輸入"
  │      ├─ 5.4 PlanDateBegin < SellingStartDateTime？ → "生效開始時間不得早於商品銷售開始時間"
  │      ├─ 5.5 PlanDateEnd > SellingEndDateTime？ → "生效結束時間不得晚於商品銷售結束時間"
  │      └─ 5.6 ExistsOverlappingPlanAsync（同商品頁生效期間重疊）？ → "此商品序號於指定生效期間已存在重疊方案，請重新輸入"
  ├─ 6. 組裝 RegularProductEntity
  ├─ 7. InsertAsync 寫入 regular_product 資料表
  │      └─ 失敗 → "儲存失敗"
  ├─ 8. UpdateSalePageIsClosedOfRegularOrderAsync（開啟商品頁定期購狀態）
  └─ 9. 回傳 Success("儲存成功")
```

---

## 10. ValidateSalePageData / ValidateSalePageDataForAddAsync

### 10.1 設計分工

| 方法 | 性質 | 使用者 | 內容 |
|---|---|---|---|
| `ValidateSalePageData` | `static`，同步，無 I/O | 只被 `ValidateSalePageDataForAddAsync` 呼叫 | 商品頁基本合法性：null / 交期類型 / 點數折抵 |
| `ValidateSalePageDataForAddAsync` | 非 `static`，async，含 DB 查詢 | 只被 `AddProductAsync` 呼叫 | 呼叫 `ValidateSalePageData` 後，再接續「生效期間 vs 銷售期間」「生效期間重疊」兩項新增流程專屬檢查 |

> **重要備註**：`EditProductAsync` **不使用**上述任一共用方法，維持獨立的三段判斷式（見 `RegularProductService.cs` 內 `EditProductAsync`），因為編輯流程有其獨立需求（不檢查生效期間重疊、無 `PlanDateBegin`/`PlanDateEnd` 概念綁定新增流程規則），且已於開發過程中明確要求「不動 Edit」。

### 10.2 原始碼

```csharp
//// 驗證商品頁是否合法可設定定期購（商品存在、交期為定期購、不含點數折抵組合），
//// 由 AddProductAsync 共用，取得 salePageData 後立即呼叫
private static RegularProductResult<object?> ValidateSalePageData(
    NineYi.Scm.Api.Models.SalePage.SalePageDataEntity? salePageData)
{
    if (salePageData == null)
        return RegularProductResult<object?>.Failure("商品序號無效，請重新輸入");

    if (salePageData.ShippingTypeDef != RegularShippingTypeDef)
        return RegularProductResult<object?>.Failure("此商品的「交期」需為定期購，才可進行設定，請先至商品頁重新確認");

    if (salePageData.HasPointsPayPairs)
        return RegularProductResult<object?>.Failure("定期購設定僅支援一般商品，請重新輸入");

    return RegularProductResult<object?>.Success();
}

//// AddProductAsync 專用：在 ValidateSalePageData 共用檢查之外，再加上新增流程特有的
//// 「生效期間 vs 商品銷售期間」「生效期間重疊」檢查（皆須查詢外部資料，
//// 無法放進 ProductAddRequestValidator，也不適合讓 EditProductAsync 共用，因為
//// ProductEditRequest 沒有 PlanDateBegin/PlanDateEnd，且編輯流程本來就允許沿用既有設定）
private async Task<RegularProductResult<object?>> ValidateSalePageDataForAddAsync(
    NineYi.Scm.Api.Models.SalePage.SalePageDataEntity? salePageData,
    ProductAddRequest request)
{
    var basicValidation = ValidateSalePageData(salePageData);
    if (basicValidation.IsSuccess == false)
    {
        this._logger.LogWarning(
            "AddProductAsync 商品頁驗證失敗，ShopId={ShopId}, ProductNo={ProductNo}, Message={Message}",
            request.ShopId, request.ProductNo, basicValidation.Message);
        return basicValidation;
    }

    var validatedSalePageData = salePageData!;

    //// 生效期間不得超出商品銷售期間：須取得 SCM 回傳的商品銷售起訖時間才能比較，
    //// 因此無法放在 ProductAddRequestValidator（純欄位/跨欄位驗證，不含外部 I/O）。
    if (request.PlanDateBegin.HasValue && request.PlanDateBegin.Value < validatedSalePageData.SellingStartDateTime)
    {
        this._logger.LogWarning(
            "AddProductAsync 生效開始時間早於商品銷售開始時間，ShopId={ShopId}, ProductNo={ProductNo}, PlanDateBegin={PlanDateBegin}, SellingStartDateTime={SellingStartDateTime}",
            request.ShopId, request.ProductNo, request.PlanDateBegin, validatedSalePageData.SellingStartDateTime);
        return RegularProductResult<object?>.Failure("生效開始時間不得早於商品銷售開始時間");
    }

    if (request.PlanDateEnd.HasValue && request.PlanDateEnd.Value > validatedSalePageData.SellingEndDateTime)
    {
        this._logger.LogWarning(
            "AddProductAsync 生效結束時間晚於商品銷售結束時間，ShopId={ShopId}, ProductNo={ProductNo}, PlanDateEnd={PlanDateEnd}, SellingEndDateTime={SellingEndDateTime}",
            request.ShopId, request.ProductNo, request.PlanDateEnd, validatedSalePageData.SellingEndDateTime);
        return RegularProductResult<object?>.Failure("生效結束時間不得晚於商品銷售結束時間");
    }

    //// 商品頁在不同定期購方案的生效期間 overlap 驗證
    if (request.PlanDateBegin.HasValue)
    {
        var hasOverlap = await this._productRepository.ExistsOverlappingPlanAsync(
            request.ShopId, request.ProductNo, request.PlanDateBegin, request.PlanDateEnd);
        if (hasOverlap)
        {
            this._logger.LogWarning(
                "AddProductAsync 生效期間重疊，ShopId={ShopId}, ProductNo={ProductNo}, PlanDateBegin={PlanDateBegin}, PlanDateEnd={PlanDateEnd}",
                request.ShopId, request.ProductNo, request.PlanDateBegin, request.PlanDateEnd);
            return RegularProductResult<object?>.Failure("此商品序號於指定生效期間已存在重疊方案，請重新輸入");
        }
    }

    return RegularProductResult<object?>.Success();
}
```

### 10.3 為何移除了「商品序號已存在」的檢查（ExistsByProductAsync）

**歷史背景**：新增流程原本在檢查完商品頁基本合法性後，還會呼叫 `ExistsByProductAsync(shopId, productNo)`——只要同 `ShopId`+`ProductNo` 底下存在**任一筆**（`rpd_status=1`）記錄就直接擋下，回傳「此商品序號已被其他定期購設定，請重新輸入」。

**問題**：這個 SQL 完全沒有比對 `rpd_plan_date_begin`/`rpd_plan_date_end`：

```sql
SELECT COUNT(1) FROM regular_product
WHERE rpd_shop_id = @ShopId AND rpd_page_id = @PageId AND rpd_status = 1
LIMIT 1
```

導致「同一商品頁、生效期間完全錯開的兩筆合法方案」也會被誤擋，且讓下面真正該做時間重疊判斷的 `ExistsOverlappingPlanAsync` 變成永遠不會被執行到的死代碼（因為只要有一筆記錄存在，就已經在 `ExistsByProductAsync` 這關被擋下，根本走不到重疊判斷）。

**修正**：**移除 `ExistsByProductAsync` 呼叫**，只保留 `ExistsOverlappingPlanAsync` 的時間重疊判斷。同一商品頁現在可以有多筆生效期間不重疊的定期購方案並存，只有生效期間**真的重疊**才會被擋下。

> **注意**：這個修正**只套用在 `AddProductAsync`**（`ValidateSalePageDataForAddAsync`）。`ValidateProductAsync`（另一支獨立的「驗證商品頁是否可設定」API）仍保留原本的 `ExistsByProductAsync` 檢查，未受影響。

### 10.4 ExistsOverlappingPlanAsync SQL（時間重疊判斷）

**檔案**：`RegularProductRepository.cs`

```csharp
//// 重疊判斷採「開區間比較 + NULL 視為無邊界」，NULL end 視為 +∞、NULL begin 視為 -∞
public async Task<bool> ExistsOverlappingPlanAsync(long shopId, long pageId, DateTime? planDateBegin, DateTime? planDateEnd)
{
    const string sql = @"
        SELECT COUNT(1)
        FROM regular_product
        WHERE rpd_shop_id = @ShopId
          AND rpd_page_id = @PageId
          AND rpd_status  = 1
          AND (rpd_plan_date_begin IS NULL OR rpd_plan_date_begin < @PlanDateEnd)
          AND (rpd_plan_date_end   IS NULL OR rpd_plan_date_end   > @PlanDateBegin)
        LIMIT 1";
    ...
}
```
邏輯：兩個區間 `[begin1, end1)` 與 `[begin2, end2)` 重疊的條件是 `begin1 < end2 AND end1 > begin2`；`NULL` 視為無邊界（begin=NULL 視為 `-∞`，end=NULL 視為 `+∞`）。

---

## 11. ValidateShipLimitInput 與配送次數計算

### 11.1 背景：AddProductAsync 不再呼叫 ValidateShipLimitInput

```csharp
//// WHY: ValidateShipLimitInput 原本同時做「驗證」與「計算 shipLimitCount」，驗證的部分
//// （ProductShippingMax/Fixed 必填、範圍 2~99）已完整搬進 ProductAddRequestValidator，
//// 這裡只保留計算 shipLimitCount 的職責，不再重複驗證，避免同一條規則兩處維護、
//// 錯誤訊息不一致的風險；ProductShippingSet 的合法性（1/2/3）也已由 Validator 保證。
var shipLimitCount = request.ProductShippingSet switch
{
    2 => request.ProductShippingMax ?? 0,
    3 => request.ProductShippingFixed ?? 0,
    _ => 0,
};
```

**過程回顧**：
- `ValidateShipLimitInput` 這個 private method 同時肩負「驗證」（`shippingMax`/`shippingFixed` 是否為 null、是否在 2~99 範圍）與「計算」（算出實際要存進 `RpdShipLimitCount` 的值）兩種職責。
- 全域驗證改用 `ValidationActionFilter` + FluentValidation 後，`ProductAddRequestValidator` 已把驗證規則完整搬過去（見第 7 節）。
- 因此 `AddProductAsync` 這裡**只保留計算職責**，改用純 `switch` 運算式，不再呼叫 `ValidateShipLimitInput`（否則等於同一條規則被驗證兩次：一次在 Filter/Validator，一次在 Service）。
- `EditProductAsync` 目前仍呼叫 `ValidateShipLimitInput`（因為 `ProductEditRequest` 尚未比照做同等的 FluentValidation 遷移，維持原狀不動）。

### 11.2 ValidateShipLimitInput 原始碼（EditProductAsync 仍在使用）

```csharp
//// 驗證配送次數限制參數，回傳錯誤訊息（null = 通過），out 計算後的 shipLimitCount
private static string? ValidateShipLimitInput(
    int shippingSet,
    int? shippingMax,
    int? shippingFixed,
    out int shipLimitCount)
{
    shipLimitCount = 0;

    switch (shippingSet)
    {
        case 1: //// 無限制：次數固定為 0
            shipLimitCount = 0;
            break;

        case 2: //// 有限制：取最大次數
            if (shippingMax == null) return "error_shipping_count";
            if (shippingMax < ShipLimitCountMin || shippingMax > ShipLimitCountMax)
                return $"請輸入{ShipLimitCountMin}~{ShipLimitCountMax}之數字";
            shipLimitCount = shippingMax.Value;
            break;

        case 3: //// 固定次數
            if (shippingFixed == null) return "error_shipping_count";
            if (shippingFixed < ShipLimitCountMin || shippingFixed > ShipLimitCountMax)
                return $"請輸入{ShipLimitCountMin}~{ShipLimitCountMax}之數字";
            shipLimitCount = shippingFixed.Value;
            break;

        default:
            return "error_shipping_count";
    }
    ...
}
```

### 11.3 計算邏輯對照表

| `ProductShippingSet` | 意義 | `shipLimitCount` 計算來源 |
|---|---|---|
| `1` | 無限制 | 固定 `0` |
| `2` | 有限制 | `ProductShippingMax`（已由 Validator 保證非 null 且在 2~99） |
| `3` | 固定次數 | `ProductShippingFixed`（已由 Validator 保證非 null 且在 2~99） |
| 其他 | 不合法（已由 Validator 擋下，理論上不會到達 Service） | `0` |

---

## 12. SerializePromotionRules（贈品規則序列化）

### 12.1 用途

把 `List<RegularProductPromotionRule>` 序列化成要存進 `rpd_promotion_json`（`jsonb` 欄位）的字串。

### 12.2 原始碼

```csharp
private static string SerializePromotionRules(List<RegularProductPromotionRule> rules)
{
    if (rules.Count == 0)
        return "[]";

    foreach (var rule in rules)
    {
        if (string.IsNullOrEmpty(rule.Id))
            rule.Id = Guid.NewGuid().ToString("N");
    }

    return JsonSerializer.Serialize(rules, new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        Converters = { new JsonStringEnumConverter() },
    });
}
```

要點：
- 若規則的 `Id` 為空（前端 Create 情境不需傳入），由後端補 `Guid.NewGuid().ToString("N")`。
- **`Converters = { new JsonStringEnumConverter() }`**：讓 `Type`/`TriggerMode`/`OutOfStockAction` 這三個 enum 欄位序列化成**文字**（如 `"Gift"`、`"Interval"`、`"Proceed"`）而非數字（如 `1`），存進 DB 的 JSON 才會是人類可讀的格式，與 API Response 給前端的格式一致。

### 12.3 Bug 修正紀錄：曾經序列化成數字的根因

修正前，這個方法自己 new 了一份獨立的 `JsonSerializerOptions`，只設了 `PropertyNamingPolicy = CamelCase`，**沒有加 `JsonStringEnumConverter`**，跟 `Program.cs` 給 Controller Response 用的全域 JSON 設定（有加 `JsonStringEnumConverter`）脫鉤，導致：
- API 回應給前端時，enum 顯示為文字（如 `"type": "Gift"`）
- 但存進 `rpd_promotion_json` 時卻是數字（如 `"type": 1`）

同一組資料在兩個地方呈現不一致格式。修正後兩處統一都是文字。

> **注意**：`GiftTypeId`（`long` 型別，純數字商品序號）不是 enum，不受此修正影響，存 DB 與回應前端都維持數字，這是正確的。

### 12.4 序列化前後對照

**修正前**（DB 內容）：
```json
[{"id": "489c49333ba94790a092c369b26eec98", "type": 1, "interval": 3, "triggerMode": 1, "outOfStockAction": 1, "regularProductPromotionGiftItemList": [{"qty": 2, "giftTypeId": 987654}]}]
```

**修正後**（DB 內容）：
```json
[{"id": "489c49333ba94790a092c369b26eec98", "type": "Gift", "interval": 3, "triggerMode": "Interval", "outOfStockAction": "Proceed", "regularProductPromotionGiftItemList": [{"qty": 2, "giftTypeId": 987654}]}]
```

> 既有髒資料（修正前已寫入的數字格式紀錄）**尚未進行 SQL migration 清理**，僅新增/未來的寫入會是文字格式；是否需要回溯清理待後續評估。

---

## 13. DB 寫入（InsertAsync / regular_product 資料表）

### 13.1 InsertAsync SQL

**檔案**：`RegularProductRepository.cs`

```sql
INSERT INTO regular_product (
    rpd_uuid, rpd_page_id, rpd_shop_id,
    rpd_category_id, rpd_sku, rpd_shop_category_id,
    rpd_title, rpd_plan_title, rpd_pic_url,
    rpd_original_price, rpd_sale_price, rpd_cost_price,
    rpd_sale_qty, rpd_limit_qty,
    rpd_ship_period, rpd_ship_limit_status, rpd_ship_limit_count,
    rpd_buy_setting,
    rpd_sale_date_begin, rpd_sale_date_end,
    rpd_plan_date_begin, rpd_plan_date_end, rpd_promotion_json,
    rpd_status, rpd_create_time, rpd_create_user
) VALUES (
    @RpdUuid, @RpdPageId, @RpdShopId,
    @RpdCategoryId, @RpdSku, @RpdShopCategoryId,
    @RpdTitle, @RpdPlanTitle, @RpdPicUrl,
    @RpdOriginalPrice, @RpdSalePrice, @RpdCostPrice,
    @RpdSaleQty, @RpdLimitQty,
    @RpdShipPeriod, @RpdShipLimitStatus, @RpdShipLimitCount,
    @RpdBuySetting,
    @RpdSaleDateBegin, @RpdSaleDateEnd,
    @RpdPlanDateBegin, @RpdPlanDateEnd, @RpdPromotionJson::jsonb,
    @RpdStatus, @RpdCreateTime, @RpdCreateUser
)
```

> 注意 `@RpdPromotionJson::jsonb`：PostgreSQL 語法，把字串參數 cast 成 `jsonb` 型別存入。

### 13.2 欄位對照表（Request → Entity → DB Column）

| Request 欄位 | Entity 屬性 | DB Column | 備註 |
|---|---|---|---|
| `ShopId` | `RpdShopId` | `rpd_shop_id` | |
| `ProductNo` | `RpdPageId` | `rpd_page_id` | |
| — | `RpdCategoryId` | `rpd_category_id` | 來自 SCM `salePageData.CategoryId` |
| — | `RpdSku` | `rpd_sku` | 與 `RpdCategoryId` 相同值（沿用 V1 PHP 行為） |
| — | `RpdShopCategoryId` | `rpd_shop_category_id` | 來自 SCM |
| — | `RpdTitle` | `rpd_title` | 來自 SCM 商品名稱 |
| — | `RpdPicUrl` | `rpd_pic_url` | 來自 SCM 商品圖 |
| — | `RpdOriginalPrice`/`RpdSalePrice`/`RpdCostPrice` | `rpd_original_price`/`rpd_sale_price`/`rpd_cost_price` | 來自 SCM 價格資訊 |
| — | `RpdSaleQty`/`RpdLimitQty` | `rpd_sale_qty`/`rpd_limit_qty` | 新增時固定為 `0` |
| `ProductShippingPeriod` | `RpdShipPeriod` | `rpd_ship_period` | `JsonSerializer.Serialize` 原樣序列化（`List<object>`，無格式限制） |
| `ProductShippingSet` | `RpdShipLimitStatus` | `rpd_ship_limit_status` | |
| （計算值）`shipLimitCount` | `RpdShipLimitCount` | `rpd_ship_limit_count` | 見第 11 節 |
| `ProductPurchaseOption` | `RpdBuySetting` | `rpd_buy_setting` | |
| — | `RpdSaleDateBegin`/`RpdSaleDateEnd` | `rpd_sale_date_begin`/`rpd_sale_date_end` | 來自 SCM 商品銷售起訖時間 |
| `PlanTitle` | `RpdPlanTitle` | `rpd_plan_title` | |
| `PlanDateBegin`/`PlanDateEnd` | `RpdPlanDateBegin`/`RpdPlanDateEnd` | `rpd_plan_date_begin`/`rpd_plan_date_end` | |
| `RegularProductPromotionRules` | `RpdPromotionJson` | `rpd_promotion_json` | 見第 12 節 `SerializePromotionRules` |
| — | `RpdStatus` | `rpd_status` | 新增時固定為 `1` |
| — | `RpdCreateTime` | `rpd_create_time` | 由 `DateTimeHelper.GetNow(timeProvider)` 產生（非主機系統時間） |
| `OperatorName` | `RpdCreateUser` | `rpd_create_user` | |

### 13.3 寫入成功後的後續動作

```csharp
//// 對應 PHP：成功插入後呼叫 V2_SalePage_UpdateSalePageIsClosedOfRegularOrder($shop_id, $page_id, false) 開啟商品頁
await this.UpdateSalePageIsClosedOfRegularOrderAsync(apiProfile, request.ProductNo, isClosed: false);
```
呼叫 SCM API 把該商品頁的「定期購關閉狀態」設為 `false`（開啟），對應 V1 PHP 行為。此呼叫失敗僅記錄 Log Warning，不影響主流程（與 V1 行為一致，避免 SCM 暫時性錯誤導致整個新增流程失敗）。

---

## 14. 錯誤訊息一覽表

| 層級 | 觸發條件 | 錯誤訊息 |
|---|---|---|
| Model Binding | enum/型別欄位反序列化失敗 | `Invalid Data: {欄位名, ...}` |
| ProductAddRequestValidator | `ShopId <= 0` | `shopId 無效` |
| ProductAddRequestValidator | `ProductNo <= 0` | `商品序號無效，請重新輸入` |
| ProductAddRequestValidator | `ProductShippingPeriod` 為空 | `請填寫週期設定` |
| ProductAddRequestValidator | `ProductShippingSet` 不在 1/2/3 | `請填寫正確的配送次數選項（1=無限制, 2=有限制, 3=固定次數）` |
| ProductAddRequestValidator | `ProductShippingMax` 未填/超出範圍（Set=2） | `請填寫正確的配送次數限制數量` / `配送次數限制數量請輸入2~99之數字` |
| ProductAddRequestValidator | `ProductShippingFixed` 未填/超出範圍（Set=3） | `請填寫正確的配送次數固定數量` / `配送次數固定數量請輸入2~99之數字` |
| ProductAddRequestValidator | `ProductPurchaseOption` 不在 1/2 | `請填寫正確的購買選項（1=僅定期購, 2=定期購+一次性）` |
| ProductAddRequestValidator | `PlanTitle` 字數超出上限 | `方案名稱字數超出上限（全半形混合20字，純半形40字）` |
| ProductAddRequestValidator | `PlanDateBegin` 未晚於現在+10分 | `生效開始時間須晚於現在時間 10 分鐘以上` |
| ProductAddRequestValidator | `PlanDateEnd` 未晚於 `PlanDateBegin` | `生效結束時間須晚於生效開始時間` |
| ProductAddRequestValidator | 贈品規則數量 > 1 | `本次僅支援單一贈品活動規則` |
| RegularProductPromotionRuleValidator | `Type != Gift` | `不支援的活動類型` |
| RegularProductPromotionRuleValidator | `TriggerMode != Interval` | `不支援的觸發模式` |
| RegularProductPromotionRuleValidator | `Interval` 未填/`<2`/超出配送次數上限 | `配送次數設定超出範圍` |
| RegularProductPromotionRuleValidator | `OutOfStockAction` 不在 Proceed/SkipPeriod | `不支援的缺貨處理方式` |
| RegularProductPromotionRuleValidator | 贈品清單為空 | `贈品清單不可為空` |
| RegularProductPromotionRuleValidator | 品項數量 > 10 | `贈品品項數量超出上限` |
| RegularProductPromotionRuleValidator | 總數量 > 50 | `贈品總數量超出上限` |
| RegularProductPromotionGiftItemValidator | `GiftTypeId <= 0` | `贈品序號無效，請重新輸入` |
| RegularProductPromotionGiftItemValidator | `Qty` 不在 1~10 | `贈品數量需介於 1~10 之間` |
| Service（AddProductAsync） | `ProductShippingPeriod.Count < 1` | `error_period` |
| Service（ValidateSalePageData） | 商品頁不存在（SCM 查無資料） | `商品序號無效，請重新輸入` |
| Service（ValidateSalePageData） | 交期非定期購（`ShippingTypeDef != 7`） | `此商品的「交期」需為定期購，才可進行設定，請先至商品頁重新確認` |
| Service（ValidateSalePageData） | 含點數折抵組合（`HasPointsPayPairs`） | `定期購設定僅支援一般商品，請重新輸入` |
| Service（ValidateSalePageDataForAddAsync） | 生效開始早於商品銷售開始 | `生效開始時間不得早於商品銷售開始時間` |
| Service（ValidateSalePageDataForAddAsync） | 生效結束晚於商品銷售結束 | `生效結束時間不得晚於商品銷售結束時間` |
| Service（ValidateSalePageDataForAddAsync） | 與其他方案生效期間重疊 | `此商品序號於指定生效期間已存在重疊方案，請重新輸入` |
| Service（AddProductAsync） | DB 寫入失敗（`InsertAsync` 回傳 0 rows） | `儲存失敗` |
| Service（AddProductAsync） | 全部通過 | `儲存成功`（Success） |

---

## 15. 已知限制與注意事項

1. **`ProductShippingPeriod` 為弱型別 `List<object>`**：程式僅檢查 `Count < 1`，不驗證陣列元素格式。正確格式應為字串或數字陣列（如 `["3","7"]` 或 `[7,14,30]`），若誤用物件格式（如 `{"day":1}`）會被原樣寫入 DB，造成同欄位存在不同格式並存的髒資料風險。**目前未補 FluentValidation 規則擋掉物件格式輸入**，待後續評估是否需要加強。

2. **既有髒資料未清理**：
   - `rpd_ship_period` 欄位可能存在物件格式（`{"day":1}`）與正確字串/數字陣列格式並存的歷史資料。
   - `rpd_promotion_json` 欄位可能存在 enum 為數字（修正前寫入）與文字（修正後寫入）並存的歷史資料。
   - 上述兩者**皆尚未進行 SQL migration 回溯清理**，僅保證修正後的新寫入格式正確。

3. **`ProductPurchaseOption` 值域疑問（尚未有結論）**：目前程式碼與文件皆定義為 `1=僅定期購, 2=定期購+一次性`。曾有提出「應該是 0 or 1」的疑問，但尚未查得對應的不同資料來源或明確需要修改的證據，此疑問目前**保留現狀（1/2）**，如有新的規格依據需再行確認調整。

4. **`ValidateShipLimitInput` 與 `ProductAddRequestValidator.ComputeShipLimitCount` 屬於兩份獨立但邏輯一致的計算**：前者服務 `EditProductAsync`（尚未遷移到 FluentValidation 的計算+驗證合一版本），後者服務 `ProductAddRequestValidator`／`RegularProductPromotionRuleValidator`。`AddProductAsync` 本身則是第三份（`switch` 運算式，只算數值不驗證）。三者的計算規則必須維持一致（`1→0`, `2→Max`, `3→Fixed`），若未來有調整配送次數選項的業務規則，需同步修改三處。

5. **`ExistsByProductAsync` 的修正僅套用於 `AddProductAsync`**：`ValidateProductAsync`（獨立的商品頁驗證 API，非新增流程本身）仍保留原本「同商品頁存在任一筆記錄即擋下」的邏輯，未跟進本次「允許生效期間錯開的多方案並存」的設計調整，是否需要一併修正待評估。

6. **`RegularProductPromotionRules` 本次範圍僅支援 0 或 1 筆**：`ProductAddRequestValidator` 已限制陣列長度 `<= 1`；型別上是 `List<T>`（保留未來擴充多筆的彈性），但目前業務邏輯與驗證規則皆只允許單筆。

7. **Enum 序列化與 Model Binding 的相依性**：`RegularProductPromotionRule` 內的 `Type`/`TriggerMode`/`OutOfStockAction` 三個 enum 欄位依賴全域 JSON 設定（`Program.cs` 的 `AddJsonOptions`）加上 `JsonStringEnumConverter` 才能接受字串輸入；若該全域設定被移除或覆寫，Request 帶入字串值的 enum 欄位將全部在 Model Binding 階段失敗。
