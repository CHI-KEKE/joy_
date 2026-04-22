# 04 — CartExtraPurchaseAuditProcessor 實作

## 檔案位置

**新建：** `src/Nine1.Commerce.Console.BL/Services/AuditProcessor/CartExtraPurchaseAuditProcessor.cs`

---

## 依賴注入

```csharp
private readonly INine1PromotionClient _promotionClient;
private readonly ILogger<CartExtraPurchaseAuditProcessor> _logger;
```

---

## IsSkip 實作

```csharp
public bool IsSkip(PayProcessContextEntity context)
{
    // non-cache 路徑不執行
    if (context.IsForCache == false) return true;

    // 沒有滿額加價購加購品則跳過
    var hasExtraPurchase = context.ShoppingCartV2.SalePageGroupList
        .SelectMany(g => g.SalePageList)
        .Any(s => s.IsCartExtraPurchase == true);

    return hasExtraPurchase == false;
}
```

---

## ExecuteAsync 主流程

```csharp
public async Task ExecuteAsync(PayProcessContextEntity context)
{
    var shopId = context.ShoppingCartV2.ShopId;
    var orderDateTime = context.ShoppingCartV2.ShoppingDateTime ?? context.EventOrderDateTime;

    // Step 1: 收集所有加購品
    var extraPurchaseItems = context.ShoppingCartV2.SalePageGroupList
        .SelectMany(g => g.SalePageList)
        .Where(s => s.IsCartExtraPurchase == true)
        .ToList();

    var salePageIds = extraPurchaseItems
        .Select(s => s.SalePageId)
        .Distinct()
        .ToList();

    // Step 2: 取得下單當下在期的活動 IDs
    var ongoingPromotions = await this._promotionClient
        .GetOngoingCartExtraPurchasePromotionsAsync(shopId, orderDateTime);

    if (ongoingPromotions.Any() == false)
    {
        // 找不到在期活動，但 Cache 有加購品 → 異常
        this.LogFail(context, "找不到在期的滿額加價購活動");
        return;
    }

    // Step 3: 彙總所有活動的加購價設定
    var allSpecialPrices = new List<GetPromotionSpecialPriceResponseEntity>();

    foreach (var promotion in ongoingPromotions)
    {
        var specialPrices = await this._promotionClient
            .GetPromotionSpecialPricesAsync(shopId, promotion.Id, salePageIds);

        allSpecialPrices.AddRange(specialPrices);
    }

    // Step 4: 逐一比對每個加購品
    foreach (var salePage in extraPurchaseItems)
    {
        var matched = allSpecialPrices.FirstOrDefault(sp =>
            sp.SalepageId == salePage.SalePageId &&
            sp.SaleProductSKUId == salePage.SaleProductSKUId &&
            sp.Id == salePage.SpecialPriceId);

        if (matched == null)
        {
            this.LogFail(context,
                $"加購品找不到對應的活動設定價格 SalePageId={salePage.SalePageId} SkuId={salePage.SaleProductSKUId} SpecialPriceId={salePage.SpecialPriceId}");
            continue;
        }

        if (salePage.Price != matched.TypeValue)
        {
            this.LogFail(context,
                $"加購品價格不符 SalePageId={salePage.SalePageId} SkuId={salePage.SaleProductSKUId} " +
                $"CachePrice={salePage.Price} ActivityPrice={matched.TypeValue}");
        }
    }
}
```

---

## Log 輸出格式（參考現有 Processor）

```csharp
private void LogFail(PayProcessContextEntity context, string reason)
{
    var metricsLog = new MetricsLogEntity
    {
        // 填入必要欄位，參考 SalepageExtendInfoAuditProcessor 的寫法
        Message = $"[CartExtraPurchaseAudit] {reason}",
        // ...
    };
    this._logger.LogInformation("{@MetricsLog}", metricsLog);
}
```

> 請參考 `SalepageExtendInfoAuditProcessor` 中 `MetricsLogEntity` 的建構方式，保持一致的 log 格式。

---

## 完整類別骨架

```csharp
public class CartExtraPurchaseAuditProcessor : IAuditProcessor
{
    private readonly INine1PromotionClient _promotionClient;
    private readonly ILogger<CartExtraPurchaseAuditProcessor> _logger;

    public CartExtraPurchaseAuditProcessor(
        INine1PromotionClient promotionClient,
        ILogger<CartExtraPurchaseAuditProcessor> logger)
    {
        this._promotionClient = promotionClient;
        this._logger = logger;
    }

    public bool IsSkip(PayProcessContextEntity context) { ... }

    public async Task ExecuteAsync(PayProcessContextEntity context) { ... }
}
```
