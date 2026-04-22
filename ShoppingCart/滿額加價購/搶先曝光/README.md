# 滿額加價購 × 搶先曝光互斥分析

> Story #596792

## 文件索引

| 檔案 | 說明 |
|------|------|
| [01-需求說明.md](./01-需求說明.md) | Story #596792 需求內容與 AC |
| [02-搶先曝光機制.md](./02-搶先曝光機制.md) | 搶先曝光（IsComingSoon）的判斷邏輯 |
| [03-完整流程追蹤.md](./03-完整流程追蹤.md) | 使用者觸發到清單渲染的完整流程 |
| [04-SPL架構分析.md](./04-SPL架構分析.md) | SPL 程式碼結構與關鍵檔案 |
| [05-BUG根因.md](./05-BUG根因.md) | 問題根源分析 |
| [06-改動建議.md](./06-改動建議.md) | 最小改動方案與實作位置 |

## 問題摘要

**搶先曝光商品（`ListingStartDateTime` 已到、`SellingStartDateTime` 未到）目前會出現在滿額加價購清單中。**

- SPL 的 `BuildPromotionEntitiesAsync` 只排除售完商品（`IsSoldOut`）
- 未對 `SellingStartDateTime` 做任何過濾
- `GetSalepageListByIdsResponse` 已有所需的時間欄位，只需補上判斷即可

## 修改層

```
只需改 SPL（九1.Salepage.Listing）
└── CartExtraPurchaseService.cs
    └── BuildPromotionEntitiesAsync()
        └── 新增 if (salepage.SellingStartDateTime > DateTime.Now) continue;
```
