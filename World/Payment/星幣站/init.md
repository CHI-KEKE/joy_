QA 使用 shopId = 80

PMW 要全 QA 切過去新的 Domain 嗎


installment - CREDITBA


HTTP Response - Status: OK, Body: "{\"MerchantID\":\"vinoair\",\"ReferenceNo\":\"MG251103U00001\",\"TxnID\":\"3236184631\",\"TxnType\":\"SALS\",\"TxnCurrency\":\"MYR\",\"TxnAmount\":\"1000.00\",\"TxnChannel\":\"CREDITBA\",\"TxnData\":{\"RequestURL\":\"https:\/\/pay.fiuu.com\/RMS\/card_intermediate.php\",\"RequestMethod\":\"GET\",\"RequestType\":\"REDIRECT\",\"RequestData\":{\"q\":\"48d1cbf7773236184631bf347de4e22ac181ff01074af94011\"}}}"

PROD - 一次 CREDIT

HTTP Response - Status: OK, Body: "{\"MerchantID\":\"arospring\",\"ReferenceNo\":\"MG251106Q00013\",\"TxnID\":\"3242901465\",\"TxnType\":\"SALS\",\"TxnCurrency\":\"MYR\",\"TxnAmount\":\"1110.10\",\"TxnChannel\":\"CREDIT\",\"TxnData\":{\"RequestURL\":\"https:\/\/pay.fiuu.com\/RMS\/redirection_to_3rdparty.php\",\"RequestMethod\":\"POST\",\"RequestType\":\"REDIRECT\",\"RequestData\":{\"q\":\"78sVptMry0CWejZCOT+qTNs36rN0v7hBBND5yOx9H9+IOWC4HQm6v+EoY8en0znbCJolQUhSHxUTwPm6AalL7Z3jujsZskFE6aVrLwePl8Y3pKsiEIDltXXT9BgQXN6IOhplhy\/Mqdszborao5GVOO6AzlB2CZgx4Rne8UN3spW+floN0A7q6l7NljXrsuovZI9msLZ1ep42kPO5F4nlo8P7oxBBaU8B36\/UQ0ExVOywJ3vimWSfYsd7yVPHxoY5YZCV1Mz8kOLSVqivX\/nYkRAvHHPqttInD4MKgbq0baUOAOjyC0NTLY\/XaCO96JiUEL5ZVbJspKg3Agnecq3gPXhBcQbJGqp6dYwzKfYXW3ErnpGZogjo+pdHeArt+ZkxD\/Tx8tJB\/017rVOaTy1y+C59Uu3NHXVI+hl3FxFpUMygvOQz0uKR3go9dn7MfYHvwv+75gxthOQZC0c9ThOisNLI1TOVKo\/EYZzW4D5SMVfRDv5LpNMterox8bZOPK+LY+9\/G05Gc2z8d64saIsA7ICKnqP3SxlwMhb8tRpv2XbZgM+doPpNTNLnFDjRt9fv83I5jYmxfqSDQ6kF7phFSseyGR1vm1WHGhDY9Ph1PTvaiwOzGHnPg83147h6rvm5d0eOSDdi73fIcLUcWPjvDBKcW32U5Hud4+7O98RFe7qpxx0M004cQsjbG5jH\/urQrDM5grswQNiakstyx7RYbYhoOdEiqi0j0M6P5yHS5tWTKiOtydrbJ8EJv3BvhZcimYJfRlXa6sJ5r+zxybrpSDj902G29v77W0dep3b0+1JIlZNXtkugFO\/FPsCRtbQBiqITwiHLgHGrwlTc0zu7IV0MVV8qQJkWypomCIx7BZf4XrXuD6tmgEf7JLfvcHPr4QOu3kTRua6JaMZUKhI0ZZKAHCVAC5dKDbNiMzGlH6TrH4OI8CGwJLrjGSupLsblG59WYXY6d3KZHIKMxJGIplYSpubwgNK0M4qUt5wvfez3Q7uGT1VhPNsbZFaSfAV2R8+BGs3k3k9ujoyPsv6JYjP4GRqPLxo3\/JyW3UoYorEO6FWlowieDDI6y2Mw1Z5v9dWMFRZKmABq0Gl40Pm0UDLkQWlXTkJk+yGDtOGCV7qNfK0tCGLKvEX26u\/MuBY4IZLYcMyRq9LA6oEpOcsChKq6gXfXzE8VsHu18Hmqn3swN2RXeN0LuQCP07R3v5pcgF3W5w\/NBVi64q31Ngx6gj8uEa9BpT\/1S8AYCc1KIxm4Cim6zBShi1Em7vJ1MXUcQOjjgByu2khNl9bu0X7qSCxhXqQ5+qnUOaIgb9HrEvXCkPA0OeA5Ijab46k5VoVvYiGA8FnuVkW\/dZ8zWFCmh7QkR1rt9hZNMzUJcV1AcXntOE2jBf4ZlPP+yVRETnt0vxHcaH4X5sDu\/Y9OXsNYNYo+V1PI59uA77Ita909\/5I2Qgy9bT7yk3D7B5XbBwJAfCXfLOS9p7cnlbnm7e6dQJbj1Z9mZ37SGJEO2pYfbRuPLsjE2Y\/mE1QncjLskwFD8aL0DSg95VJT1dv+xEnfbzOdtfNxHOVURwdfT\/AQqfy+3kXaWAXHkQEIeuXliK72twqbfkw1UZ1wCI50IQK3NlmRQZjuAkoM5dySasH6NTSAflK\/N+UUvhbsHlc9FswJVPs9dMno1CLPnLE4YJCFRh\/KU0sV8Dk8larZKUs0zaqflFLkPZy\/PkBqJURdphKtpVWk+EOUhMD\/wEJQWWSmXdoJTNLLYj11BDXUJNGuuqtWYAzBLmOSn7v4TcrsHxm71WismLw3SEagoEed40nhEPfz0EC4R27cDZzI1W2NE3rL\/HYEF++tq\/H59P63APAHaO\/wvAgsB9vNxsynX2uNaQ\/8\/ny0T\/p2bib6EbEN49NI5jovHXXuF2g9kjhxY5TbuyFPcl\/bx5cjnTdGja9ZZI27J+yCfCl0+Zf7hgD476uJ9wV3s1wRsmb2iUgkIIABrDhto+TeTA8BQlaDQYEL\/rSFY\/SIXQtessixmQIzla2+5jSe4BLeVzHzk1g1\/2S\/fXISQUcfbmLGNAXjowJHlMzTbYrk\/XURA1rkXbIYJ0BBAxMiwIVMT+rYS4LbtjJ8QrIKM3iWh8LF2ftAE4feTd6PIJZOr1Vc\/tqrFBHgyyIFU\/XkAgluDA0xETvWOUuNB\/+o6Dt3JvNKGfRqoHY4C9pBTnlQVKAixiWlEBCmmhNM1RHJY0+Sr2oK10Jbi+m1mqRn\/4MOd2aU1hJFRAckMEC0JEmpOS0NJXMROV1gcTPKdHv7KIn7eX58sP0fLzxI+Se0UJVRpT3d1IcPda0HLjKETZfWWeA22qRmktRBjgtf1pxDkVvhayMN\/KC6cybztil9zVgJp3SJOn0ASKQZC9o8uH3ciGdbFeLpR6JCqwBg+x7Ryl1w\/4xsRvdt6N6jEeeva5WiE4ucRTQF83lVN3wzd6smCxJPRuGsx0qmO7pvl6YpIranAVzR09MePdJ\/Jj4pmjCS9q9acKm++V18EhtB1KQhmrbGSJDd1I\/DpKRWBrlntavw28qBO\/tASTTLBV7Iw7FLcA6RbRQN1dJMVfyx9Yjs+rscBlvMX\/k\/J8PAOCsb73OXx1xOuXJZvliHumoGD4UQDMh\/2CT+N6iXNuK0buqRESZzshX5PKcmMxitVOFtWOUsOordP2m\/Zppzs4szrEgjPF2DJrV75PxuGej+cld\/7uhrkz+sopSA8M0=:a767400cf3790ff6c2800c0abb1b1733\"}}}"





## 第三方金物流 - 要 by salesmarket 顯示

#### API


`https://sms.qa1.my.91dev.tw/Api/ThirdPartyServices/Payment/GetPaymentSettingsInfo?shopId=80`

#### Request

shopId = 80

#### Response
```json
{
    "Status": "Success",
    "Data": [
        {
            "ShopId": 80,
            "Key": 33554432,
            "IsSettingsValid": true,
            "SettingsStatus": "NotFilled",
            "IsDisplayEditHistory": true,
            "PaymentMethods": [
                "信用卡一次付清",
                "信用卡分期付款",
                "網路銀行",
                "Touch 'n Go",
                "Boost",
                "Grab Pay"
            ],
            "ApplyStatusDef": null,
            "EnableStatusDef": null
        },
        {
            "ShopId": 80,
            "Key": 4294967296,
            "IsSettingsValid": false,
            "SettingsStatus": "NotFilled",
            "IsDisplayEditHistory": true,
            "PaymentMethods": [
                "Atome"
            ],
            "ApplyStatusDef": null,
            "EnableStatusDef": null
        }
    ],
    "ErrorMessage": null,
    "TimeStamp": "2025-11-06T11:44:07.3877554+08:00"
}
```


#### 主邏輯

key: 

        CreditCardOnce_Razer = 1L << 25,


ThirdPartyServicesService.cs

誰有實作
IThirdPartyServiceSettingService


註冊 (Razer 相關)
C:\91APP\NineYi.Sms\CrossLayer\Modules\BL\ServiceModule.cs
```csharp
builder.RegisterType<RazerMSSettingService>()
    .Keyed<IThirdPartyServiceSettingService>(nameof(PayProfileTypeEnum.GlobalPay));

//// CreditCardInstallment => SimpleShopDefault
//// CreditCardOnce / OnlineBanking / Boost / GrabPay / TNG_
//// => ShopSecret
builder.RegisterType<SimpleShopDefaultPaymentSettingService>()
    .Keyed<IThirdPartyServiceSettingService>(nameof(PayProfileTypeEnum.CreditCardInstallment_Razer));

builder.RegisterType<SimpleShopSecretPaymentSettingService>()
    .Keyed<IThirdPartyServiceSettingService>(nameof(PayProfileTypeEnum.CreditCardOnce_Razer));

builder.RegisterType<SimpleShopSecretPaymentSettingService>()
    .Keyed<IThirdPartyServiceSettingService>(nameof(PayProfileTypeEnum.OnlineBanking_Razer));

builder.RegisterType<SimpleShopSecretPaymentSettingService>()
    .Keyed<IThirdPartyServiceSettingService>(nameof(PayProfileTypeEnum.Boost_Razer));

builder.RegisterType<SimpleShopSecretPaymentSettingService>()
    .Keyed<IThirdPartyServiceSettingService>(nameof(PayProfileTypeEnum.GrabPay_Razer));

builder.RegisterType<SimpleShopSecretPaymentSettingService>()
    .Keyed<IThirdPartyServiceSettingService>(nameof(PayProfileTypeEnum.TNG_Razer));
```

shopDefault

GroupTypeDef
CreditCardOnce_Razer : Razer_CreditCardOnce
CreditCardInstallment_Razer : Razer_CreditCardInstallment

Key 就會拿到
ShopDefault_Key
Razer_CreditCard_MerchantId
Razer_CreditCard_SecretKey
Razer_CreditCard_VerifyKey


GetServiceSettingInfo




### 後台去哪裡取 SalesMarket



        /// <summary>
        /// IMarketModuleAgentService
        /// </summary>
        private readonly IMarketModuleAgentService _marketModuleAgentService;

                /// <summary>
        /// Initializes a new instance of the <see cref="PromoCodeBaseValidator{T}"/> class.
        /// </summary>
        /// <param name="marketModuleAgentService">IMarketModuleAgentService</param>
        public PromoCodeBaseValidator(IMarketModuleAgentService marketModuleAgentService)
        {
            this._marketModuleAgentService = marketModuleAgentService;




this._marketModuleAgentService.GetShopSalesMarketSettings(promoCode.ShopId).CurrencyDecimalDigits.Equals(0)

## ShopPayShippingDefault_PayProfileTypeDef

- 如果是其他 asiapay 可能會沒有付款方式


## 搞錯 ServiceSettingService

SimpleShopSecretPaymentSettingService.cs

RazerMSShopSecretSetting.cs


## new endpoint 異常

https://sandbox-payment.fiuu.com/RMS/q_by_tid.php
https://sandbox-payment.fiuu.com/RMS/API/refundAPI/index.php
https://sandbox-payment.fiuu.com/RMS/API/refundAPI/q_by_txn.php 


Hi @~JackyHu , for sandbox need to use sandbox-api.fiuu.com domain

For production, you need to use api.fiuu.com domain ya


測試卡

https://wiki.91app.com/pages/viewpage.action?pageId=56342554



## db

use WebStoreDB

select Supplier_SalesCurrency,Supplier_SalesMarket,*
from Supplier(nolock)
where Supplier_Id = 76

select Shop_SupplierId,*
from Shop(nolock)
where Shop_Id= 80



-- GrabPay_Razer
--　OnlineBanking_Razer
-- CreditCardOnce_Razer
use ERPDB


select PayProfile_Currency,PayProfile_TypeDef,PayProfile_StatisticsTypeDef,*
from PayProfile(nolock)
--where PayProfile_TypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')
--and PayProfile_StatisticsTypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')

update PayProfile
set PayProfile_Currency = 　N'["MYR","SGD"]'
where PayProfile_TypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')
and PayProfile_StatisticsTypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')

select PayProfile_Currency,PayProfile_TypeDef,PayProfile_StatisticsTypeDef,*
from PayProfile(nolock)
where PayProfile_TypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')
and PayProfile_StatisticsTypeDef in ('CreditCardOnce_Razer','OnlineBanking_Razer','GrabPay_Razer')



## 付款 / 配送設定
 

https://sms.qa1.my.91dev.tw/Api/Shop/GetPayType

ShopPayShippingDefault
_webStoreDbContext.SupplierPayType
csp_GetPayProfile

儲存按鈕 會吃前端 config
WebSite/WebSite/TypeScripts/Configs/localization.config.ts


```json
{
    "Status": "Success",
    "Data": [
        {
            "SupplierId": 34,
            "ShopId": 32,
            "TypeDef": "CreditCardOnce_Razer",
            "TypeDefDesc": "Razer - 信用卡一次付清",
            "PayProfilePaymentMethod": null,
            "PayProfileTypeDef": "CreditCardOnce_Razer",
            "StatisticsTypeDef": "CreditCardOnce_Razer",
            "IsEnabled": true,
            "IsDefault": true,
            "Editable": true,
            "ShopInstallment": null,
            "ThirdPartyInstallment": null,
            "InvoiceIssueType": "NoInvoiceRequired",
            "CanUseForeignCard": null
        },
        {
            "SupplierId": 34,
            "ShopId": 32,
            "TypeDef": "CreditCardInstallment_Razer",
            "TypeDefDesc": "Razer - 信用卡分期付款",
            "PayProfilePaymentMethod": null,
            "PayProfileTypeDef": "CreditCardInstallment_Razer",
            "StatisticsTypeDef": "CreditCardInstallment_Razer",
            "IsEnabled": true,
            "IsDefault": true,
            "Editable": true,
            "ShopInstallment": null,
            "ThirdPartyInstallment": [
                {
                    "ShopInstallmentId": 13,
                    "SupplierId": 34,
                    "ShopId": 32,
                    "IsEnabled": true,
                    "InstallmentId": 2,
                    "InstallmentDef": 3,
                    "InstallmentDefDesc": "3 期 0 利率",
                    "HasInterest": false,
                    "Rate": 0,
                    "AmtLimit": 300,
                    "BankList": [
                        {
                            "BankPayTypeId": 1,
                            "BankName": "HSBC",
                            "BankCode": "001",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 500.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 7,
                            "BankName": "Standard Chartered Bank",
                            "BankCode": "002",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 1000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 1,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 13,
                            "BankName": "AmBank",
                            "BankCode": "003",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 500.00)",
                            "CardBrands": [
                                "Visa"
                            ]
                        },
                        {
                            "BankPayTypeId": 19,
                            "BankName": "Hong Leong Bank",
                            "BankCode": "004",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 500.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 24,
                            "BankName": "Affin Bank",
                            "BankCode": "005",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 500.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 28,
                            "BankName": "RHB Bank",
                            "BankCode": "006",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 500.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        }
                    ]
                },
                {
                    "ShopInstallmentId": 14,
                    "SupplierId": 34,
                    "ShopId": 32,
                    "IsEnabled": true,
                    "InstallmentId": 3,
                    "InstallmentDef": 6,
                    "InstallmentDefDesc": "6 期 0 利率",
                    "HasInterest": false,
                    "Rate": 0,
                    "AmtLimit": 500,
                    "BankList": [
                        {
                            "BankPayTypeId": 2,
                            "BankName": "HSBC",
                            "BankCode": "001",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 500.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 8,
                            "BankName": "Standard Chartered Bank",
                            "BankCode": "002",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 1000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 1,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 14,
                            "BankName": "AmBank",
                            "BankCode": "003",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 500.00)",
                            "CardBrands": [
                                "Visa"
                            ]
                        },
                        {
                            "BankPayTypeId": 20,
                            "BankName": "Hong Leong Bank",
                            "BankCode": "004",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 500.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 25,
                            "BankName": "Affin Bank",
                            "BankCode": "005",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 500.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 29,
                            "BankName": "RHB Bank",
                            "BankCode": "006",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 500.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        }
                    ]
                },
                {
                    "ShopInstallmentId": 15,
                    "SupplierId": 34,
                    "ShopId": 32,
                    "IsEnabled": true,
                    "InstallmentId": 4,
                    "InstallmentDef": 12,
                    "InstallmentDefDesc": "12 期 0 利率",
                    "HasInterest": false,
                    "Rate": 0,
                    "AmtLimit": 500,
                    "BankList": [
                        {
                            "BankPayTypeId": 3,
                            "BankName": "HSBC",
                            "BankCode": "001",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 1000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 1,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 9,
                            "BankName": "Standard Chartered Bank",
                            "BankCode": "002",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 1000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 1,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 15,
                            "BankName": "AmBank",
                            "BankCode": "003",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 1000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 1,000.00)",
                            "CardBrands": [
                                "Visa"
                            ]
                        },
                        {
                            "BankPayTypeId": 21,
                            "BankName": "Hong Leong Bank",
                            "BankCode": "004",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 1000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 1,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 26,
                            "BankName": "Affin Bank",
                            "BankCode": "005",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 1000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 1,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 30,
                            "BankName": "RHB Bank",
                            "BankCode": "006",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 1000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 1,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        }
                    ]
                },
                {
                    "ShopInstallmentId": 16,
                    "SupplierId": 34,
                    "ShopId": 32,
                    "IsEnabled": true,
                    "InstallmentId": 5,
                    "InstallmentDef": 18,
                    "InstallmentDefDesc": "18 期 0 利率",
                    "HasInterest": false,
                    "Rate": 0,
                    "AmtLimit": 1500,
                    "BankList": [
                        {
                            "BankPayTypeId": 4,
                            "BankName": "HSBC",
                            "BankCode": "001",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 1500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 1,500.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 10,
                            "BankName": "Standard Chartered Bank",
                            "BankCode": "002",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 1500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 1,500.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 16,
                            "BankName": "AmBank",
                            "BankCode": "003",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 1500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 1,500.00)",
                            "CardBrands": [
                                "Visa"
                            ]
                        },
                        {
                            "BankPayTypeId": 22,
                            "BankName": "Hong Leong Bank",
                            "BankCode": "004",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 1500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 1,500.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 31,
                            "BankName": "RHB Bank",
                            "BankCode": "006",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 1500,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 1,500.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        }
                    ]
                },
                {
                    "ShopInstallmentId": 17,
                    "SupplierId": 34,
                    "ShopId": 32,
                    "IsEnabled": true,
                    "InstallmentId": 6,
                    "InstallmentDef": 24,
                    "InstallmentDefDesc": "24 期 0 利率",
                    "HasInterest": false,
                    "Rate": 0,
                    "AmtLimit": 500,
                    "BankList": [
                        {
                            "BankPayTypeId": 5,
                            "BankName": "HSBC",
                            "BankCode": "001",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 2000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 2,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 11,
                            "BankName": "Standard Chartered Bank",
                            "BankCode": "002",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 2000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 2,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 17,
                            "BankName": "AmBank",
                            "BankCode": "003",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 2000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 2,000.00)",
                            "CardBrands": [
                                "Visa"
                            ]
                        },
                        {
                            "BankPayTypeId": 23,
                            "BankName": "Hong Leong Bank",
                            "BankCode": "004",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 2000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 2,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 27,
                            "BankName": "Affin Bank",
                            "BankCode": "005",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 2000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 2,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 32,
                            "BankName": "RHB Bank",
                            "BankCode": "006",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 2000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 2,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        }
                    ]
                },
                {
                    "ShopInstallmentId": 18,
                    "SupplierId": 34,
                    "ShopId": 32,
                    "IsEnabled": true,
                    "InstallmentId": 7,
                    "InstallmentDef": 36,
                    "InstallmentDefDesc": "36 期 0 利率",
                    "HasInterest": false,
                    "Rate": 0,
                    "AmtLimit": 2000,
                    "BankList": [
                        {
                            "BankPayTypeId": 6,
                            "BankName": "HSBC",
                            "BankCode": "001",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 3000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 3,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 12,
                            "BankName": "Standard Chartered Bank",
                            "BankCode": "002",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 3000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 3,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        },
                        {
                            "BankPayTypeId": 18,
                            "BankName": "AmBank",
                            "BankCode": "003",
                            "IsEnabled": true,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 3000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 3,000.00)",
                            "CardBrands": [
                                "Visa"
                            ]
                        },
                        {
                            "BankPayTypeId": 33,
                            "BankName": "RHB Bank",
                            "BankCode": "006",
                            "IsEnabled": false,
                            "HasInterest": false,
                            "Rate": 0,
                            "AmtLimit": 3000,
                            "AmtLimitDesc": "(消費者單次結帳金額需大於：RM 3,000.00)",
                            "CardBrands": [
                                "Visa",
                                "MasterCard"
                            ]
                        }
                    ]
                }
            ],
            "InvoiceIssueType": "NoInvoiceRequired",
            "CanUseForeignCard": null
        },
        {
            "SupplierId": 34,
            "ShopId": 32,
            "TypeDef": "OnlineBanking_Razer",
            "TypeDefDesc": "Razer - 網路銀行",
            "PayProfilePaymentMethod": null,
            "PayProfileTypeDef": "OnlineBanking_Razer",
            "StatisticsTypeDef": "OnlineBanking_Razer",
            "IsEnabled": true,
            "IsDefault": true,
            "Editable": false,
            "ShopInstallment": null,
            "ThirdPartyInstallment": null,
            "InvoiceIssueType": "NoInvoiceRequired",
            "CanUseForeignCard": null
        },
        {
            "SupplierId": 34,
            "ShopId": 32,
            "TypeDef": "TNG_Razer",
            "TypeDefDesc": "Razer - Touch 'n Go",
            "PayProfilePaymentMethod": null,
            "PayProfileTypeDef": "TNG_Razer",
            "StatisticsTypeDef": "TNG_Razer",
            "IsEnabled": true,
            "IsDefault": true,
            "Editable": true,
            "ShopInstallment": null,
            "ThirdPartyInstallment": null,
            "InvoiceIssueType": "NoInvoiceRequired",
            "CanUseForeignCard": null
        },
        {
            "SupplierId": 34,
            "ShopId": 32,
            "TypeDef": "Boost_Razer",
            "TypeDefDesc": "Razer - Boost",
            "PayProfilePaymentMethod": null,
            "PayProfileTypeDef": "Boost_Razer",
            "StatisticsTypeDef": "Boost_Razer",
            "IsEnabled": true,
            "IsDefault": true,
            "Editable": true,
            "ShopInstallment": null,
            "ThirdPartyInstallment": null,
            "InvoiceIssueType": "NoInvoiceRequired",
            "CanUseForeignCard": null
        },
        {
            "SupplierId": 34,
            "ShopId": 32,
            "TypeDef": "GrabPay_Razer",
            "TypeDefDesc": "Razer - GrabPay",
            "PayProfilePaymentMethod": null,
            "PayProfileTypeDef": "GrabPay_Razer",
            "StatisticsTypeDef": "GrabPay_Razer",
            "IsEnabled": true,
            "IsDefault": true,
            "Editable": true,
            "ShopInstallment": null,
            "ThirdPartyInstallment": null,
            "InvoiceIssueType": "NoInvoiceRequired",
            "CanUseForeignCard": null
        }
    ],
    "ErrorMessage": null,
    "TimeStamp": "2025-11-12T15:24:16.1126577+08:00"
}
```

## 付款設定頁 savepaytype

https://sms.qa1.my.91dev.tw/Api/Shop/SavePayType



他會 leftjoin shippingpaytype / shoppaytype, right 沒東西是 null 就會 null reference

```csharp
public List<string> MergeSupplierPayType(List<SupplierPayType> table, int supplierId)
{
    var original = this.GetSupplierPayType(supplierId).ToList();
    //// Sync (比對後異動資料)
    var left = from input in table
                join db in original on new { SupplierPayType_SupplierId = input.SupplierPayType_SupplierId, SupplierPayType_TypeDef = input.SupplierPayType_TypeDef }
                equals new { db.SupplierPayType_SupplierId, db.SupplierPayType_TypeDef } into g
                from l in g.DefaultIfEmpty(new SupplierPayType { SupplierPayType_Id = 0 })
                select new
                {
                    Id = l.SupplierPayType_Id,
                    TypeDef = input.SupplierPayType_TypeDef,
                    SupplierId = input.SupplierPayType_SupplierId,
                    IsDefault = input.SupplierPayType_IsDefault,
                    IsEnabled = input.SupplierPayType_IsEnabled,
                    CreatedDateTime = l.SupplierPayType_CreatedDateTime,
                    CreatedUser = l.SupplierPayType_CreatedUser,
                    RowVersion = l.SupplierPayType_Rowversion
                };
    var right = from db in original
                join input in table on new { SupplierPayType_SupplierId = db.SupplierPayType_SupplierId, SupplierPayType_TypeDef = db.SupplierPayType_TypeDef }
                equals new { input.SupplierPayType_SupplierId, input.SupplierPayType_TypeDef } into g
                from r in g.DefaultIfEmpty()
                select new
                {
                    Id = db.SupplierPayType_Id,
                    TypeDef = r.SupplierPayType_TypeDef,
                    SupplierId = r.SupplierPayType_SupplierId,
                    IsDefault = r.SupplierPayType_IsDefault,
                    IsEnabled = r.SupplierPayType_IsEnabled,
                    CreatedDateTime = db.SupplierPayType_CreatedDateTime,
                    CreatedUser = db.SupplierPayType_CreatedUser,
                    RowVersion = db.SupplierPayType_Rowversion
                };
```



IsEditShoppingNoticePaymentContentEnabled => SG 沒定義會是 true, 就會去更新付款方式說明

但因為 razer 沒特別定義好像不會更新

_webStoreDbContext.ShoppingNotice


removedSupplierPayTypes 讀 isEnabled

要執行
RemoveShopPayTypeProcess

csp_RemoveSalePagePayTypeAndRelatedShippingType









## 檢查贈品


https://sms.qa1.my.91dev.tw/Api/SaleProduct/CheckIsPromotionFreeGift

{
    "Id": 80,
    "Value": [
        6086
    ]
}

```sql
select *
from SaleProduct(nolock)
where SaleProduct_ValidFlag = 1
and SaleProduct_ShopId = 80
and SaleProduct_Id = 6058

update SaleProduct
set SaleProduct_IsGift = 1
where SaleProduct_ValidFlag = 1
and SaleProduct_ShopId = 80
and SaleProduct_Id = 6058

select *
from SaleProduct(nolock)
where SaleProduct_ValidFlag = 1
and SaleProduct_ShopId = 80
and SaleProduct_Id = 6058
```


優惠碼活動 = 993
滿額贈 = 992



## 商品頁看不到, 結帳看不到任何付款

非購物商店設定開關
IsNoneShoppingStore

```JSON
QA
{
    "80": {
        "isNoneShoppingStore": true
    }
}
Prod
{
    "200130": {
        "isNoneShoppingStore": true
    },
    "200131": {
        "isNoneShoppingStore": true
    }
}
```


<div class="collapse-group" ng-hide="SalePageIndexCtrl.IsCompactSalePage || SalePageIndexCtrl.IsNoneShoppingStore || SalePageIndexCtrl.IsInStoreOnlySalepage">


C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\MobileWebMallV2\TypeScripts\Modules\SalePage\SalePageIndexController.ts
this.IsNoneShoppingStore = this.CustomSettingUtility.getCustomSetting('isNoneShoppingStore') || false;


public getCustomSetting(settingName: string) {
            const env = window["nineyi"] && window["nineyi"].env.toLowerCase() || undefined;
            const market = this.Market.toLowerCase();
            const shopId = this.ShopId;

            return this.CustomSetting.getSetting(env, market, shopId, settingName);
        }



## shopID 改到 83



## 商品頁全部套用失效??

```json
{
    "ShopId": 83,
    "PayType": "GrabPay_Razer",
    "IsUpdateMajor": true,
    "IsUpdateGift": false
}
```

https://sms.qa1.my.91dev.tw/Api/Shop/ApplyShopPayType



## 新增商品套不了!!


付款方式都必須支援信用卡一次付款


https://sms.qa1.my.91dev.tw/api/SalePage/CreateSalePage

{
    "UniqueKey": "7c96ab16-0ba0-4ae5-aa55-e9443e074867",
    "SalePage": {
        "ShopId": 83,
        "CategoryId": 0,
        "ShopCategoryId": 457,
        "Title": "看grabpay 能不能套到",
        "SellingStartDateTime": "2025-11-14T09:02:33.000Z",
        "SellingEndDateTime": "2026-11-14T09:02:33.000Z",
        "ApplyType": "一般",
        "ExpectShippingDate": "2025-11-17T09:02:34.770Z",
        "ShippingPrepareDay": 3,
        "ShippingTypes": [
            508,
            513,
            514
        ],
        "PayTypes": [
            "CreditCardOnce_Razer"
        ],
        "Brand": "",
        "Type": "",
        "Specifications": {},
        "ProductHighlight": "",
        "ProductDescription": "",
        "SalePageMoreInfoVideo": {
            "SalePageVideoType": 1,
            "SalePageVideoPositionType": 0,
            "VideoUrl": ""
        },
        "MoreInfo": "",
        "HasSku": false,
        "SkuList": [],
        "SuggestPrice": 30,
        "Price": 30,
        "Cost": 10,
        "OnceQty": 1,
        "SafetyStockQty": 50,
        "Qty": 5000,
        "OuterId": "",
        "DistributionCode": "",
        "ShowCategoryLevel": [],
        "ShowShopCategoryLevel": [],
        "MainImageList": [],
        "SalePageMainImageVideo": {
            "SalePageVideoType": 0,
            "SalePageVideoPositionType": 0,
            "VideoUrl": ""
        },
        "SkuImageList": [],
        "SeoTitle": "",
        "SeoKeywords": "",
        "SeoDescription": "",
        "IsClosed": false,
        "IsShowTradesOrderList": false,
        "IsShowSoldQty": false,
        "TemperatureTypeDef": "Normal",
        "Length": 1,
        "Width": 1,
        "Height": 1,
        "Weight": 1,
        "Status": "Normal",
        "IsSeparated": false,
        "DeliveryPeriod": "",
        "ShippingTypeDef": null,
        "IsLimitedPickupPeriodDays": false,
        "PickupPeriodDays": 0,
        "CartonQty": 0,
        "IsInvoiceByCartonQty": false,
        "IsShowStockQty": false,
        "TaxTypeDef": "Taxable",
        "IsReturnable": true,
        "HasPointsPayPairs": false,
        "PointsPayPairsList": [],
        "IsEnableBookingPickupDate": false,
        "PrepareDays": null,
        "AvailablePickupDays": null,
        "AvailablePickupStartDateTime": null,
        "AvailablePickupEndDateTime": null,
        "SalePageMLItems": [
            {
                "LangId": "en-US",
                "Title": "",
                "SEOTitle": "",
                "SEOKeywords": "",
                "SEODescription": ""
            },
            {
                "LangId": "ms-MY",
                "Title": "",
                "SEOTitle": "",
                "SEOKeywords": "",
                "SEODescription": ""
            },
            {
                "LangId": "zh-CN",
                "Title": "",
                "SEOTitle": "",
                "SEOKeywords": "",
                "SEODescription": ""
            }
        ],
        "SaleProductSkuMLItems": [],
        "SaleProductDescMLItems": [
            {
                "LangId": "en-US",
                "SubDescript": "",
                "ShortContent": "",
                "Content": ""
            },
            {
                "LangId": "ms-MY",
                "SubDescript": "",
                "ShortContent": "",
                "Content": ""
            },
            {
                "LangId": "zh-CN",
                "SubDescript": "",
                "ShortContent": "",
                "Content": ""
            }
        ],
        "SalesModeTypeDef": 1,
        "SoldOutActionType": "OutOfStock",
        "ProductTypeDef": "Physical",
        "VirtualProductProviderTypeDef": "",
        "ParentIdList": [],
        "IsRestricted": false,
        "SalePageSpecChartId": null,
        "SalepageBundle": null,
        "SalesTypeDef": "Normal",
        "SalesChannel": [
            "Online"
        ]
    },
    "MainImageList": [
        {
            "Type": 2,
            "Operation": 1,
            "FileName": "napkin",
            "Group": "SalePage",
            "Key": "7c96ab16-0ba0-4ae5-aa55-e9443e074867"
        }
    ],
    "SkuImageList": [],
    "VideoList": [],
    "TagValues": [],
    "Brands": [],
    "SalePageSpecChartId": null,
    "SalepageBundle": null
}



最後會打 scmapiv2 :
/v2/Salepage/CreateMain


C:\91APP\NineYi.Scm.ApiV2\BusinessLogic\Services\PaymentLogistics\PaymentLogisticsService.cs

return Translations.Backend.Validator.CreateRequest.PayTypesNeedCreditOnce;


結果是預設有錯

use WebStoreDB


select *
from ShopPayShippingDefault(nolock)
where ShopPayShippingDefault_ValidFlag =1
and ShopPayShippingDefault_ShopId = 83




##　前台折扣活動看到奇怪ss

DiscountReachPriceWithFreeGift

C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\MobileWebMallV2\ClientApp\src\promotion\components\promotionEngineDetail\promotionCard.tsx

C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\MobileWebMallV2\ClientApp\src\common\components\currencyWord\currencyPreferred.tsx

{
    //// 單階
    var firstLevel = this._rule.ReachPriceFreeGiftPairs.OrderBy(x => x.ReachPrice).First();
    rule.Append(StringUtility.PeacefulFormat(Translation.Backend.Service.PromotionEngineDiscountReachPriceWithFreegift.ReachPriceGetGift, this._currencyService.ToCurrency(this._context.ShopId, firstLevel.ReachPrice), firstLevel.Gift.Qty));
}