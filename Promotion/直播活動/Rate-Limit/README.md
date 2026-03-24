# Rate Limit 問題整理

## 問題核心

Rate Limit 問題的核心是 **Meta Graph API 每秒 250 req/s 的即時速率限制**。

所有商店共用同一個 App 配額，在高併發直播場景（大量顧客同時留言關鍵字）下，系統的 API 呼叫量很容易在短時間內觸頂，導致留言處理延遲。

---

## 文件索引

| 檔案 | 說明 |
|------|------|
| [01_meta-rate-limit.md](./01_meta-rate-limit.md) | Meta Graph API 三層 Rate Limit 架構詳解 |
| [02_polling.md](./02_polling.md) | Polling 機制調整方案 |

---

## Action Items

| 項目 | 負責方 |
|------|--------|
| Rate Limit 調整 rps 上限至 100 | 市場部 |
| Header 紀錄 API Rate Limit Alert | 後端 |
| 綁定流程補上 Webhook 訂閱欄位（`feed`、`live_videos`、`message_reactions`、`messages`）| 後端 |

---

## 結論

目前應對方向：

1. **MQ + 獨立 Polling Worker**：管控呼叫頻率，避免高併發時直接打爆 API
2. **Header 記錄 API Rate Limit Alert**：即時監控 API 剩餘配額，提早發現異常
3. **Webhook 訂閱補齊欄位**：確保直播相關事件（留言、互動）都能即時推送，降低對 polling 的依賴
