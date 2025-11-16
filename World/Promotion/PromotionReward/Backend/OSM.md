
## 📋 目錄
1. [活動類型](#活動類型)
2. [APIs](#apis)
3. [Mapper](#mapper)

---

## 活動類型

- 新版給點活動類型：RewardReachPriceWithPoint2, RewardReachPriceWithRatePoint2
- 新版給券活動類型：RewardReachPriceWithCoupon

## APIs


**COPY**
`https://store.91app.hk/Api/PromotionEngine/GetPromotionEngineReplicant`
`https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/GetPromotionEngineReplicant`

**CREATE**
`https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/Create`

**DELETE**
`https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/DeletePromotionRewardPoint`

**GET**
新版 API `https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/GetPromotionEngineDetail?promotionEngineId=5914&promotionEngineTypeDef=RewardReachPriceWithRatePoint2&shopId=2`
舊版 API `https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/GetPromotionRewardPointDetail?promotionEngineId=5915`

**UPDATE**
update `https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/UpdatePromotionEngine`
舊update `https://sms.qa1.hk.91dev.tw/Api/PromotionEngine/UpdatePromotionRewardPoint`


## Mapper

```csharp
this.CreateMap<GetPromotionResponseEntity, PromotionEngineUpdateEntity>()
this.CreateMap<PromotionEngineUpdateEntity, PromotionEngineCreateEntity>()
```