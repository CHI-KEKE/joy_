## Fiuu 文件

https://91app.slack.com/archives/G019P1APXMY/p1758703072649869?thread_ts=1758698124.491279&cid=G019P1APXMY

file:///C:/Users/Allen%20Lin/Downloads/[official%20API]%20Fiuu%20API%20Spec%20for%20Merchant%20(v13.90)%20(5).pdf




## 開關

```bash
ShopStaticSetting
- ShopStaticSetting_GroupName = 'RazerPay'
- ShopStaticSetting_Key = 'IsFiuuEnable'
- ShopStaticSetting_ShopId IN (0, 200071, 200136)
```



## 語法

```sql
-- select
SELECT *
FROM
    dbo.ShopStaticSetting WITH (NOLOCK)
WHERE
    ShopStaticSetting_ValidFlag = 1
    AND ShopStaticSetting_GroupName = 'RazerPay'
    AND ShopStaticSetting_Key = 'IsFiuuEnable'
    AND ShopStaticSetting_ShopId IN (0, 200071, 200136)
ORDER BY
    ShopStaticSetting_ShopId;

-- insert setting
INSERT dbo.ShopStaticSetting
    (
    ShopStaticSetting_ShopId
    , ShopStaticSetting_GroupName
    , ShopStaticSetting_Key
    , ShopStaticSetting_Value
    , ShopStaticSetting_Description
    , ShopStaticSetting_CreatedUser
    , ShopStaticSetting_UpdatedUser
    )
VALUES
    (
        0
        , 'RazerPay'
        , 'IsFiuuEnable'
        , 'false'
        , 'Toggle Fiuu payment integration'
        , 'VSTS544811'
        , 'VSTS544811'
    )
  ,
    (
        200071 -- 馬幣站
        , 'RazerPay'
        , 'IsFiuuEnable'
        , 'true'
        , 'Toggle Fiuu payment integration'
        , 'VSTS544811'
        , 'VSTS544811'
    )
  ,
    (
        200136 -- 星幣站
        , 'RazerPay'
        , 'IsFiuuEnable'
        , 'true'
        , 'Toggle Fiuu payment integration'
        , 'VSTS544811'
        , 'VSTS544811'
    );

-- verify
SELECT *
FROM
    dbo.ShopStaticSetting WITH (NOLOCK)
WHERE
    ShopStaticSetting_ValidFlag = 1
    AND ShopStaticSetting_GroupName = 'RazerPay'
    AND ShopStaticSetting_Key = 'IsFiuuEnable'
    AND ShopStaticSetting_ShopId IN (0, 200071, 200136)
ORDER BY
    ShopStaticSetting_ShopId;
```