# PublishableKey — 位置查詢

## Step 1：確認帳戶類型

```sql
select ShopDefault_ShopId, ShopDefault_GroupTypeDef, ShopDefault_Key, ShopDefault_NewValue, *
from ShopDefault(nolock)
where ShopDefault_ValidFlag = 1
and ShopDefault_ShopId = 125  -- 幾號店???
and ShopDefault_GroupTypeDef = 'Stripe'
```

## Step 2：根據帳戶類型找出 pk

```sql
DECLARE @secretKey VARCHAR(50) = '', -- {帳戶類型}PublishableKey 例如: StandardUATPublishableKey
        @shopId BIGINT = ;           -- 商店 ID

select ShopSecret_ShopId, ShopSecret_Key, ShopSecret_Value, *
from ShopSecret(nolock)
where ShopSecret_GroupName = 'Stripe'
and ShopSecret_ShopId = @shopId
and ShopSecret_Key = @secretKey
and ShopSecret_ValidFlag = 1
```

## 帳戶類型說明

| 帳戶類型 | 說明 |
|----------|------|
| **Standard** | 會根據商店有所不同，使用連結帳戶的 pk + acct |
| **Custom** | shopId = 0，只會帶一把 |
