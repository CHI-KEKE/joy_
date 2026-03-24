
給點紀錄稽核監控到異常
市場環境: HK-Prod
TG Code: TG251028Z00085
稽核到下列異常:
35248_TG251028Z00085 BookingTime與預期不符，時間差異: 24.00 小時，超過允許範圍 1 小時




## 訂單是否已出貨

```csharp
return detail.SalesOrderSlaveStatusDef == nameof(SalesOrderSlaveStatusDefEnum.Finish) ||
        detail.SalesOrderSlaveStatusDef == nameof(SalesOrderSlaveStatusDefEnum.Cancel);
```

這次是走未出貨的路徑

正流程會 + N 天 所以稽核時是未來時間
但當下加 N 天的時間應該要瞄準 OrderTime

我要以還沒有被延遲的時機點為基準當作 改成 OrderTime 的基準

重點不是 shipping 了 而是要以 "第一次為時機點瞄準"


booktime < datetime now 可以為基準嗎???

==> 這個情景就是已經不更新 bookingtime 了該給了的情境吧 也就是已完成出貨


多次延遲後，某次出貨了，正流程就不會更新 bookingtime，會給點，而且 bookgtime 會是當天 0:00，稽核時當下一定 > bookgtime 這時候如果改成 orderdate 會誤判!!


還是加上一個條件 叫做 ordertime - updatetime < 1 

因為 > 1 的情境就是有延遲!


結論

updatetime - ordertime < 1 改用 ordertime 為判斷式

語意 : 你最後一次更新錶的時間與下單時間間格只有小於 1 表示是下單那一次的稽核，這時候把基準改成 ordertime是正確的


條件 1 : N 天
條件 2 : updatetime - ordertime < 1

比較 orderdateme + n 天 vs bookingtime 當下押的


假設我的判斷條件改成

update - order < 3hr, 使用 ordertime 為基準
指定日期會怎樣??




## 改拉 rewardHistory 撈沒有 setBookingtime 的


AuditPromotionRewardLoyaltyPointsV2Job

local test


-j AuditPromotionRewardLoyaltyPointsV2

DOTNET_ENVIRONMENT Development
N1_MARKET TW
N1_ENVIRONMENT QA
N1_LOCAL_MODE True



hkqa2 
promotoinid :11094 (固定給分)
2026/01/28 11:00起
2027/01/30 00:00止
滿一塊就行


TG : TG260128M00001


{service="hk-qa-promotion-service"}
|json
| _props_TaskId = `e54916e2-4742-40be-b6bc-9eb5b388137c`
|json
| line_format "{{._msg}}"

[VerifyBookingTimeForUnshippedOrder] 開始驗證未出貨訂單 BookingTime，Order: 11094_TG260128M00001

未出貨訂單確認是否曾經延遲 BookingTime

是否為曾經執行 setBookingTime : False

hasDelayBookingTime 為 false 且為 + N 天

[VerifyBookingTimeForUnshippedOrder] 使用 OrderTime 計算，Order: 11094_TG260128M00001

[VerifyBookingTimeForUnshippedOrder] RewardDays > 0，將預期時間的時分秒歸零，Order: 11094_TG260128M00001

2026-01-28T05:01:45.0543839Z [Information] [VerifyBookingTimeForUnshippedOrder] Order: 11094_TG260128M00001, UpdateTime: 2026-01-28 11:17:14, OrderTime: 2026-01-28 11:16:38, RewardDays: 1, ExpectedTime: 2026-01-29 00:00:00, ActualBookingTime: 2026-01-29 00:00:00, TimeDifference: 0.00 hours


[VerifyBookingTimeForUnshippedOrder] BookingTime 驗證成功，Order: 11094_TG260128M00001, 時間差異: 0.00 小時，在允許範圍內





## 建一個 + 8 天活動看看


11102



## 晚上 11:58 成1張單看看

觀察 + 8 天 & + 1 天 狀況



11102_TG260129H00002


local 測試 qa debug data 看起來會取得該 history 資料沒錯