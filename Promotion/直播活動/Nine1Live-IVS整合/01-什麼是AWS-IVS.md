# 什麼是 AWS IVS？

## 定義

**IVS = Amazon Interactive Video Service**

AWS 提供的**托管式直播串流服務**，開發者不需要自己架串流伺服器，直接呼叫 API 即可建立直播頻道。

> 底層技術基於 Amazon Twitch 的直播基礎設施（Twitch 也是 Amazon 旗下公司）。

---

## 技術規格

| 項目 | 說明 |
|------|------|
| **輸入協定** | RTMP（商家用 OBS、手機 App 推流） |
| **輸出協定** | HLS（消費者瀏覽器直接播放） |
| **延遲模式** | NORMAL / LOW / ULTRA_LOW（< 1 秒） |
| **頻道類型** | STANDARD / BASIC / ADVANCED_SD / ADVANCED_HD |
| **計費方式** | 按直播分鐘數 + 觀看流量計費 |

---

## 為什麼 91APP 選用 IVS 而不自己架？

| 優點 | 說明 |
|------|------|
| ✅ 免維運 | 不需要自己管串流伺服器、CDN |
| ✅ 低延遲 | ULTRA_LOW 模式可達 < 1 秒 |
| ✅ 自動錄製 | 錄製存到 S3，支援 VOD 回放 |
| ✅ 防盜播 | 私有頻道需 JWT Token 才能觀看 |
| ✅ 全球 CDN | AWS 自動處理全球分發 |

---

## 在 91APP 中的定位

LiveBuy 原本只支援 **Facebook** 直播。後來為了讓商家能在**自家平台**開直播（不依賴 Facebook），新增了 `Nine1Live` 平台，底層就是 **AWS IVS**。

```
PlatformTypeEnum
├── Facebook       ← 原有
├── Instagram      ← 原有
├── YouTube        ← 原有
├── Livebuy        ← 原有
└── Nine1Live      ← 新增（底層 = AWS IVS）
```

**相關程式碼位置：**
- `Nine1.Livebuy.BL.BE/LiveSessions/Enum/PlatformTypeEnum.cs`
- `Nine1.Livebuy.BL.Services/Nine1Live/Nine1LiveStreamService.cs`
