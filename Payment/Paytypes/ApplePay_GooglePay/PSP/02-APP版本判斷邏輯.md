# PSP (PaymentServiceProfile) — APP 版本判斷邏輯

## 說明

若 APP 版本過舊（24.4 以前）不會抓 PSP，有可能導致預設的 NCCC 傳入，造成走錯付款流程。

---

## 程式碼邏輯

```csharp
private async Task SetCheckoutPaymentServiceProvider(CheckoutContext context, string key, CheckoutPaymentServiceProviderEntity checkoutPSP)
{
    var profile = await GetPaymentServiceProviderProfile(context.ShopId, key);
    if (profile == null)
    {
        return;
    }

    var userClientTrack = context.Data.UserClientTrack;
    if (userClientTrack.IsApp == false
        || profile!.AppVer == null
        || IsAppVerSupported())
    {
        checkoutPSP.PaymentServiceProvider = profile!.PaymentServiceProvider;
        checkoutPSP.Acquiring = profile!.Acquiring;
    }

    bool IsAppVerSupported()
    {
        return string.IsNullOrWhiteSpace(userClientTrack.AppVersion) == false &&
        Version.TryParse(userClientTrack.AppVersion, out _) &&
        userClientTrack.CheckAppVersionIsUnsupported(profile!.AppVer!) == false;
    }
}
```

---

## 判斷條件說明

| 條件 | 結果 |
|------|------|
| 不是 APP（`IsApp == false`） | 直接套用 PSP Profile |
| `AppVer == null`（不指定版本限制） | 直接套用 PSP Profile |
| APP 版本符合要求（`IsAppVerSupported() == true`） | 套用 PSP Profile |
| APP 版本不符合（過舊） | **不套用**，維持原有設定（可能落入 NCCC 預設） |
