# DynamoDB Table：ProductKeyword（直播可販售清單）

## 用途

DynamoDB 不是商品資料庫，而是「**直播可販售清單 + 關鍵字索引表**」。

解決以下查詢：
- 這場直播有哪些商品？
- 某個關鍵字能不能對到 SKU？
- 這個 SKU 是否在直播中有效？

---

## 資料關係

```
LiveSession × SalePage × SKU
```

一場直播活動（LiveSession）對應多筆商品（SalePage），每個商品下有多個 SKU，每個 SKU 對應各自的關鍵字。

---

## 欄位說明

> 欄位名稱從程式碼與 entity 中推導，原始文件未提供完整 schema definition。

| 欄位 | 說明 |
|------|------|
| `LiveSessionId` (PK) | 直播場次 ID |
| `SalePageId` (SK 之一) | 商品頁序號 |
| `SkuId` | SKU ID |
| `Keyword` | 對應關鍵字（新增時預設為空，需後續設定）|
| `Status` | 狀態（新增時預設為關閉）|
| `Source` | 來源（前台新增 / 匯入等）|

---

## 商品上限

每場直播最多可新增 **500 筆** 商品（SalePage 計算）。

---

## 寫入時機與程式碼

### 新增商品到直播（`POST /api/live/sessions/{liveSessionId}/products`）

```csharp
// Controller 接收請求後，轉發給服務層處理
var result = await _liveSessionService.AddLiveSessionProductsAsync(
    liveSessionId,
    request.ShopId,
    request,
    cancellationToken);
```

**Service 層處理流程：**

#### 步驟 1：驗證直播場次存在與權限

```csharp
await ValidateLiveSessionAccessAsync(liveSessionId, shopId, cancellationToken);
// - 檢查直播場次是否存在
// - 確認這個商店有權限操作這個直播場次
```

#### 步驟 2：過濾重複商品

```csharp
var (newSalePageIds, duplicateFailedProducts, currentCount, maxAllowedCount) =
    await FilterAndValidateSalePageIdsAsync(liveSessionId, request.SalePageIds, cancellationToken);
// - 去除輸入中的重複序號
// - 排除已在直播中的商品
// - 確認總數不超過 500 筆上限
```

#### 步驟 3：批次驗證每個商品（呼叫 SKU Query API）

```csharp
var (productKeywordEntities, successSalePageIds, validationFailedProducts) =
    await ValidateProductsAndCreateEntitiesAsync(
        liveSessionId, shopId, newSalePageIds, maxAllowedCount,
        request.Source, cancellationToken);

ValidateProductRestrictions(skuQueryResponse)
```

**不支援的商品類型：**

| 條件 | 錯誤訊息 |
|------|---------|
| 商品頁狀態為關閉 | 不支援商品頁狀態為關閉 |
| 組合商品 | 不支援組合商品 |
| 贈品 | 不支援贈品 |
| NFT 商品 | 不支援 NFT |
| 電子禮券 | 不支援電子禮券 |
| 純門市商品 | 不支援純門市商品 |
| 點加金商品 | 不支援點加金商品 |

#### 步驟 4：寫入 DynamoDB

```csharp
//// 每個 SKU 都會建立一筆記錄，預設關鍵字為空，狀態為關閉
var writeResult = await _productKeywordRepository.AddProductKeywordsAsync(
    liveSessionId,
    productKeywordEntities,
    cancellationToken);
```

#### 步驟 5：回傳結果

```csharp
return new AddLiveSessionProductsResponseEntity
{
    AddedCount = addedCount,            // 成功加入的數量
    TotalProducts = totalProducts,      // 直播活動總商品數
    FailedProducts = failedSalePageIds  // 失敗的商品序號列表
};
```

---

## 完整資料流程

```bash
使用者輸入序號
    ↓
前端驗證（格式、數量）
    ↓
POST /api/live/sessions/{liveSessionId}/products
    ↓
Service 層處理：
  1. 驗證權限
  2. 過濾重複
  3. 批次查詢商品（SKU Query API）
  4. 檢查商品限制
  5. 寫入 DynamoDB（關鍵字空、狀態關閉）
    ↓
回傳結果
    ↓
前端顯示：
  - 全部成功：Toast
  - 部分失敗：Dialog 顯示失敗清單
  - 全部失敗：Dialog 要求返回編輯
```

---

## 來源檔案

- `直播可以用的商品清單.md`
