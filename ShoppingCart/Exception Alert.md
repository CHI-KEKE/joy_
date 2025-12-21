## channelType ="" 空


https://91app.slack.com/archives/C3DB30C3T/p1762234699459089

原始錯誤

Response Data: {"type":"https://tools.ietf.org/html/rfc9110#section-15.5.1","title":"One or more validation errors occurred.","status":400,"errors":{"$.Channel":["The JSON value could not be converted to NineYi.Msa.Promotion.Engine.SalesChannelEnum. Path: $.Channel | LineNumber: 0 | BytePositionInLine: 314."]},"traceId":"00-e0dd204380a103a6f3c38f31cd4a4df4-9de24bf704e61246-00"}


帶著 requestid 0HNGLP3BRGDO4:0000016B查

{service="prod-cart-service"}
|json
|= `0HNGLP3BRGDO4:0000016B`
|json
| line_format "{{._msg}}"

channelType ="" 空

```json
Request Payload: "{\"Shop\":{\"Id\":76,\"Tags\":[\"EnableAddOns\"]},\"User\":{\"Id\":\"0\",\"Tags\":[\"AllUserScope\",\"CrmShopMemberCard:240\"],\"OuterId\":\"\",\"ShopMemberCode\":\"\"},\"Shipping\":{\"ShippingProfileTypeDef\":\"Home\",\"CountryProfileId\":0,\"LocationId\":0},\"Payment\":{\"PayProfileTypeDef\":\"CreditCardOnce_Cybersource\",\"MultiPayItems\":[]},\"Channel\":\"\",\"CurrencyDecimalDigits\":2,\"SalepageSkuList\":[{\"SalepageId\":414291,\"SkuId\":3559351,\"Price\":399.00,\"Qty\":6,\"Flags\":[],\"OptionalTypeDef\":\"\",\"OptionalTypeId\":0,\"OuterId\":\"399025010220\",\"SuggestPrice\":699.00,\"CartExtendInfoItemGroup\":0,\"CartExtendInfoItemType\":\"Major\",\"CartExtendInfos\":[],\"CartId\":26374200},{\"SalepageId\":536537,\"SkuId\":3910055,\"Price\":459.00,\"Qty\":1,\"Flags\":[],\"OptionalTypeDef\":\"\",\"OptionalTypeId\":0,\"OuterId\":\"377903190210\",\"SuggestPrice\":559.00,\"CartExtendInfoItemGroup\":0,\"CartExtendInfoItemType\":\"Major\",\"CartExtendInfos\":[],\"CartId\":26374212}],\"FeeList\":[{\"Id\":688,\"Type\":\"ShippingFee\",\"Price\":30.00,\"ExtendInfo\":{\"ShippingProfileTypeDef\":\"Home\",\"IsDomesticWeightPricing\":false,\"TemperatureTypeDef\":\"Normal\",\"ShippingType\":\"688\",\"IsLocal\":false}}],\"Promotion\":{\"SelectedDesignatePaymentPromotionId\":0},\"CouponSetting\":{\"MultipleRedeem\":{\"Discount\":{\"IsMultiple\":false,\"Qty\":1},\"Gift\":{\"IsMultiple\":true,\"Qty\":9999},\"Shipping\":{\"IsMultiple\":false,\"Qty\":1}}},\"CouponList\":[],\"Options\":{\"IsVerbose\":false,\"IsCouponPreSelect\":true}}"
```