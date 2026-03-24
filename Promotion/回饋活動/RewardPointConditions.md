## 條件設定節點

<br>

| 項目名稱 | 欄位名稱 | 說明 |
|---------|----------|------|
| 訂單來源 | SourceTypeDef | 來源如：APP、Web、POS等 |
| 配送方式 | DeliveryMethod | 宅配、自取、超商取貨等 |
| 訂單狀態 | StatusDef | 如已付款、已完成、取消等 |
| 給點天數 | RewardDays | 訂單成立後 N 天給點 |
| 指定時間 | RewardDateTime | 指定時間(線下前端會帶整點時間) |
| 贈與時間點類型 | RewardTimingType | RewardTimingTypeEnum (ByDays, ByDateTime) |

<br>

## 資料儲存方式

**1. 儲存在資料庫中** PromotionEngineSetting.PromotionEngineSetting_ExtendInfo 格式：JSON 序列化字串

儲存內容
```csharp
setting.PromotionEngineSetting_ExtendInfo = JsonSerializer.Serialize(entity.RewardPointConditions);
```

**2. 備份至 S3 的 Key 結構** RewardPointConditions

S3 Key 範本：{環境}/Promotion/Reward/{ShopId}/{PromotionEngineId}/{Identity}_{PromotionEngineDateTime}.json
範例：Prod/Promotion/Reward/9999/12345/PromoA_2506111230.json
PromotionEngineDateTime:yyMMddHHmmss 是日期時間格式（例如：2506111230 表示 2025/06/11 12:30）

- 正流程會拿最新的資料 => ExtendInfo
- 逆流程追朔舊資料會拿 S3