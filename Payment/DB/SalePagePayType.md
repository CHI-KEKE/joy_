```sql
use WebStoreDB

DECLARE @payType VARCHAR(30) = 'ApplePay';

SELECT SalePagePayType_SalePageId AS N'商品序號',
       SalePage_Title,
       SalePage_ShopId AS N'商店序號',
       SalePagePayType_CreatedDateTime AS N'金流套用時間',
       *
FROM SalePagePayType(nolock)
INNER JOIN SalePage(nolock)
    ON SalePagePayType_SalePageId = SalePage_Id
WHERE SalePagePayType_ValidFlag = 1
AND SalePage_ValidFlag = 1
AND SalePagePayType_TypeDef = 'ApplePay'

select *
from ShopCategory(nolock)
where ShopCategory_ValidFlag = 1
and ShopCategory_ShopId = 2
order by ShopCategory_CreatedDateTime desc
```