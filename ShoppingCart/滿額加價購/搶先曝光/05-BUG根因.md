# BUG 根因分析

## 問題描述

滿額加價購（`CartReachPriceExtraPurchase`）的加購品清單，顯示了處於搶先曝光狀態的商品。這類商品 `SellingStartDateTime > now`，用戶無法實際購買。

---

## 根因定位

**位置**：`CartExtraPurchaseService.BuildPromotionEntitiesAsync()`

```csharp
// 問題程式碼（L627 附近）
foreach (var salePageItem in conditionSalePageItems)
{
    var salePageId = salePageItem.SalePageId;
    var salepage = salePageDic[salePageId];

    //// 過濾售完商品
    if (salepage.IsSoldOut)
    {
        continue;
    }

    // ❌ 缺少以下判斷：
    // if (salepage.SellingStartDateTime > DateTime.Now) continue;

    var salePageEntity = await this.BuildSalePageEntityAsync(...);
    conditionEntity.SalePageList.Add(salePageEntity);
}
```

**唯一的過濾條件只有 `IsSoldOut`**，沒有 `SellingStartDateTime` 的判斷。

---

## 為什麼其他層無法修

| 層 | 原因 |
|----|------|
| MWeb 前端 | `ICartExtraPurchaseSalePage` 沒有 `SellingStartDateTime` 或 `isComingSoon` 欄位，無資料可判斷 |
| Cart Domain API | `SplCartExtraPurchaseResponseEntity` 也沒有時間欄位，SPL 就沒回傳 |
| SPL（本層） | `GetSalepageListByIdsResponse` **已有** `SellingStartDateTime`，資料就在手邊 |

---

## 為什麼 `IsSoldOut` 不能代替

| 情境 | `IsSoldOut` | `SellingStartDateTime > now` |
|------|------------|------------------------------|
| 搶先曝光（有庫存但未開賣） | `false`（有庫存） | `true`（未開賣） |
| 搶先曝光（已售完） | `true` | `true` |
| 正常商品售完 | `true` | `false` |

搶先曝光且有庫存的商品，`IsSoldOut = false`，目前**無法被過濾掉**。

---

## 為什麼不用 `IsComingSoon`

```csharp
// IsComingSoon 定義
public bool IsComingSoon =>
    ListingStartDateTime <= DateTime.Now &&
    DateTime.Now < SellingStartDateTime &&
    EnableIsComingSoon;   // ← 含商店開關
```

AC 明確說**忽略 `EnableIsComingSoon`**，若直接用 `IsComingSoon`：

- 當 `EnableIsComingSoon = false` 時，`IsComingSoon = false`
- 導致搶先曝光商品（`SellingStartDateTime` 未到）仍然進入清單
- **不符合 AC 要求**

---

## 影響範圍

- 僅影響新版滿額加價購（`CartReachPriceExtraPurchase`）的加購品清單
- 舊版加購（`PurchaseExtra` 表）使用獨立的 `PurchaseExtraRepository`，不受影響
- 商品加購（`CartAddOns`）使用不同流程，不受影響
