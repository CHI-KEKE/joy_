https://91appinc.visualstudio.com/DailyResource/_workitems/edit/577050

[HK] 第三方物流商訂單管理：訂單配號與列印託運單匯出的訂單不一致


https://91appinc.visualstudio.com/DailyResource/_workitems/edit/577050


PROD HK 2 : TG260127R00042

第三方物流商訂單管理 > 訂單配號 > 宅配



## 配號

第三方物流商訂單管理 > 訂單配號 > 宅配 撈取區間 1/27 15:33 – 1/28 15:32

https://store.91app.hk/Api/Delivery/GetDeliverySalesOrderList

```json
{
    "SearchItem": {
        "ShopId": 2,
        "DateTimeType": "ShippingData_CreditCheckDateTime",
        "StartDateTime": "2026-01-27T07:30:00.000Z",
        "EndDateTime": "2026-01-28T08:00:00.000Z",
        "TgCode": null,
        "TmCode": null,
        "TsCode": null,
        "SaleProductSKUOuterId": null,
        "ConfirmTypeDef": null,
        "SaleProductShippingTypeDef": null,
        "ReceiverName": null,
        "ReceiverMobile": null,
        "SaleProductTitle": null,
        "TemperatureTypeDef": null,
        "DeliveryType": 7
    },
    "Skip": 0,
    "Take": 25
}

```


RESPONSE

Data >> List >> SlaveList >> DateTime

## 第一層

ateTime = i.ShippingData_CreditCheckDateTime.Value,


csp = csp_GetDeliverySalesOrderSlaveShippingMallData


#tmpResultExpectShippingDateData
- OrderSlaveFlow
- #tmpExpectSlaveData
  - #tmpExpectSlave
    - SalesOrderSlave
    - SalesOrder
    - SalesOrderReceiver
    - ZipCode
    - SalesOrderFee
    - SalesOrderGroup
    - ShippingArea
    - CountryProfile
  - @DateTimeType
    - ShippingData_CreditCheckDateTime => CreditCheck_DateTime
    - AtmAccount_UpdatedDateTime
    - SalesOrderThirdPartyPayment_UpdatedDateTime
    - CashOnDelivery : OrderSlaveFlow_TransToERPDateTime
    - ShippingData_SalesOrderSlavePayProfileTypeDef = FreeOfCharge => ShippingData_SalesOrderSlaveDateTime
(CreditCheck.CreditCheck_TradesOrderGroupCode = ExpectSalve.ShippingData_TradesOrderGroupCode)


## #tmpOrderPayData - PaySuccessDateTime

CreditCheck => CreditCheck_DateTime
AtmAccount => AtmAccount_UpdatedDateTime
SalesOrderThirdPartyPayment => SalesOrderThirdPartyPayment_DateTime
ShippingData_SalesOrderSlaveShippingProfileTypeDef = CashOnDelivery => OrderSlaveFlow_TransToERPDateTime
ShippingData_PayProfileTypeDef = FreeOfCharge => OrderSlaveFlow_TransToERPDateTime

## @DateTimeType

ShippingData_SalesOrderSlaveConfirmDateTime




#tmpPromotion


```json
{
    "Status": "Success",
    "Data": {
        "List": [
            {
                "Master": {
                    "TgCode": "TG260128Q00092",
                    "TmCode": "TM260128Q00086",
                    "TotalPayment": null,
                    "TgReceiptAmount": null,
                    "TotalFee": null,
                    "TotalFeeReceiptAmount": null,
                    "SalesOrderFeeTotalDiscount": null,
                    "GlobalReceipt": null,
                    "PaidAmount": 0,
                    "IsEnableNonReturnableSaleProduct": false,
                    "TotalPoints": null,
                    "IsRefundInfoCollectingEnabled": false,
                    "IsShowCourierProgress": false,
                    "SalesOrderFeeECouponId": null,
                    "SalesOrderFeeECouponSlaveId": null,
                    "SalesOrderFeeECouponDiscount": null,
                    "StickerPointInfo": null,
                    "WelfarePointInfo": null,
                    "ShopId": 0,
                    "PaymentDetail": null,
                    "RefundTypeDefOption": null,
                    "IsOversea": null,
                    "HasOtherPayProfileType": false,
                    "GiftCardRefundInstruction": null
                },
                "SlaveList": [
                    {
                        "TradesOrderGroupCode": "TG260128Q00092",
                        "TradesOrderCode": "TM260128Q00086",
                        "TradesOrderSlaveCode": "TS260128Q000252",
                        "ShopShippingTypeName": "付款後順豐站及營業點免運費",
                        "SalesOrderFee": 9,
                        "TemperatureTypeDef": "Normal",
                        "TemperatureTypeDefDesc": "常溫",
                        "DateTime": "2026-01-28T14:40:20.703+08:00",
                        "ConfirmDateTime": null,
                        "ExpectShippingDate": "2026-01-30T14:40:20.877+08:00",
                        "ShippingTypeDef": 1,
                        "ShippingTypeDefDesc": "一般",
                        "SaleProductTitle": "[編輯]編輯為回補",
                        "SkuForDisplay": "L",
                        "SalePageId": 549058,
                        "SaleProductSKUOuterId": "",
                        "ProductAttribute": null,
                        "Qty": 4,
                        "Price": 1,
                        "TotalDiscount": 0,
                        "TotalPayment": 4,
                        "PromotionIds": "36751",
                        "PromotionNames": "HK滿額贈",
                        "PromotionDiscount": 0,
                        "ECouponId": null,
                        "ECouponName": null,
                        "ECouponDiscount": 0,
                        "ECouponSlaveInfos": [],
                        "ReceiverName": "哈哈哈",
                        "ReceiverMobile": "41234567",
                        "ReceiverAddress": "香港灣仔區香港香港島灣仔區灣仔謝斐道404-406號文石大廈地下A及B號舖^852T^",
                        "ReceiverZipCode": "",
                        "SupplierNote": null,
                        "SalesOrderMemo": "{CustomOrderMemo}",
                        "StatusDef": "WaitingToShipping",
                        "StatusDefDesc": "已成立",
                        "DeviceAPPMappingPlatformDef": "官網(PC)",
                        "SalePageGiftGroupCode": null,
                        "SalesOrderSlaveId": 8272146,
                        "SalesOrderId": 2540231,
                        "SupplierId": 2,
                        "ShopId": 2,
                        "IsGift": false,
                        "IsSalePageGift": false,
                        "SalePageGiftId": null,
                        "SalePageGiftGroupSeq": null,
                        "HasSalePageGift": false,
                        "HasGift": true,
                        "InvoiceTitle": null,
                        "InvoiceAddress": "",
                        "InvoiceNo": null,
                        "InvoiceZipCode": null,
                        "ShippingProfileTypeDef": "SFBusinessStores",
                        "ShippingProfileTypeDefDesc": "順豐站及營業點",
                        "ReceiverCountryCode": "852",
                        "ReceiverCountry": "Hong Kong",
                        "ReceiverState": "",
                        "ReceiverCity": "香港",
                        "ReceiverDistrict": "灣仔區",
                        "ReceiverStreet": "香港香港島灣仔區灣仔謝斐道404-406號文石大廈地下A及B號舖^852T^",
                        "ReceiverStoreId": "852T",
                        "ReceiverStoreName": "852T",
                        "GiftECouponId": 0,
                        "SaleProductWeight": 1,
                        "WeightSubtotal": 4,
                        "BookingPickupDate": null,
                        "BookingPickupPeriod": null,
                        "IsUsingCustomPackaging": false,
                        "Id": 8272146,
                        "VerificationToken": "7f24eb5daf83cf6ff689bc4303697e532f3e0d12f682e22cb25e5635ff8cf947c49bb34749f3fdde3950b22c218a123c8c028712fed72b9ecedfe96d810486be"
                    },
                    {
                        "TradesOrderGroupCode": "TG260128Q00092",
                        "TradesOrderCode": "TM260128Q00086",
                        "TradesOrderSlaveCode": "TS260128Q000253",
                        "ShopShippingTypeName": "付款後順豐站及營業點免運費",
                        "SalesOrderFee": 9,
                        "TemperatureTypeDef": "Normal",
                        "TemperatureTypeDefDesc": "常溫",
                        "DateTime": "2026-01-28T14:40:20.703+08:00",
                        "ConfirmDateTime": null,
                        "ExpectShippingDate": "2026-01-30T14:40:20.907+08:00",
                        "ShippingTypeDef": 1,
                        "ShippingTypeDefDesc": "一般",
                        "SaleProductTitle": "藍藍贈品",
                        "SkuForDisplay": "",
                        "SalePageId": 529455,
                        "SaleProductSKUOuterId": "FREE001",
                        "ProductAttribute": null,
                        "Qty": 1,
                        "Price": 0,
                        "TotalDiscount": 0,
                        "TotalPayment": 0,
                        "PromotionIds": "",
                        "PromotionNames": "",
                        "PromotionDiscount": 0,
                        "ECouponId": null,
                        "ECouponName": null,
                        "ECouponDiscount": 0,
                        "ECouponSlaveInfos": [],
                        "ReceiverName": "哈哈哈",
                        "ReceiverMobile": "41234567",
                        "ReceiverAddress": "香港灣仔區香港香港島灣仔區灣仔謝斐道404-406號文石大廈地下A及B號舖^852T^",
                        "ReceiverZipCode": "",
                        "SupplierNote": null,
                        "SalesOrderMemo": "{CustomOrderMemo}",
                        "StatusDef": "WaitingToShipping",
                        "StatusDefDesc": "已成立",
                        "DeviceAPPMappingPlatformDef": "官網(PC)",
                        "SalePageGiftGroupCode": null,
                        "SalesOrderSlaveId": 8272147,
                        "SalesOrderId": 2540231,
                        "SupplierId": 2,
                        "ShopId": 2,
                        "IsGift": true,
                        "IsSalePageGift": false,
                        "SalePageGiftId": null,
                        "SalePageGiftGroupSeq": null,
                        "HasSalePageGift": false,
                        "HasGift": false,
                        "InvoiceTitle": null,
                        "InvoiceAddress": "",
                        "InvoiceNo": null,
                        "InvoiceZipCode": null,
                        "ShippingProfileTypeDef": "SFBusinessStores",
                        "ShippingProfileTypeDefDesc": "順豐站及營業點",
                        "ReceiverCountryCode": "852",
                        "ReceiverCountry": "Hong Kong",
                        "ReceiverState": "",
                        "ReceiverCity": "香港",
                        "ReceiverDistrict": "灣仔區",
                        "ReceiverStreet": "香港香港島灣仔區灣仔謝斐道404-406號文石大廈地下A及B號舖^852T^",
                        "ReceiverStoreId": "852T",
                        "ReceiverStoreName": "852T",
                        "GiftECouponId": 0,
                        "SaleProductWeight": 1,
                        "WeightSubtotal": 1,
                        "BookingPickupDate": null,
                        "BookingPickupPeriod": null,
                        "IsUsingCustomPackaging": false,
                        "Id": 8272147,
                        "VerificationToken": "df3bb57eab300c61733999d7ccad6c22d2d6facce35c4bb8af14b170608462aef9468e0e10c2d3db1af5ca13d4dc222904bb24d09eccbe05e612098041fb150d"
                    }
                ],
                "IsStoreOpen": true
            }
        ],
        "PageCount": 0,
        "TotalCount": 1
    },
    "ErrorMessage": null,
    "TimeStamp": "2026-01-28T14:43:52.2718722+08:00"
}
```



## 貨運單


第三方物流商訂單管理 > 列印託運單/出貨 > 宅配 > 同樣撈取區間 1/27 15:33 – 1/28 15:32

反而在區間 1/26 15:33 – 1/27 15:32 內找到



https://store.91app.hk/Api/Delivery/GetShippingDataList


```json
{
    "ShopId": 2,
    "DateTimeType": "SalesOrderSlaveDate",
    "DateTimeStart": "2026-01-25T16:00:00.000Z",
    "DateTimeEnd": "2026-01-28T15:59:59.000Z",
    "ShippingOrderSlaveOuterCodes": [],
    "TradesOrderGroupCode": "TG260127R00042",
    "TradesOrderCode": null,
    "TradesOrderSlaveCode": null,
    "ReceiverName": null,
    "ShippingTypeDef": null,
    "TemperatureTypeDef": null,
    "ShippingOrderStatusDef": "AllocatedCode",
    "ForwarderDef": 0,
    "ReceiverPhone": null,
    "DeliveryType": 0
}
```

Data >> List >> SlaveList >> SalesOrderSlaveDateTime


## 第一層

####　DeliveryEntityMappingProfile

.ForMember(i => i.SalesOrderSlaveDateTime, s => s.MapFrom(i => i.ShippingData_SalesOrderSlaveDateTime))


#### csp

csp_GetDeliveryShippingData


- ShippingOrderSlave_SalesOrderSlaveDateTime AS ShippingData_SalesOrderSlaveDateTime,				--轉單日期(訂單成立日)
  - ShippingOrderSlave
  - #tmpPromotion
  - SalesOrderSlave
  - ShippingProfile
  - PayProfile
  - SalesOrderReceiver
  - ShippingArea
  - CountryProfile





```json
{
    "Status": "Success",
    "Data": {
        "List": [
            {
                "Master": "SF5120377132341",
                "SlaveList": [
                    {
                        "ShippingOrderId": 2495721,
                        "ShippingOrderSlaveId": 6961692,
                        "CollectDate": "2026-01-27T17:07:10.77+08:00",
                        "ExpectDeliveryArrivalDate": null,
                        "SpecifiedTimePeriod": null,
                        "SpecifiedTimePeriodDesc": "不指定",
                        "ShippingOrderSlaveOuterCode": "SF5120377132341",
                        "ShippingOrderSlaveStatusDef": "AllocatedCode",
                        "ShippingOrderSlaveStatusDefDesc": "已配號",
                        "ShippingOrderDateTime": "2026-01-27T17:07:10.77+08:00",
                        "TCatTransitCenterCode": null,
                        "TCatForwardCode": null,
                        "ForwarderDef": "SFExpress",
                        "ForwarderPostalCode": "852",
                        "ForwarderTransportCode": null,
                        "ReceiverCountryCode": "852",
                        "ReceiverZipCode": "",
                        "CountryAliasCode": "HK",
                        "ReceiverCountry": "Hong Kong",
                        "ReceiverState": "",
                        "ReceiverCity": "新界",
                        "ReceiverDistrict": "離島區",
                        "ReceiverStreet": "Lalaland 10/F",
                        "ShippingProfileTypeDef": "Home",
                        "ShippingProfileTypeDefDesc": "宅配",
                        "ReceiverStoreId": "",
                        "ReceiverStoreName": "",
                        "GiftECouponId": 0,
                        "TotalWeight": 1,
                        "ProductDeclarationName": null,
                        "BookingPickupDate": null,
                        "BookingPickupPeriod": null,
                        "IsUsingCustomPackaging": false,
                        "ShopId": 2,
                        "SalesOrderId": 2538543,
                        "SalesOrderSlaveId": 8265916,
                        "TradesOrderGroupCode": "TG260127R00042",
                        "TradesOrderCode": "TM260127R00030",
                        "TradesOrderSlaveCode": "TS260127R000130",
                        "SalesOrderSlaveDateTime": "2026-01-27T15:32:11.717+08:00",
                        "ShippingTypeDef": 1,
                        "ShippingTypeDefDesc": "一般",
                        "ExpectShippingDate": "2026-01-29T15:32:11.717+08:00",
                        "PayProfileStatisticsTypeDef": "CustomOfflinePayment",
                        "PayProfileStatisticsTypeDefDesc": "其他轉帳方式",
                        "ShippingProfileStatisticsTypeDef": "Home",
                        "ShippingProfileStatisticsTypeDefDesc": "宅配",
                        "ReceiverName": "Wong Mira",
                        "ReceiverMobile": "62207191",
                        "ReceiverAddress": "新界離島區Lalaland 10/F",
                        "DeviceAPPMappingPlatformDef": "官網(PC)",
                        "SaleProductTitle": "(不要動) Mira測試 - 好期待 $80",
                        "SkuForDisplay": "",
                        "SaleProductSkuOuterId": "ABC123",
                        "Qty": 1,
                        "Price": 80,
                        "Fee": 0,
                        "SupplierNote": null,
                        "Memo": "{CustomOrderMemo}",
                        "PromotionIds": "36751",
                        "PromotionNames": "HK滿額贈",
                        "TotalDiscount": 0,
                        "TotalPayment": 80,
                        "PromotionDiscount": 0,
                        "ECouponId": null,
                        "ECouponName": null,
                        "ECouponDiscount": 0,
                        "ECouponSlaveInfos": [],
                        "SaleProductAttribute": "主件",
                        "SalePageGiftGroupCode": "",
                        "TemperatureTypeDef": "Normal",
                        "TemperatureTypeDefDesc": "常溫",
                        "SalesOrderSlaveStatusDef": "WaitingToShipping",
                        "SalesOrderSlaveStatusDefDesc": "已成立",
                        "ShopShippingTypeName": "電子門票專用",
                        "SalePageId": 482805,
                        "IsMajor": true,
                        "IsGift": false,
                        "HasGift": true,
                        "IsSalePageGift": false,
                        "HasSalePageGift": false,
                        "SalePageGiftId": null,
                        "SalePageGiftGroupSeq": null,
                        "StatusUpdatedDateTime": "2026-01-27T17:07:10.77+08:00",
                        "PointsPayPair": null,
                        "Id": 8265916,
                        "VerificationToken": "a00e137ad575341a1b376e1b8f671436ad1e26c442704bc4ecd921e819ba028ff2cc861020de494572e955d47a9a05e37b65e1e3ccfd6f893a4150adcba06db2"
                    },
                    {
                        "ShippingOrderId": 2495721,
                        "ShippingOrderSlaveId": 6961693,
                        "CollectDate": "2026-01-27T17:07:10.77+08:00",
                        "ExpectDeliveryArrivalDate": null,
                        "SpecifiedTimePeriod": null,
                        "SpecifiedTimePeriodDesc": "不指定",
                        "ShippingOrderSlaveOuterCode": "SF5120377132341",
                        "ShippingOrderSlaveStatusDef": "AllocatedCode",
                        "ShippingOrderSlaveStatusDefDesc": "已配號",
                        "ShippingOrderDateTime": "2026-01-27T17:07:10.77+08:00",
                        "TCatTransitCenterCode": null,
                        "TCatForwardCode": null,
                        "ForwarderDef": "SFExpress",
                        "ForwarderPostalCode": "852",
                        "ForwarderTransportCode": null,
                        "ReceiverCountryCode": "852",
                        "ReceiverZipCode": "",
                        "CountryAliasCode": "HK",
                        "ReceiverCountry": "Hong Kong",
                        "ReceiverState": "",
                        "ReceiverCity": "新界",
                        "ReceiverDistrict": "離島區",
                        "ReceiverStreet": "Lalaland 10/F",
                        "ShippingProfileTypeDef": "Home",
                        "ShippingProfileTypeDefDesc": "宅配",
                        "ReceiverStoreId": "",
                        "ReceiverStoreName": "",
                        "GiftECouponId": 0,
                        "TotalWeight": 1,
                        "ProductDeclarationName": null,
                        "BookingPickupDate": null,
                        "BookingPickupPeriod": null,
                        "IsUsingCustomPackaging": false,
                        "ShopId": 2,
                        "SalesOrderId": 2538543,
                        "SalesOrderSlaveId": 8265917,
                        "TradesOrderGroupCode": "TG260127R00042",
                        "TradesOrderCode": "TM260127R00030",
                        "TradesOrderSlaveCode": "TS260127R000131",
                        "SalesOrderSlaveDateTime": "2026-01-27T15:32:11.73+08:00",
                        "ShippingTypeDef": 1,
                        "ShippingTypeDefDesc": "一般",
                        "ExpectShippingDate": "2026-01-29T15:32:11.73+08:00",
                        "PayProfileStatisticsTypeDef": "CustomOfflinePayment",
                        "PayProfileStatisticsTypeDefDesc": "其他轉帳方式",
                        "ShippingProfileStatisticsTypeDef": "Home",
                        "ShippingProfileStatisticsTypeDefDesc": "宅配",
                        "ReceiverName": "Wong Mira",
                        "ReceiverMobile": "62207191",
                        "ReceiverAddress": "新界離島區Lalaland 10/F",
                        "DeviceAPPMappingPlatformDef": "官網(PC)",
                        "SaleProductTitle": "藍藍贈品",
                        "SkuForDisplay": "",
                        "SaleProductSkuOuterId": "FREE001",
                        "Qty": 1,
                        "Price": 0,
                        "Fee": 0,
                        "SupplierNote": null,
                        "Memo": "{CustomOrderMemo}",
                        "PromotionIds": "",
                        "PromotionNames": "",
                        "TotalDiscount": 0,
                        "TotalPayment": 0,
                        "PromotionDiscount": 0,
                        "ECouponId": null,
                        "ECouponName": null,
                        "ECouponDiscount": 0,
                        "ECouponSlaveInfos": [],
                        "SaleProductAttribute": "活動贈品",
                        "SalePageGiftGroupCode": "",
                        "TemperatureTypeDef": "Normal",
                        "TemperatureTypeDefDesc": "常溫",
                        "SalesOrderSlaveStatusDef": "WaitingToShipping",
                        "SalesOrderSlaveStatusDefDesc": "已成立",
                        "ShopShippingTypeName": "電子門票專用",
                        "SalePageId": 529455,
                        "IsMajor": false,
                        "IsGift": true,
                        "HasGift": false,
                        "IsSalePageGift": false,
                        "HasSalePageGift": false,
                        "SalePageGiftId": null,
                        "SalePageGiftGroupSeq": null,
                        "StatusUpdatedDateTime": "2026-01-27T17:07:10.77+08:00",
                        "PointsPayPair": null,
                        "Id": 8265917,
                        "VerificationToken": "632a48067e66a15a19896114262c179e8f4e875db227ed4181ffc1995d9103e211ab28f7e1ced3e72f99f5424c9bd87aa786520e937713a1696e9ff339fd77bc"
                    }
                ],
                "IsStoreOpen": true
            }
        ],
        "PageCount": 0,
        "TotalCount": 1
    },
    "ErrorMessage": null,
    "TimeStamp": "2026-01-28T14:50:30.7349736+08:00"
}


```


"SalesOrderSlaveDateTime": "2026-01-27T15:32:11.717+08:00",
