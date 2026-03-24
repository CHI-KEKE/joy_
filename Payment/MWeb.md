## 關鍵字

RegisterThirdPartyProcess
TradesOrderPaymentService
PaymentMiddlewareService
PayChannelHelper
QFPayPayChannelService
StripePayChannelService.cs
RegisterThirdPartyFinishProcess 付款後轉倒流程

頁面轉導
C:\91APP\NineYi.WebStore.MobileWebMall\WebStore\Frontend\MobileWebMallV2\Controllers\PayChannelController.cs => PayChannelReturn

GetSalePageV2ProcessContext
this.GetProcessorList<ISalePageV2Processor>("GetSalePageV2EntityProcess");
GetSalePageDataProcessor
GetPayTypeProcessor
GetPayTypeDescriptionProcessor


ThirdPartyFinishProcess
ThirdPartyProcess

FinishPayment



## 哪裡建立 TradesOrderThirdPartyPayment

Mapper.CreateMap<TradesOrderGroup, TradesOrderThirdPartyPayment>()
    .ForMember(i => i.TradesOrderThirdPartyPayment_ShopId, s => s.MapFrom(i => i.TradesOrderGroup_ShopId))
    .ForMember(i => i.TradesOrderThirdPartyPayment_TradesOrderGroupCode, s => s.MapFrom(i => i.TradesOrderGroup_Code))
    .ForMember(i => i.TradesOrderThirdPartyPayment_DateTime, s => s.MapFrom(i => i.TradesOrderGroup_DateTime))
    .ForMember(i => i.TradesOrderThirdPartyPayment_TotalPayment, s => s.MapFrom(i => i.TradesOrderGroup_TotalPayment))
    .ForMember(i => i.TradesOrderThirdPartyPayment_StatusDef, s => s.MapFrom(i => "New"))
    .ForMember(i => i.TradesOrderThirdPartyPayment_StatusUpdatedDateTime, s => s.MapFrom(i => i.TradesOrderGroup_DateTime));


C:\91APP\nineyi.webstore.mobilewebmall\WebStore\DA\WebStoreDBV2\Mappers\TradesOrderThirdPartyPaymentEntityMappingProfile.cs


tradesordergroup 與 tradesorderthirdpartypayment 關聯起來產生資料所以應該會是同時的



這裡會看到同時

```sql
    use WebStoreDB

select TradesOrderThirdPartyPayment_DateTime,TradesOrderGroup_DateTime,*
from TradesOrderThirdPartyPayment(nolock)
inner join TradesOrderGroup(nolock)
on TradesOrderThirdPartyPayment_TradesOrderGroupCode = TradesOrderGroup_Code
where TradesOrderThirdPartyPayment_ValidFlag = 1
```