```sql
SELECT
    TradesOrderSlave_Id AS 'TsId',
    TradesOrderSlave_Code AS 'TsCode',
    TradesOrderSlave_SalePageId AS 'SalepageId',
    TradesOrderSlave_SaleProductSKUId AS 'SkuId',
    TradesOrderSlaveMapping_SalePageId AS 'MainSalepageId',
    TradesOrderSlaveMapping_SaleProductSKUId AS 'MainSkuId',
    TradesOrderSlaveMapping_ItemGroup AS 'ItemGroup',
	TradesOrderSlavePromotionEngine_TypeDef
FROM dbo.TradesOrderGroup WITH (NOLOCK)
JOIN dbo.OrderSlaveFlow WITH (NOLOCK) ON TradesOrderGroup_Id = OrderSlaveFlow_TradesOrderGroupId AND OrderSlaveFlow_ValidFlag = 1
JOIN dbo.TradesOrderSlave WITH (NOLOCK) ON OrderSlaveFlow_TradesOrderSlaveCode = TradesOrderSlave_Code AND TradesOrderSlave_ValidFlag = 1
LEFT JOIN dbo.TradesOrderSlaveMapping WITH (NOLOCK) ON TradesOrderGroup_Id = TradesOrderSlaveMapping_TradesOrderGroupId
                                                           AND TradesOrderGroup_ShopId = TradesOrderSlaveMapping_ShopId
                                                           AND TradesOrderSlave_TradesOrderId = TradesOrderSlaveMapping_TradesOrderId
                                                           AND TradesOrderSlave_ItemGroup = TradesOrderSlaveMapping_ItemGroup
														   AND TradesOrderSlaveMapping_ValidFlag = 1
LEFT JOIN dbo.TradesOrderSlavePromotionEngine WITH (NOLOCK) ON TradesOrderSlavePromotionEngine_TradesOrderSlaveId = TradesOrderSlave_Id
WHERE TradesOrderGroup_ValidFlag = 1
  AND TradesOrderGroup_Code IN ('TG260513RA00Y5');
```