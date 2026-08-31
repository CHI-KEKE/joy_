


C:\91APP\nineyi.webstore.mobilewebmall\WebStore\Frontend\BLV2\PayProcesses\Processors\CreateRegularOrderProcessor.cs

往上一層 GetProcessContextForRegularOrderPay =>

            payProcessContext.RegularOrderProvider = shoppingCart.RegularOrderProvider;


CreateRegularOrderProcessor => 這是付款完成後會做的事情








C:\91APP\Cart\cart2\nine1.cart\src\BusinessLogic\Nine1.Cart.BL.Services\Processor\Checkout\Complete\MergeRequestAndOldPayProcessContextProcessor.cs



考慮用

C:\91APP\Cart\cart2\nine1.cart\src\BusinessLogic\Nine1.Cart.BL.BE\CartProcessor\OldCart\ShoppingCartClientEntity.cs

判斷


                return this.SalePageGroupList.SelectMany(i => i.SalePageList).Any(i => i.SaleProductShippingTypeDef == SaleProductShippingTypeEnum.RegularOrder.ToString());



## 先來測試排成


MG260617M00001
MS260617M000001

memberid = 99000133
sku = 165
rgo_ship_period = 7
rgo_ship_count = 1
rgo_next_ship_date = 2026-06-26
rgo_ship_date = 2026-06-19


## 預期 init 資料

var firstOrderDate = context.SalesOrderItem.OrderDateTime ?? now;
var shipDate       = DateOnly.FromDateTime(firstOrderDate.AddDays(GenerateNewOrderDiffDays));
var nextShipDate   = shipDate.AddDays(context.RegularOrderData.OrderFrequency);




#### createorder 要撈到並且塞detail


where spi.SpiEnableRegular == true
&& spi.SpiRegularProvider == Nine1V2Provider
&& rgo.RgoStatus == 1
&& rgo.RgoInSchedule == false
&& rgo.RgoShipPeriod > 0
&& (rgo.RgoPauseDate == null || rgo.RgoNextShipDate < rgo.RgoPauseDate)
&& (rgo.RgoShipLimitCount == 0 || rgo.RgoShipCount < rgo.RgoShipLimitCount)
&& rgo.RgoNextShipDate == targetDate

重點:


rgo.RgoStatus == 1
rgo.RgoInSchedule == false
&& rgo.RgoShipPeriod > 0
&& (rgo.RgoPauseDate == null || rgo.RgoNextShipDate < rgo.RgoPauseDate)
&& (rgo.RgoShipLimitCount == 0 || rgo.RgoShipCount < rgo.RgoShipLimitCount)
&& rgo.RgoNextShipDate == targetDate



=> 看起來只要改targetDate 跟 period變成1


```sql
 UPDATE regular_order
 SET rgo_next_ship_date = ADDDATE(CURDATE(), 2),
     rgo_ship_period = 1,
     rgo_update_user = 'TEST',
     rgo_update_time = NOW()
 WHERE rgo_pk = 256;

```


MG260617Y00001
MS260617Y000001






#### create 要撈到打api


where spi.SpiEnableRegular == true
&& spi.SpiRegularProvider == Nine1V2Provider
&& rgo.RgoStatus != 3
&& rod.RodStatus == 1
&& (rod.RodCreateOrderStatus == 0 || rod.RodCreateOrderStatus == 2)
&& rod.RodShipDate > today
&& rod.RodShipDate <= upperDate


```sql
UPDATE regular_order_detail
 SET 
     rod_create_order_date = DATE_SUB(CURDATE(), INTERVAL 1 DAY),
     rod_update_user = 'TEST',
     rod_update_time = NOW(),
     rod_ship_date = DATE_SUB(CURDATE(), INTERVAL 1 DAY)
 WHERE rod_pk = 4992;
```


打api失敗預期狀態

rod.RodCreateOrderStatus    = 2;
rod.RodCreateOrderErrorCode = errorCode;
rod.RodDaemonActiveTime     = DateTime.UtcNow.AddHours(8);


.SetProperty(r => r.RgoCreateOrderStatus,    (sbyte)2)
.SetProperty(r => r.RgoCreateOrderErrorCode, errorCode),


成功預期

rod.RodTgId                 = (int)tgId;
rod.RodTgCode               = tgCode;
rod.RodTsCode               = tsCode;
rod.RodCreateOrderStatus    = 1;
rod.RodCreateOrderErrorCode = string.Empty;
rod.RodDaemonActiveTime     = DateTime.UtcNow.AddHours(8);

var newShipDate     = rgo.RgoNextShipDate!.Value;
var newNextShipDate = newShipDate.AddDays(rgo.RgoShipPeriod);
var now             = DateTime.UtcNow.AddHours(8);
.SetProperty(r => r.RgoShipDate,             newShipDate)
.SetProperty(r => r.RgoNextShipDate,         newNextShipDate)
.SetProperty(r => r.RgoShipCount,            rodShipSequence)
.SetProperty(r => r.RgoInSchedule,           false)
.SetProperty(r => r.RgoCreateOrderStatus,    (sbyte)1)
.SetProperty(r => r.RgoCreateOrderErrorCode, string.Empty),



RomRgoPk = rgoPk,
RomRodPk = rodPk,


## mweb subgen 有問題

2Processor.GetRegularOrderProcessor|83|取得商品頁ID: 7389,SkuId: 0 資訊有問題 

salepageid = 7389
skuid = 0


var saleProductSKUId = regularOrderEntity.SaleProductSKUId;
private void ValidateSalePage(RegularOrderEntity regularOrderEntity)

this.ShoppingCartNodeMappingToRegularOrderEntity(context, regularOrderEntity);


regularOrderEntity.SaleProductSKUId = context.RegularOrderData.Data.SaleProductSKUId;



最後看起來是因為init job 時 skuid 沒有正確塞入



重新下單

來測 fail 過期



後來發現 ui 訂單 電話是 null 因為沒有加密




## 失敗單廢棄延期



where spi.SpiEnableRegular == true
&& spi.SpiRegularProvider == Nine1V2Provider
&& rgo.RgoStatus == 1
&& rgo.RgoCreateOrderStatus == 2
&& rgo.RgoNextShipDate < today


&& rgo.RgoStatus == 1
&& rgo.RgoCreateOrderStatus == 2
&& rgo.RgoNextShipDate < today




hkqa salepageid = 65828


❯ 我遇到一個問題,打 createorder job 在打 subscriptionautogen api 後成功後,order detail 應該會rod.RodCreateOrderStatus    = 1; 但更新完仍然是2        











一個個確認 autogen 錯誤

"_props":{"RodPk":4977,"Error":"Other"}}
rod_pk=4977