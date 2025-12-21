









## 開關

ShopStaticSetting
- ShopStaticSetting_GroupName = 'RazerPay'
- ShopStaticSetting_Key = 'IsFiuuEnable'
- ShopStaticSetting_ShopId IN (0, 200071, 200136)



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