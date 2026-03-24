
## 語系

QA:MY:SMS:Utility:Definition:WebStore:PayProfile-zh-TW => 清
Translation:QA:i18n:NineYi.Sms:backend.definition.PayProfile:zh-TW.json => 看
多語系有跟 DEFINITION 有關的都要清理一次!



## GetPayType - 顯示有哪些可以套用


#### API

https://sms.qa1.my.91dev.tw/Api/Shop/GetPayType

```json
[
    {
        "SupplierId": 79,
        "ShopId": 83,
        "TypeDef": "CreditCardOnce_Razer",
        "TypeDefDesc": "Razer - 信用卡一次付清",
        "PayProfilePaymentMethod": null,
        "PayProfileTypeDef": "CreditCardOnce_Razer",
        "StatisticsTypeDef": "CreditCardOnce_Razer",
        "IsEnabled": true,
        "IsDefault": true,
        "Editable": false,
        "ShopInstallment": null,
        "ThirdPartyInstallment": null,
        "InvoiceIssueType": "Issue",
        "CanUseForeignCard": null,
        "PaymentProcessingMethodDef": "ThroughPSP"
    },
    {
        "SupplierId": 79,
        "ShopId": 83,
        "TypeDef": "GrabPay_Razer",
        "TypeDefDesc": "Razer - GrabPay",
        "PayProfilePaymentMethod": null,
        "PayProfileTypeDef": "GrabPay_Razer",
        "StatisticsTypeDef": "GrabPay_Razer",
        "IsEnabled": true,
        "IsDefault": false,
        "Editable": true,
        "ShopInstallment": null,
        "ThirdPartyInstallment": null,
        "InvoiceIssueType": "Issue",
        "CanUseForeignCard": null,
        "PaymentProcessingMethodDef": "ThroughPSP"
    }
]
```


#### DB

- ShopPayShippingDefault 找商店預設金流
- supplier 比對 currency

```csharp
var payTypes = this._payShippingTypeRepository.GetSupplierPayType(supplierId).ToList();
```

- csp 會比對 Currency csp_GetPayProfile

```csharp
var profiles = this._payShippingTypeRepository.GetPayProfile(shopId);
```
```sql
	-- 取得廠商序號和客戶使用幣別
	SELECT @supplierId = Shop_SupplierId
		 , @shopTypeDef = Shop_TypeDef
		 , @supplierCurrency = Supplier_SalesCurrency
	FROM [ERPDB].[dbo].[Shop] WITH (NOLOCK)
	INNER JOIN [ERPDB].[dbo].[Supplier] WITH (NOLOCK) 
	  ON Shop_SupplierId = Supplier_Id
	WHERE Shop_Id = @ShopId

    -- 取得可使用的物流設定檔
    INSERT INTO #tmpPayProfileData
    SELECT ROW_NUMBER() OVER(ORDER BY [PayProfile_Id]) AS PayProfile_Num
		  ,[PayProfile_TypeDef]
		  ,[PayProfile_StatisticsTypeDef]
		  ,[PayProfile_SupplierStoreProfileDistributorDef]
		  ,[PayProfile_HasCvsContract]
		  ,[PayProfile_DisplaySort]
		  ,[PayProfile_IsCheckEnabledSetting]
		  ,[PayProfile_Currency] 
	FROM [ERPDB].[dbo].[PayProfile] WITH (NOLOCK)
	CROSS APPLY OPENJSON(PayProfile_Currency,'$')
	WHERE [PayProfile_ValidFlag] = 1
	AND Value = @supplierCurrency
	ORDER BY [PayProfile_DisplaySort]
```