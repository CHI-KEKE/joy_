


## ShopInstallmentEntity

DisplayName : 分期顯示文字

```CSHARP
/// <summary>
/// 分期顯示文字
/// </summary>
public string DisplayName
{
    get
    {

        string displayName = string.Format(
            Translation.Backend.Entity.ShopInstallment.PayForPayments,
            this.InstallmentDef,
            this.HasInterest ? Translation.Backend.Entity.ShopInstallment.InterestRates : Translation.Backend.Entity.ShopInstallment.ZeroInterestRates);

        //// 金額.ToCurrency()，要改用CurrencyService.ToCurrency(shopId, 金額)，因BE專案無法使用BLV2
        //// 先用暫解.ToString("0.####")只顯示數值，待前端確認要補上幣別，再把BE, BL搬到V2，從Controller改呼叫 V2 Service
        var eachInstallmentPrice = ((decimal)this.EachInstallmentPrice).ToString("0.####");

        if (this.EachInstallmentPrice > 0)
        {
            displayName = string.Format(
                Translation.Backend.Entity.ShopInstallment.PerMonthInstallment,
                displayName,
                eachInstallmentPrice);
        }


        return displayName;
    }

}
```