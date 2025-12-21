## APIs


`https://store.91app.hk/Api/PromotionEngine/GetPromotionEngineReplicant`
`https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/GetPromotionEngineReplicant`

`https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/Create`

`https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/DeletePromotionRewardPoint`

新版 API `https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/GetPromotionEngineDetail?promotionEngineId=5914&promotionEngineTypeDef=RewardReachPriceWithRatePoint2&shopId=2`
舊版 API `https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/GetPromotionRewardPointDetail?promotionEngineId=5915`

update `https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/UpdatePromotionEngine`
舊update `https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/UpdatePromotionRewardPoint`

<br>
<br>

## Mapper

```csharp
this.CreateMap<GetPromotionResponseEntity, PromotionEngineUpdateEntity>()
this.CreateMap<PromotionEngineUpdateEntity, PromotionEngineCreateEntity>()
```