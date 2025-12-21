

## PushNotification


```sql
use NotificationDB

-- MyECoupon / TradesOrderDetail / TradesOrderDetailV2 / PriceReduction / CustomerService
select PushNotification_TypeDef,
COUNT(1)
from PushNotification(nolock)
where PushNotification_ValidFlag = 1
and PushNotification_StatusDef = 'Normal'
and PushNotification_DateTime < GETDATE()
group by PushNotification_TypeDef
```

