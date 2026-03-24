


MG260204P00004
"Id": 16434


orderSlave 裡面有 ShopShippingTypeId => 可以拿到 ShopShippingType_FeeTypeDef(運費類型)

- Fixed 固定運費
- Free 免運費
- OverPrice 滿額免運
- PriceRule 多階運費
- WeightBilling 重量計費



整理部分Qty取消資訊，調整Qty數量、總價格

```csharp
//// 實際Qty為一開始購買的Qty 扣除 取消的Qty，退貨的Qty
entity.Qty = entity.Qty - entity.OrderSlaveFlowCancelQty - entity.OrderSlaveFlowReturnGoodsQty;

//// 補退單為正數，取消金額、退款金額為負數，扣除之後便為實際付款金額
entity.TotalPayment = entity.TotalPayment - entity.OrderSlaveFlowCancelTotalPayment - entity.OrderSlaveFlowReturnGoodsTotalPayment +
                        entity.OrderSlaveFlowOrderSpreadsAmount; ////補退金額
```



detail.IsPartialReturnGoodsEnabled : 商店開放部分退貨
