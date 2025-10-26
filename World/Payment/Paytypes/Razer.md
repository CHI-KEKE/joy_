




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

(CartEntity)
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




## 訂購完成頁文案

https://lilychuang2.shop.qa1.my.91dev.tw/V2/Pay/Finish/?k=3020f014-05e4-4d63-ba30-af518e09cd61&shopId=4#complete

## 「付款方式」顯示「信用卡{6}期0利率」

```csharp
var jsondata = @Html.Raw(rx.Replace(Newtonsoft.Json.JsonConvert.SerializeObject(this.Model.PayContext).Replace("'", "\\'"), ""));
var displaySalePageGroupList = Html.Raw(Newtonsoft.Json.JsonConvert.SerializeObject(this.Model.DisplaySalePageGroupList));
window.nineyi.ServerData
    payProcessData: @jsondata
    displaySalePageGroupList: @displaySalePageGroupList
```

### inspect

<!-- C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\MobileWebMallV2\ClientApp\src\shopping\components\payFinish\payAndInvoiceInfo.tsx

```TYPESCRIPT
import { IPayAndInvoiceInfoData, IDeliveryInfoData } from '@src/shopping/components/payFinish/usePayProcessData';
interface IPayAndInvoiceInfoProps {
    currencyFormat: ICurrencyFormat;
    data: IPayAndInvoiceInfoData;
    isShowMemberInvoiceInfo: boolean;
    deliveryInfoData: IDeliveryInfoData;
}
<MainContentRow>
        <Translate value="client.shopping.pay_finish.pay_mode" />：
        <MainContentRowItem data-qe-id="payTypeDisplayName">
            {data.selectedCheckoutPayTypeList.map((item) => (
                <span key={`payTypeDisplayName-${item.payProfileType}`}>
                    <span>{`${item.name}`}</span>&nbsp;
                    <span>
                        <Currency
                            value={
                                isUseExchangeRateCurrency
                                    ? exchangeRateTotalPayment
                                    : item.amount
                            }
                            decimal={decimalDigits}
                            symbol={symbol}
                            rate={rate}
                            isDisableTheme
                            isNegativeSymbolPrefix
                        />
                    </span>
                </span>
            ))}
        </MainContentRowItem>
    </MainContentRow>
</MainContent>
```

data: IPayAndInvoiceInfoData

1. 追查 IPayAndInvoiceInfoData


C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\MobileWebMallV2\ClientApp\src\shopping\components\payFinish\payFinishContext.ts
```TYPESCRIPT
import usePayProcessData from '@src/shopping/components/payFinish/usePayProcessData';
const usePayFinish = () => {
    const { payProcessData } = getServerData<{ payProcessData: IPayProcessContextEntity }>();
```


所以應該是 load 頁面的時候拿 server data 的 selectedCheckoutPayTypeList 的 payProfileType & name -->

C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\MobileWebMallV2\ClientApp\src\shopping\components\payFinish\usePayProcessData.ts

```typescript
selectedCheckoutPayTypeDisplayName: payProcessData.ShoppingCartV2.SelectedCheckoutPayTypeGroup.DisplayName

<InfoRow>
    {/* 付款方式 */}
    <InfoTitle>
        <Translate value="client.shopping.pay_finish.pay_mode" />
    </InfoTitle>
    <InfoContent data-testid="payTypeDisplayName">
        {data.selectedCheckoutPayTypeDisplayName}
    </InfoContent>
</InfoRow>
```




## 訂單總額下方顯示「{6}期0利率，每期 {16.67}」（依實際金額計算）

C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\MobileWebMallV2\ClientApp\src\shopping\components\common\cart\amountCalculationList.tsx


```typescript
import { IAmountCalculationData } from '@src/shopping/hooks/useAmountCalculationData';
interface IAmountCalculationListProps {
    currencyFormat?: ICurrencyFormat;
    data: IAmountCalculationData;
}
const AmountCalculationList: FC<IAmountCalculationListProps> = ({ data, currencyFormat }) => {
    const {
        attributes,
        crmShopMemberCardName,
        deliveryFeeInfoList,
        designatePaymentPromotionDiscountForDisplay,
        eCouponDiscount,
        freeShippingECouponDiscount,
        installmentInfo,
        isUseExchangeRateCurrency,
        loyaltyPointDiscount,
        memberTierPromotionDiscount,
        promotionDiscount,
        shopShippingTypePromotionDiscount,
        subTotalPayment,
        totalFee,
        totalLoyaltyPointsPay,
        totalPayment,
        totalPrice,
    } = data;

<AmountCalculationListItem>
    <TotalPaymentTitle>
        {/* 訂單總額 */}
        {I18n.t('client.shopping.checkouts.order_total_price')}
    </TotalPaymentTitle>
    <TotalPaymentAmount>
        <Currency value={totalPayment} decimal={decimalDigits} symbol={symbol} rate={rate} />
        {/* (分N期，每期約$N) */}
        {installmentInfo && (
            <React.Fragment>
                <br />
                <AmountCalculationHelpText>({installmentInfo.DisplayName})</AmountCalculationHelpText>
            </React.Fragment>
        )}
    </TotalPaymentAmount>
</AmountCalculationListItem>
```

installmentInfo
installmentInfo.DisplayName


### 追查 data

C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\MobileWebMallV2\ClientApp\src\shopping\hooks\useAmountCalculationData.tsx

```typescript
import { IShoppingCartClientEntity } from '@src/shopping/models/models';
interface IUseAmountCalculationDataProps {
    cartData: IShoppingCartClientEntity;
    selectedDeliveryFeeInfoList?: IDeliveryFeeInfo[];
}
export interface IAmountCalculationData {
    // TODO 再湊多少享優惠，shoppingCart.shopShippingTypeHints
    // TODO 交易幣別提示
    attributes: {
        isOnlyNormalTemperature: boolean;
    };
    crmShopMemberCardName: string;
    deliveryFeeInfoList: {
        fee: number;
        hint: string;
        temperature: string;
    }[];
    designatePaymentPromotionDiscountForDisplay: number;
    eCouponDiscount: number;
    freeShippingECouponDiscount: number;
    installmentInfo: NineYi.WebStore.Frontend.BE.Shop.ShopInstallmentEntity;
    isUseExchangeRateCurrency: boolean;
    loyaltyPointDiscount: number;
    memberTierPromotionDiscount: number;
    promotionDiscount: number;
    shopShippingTypePromotionDiscount: number;
    subTotalPayment: number;
    totalFee: number;
    totalLoyaltyPointsPay: number;
    totalPayment: number;
    totalPrice: number;
}
installmentInfo: cartData.SelectedCheckoutPayTypeGroup?.InstallmentType
```



## SelectedPayTypeGroup


C:\91APP\Cart\cart2\nine1.cart\src\BusinessLogic\Nine1.Cart.BL.Services\CheckOut\CheckOutService.cs

會在 PaySet 就設定! 所以在這裡選到正確的分期維度很重要


## GetShoppingCartInstallmentProcessor

C:\91APP\Cart\cart2\nine1.cart\src\BusinessLogic\Nine1.Cart.BL.Services\Processor\Cart\Create\GetShoppingCartInstallmentProcessor.cs

本來就會拉 shopInstallment 的 shopInstallmentIds

當前端戴上 InstallmentId, 我在 payset 就可以拿到每期的資料





## db


```sql
-- shopInstallmentBank
select *
from ShopInstallmentBank (nolock)

-- iNSERT INTO ShopInstallmentBank
-- (
--     ShopInstallmentBank_ShopId,
--     ShopInstallmentBank_ShopInstallmentId,
--     ShopInstallmentBank_BankPayTypeId,
--     ShopInstallmentBank_AmtLimit,
--     ShopInstallmentBank_Rate,
--     ShopInstallmentBank_HasInterest,
--     ShopInstallmentBank_CreatedDateTime,
--     ShopInstallmentBank_CreatedUser,
--     ShopInstallmentBank_UpdatedTimes,
--     ShopInstallmentBank_UpdatedDateTime,
--     ShopInstallmentBank_UpdatedUser,
--     ShopInstallmentBank_ValidFlag
-- )
-- VALUES
-- (
--     32,                     -- ShopInstallmentBank_ShopId
--     204,                      -- ShopInstallmentBank_ShopInstallmentId
--     57,                       -- ShopInstallmentBank_BankPayTypeId
--     10,                       -- ShopInstallmentBank_AmtLimit
--     0.000,                     -- ShopInstallmentBank_Rate
--     0,                         -- ShopInstallmentBank_HasInterest
--     getdate(), -- ShopInstallmentBank_CreatedDateTime
--     'allenlin@nine-yi.com',    -- ShopInstallmentBank_CreatedUser
--     0,                         -- ShopInstallmentBank_UpdatedTimes
--     getdate(), -- ShopInstallmentBank_UpdatedDateTime
--     'allenlin@nine-yi.com',    -- ShopInstallmentBank_UpdatedUser
--     1                         -- ShopInstallmentBank_ValidFlag
-- );
-- --(
-- --    32,                     -- ShopInstallmentBank_ShopId
-- --    1,                      -- ShopInstallmentBank_ShopInstallmentId
-- --    58,                       -- ShopInstallmentBank_BankPayTypeId
-- --    10,                   -- ShopInstallmentBank_AmtLimit
-- --    0.000,                     -- ShopInstallmentBank_Rate
-- --    0,                         -- ShopInstallmentBank_HasInterest
-- --    getdate(), -- ShopInstallmentBank_CreatedDateTime
-- --    'allenlin@nine-yi.com',    -- ShopInstallmentBank_CreatedUser
-- --    0,                         -- ShopInstallmentBank_UpdatedTimes
-- --    getdate(), -- ShopInstallmentBank_UpdatedDateTime
-- --    'allenlin@nine-yi.com',    -- ShopInstallmentBank_UpdatedUser
-- --    1                         -- ShopInstallmentBank_ValidFlag
-- --);

-- shopInstallment
select *
from ShopInstallment

-- INSERT INTO ShopInstallment
-- (
--     ShopInstallment_SupplierId,
--     ShopInstallment_ShopId,
--     ShopInstallment_InstallmentId,--
--     ShopInstallment_InstallmentDef,--
--     ShopInstallment_InstallmentRate,--
--     ShopInstallment_HasInterest,--
--     ShopInstallment_CreatedDateTime,
--     ShopInstallment_CreatedUser,
--     ShopInstallment_UpdatedTimes,
--     ShopInstallment_UpdatedDateTime,
--     ShopInstallment_UpdatedUser,
--     ShopInstallment_ValidFlag
-- )
-- VALUES
-- (
--     34,
--     32,
--     3,
--     12,
--     0.000,
--     0,
--     getdate(),
--     'allenlin@nine-yi.com',
--     0,
--     getdate(),
--     'allenlin@nine-yi.com',
--     1
-- )
--,
--(
--    100,
--    100,
--    2,
--    6,
--    0.000,
--    0,
--    getdate(),
--    'allenlin@nine-yi.com',
--    0,
--    getdate(),
--    'allenlin@nine-yi.com',
--    1
--);

--Bank
select *
from Bank

-- BankPaytype
select BankPayType_BankName,STRING_AGG(BankPayType_InstallmentDef,',') as N'支援期數'
from BankPayType(nolock)
WHERE BankPayType_ValidFlag = 1
group by BankPayType_BankName


--join all
use WebStoreDB

declare @shopId bigint = 32;

SELECT 
    si.ShopInstallment_Id           AS ShopInstallmentId,
    i.Installment_Id                AS InstallmentId,
    si.ShopInstallment_InstallmentDef AS ShopInstallmentDef,
    sib.ShopInstallmentBank_AmtLimit AS ShopInstallmentBankAmtLimit,
    sib.ShopInstallmentBank_Rate     AS ShopInstallmentBankRate,
    sib.ShopInstallmentBank_HasInterest AS ShopInstallmentBankHasInterest,
    bpt.BankPayType_BankName        AS ShopInstallmentBankName
FROM ShopInstallmentBank sib
INNER JOIN ShopInstallment si
    ON sib.ShopInstallmentBank_ShopInstallmentId = si.ShopInstallment_Id
INNER JOIN Installment i
    ON si.ShopInstallment_InstallmentId = i.Installment_Id
INNER JOIN BankPayType bpt
    ON sib.ShopInstallmentBank_BankPayTypeId = bpt.BankPayType_Id
WHERE i.Installment_StatusDef = 1
  AND i.Installment_ValidFlag = 1
  AND si.ShopInstallment_ShopId = @shopId
  AND si.ShopInstallment_ValidFlag = 1
  AND sib.ShopInstallmentBank_ShopId = @shopId
  AND sib.ShopInstallmentBank_ValidFlag = 1
  AND bpt.BankPayType_StatusDef = 1
  AND bpt.BankPayType_ValidFlag = 1;


  SELECT 
    si.ShopInstallment_Id AS ShopInstallmentId,
    i.Installment_Id AS InstallmentId,
    si.ShopInstallment_InstallmentDef AS ShopInstallmentDef,
    sib.ShopInstallmentBank_AmtLimit AS ShopInstallmentBankAmtLimit,
    sib.ShopInstallmentBank_Rate AS ShopInstallmentBankRate,
    sib.ShopInstallmentBank_HasInterest AS ShopInstallmentBankHasInterest,
    bpt.BankPayType_BankName AS ShopInstallmentBankName
FROM (
    SELECT *
    FROM [ShopInstallmentBank] AS [sib]
    WHERE [sib].[ShopInstallmentBank_ShopId] = @shopId
      AND [sib].[ShopInstallmentBank_ValidFlag] = 1
) AS [sib]
INNER JOIN (
    SELECT *
    FROM [ShopInstallment] AS [si]
    WHERE [si].[ShopInstallment_ShopId] = @shopId
      AND [si].[ShopInstallment_ValidFlag] = 1
) AS [si] 
    ON [sib].[ShopInstallmentBank_ShopInstallmentId] = [si].[ShopInstallment_Id]
INNER JOIN (
    SELECT *
    FROM [Installment] AS [i]
    WHERE [i].[Installment_StatusDef] = 1
      AND [i].[Installment_ValidFlag] = 1
) AS [i]
    ON [si].[ShopInstallment_InstallmentId] = [i].[Installment_Id]
INNER JOIN (
    SELECT *
    FROM [BankPayType] AS [bpt]
    WHERE [bpt].[BankPayType_StatusDef] = 1
      AND [bpt].[BankPayType_ValidFlag] = 1
) AS [bpt]
    ON [sib].[ShopInstallmentBank_BankPayTypeId] = [bpt].[BankPayType_Id];


-- salepagePaytype
select SalePagePayType_ValidFlag,SalePagePayType_TypeDef,*
from SalePagePayType(nolock)
where SalePagePayType_SalePageId = 6036

--shopPaytype
select *
from ShopPayType
where ShopPayType_TypeDef = 'CreditCardInstallment_Razer'
--where PayProfile_TypeDef = 'CreditCardInstallment_Razer'

--INSERT INTO ShopPayType
--(
--    ShopPayType_SupplierId,
--    ShopPayType_ShopId,
--    ShopPayType_TypeDef,
--    ShopPayType_IsEnabled,
--    ShopPayType_CreatedDateTime, -- 
--    ShopPayType_CreatedUser, --
--    ShopPayType_UpdatedTimes, --
--    ShopPayType_UpdatedDateTime, --
--    ShopPayType_UpdatedUser, --
--    ShopPayType_ValidFlag, -- 
--    ShopPayType_PayProfileTypeDef
--)
--VALUES
--(
--    34,
--    32,
--    'CreditCardInstallment_Razer',
--    1,
--    getdate(),
--    'allenlin@nine-yi.com',
--    0,
--    getdate(),
--    'allenlin@nine-yi.com',
--    1,
--    'CreditCardInstallment_Razer'
--);

--supplierPaytype
select *
from SupplierPayType
where SupplierPayType_ValidFlag = 1
and SupplierPayType_TypeDef = 'CreditCardInstallment_Razer'


--INSERT INTO SupplierPayType
--(
--    SupplierPayType_TypeDef,
--    SupplierPayType_SupplierId,
--    SupplierPayType_IsEnabled,
--    SupplierPayType_IsDefault,
--    SupplierPayType_CreatedDateTime,
--    SupplierPayType_CreatedUser,
--    SupplierPayType_UpdatedTimes,
--    SupplierPayType_UpdatedDateTime,
--    SupplierPayType_UpdatedUser,
--    SupplierPayType_ValidFlag,
--    SupplierPayType_PayProfileTypeDef
--)
--VALUES
--(
--    'CreditCardInstallment_Razer',
--    34,
--    1,
--    0,
--    getdate(),
--    'allenlin@nine-yi.com',
--    1,
--    getdate(),
--    'allenlin@nine-yi.com',
--    1,
--    'CreditCardInstallment_Razer'
--);



--PayShippingMapping
select *
from PayShippingMapping



```




## error code 分析


🔴 Failed (failed)
銀行端原因：

信用卡餘額不足
信用卡已過期
信用卡號碼錯誤
CVV 驗證失敗
發卡銀行系統暫時無法處理
交易金額超過單筆或日限額
🔴 RejectHold (reject/hold)
銀行端原因：

風險管控：交易被銀行風控系統標記為可疑
地理位置限制：跨國交易被拒絕
交易模式異常：與持卡人平常消費習慣不符
黑名單商戶：該商戶被銀行列入高風險清單
反洗錢檢查：觸發 AML (Anti-Money Laundering) 規則
🔴 Blocked (blocked)
銀行端原因：

信用卡凍結：持卡人主動凍結或銀行凍結
發卡銀行封鎖：發卡行暫停該卡片使用
商戶限制：銀行限制該卡片在特定商戶消費
安全性封鎖：多次輸入錯誤密碼或異常活動
🔴 Cancelled (cancelled)
銀行端原因：

持卡人取消：在 3D 驗證過程中主動取消
超時取消：3D 驗證超過時間限制
發卡行取消：發卡銀行在處理過程中主動取消
🔴 Chargeback (chargeback)
銀行端原因：

持卡人爭議：持卡人向銀行申請退款爭議
未經授權交易：持卡人否認進行此交易
商品/服務糾紛：對商品品質或服務不滿意
重複扣款：同一筆交易被重複處理
🔴 Release (release)
銀行端原因：

授權取消：預授權交易被釋放未完成結算
商戶主動釋放：商戶決定不完成此筆交易
超時釋放：預授權超過有效期限自動釋放
🔴 ReqCancel / ReqChargeback
銀行端原因：

系統性取消：銀行系統要求取消交易
合規性要求：因法規遵循要求必須取消
技術性問題：銀行技術系統問題導致
⚠️ Pending / Unknown (等待中)
銀行端原因：

3D 驗證進行中：等待持卡人完成 3D Secure 驗證
銀行審核中：大額交易需要人工審核
跨行清算延遲：銀行間系統處理延遲
網路延遲：銀行回應延遲或連線問題



## 文案

### 預設 Razer catch 失敗文案


backend.service.payment_middleware

```csharp
/// <summary>
/// 信用卡一次付清目前無法使用，請嘗試其他付款
/// </summary>
public static string RequestPaymentErrorCreditCardOnceRazer { get { return GetString("RequestPaymentError_CreditCardOnce_Razer"); }}
```


### paymenymiddleware 包成失敗的文案

backend.v2.pay_channel

```csharp
/// <summary>
/// 付款失敗，請回購物車重新結帳
/// </summary>
public static string ErrorFail { get { return GetString("error_fail"); }}
```



## Razer 最低金額不符預期， API 規格以及版本對不起來，導致最低金額有問題


## 最低金額 / 支援卡別 hardcode 後續維護需要注意 



## veno 後台


MerchantID： vinoair
Email：boonhan.ng@fiamma.com.my
Password：20rehyTt
[4:37 PM] 他們Razer登入賬密