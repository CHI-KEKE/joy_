# Coupon Web API（HK Prod / MY Prod）ExceededTimesLimiter 對齊評估

更新時間：2026-08-26 16:13（UTC+8）  
評估目標：確認是否可在 `coupon-web-api` 補齊與 TW Prod 相同設定：

- `_N1CONFIG.ExceededTimesLimiters.ECoupon.RedeemAsync_MemberId = true|5|600|3600`
- `_N1CONFIG.ExceededTimesLimiters.ECoupon.TransferAsync_MemberId = true|5|600|3600`

---

## 1) 前提與程式行為

`CouponItemController` 路由：

- `POST /api/coupon-items/redeem`（`[Route("redeem")]`）
- `POST /api/coupon-items/transfer`（`[Route("transfer")]`）

限流的 request key：

- Redeem：`MemberId`
- Transfer：`TransfereeMemberId`（程式中是 `request.TransfereeMemberId.ToString()`）

計數方式重點：

- 進入 API 時先 `ExamineTryOverTimesLockApiAsync(..., isExamineWithoutAddTryTimes: true)` 只檢查不加次數
- 僅在 `RedeemInvalidException` / `TransferInvalidException` catch 時，才 `TryTimesAsync(...)` 做 +1 計數

---

## 2) Loki 查詢範圍

- Datasource
  - HK Prod：`HK-Prod-Loki`（uid：`RjRcuuN4k`）
  - MY Prod：`MY-Prod-Loki`（uid：`LZjWWydVz`）
- Label 篩選：`{service="prod-coupon-service", container="coupon-web-api"}`
- 主要觀察區間：近 7 天（`now-7d` ~ `now`）
- 補充觀察：MY 近 30 天（因 7 天近乎無流量）

---

## 3) 實際數據（Loki）

### HK Prod（近 7 天）

- Redeem 總量：`1517`
- Transfer 總量：`23`
- Redeem 單會員 7 天內最高：`memberId=8957733`，總次數 `6`
- Transfer 單會員 7 天內最高：`transfereeMemberId=4724977`（或 4726503），總次數 `3`
- 10 分鐘窗口峰值（對應 `CacheSeconds=600`）
  - Redeem（8957733）：最高 `3 / 10m`
  - Transfer（4724977）：最高 `3 / 10m`
- 30 天 `RedeemInvalidException`：0
- 30 天 `TransferInvalidException`：0

### MY Prod（近 7 天 / 30 天）

- 近 7 天 Redeem：`0`
- 近 7 天 Transfer：`0`
- 近 30 天 Redeem：`1`
- 近 30 天 Transfer：`0`
- 近 30 天 Redeem top member：`memberId=2813930`，次數 `1`
- 30 天 `RedeemInvalidException`：0
- 30 天 `TransferInvalidException`：0

---

## 4) 風險評估（對齊 `true|5|600|3600`）

門檻是「同一 Member 於 600 秒內 > 5 次才會被鎖」。

以目前 HK / MY 的實際流量：

- HK 的 Redeem / Transfer 單會員 10 分鐘峰值都只有 `3`
- MY 幾乎沒有流量
- 且 30 天都沒有看到對應 invalid exception 大量出現

**結論：HK Prod / MY Prod 補上 TW Prod 同值設定，預期不會擋到正常流量，風險低。**

---

## 5) 本次使用的核心 LogQL

```logql
# 7天總量：Redeem
count_over_time({service="prod-coupon-service", container="coupon-web-api"} |= "Nine1HttpLog" |= "/api/coupon-items/redeem" [7d])

# 7天總量：Transfer
count_over_time({service="prod-coupon-service", container="coupon-web-api"} |= "Nine1HttpLog" |= "/api/coupon-items/transfer" [7d])

# HK Redeem top member（7天）
topk(20, sum by (memberId) (count_over_time(
{service="prod-coupon-service", container="coupon-web-api"}
|= "coupon-items/redeem" |= "request："
| regexp `memberId.{3}(?P<memberId>\d+)`
[7d])))

# HK Transfer top member（7天，抓 transfereeMemberId）
topk(20, sum by (memberId) (count_over_time(
{service="prod-coupon-service", container="coupon-web-api"}
|= "coupon-items/transfer" |= "request："
| regexp `transfereeMemberId.{3}(?P<memberId>\d+)`
[7d])))

# 10分鐘窗口檢查（範例）
count_over_time({service="prod-coupon-service", container="coupon-web-api"} |= "coupon-items/redeem" |= "request：" |= "8957733" [10m])
count_over_time({service="prod-coupon-service", container="coupon-web-api"} |= "coupon-items/transfer" |= "request：" |= "4724977" [10m])

# 30天 invalid exception
count_over_time({service="prod-coupon-service", container="coupon-web-api"} |= "RedeemInvalidException" [30d])
count_over_time({service="prod-coupon-service", container="coupon-web-api"} |= "TransferInvalidException" [30d])
```

