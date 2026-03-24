```sql
USE WebStoreDB


SELECT Supplier_SalesMarket,*
FROM Supplier(NOLOCK)

-- 200100, 200099

SELECT *
FROM Shop(NOLOCK)
WHERE Shop_ValidFlag = 1
AND Shop_SupplierId IN (200100, 200099)
```