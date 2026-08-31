use WebStoreDB


select TradesOrderThirdPartyPayment_CreatedDateTime,TradesOrderThirdPartyPayment_ResponseMsg,TradesOrderThirdPartyPayment_UpdatedUser,Shop_Name,Shop_Id,TradesOrderThirdPartyPayment_TypeDef,TradesOrderThirdPartyPayment_DateTime,TradesOrderThirdPartyPayment_StatusDef,TradesOrderThirdPartyPayment_ShopId,TradesOrderThirdPartyPayment_TradesOrderGroupCode,*
from TradesOrderThirdPartyPayment(nolock)
inner join Shop(nolock)
on TradesOrderThirdPartyPayment_ShopId = Shop_Id
where TradesOrderThirdPartyPayment_ValidFlag = 1
and TradesOrderThirdPartyPayment_TypeDef = 'TNG_Razer'
and TradesOrderThirdPartyPayment_StatusUpdatedDateTime > '2026-05-05'
and TradesOrderThirdPartyPayment_StatusUpdatedDateTime < '2026-05-06'




SELECT
    TradesOrderThirdPartyPayment_TypeDef,
    CAST(
        SUM(CASE 
            WHEN TradesOrderThirdPartyPayment_StatusDef = 'Success' THEN 1 
            ELSE 0 
        END) AS decimal(18, 4)
    ) / COUNT(*) *100 AS FailRatio
FROM TradesOrderThirdPartyPayment WITH (NOLOCK)
WHERE TradesOrderThirdPartyPayment_ValidFlag = 1
  AND TradesOrderThirdPartyPayment_TypeDef = 'GrabPay_AsiaPay'
  AND TradesOrderThirdPartyPayment_DateTime >= '2026-05-05'
  AND TradesOrderThirdPartyPayment_DateTime <  '2026-05-06'



select TradesOrderThirdPartyPayment_TypeDef,
		CAST(SUM(CASE WHEN TradesOrderThirdPartyPayment_StatusDef = 'Success' then 1 else 0 end) AS DECIMAL(10,2))
		/
		COUNT(*) * 100 AS 'Success Ratio',
		count(*) as 'Total-Count'
from TradesOrderThirdPartyPayment(nolock)
where TradesOrderThirdPartyPayment_ValidFlag = 1
and TradesOrderThirdPartyPayment_DateTime >= '2026-05-05'
and TradesOrderThirdPartyPayment_DateTime < '2026-05-06'
and TradesOrderThirdPartyPayment_ShopId != 200017
group by TradesOrderThirdPartyPayment_TypeDef



select TradesOrderThirdPartyPayment_TypeDef,
		CAST(SUM(CASE WHEN LEN(TradesOrderThirdPartyPayment_UpdatedUser) = 36 then 1 else 0 end) AS DECIMAL(10,2))
		/
		COUNT(*) * 100 AS 'Recheck Rate',
		count(*) as 'Total-Count'
from TradesOrderThirdPartyPayment(nolock)
where TradesOrderThirdPartyPayment_ValidFlag = 1
and TradesOrderThirdPartyPayment_StatusUpdatedDateTime >= '2026-05-05'
and TradesOrderThirdPartyPayment_StatusUpdatedDateTime < '2026-05-06'
--and TradesOrderThirdPartyPayment_ShopId != 200017
group by TradesOrderThirdPartyPayment_TypeDef