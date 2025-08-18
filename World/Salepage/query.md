# Query 查詢語法

## 目錄
1. [庫存查詢](#1-庫存查詢)
2. [商品付款方式](#2-商品付款方式)
3. [商品內容](#3-商品內容)

<br>

---

## 1. 庫存查詢

```sql
USE WebStoreDB
GO

SELECT
	SaleProductSKU_SalePageId AS N'商品頁序號'
   ,SalePage_Title AS N'商品名稱'
   ,ProductStock_TotalQty AS N'總量(上架總量)'
   ,ProductStock_RegQty AS N'註冊量(總訂單量包含被取消)'
   ,ProductStock_CancelQty AS N'取消量'
   ,ProductStock_SellingQty AS N'可售數量'
FROM dbo.SaleProductSKU WITH (NOLOCK)
INNER JOIN dbo.ProductStock WITH (NOLOCK)
	ON SaleProductSKU_Id = ProductStock_SaleProductSKUId
		AND ProductStock_ValidFlag = 1
		AND SaleProductSKU_ValidFlag = 1
INNER JOIN dbo.SalePage WITH (NOLOCK)
	ON SaleProductSKU_SalePageId = SalePage_Id
		AND SalePage_ValidFlag = 1
WHERE SaleProductSKU_SalePageId IN (61482)
```

<br>

---

## 2. 商品付款方式

```sql
select top 1 SalePagePayType_TypeDef,SalePagePayType_PayProfileTypeDef,*
from SalePagePayType(nolock)
```

<br>

---

## 3. 商品內容

```sql
SELECT SalePage_Title,SalePage_SubTitle,SalePage_Price,*
FROM SalePage(NOLOCK)
WHERE SalePage_Id = 61482
```

<br>

---