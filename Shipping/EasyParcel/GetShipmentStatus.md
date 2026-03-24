
## GlobalShipping API

https://sms.qa1.my.91dev.tw/Api/GlobalShipping/GetShipmentStatus

#### Request

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


{
  "access": "Success",
  "result": [
    {
      "Parcel_Number": "EP-NIBG6",
      "Ship_Status": "Schedule In Arrangement",
      "Airway-Bill": "Awb not available",
      "Airway-Bill-Link": ""
    }
  ],
  "status": "Success",
  "remarks": "Correct Order_no",
  "order_no": "EI-0FPRR",
  "api_status": "Success",
  "error_code": "0",
  "error_remark": ""
}
```


用 ShippingOrderSlave_OuterCode 去換 responseObj.Result.First().AirwayBill，討論關於及時拿到貨態的問題，出貨當下拿到單號會有延遲