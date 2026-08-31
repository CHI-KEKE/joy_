
## PayProfile


```sql
-- 查詢 GrabPay_AsiaPay 的 Definition 設定
SELECT Definition_Desc, Definition_Code, *
FROM Definition (NOLOCK)
WHERE Definition_TableName = 'PayProfile'
    AND Definition_ColumnName = 'PayProfile_TypeDef'
    AND Definition_Code = 'GrabPay_AsiaPay' -- TNG_Razer
```


## ShopPayType

```sql
USE WebStoreDB

-- 查詢所有有效的 ShopPayType Definition
SELECT *
FROM Definition (NOLOCK)
WHERE Definition_ValidFlag = 1
    AND Definition_TableName = 'ShopPayType'
    AND Definition_ColumnName = 'ShopPayType_TypeDef'
```


## SalesOrderThirdPartyPayment


```sql
USE ERPDB

-- 查詢第三方支付狀態 Definition
SELECT *
FROM dbo.Definition (NOLOCK)
WHERE Definition_ColumnName = 'SalesOrderThirdPartyPayment_StatusDef'
    -- AND Definition_TableName = 'SalesOrderThirdPartyPayment'
    -- AND Definition_Code = 'TwoCTwoP'
    -- Definition_Note = N'第三方支付類型'
```

<br>

## RefundRequest_TypeDef

```sql
USE ERPDB

-- 查詢 GrabPay_AsiaPay 退款類型設定
SELECT *
FROM Definition (NOLOCK)
WHERE Definition_ValidFlag = 1
    AND Definition_ColumnName = 'RefundRequest_TypeDef'
    AND Definition_TableName = 'RefundRequest'
    AND Definition_Code = 'GrabPay_AsiaPay'
```