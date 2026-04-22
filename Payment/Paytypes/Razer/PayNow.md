# Razer — PayNow（新加坡）

付款類型：`PayNow_Razer`
形式：QR Code 轉帳（注意：曾出現詐騙案例）

<br>

## 查詢訂單 SQL

```sql
USE WebStoreDB
SELECT
    TradesOrderThirdPartyPayment_StatusDef,
    TradesOrderSlave_SalePageId,
    ShopCategorySalePage_CategoryId,
    TradesOrderSlave_StatusDef,
    TradesOrderThirdPartyPayment_DateTime,
    TradesOrderGroup_Code,
    TradesOrder_Code,
    TradesOrderSlave_Code,
    *
FROM TradesOrderGroup (NOLOCK)
INNER JOIN TradesOrder (NOLOCK)
    ON TradesOrder_TradesOrderGroupId = TradesOrderGroup_Id
INNER JOIN TradesOrderSlave (NOLOCK)
    ON TradesOrderSlave_TradesOrderId = TradesOrder_Id
INNER JOIN TradesOrderThirdPartyPayment (NOLOCK)
    ON TradesOrderGroup_Code = TradesOrderThirdPartyPayment_TradesOrderGroupCode
INNER JOIN ShopCategorySalePage (NOLOCK)
    ON ShopCategorySalePage_SalePageId = TradesOrderSlave_SalePageId
WHERE TradesOrderGroup_ValidFlag = 1
    --AND TradesOrderGroup_Code = 'TG250812PB0001'
    --AND TradesOrderGroup_Code >= 'TG250808BA00LN'  -- 訂單編號範圍
    --AND TradesOrderGroup_ShopId = 41571  -- 指定店鋪
    --AND TradesOrderGroup_CrmShopMemberCardId IN (4521,4522,4523)  -- 指定會員卡
    --AND TradesOrderGroup_TotalPayment >= 8800  -- 最小金額
    --AND TradesOrderGroup_TrackSourceTypeDef IN ('AndriodApp','iOSApp')  -- 行動裝置來源
    -- AND TradesOrderGroup_CreatedDateTime >= '2025-08-08 11:00'  -- 可選：時間篩選
    AND TradesOrderThirdPartyPayment_TypeDef = 'PayNow_Razer'
    AND TradesOrderThirdPartyPayment_ValidFlag = 1
```

<br>

## QueryPayment

**API 端點：** `/api/v1.0/QueryPayment/PayNow_Razer`

**Request Body 範例：**
```json
{
  "request_id": "dfa18839-ecf7-4a86-bd14-53885442dab6",
  "transaction_id": "3471805622",
  "country": "MY",
  "extend_info": {
    "query_string": "",
    "amount": 1.00,
    "transaction_id": "3471805622",
    "tg_code": "MG260203Z00002",
    "is_fiuu_enable": true
  }
}
```

**PMW → Razer Request：**
```json
{
  "amount": 1.00,
  "txID": "3471805622",
  "domain": "elemis01",
  "skey": "920ad57adad1a79328785042437e9360",
  "url": null,
  "type": null,
  "oID": null,
  "req4token": 0
}
```

**Fiuu Query API：** `https://api.fiuu.com/RMS/q_by_tid.php`

<br>

## QueryPayment Response 範例

### Pending

```
StatCode: 22
StatName: Pending
TranID: 3471805622
Amount: 1.00
Domain: elemis01
VrfKey: fe15c18f956e490b189d7da40fa06ff9
Channel: PayNow
OrderID: MG260203Z00002
Currency: SGD
```

### Failed

```
StatCode: 11
StatName: failed
OrderID: MG260203Z00002
Amount: 1.00
TranID: 3471805622
Domain: elemis01
BillingDate: 2026-02-03 23:42:11
BillingName: zskcnd
VrfKey: 7ca29813b2f5964b9cc3b29d5d5119ac
Channel: PayNow
Currency: SGD
ErrorCode: EW_FAILED
ErrorDesc: ER0014
```
