# Polling 機制調整

## Meta 官方建議做法

Meta 官方建議採用 **reverse chronological** 搭配 `since` 參數持續 polling，以確保不遺漏任何留言。

```
GET /{live-video-id}/comments?order=reverse_chronological&since={timestamp}
```

然而，Graph API 明確受 Rate Limit 限制，在高併發直播場景下（大量顧客同時留言），若不控制呼叫頻率，無法保證無痛擴展。

---

## 問題根源

原本的內建 polling 機制在高流量時面臨兩個問題：

1. **呼叫頻率過高** → 快速消耗 App-Level 250 req/s 配額
2. **每筆留言都建立一個 MQ Task** → polling 範圍或頻率過高時，Task 數量暴增，增加系統壓力

---

## 調整方案

| 項目 | 調整前 | 調整後 |
|------|--------|--------|
| 執行方式 | Service 內建 polling | 獨立 Worker 執行 |
| 頻率 | — | 每 10 秒一次 |
| 抓取範圍 | — | 往前拉 1 分鐘的資料 |

### 為何設定「往前拉 1 分鐘」

每 10 秒執行一次 polling，但往前拉 1 分鐘（60 秒）的資料，目的是確保留言不因網路延遲或短暫失敗而遺漏。

1 分鐘範圍的設計讓相鄰兩次 polling 的時間窗口有足夠重疊，即使某次執行略有延遲，下一次仍能補抓到同樣的留言，再由 MQ 層去重。

---

## 目前狀態

- 各 QA 環境已將 polling 改為獨立 Worker 執行
- 預計關閉原本 Service 內建的 polling 機制
