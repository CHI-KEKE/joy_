


SubmitOrder => PayOrder


## SubmitOrder

payload


```json
{
    "CollectDate": "2025/12/15",
    "Content": "erteyrut",
    "ServiceCode": "EP-CS0NO",
    "Value": 10,
    "Weight": 0.001,
    "PackingList": [
        {
            "TradesOrderSlaveCode": "MS251201M000003",
            "SalesOrderSlaveId": 23575,
            "ShippingType": 0
        }
    ],
    "SalesOrderId": "15278",
    "ShopId": 83,
    "TradesOrderCode": "MM251201M00002"
}
```


salesOrderSlaveIds => 建立 ShippingOrderSlave


## ShippingOrder

### 範例資料

| 欄位 | 值 | 說明 |
|------|-----|------|
| ShippingOrder_Id | 3239 | 物流單編號 |
| ShippingOrder_Code | SO251215L00001 | 物流單代碼 |
| ShippingOrder_SourceDef | SalesOrder | 來源定義 |
| ShippingOrder_SourceId | 0 | 來源編號 |
| ShippingOrder_TypeDef | Home | 配送類型(宅配) |
| ShippingOrder_ForwarderDef | 9 | 物流商定義(EasyParcel) |
| ShippingOrder_SupplierId | 79 | 供應商編號 |
| ShippingOrder_Fee | 0.00 | 運費 |
| ShippingOrder_Cost | 0.00 | 成本 |
| ShippingOrder_DateTime | 2025-12-15 10:06:12.160 | 物流單建立時間 |
| ShippingOrder_DeliveryDateTime | 2025-12-15 10:06:12.160 | 預計配送時間 |
| ShippingOrder_Note | NULL | 備註 |
| ShippingOrder_CreatedDateTime | 2025-12-15 10:06:12.587 | 建立時間 |
| ShippingOrder_CreatedUser | allenlin@nine-yi.com | 建立使用者 |
| ShippingOrder_UpdatedTimes | 0 | 更新次數 |
| ShippingOrder_SalesOrderFeeId | 15278 | 訂單運費編號 |
| ShippingOrder_SalesOrderFeeFee | 65.00 | 訂單運費金額 |
| ShippingOrder_CreatedSource | SCMAPI | 建立來源 |
| ShippingOrder_GoodsToLogisticCenterDate | NULL | 商品到物流中心日期 |
| ShippingOrder_SuggestGoodsArrivalDate | NULL | 建議商品到貨日期 |
| ShippingOrder_TransportTypeDef | NULL | 運輸類型定義 |
| ShippingOrder_SalesOrderId | 15278 | 訂單編號 |
| ShippingOrder_TradesOrderCode | MM251201M00002 | 交易訂單代碼 |
| ShippingOrder_SalesOrderMemo | (空字串) | 訂單備註 |
| ShippingOrder_DeviceAPPMappingPlatformDef | 官網(PC) | 裝置平台定義 |
| ShippingOrder_TradesOrderGroupCode | MG251201M00002 | 交易訂單群組代碼 |
| ShippingOrder_MemberId | 98000050 | 會員編號 |
| ShippingOrder_VipMemberInfoFullName | NULL | 會員姓名 |
| ShippingOrder_VipMemberInfoCellPhone | 912345678 | 會員手機 |
| ShippingOrder_VipMemberInfoEmail | G@GMIL.COM | 會員信箱 |
| ShippingOrder_LocationId | 0 | 地點編號 |
| ShippingOrder_AvailablePickupDays | NULL | 可取貨天數 |
| ShippingOrder_TotalPayment | 10.00 | 總付款金額 |
| ShippingOrder_IsTracked | NULL | 是否已追蹤 |
| ShippingOrder_ExpectDeliveryArrivalDate | NULL | 預計配送到貨日期 |
| ShippingOrder_SpecifiedTimePeriod | NULL | 指定時段 |
| ShippingOrder_ForwarderPostalCode | (空字串) | 物流商郵遞區號 |
| ShippingOrder_ForwarderTransportCode | (空字串) | 物流商運輸代碼 |
| ShippingOrder_ExpectDeliveryTimePeriod | (空字串) | 預計配送時段 |
| ShippingOrder_StatusCode | NULL | 狀態代碼 |
| ShippingOrder_StatusCause | NULL | 狀態原因 |
| ShippingOrder_StorePaymentAmount | NULL | 店家付款金額 |


## ShippingOrderSlave

### 範例資料

| 欄位 | 值 | 說明 |
|------|-----|------|
| ShippingOrderSlave_ShippingOrderId | 3239 | 物流單編號 |
| ShippingOrderSlave_SalesOrderSlaveId | 23575 | 訂單明細編號 |
| ShippingOrderSlave_TradesOrderSlaveId | 24580 | 交易訂單明細編號 |
| ShippingOrderSlave_TradesOrderSlaveCode | MS251201M000003 | 交易訂單明細代碼 |
| ShippingOrderSlave_SupplierId | 79 | 供應商編號 |
| ShippingOrderSlave_OuterCode | EI-0FPGE | 外部代碼 |
| ShippingOrderSlave_GoodsSKUSupplierOuterId | NULL | 商品SKU供應商外部編號 |
| ShippingOrderSlave_GoodsId | 0 | 商品編號 |
| ShippingOrderSlave_GoodsSKUId | 0 | 商品SKU編號 |
| ShippingOrderSlave_GoodsSKUSupplierId | 0 | 商品SKU供應商編號 |
| ShippingOrderSlave_SaleProductTitle | aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa | 銷售商品標題 |
| ShippingOrderSlave_SaleProductSKUPropertyNameSet | -1:-1:-1:-1 | 銷售商品SKU屬性名稱集合 |
| ShippingOrderSlave_Qty | 1 | 數量 |
| ShippingOrderSlave_Cost | 0.00 | 成本 |
| ShippingOrderSlave_Price | 10.00 | 價格 |
| ShippingOrderSlave_ExpectDateTime | 2025-12-03 11:40:36.560 | 預計時間 |
| ShippingOrderSlave_PickupContacter | NULL | 取貨聯絡人 |
| ShippingOrderSlave_PickupPhone | NULL | 取貨電話 |
| ShippingOrderSlave_PickupAddress | NULL | 取貨地址 |
| ShippingOrderSlave_PickupCity | NULL | 取貨城市 |
| ShippingOrderSlave_PickupCountry | NULL | 取貨國家 |
| ShippingOrderSlave_PickupState | NULL | 取貨州/省 |
| ShippingOrderSlave_PickupDistrict | NULL | 取貨地區 |
| ShippingOrderSlave_PickupZipCode | NULL | 取貨郵遞區號 |
| ShippingOrderSlave_ReceiverName | AllenLinGGGG | 收件人姓名 |
| ShippingOrderSlave_ReceiverPhone | 88888888 | 收件人電話 |
| ShippingOrderSlave_ReceiverAddress | ADDDDDRESSS | 收件人地址 |
| ShippingOrderSlave_ReceiverCity | (空字串) | 收件人城市 |
| ShippingOrderSlave_ReceiverCountry | Singapore | 收件人國家 |
| ShippingOrderSlave_ReceiverState | (空字串) | 收件人州/省 |
| ShippingOrderSlave_ReceiverDistrict | (空字串) | 收件人地區 |
| ShippingOrderSlave_ReceiverZipCode | (空白) | 收件人郵遞區號 |
| ShippingOrderSlave_StatusDef | Finish | 狀態定義 |
| ShippingOrderSlave_StatusCasue | NULL | 狀態原因 |
| ShippingOrderSlave_StatusUpdatedUser | unlogin | 狀態更新使用者 |
| ShippingOrderSlave_StatusUpdatedDateTime | 2025-12-15 10:06:12.160 | 狀態更新時間 |
| ShippingOrderSlave_IsClosed | 1 | 是否關閉 |
| ShippingOrderSlave_StoreId | NULL | 商店編號 |
| ShippingOrderSlave_StoreName | NULL | 商店名稱 |
| ShippingOrderSlave_DateTime | 2025-12-15 10:06:12.160 | 時間 |
| ShippingOrderSlave_DataTransferDateTime | NULL | 資料轉移時間 |
| ShippingOrderSlave_ShippingConfirmDateTime | 2025-12-15 10:06:12.160 | 出貨確認時間 |
| ShippingOrderSlave_HandOverDateTime | NULL | 交接時間 |
| ShippingOrderSlave_EntryAccountingDateTime | 2025-12-15 10:06:12.160 | 入帳時間 |
| ShippingOrderSlave_DistributorDef | NULL | 經銷商定義 |
| ShippingOrderSlave_ShopId | 83 | 商店編號 |
| ShippingOrderSlave_SalePageId | 6121 | 銷售頁編號 |
| ShippingOrderSlave_SaleProductId | 6091 | 銷售商品編號 |
| ShippingOrderSlave_SaleProductSKUId | 8130 | 銷售商品SKU編號 |
| ShippingOrderSlave_CreatedSource | SCMAPI | 建立來源 |
| ShippingOrderSlave_TotalDiscount | 0.00 | 總折扣 |
| ShippingOrderSlave_TotalPayment | 10.00 | 總付款金額 |
| ShippingOrderSlave_TransportTypeDef | NULL | 運輸類型定義 |
| ShippingOrderSlave_FlowTypeDef | NULL | 流程類型定義 |
| ShippingOrderSalve_ReverseStatusDef | NULL | 逆向狀態定義 |
| ShippingOrderSalve_ReverseUpdateDateTime | NULL | 逆向更新時間 |
| ShippingOrderSalve_ReverseStoreName | NULL | 逆向商店名稱 |
| ShippingOrderSalve_PrintCode | NULL | 列印代碼 |
| ShippingOrderSalve_SevenElevenShipmentNo | NULL | 7-11 出貨編號 |
| ShippingOrderSalve_MasterCode | NULL | 主代碼 |
| ShippingOrderSlave_TemperatureDeliveryTicketReservedNo | NULL | 溫控配送票券保留號碼 |
| ShippingOrderSlave_TemperatureTypeDef | Normal | 溫度類型定義 |
| ShippingOrderSlave_TemperatureProductDCShippingDate | NULL | 溫控商品物流中心出貨日期 |
| ShippingOrderSlave_SalesOrderSlaveDateTime | 2025-12-01 11:40:36.560 | 訂單明細時間 |
| ShippingOrderSlave_SalesOrderSlaveExpectShippingDate | 2025-12-03 11:40:36.560 | 訂單明細預計出貨日期 |
| ShippingOrderSlave_ShippingTypeDef | 1 | 物流類型定義 |
| ShippingOrderSlave_ShippingDate | NULL | 出貨日期 |
| ShippingOrderSlave_ShippingWaitingDays | NULL | 出貨等待天數 |
| ShippingOrderSlave_SaleProductLength | 1 | 銷售商品長度 |
| ShippingOrderSlave_SaleProductWidth | 1 | 銷售商品寬度 |
| ShippingOrderSlave_SaleProductHeight | 1 | 銷售商品高度 |
| ShippingOrderSlave_SaleProductWeight | 1 | 銷售商品重量 |
| ShippingOrderSlave_PayTypeDef | CreditCardOnce_Razer | 付款類型定義 |
| ShippingOrderSlave_InstallmentDef | NULL | 分期定義 |
| ShippingOrderSlave_PromotionDiscount | 0.00 | 活動折扣 |
| ShippingOrderSlave_ECouponDiscount | 0.00 | 電子票券折扣 |
| ShippingOrderSlave_SupplierNote | NULL | 供應商備註 |
| ShippingOrderSlave_IsMajor | 1 | 是否為主要商品 |
| ShippingOrderSlave_IsGift | 0 | 是否為贈品 |
| ShippingOrderSlave_HasGift | 0 | 是否有贈品 |
| ShippingOrderSlave_IsSalePageGift | 0 | 是否為活動頁贈品 |
| ShippingOrderSlave_HasSalePageGift | 0 | 是否有活動頁贈品 |
| ShippingOrderSlave_SalePageGiftId | NULL | 活動頁贈品編號 |
| ShippingOrderSlave_SalePageGiftGroupSeq | NULL | 活動頁贈品群組序號 |
| ShippingOrderSlave_PayProfileTypeDef | CreditCardOnce_Razer | 付款設定類型定義 |
| ShippingOrderSlave_ShippingProfileTypeDef | Home | 物流設定類型定義 |
| ShippingOrderSlave_CurrentShippingAmount | 0.00 | 目前運費金額 |
| ShippingOrderSlave_ProductDeclarationName | NULL | 商品申報名稱 |
| ShippingOrderSlave_StorePaymentAmount | NULL | 商店付款金額 |




##　shipmentStatus


https://sms.qa1.my.91dev.tw/Api/GlobalShipping/GetShipmentStatus

#### request

```json
{
    "ShippingOrderCode": "SO251216P00001",
    "ShopId": 83
}

```

#### SMS response

```json
{
    "OuterCode": "TP034389224834730SG",
    "TradesOrderGroupCode": "MG251216P00001",
    "Info": {
        "BillLink": "https://demo.connect.easyparcel.sg/?ac=AWBLabel&id=RVAtMjNEd3FMN0llIzM4Nzk0MA%3D%3D"
    }
}
```


#### Easy Response

```json
{
  "access": "Success",
  "result": [
    {
      "Parcel_Number": "EP-NIB9C",
      "Ship_Status": "Schedule In Arrangement",
      "Airway-Bill": "TP559069090784186SG",
      "Airway-Bill-Link": "https:\/\/demo.connect.easyparcel.sg\/?ac=AWBLabel&id=RVAtMjNEd3FMN0llIzM4NzkzNg%3D%3D"
    }
  ],
  "status": "Success",
  "remarks": "Correct Order_no",
  "order_no": "EI-0FPMY",
  "api_status": "Success",
  "error_code": "0",
  "error_remark": ""
}
```


用 ShippingOrderSlave_OuterCode 去換 responseObj.Result.First().AirwayBill


討論關於及時拿到貨態的問題，出貨當下拿到單號會有延遲











## 訂單要呈現貨運追蹤碼不是 easy EI 碼


https://mytestmina.shop.qa1.my.91dev.tw/webapi/MemberTradesOrder/GetList?shopId=83&startIndex=5&maxCount=5&lang=zh-TW


"CurrentStatusQueryUrl": "/V2/TradesOrder/RedirectToEasyParcel?orderNo=EI-0FPGE&shopId=83"

"OrderSlaveFlowShippingOrderSlaveOuterCode": "EI-0FPGE"


