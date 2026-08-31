## ETag 沒有變，Fetch 根本不下載

manifest ETag == S3 ETag  → NoChange，直接跳過

S3 上的檔案如果沒有被更新，ETag 不變，Fetch 不會重新下載，Redis 內容維持原樣。



## InMemory Cache 還沒過期（60 秒內）

Fetch 已更新 Redis → 但 InMemory 還快取著舊的（壞的）內容 → 繼續顯示 key
Fetch 完要等最多 60 秒 InMemory 自然過期，或要手動呼叫 PurgeCache。


## S3 上的檔案本身就是壞的

翻譯服務沒有正確部署 → S3 內容就是空的或 key 對不上 → Fetch 拉下來也是壞的
Fetch 不能修復來源本身的問題。

## Module 根本不在設定清單裡

```csharp
if (!this._translationSettings.Modules.Any(x => x.Name == module))
// → 這個 module 的翻譯從來沒被 Fetch 過，Redis 沒有這份資料
```

Fetch 只拉 config 裡設定的 modules，沒設定的永遠不會拉。



## BackupStorage 也壞了

Redis 壞 → fallback 讀磁碟 → 磁碟沒有檔案 → 全部找不到 → 顯示 key




## 正確排查順序

```bash
畫面出現 key 值
│
├─ 1. 確認 S3 上翻譯檔案內容是否正確
│       ↓ 不對 → 找翻譯服務 / 重新發布翻譯
│
├─ 2. 確認 Redis 裡是否有資料（用 redis-cli GET 那個 key）
│       ↓ 沒有 → 確認 module 有沒有在設定清單，再 Fetch
│
├─ 3. Fetch → 但要確認 ETag 有沒有變（看 Fetch 回傳的 IsChanged）
│       ↓ IsChanged=false → 表示 S3 沒更新，Fetch 沒效
│
├─ 4. 呼叫 PurgeCache 清掉 InMemory
│       ↓ 讓下一次 GetString 強制重讀 Redis
│
└─ 5. 若以上都正常，問題在前端取得 key 的 module 路徑寫錯
```