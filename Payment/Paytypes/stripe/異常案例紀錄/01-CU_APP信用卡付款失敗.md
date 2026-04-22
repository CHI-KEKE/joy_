# 異常案例 01 — 特定消費者無法完成信用卡付款（CU APP）

### 1.1 特定消費者無法完成信用卡付款 - CU APP

#### 基本資訊
- **VSTS**：https://91appinc.visualstudio.com/DailyResource/_workitems/edit/512563
- **客戶序號**：25
- **商店序號**：28
- **商店名稱**：CU APP

#### 問題概述
消費者使用 HSBC MasterCard 付款失敗，系統顯示「暫不支援此信用卡，請更換信用卡重新結帳」。該卡在其他平台使用正常，但在我們平台重複嘗試都失敗。

#### 異常資訊
| 項目 | 詳情 |
|------|------|
| 會員電話 | +852-91832120 |
| 訂單時間 | 2025/07/16 上午 11:00-12:00 |
| 信用卡類型 | HSBC MasterCard |
| 驗證方式 | 跳轉至 HSBC 銀行 App 授權 |

#### 調查步驟

##### 1.1.1 確認會員資料
```sql
USE WebStoreDB

SELECT *
FROM VipMemberInfo(NOLOCK)
WHERE VipMemberInfo_ValidFlag = 1
  AND VipMemberInfo_CellPhone = '91832120'
  AND VipMemberInfo_CountryCode = 852
  AND VipMemberInfo_ShopId = 28
```
![alt text](../Img/image.png)

##### 1.1.2 查詢第三方支付紀錄
```sql
SELECT *
FROM TradesOrderThirdPartyPayment(NOLOCK)
WHERE TradesOrderThirdPartyPayment_ValidFlag = 1
  AND TradesOrderThirdPartyPayment_ShopId = 28
  AND TradesOrderThirdPartyPayment_TypeDef = 'CreditCardOnce_Stripe'
  AND TradesOrderThirdPartyPayment_CreatedDateTime >= '2025/07/16 00:00'
  AND TradesOrderThirdPartyPayment_CreatedDateTime <= '2025/07/17 00:00'
  AND TradesOrderThirdPartyPayment_CreatedUser = '1751836' -- MemberId
```
![alt text](../Img/image-1.png)

##### 1.1.3 Stripe 後台查詢
![alt text](../Img/image-2.png)
![alt text](../Img/image-3.png)
![alt text](../Img/image-5.png)

##### 1.1.4 信用卡驗證 Log 查詢
```sql
SELECT * FROM "hk_prod_webstore"."webstore_web_iislog"
WHERE date = '2025/07/16'
  AND cs_uri_stem = '/webapi/CreditCard/Validate'
  AND cs_uri_query LIKE '%ShopId=83%';
```
![alt text](../Img/image-4.png)

##### 1.1.5 確認記住信用卡狀態
```sql
SELECT *
FROM PayTypeExpress(nolock)
WHERE PayTypeExpress_ValidFlag = 1
  AND PayTypeExpress_ShopId = 28
  AND PayTypeExpress_MemberId = 1751836
```

#### 分析結果
- 實際交易時間：10:00
- 卡片到期日：2032/01（尚未到期）
- Stripe 系統認定卡片過期
- **後續處理**：需聯繫 Stripe 釐清該交易問題

<br>
