


## ShoppingCartClientPayTypeEntity

```CSHARP
/// <summary>
/// 信用卡卡別
/// </summary>
public List<string> SupportedCardBrandList
{
    get
    {
        switch (this.StatisticsTypeDef)
        {
            case nameof(StatisticsTypeDefEnum.CreditCardOnce):
            case nameof(StatisticsTypeDefEnum.CreditCardInstallment):
                {
                    return new List<string>
                    {
                        nameof(CreditCardBrandEnum.Visa),
                        nameof(CreditCardBrandEnum.MasterCard),
                        nameof(CreditCardBrandEnum.JCB)
                    };
                }

            case nameof(StatisticsTypeDefEnum.CreditCardOnce_Stripe):
                {
                    return new List<string>
                    {
                        nameof(CreditCardBrandEnum.Visa),
                        nameof(CreditCardBrandEnum.MasterCard),
                        nameof(CreditCardBrandEnum.AMEX),
                        nameof(CreditCardBrandEnum.UnionPay)
                    };
                }

            case nameof(StatisticsTypeDefEnum.CreditCardOnce_CheckoutDotCom):
                {
                    return new List<string>
                    {
                        nameof(CreditCardBrandEnum.Visa),
                        nameof(CreditCardBrandEnum.MasterCard),
                        nameof(CreditCardBrandEnum.AMEX)
                    };
                }

            case nameof(StatisticsTypeDefEnum.CreditCardOnce_Razer):
                {
                    return new List<string>
                    {
                        nameof(CreditCardBrandEnum.Visa),
                        nameof(CreditCardBrandEnum.MasterCard)
                    };
                }

            case nameof(StatisticsTypeDefEnum.CreditCardOnce_AsiaPay):
                {
                    return new List<string>
                    {
                        nameof(CreditCardBrandEnum.Visa),
                        nameof(CreditCardBrandEnum.MasterCard),
                    };
                }

            default:
                {
                    return default(List<string>);
                }
        }
    }
}
```