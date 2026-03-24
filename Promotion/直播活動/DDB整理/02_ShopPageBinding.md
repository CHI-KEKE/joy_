# DynamoDB Table：ShopPageBinding

## 設定

| 項目 | 值 |
|------|-----|
| 用途 | 商店與 Facebook 粉專的綁定記錄 |

---

## 欄位說明

| 欄位 | 類型 | 說明 | 範例 |
|------|------|------|------|
| `ShopId` (PK) | String | 商店 ID | `"123"` |
| `Platform` | String | 平台名稱 | `"Facebook"` |
| `PlatformPageId` (SK) | String | 粉專 ID | `"987654321"` |
| `PageName` | String | 粉專名稱 | `"91APP 官方粉絲團"` |
| `PageAccessToken` | String | **加密**的 Page Access Token | `"encrypted_token..."` |
| `UserAccessToken` | String | **加密**的 User Access Token | `"encrypted_token..."` |
| `PicUrl` | String | 粉專大頭照 URL | `"https://..."` |
| `BindingStatus` | String | 綁定狀態（見下方）| `"Active"` |
| `IsEnabled` | Boolean | 是否啟用 | `true` / `false` |
| `WebhookSubscribed` | Boolean | 是否成功訂閱 Webhook | `true` / `false` |
| `TokenExpiresAt` | DateTime | Token 過期時間 | `2025-03-01T00:00:00Z` |
| `UserIdList` | String (JSON) | 授權者 userId 清單 | `["user_456"]` |
| `CreatedBy` | String | 建立者 | `"user_456"` |
| `UpdatedBy` | String | 更新者 | `"user_456"` |
| `CreatedAt` | DateTime | 建立時間 | `2024-12-30T10:00:00Z` |
| `UpdatedAt` | DateTime | 更新時間 | `2024-12-30T10:05:00Z` |

---

## Key 設計

```
Partition Key: ShopId         → 以商店為單位查詢所有綁定粉專
Sort Key:      PlatformPageId → 快速定位特定粉專的綁定
```

支援查詢模式：
- 「這間商店有哪些已綁定的粉專？」→ Query by ShopId
- 「這間商店是否已綁定某個粉專？」→ GetItem by ShopId + PlatformPageId

---

## BindingStatus 狀態說明

| 狀態 | 意義 |
|------|------|
| `Active` | 綁定正常，可使用 |
| `TokenExpired` | Token 已過期，需重新授權 |
| `Disconnected` | 已解除綁定（軟刪除，保留紀錄）|
| `WebhookPending` | OAuth 完成但 Webhook 訂閱失敗，待重試 |

---

## Token 加密機制

`PageAccessToken` 和 `UserAccessToken` 在存入 DynamoDB 前會加密：

- 加密：`ShopPageBindingService.Encrypt(entity)`
- 解密：`ShopPageBindingService.Decrypt(entity)`

讀取後需先解密才能使用，避免 Token 明文洩露。

---

## 寫入時機與程式碼

### OAuth 完成後初次寫入（`IsEnabled = false`）

```csharp
var entity = new ShopPageBinding
{
    ShopId = shopId,                           // Partition Key
    Platform = "Facebook",
    PlatformPageId = page.PageId,              // Sort Key

    PageName = page.PageName,
    PageAccessToken = page.PageAccessToken,    // ← 重要！加密後存入
    PicUrl = page.Picture?.Data?.Url,

    BindingStatus = BindingStatusEnum.Active,
    IsEnabled = false,                         // ← 預設未啟用
    WebhookSubscribed = false,

    TokenExpiresAt = tokenExpiresAt,
    UserAccessToken = userToken.AccessToken,
    UserIdList = JsonSerializer.Serialize(new[] { userId }),

    CreatedBy = userId,
    UpdatedBy = userId,
};
await this.shopPageBindingService.AddBindingAsync(entity);
```

### 使用者手動啟用後更新（`IsEnabled = true`）

```csharp
// 讀取 → 解密 → 啟用 → 加密 → 更新
var entity = await this.repository.GetByIdAsync(shopId, platform, platformPageId);
this.Decrypt(entity);
entity.IsEnabled = true;
this.Encrypt(entity);
await this.repository.UpdateAsync(entity, userId);
```

---

## 資料關係

- 一店可綁多粉專
- 一粉專可被多店綁定

---

## 來源檔案

- `粉專綁定整理/04_資料模型.md`
- `粉專綁定整理/02_OAuth授權流程.md`（步驟 7 SaveBindingsAsync 程式碼）
