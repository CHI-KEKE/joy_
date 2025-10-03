




## CreditCardInstallment_Razer 分期購物車顯示在 P2


### API

API : https://10230.shop.qa.91dev.tw/shopping/api/checkout/pay-set?lang=zh-TW&shopId=10230

Request Body
```JSON
{
    "checkoutUniqueKey": "ef17f951-7535-46e6-98bf-c546313804ab",
    "checkoutPayType": {
        "statisticsTypeDef": "CreditCardInstallment_Razer",
        "installmentType": {
            "id": 4528
        },
        "checkoutStoreCredit": null
    }
}
```

### 取得 InstallmentList

Controller : CartsController
Action : create

ProcessorTypeEnum.GetCart => GetCartProcessorLayers => GetShoppingCartInstallmentProcessor


C:\91APP\Cart\nine1.cart\src\BusinessLogic\Nine1.Cart.BL.Services\Processor\Cart\Create\GetShoppingCartInstallmentProcessor.cs

**DB**

ShopInstallment + Installment

Id, InstallmentId, InstallmentDef, InstallmentRate, InstallmentAmountLimit, HasInterest

**TW 現況**
台灣是拉 BankPaytype, 結合 paymentGateway 收單行資料去撈 沒有by shop 特性
還有用 shopStaticSettingS 存 installment 對應bankCode, 但我們會讓商店在後台存等於 staticSetting 會一直更改

**目前想要做**

想要再新增 BankId 在 ShopInstallment, 一次撈出

**資料節點**
context.Data.InstallmentList


**開幹**

既有 : ShopInstallment + installment

可以拿到 

Id = IShopInstallment_Id 
InstallmentDef = ShopInstallment_InstallmentDef
HasInterest = shopInstallment.ShopInstallment_HasInterest



**Cache**

60 min


## 整理資料進 MatchedInstallmentList

C:\91APP\Cart\nine1.cart\src\BusinessLogic\Nine1.Cart.BL.Services\Processor\Cart\Create\CalculateShoppingCartInstallmentProcessor.cs

要比對購物車金額 TotalFee or TotalPayment 之類的, 要寫死 最低金額比對
CartContext.Data.TotalPrice 可以拿到總銷售額

## 計算每期金額

C:\91APP\Cart\nine1.cart\src\BusinessLogic\Nine1.Cart.BL.Services\Processor\Cart\Create\CalculateShoppingCartEachInstallmentPriceProcessor.cs


塞入 installmentType.EachInstallmentPrice

## 回傳給前端

C:\91APP\Cart\nine1.cart\src\BusinessLogic\Nine1.Cart.BL.Services\Processor\Cart\Create\CalculateShoppingCartCheckoutPayShippingProcessor.cs

MY 要整理一夏 BankList

if (payTypeGroup.Key == StatisticsTypeDefEnum.CreditCardInstallment.ToString())
{
    //// 信用卡分期需依照商店可用分期方式，拆出多個付款方式，使用期數排序
    foreach (var installment in matchedInstallmentList.OrderBy(i => i.HasInterest).ThenBy(i => i.InstallmentDef))
    {
        payTypeGroupList.Add(new CheckoutPayTypeGroupEntity
        {
            StatisticsTypeDef = payTypeGroup.Key,
            InstallmentType = installment,
            PayTypeList = payTypeGroup.ToList()
        });
    }
}

shoppingCart.CheckoutType.DisplayPayTypeList = payTypeGroupList;















## DB


```SQL
use WebStoreDB


select *
from ShopInstallment(nolock)
inner join Installment
on Installment_Id = ShopInstallment_InstallmentId
where ShopInstallment_ValidFlag = 1
```


## BankPayType

- 以 installmentId 為維度
- (一次性 or 分期) / 哪間銀行

Installment

- 期數
- 以 MY 需求來說，installment + bank 的維度, 所以同樣是 3 期會有很多 installmentId



## 金物流交集

C:\91APP\Cart\nine1.cart\src\BusinessLogic\Nine1.Cart.BL.Services\PayShippingMapping\PayShippingMappingService.cs

## P2 金流選項

前端打 CheckoutController >> Get API

checkout?checkoutUniqueKey=${checkoutUniqueKey


```PLAINTEXT
1. @cart/src/providers/checkout.provider
2. useCheckoutMachine()   ← (內部很可能呼叫 checkout API)
3. machineContext.checkoutData
4. CheckoutProvider 的 value.checkoutData
5. useContext(CheckoutContext) → checkoutData
6. checkoutType = checkoutData.checkoutType
7. displayPayTypeList = checkoutType.displayPayTypeList
8. payTypeList = getPayTypeList(displayPayTypeList)
9. 
<PaymentListContainer>
    <PaymentList
        payTypeList={payTypeList}
        currentValue={currentValue}
        currentShippingType={selectedCheckoutShippingTypeGroup?.shippingProfileTypeDef}
        handleCurrentValueChange={handlePayTypeChange}
        openDialog={openDialog}
        closeDialog={closeDialog}
        isMobile={isMobile}
        isShowCashOnDeliveryItem={isShowCashOnDeliveryItem}
        isShowMoreStorePayMethodItem={getIsEnableMoreStorePayMethod(
            selectedCheckoutShippingTypeGroup?.shippingProfileTypeDef,
            displayShippingTypeList
        )}
        payMethodTagList={shopPayTypeDisplaySettingDetailList}
        selectedDesignatePromotion={checkoutData.selectedDesignatePaymentPromotionId}
        designatePromotionClick={setDesignatePromotion}
    />
</PaymentListContainer>
```



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


## 怎麼傳送分期交易資料

https://shop2.shop.qa1.hk.91dev.tw/shopping/api/checkout/complete?lang=zh-HK&shopId=2

paymentMiddlewareCreditCardInfo
```json
{
    "creditCardNo": "4242",
    "creditCardDate": "0130",
    "creditCardCvv": "***",
    "brand": "Visa",
    "identity": "824EFC5FDF3AFDF37DBAFAB82B9A1529FE26258A7BB839AF5520858DA4B46C6E",
    "ExtendInfo":{
        "InstallmentDef":3
    }
}
```



## Razer 串前台付款

C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\BLV2\ThirdPartyPay\PaymentMiddlewareService.cs

CreditCardOnce_Razer
CreditCardInstallment_Razer


因為 Resolve Paychannel 的方式是取 _ 後面那一個所以會拿到相同的 PayChannelService


## 交易貸幣別的方式

根據市場

```csharp
var currency = this.GetRequestCurrency(tgCode, context.ShoppingCartV2.ShopId, context.PayProfileType);

       /// <summary>
        /// 取得貨幣符號
        /// </summary>
        public static string DefaultCurrencyCode
        {
            get
            {
                if (_defaultCountry == string.Empty)
                {
                    throw new Exception("Please set default country.");
                }

                switch (_defaultCountry)
                {
                    case "MY":
                        return "MYR";

                    case "HK":
                        return "HKD";

                    case "TW":
                    default:
                        return "TWD";
                }
            }
        }
```

## 怎麼帶 context.txn_channel

```csharp
private string GetChannel(PayProcessContextEntity context)
{
    switch (context.PayProfileType)
    {
        case PayProfileTypeDefEnum.CreditCardOnce_Razer:
            return "CREDIT";
        case PayProfileTypeDefEnum.CreditCardInstallment_Razer:
            return "CREDITBA";
        case PayProfileTypeDefEnum.OnlineBanking_Razer:
            return context.ShoppingCartV2.SelectedCheckoutPayTypeGroup.PayTypeChannel.PayTypeChannelCode.ToUpper();
        case PayProfileTypeDefEnum.GrabPay_Razer:
            return "GRABPAY";
        case PayProfileTypeDefEnum.Boost_Razer:
            return "BOOST";
        case PayProfileTypeDefEnum.TNG_Razer:
            return "TNG-EWALLET";
        default:
            throw new NotImplementedException("Unsupport PayProfileType");
    }
}
```


## GetPayExtendInfo

context.PayProfileType == PayProfileTypeDefEnum.CreditCardOnce_Razer



## paymentmiddleware params

付款
RMS/API/Direct/1.4.0/index.php






## checkout get 評估

shopping/api/checkout?checkoutUniqueKey=e6e4706c-1a39-4157-823b-56b90fc9fb5f&lang=zh-TW&shopId=10230

只是拿快取得資料

