# Shipment Query 查詢語法

## 目錄
1. [ShippingOrder 出貨單查詢](#1-shippingorder-出貨單查詢)
2. [ShipTracing 物流追蹤查詢](#2-shiptracing-物流追蹤查詢)

<br>

---

## 1. ShippingOrder 出貨單查詢

```sql
use ERPDB

select ShippingOrder_ForwarderDef,ShippingOrder_Id,ShippingOrder_Code,*

from ShippingOrder (nolock)
where ShippingOrder_ValidFlag = 1
and ShippingOrder_TradesOrderGroupCode = 'TG231205Q00069'
--and ShippingOrder_Id  = '1525276';
```

<br>

### 出貨時間欄位

ShippingOrderSlave_UpdatedDateTime

<br>

範例：2024-01-27 16:38:20.897

<br>

---

## 2. ShipTracing 物流追蹤查詢

```sql
select ShipmentTracing_ShippingOrderId,ShipmentTracing_DateTime AS Tracing_DateTime,ShipmentTracing_StatusDef,*
from ShipmentTracing(nolock)
where ShipmentTracing_ShippingOrderId = '1545090'
order by ShipmentTracing_DateTime desc;
```

<br>

---