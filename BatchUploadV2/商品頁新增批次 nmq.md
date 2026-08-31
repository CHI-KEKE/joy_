

## 商品頁新增批次 nmq


SalePageCreateBatchProcess



            //// 註冊大量上傳批次處理 Service
            this.Builder.RegisterType<CreateSalePageUploadService>()
                   .WithParameter(new NamedParameter("batchUploadEntity", this.BatchUploadEntity))
                   .Named<IBatchUploadService>(BatchUploadTypeEnum.SalePageCreate.ToString());  //// 91mai格式批次新增商品頁






CreateSalePageUploadService

SalePageCreate => BatchUploadCreateSalePageService

sms 完全沒驗證這段~


## 批次新增商品頁問甚麼行為不同

https://91appinc.visualstudio.com/G11n/_workitems/edit/550414

C:\91APP\NMQ\nineyi.scm.nmqv2\SCM\Frontend\BLV2\BatchUpload\CreateSalePageUploadService.cs

line 1333


by shop

select *
from ShopPayShippingDefault(nolock)
where ShopPayShippingDefault_ShopId = 7


select *
from SupplierPayType(nolock)
where SupplierPayType_ValidFlag  =1
and SupplierPayType_SupplierId = 79

select *
from ShopPayType(nolock)
where ShopPayType_ValidFlag  =1
and ShopPayType_ShopId= 83

select *
from Definition(nolock)
where Definition_ValidFlag = 1
and Definition_TableName = 'PayProfile'
and Definition_Code = 'CreditCardInstallment_Razer'


select *
from PayShippingMapping(nolock)
where PayShippingMapping_ValidFlag = 1
and PayShippingMapping_PayProfileTypeDef in ('CreditCardOnce_Razer','CreditCardInstallment_Razer')