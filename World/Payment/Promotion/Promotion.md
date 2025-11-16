


## 折扣活動指定金流 (滿額贈) DiscountReachPriceWithFreeGift

https://sms.qa1.my.91dev.tw/CommerceCloud/PromotionEngine/Create?shopId=32&type=DiscountReachPriceWithFreeGift




https://sms.qa1.my.91dev.tw/Api/Shop/GetShopPayProfiles?shopId=32



https://91appinc.visualstudio.com/G11n/_workitems/edit/537336


目前邏輯是 : 依照字母排序 (全市場)

台灣現況

分期付款 CreditCardInstallment > 一次付清 CreditCardOnce

MY_RAZER 相同

分期付款 CreditCardInstallment_Razer > 次付清 CreditCardOnce_Razer

promotionId : 970


活動明細
購物車



### 多語系問題

PromotionEngineSetting_PayProfileTypeDef

多語系 key
backend.definition.PayProfile

detail redis cache key

Cache:QA:WebAPI:MY:PromotionEngine:GetDetail-2021060917:32-970-0-Web-0-zh-TW

程式組出來的 : PayProfile_TypeDef_CreditCardInstallment_Razer 符合預期 問題是他怎麼會找不到 key 值


已確認 清除 redisCache, webapi fetch, r=t, ctrl + f5

需確認
s3 是否有資料
	"PayProfile_TypeDef_CreditCardInstallment_Razer": "信用卡分期付款" => 有!
是否需要上code => 理論上不用 他是從client抓的
debug url
https://moiicrm.shop.qa1.my.91dev.tw/webapi/translations/debug/backend.definition.PayProfile.PayProfile_TypeDef_CreditCardInstallment_Razer/en-US
"TextFromFetch": "#ERROR: Translation file or key was not found.", ==> why!!!?????
"TextFromRemote": "Credit card installments" (edited) 


==> 也就是為甚麼 fetch 會失效

確認redis cache

Translation:QA1MY:i18n:NineYi.WebStore.MallAndApi:i18n.manifest

刪除對應 Module 的 ETag => 重新 fetch 後就成功了



## 優惠碼活動 PromoCodeKOLReachPriceAmount


活動明細頁

DiscountReachPriceWithFreeGift

PromoCodeKOLReachPriceAmount
優惠碼活動前台列表看不到 
```csharp
        /// <summary>
        /// Mappings 成 VisiblePlatformTypeList
        /// </summary>
        /// <param name="channelTypeList">給點通路</param>
        /// <remarks>
        /// 為了餵進 GetPromotionEngineRule，需做型別轉換
        /// </remarks>
        /// <returns>VisiblePlatformTypeList</returns>
        private List<PromotionEnginePlatformTypeEntity> MappingToVisiblePlatformTypeList(List<string> channelTypeList)
        {
            //// 因商業邏輯考量，因為是優惠碼推薦活動，在購物流程才輸入 PromoCode 中活動
            //// 故在前台裝置 Web/AppAndroid/AppIOS 都不顯示，否則會出現在前台活動列表中顯示
            var result = new List<PromotionEnginePlatformTypeEntity>()
            {
                new PromotionEnginePlatformTypeEntity { Platform = nameof(PromotionEnginePlatformTypeEnum.Web), IsEnabled = false },
                new PromotionEnginePlatformTypeEntity { Platform = nameof(PromotionEnginePlatformTypeEnum.AppAndroid), IsEnabled = false },
                new PromotionEnginePlatformTypeEntity { Platform = nameof(PromotionEnginePlatformTypeEnum.AppIOS), IsEnabled = false }
            };

            return result;
        }
```

明細也會濾除
```csharp
//// 由於優惠碼活動、單品特價、新活動給點(券)不可查閱活動明細，所以直接回應查無活動。
if (PromotionEngineTypeEnum.AllPromoCode.HasFlag(promotion.PromotionEngineTypeDefEnum)
|| promotion.PromotionEngineTypeDefEnum == PromotionEngineTypeEnum.DiscountByMemberWithPrice
|| promotion.PromotionEngineTypeDefEnum == PromotionEngineTypeEnum.RestrictedPurchasesByMember
|| promotion.PromotionEngineTypeDefEnum == PromotionEngineTypeEnum.RewardReachPriceWithPoint2
|| promotion.PromotionEngineTypeDefEnum == PromotionEngineTypeEnum.RewardReachPriceWithRatePoint2
|| promotion.PromotionEngineTypeDefEnum == PromotionEngineTypeEnum.RewardReachPriceWithCoupon)
{
    throw new PromotionEngineException(PromotionEngineExceptionTypeEnum.PromotionNotFound);
}

```


PromotionEngineSetting_IsWebVisible
1




```sql
use WebStoreDB


select PromotionEngine_TypeDef,PromotionEngine_DisplaySetting,PromotionEngineSetting_IsWebVisible,*
from PromotionEngine(nolock)
inner join PromotionEngineSetting(nolock)
on PromotionEngineSetting_PromotionEngineId = PromotionEngine_Id
where PromotionEngine_ValidFlag = 1
and PromotionEngine_TypeDef in ('PromoCodeKOLReachPriceAmount','DiscountReachPriceWithFreeGift')
and PromotionEngine_ShopId = 32
```



## 指定金流折價

https://91appinc.visualstudio.com/G11n/_workitems/edit/548268