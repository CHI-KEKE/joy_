# 消費者申請退貨 API

## API 端點

```bash
POST /webapi/ReturnGoodsRequestV2/CreateRequest?lang=zh-TW&shopId=4
```

範例：`https://lilychuang2.shop.qa1.my.91dev.tw/webapi/ReturnGoodsRequestV2/CreateRequest?lang=zh-TW&shopId=4`

---

## Request

```json
{
    "Id": 16434,
    "Code": "MG260204P00004",
    "ShopId": 4,
    "TradesOrderList": [
        {
            "Id": 16439,
            "Code": "MM260204PA0001",
            "TradesOrderGroupId": 16434,
            "TemperatureTypeDef": "Normal",
            "TradesOrderSlaveList": [
                {
                    "TemperatureTypeDef": "Normal",
                    "OrderSlaveFlowStatusForUserDef": "ShippingFinish",
                    "OrderSlaveFlowCanCancel": false,
                    "OrderSlaveFlowCanReturn": true,
                    "OrderSlaveFlowCanChange": true,
                    "OrderSlaveFlowIsCancelRequesting": false,
                    "SalePageGiftList": [],
                    "SaleProductSKUForDisplay": "",
                    "OrderSlaveFlowCancelOrderSlaveId": null,
                    "OrderSlaveFlowReturnGoodsOrderSlaveId": null,
                    "CauseDef": "5",
                    "DefaultSelected": true,
                    "DefaultEnabled": true,
                    "HasPartialReturn": false,
                    "ShippingType": 1,
                    "ShippingProfileTypeDef": "Home",
                    "OrderSlaveFlowStatusDef": "WaitingToFinish",
                    "IsReturnable": true,
                    "ECouponSlaveId": 0,
                    "PartialRefundAmount": 0,
                    "SaleProductReturnGoodsTypeDef": ["NormalReturn"],
                    "SaleProductVirtualProductProviderTypeDef": "",
                    "SaleProductProductTypeDef": "Physical",
                    "VoucherRefundAmountInfo": null,
                    "TradesOrderSlaveCurrency": null,
                    "IsSalepageAddOns": false,
                    "ItemType": "",
                    "ItemGroup": 0,
                    "TradesOrderSlavePaymentDetail": {
                        "PayMethods": [
                            {
                                "PayProfileType": "CreditCardOnce_Razer",
                                "PaymentProcessingMethod": null,
                                "Amount": 5
                            }
                        ]
                    },
                    "SalepageRuleTypeList": [],
                    "Id": 25168,
                    "Code": "MS260204PA00001",
                    "SalePageId": 7302,
                    "SalePageGroupSeq": 0,
                    "SaleProductTitle": "atome",
                    "TradesOrderId": 16439,
                    "IsMajor": true,
                    "IsGift": false,
                    "IsExtra": false,
                    "IsSalePageGift": false,
                    "TotalPayment": 5,
                    "Price": 5,
                    "Qty": 1,
                    "ECouponDiscount": 0,
                    "PromotionDiscount": 0,
                    "PicUrl": "//img.qa1.my.91dev.tw/webapi/imagesV3/Cropped/SalePage/7302/0/639056300826370000?v=1",
                    "SalePageGiftId": null,
                    "SalePageGiftGroupSeq": null,
                    "SalePageUpdatedDateTime": "/Date(1770004482637+0800)/",
                    "SaleProductSKUId": 9341,
                    "SaleProductSKUImageCount": 0,
                    "SaleProductSKUUpdatedDateTime": "/Date(1769999072277+0800)/",
                    "OtherPayProfileTypeDef": ""
                },
                {
                    "TemperatureTypeDef": "Normal",
                    "OrderSlaveFlowStatusForUserDef": "ShippingFinish",
                    "OrderSlaveFlowCanCancel": false,
                    "OrderSlaveFlowCanReturn": true,
                    "OrderSlaveFlowCanChange": true,
                    "OrderSlaveFlowIsCancelRequesting": false,
                    "SalePageGiftList": [],
                    "SaleProductSKUForDisplay": "",
                    "OrderSlaveFlowCancelOrderSlaveId": null,
                    "OrderSlaveFlowReturnGoodsOrderSlaveId": null,
                    "CauseDef": "5",
                    "DefaultSelected": true,
                    "DefaultEnabled": true,
                    "HasPartialReturn": false,
                    "ShippingType": 1,
                    "ShippingProfileTypeDef": "Home",
                    "OrderSlaveFlowStatusDef": "WaitingToFinish",
                    "IsReturnable": true,
                    "ECouponSlaveId": 0,
                    "PartialRefundAmount": 0,
                    "SaleProductReturnGoodsTypeDef": ["NormalReturn"],
                    "SaleProductVirtualProductProviderTypeDef": "",
                    "SaleProductProductTypeDef": "Physical",
                    "VoucherRefundAmountInfo": null,
                    "TradesOrderSlaveCurrency": null,
                    "IsSalepageAddOns": false,
                    "ItemType": "",
                    "ItemGroup": 0,
                    "TradesOrderSlavePaymentDetail": {
                        "PayMethods": [
                            {
                                "PayProfileType": "CreditCardOnce_Razer",
                                "PaymentProcessingMethod": null,
                                "Amount": 5
                            }
                        ]
                    },
                    "SalepageRuleTypeList": [],
                    "Id": 25169,
                    "Code": "MS260204PA00002",
                    "SalePageId": 7302,
                    "SalePageGroupSeq": 0,
                    "SaleProductTitle": "atome",
                    "TradesOrderId": 16439,
                    "IsMajor": true,
                    "IsGift": false,
                    "IsExtra": false,
                    "IsSalePageGift": false,
                    "TotalPayment": 5,
                    "Price": 5,
                    "Qty": 1,
                    "ECouponDiscount": 0,
                    "PromotionDiscount": 0,
                    "PicUrl": "//img.qa1.my.91dev.tw/webapi/imagesV3/Cropped/SalePage/7302/0/639056300826370000?v=1",
                    "SalePageGiftId": null,
                    "SalePageGiftGroupSeq": null,
                    "SalePageUpdatedDateTime": "/Date(1770004482637+0800)/",
                    "SaleProductSKUId": 9341,
                    "SaleProductSKUImageCount": 0,
                    "SaleProductSKUUpdatedDateTime": "/Date(1769999072277+0800)/",
                    "OtherPayProfileTypeDef": ""
                }
            ],
            "TotalFee": 10,
            "LocationId": 0,
            "TradesOrderFeeShopShippingTypePromotionId": 0,
            "TradesOrderFeeShopShippingTypePromotionName": null,
            "TradesOrderFeeTotalDiscount": 0,
            "TradesOrderFeeECouponId": 0,
            "TradesOrderFeeECouponSlaveId": 0,
            "TradesOrderFeeECouponDiscount": 0,
            "TradesOrderReceiverStoreId": ""
        }
    ],
    "RefundInfo": {
        "Amount": 10,
        "BankAccountNo": null,
        "BankAccountOwner": null,
        "BankName": null,
        "BankSubName": null,
        "IdentityId": null
    },
    "MemberLocationInfo": {
        "Address": "FEFEFEFE",
        "State": "Johor",
        "ZipCode": "82100",
        "City": "Ayer Baloi",
        "District": null,
        "FullName": "AllenLinaaa",
        "CellPhone": "01789012111",
        "CountryCode": null,
        "CountryEnglishName": null,
        "CountryProfileId": null
    },
    "CauseDescription": "ffff",
    "PayInfo": {
        "PayProfileTypeDef": 33554432,
        "PayProfileTypeDefDesc": "CreditCardOnce_Razer",
        "CreditCardBankName": null,
        "TotalPayment": 20,
        "PayProfileTypeDisplayName": "信用卡一次付清",
        "TradesOrderGroupCurrency": null,
        "MemberTradesOrderGroupPayInfoForReverseFlowCurrencyInfo": null,
        "PaymentDetail": {
            "PayMethods": [
                {
                    "PayProfileType": "CreditCardOnce_Razer",
                    "PaymentProcessingMethod": null,
                    "Amount": 20
                }
            ]
        },
        "MemberTradesOrderPaymentDetail": {
            "PayMethods": [
                {
                    "PayProfileType": "CreditCardOnce_Razer",
                    "DisplayName": "信用卡一次付清",
                    "Amount": 20,
                    "UsageDetails": [],
                    "UsageDetailsDescription": ""
                }
            ]
        },
        "OtherPayTypeDisplayName": null
    },
    "IsAgreedToDelegate": true,
    "HasActivity": false,
    "HasPurchaseExtra": false,
    "Description": "",
    "DescriptionMulHtmlUrl": "https://static.qa1.my.91dev.tw/QA/ChangeReturnContentTranslatedFile/4/1/zh-TW.txt?v=1746025190",
    "PickupInfo": {
        "TradesOrderReceiverZipCode": "82100",
        "TradesOrderReceiverAddress": "FEFEFEFE",
        "TradesOrderReceiverState": "Johor",
        "TradesOrderReceiverCity": "Ayer Baloi",
        "TradesOrderReceiverDistrict": ""
    },
    "IsPartialReturnGoodsEnabled": false,
    "IsGiftCardPartialReverseEnabled": true,
    "InvoiceDescriptiondef": "Hidden",
    "ShippingTypeDef": "Home",
    "ShippingCountry": "Malaysia",
    "DateTime": "/Date(1770184003833+0800)/",
    "IsRefundInfoCollectingEnabled": false,
    "RefundTypeDefOption": null,
    "ReturnShippingTypeDef": 1,
    "TradesOrderSlaveWithUnAllocatePromotionEngineList": [],
    "IsOversea": null,
    "GiftCardRefundInstruction": null,
    "GiftCardReverseDisplayInfo": null
}
```

---

## Response

```json
{
    "ReturnCode": "API0001",
    "Data": null,
    "Message": "Success"
}
```

---

## ReturnGoodsRequestEntity 欄位

> 來源：webstoredb

### 產生 ReturnGoodsRequestEntity

```csharp
//// 退貨申請單單號
ReturnGoodsRequest_Code = code,

//// 退貨原因
//// 備註: 雖然退貨原因是必填寫欄位 但是滿額贈品不會有取消原因 所以需要補預設值
ReturnGoodsRequest_CauseDef = selectedTs.CauseDef ?? "4",

//// 退貨描述
ReturnGoodsRequest_CauseDesc = confirmEntity.CauseDescription,

ReturnGoodsRequest_IsClosed = false,
ReturnGoodsRequest_MemberId = memberId,

//// 退貨單狀態
ReturnGoodsRequest_StatusDef = ReturnGoodsRequestStatusEnum.WaitingToTrans.ToString(),
ReturnGoodsRequest_StatusUpdatedDateTime = DateTime.Now,
ReturnGoodsRequest_StatusUpdatedUser = memberId.ToString(),

//// 付款方式
ReturnGoodsRequest_PayTypeDef = payProfileTypeDef.ToString(),
ReturnGoodsRequest_PayProfileTypeDef = payProfileTypeDef.ToString(),

//// 主單子單資訊
ReturnGoodsRequest_TradesOrderId = tsFromDb.TradesOrderId,
ReturnGoodsRequest_Qty = Convert.ToInt16(tsFromDb.Qty),
ReturnGoodsRequest_TradesOrderSlaveId = tsFromDb.Id,
ReturnGoodsRequest_TradesOrderSlaveCode = tsFromDb.Code,

//// 退款資訊
ReturnGoodsRequest_BankName = confirmEntity.RefundInfo.BankName,
RerurnGoodsRequest_BankSubName = confirmEntity.RefundInfo.BankSubName,
RerurnGoodsRequest_BankAccountNo = confirmEntity.RefundInfo.BankAccountNo,
RerurnGoodsRequest_BankAccountOwner = confirmEntity.RefundInfo.BankAccountOwner,
ReturnGoodsRequest_IdentityId = confirmEntity.RefundInfo.IdentityId,
ReturnGoodsRequest_SaleProductProductTypeDef = tsFromDb.SaleProductProductTypeDef,
ReturnGoodsRequest_SaleProductVirtualProductProviderTypeDef = tsFromDb.SaleProductVirtualProductProviderTypeDef,
ReturnGoodsRequest_VirtualProducInfo = tsFromDb.ShippingProfileTypeDef == ShippingProfileTypeDefEnum.DigitalDelivery.ToString() && tsFromDb.SaleProductVirtualProductProviderTypeDef == "Edenred" ?
    tsFromDb.VoucherRefundAmountInfo.VoucherList.Select(x => new VoucherRefundEntity() { Id = x.VoucherId, RefundQty = x.RefundQty }).ToJSON() : string.Empty,
ReturnGoodsRequest_NonRefundAmount = tsFromDb.ShippingProfileTypeDef == ShippingProfileTypeDefEnum.DigitalDelivery.ToString() && tsFromDb.SaleProductVirtualProductProviderTypeDef == "Edenred" ?
    tsFromDb.VoucherRefundAmountInfo.VoucherList.Sum(x => x.UnRefundableAmt) : 0,

//// 取貨資訊
ReturnGoodsRequest_ZipCode = confirmEntity.MemberLocationInfo.ZipCode ?? string.Empty,
ReturnGoodsRequest_State = confirmEntity.MemberLocationInfo.State ?? string.Empty,
ReturnGoodsRequest_City = confirmEntity.MemberLocationInfo.City ?? string.Empty,
ReturnGoodsRequest_District = confirmEntity.MemberLocationInfo.District ?? string.Empty,
ReturnGoodsRequest_Address = confirmEntity.MemberLocationInfo.Address == null ? string.Empty : confirmEntity.MemberLocationInfo.Address,
ReturnGoodsRequest_CellPhone = confirmEntity.MemberLocationInfo.CellPhone,
ReturnGoodsRequest_Contact = confirmEntity.MemberLocationInfo.FullName,
ReturnGoodsRequest_County = confirmEntity.MemberLocationInfo.CountryEnglishName,
ReturnGoodsRequest_CountryCode = confirmEntity.MemberLocationInfo.CountryCode,
ReturnGoodsRequest_CountryProfileId = confirmEntity.MemberLocationInfo.CountryProfileId,

//// 退貨方式
ReturnGoodsRequest_TypeDef = ((byte)confirmEntity.ReturnShippingTypeDef).ToString(),

//// 若訂單子單上有加註 不代開發票 應將代為處理發票或折讓相關事宜 取消勾選
RerurnGoodsRequest_IsAgreedToDelegate = tsFromDb.IsIssuedInvoice ? confirmEntity.IsAgreedToDelegate : false
```

### webstoredb 寫入流程

```csharp
//// 1. 建立退貨申請單
//// 2. 更新 OrderSlaveFlow 狀態與相關退貨資訊

//// 退貨相關資訊
flow.OrderSlaveFlow_ReturnGoodsRequestId = request.ReturnGoodsRequest_Id;
flow.OrderSlaveFlow_ReturnGoodsRequestIsClosed = false;
flow.OrderSlaveFlow_ReturnGoodsRequestStatusCause = request.ReturnGoodsRequest_StatusCause;

//// 退貨單狀態切換為待轉單
flow.OrderSlaveFlow_ReturnGoodsRequestStatusDef = ReturnGoodsRequestStatusEnum.WaitingToTrans.ToString();
flow.OrderSlaveFlow_ReturnGoodsRequestStatusUpdatedDateTime = request.ReturnGoodsRequest_StatusUpdatedDateTime;

//// OrderSlaveFlow 狀態切換為退貨處理中
flow.OrderSlaveFlow_StatusForSCMDef = MemberTradesOrderOrderSlaveFlowStatusForScmEnum.Hide.ToString();
flow.OrderSlaveFlow_StatusDef = MemberTradesOrderOrderSlaveFlowStatusEnum.ReturnGoodsRequesting.ToString();
flow.OrderSlaveFlow_StatusForUserDef = MemberTradesOrderOrderSlaveFlowStatusForUserEnum.ReturnGoodsRequesting.ToString();

flow.OrderSlaveFlow_CanCancel = false;
flow.OrderSlaveFlow_CanReturn = false;
flow.OrderSlaveFlow_CanChange = false;
```

---

## 其他欄位

- `detail.IsPartialReturnGoodsEnabled`：商店是否開放部分退貨
