# 📊 資料表查詢參考

<br>

## 📖 目錄
  - [📱 推播訊息相關](#-推播訊息相關)
    - [AppPushProfile](#apppushprofile)
    - [DeviceAPPMapping](#deviceappmapping)
    - [PushNotificationSlave](#pushnotificationslave)
  - [💳 金流相關](#-金流相關)
    - [Shop（商店是否套用預設金物流）](#shop商店是否套用預設金物流)
    - [PayTypeExpress（記住信用卡相關資訊）](#paytypeexpress記住信用卡相關資訊)
    - [PayProfile（WebStoreDB 查詢有哪些付款方式）](#payprofilewebstoredb-查詢有哪些付款方式)
    - [RefundRequest](#refundrequest)
    - [ShopPayShippingDefault（預設金物流類型）](#shoppayshippingdefault預設金物流類型)
    - [SalePagePayType（商品頁支援的付款方式）](#salepaypaytype商品頁支援的付款方式)
    - [ShopPayType（商店支援的付款方式）](#shoppaytype商店支援的付款方式)
    - [SupplierPayType（廠商支援的付款方式）](#supplierpaytype廠商支援的付款方式)
    - [Supplier（可查廠商銷售幣別）](#supplier可查廠商銷售幣別)
  - [📦 訂單相關](#-訂單相關)
    - [ExpenseOrderSlave（銷售單用於報表）](#expenseorderslave銷售單用於報表)
    - [OrderSlaveFlow（大表綜合資訊）](#orderslaveflow大表綜合資訊)
    - [SalesOrderThirdPartyPayment](#salesorderthirdpartypayment)
    - [TradesOrderThirdPartyPayment（使用第三方金流支付的訂單表）](#tradesorderthirdpartypayment使用第三方金流支付的訂單表)
    - [TradesOrderGroup / TradesOrderSlave / OrderSlaveFlow](#tradesordergroup--tradesorderslave--orderslaveflow)
    - [SalesOrderSlaveDateInfo](#salesOrderSlaveDateInfo)
<br>

---

## 📱 推播訊息相關

### AppPushProfile

案例 1：GUID 查詢

<br>

```sql
select *
from AppPushProfile(nolock)
where AppPushProfile_ValidFlag = 1
and AppPushProfile_DeviceAPPMappingGUID = '635e37bd-f477-4e7d-a098-6291df9f161d'
```

<br>

### DeviceAPPMapping

案例 1：GUID 查詢

<br>

```sql
select *
from DeviceAPPMapping(nolock)
where DeviceAPPMapping_ValidFlag = 1
and DeviceAPPMapping_GUID = '635e37bd-f477-4e7d-a098-6291df9f161d'
```

<br>

### PushNotificationSlave

案例 1：Token 查 MemberId

<br>

```sql
use NotificationDB

select PushNotificationSlave_ShopId,PushNotificationSlave_MemberId,*
from PushNotificationSlave(nolock)
where PushNotificationSlave_Token LIKE 'cxNTxanSQqu%'
```

<br>

案例 2：GUID 查資料

<br>

```sql
USE NotificationDB

Select *
from PushNotificationSlave(nolock)
where PushNotificationSlave_ValidFlag = 1
and PushNotificationSlave_Id = 10512
and PushNotificationSlave_GUID = '635e37bd-f477-4e7d-a098-6291df9f161d'
```

<br>

---

## 💳 金流相關

### Shop（商店是否套用預設金物流）

```sql
select Shop_IsPayShippingDefault,*
from Shop(nolock)
where Shop_Id = 44;
```

<br>

### PayTypeExpress（記住信用卡相關資訊）

```sql
select PayTypeExpress_ShopId,COUNT(*) as 數量
from PayTypeExpress(nolock)
where PayTypeExpress_ValidFlag = 1
and PayTypeExpress_ShopId not in (2,11)
group by PayTypeExpress_ShopId
order by 數量 asc
```

<br>

### PayProfile（WebStoreDB 查詢有哪些付款方式）

```sql
USE WebStoreDB

SELECT TOP 100 *
FROM dbo.PayProfile WITH (NOLOCK)
WHERE PayProfile_ValidFlag = 1;
```

<br>

### RefundRequest

案例 1：想走線下匯款但同時跑 job 失敗的單

<br>

```sql
use ERPDB
Select RefundRequest_StatusUpdatedUser, RefundRequest_PayTypeDef,RefundRequest_TypeDef,RefundRequest_StatusDef,*
from RefundRequest(nolock)
where RefundRequest_PayTypeDef like '%AsiaPay%'
and RefundRequest_TypeDef = 'Remittance'
and RefundRequest_StatusDef = 'RefundRequestFail'
```

<br>

### ShopPayShippingDefault（預設金物流類型）

案例 1：移除預設金物流 = 信用卡一次 + 宅配

<br>

```sql
-- SELECT
SELECT ShopPayShippingDefault_ValidFlag,*
FROM dbo.ShopPayShippingDefault WITH(NOLOCK)
where ShopPayShippingDefault_ShopId = 200123
and ShopPayShippingDefault_ValidFlag = 1;

-- BACKUP
SELECT *
into MATempDB.dbo.tmp_WebStoreDB_ShopPayShippingDefault_VSTS359130
FROM dbo.ShopPayShippingDefault WITH(NOLOCK)
where ShopPayShippingDefault_ShopId = 200123
and ShopPayShippingDefault_ValidFlag = 1;

-- UPDATE
UPDATE dbo.ShopPayShippingDefault
set ShopPayShippingDefault_ValidFlag = 0
, ShopPayShippingDefault_UpdatedDateTime = getdate()
, ShopPayShippingDefault_UpdatedTimes = ShopPayShippingDefault_UpdatedTimes%255+1
, ShopPayShippingDefault_UpdatedUser = 'VSTS359130'
where ShopPayShippingDefault_ShopId = 200123
and ShopPayShippingDefault_ValidFlag = 1;

-- VERIFY
SELECT ShopPayShippingDefault_ValidFlag,*
FROM dbo.ShopPayShippingDefault WITH(NOLOCK)
where ShopPayShippingDefault_ShopId = 200123
and ShopPayShippingDefault_ValidFlag = 1;
```

<br>

### SalePagePayType（商品頁支援的付款方式）

```sql
select top 100 *
from SalePagePayType(nolock)
where SalePagePayType_SalePageId = 3831;
```

<br>

### ShopPayType（商店支援的付款方式）

```sql
select *
from ShopPayType(nolock)
where ShopPayType_ShopId = 4
and ShopPayType_ValidFlag = 1
order by ShopPayType_UpdatedDateTime
```

<br>

### SupplierPayType（廠商支援的付款方式）

```sql
select *
from SupplierPayType(nolock)
where SupplierPayType_ValidFlag = 1
and SupplierPayType_SupplierId = 43
```

<br>

### Supplier（可查廠商銷售幣別）

```sql
USE ERPDB

SELECT TOP 30 Supplier_Salescurrency
FROM dbo.Supplier WITH(NOLOCK)
JOIN DBO.Shop 
ON Supplier_Id = Shop_SupplierId
WHERE Shop_Id = 17
AND Supplier_ValidFlag = 1
```

<br>

---

## 📦 訂單相關

### ExpenseOrderSlave（銷售單用於報表）

案例 1：直收直付報表確認

<br>

```sql
select OrderSlaveFlow.OrderSlaveFlow_PayProfileTypeDef,sum(ExpenseOrderSlave_ChangedAmount) as '總金額'
from ExpenseOrderSlave(NOLOCK)
INNER JOIN ExpenseOrder(NOLOCK)
ON ExpenseOrder_Id = ExpenseOrderSlave_ExpenseOrderId
INNER JOIN OrderSlaveFlow(NOLOCK)
on OrderSlaveFlow.OrderSlaveFlow_TradesOrderSlaveCode = ExpenseOrderSlave_SourceCode COLLATE DATABASE_DEFAULT
where ExpenseOrderSlave_SourceDef in ('OrderEstablishedFee','ReturnOrderRefund','RechargeReceipt','CancelOrderRefund','AbnormalOrderRefund',
    'OrderFeeFee','OrderFeeRefund', 'Audience_SMS')
AND OrderSlaveFlow.OrderSlaveFlow_PayProfileTypeDef in ('TNG_AsiaPay','GrabPay_AsiaPay')
OR ExpenseOrder_ItemTypeDef IN ('PaymentProcFee','ShortMessageFee','MarketingSMSFee','SalesFee')
AND OrderSlaveFlow.OrderSlaveFlow_PayProfileTypeDef in ('TNG_AsiaPay','GrabPay_AsiaPay')
group by OrderSlaveFlow.OrderSlaveFlow_PayProfileTypeDef;
```

<br>

### OrderSlaveFlow（大表綜合資訊）

```sql
SELECT OrderSlaveFlow_TradesOrderGroupCode, OrderSlaveFlow_SalesOrderSlaveShopId, *
FROM dbo.OrderSlaveFlow(NOLOCK)
WHERE OrderSlaveFlow_TradesOrderSlaveId = 38000
```

<br>

### SalesOrderThirdPartyPayment

案例 1：壓 AsiaPay SaleOrder 關帳時間

<br>

```sql
USE ERPDB

select SalesOrderThirdPartyPayment_CreatedDateTime,SalesOrderThirdPartyPayment_DateTime,SalesOrderThirdPartyPayment_UpdatedDateTime,*
from SalesOrderThirdPartyPayment(nolock)
where SalesOrderThirdPartyPayment_TradesOrderGroupCode = 'MG240207R00002'

UPDATE SalesOrderThirdPartyPayment
SET SalesOrderThirdPartyPayment_CreatedDateTime = DATEADD(DAY, -181, SalesOrderThirdPartyPayment_CreatedDateTime),
SalesOrderThirdPartyPayment_DateTime = DATEADD(DAY, -181, SalesOrderThirdPartyPayment_DateTime),
SalesOrderThirdPartyPayment_UpdatedDateTime = DATEADD(DAY, -181, SalesOrderThirdPartyPayment_UpdatedDateTime)
where SalesOrderThirdPartyPayment_TradesOrderGroupCode = 'MG240207R00002'

select SalesOrderThirdPartyPayment_CreatedDateTime,SalesOrderThirdPartyPayment_DateTime,SalesOrderThirdPartyPayment_UpdatedDateTime,*
from SalesOrderThirdPartyPayment(nolock)
where SalesOrderThirdPartyPayment_TradesOrderGroupCode = 'MG240207R00002'
```

<br>

### TradesOrderThirdPartyPayment（使用第三方金流支付的訂單表）

查最新 2c2p 訂單：

<br>

```sql
USE WebStoreDB

SELECT TOP 50 *
FROM TradesOrderThirdPartyPayment 
WHERE TradesOrderThirdPartyPayment_TypeDef = 'TwoCTwoP'
ORDER BY TradesOrderThirdPartyPayment_Id DESC;
```

<br>

失敗訂單相關：

<br>

```sql
use webstore
select distinct tradesOrderThirdPartyPayment
from TradesOrderThirdPartyPayment with (nolock)
where TradesOrderThirdPartyPayment_statusdef = 'FAIL'
ORDER BY TradesOrderThirdPartyPayment_id desc
```

<br>

### TradesOrderGroup / TradesOrderSlave / OrderSlaveFlow

案例 1：查表（GroupCode → TradesOrderSlave_Id, SaleProductSKUId, TotalPrice, ECouponDiscount, LoyaltyPointDiscount, PromotionDiscount, TotalPayment）

<br>

```sql
SELECT *
FROM dbo.TradesOrderGroup(NOLOCK)
WHERE TradesOrderGroup_Code = 'TG231212LA00M5'
    AND TradesOrderGroup_ValidFlag = 1

SELECT TradesOrderSlave_Id, TradesOrderSlave_SaleProductSKUId, TradesOrderSlave_TotalPrice, TradesOrderSlave_ECouponDiscount, TradesOrderSlave_LoyaltyPointDiscount, TradesOrderSlave_PromotionDiscount, TradesOrderSlave_TotalPayment
FROM dbo.OrderSlaveFlow(NOLOCK)
JOIN dbo.TradesOrderSlave(NOLOCK)
    ON TradesOrderSlave_ValidFlag = 1
    AND TradesOrderSlave_Id = OrderSlaveFlow_TradesOrderSlaveId
WHERE OrderSlaveFlow_ValidFlag = 1
    AND OrderSlaveFlow_TradesOrderGroupCode = 'TG231212LA00M5'
    AND OrderSlaveFlow_TradesOrderGroupId = 81886547
```

<br>

案例 2：查表（UniqueKey → GroupCode → TradesOrderThirdPartyPayment_StatusDef）

<br>

```sql
use WebStoreDB

Declare @tgcode varchar(20);

select top 1 @tgcode = TradesOrderGroup_Code
from TradesOrderGroup(nolock)
where TradesOrderGroup_UniqueKey = 'xxxxxx'

select TradesOrderThirdPartyPayment_StatusDef,TradesOrderThirdPartyPayment_TradesOrderGroupCode,TradesOrderThirdPartyPayment_DateTime
from TradesOrderThirdPartyPayment(nolock)
where TradesOrderThirdPartyPayment_TradesOrderGroupCode = @tgCode

select *
from OrderSlaveFlow(nolock)
where OrderSlaveFlow_TradesOrderGroupCode = @tgCode
```

<br>

案例 3：TradesOrderSlave 看 Discount

<br>

```sql
SELECT TradesOrderSlave_Id, TradesOrderSlave_SaleProductSKUId, TradesOrderSlave_TotalPrice, TradesOrderSlave_ECouponDiscount, TradesOrderSlave_LoyaltyPointDiscount, TradesOrderSlave_PromotionDiscount, TradesOrderSlave_TotalPayment
FROM dbo.OrderSlaveFlow(NOLOCK)
JOIN dbo.TradesOrderSlave(NOLOCK)
    ON TradesOrderSlave_ValidFlag = 1
    AND TradesOrderSlave_Id = OrderSlaveFlow_TradesOrderSlaveId
WHERE OrderSlaveFlow_ValidFlag = 1
    AND OrderSlaveFlow_TradesOrderGroupCode = 'TG231212LA00M5'
    AND OrderSlaveFlow_TradesOrderGroupId = 81886547
```

<br>

案例 4：查付款方式

<br>

```sql
Select TradesOrder_PayProfileTypeDef,*
from TradesOrder(nolock)
where TradesOrder_ValidFlag = 1
and TradesOrder_Code = 'MM240207K00004'

Select TradesOrderSlave_PayProfileTypeDef,*
from TradesOrderSlave(nolock)
where TradesOrderSlave_ValidFlag = 1
and TradesOrderSlave_Code = 'MS240207K000004'
```

<br>

案例 5：壓 TradesOrder 關帳時間

<br>

```sql
USE WebStoreDB

select TradesOrderGroup_CreatedDateTime,TradesOrderGroup_DateTime,TradesOrderGroup_UpdatedDateTime,*
from TradesOrderGroup(nolock)
where TradesOrderGroup_Code = 'MG240207K00005';

UPDATE TradesOrderGroup
SET TradesOrderGroup_CreatedDateTime = DATEADD(DAY, -181, TradesOrderGroup_CreatedDateTime),
TradesOrderGroup_DateTime = DATEADD(DAY, -181, TradesOrderGroup_DateTime),
TradesOrderGroup_UpdatedDateTime = DATEADD(DAY, -181, TradesOrderGroup_UpdatedDateTime)
where TradesOrderGroup_Code = 'MG240207K00005';

select TradesOrderGroup_CreatedDateTime,TradesOrderGroup_DateTime,TradesOrderGroup_UpdatedDateTime,*
from TradesOrderGroup(nolock)
where TradesOrderGroup_Code = 'MG240207K00005';
```

<br>

案例 6：訂單成立→出貨→轉單時間差

<br>

```
OrderSlaveFlow_CreatedDateTime: 2024-01-27 16:32:45.743
ShippingOrderSlave_UpdatedDateTime: 2024-01-27 16:38:20.897
OrderSlaveFlow_TransToERPDateTime: 2024-01-27 16:33:08.453
```

<br>

案例 7：某張訂單編號去查詢會員 ID & ShopId

<br>

```sql
USE WebStoreDB

DECLARE @tgNumber VARCHAR(20) = 'TG231129T00002'
SELECT TradesOrderGroup_MemberId, TradesOrderGroup_ShopId
FROM dbo.TradesOrderGroup WITH (nolock)
WHERE TradesOrderGroup_ValidFlag = 1
AND TradesOrderGroup_Code = @tgNumber;
```

<br>

另一個版本：

<br>

```sql
use WebstoreDB

Declare @tgCode varchar(20) = 'TGNumber'
Select TradesOrderGroup_MemberId,TradesOrderGroup_ShopId
from dbo.TradesOrderGroup (nolock)
where TradesOrderGroup_ValidFlag = 1 
and TradesOrderGroup_Code = @tgCode;
```

<br>



### SalesOrderSlaveDateInfo

有建議訂單時間的索引

---
