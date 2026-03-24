


```csharp
//// 舊版APP因相容緣故, 本島/離島 DeliveryTypeList 放一起, 導致ShippingAreaId有落差, 重新指定ShippingAreaId
shoppingCart.SelectedCheckoutShippingTypeGroup.ShippingAreaId = shoppingCart.SelectedCheckoutShippingTypeGroup.DeliveryTypeList.Find(i => i.IsSelected == true).ShippingAreaId;
```