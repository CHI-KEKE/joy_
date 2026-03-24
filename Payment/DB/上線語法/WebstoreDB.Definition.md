

```sql
-- Start: Verify
SELECT *
FROM WebStoreDB.dbo.Definition WITH(NOLOCK)
WHERE
	Definition_Code in( 'PayNow_Razer','CreditCardOnce_Razer')
	AND Definition_ColumnName IN 
	(
		'PayProfile_TypeDef', 'PayProfile_StatisticsTypeDef', 'SalePagePayType_TypeDef', 
		'ShopPayType_TypeDef', 'SupplierPayType_TypeDef', 'TradesOrderThirdPartyPayment_TypeDef'
	)
ORDER BY Definition_TableName, Definition_ColumnName
-- End: Verify
```