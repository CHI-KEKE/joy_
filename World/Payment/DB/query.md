# Query 文件

## 目錄
1. [退款表](#1-退款表)
2. [SalesOrderGroup](#2-salesordergroup)
3. [ThirdPartyPayment](#3-thirdpartypayment)
4. [大表](#4-大表)
5. [取消單](#5-取消單)
6. [退貨單](#6-退貨單)
7. [特定金流商品頁資訊與法](#7-特定金流商品頁資訊與法)

<br>

---

## 1. 退款表

<br>

```sql
use ERPDB

select RefundRequest_ResponseMsg,RefundRequest_TransactionId,RefundRequest_CreatedDateTime,*
from RefundRequest(nolock)
where RefundRequest_ValidFlag = 1
and RefundRequest_Id in (121537,124652)
and RefundRequest_ResponseMsg like '%Business Rules Incorrect!%'
and RefundRequest_StatusDef in ('RefundRequestFail','RefundRequestGrouping')
and RefundRequest_UpdatedDateTime > '2025-07-01'
```

<br>

---

## 2. SalesOrderGroup

<br>

```sql
SELECT SalesOrderGroup_DateTime,SalesOrderGroup_ShopId,*
FROM SalesOrderGroup(NOLOCK)
WHERE SalesOrderGroup_TradesOrderGroupCode IN ('TG240229K00030','TG240118M00056')
```

<br>

---

## 3. ThirdPartyPayment

<br>

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

<br>

---

## 4. 大表

<br>

```sql
select OrderSlaveFlow_StatusDef,OrderSlaveFlow_StatusForUserDef,OrderSlaveFlow_StatusForSCMDef,OrderSlaveFlow_SalesOrderSlaveStatusDef,*
from OrderSlaveFlow(nolock)
where OrderSlaveFlow_ValidFlag = 1
and OrderSlaveFlow_TradesOrderGroupCode = 'TG250626K00002'
```

<br>

---

## 5. 取消單

<br>

```sql
select *
from CancelOrderSlave(nolock)
where CancelOrderSlave_Id = 12285
```

<br>

```sql
uSE WebStoreDB
select CancelRequest_TradesOrderSlaveId,*
from CancelRequest(nolock)
where CancelRequest_TradesOrderSlaveCode = 'TS250701P000011'
```

<br>

---

## 6. 退貨單

<br>

```sql
uSE ERPDB

SELECT OrderSlaveFlow_CancelOrderSlaveId,ReturnGoodsOrderSlave_IsClosed,OrderSlaveFlow_TradesOrderSlaveStatusDef,*
FROM OrderSlaveFlow(NOLOCK)
left join ReturnGoodsOrderSlave(nolock)
on OrderSlaveFlow_TradesOrderSlaveId = ReturnGoodsOrderSlave_TradesOrderSlaveId
WHERE OrderSlaveFlow_ValidFlag = 1
AND OrderSlaveFlow_TradesOrderSlaveCode = 'TS250701P000011'
```

<br>

---

## 7. 特定金流商品頁資訊與法

<br>

```sql
use WebStoreDB

DECLARE @payType VARCHAR(30) = 'ApplePay';

SELECT SalePagePayType_SalePageId AS N'商品序號',
       SalePage_Title,
       SalePage_ShopId AS N'商店序號',
       SalePagePayType_CreatedDateTime AS N'金流套用時間',
       *
FROM SalePagePayType(nolock)
INNER JOIN SalePage(nolock)
    ON SalePagePayType_SalePageId = SalePage_Id
WHERE SalePagePayType_ValidFlag = 1
AND SalePage_ValidFlag = 1
AND SalePagePayType_TypeDef = 'ApplePay'

select *
from ShopCategory(nolock)
where ShopCategory_ValidFlag = 1
and ShopCategory_ShopId = 2
order by ShopCategory_CreatedDateTime desc
```

<br>