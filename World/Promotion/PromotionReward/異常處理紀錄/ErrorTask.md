# ErrorTask 錯誤處理文件

## 目錄
1. [Lock 失敗](#lock-失敗)
2. [呼叫 ECoupon API失敗](#券不足呼叫-ECoupon-API-失敗)
3. [The JSON value is not in a supported DateTime format](#3-the-json-value-is-not-in-a-supported-datetime-format)
4. [SalepageCollectionException](#4-salepagecollectionexception)
5. [PromotionRecycleCoupon 找不到 DDB 資料](#5-promotionrecyclecoupon-找不到-ddb-資料)
6. [點數中心 API 與 DDB 回收點數紀錄資料不對稱](#6-點數中心-api-與-ddb-回收點數紀錄資料不對稱)

<br>

---

## Lock 失敗

- 佔額與回饋互搶
- 正逆流程互搶
- 稽核正流程互搶

<br>
<br>

## 券不足呼叫 ECoupon API 失敗


- 訊息：優惠券回饋失敗，券數不足，ECouponId:
**主子單狀態：** WaitingToReward
**主子單UpdateReason：** 給券異常，本次已給予...

- 會在活動結束時間後 180 天內持續延遲 bookingTime
- 若經過重算，逆流程可能因為現狀以滿足壓上 Reward


## 呼叫 ECoupon API 失敗

記 log，優惠券回饋失敗，ECouponId:...

<br>
<br>

## 3. The JSON value is not in a supported DateTime format

### 訊息

```
System.FormatException: The JSON value is not in a supported DateTime format.

{
  "Id": "74c109d4-1da1-4678-8e9f-576f08127107",
  "SourceId": "",
  "JobName": "PromotionRewardCoupon",
  "Data": {
    "ShopId": 2,
    "PromotionEngineId": 6716,
    "PromotionEngineType": "RewardReachPriceWithCoupon",
    "OrderTypeDefEnum": "Others",
    "TradesOrderGroupCode": null,
    "CrmSalesOrderId": 329356,
    "OrderDateTime": "2025-05-08 20:00:00.000"
  }
}
```

<br>

### 原因

JSON 欄位內容中 `"OrderDateTime":"2025-05-08 20:00:00.000"` 無法被 System.Text.Json 正確解析為 DateTime
DateTime 少了 T 格式問題

<br>
<br>

## SalepageCollectionException

### 訊息

```
{"errorCode":"SalepageCollectionException","message":"HttpRequestException","data":"Response status code does not indicate success: 400 (Bad Request)."}
```

<br>

### 原因

可以打 saleapge-collection API 看看，掛掉請 arch 協助

<br>
<br>

## PromotionRecycleCoupon 找不到 DDB 資料

https://91app.slack.com/archives/C08BE4B4KQW/p1753756490298899

### 問題分析

<br>

- PromotionRewardLoyaltyPointsDispatcherV2Job 沒有被觸發
- OrderCreated Event 沒看到對應的TG
- OrderFailed 有紀錄
- OrderRecheck 後訂單狀態為 Timeout 觸發了 OrderCancelled

Athena 可以查
```SQL
SELECT * FROM "hk_prod_nmqv3"."archive_event"
WHERE event_name = 'OrderCreated'
AND date = '2025/06/15'
AND event LIKE '%TG250616A00008%'
LIMIT 10;
```

-  SalesOrderGroup
-  invalid => OrderFailed

<br>

### 結論

<br>

DDB 給券應該要檢查訂單狀態做噴錯

<br>
<br>

## 6. 點數中心 API 與 DDB 回收點數紀錄資料不對稱

### 問題描述

<br>

點數回收的 job 執行時，在進行逆流程主邏輯前，會先篩選 detail 資料抓出 "要退的對應子單"，但因為這個篩選是 by TS Code，因此以線下訂單而言，TS_正 + TS_逆都被帶出來往後走，到真正的逆流程退點時，被檢查邏輯所阻擋，造成 "不該退逆向單" 的狀況

<br>

### 問題分析

<br>

- **篩選邏輯問題**: 使用 TS Code 進行篩選時，會同時抓到正向單和逆向單
- **線下訂單特性**: TS_正 + TS_逆 都會被篩選條件帶出
- **檢查邏輯阻擋**: 在逆流程退點時被後續檢查邏輯阻擋
- **錯誤結果**: 造成不該退逆向單的異常狀況

<br>

### 修正方案

<br>

修正為要對應到 `OriginalCrmSalesOrderSlaveId` 才是真正要退的子單

**相關 MR**: https://gitlab.91app.com/commerce-cloud/nine1.promotion/nine1.promotion.worker/-/merge_requests/817

<br>