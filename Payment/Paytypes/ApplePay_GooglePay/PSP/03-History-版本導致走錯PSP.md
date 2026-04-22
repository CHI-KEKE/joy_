# PSP (PaymentServiceProfile) — History：版本導致走錯 PSP 問題

## 案件說明

**VSTS**：https://91appinc.visualstudio.com/G11n/_workitems/edit/453982

2024/12/23 Luke 反應 4 & 12 店偶發出現戳到 TW ApplePay Decrypt 的結帳需求。

**Elmah**：http://elmahdashboard.91app.hk/Log/Details/a4e8cba2-f1a1-4398-8e1d-d1c63774b2bb

錯誤訊息：無法取得 Apple Pay Decrypt 結果，回傳狀態：ProtocolError

---

## 釐清過程

### Athena 查詢

```sql
-- 4 店的錯誤
select * from "hk_prod_webstore"."webstore_web_nlog"
where date = '2024/12/23'
and controller = 'tradesOrderLite'
and action = 'CompleteForNewCart'
and requestid = '{"message":"202412230856540938'
limit 100;

-- 12 店的錯誤
select * from "hk_prod_webstore"."webstore_web_nlog"
where date = '2024/12/21'
and controller = 'tradesOrderLite'
and action = 'CompleteForNewCart'
and requestid = '{"message":"202412211334273896'
limit 100;
```

可以得到 MemberId：`'2216075'`、`'2275917'`

---

### 比對版本

查 `TradesOrderGroup_TrackAppVersion` 後發現：

- **成功案例**（4 號店隨機一筆）：版本 `24.13.0`，TG：TG241223A00010
- **失敗案例**（Luke 反應）：版本均為 `24.11.5`

測試 HK 2 號店 24.11.5，選了 Apple Pay，前端 JSI 傳來 `paymentServiceProvider: NCCC`。

前端三元判斷邏輯：
- **有值**：帶後端給的 `paymentServiceProvider`
- **無值**（null / undefined / ''）：預設帶 `PaymentServiceProviderEnum.NCCC`

---

## 結論

**24.11.5 版本不該出現 ApplePay 的問題**，舊版本因未抓取 PSP Profile，落入 NCCC 預設，導致戳到 TW ApplePay Decrypt 流程。

![alt text](../../Img/image-4.png)
