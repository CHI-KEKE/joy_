# regular-check-housekeeper — 邏輯說明

> PHP 檔案：`admin/daemon/regular_check_housekeeper.php`
> 對應排程：每天台灣時間 11:30（`30 03 * * *`）
> pg_id：8

---

## 一句話摘要

查詢今日的 `regular_daily_create` 統計紀錄，若數字不一致（預期 ≠ 實際，或成功+失敗 ≠ 實際）即發送 Slack 告警通知工程師。

---

## 執行流程

```
main()
├── regular_daily_create.load_by_date(today)
│   ├── 找到紀錄 → 檢查數字一致性
│   │   ├── expectQty != executeQty          → 發 Slack 告警
│   │   ├── executeQty != (success + failed) → 發 Slack 告警
│   │   └── success == 0 && failed > 0       → 發 Slack 告警
│   └── 找不到紀錄
│       └── 發 Slack 告警（排程未正常執行）
└── alert_to_slack()
```

---

## 告警條件

| 條件 | 告警訊息 |
|---|---|
| `regular_daily_create` 今日紀錄不存在 | 「第二期成單排程未正常執行」 |
| `rdc_estimate != rdc_execute` | 「第二期成單數量異常」 |
| `rdc_execute != rdc_success + rdc_failed` | 「第二期成單數量異常」 |
| `rdc_success == 0 && rdc_failed > 0` | 「第二期成單數量異常」 |

---

## Slack 告警格式

```
username: [TW][QA]RegularPurchase Alert    ← 環境資訊
title:    第二期成單數量異常，請 @S039NMHFHCK 協助查詢原因
text:     預期執行數量：N
          實際執行數量：N
          成功數量：N
          失敗數量：N
```

> **注意**：`<!subteam^S039NMHFHCK>` 是 Slack User Group ID，翻寫時需確認新環境的 Group ID。

---

## 涉及資料表

| 資料表 | 操作 | 說明 |
|---|---|---|
| `regular_daily_create` | SELECT | 查今日建單統計（由 regular-create-housekeeper 寫入） |

---

## 外部依賴

| 依賴 | 說明 |
|---|---|
| MySQL | 查 regular_daily_create |
| **Slack Webhook** | 唯一依賴 Slack 的 job，URL 由 `Gb_Config::` 設定 |

---

## 與其他排程的關係

```
regular_create_housekeeper Step ①
  └─ INSERT regular_daily_create
       rdc_estimate = 預估工作量
       rdc_execute  = 0
       rdc_success  = 0
       rdc_failed   = 0

regular_create_housekeeper Step ⑤
  └─ UPDATE regular_daily_create
       rdc_execute  = 今天實際處理筆數
       rdc_success  = 成功筆數
       rdc_failed   = 失敗筆數

regular_check_housekeeper（pg_id=8，1小時後執行）
  └─ 比對 estimate vs execute/success/failed
       → 異常 → Slack 告警
```

> 這三支有時間依賴關係，regular-check 必須在 regular-create 完成後才有意義。

---

## 翻寫 .NET Core 注意事項

1. **Slack Webhook URL 設定**：新版需確認 Slack Webhook URL 與 Channel 設定來源（secrets 或 appsettings）。
2. **Slack User Group ID**：`S039NMHFHCK` 是固定的 Slack Group ID，需確認新環境是否相同。
3. **執行順序依賴**：新版排程仍需確保 regular-create（10:15）在 regular-check（11:30）之前完成。
4. **Prod/QA 環境標籤**：Slack 訊息包含環境標籤，新版需正確注入環境變數。
