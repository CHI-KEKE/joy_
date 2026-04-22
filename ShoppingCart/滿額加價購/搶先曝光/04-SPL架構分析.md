# SPL 架構分析

> Repo：`nine1.salepage.listing.web.api`
> 路徑：`C:\91APP\Salepage\salepage-listing\nine1.salepage.listing.web.api\src`

## 專案結構

```
src/
├── BusinessLogic/
│   ├── Nine1.Salepage.Listing.BL.BE/          # Business Entity 層
│   │   ├── SalePage/
│   │   │   └── GetSalepageListByIdsResponse.cs ← 含 IsComingSoon / SellingStartDateTime
│   │   ├── Cart/
│   │   │   └── CartExtraPurchasePromotionEntity.cs
│   │   └── PurchaseExtra/
│   │       └── PurchaseExtraSalePageEntity.cs  ← 舊版 Entity
│   │
│   └── Nine1.Salepage.Listing.BL.Services/    # 服務層
│       └── Cart/
│           ├── ICartExtraPurchaseService.cs
│           └── CartExtraPurchaseService.cs     ← ⚠️ 新版流程（主要修改點）
│
└── DataAccess/
    └── Nine1.Salepage.Listing.DA.Repositories/
        └── Salepage/
            ├── IPurchaseExtraRepository.cs
            └── PurchaseExtraRepository.cs      ← 舊版流程（PurchaseExtra 表）
```

---

## 新版流程 vs 舊版流程

| 項目 | 新版（PromotionEngine） | 舊版（PurchaseExtra 表） |
|------|------------------------|------------------------|
| 活動來源 | `PromotionEngine` 表 | `PurchaseExtra` 表 |
| 商品來源 | `PromotionEngineSpecialPrice` | `PurchaseExtraSlave` |
| 進入點 | `CartExtraPurchaseService` | `PurchaseExtraRepository` |
| 時間過濾 | ❌ 只過濾售完 | `ListingStartDateTime <= startTime` |
| 目前使用 | ✅ 滿額加價購（CartReachPriceExtraPurchase） | 舊版商品加購專區 |

> **注意**：Story #596792 涉及的是**新版流程（PromotionEngine）**，舊版不在本次修改範圍。

---

## 關鍵類別：`GetSalepageListByIdsResponse`

路徑：`BL.BE/SalePage/GetSalepageListByIdsResponse.cs`

```csharp
public class GetSalepageListByIdsResponse
{
    // 時間欄位（已存在）
    public DateTime SellingStartDateTime { get; set; }  // 銷售開始日期
    public DateTime SellingEndDateTime   { get; set; }  // 銷售結束日期
    public DateTime ListingStartDateTime { get; set; }  // 上架開始日期
    public DateTime ListingEndDateTime   { get; set; }  // 上架結束日期

    // 搶先曝光相關（已存在）
    public bool EnableIsComingSoon { get; set; }

    // 計算屬性（含商店開關，AC 說忽略開關，故不直接使用）
    public bool IsComingSoon =>
        ListingStartDateTime <= DateTime.Now &&
        DateTime.Now < SellingStartDateTime &&
        EnableIsComingSoon;

    // 售完（目前唯一被用於過濾的欄位）
    public bool IsSoldOut { get; set; }
}
```

**結論**：所需的 `SellingStartDateTime` 欄位**已存在**於服務內部，不需要從外部引入新欄位。

---

## 關鍵方法：`BuildPromotionEntitiesAsync`

路徑：`BL.Services/Cart/CartExtraPurchaseService.cs`

此方法負責組合最終的活動+條件+商品結構，是商品進入清單的**最後一道關卡**。

```csharp
// 目前邏輯（簡化版）
foreach (var salePageItem in conditionSalePageItems)
{
    var salepage = salePageDic[salePageId];

    if (salepage.IsSoldOut) continue;   // ✅ 已有售完過濾

    // ❌ 缺少 SellingStartDateTime 過濾

    conditionEntity.SalePageList.Add(await BuildSalePageEntityAsync(...));
}
```
