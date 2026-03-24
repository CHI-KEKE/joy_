## SavePayType - 儲存要套用的金流


用前端傳入的付款方式清單（table），去跟資料庫目前的 supplier 付款設定（original）做差異比對，然後：

新的 → Insert
原本有、還啟用 → Update
原本有、現在關掉 → Delete
最後回傳「這次被移除的付款方式 TypeDef 清單」。
DB 有，但前端沒送



#### 儲存按鈕 會吃前端 config

WebSite/WebSite/TypeScripts/Configs/localization.config.ts


#### Success Response

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
                ...
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


#### 錯誤

```bash
The requested service 'PayNow_Razer (NineYi.Sms.BL.Services.ThirdPartyServices.IThirdPartyServiceSettingService)' has not been registered. To avoid this exception, either register a component to provide the service, check for service registration using IsRegistered(), or use the ResolveOptional() method to resolve an optional dependency.
```

要註冊 IThirdPartyServiceSettingService


## API

https://sms.qa1.my.91dev.tw/Api/Shop/SavePayType


#### JOIN Logic

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


#### ShoppingNotice

- IsEditShoppingNoticePaymentContentEnabled => SG 沒定義會是 true, 就會去更新付款方式說明但因為 razer 沒特別定義好像不會更新 _webStoreDbContext.ShoppingNotice

#### removedSupplierPayTypes

- removedSupplierPayTypes 讀 isEnabled
- 要執行 RemoveShopPayTypeProcess
- csp_RemoveSalePagePayTypeAndRelatedShippingType



## 關鍵語法


```csharp
//// 新增篩除 supplierPaytype
removedSupplierPayTypes = this._payShippingTypeRepository.MergeSupplierPayType(supplierPayType, supplierId);

//// 新增篩除 shopPayype
this._payShippingTypeRepository.MergeShopPayType(shopPayType, shop.ShopId);
```

## 怎樣可以 Editable

Editable=true 只要符合其中之一：

1. StoreCredit


Editable 一定是 true

2. 非 StoreCredit

如果 defaultPayTypes.Any(i => i.ShopPayShippingDefault_PayProfileTypeDef.Contains(p.PayProfile_TypeDef)) 為 true → Editable = false
否則 → Editable = true


只要這個 PayProfile TypeDef 沒出現在任何一筆 defaultPayTypes 的 “ShopPayShippingDefault_PayProfileTypeDef” 裡（Contains 判定）→ Editable = true