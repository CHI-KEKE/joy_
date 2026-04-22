# Story #596790 — 分享活動互斥（加購品）

## 文件索引

| 文件 | 內容 |
|------|------|
| [01-需求說明.md](./01-需求說明.md) | Story AC、情境說明 |
| [02-分享活動機制.md](./02-分享活動機制.md) | 分享活動在各層的實作方式 |
| [03-完整流程追蹤.md](./03-完整流程追蹤.md) | 舊流程 vs 新流程的路徑差異 |
| [04-BUG根因.md](./04-BUG根因.md) | 根因分析與新舊流程差異 |
| [05-改動建議.md](./05-改動建議.md) | 結論與建議動作 |

## 核心結論

**現狀不需要改 Code，加購品已自然繞過分享活動限制。**

- SPL `SalepageService.SalepagesSearchByIdsAsync()` L235 硬設 `IsIncludingShareToBuy = true`，分享活動商品會出現在加購清單
- Cart Domain 所有 Processor 不讀取 `IsShareToBuy`，不阻擋加購或結帳
- CartClientApp `ISalepage` 介面無 `isShareToBuy` 欄位，不顯示任何分享要求提示
- 舊版 `addToCartDirective` 的分享阻斷，新版加購流程完全不走此路徑
