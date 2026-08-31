
## 🏷️ Attribute 設定

在 Property 上掛載 RequireDefinition Attribute 來標記需要套用 Definition 的欄位：

<br>

```csharp
/// <summary>
/// TypeDefDesc
/// </summary>
[RequireDefinition(Table = "ShopPayType", Column = "ShopPayType_TypeDef")]
public string TypeDefDesc { get; set; }
```