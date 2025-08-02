# ApplePay 文件

## 目錄
1. [異常紀錄](#1-異常紀錄)

<br>

---

## 1. 異常紀錄

### 1.1 有1小時前的訂單未轉至ERP

<br>

**1.1.1 WebAPI log**

<br>

```sql
SELECT * FROM "hk_prod_webstore"."webstore_web_nlog"
where 1 =1
--and message like '%TG250702T00022%'
and requestid = '{"message":"202507021716384639'
and date = '2025/07/02'
limit 1000;
```

<br>

![alt text](./image-6.png)

<br>

**1.1.2 DB 狀態**

<br>

![alt text](./image-7.png)

<br>

**1.1.3 Stripe 後台**

<br>

![alt text](./image-8.png)

<br>

**1.1.4 壓狀態PR**

<br>

https://bitbucket.org/nineyi/nineyi.database.operation/pull-requests/22492/dif

<br>

**1.1.5 Paymentmiddleware**

<br>

```
/api/v1.0/pay/ApplePay_Stripe/TG250702T00022
RESPONSE BODY:\n{\n  \"request_id\":
```

<br>