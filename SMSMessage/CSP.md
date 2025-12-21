


configdb

ShortMessage.AreaType.UseSalesMarket



TRUE / FALSE



csp_TransShortMessageDM



cfn_GetSalesMarketSettings


## 塞開關

- ShortMessage.AreaType.UseSalesMarket.Enable
- true / false

```sql
use ConfigDB

select *
from AppSetting(nolock)
where AppSetting_ValidFlag = 1
and AppSetting_Key = 'ShortMessage.AreaType.UseSalesMarket.Enable'

INSERT INTO AppSetting
(
    AppSetting_Key,
    AppSetting_Value,
    AppSetting_CreatedDateTime,
    AppSetting_CreatedUser,
    AppSetting_UpdatedTimes,
    AppSetting_UpdatedDateTime,
    AppSetting_UpdatedUser,
    AppSetting_ValidFlag,
    AppSetting_Description
)
VALUES
(
    'ShortMessage.AreaType.UseSalesMarket.Enable',
    'false',
    GETDATE(),
    'VSTS548387',
    0,
    GETDATE(),
    'VSTS548387',
    1,
    N'銷售市場區分國內外簡訊類型'
);

select *
from AppSetting(nolock)
where AppSetting_ValidFlag = 1
and AppSetting_Key = 'ShortMessage.AreaType.UseSalesMarket.Enable'
```




NineYiDW_TransDataFromNMQV2DB => csp: [csp_TransShortMessageDM]