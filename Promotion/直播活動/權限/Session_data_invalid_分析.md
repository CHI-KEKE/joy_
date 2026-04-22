# Session data invalid 日誌分析

> 專案：`nine1.live.buy`  
> 來源檔案：`src/Web/Nine1.Livebuy.Web.Api/Sso/Middleware/SsoAuthenticationMiddleware.cs`

---

## 日誌訊息

```
Session data invalid for session {SessionId};
auth null={IsAuthNull}
user null={IsUserNull}
authKeyExists={AuthKeyExists}
userKeyExists={UserKeyExists}
created={CreatedAt:o}
lastAct={LastActivity:o}
```

---

## 觸發位置

此 `LogWarning` 發生於 `SsoAuthenticationMiddleware` 的 SSO 認證流程中，由 `SessionPipelineHelper.IsSessionDataInvalid()` 回傳 `true` 時觸發。

---

## `IsSessionDataInvalid` 的三個判斷條件

```csharp
// 條件 1：任一資料從 Redis 讀不到
if (authData == null || userData == null)
    return true;

// 條件 2：時間戳記為預設值（資料損壞）
if (authData.CreatedAt == default || userData.LastActivity == default)
    return true;

// 條件 3：超過絕對逾時時間
if (now > authData.CreatedAt.Add(options.AbsoluteTimeout))
    return true;
```

---

## 對應的實際情境

| 情境 | `IsAuthNull` | `IsUserNull` | `AuthKeyExists` | `UserKeyExists` | 說明 |
|---|---|---|---|---|---|
| **Session 自然過期** | `true` | `true` | `false` | `false` | Redis TTL 到期，兩筆 key 都消失 |
| **部分 key 遺失** | `true` | `false` | `false` | `true` | Auth key 被提前刪除但 User key 尚在（資料不一致）|
| **絕對逾時** (`AbsoluteTimeout`) | `false` | `false` | `true` | `true` | 兩筆 key 都在，但 Session 建立時間 + AbsoluteTimeout < 現在時間 |
| **資料損壞** | `false` | `false` | `true` | `true` | Key 存在但 `CreatedAt` 或 `LastActivity` 為 `default` |

---

## 執行流程

```
請求帶 sid cookie
  → 從 Redis 讀取 authData / userData
  → IsSessionDataInvalid() == true
    → 額外查詢 key presence（記錄 AuthKeyExists / UserKeyExists）
    → LogWarning（即此訊息）
    → HTTP 401 Unauthorized 回傳
```

---

## 日誌欄位解讀重點

| 狀況 | 代表意義 |
|---|---|
| `authKeyExists=true` 且 `auth null=true` | Redis key 存在但資料反序列化失敗（內容損壞） |
| `authKeyExists=false` 且 `auth null=true` | Redis TTL 已到期，key 已消失 |
| `auth null=false` 且 `lastAct=0001-01-01` | Session 資料存在但 `LastActivity` 初始化異常 |

---

## 最終行為

觸發此日誌後，無論哪種情境，伺服器均回傳 **HTTP 401 Unauthorized**，要求用戶端重新登入。
