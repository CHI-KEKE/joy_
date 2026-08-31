## 表用途

`regular_daily_create` 是**每日建單統計稽核表**，由 `RegularCreateOrderAuditJob` 每日寫入。  
記錄當天預估應建單數 vs. 實際執行數 vs. 成功/失敗數，供 SRE 監控定期購排程健康狀況。  
若 `rdc_estimate ≠ rdc_execute` 或 `rdc_success = 0 AND rdc_failed > 0` 即視為異常。

> 此表**無 status 欄位**，所有欄位皆為統計數字。

---

## 語法

```sql
-- 基本查詢
SELECT *
FROM regular_daily_create
ORDER BY rdc_date DESC
LIMIT 7;

-- 查今天的稽核資料（Job 實際查的）
SELECT *
FROM regular_daily_create
WHERE rdc_date = CURDATE();

--找出歷史上異常的日期
-- 預估數 ≠ 實際執行數，或成功+失敗 ≠ 執行數，或全部失敗
SELECT *
FROM regular_daily_create
WHERE rdc_estimate != rdc_execute                        -- 預估 ≠ 實際執行
OR rdc_execute != (rdc_success + rdc_failed)         -- 有漏計
OR (rdc_success = 0 AND rdc_failed > 0)              -- 全部失敗
ORDER BY rdc_date DESC;

--查特定日期
SELECT *
FROM regular_daily_create
WHERE rdc_date = '2026-05-25';
```

## 欄位說明

┌────────────────┬───────────────────────────┐
│ 欄位           │ 意義                      │
├────────────────┼───────────────────────────┤
│ rdc_date       │ 紀錄日期                  │
├────────────────┼───────────────────────────┤
│ rdc_estimate   │ 預估應處理訂閱筆數        │
├────────────────┼───────────────────────────┤
│ rdc_execute    │ 實際嘗試呼叫建單 API 筆數 │
├────────────────┼───────────────────────────┤
│ rdc_success    │ 建單成功筆數              │
├────────────────┼───────────────────────────┤
│ rdc_failed     │ 建單失敗筆數              │
└────────────────┴───────────────────────────┘



##　💡 正常狀態

- rdc_estimate = rdc_execute
- rdc_execute = rdc_success + rdc_failed
- rdc_success > 0