


DoPayProcessFlowProcessor.cs => ConvertMemberCartToWebStoreGetCart


ConvertMemberCartToWebStoreGetCart => MappingOldCartSelectedEntity


cartEntity.SelectedCheckoutPayTypeGroup = selectedPay 不見??


GetCartAndCalculateProcessor 可能有處理上的問題???


帶入優惠碼就會 null reference


看redis

SelectedCheckoutPayTypeGroup

{
    "DisplayName": "信用卡一次付款",
    "Description": "",
    "Declaration": "",
    "StatisticsTypeDef": "CreditCardOnce_Razer",
    "InstallmentType": null,
    "PayTypeList": [
        {
            "PayProfileTypeDef": "CreditCardOnce_Razer",
            "Name": "還沒時間打字",
            "StatisticsTypeDef": "CreditCardOnce_Razer",
            "SupportedCardBrandList": [
                "Visa",
                "MasterCard"
            ]
        }
    ],
    "PayPromotionDisplayWording": null,
    "PayPromotionStartDateTime": null,
    "PayPromotionEndDateTime": null,
    "LimitedBanks": [],
    "DisplayIconUrl": "",
    "AdditionalInfo": null
}



CalculatePromoCodeProcessor



特定商品頁問題???


{service="my-qa-cart-service"}
|json
|= `0HNGMGUCR1HSK:00000016`
|json
| line_format "{{._msg}}"


大於某個金額套用優惠碼會死掉!!


redis 有個 key : PromoCodeDispatch 內容


{
    "DispatchStatus": 2,
    "DispatchStatusData": null,
    "PromoCode": "allen",
    "PromoCodeType": "PromoCodeKOL",
    "PromoCodeInfo": {
        "PromoCodeType": "PromoCodeKOL",
        "PromoCode": "allen",
        "PromotionEngineId": 971,
        "PromoCodePoolGroupId": null,
        "PaymentTypes": [
            "CreditCardInstallment_Razer"
        ],
        "IsUseFirstPurchasePromotion": false,
        "DispatchStatus": 2,
        "DispatchStatusData": null,
        "DiscountPercentage": null,
        "DiscountAmount": 1,
        "TargetTypeDef": "Shop",
        "MaxDiscountAmount": null,
        "MaxDiscountPercentage": 0.5
    }
}





context.Data.CheckoutType.PayTypeList


已知 promoCodeInfo.PaymentTypes 有分期razer => 





shoppingCart.CheckoutType.DisplayPayTypeList 要有資料需要 matchedInstallmentList 有資料




 context.Data.CheckoutType.PayTypeList ???





 selectedPay = cartEntity.SelectedCheckoutPayTypeGroup why null???