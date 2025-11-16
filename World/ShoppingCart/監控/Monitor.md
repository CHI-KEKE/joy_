
## ShoppingCart

**ShoppingCart Loki Log**：https://monitoring-dashboard.91app.io/d/3dSbCsL4k/shoppingcart-loki-log?orgId=2&refresh=30s&var-MarketENV=TW-Prod&var-Service=prod-promotion-service&var-Message=&var-Class=&var-RequestPath=%2Fapi%2Fpromotion-rules%2Fsalepage-update&var-RequestId=&var-Level=Error&var-Loki=ZIOlfD44k&var-Cluster=hxdP8t7Vz&var-tid=&var-ExceptionType=System.ArgumentNullException&var-Source=&var-ErrorCode=&from=now-3h&to=now

![alt text](./Img/image-12.png)

<br>

**篩選方式範例**：
- Service：`prod-cart-service`
- Level：`Error`
- ExceptionType：`System.ArgumentException`

![alt text](./Img/image-14.png)



## 13. Shopping Service Alert 完整監控面板

**監控中心 URL**：
```
https://monitoring-dashboard.91app.io/d/aen3tgg0mmvpcd/shopping-service-alert?orgId=2&from=now-6h&to=now&timezone=Asia%2FTaipei&var-MarketENV=TW-Prod&var-Loki=ZIOlfD44k&var-Cluster=hxdP8t7Vz&var-Namespace=prod-shopping-service&var-Sandbox_Namespace=sandbox-api-gateway&var-CacheClusterID=backend-redis-2-001&var-CloudWatch=kYZD-B7Vk&var-LOG_CONTAIN_STRING=&var-topk_1_node=ip-10-2-218-109.ap-northeast-1.compute.internal&var-Quey_Taints=sg&var-Service_Catalog=appgen
```



## HK Cart Service Exception Alert  (5分間隔-往前看10分鐘數據)

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


## lentency



{service="prod-shopping-service"}
|=`Nine1HttpLog`
|json
| line_format "{{._msg}}"
| json
| TimeTaken > 24000
| line_format "{{.UriStem}} {{.TimeTaken}}"