# 商品 APP 獨賣設定

## 功能說明

「APP 獨賣」是指將商品設定為僅限在 APP 上購買，網頁版不提供購買入口。

---

## 設定頁面入口

**頁面名稱**：促銷管理 → 商品活動列表

**QA 環境網址**：
```
https://sms.qa.91dev.tw/Promotion/ActivityList?shopId=12765
```

**路由**：`/Promotion/ActivityList`

---

## 程式碼對應位置

| 類型 | 路徑 |
|------|------|
| MVC Controller | `WebSite/WebSite/Controllers/PromotionController.cs` → `ActivityList()` |
| View | `WebSite/WebSite/Views/Promotion/ActivityList.cshtml` |
| TS Controller | `WebSite/WebSite/TypeScripts/Modules/Promotion/activityList.controller.ts` |
| 資料庫欄位 | `WebStoreDB.SalePage.SalePage_IsAPPOnly` (bit) |
| BE Entity | `BusinessLogic/BE/SalePages/SalePageDataEntity.cs` → `IsAPPOnly` |
| Mapper | `CrossLayer/Mappers/SalePages/SalePageDataEntityMappingProfile.cs` |

---

## 操作流程

1. 進入「商品活動列表」頁面，輸入條件搜尋商品
2. 勾選一或多筆要設定的商品
3. 點擊「**APP 獨賣**」按鈕（頁面上下各一，僅一般商店顯示）
4. 跳出 Popup 對話框，選擇「**新增**」或「**移除**」APP 獨賣設定
5. 確認後系統呼叫 `UpdateSalePageActivity()`，透過 SCM API 更新資料庫

---

## 注意事項

- 此功能**僅限「一般商店」**，輕量版 (Lite) 商店不顯示此按鈕
  - 前端判斷：`ng-if="ActivityListCtrl.IsGeneralShop"`
- APP 獨賣、限購、分享後購買三種設定共用同一個 Popup，以 `SalePageActivityTypeEnum.AppOnly` 區分
- 設定後，商品清單頁 (`/SalePage/List`) 會顯示「APP 獨賣」標籤

---

## 資料層流程

```
前端 (ActivityList.cshtml)
  → 點擊「APP 獨賣」按鈕
  → OpenSettingAppOnlyDialog()  [activityList.controller.ts]
  → 選擇新增/移除後 UpdateSalePageActivity()
  → SalePageService.UpdateSalePageActivity()  [BL.Services]
  → ScmApi ISalePageClient.UpdateSalePageActivity()
  → 更新 WebStoreDB.SalePage.SalePage_IsAPPOnly
```

---

## 相關 Entity 欄位

| Entity | 屬性 | 說明 |
|--------|------|------|
| `SalePageDataEntity` | `bool IsAPPOnly` | 商品詳細資料 |
| `SalePageListEntity` | `bool? IsAPPOnly` | 商品清單 |
| `SalePageActivityEntity` | `bool IsAPPOnly` | 活動商品清單 |
| `SalePageListSearchParameterEntity` | `bool? IsLimitedSalePage` | 查詢條件（`true` 代表含 APP 獨賣商品） |

---

## 顯示邏輯

### 商品清單頁 (`/SalePage/List`)

```html
<p data-ng-show="Sale.IsAPPOnly" class="exposure">APP 獨賣</p>
```

### 商品活動列表 (`/Promotion/ActivityList`)

```html
<p data-ng-show="Item.IsAPPOnly">APP 獨賣</p>
```
