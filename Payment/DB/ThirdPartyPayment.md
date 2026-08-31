```sql
USE WebStoreDB

	SELECT *
		FROM dbo.TradesOrderThirdPartyPayment WITH (NOLOCK)
		WHERE TradesOrderThirdPartyPayment_ValidFlag = 1
		AND TradesOrderThirdPartyPayment_TradesOrderGroupCode = 'TG250820W00092'
		AND TradesOrderThirdPartyPayment_ValidFlag = 1
```

```sql
USE WebStoreDB

declare @startTime DATETIME = '2025-01-01';
declare @endTime DATETIME = '2025-05-01';

	SELECT *
		FROM dbo.TradesOrderThirdPartyPayment WITH (NOLOCK)
		WHERE TradesOrderThirdPartyPayment_ValidFlag = 1
		AND TradesOrderThirdPartyPayment_TradesOrderGroupCode = 'TG250626K00002'
		and TradesOrderThirdPartyPayment_TypeDef IN ('CreditCardOnce_Stripe','CreditCardOnce_CheckoutDotCom','EWallet_PayMe','AliPayHK_EftPay','WechatPayHK_EftPay','BoCPay_SwiftPass','UnionPay_EftPay','Atome','TwoCTwoP','QFPay','CreditCardOnce_Cybersource')
		AND TradesOrderThirdPartyPayment_StatusDef IN('New', 'WaitingToPay')
		AND TradesOrderThirdPartyPayment_StatusUpdatedDateTime > @startTime
		AND TradesOrderThirdPartyPayment_StatusUpdatedDateTime <= @endTime
		AND TradesOrderThirdPartyPayment_ValidFlag = 1
```


## 付款失敗 memberuid / 卡別國別

```sql
use WebStoreDB;

with TopFailMembers as
(
    select top (10)
        tog.TradesOrderGroup_MemberId,
        count(*) as FailCount
    from TradesOrderThirdPartyPayment as ttp with (nolock)
    inner join TradesOrderGroup as tog with (nolock)
        on ttp.TradesOrderThirdPartyPayment_TradesOrderGroupId = tog.TradesOrderGroup_Id
    where ttp.TradesOrderThirdPartyPayment_ValidFlag = 1
      and ttp.TradesOrderThirdPartyPayment_StatusDef = 'Fail'
    group by tog.TradesOrderGroup_MemberId
    order by count(*) desc
)
select
    vmi.VipMemberInfo_ShopId,
    tfm.TradesOrderGroup_MemberId,
    vmi.VipMemberInfo_CountryCode,
    vmi.VipMemberInfo_CellPhone,
    vmi.VipMemberInfo_FullName,
    tfm.FailCount
from TopFailMembers as tfm
inner join VipMemberInfo as vmi with (nolock)
    on tfm.TradesOrderGroup_MemberId = vmi.VipMemberInfo_MemberId
order by tfm.FailCount desc;
```