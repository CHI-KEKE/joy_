## LiveBuy 每個 Request 實際用哪個語系

由 `TranslationsMiddleware` 處理，按優先順序決定：

```
1. Query String  ?lang=zh-TW
       ↓ 沒有
2. Cookie  lang=zh-TW
       ↓ 沒有
3. 使用者偏好語系 (UserPreferenceService → DynamoDB)
       └─ 需要先從 ShopService cache 拿 SupplierId
       └─ 用 {SupplierId}_{UserId} 查 DDB
       ↓ 未登入 or 未設定
4. AppSettings DefaultLocale（例如 zh-HK）
```

設定完成後執行：
```csharp
CultureInfo.CurrentCulture   = culture;
CultureInfo.CurrentUICulture = culture;
```

### 與 OSM 的差異
| | OSM (WebStore) | LiveBuy |
|---|---|---|
| 語系決定方式 | `DetectCrossBorderDataForWeb()` | `TranslationsMiddleware` |
| 商店語系設定介入 | ✅ ShopCrossBorder | ❌ 不查商店設定 |
| 使用者偏好 | ❌ | ✅ DDB |
| Accept-Language Header | ✅ | ❌ |
| 預設語系 fallback | AppSettings | AppSettings |