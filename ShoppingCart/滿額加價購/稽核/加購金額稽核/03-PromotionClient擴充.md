# 03 — PromotionClient 擴充

## 異動檔案

| 檔案 | 異動類型 |
|---|---|
| `INine1PromotionClient.cs` | 新增兩個方法宣告 |
| `Nine1PromotionClient.cs` | 新增兩個方法實作 |
| `Model/ApiClients/Promotion/` | 新增兩個 Response Model |

---

## 1. 新增 Response Models

### `GetOngoingPromotionResponseEntity.cs`

**路徑：** `src/Nine1.Commerce.Console.BL/Model/ApiClients/Promotion/GetOngoingPromotionResponseEntity.cs`

```csharp
namespace Nine1.Commerce.Console.BL.Model.ApiClients.Promotion;

public class GetOngoingPromotionResponseEntity
{
    /// <summary>
    /// 活動序號（即 PromotionEngineId）
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 活動名稱
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// 活動類型
    /// </summary>
    public string TypeDef { get; set; }

    /// <summary>
    /// 活動開始時間
    /// </summary>
    public DateTime StartDateTime { get; set; }

    /// <summary>
    /// 活動結束時間
    /// </summary>
    public DateTime EndDateTime { get; set; }
}
```

### `GetPromotionSpecialPriceResponseEntity.cs`

**路徑：** `src/Nine1.Commerce.Console.BL/Model/ApiClients/Promotion/GetPromotionSpecialPriceResponseEntity.cs`

```csharp
namespace Nine1.Commerce.Console.BL.Model.ApiClients.Promotion;

public class GetPromotionSpecialPriceResponseEntity
{
    /// <summary>
    /// 特殊價格序號（即 SpecialPriceId）
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 商品頁序號
    /// </summary>
    public long SalepageId { get; set; }

    /// <summary>
    /// 商品選項序號
    /// </summary>
    public long SaleProductSKUId { get; set; }

    /// <summary>
    /// 活動設定加購價
    /// </summary>
    public decimal TypeValue { get; set; }

    /// <summary>
    /// 門檻序號
    /// </summary>
    public int ConditionId { get; set; }
}
```

---

## 2. INine1PromotionClient 新增方法

```csharp
/// <summary>
/// 取得指定時間點在期的滿額加價購活動清單
/// </summary>
/// <param name="shopId">商店序號</param>
/// <param name="orderDateTime">下單時間</param>
/// <returns>在期活動清單</returns>
Task<List<GetOngoingPromotionResponseEntity>> GetOngoingCartExtraPurchasePromotionsAsync(
    long shopId,
    DateTime orderDateTime);

/// <summary>
/// 取得指定活動的加購品特殊價格清單
/// </summary>
/// <param name="shopId">商店序號</param>
/// <param name="promotionEngineId">活動序號</param>
/// <param name="salePageIds">加購品商品頁序號清單</param>
/// <returns>加購品特殊價格清單</returns>
Task<List<GetPromotionSpecialPriceResponseEntity>> GetPromotionSpecialPricesAsync(
    long shopId,
    int promotionEngineId,
    List<long> salePageIds);
```

---

## 3. Nine1PromotionClient 方法實作

### GetOngoingCartExtraPurchasePromotionsAsync

```csharp
public async Task<List<GetOngoingPromotionResponseEntity>> GetOngoingCartExtraPurchasePromotionsAsync(
    long shopId,
    DateTime orderDateTime)
{
    var baseAddress = this._configuration["_N1CONFIG:InternalServiceDomains:PromotionService:HttpClientSetting:Uri"];
    var url = $"{baseAddress}/api/promotion-rule/ongoing-list";
    var headers = this.GetRequestHeaders(shopId);

    var body = new
    {
        Type = new[] { "CartReachPriceExtraPurchase" },
        StartDate = orderDateTime,
        EndDate = DateTime.Now
    };

    var result = await this._sendRequestService.SendPostRequestAsync(shopId, url, headers, body);
    var response = JsonSerializer.Deserialize<ApiResponseEntity<List<GetOngoingPromotionResponseEntity>>>(result.Response);

    return response?.Data ?? new List<GetOngoingPromotionResponseEntity>();
}
```

### GetPromotionSpecialPricesAsync

```csharp
public async Task<List<GetPromotionSpecialPriceResponseEntity>> GetPromotionSpecialPricesAsync(
    long shopId,
    int promotionEngineId,
    List<long> salePageIds)
{
    var baseAddress = this._configuration["_N1CONFIG:InternalServiceDomains:PromotionService:HttpClientSetting:Uri"];
    var url = $"{baseAddress}/api/promotion-price-lists/get-promotion-price";
    var headers = this.GetRequestHeaders(shopId);

    var body = new
    {
        PromotionEngineId = promotionEngineId,
        SalePageIds = salePageIds
    };

    var result = await this._sendRequestService.SendPostRequestAsync(shopId, url, headers, body);
    var response = JsonSerializer.Deserialize<ApiResponseEntity<List<GetPromotionSpecialPriceResponseEntity>>>(result.Response);

    return response?.Data ?? new List<GetPromotionSpecialPriceResponseEntity>();
}
```

---

## 補充：ApiResponseEntity wrapper

Promotion API 回傳格式為：
```json
{ "Code": "Success", "Data": [...] }
```

確認 `Nine1.Commerce.Console.BL` 內是否已有對應的 wrapper model，若無則需新增：
```csharp
public class ApiResponseEntity<T>
{
    public string Code { get; set; }
    public T Data { get; set; }
}
```
