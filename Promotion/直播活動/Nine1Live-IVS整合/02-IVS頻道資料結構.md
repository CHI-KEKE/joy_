# IVS 頻道資料結構

## 概述

每個直播場次（LiveSession）建立時，系統會去 AWS IVS **建立一個專屬頻道**，並將頻道資訊巢狀存入 DynamoDB 的 `IvsChannel` 欄位。

**對應實體：** `LiveSessionIvsChannelEntity.cs`

---

## 欄位說明

| 欄位 | 類型 | 用途 |
|------|------|------|
| `ChannelArn` | string | 頻道唯一識別碼，呼叫所有 IVS API 時使用 |
| `IngestEndpoint` | string | 商家設定 OBS 的 **RTMP 推流網址** |
| `PlaybackUrl` | string | 消費者觀看的 **HLS 播放連結** |
| `StreamKeyCipherText` | string | 推流驗證密鑰（以 AES 加密後存放） |
| `StreamKeyArn` | string | StreamKey 的 ARN，用於 IVS 管理操作 |
| `Authorized` | bool | 是否私有頻道（`true` = 需 JWT 才能觀看，防盜播） |
| `LatencyMode` | string | 延遲模式（`NORMAL` / `LOW` / `ULTRA_LOW`） |
| `ChannelType` | string | 頻道類型（`STANDARD` / `BASIC` / `ADVANCED_SD` / `ADVANCED_HD`） |
| `PlaybackTokenIssuer` | string? | Playback JWT 簽發者 |
| `PlaybackTokenAudience` | string? | Playback JWT 受眾 |
| `RecordingConfigurationArn` | string? | 錄製組態 ARN，有值代表自動錄製到 S3 |
| `CreatedAt` | DateTime | 頻道建立時間 |
| `LastIssuedPlaybackTokenAt` | DateTime? | 最後一次發放 Playback Token 的時間 |
| `ProvisionStatus` | string | 資源佈建狀態（`Pending` / `Ready` / `Failed`） |
| `StreamStartedAt` | DateTime? | 直播開始時間（由 EventBridge 事件更新） |
| `StreamEndedAt` | DateTime? | 直播結束時間（由 EventBridge 事件更新） |
| `VodUrl` | string? | VOD 回放網址（直播結束後由 S3/CloudFront 提供） |
| `VodS3KeyPrefix` | string? | IVS 錄製的 S3 Key Prefix |

---

## 頻道命名規則

IVS 頻道名稱格式：`shop-{shopId}-{liveSessionId}`

此設計讓 AWS EventBridge 事件回呼時，可以從 `ChannelName` 直接解析出 `LiveSessionId`，不需要對 DynamoDB 做昂貴的 Scan 操作。

```
ChannelName: shop-12345-abc-uuid-def
              │    │      │
              │    ShopId  LiveSessionId
              固定前綴
```

---

## 相關程式碼位置

- `Nine1.Livebuy.BL.BE/LiveSessions/LiveSessionIvsChannelEntity.cs`
- `Nine1.Livebuy.BL.Services/Nine1Live/Nine1LiveStreamService.cs`
