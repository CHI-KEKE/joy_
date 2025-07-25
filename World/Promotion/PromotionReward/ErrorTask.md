# ErrorTask 錯誤處理文件

## 目錄
1. [Lock 失敗](#1-lock-失敗)
2. [呼叫 ECoupon API失敗](#2-呼叫-ecoupon-api失敗)
3. [The JSON value is not in a supported DateTime format](#3-the-json-value-is-not-in-a-supported-datetime-format)
4. [SalepageCollectionException](#4-salepagecollectionexception)

<br>

---

## 1. Lock 失敗

### 紀錄

```
Reward Coupon Error, Exception： System.ApplicationException: 6684_TG250508L00002 Lock 失敗

HK_QA_Loyalty_PromotionReward

{ Type: "PromotionRewardCouponJob", TargetType: "NMQ Task Data", TargetId: "{\"ShopId\":2,\"TradesOrderGroupCode\":\"TG250508L00002\",\"CrmSalesOrderId\":0,\"OrderDateTime\":\"2025-05-08T10:44:49.5521107+08:00\",\"PromotionEngineId\":6684,\"PromotionEngineType\":null,\"OrderTypeDefEnum\":\"ECom\"}", Message: "Reward Coupon Error, Exception： System.ApplicationException: 6684_TG250508L00002 Lock 失敗",

at Nine1.Promotion.Console.NMQv3Worker.Jobs.PromotionReward.PromotionRewardCouponJob.DoJobAsync(String data, CancellationToken cancellationToken) in /src/Nine1.Promotion.Console.NMQv3Worker/Jobs/PromotionReward/PromotionRewardCouponJob.cs:line 150"HK_QA_Loyalty_PromotionReward
```

<br>

### 解法

確認被甚麼其他 job 壞掉卡住後的原因後手動解 lock

<br>

---

## 2. 呼叫 ECoupon API失敗

### 券不足

會有訊息：優惠券回饋失敗，券數不足，ECouponId:

<br>

會在活動結束時間後 180 天內持續延遲 bookingTime

<br>

**主子單狀態：** WaitingToReward

**主子單UpdateReason：** 給券異常，本次已給予...

<br>

### 真的失敗

一樣紀錄，但謹記 log：優惠券回饋失敗，ECouponId:...

<br>

---

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

<br>

DateTime 少了 T 格式問題

<br>

---

## 4. SalepageCollectionException

### 訊息

```
{"errorCode":"SalepageCollectionException","message":"HttpRequestException","data":"Response status code does not indicate success: 400 (Bad Request)."}
```

<br>

### 原因

為 salepage Collection 的 API 掛掉

<br>