# GetIsEnabledInSettingByShopId 判斷邏輯說明

## 概述

`SettingHelper.GetIsEnabledInSettingByShopId` 是一個共用方法，用來判斷「特定商店是否啟用某項功能」。  
目前用於控制 **使用者偏好語系（UserPreferLanguage）** 功能的 per-shop 開關。

對應的設定 key：`_N1CONFIG:TranslationSetting:UserPreferLanguage.EnabledShopIds`

---

## 設定字串格式

```
"{全域開關}|{個別商店清單}|{商店範圍清單}"
```

三個區段以 `|` 分隔，缺一不可（共三段）。

| 區段 | 說明 | 範例值 |
|------|------|--------|
| 全域開關 | `true` / `false` | `true` |
| 個別商店清單 | 逗號分隔的 ShopId，不指定時填 `none` | `8,233` 或 `none` |
| 商店範圍清單 | 逗號分隔的範圍，不指定時填 `none` | `30-40,100-120` 或 `none` |

---

## 判斷流程

```
輸入：shopId + 設定字串
         ↓
[關卡 1] 全域開關 = false？
         → YES → 回傳 false（不管後兩段，直接拒絕）
         → NO ↓
[關卡 2] 個別清單 = "none" 且 範圍清單 = "none"？
         → YES → 回傳 true（全部商店開放）
         → NO ↓
[關卡 3-A] shopId 在個別清單裡？
         → YES → 回傳 true
         → NO ↓
[關卡 3-B] shopId 落在任一範圍內？
         → YES → 回傳 true
         → NO → 回傳 false
```

> **三道關卡按順序檢查，任一關卡 return 就不繼續往下。**

---

## 實例驗證

### 範例一：全域關閉

```
設定值：false|none|none
```

| ShopId | 結果 | 原因 |
|--------|------|------|
| 任意值 | ❌ false | 全域開關為 false，直接拒絕 |

---

### 範例二：全部開放

```
設定值：true|none|none
```

| ShopId | 結果 | 原因 |
|--------|------|------|
| 任意值 | ✅ true | 全域開啟且未指定任何條件，全部放行 |

---

### 範例三：個別商店白名單

```
設定值：true|2|none
```

| ShopId | 結果 | 原因 |
|--------|------|------|
| 2 | ✅ true | 個別清單命中 |
| 99 | ❌ false | 不在清單，範圍也是 none |

---

### 範例四：多個商店白名單

```
設定值：true|10229,10230,12595|none
```

| ShopId | 結果 | 原因 |
|--------|------|------|
| 10229 | ✅ true | 個別清單命中 |
| 10230 | ✅ true | 個別清單命中 |
| 12595 | ✅ true | 個別清單命中 |
| 99999 | ❌ false | 不在清單，範圍也是 none |

---

### 範例五：個別商店 + 範圍同時指定

```
設定值：true|8,233|30-40,100-120
```

| ShopId | 結果 | 原因 |
|--------|------|------|
| 8 | ✅ true | 個別清單命中 |
| 233 | ✅ true | 個別清單命中 |
| 35 | ✅ true | 落在 30~40 範圍內 |
| 110 | ✅ true | 落在 100~120 範圍內 |
| 50 | ❌ false | 不在清單，也不在任何範圍 |
| 9 | ❌ false | 不在清單，也不在任何範圍 |

---

## 各環境設定值對照

| 環境 | 設定值 | 說明 |
|------|--------|------|
| Prod HK | `true\|none\|none` | 全部商店開啟 |
| Prod MY | `true\|none\|none` | 全部商店開啟 |
| Prod TW | `false\|none\|none` | 全部關閉（尚未開放）|
| HK QA | `true\|2\|none` | 僅 shopId=2 開啟 |
| MY QA | `true\|4\|none` | 僅 shopId=4 開啟 |
| TW QA | `true\|10229,10230,12595\|none` | 僅指定 3 間測試店開啟 |
| Local Dev | `false\|none\|none` | 預設全部關閉 |

---

## 常見誤解

### `none` 不是數字 0

`none` 是佔位符，代表「**未指定任何條件**」。  
`true|none|none` 的含意是：「全域開啟，但沒有任何限制條件」→ **全部放行**。

### 全域開關優先級最高

只要全域開關為 `false`，後兩段的內容完全不會被讀取。  
例如 `false|8,233|30-40` 的結果永遠是 `false`，即使 shopId=8 也一樣。

### 個別清單與範圍清單是 OR 關係

兩者只要滿足其中一個條件即回傳 `true`，不需要同時符合。

---

## 程式碼位置

| 項目 | 路徑 |
|------|------|
| 共用方法 | `Common/Nine1.Livebuy.Common.Utils/Helpers/SettingHelper.cs` |
| 呼叫位置 | `Web/Nine1.Livebuy.Web.Api/Middleware/TranslationsMiddleware.cs` |
| 設定 key | `_N1CONFIG:TranslationSetting:UserPreferLanguage.EnabledShopIds` |
| 環境 config 範例 | `config/settings.HK-QA.json`、`config/settings.TW-QA.json` 等 |
