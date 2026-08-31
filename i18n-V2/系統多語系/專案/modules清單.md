## 前台

WebStore\Translation\TranslationSettings.cs


這裡的 Modules[] 陣列寫死了所有後端 Server 用的 module，目前有將近 100+ 個，例如：

```csharp
new ModuleSetting { Name = "backend.definition.PayProfile" },
new ModuleSetting { Name = "backend.service.payment_middleware" },
new ModuleSetting { Name = "NineYi.WebStore.Translation.Common" },
// ...

```

