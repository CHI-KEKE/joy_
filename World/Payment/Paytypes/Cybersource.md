# Cybersource 文件

![alt text](./image-1.png)

## 目錄
1. [domain](#1-domain)
2. [關鍵字](#2-關鍵字)
3. [Pay](#3-pay)
4. [Query](#4-query)
5. [RefundQuery](#5-refundquery)
6. [寄信](#6-寄信)
7. [異常紀錄](#7-異常紀錄)
8. [Refund](#8-refund)

<br>

---

## 1. domain

**BaseUrl:** https://api.cybersource.com

<br>

**PaymentUrl:** https://secureacceptance.cybersource.com/pay

<br>

---

## 2. 關鍵字

CreditCardOnce_Cybersource

<br>

---

## 3. Pay

### 成功的 Pay Response

<br>

```json
{
  "_links": {
    "self": {
      "href": "https://api.cybersource.com/tss/v2/searches/62064e83-efc6-4664-8a54-e5cf79a88144",
      "method": "GET"
    }
  },
  "searchId": "62064e83-efc6-4664-8a54-e5cf79a88144",
  "save": false,
  "query": "clientReferenceInformation.code:TG250619Z00085",
  "count": 3,
  "totalCount": 3,
  "limit": 20,
  "offset": 0,
  "timezone": "GMT",
  "submitTimeUtc": "2025-06-19T16:00:10Z",
  "_embedded": {
    "transactionSummaries": [
      {
        "id": "7503486675756718803616",
        "submitTimeUtc": "2025-06-19T15:57:47Z",
        "merchantId": "gphk000048256113",
        "applicationInformation": {
          "applications": [
            {
              "name": "ics_bill",
              "reasonCode": "100",
              "rCode": "1",
              "rFlag": "SOK",
              "reconciliationId": "7503486675756718803616",
              "rMessage": "Request was processed successfully.",
              "returnCode": "1260000"
            },
            {
              "name": "ics_pa_validate",
              "reasonCode": "100",
              "rCode": "1",
              "rFlag": "SOK",
              "reconciliationId": "7503486675756718803616",
              "rMessage": "ics_pa_validate service was successful",
              "returnCode": "1050001"
            },
            {
              "name": "ics_auth",
              "reasonCode": "100",
              "rCode": "1",
              "rFlag": "SOK",
              "reconciliationId": "7503486675756718803616",
              "rMessage": "Request was processed successfully.",
              "returnCode": "1010000"
            }
          ],
          "reasonCode": "100",
          "rCode": "1",
          "rFlag": "SOK"
        },
        "buyerInformation": {},
        "clientReferenceInformation": {
          "code": "TG250619Z00085",
          "applicationName": "Secure Acceptance Web/Mobile",
          "partner": {}
        },
        "consumerAuthenticationInformation": {
          "transactionId": "212JSW4nA8kwuQhymGU1",
          "eciRaw": "2"
        },
        "deviceInformation": {
          "ipAddress": "124.217.189.178"
        },
        "fraudMarkingInformation": {},
        "merchantInformation": {
          "resellerId": "cybsgpap"
        },
        "orderInformation": {
          "billTo": {
            "address1": "軒尼詩道130號",
            "state": "N/A",
            "city": "灣仔",
            "country": "HK",
            "postalCode": "N/A",
            "email": "annwkwong@ymail.com",
            "firstName": "WAI KUEN",
            "lastName": "WONG"
          },
          "shipTo": {},
          "amountDetails": {
            "totalAmount": "402.50",
            "currency": "HKD"
          }
        },
        "paymentInformation": {
          "paymentType": {
            "type": "credit card",
            "method": "MC"
          },
          "customer": {},
          "card": {
            "suffix": "1279",
            "prefix": "528946",
            "type": "002"
          }
        },
        "processingInformation": {
          "commerceIndicator": "6",
          "commerceIndicatorLabel": "spa",
          "authorizationOptions": {
            "authIndicator": "1"
          }
        },
        "processorInformation": {
          "processor": {
            "name": "vdchsbcbank"
          },
          "approvalCode": "385869",
          "eventStatus": "Pending",
          "retrievalReferenceNumber": "517015918327"
        },
        "pointOfSaleInformation": {
          "partner": {},
          "emv": {}
        },
        "riskInformation": {
          "providers": {
            "fingerPrint": {}
          }
        },
        "_links": {
          "transactionDetail": {
            "href": "https://api.cybersource.com/tss/v2/transactions/7503486675756718803616",
            "method": "GET"
          }
        },
        "installmentInformation": {},
        "errorInformation": {}
      },
      {
        "id": "7503486126686712803598",
        "submitTimeUtc": "2025-06-19T15:56:52Z",
        "merchantId": "gphk000048256113",
        "applicationInformation": {
          "applications": [
            {
              "name": "ics_bill"
            },
            {
              "name": "ics_auth"
            },
            {
              "name": "ics_arc"
            },
            {
              "name": "ics_pa_enroll",
              "reasonCode": "475",
              "rCode": "0",
              "rFlag": "DAUTHENTICATE",
              "rMessage": "The cardholder is enrolled in Payer Authentication.  Please authenticate before proceeding with authorization.",
              "returnCode": "1052000"
            }
          ],
          "reasonCode": "475",
          "rCode": "0",
          "rFlag": "DAUTHENTICATE"
        },
        "buyerInformation": {},
        "clientReferenceInformation": {
          "code": "TG250619Z00085",
          "applicationName": "Secure Acceptance Web/Mobile",
          "partner": {}
        },
        "consumerAuthenticationInformation": {
          "transactionId": "212JSW4nA8kwuQhymGU1"
        },
        "deviceInformation": {
          "ipAddress": "124.217.189.178"
        },
        "fraudMarkingInformation": {},
        "merchantInformation": {
          "resellerId": "cybsgpap"
        },
        "orderInformation": {
          "billTo": {
            "address1": "軒尼詩道130號",
            "state": "N/A",
            "city": "灣仔",
            "country": "HK",
            "postalCode": "N/A",
            "email": "annwkwong@ymail.com",
            "firstName": "WAI KUEN",
            "lastName": "WONG"
          },
          "shipTo": {},
          "amountDetails": {}
        },
        "paymentInformation": {
          "paymentType": {
            "type": "credit card",
            "method": "MC"
          },
          "customer": {},
          "card": {
            "suffix": "1279",
            "prefix": "528946",
            "type": "002"
          }
        },
        "processingInformation": {},
        "processorInformation": {
          "processor": {}
        },
        "pointOfSaleInformation": {
          "partner": {},
          "emv": {}
        },
        "riskInformation": {
          "providers": {
            "fingerPrint": {}
          }
        },
        "_links": {
          "transactionDetail": {
            "href": "https://api.cybersource.com/tss/v2/transactions/7503486126686712803598",
            "method": "GET"
          }
        },
        "installmentInformation": {},
        "errorInformation": {}
      },
      {
        "id": "7503486015566714703225",
        "submitTimeUtc": "2025-06-19T15:56:41Z",
        "merchantId": "gphk000048256113",
        "applicationInformation": {
          "applications": [
            {
              "name": "ics_pa_setup",
              "reasonCode": "100",
              "rCode": "1",
              "rFlag": "SOK",
              "rMessage": "Setup complete.",
              "returnCode": "1865000"
            }
          ],
          "reasonCode": "100",
          "rCode": "1",
          "rFlag": "SOK"
        },
        "buyerInformation": {},
        "clientReferenceInformation": {
          "code": "TG250619Z00085",
          "applicationName": "Secure Acceptance Web/Mobile",
          "partner": {}
        },
        "consumerAuthenticationInformation": {},
        "deviceInformation": {},
        "fraudMarkingInformation": {},
        "merchantInformation": {
          "resellerId": "cybsgpap"
        },
        "orderInformation": {
          "billTo": {},
          "shipTo": {},
          "amountDetails": {}
        },
        "paymentInformation": {
          "paymentType": {
            "type": "credit card",
            "method": "MC"
          },
          "customer": {},
          "card": {
            "suffix": "1279",
            "prefix": "528946",
            "type": "002"
          }
        },
        "processingInformation": {},
        "processorInformation": {
          "processor": {}
        },
        "pointOfSaleInformation": {
          "partner": {},
          "emv": {}
        },
        "riskInformation": {
          "providers": {
            "fingerPrint": {}
          }
        },
        "_links": {
          "transactionDetail": {
            "href": "https://api.cybersource.com/tss/v2/transactions/7503486015566714703225",
            "method": "GET"
          }
        },
        "installmentInformation": {},
        "errorInformation": {}
      }
    ]
  }
}
```

<br>

---

## 4. Query

/api/v1.0/QueryPayment/CreditCardOnce_Cybersource

<br>

---

## 5. RefundQuery

RefundQuery/CreditCardOnce_Cybersource

<br>

---

## 6. 寄信

**主旨：** [91APP x PUMA x Cybersource] Request for Refund Status Clarification – Order TG250613A00030

<br>

```
Dear Cybersource Team,

I hope you are doing well. This is Allen from 91APP.

First of all, I would like to express our sincere appreciation for your support with our previous inquiry — your assistance was very helpful.

We are reaching out again to seek your help in clarifying the refund status for one of our recent orders.  

Merchant ID (MID): gphk000048256113
Order Number: TG250613A00030
TransactionId :  7497456448616290803297
Refund Endpoint: https://api.cybersource.com/pts/v2/payments/{transactionId}/refunds/

Refund API Response Message:
  "reason": "UNAUTHORIZED_CARD",
  "message": "Decline - Inactive card or card not authorized for card-not-present transactions."

We noticed that the API response indicates the card is inactive with the message "Decline - Inactive card or card not authorized for card-not-present transactions."

We would like to confirm the current refund status for this order and verify whether the decline was specifically due to the card being inactive or expired.

Could you please provide any additional details about this status and your recommendations on how we should proceed to ensure the consumer receives their refund?

Best regards,
Allen
91APP  
--
91APP 品牌新零售 虛實融合 OMO 最佳夥伴
台北市南港區八德路四段 768 巷 5 號 6 樓
Tel: 02-2653-8091#532
Email: allenlin@91app.com
Site: www.91app.com
```

<br>

---

## 7. 異常紀錄

### 7.1 RefundQuery 4003 斷掉

<br>

Refund 完壓 RefundProcesing 之後 RefundQuery 斷掉

<br>

- 1:02 執行 Refund
- 之後每一小時問 RefundQuery 問到斷掉
- 最後一次問 refund query 的結果是 Pending 4003
- 因此進行 redo
- 成行成功 finish
- 最後 refundquery 結果為 Transmitted 成功

<br>

---

## 8. Refund

<br>

/Refund/CreditCardOnce_Cybersource

<br>

**Request Body**

<br>

```json
{
  "request_id": "b27e0095-6ad9-4770-a9c4-c95239cbea1b",
  "transaction_id": "7408093272826953803034",
  "amount": 0.0,
  "currency": null,
  "extend_info": {
    "RefundRequestTransactionId": "7425333499856428303980"
  }
}
```

<br>

**Response**

<br>

```json
{
  "request_id": "9f701345-da48-424f-9f95-c963e241c2f3",
  "transaction_id": "7425333499856428303980",
  "return_code": "4003",
  "return_message": null,
  "extend_info": {}
}
```

<br>

```json
{
  "request_id": "df00d580-e932-4901-b719-62235dd38ec4",
  "transaction_id": "7425333499856428303980",
  "return_code": "4003",
  "return_message": "Request was processed successfully.",
  "extend_info": {}
}
```

<br>

```json
{
  "_links": {
    "void": {
      "method": "POST",
      "href": "/pts/v2/refunds/7425333499856428303980/voids"
    },
    "self": {
      "method": "GET",
      "href": "/pts/v2/refunds/7425333499856428303980"
    }
  },
  "clientReferenceInformation": {
    "code": "TG250301Q00010"
  },
  "id": "7425333499856428303980",
  "orderInformation": {
    "amountDetails": {
      "currency": "HKD"
    }
  },
  "reconciliationId": "7408093272826953803034",
  "refundAmountDetails": {
    "currency": "HKD",
    "refundAmount": "764.15"
  },
  "status": "PENDING",
  "submitTimeUtc": "2025-03-21T05:02:30Z"
}
```

<br>