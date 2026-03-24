
## 活動資訊

**(1折) 2024/11/1 (五) 11:00 ~ 13:00**
**商店**：(17)香港莎莎網店
**活動時間**：2024/11/1 (五) 11:00 ~ 13:00
**活動名稱**：HK SaSa - 限時搶購 (1折)
**商品頁序號**：289357, 289374, 361813
**商品頁是否需清快取**：否
**銷售平台**：官網/APP
**網址**：https://www.sasa.com.hk/
**導流來源**：Ads／FB / eDM / App push / whatsapp/ Omnichat/ SMS

<br>
<br>

## 配置

- ASG 機器 c6i.xlarge * ,  c6i.xlarge * 8（ASG * 6 + 寵物機 * 2）

<br>
<br>

## 關 PageView

**RedisKey**: Cache:Prod:WebAPI:CircuitBreakerService:PageView-20200714:EnableAddPV-en-US
Cache:Prod:WebAPI:CircuitBreakerService:PageView-20200714:EnableAddPV-zh-HK
**過期時間** : 2024-11-01 13:00

<br>
<br>

## By SKU 一秒內同時加入購物車數量調整

**value** : 10 -> 2
**RedisKey**:
Cache:Prod:WebAPI:CircuitBreakerService:ExceededTimesLimiterSettings-20200826:ShoppingCart-InsertItem-SaleProductSKUId-en-US
Cache:Prod:WebAPI:CircuitBreakerService:ExceededTimesLimiterSettings-20200826:ShoppingCart-InsertItem-SaleProductSKUId-zh-HK
**過期時間**: 2024-11-01 13:00

<br>
<br>

## 監控

- rps

<br>
<br>

## 查庫存

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
WHERE SaleProductSKU_SalePageId IN (289357, 289374, 361813)
```

<br>
<br>

## Slack

https://91app.slack.com/archives/G06A3GDC7/p1730428201725119